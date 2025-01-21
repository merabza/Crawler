using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CrawlerDb.Models;
using DoCrawler.Domain;
using DoCrawler.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using RobotsTxt;

namespace DoCrawler.States;

public sealed class ParseOnePageState : State
{
    private readonly string _content;
    private readonly ParseOnePageParameters _par;
    private readonly UrlModel _url;
    public readonly List<string> ListOfUris = [];
    public readonly List<UriTerm> UriTerms = [];
    private Uri? _currentUri;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ParseOnePageState(ILogger logger, ParseOnePageParameters par, string content, UrlModel url) : base(logger,
        "Parse One Page")
    {
        _par = par;
        _content = content;
        _url = url;
    }

    public override void Execute()
    {
        _currentUri = UriFabric.GetUri(_url.UrlName);

        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(_content);

        var nodeHtml = htmlDoc.DocumentNode.ChildNodes.FirstOrDefault(s => s.Name == "html");
        if (nodeHtml == null)
            return;

        var nodeHead = nodeHtml.ChildNodes.FirstOrDefault(s => s.Name == "head");
        var nodeBase = nodeHead?.ChildNodes.FirstOrDefault(s => s.Name == "base");
        var attributeHref = nodeBase?.Attributes.FirstOrDefault(s => s.Name == "href");
        if (attributeHref != null && attributeHref.Value != string.Empty)
            _currentUri = UriFabric.GetUri(attributeHref.Value);
        if (_currentUri == null)
            return;
        ExtractAllLinks(htmlDoc.DocumentNode);

        var innerText = ExtractText(htmlDoc.DocumentNode);

        ParseParagraphs(innerText);
        //ეს ვარიანტი არ მუშაობს სწორად,
        //რადგან ზოგ შემთხვევაში ვერ ხვდება პარაგრაფების საზღვრებს და
        //შედეგად მიიღება რამდენიმე სიტყვა ერთად გადაბმული
        //ParseParagraphs(htmlDoc.DocumentNode.InnerText);
    }

    private static string ExtractText(HtmlNode htmlDocDocumentNode)
    {
        StringBuilder sb = new();
        foreach (var node in htmlDocDocumentNode.SelectNodes("//text()"))
        {
            if (node.ParentNode.Name is "script" or "style")
                continue;

            var text = WebUtility.HtmlDecode(node.InnerText).Trim();

            if (text == string.Empty)
                continue;

            if (node.NextSibling is not null && node.NextSibling.Name == "b")
            {
                sb.Append(text);
                continue;
            }

            if (node.ParentNode.Name == "b")
            {
                if (node.ParentNode.NextSibling != null)
                    sb.Append(text);
                else
                    sb.AppendLine(text);
            }
            else
            {
                sb.AppendLine(text);
            }
        }

        return sb.ToString();
    }

    private void ExtractAllLinks(HtmlNode htmlDocDocumentNode)
    {
        var links = htmlDocDocumentNode.SelectNodes("//a[@href]");
        if (links is null || links.Count == 0)
            return;
        foreach (var link in links)
        {
            // Get the value of the HREF attribute
            var hrefValue = link.GetAttributeValue("href", string.Empty);
            ExtractUrl(hrefValue);
        }
    }

    private void ParseParagraphs(string content)
    {
        var text = WebUtility.HtmlDecode(content);
        //თუ ტექსტი საერთოდ არ შეიცავს ქართულ ასოებს, მაშინ არ გვაინტერესებს
        if (!text.Any(c => _par.Alphabet.Contains(c)))
            return;

        var paragraphs = Regex.Split(text, "\r\n|\r|\n");
        foreach (var paragraph in paragraphs)
        {
            var parTrim = paragraph.Trim();
            if (parTrim == string.Empty)
                continue;

            //FIXME პარამეტრებზე უნდა გადავიდეს პარაგრაფების შესახებ ინფორმაციის შენახვის საჭიროება
            //UriTerms.Add(new UriTerm { TermType = ETermType.ParagraphStart});
            ParseStatements(parTrim);
            //FIXME პარამეტრებზე უნდა გადავიდეს პარაგრაფების შესახებ ინფორმაციის შენახვის საჭიროება
            //UriTerms.Add(new UriTerm { TermType = ETermType.ParagraphFinish});
        }
    }

    private void ParseStatements(string context)
    {
        //თუ კონტენტი ცარიელია, გასაანალიზებელიც არაფერია
        if (context == string.Empty)
            return;

        //თუ ტექსტი საერთოდ არ შეიცავს ქართულ ასოებს, მაშინ არ გვაინტერესებს
        if (!ContainsAnyAlphabetSymbols(context))
            return;

        Regex re = new(_par.SegmentFinisherPunctuationsRegex);
        var strTestParts = re.Split(context);
        if (strTestParts.Length == 1)
        {
            AddStatementStart();
            ParsePunctuations(strTestParts[0]);
            AddStatementFinish();
        }
        else
        {
            for (var i = 1; i < strTestParts.Length; i += 2)
            {
                AddStatementStart();
                ParsePunctuations(strTestParts[i - 1]);
                AddPunctuation(strTestParts[i]);
                AddStatementFinish();
            }

            if (strTestParts.Length % 2 != 1)
                return;

            var lastPart = strTestParts[^1];
            if (lastPart == string.Empty)
                return;

            AddStatementStart();
            ParsePunctuations(lastPart);
            AddStatementFinish();
        }
    }

    //ეს ფუნქცია ამოწმებს შეიცავს თუ არა ჩვენი ანბანის ასოებს
    //ჩვენს შემთხვევაში ეს არის ქართული ანბანის ასოები
    private bool ContainsAnyAlphabetSymbols(string context)
    {
        return context.Any(c => _par.Alphabet.Contains(c));
    }

    private void AddStatementFinish()
    {
        UriTerms.Add(new UriTerm(ETermType.StatementFinish));
    }

    private void AddStatementStart()
    {
        UriTerms.Add(new UriTerm(ETermType.StatementStart));
    }

    private void ParsePunctuations(string context)
    {
        if (context == string.Empty)
            return;

        Regex re = new(_par.PunctuationsRegex); //ყველა პუნქტუაციის ნიშანი
        var strTestParts = re.Split(context);

        if (strTestParts.Length == 1)
        {
            ParseWords(strTestParts[0]);
        }
        else
        {
            for (var i = 1; i < strTestParts.Length; i += 2)
            {
                ParseWords(strTestParts[i - 1]);
                AddPunctuation(strTestParts[i]);
            }

            if (strTestParts.Length % 2 != 1)
                return;

            ParseWords(strTestParts[^1]);
        }
    }

    private void AddPunctuation(string pText)
    {
        UriTerms.Add(new UriTerm(ETermType.Punctuation, pText));
    }

    private void ParseWords(string context)
    {
        //ცარელა სტრიქონს არ განვიხილავთ
        if (context == string.Empty)
            return;
        //ყველა ის პუნქტუაციის ნიშანი, რომელიც არ შეიძლება აღიქმებოდეს სიტყვის ნაწილად
        Regex re = new(_par.WordDelimiterRegex);
        var strTestParts = re.Split(context);
        if (strTestParts.Length < 3)
            AddWord(strTestParts[0]);
        else
            for (var i = 0; i < strTestParts.Length; i += 3)
                AddWord(strTestParts[i]);
        //AddWord(strTestParts[^1]);
    }

    private void AddWord(string word)
    {
        /*word.Contains("ახლა")*/

        //ესე დროებით გავაკეთე, რომ არ შემეშალოს ხელი სხვა ანომალიების აღმოჩენაში
        //შემდგომში აკრძალული ან დასაშვები სიმბოლოების სია უნდა გაკეთდეს და მას უნდა დავეყრდნოთ
        var trimmedWord = word.Trim('\x200B');
        trimmedWord = trimmedWord.Trim('\x200C');

        //ესეც დროებითია სიტყვის თავში დასმული ვარსკვლავი აღნიშნავს სქოლიოს. მომავალში ისე უნდა გავაკეთო,
        //რომ სქოლიო გამოვიცნო და აქ ვარსკვლავიანი სწიტყვა აღარ უნდა მოვიდეს
        //trimmedWord = trimmedWord.TrimStart('*');

        //ცარელა სიტყვის შენახვა არ გვჭირდება
        if (trimmedWord == string.Empty)
            return;
        //თუ ტექსტი საერთოდ არ შეიცავს ქართულ ასოებს, მაშინ არ გვაინტერესებს
        UriTerms.Add(ContainsAnyAlphabetSymbols(trimmedWord)
            ? new UriTerm(ETermType.Word, trimmedWord)
            : new UriTerm(ETermType.ForeignWord, trimmedWord));
    }

    private void ExtractUrl(string uriCandidate)
    {
        //ელექტრონული ფოსტის მისამართების შენახვა არ გვჭირდება
        //ასევე არ გვჭირდება ისეთი ლინკების შენახვა, რომელიც მხოლოდ ფრაგმენტს აღნიშნავს
        if (uriCandidate.StartsWith("mailto:") || uriCandidate.StartsWith('#'))
            return;

        var strUri = uriCandidate.Trim('"', '\'', '#', ' ', '>');
        try
        {
            var newUri = UriFabric.GetUri(strUri);
            if (newUri == null || !newUri.IsAbsoluteUri)
            {
                if (_currentUri is null)
                    return;
                var tempUri = UriFabric.GetUri(_currentUri, strUri);
                if (tempUri == null)
                    return;
                strUri = GetAbsoluteUri(tempUri);
                if (strUri == null)
                    return;
                newUri = UriFabric.GetUri(strUri);
            }

            if (newUri == null)
                return;

            if (newUri.Scheme != Uri.UriSchemeHttp && newUri.Scheme != Uri.UriSchemeHttps)
                return;

            if (newUri.LocalPath.Contains("//"))
                return;

            //if (newUri.Host != CurrentUri.Host && KeepSameServer == true)
            //  continue;
            //newUri.Depth = uri.Depth + 1;
            //Loger.Instance.LogMessage(newUri.Query.Count().ToString());

            // foo://example.com:8042/over/there?name=ferret#nose
            //  \_/   \______________/\_________/ \_________/ \__/
            //   |           |            |            |        |
            //scheme     authority       path        query   fragment
            //   |   _____________________|__
            //  / \ /                        \
            //  urn:example:animal:ferret:nose
            //string strUri = NewUri.AbsoluteUri;
            //Uri AbsUri = new Uri(strUri);
            var startQuery = newUri.Query;
            if (startQuery != string.Empty)
            {
                //თუ მისამართი შეიცავს ქვერის ნაწილს
                var newQuery = NormalizeQuery(startQuery, '&');
                newQuery = NormalizeQuery(newQuery, ';');
                //ფრაგმენტი არა გვჭირდება +AbsUri.Fragment;
                strUri = newUri.Scheme + "://" + newUri.Authority + newUri.LocalPath + newQuery;

                //თუ მისამართი შეიცავს ქვერის ნაწილს ვინახავთ ქვერის გამოყენებით
                //და მერე კიდევ ქვერის გარეშეც ერთი სტრიქონის მერე
                AddUriUri(strUri);
            }
            else
            {
                //თუ მისამართი არ შეიცავს ქვერის ნაწილს
                //ფრაგმენტი არა გვჭირდება +AbsUri.Fragment;
                strUri = newUri.Scheme + "://" + newUri.Authority + newUri.LocalPath;
                AddUriUri(strUri);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
        }
    }

    private void AddUriUri(string strUri)
    {
        ListOfUris.Add(strUri);
    }

    private static string? GetAbsoluteUri(Uri tempUri)
    {
        string? strToRet = null;
        try
        {
            strToRet = tempUri.AbsoluteUri;
        }
        catch (UriFormatException)
        {
        }

        return strToRet;
    }

    private static string NormalizeQuery(string startQuery, char delimiter)
    {
        char[] delimiters = { delimiter };
        if (startQuery.Length <= 1)
            return startQuery;

        var parts = startQuery[1..].Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
        var newQuery = string.Empty;
        var isLeastOneAdded = false;
        foreach (var p in parts)
        {
            if (isLeastOneAdded)
                newQuery += delimiter;
            newQuery += p;
            isLeastOneAdded = true;
        }

        return "?" + newQuery;
    }
}