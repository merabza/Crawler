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

namespace DoCrawler.States;

public sealed class ParseOnePageState : State
{
    //private readonly BatchPart _batchPart;
    private readonly string _content;

    //private readonly ICrawlerRepository _repository;
    private readonly ParseOnePageParameters _par;

    private readonly UrlModel _url;

    public readonly List<string> ListOfUris = new();

    //private readonly List<TextTerm> _listOfTextParts = new();
    public readonly List<UriTerm> UriTerms = new();

    //private readonly StringBuilder _sbAllText = new();
    private Uri? _currentUri;

    //, ICrawlerRepository repository, BatchPart batchPart
    public ParseOnePageState(ILogger logger, ParseOnePageParameters par, string content, UrlModel url) : base(logger,
        "Parse One Page")
    {
        //_repository = repository;
        _par = par;
        //_batchPart = batchPart;
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
        if (attributeHref != null && attributeHref.Value != "")
            _currentUri = UriFabric.GetUri(attributeHref.Value);
        if (_currentUri == null)
            return;
        ExtractAllLinks(htmlDoc.DocumentNode);

        var innerText = ExtractText(htmlDoc.DocumentNode);


        //string innerText = ParseHtmlNodeAndChildren(htmlDoc.DocumentNode);
        ParseParagraphs(innerText);
        //ეს ვარიანტი არ მუშაობს სწორად,
        //რადგან ზოგ შემთხვევაში ვერ ხვდება პარაგრაფების საზღვრებს და
        //შედეგად მიიღება რამდენიმე სიტყვა ერთად გადაბმული
        //ParseParagraphs(htmlDoc.DocumentNode.InnerText);
    }

    //private StringBuilder _sbAllText;
    //private string ParseHtmlNodeAndChildren(HtmlNode htmlNode)
    //{
    //  _sbAllText = new StringBuilder();
    //  Queue<HtmlNodeParse> nodes = new Queue<HtmlNodeParse>();
    //  nodes.Enqueue(new HtmlNodeParse(htmlNode, false));
    //  while (nodes.Count > 0)
    //  {
    //    HtmlNodeParse currentHtmlNodeParse = nodes.Dequeue();
    //    HtmlNode currentHtmlNode = currentHtmlNodeParse.HtmlNode;
    //    ParseHtmlNode(currentHtmlNode, currentHtmlNodeParse.InScript);
    //    if (currentHtmlNode.NodeType != HtmlNodeType.Comment && currentHtmlNode.NodeType != HtmlNodeType.Text)
    //    {
    //      bool inScript = (currentHtmlNode.NodeType == HtmlNodeType.Element &&
    //                       currentHtmlNode.Name.ToLower() == "script");
    //      foreach (HtmlNode childNode in currentHtmlNode.ChildNodes)
    //        nodes.Enqueue(new HtmlNodeParse(childNode, inScript));
    //    }
    //  }

    //  return _sbAllText.ToString();
    //}

    //private void ParseHtmlNode(HtmlNode htmlNode, bool inScript = false)
    //{

    //  switch (htmlNode.NodeType)
    //  {
    //    //case HtmlNodeType.Comment:
    //    //  ExtractSegments(htmlNode.InnerText, htmlNode.StreamPosition, ESegmentParseMethod.ForScript);
    //    //  break;
    //    case HtmlNodeType.Text:
    //      ExtractSegments(htmlNode.InnerText, htmlNode.StreamPosition, (inScript ? ESegmentParseMethod.ForScript : ESegmentParseMethod.Main));
    //      break;
    //    default:
    //      if (htmlNode.NodeType == HtmlNodeType.Element)
    //      {
    //        foreach (HtmlAttribute attr in htmlNode.Attributes)
    //        {
    //          string attrName = attr.Name.ToLower();
    //          if (attrName == "href" || attrName == "src")
    //            ExtractUrl(attr.Value);
    //          else
    //          {
    //            ExtractSegments(attr.Name, htmlNode.StreamPosition, ESegmentParseMethod.ForScript);
    //            ExtractSegments(attr.Value, htmlNode.StreamPosition + attr.Name.Length + 1, ESegmentParseMethod.ForScript);
    //          }
    //        }
    //      }
    //      break;
    //  }
    //}

    //private void ExtractSegments(string fromText, int textStartPosition, ESegmentParseMethod parseMethod)
    //{
    //  string text = WebUtility.HtmlDecode(fromText);
    //  //თუ ტექსტი საერთოდ არ შეიცავს ქართულ ასოებს, მაშინ არ გვაინტერესებს
    //  if (!text.Any(c => _par.Alphabet.Contains(c)))
    //    return;
    //  //ჯერ ტექსტი დავშალოთ წინადადებებად და შემდეგ წინადადებები დავშალოთ სიტყვებად

    //  //clsSentenceUriPos LastSertence = null;

    //  switch (parseMethod)
    //  {
    //    case ESegmentParseMethod.Main:
    //      //ის პუნქტუაციის ნიშანი, რომელიც ამთავრებს წინადადებას
    //      Regex re = new Regex(_par.GetSegmentFinisherPunctuationsRegex());

    //      string[] strTestParts = re.Split(text);
    //      if (strTestParts.Length == 1)
    //        CreateSegment(strTestParts[0], "", textStartPosition);
    //      else
    //      {
    //        bool atLeastOneAdded = false;
    //        for (int i = 1; i < strTestParts.Length; i += 2)
    //        {
    //          if (atLeastOneAdded)
    //            _sbAllText.Append(" ");
    //          CreateSegment(strTestParts[i - 1], strTestParts[i], textStartPosition);
    //          atLeastOneAdded = true;
    //        }
    //      }
    //      break;
    //      //case ESegmentParseMethod.ForScript:
    //      //  StringBuilder currentSegment = new StringBuilder();
    //      //  string possibleSymbols = CrowlerMasterData.Instance.GetPossibleSymbols();
    //      //  for (int i = 0; i < fromText.Length; i++)
    //      //  {
    //      //    if (possibleSymbols.Contains(fromText[i]))
    //      //    {
    //      //      if (currentSegment.Length == 0) //თუ ეს სიმბოლო წინადადებაში პირველია დავიმახსოვროთ მისი პოზიცია
    //      //        segmentPosition = i;
    //      //      currentSegment.Append(fromText[i]);
    //      //    }
    //      //    else if (currentSegment.Length > 0)
    //      //    {
    //      //      CreateSegment(currentSegment.ToString(), "", textStartPosition + segmentPosition);
    //      //      //სეგმენტი უკვე შენახულია, ვემზადებით ახალი სიტყვის მისაღებად
    //      //      currentSegment = new StringBuilder();
    //      //    }
    //      //  }
    //      //  if (currentSegment.Length > 0)
    //      //    CreateSegment(currentSegment.ToString(), "", textStartPosition + segmentPosition);
    //      //  break;
    //  }

    //  _sbAllText.Append(Environment.NewLine);
    //}

    //private void CreateSegment(string segmentText, string segmentFinisherText, int startPosition)
    //{
    //  string segmentTextTrimed = segmentText.TrimStartEnd('\'', '#', ' ');
    //  if (segmentTextTrimed == "") return;
    //  //წინადადება ერთ მაინც ქართულ ასოს უნდა შეიცავდეს
    //  if (!segmentTextTrimed.Any(c => _par.Alphabet.Contains(c)))
    //    return;

    //  segmentTextTrimed += segmentFinisherText;
    //  //Segment segment = new Segment(segmentTextTrimed, _url.BatchId, _url.UrlId, startPosition);
    //  //_listOfTextParts.Add(segment);
    //  //ExtractPunctuations(segmentTextTrimed, startPosition, segment);
    //  //ExtractWords(segmentTextTrimed, startPosition, segment);
    //  _sbAllText.Append(segmentTextTrimed);
    //}

    private static string ExtractText(HtmlNode htmlDocDocumentNode)
    {
        StringBuilder sb = new();
        foreach (var node in htmlDocDocumentNode.SelectNodes("//text()"))
        {
            if (node.ParentNode.Name == "script" || node.ParentNode.Name == "style")
                continue;

            var text = WebUtility.HtmlDecode(node.InnerText).Trim();

            if (text == string.Empty)
                continue;

            if (node.NextSibling != null && node.NextSibling.Name == "b")
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
        foreach (var link in htmlDocDocumentNode.SelectNodes("//a[@href]"))
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
        Regex re = new(_par.SegmentFinisherPunctuationsRegex);
        var strTestParts = re.Split(context);
        if (strTestParts.Length == 1)
        {
            //CreateSegment(strTestParts[0], "", 0);
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

            AddStatementStart();
            ParsePunctuations(strTestParts[^1]);
            AddStatementFinish();
        }
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
        //ყველა ის პუნქტუაციის ნიშანი, რომელიც არ შეიძლება აღიქმებოდეს სიტყვის ნაწილად
        Regex re = new(_par.WordDelimiterRegex);
        var strTestParts = re.Split(context);
        if (strTestParts.Length < 3)
        {
            AddWord(strTestParts[0]);
        }
        else
        {
            for (var i = 2; i < strTestParts.Length; i += 3) AddWord(strTestParts[i - 2]);

            AddWord(strTestParts[^1]);
        }
    }

    private void AddWord(string word)
    {
        if (word != string.Empty)
            UriTerms.Add(new UriTerm(ETermType.Word, word));
    }


    //private void ParseHtmlNodeAndChildren(HtmlNode htmlNode)
    //{
    //  Queue<HtmlNodeParse> nodes = new Queue<HtmlNodeParse>();
    //  nodes.Enqueue(new HtmlNodeParse(htmlNode, false));
    //  while (nodes.Count > 0)
    //  {
    //    HtmlNodeParse currentHtmlNodeParse = nodes.Dequeue();
    //    HtmlNode currentHtmlNode = currentHtmlNodeParse.HtmlNode;
    //    ParseHtmlNode(currentHtmlNode, currentHtmlNodeParse.InScript);
    //    if (currentHtmlNode.NodeType == HtmlNodeType.Comment || currentHtmlNode.NodeType == HtmlNodeType.Text)
    //      continue;
    //    bool inScript = (currentHtmlNode.NodeType == HtmlNodeType.Element &&
    //                     currentHtmlNode.Name.ToLower() == "script");
    //    foreach (HtmlNode childNode in currentHtmlNode.ChildNodes)
    //      nodes.Enqueue(new HtmlNodeParse(childNode, inScript));
    //  }
    //}


    //private void ParseHtmlNode(HtmlNode htmlNode, bool inScript = false)
    //{

    //  //if (_bp != null)
    //  //  _bp.SubCounted = htmlNode.StreamPosition;
    //  switch (htmlNode.NodeType)
    //  {
    //    case HtmlNodeType.Comment:
    //      ExtractSegments(htmlNode.InnerText, htmlNode.StreamPosition, ESegmentParseMethod.ForScript);
    //      break;
    //    case HtmlNodeType.Text:
    //      ExtractSegments(htmlNode.InnerText, htmlNode.StreamPosition, (inScript ? ESegmentParseMethod.ForScript : ESegmentParseMethod.Main));
    //      break;
    //    default:
    //      if (htmlNode.NodeType == HtmlNodeType.Element)
    //      {
    //        foreach (HtmlAttribute attr in htmlNode.Attributes)
    //        {
    //          string attrName = attr.Name.ToLower();
    //          if (attrName == "href" || attrName == "src")
    //            ExtractUrl(attr.Value);
    //          else
    //          {
    //            ExtractSegments(attr.Name, htmlNode.StreamPosition, ESegmentParseMethod.ForScript);
    //            ExtractSegments(attr.Value, htmlNode.StreamPosition + attr.Name.Length + 1, ESegmentParseMethod.ForScript);
    //          }
    //        }
    //      }
    //      break;
    //  }
    //}


    //, int textStartPosition, ESegmentParseMethod parseMethod
    //private void ExtractSegments(string fromText)
    //{
    //  string text = WebUtility.HtmlDecode(fromText);
    //  //თუ ტექსტი საერთოდ არ შეიცავს ქართულ ასოებს, მაშინ არ გვაინტერესებს
    //  if (!text.Any(c => _par.Alphabet.Contains(c)))
    //    return;
    //  //ჯერ ტექსტი დავშალოთ წინადადებებად და შემდეგ წინადადებები დავშალოთ სიტყვებად

    //  int segmentPosition = 0;

    //  //switch (parseMethod)
    //  //{
    //  //  case ESegmentParseMethod.Main:
    //  //ის პუნქტუაციის ნიშანი, რომელიც ამთავრებს წინადადებას
    //  Regex re = new Regex(_par.GetSegmentFinisherPunctuationsRegex());
    //  string[] strTestParts = re.Split(text);
    //  if (strTestParts.Length == 1)
    //    CreateSegment(strTestParts[0], "", 0);
    //  else
    //  {
    //    //bool atLeastOneAdded = false;
    //    for (int i = 1; i < strTestParts.Length; i += 2)
    //    {
    //      //if (atLeastOneAdded)
    //      //  _sbAllText.Append(" ");
    //      CreateSegment(strTestParts[i - 1], strTestParts[i], segmentPosition);
    //      segmentPosition += strTestParts[i - 1].Length;
    //      segmentPosition += strTestParts[i].Length;
    //      //atLeastOneAdded = true;
    //    }
    //    if (strTestParts.Length % 2 == 1)
    //      CreateSegment(strTestParts[^1], "", segmentPosition);
    //  }
    //  //    break;
    //  //  case ESegmentParseMethod.ForScript:
    //  //    StringBuilder currentSegment = new StringBuilder();
    //  //    string possibleSymbols = _par.GetPossibleSymbols();
    //  //    for (int i = 0; i < fromText.Length; i++)
    //  //    {
    //  //      if (possibleSymbols.Contains(fromText[i]))
    //  //      {
    //  //        if (currentSegment.Length == 0) //თუ ეს სიმბოლო წინადადებაში პირველია დავიმახსოვროთ მისი პოზიცია
    //  //          segmentPosition = i;
    //  //        currentSegment.Append(fromText[i]);
    //  //      }
    //  //      else if (currentSegment.Length > 0)
    //  //      {
    //  //        CreateSegment(currentSegment.ToString(), "", textStartPosition + segmentPosition);
    //  //        //სეგმენტი უკვე შენახულია, ვემზადებით ახალი სიტყვის მისაღებად
    //  //        currentSegment = new StringBuilder();
    //  //      }
    //  //    }
    //  //    if (currentSegment.Length > 0)
    //  //      CreateSegment(currentSegment.ToString(), "", textStartPosition + segmentPosition);
    //  //    break;
    //  //}

    //  //_sbAllText.Append(Environment.NewLine);
    //}


    private void ExtractUrl(string uriCandidate)
    {
        if (uriCandidate.StartsWith("mailto:"))
            return; //ელექტრონული ფოსტის მისამართების შენახვა არ გვჭირდება

        //UrlEncoder urlEncoder = UrlEncoder.Default;
        //string strUri = urlEncoder.Encode(uriCandidate);
        var strUri = uriCandidate.Trim('"', '\'', '#', ' ', '>');
        try
        {
            var newUri = UriFabric.GetUri(strUri);
            if (newUri == null || !newUri.IsAbsoluteUri) //if (struri.IndexOf("..") != -1 || struri.StartsWith("/"))
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

            if (newUri != null)
            {
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
                if (startQuery != "")
                {
                    //თუ მისამართი შეიცავს ქვერის ნაწილს
                    var newQuery = NormalizeQuery(startQuery, '&');
                    newQuery = NormalizeQuery(newQuery, ';');
                    //ფრაგმენტი არა გვჭირდება +AbsUri.Fragment;
                    strUri = newUri.Scheme + "://" + newUri.Authority + newUri.LocalPath + newQuery;

                    //if (ProcData.Instance.ProhibitedQueries.ContainsKey(newUri.Host))
                    //{
                    //  char[] delimiters = { '&', ';' };
                    //  foreach (string queryitem in newQuery.Substring(1).Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Distinct())
                    //  {
                    //    string[] qp = queryitem.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    //    if (qp.Length > 0 &&
                    //      ProcData.Instance.ProhibitedQueries[newUri.Host].Contains(qp[0]))
                    //      return;
                    //  }
                    //}
                    //თუ მისამართი შეიცავს ქვერის ნაწილს ვინახავთ ქვერის გამოყენებით
                    //და მერე კიდევ ქვერის გარეშეც ერთი სტრიქონის მერე
                    AddUriUri(strUri);
                }

                //ქვერის ნაწილს შეიცავს თუ არა მისამართი ქვერის გარეშე ვარიანტსაც ვინახავთ
                strUri = newUri.Scheme + "://" + newUri.Authority +
                         newUri.LocalPath; //ფრაგმენტი არა გვჭირდება +AbsUri.Fragment;
                AddUriUri(strUri);

                ////მოვახდინოთ LocalPath ნაწილის ნელნელა დაპატარავება, სანამ არ მოხდება მისი დაყვანა ერთ სიმბოლომდე /
                //int segmentsCount = newUri.Segments.Length;
                //if (segmentsCount > 1)
                //{
                //  for (int i = segmentsCount - 1; i > 0; i--)
                //  {
                //    StringBuilder sbPath = new StringBuilder();
                //    //string curPath = "";
                //    for (int j = 0; j < i; j++)
                //      sbPath.Append(newUri.Segments[j]);
                //    strUri = newUri.Scheme + "://" + newUri.Authority + sbPath;
                //    _listOfUriUri.Add(new UriUri(_batchPart.BpId, _url.UrlId, strUri));//, true
                //  }
                //}
                //Loger.Instance.LogMessage("Segments of uri: " +newUri.ToString());
                //foreach (string urisegment in newUri.Segments)
                //{
                //  Loger.Instance.LogMessage(urisegment);   
                //}
                //Loger.Instance.LogMessage("!");
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

    //private void CreateSegment(string segmentText, string segmentFinisherText, int startPosition)
    //{
    //  string segmentTextTrimmed = segmentText.TrimStartEnd('\'', '#', ' ');
    //  if (segmentTextTrimmed == "") return;
    //  //წინადადება ერთ მაინც ქართულ ასოს უნდა შეიცავდეს
    //  if (!segmentTextTrimmed.Any(c => _par.Alphabet.Contains(c)))
    //    return;
    //  segmentTextTrimmed += segmentFinisherText;
    //  SegmentTerm segment = new SegmentTerm(segmentTextTrimmed, _batchPart.BpId, _url.UrlId, startPosition);
    //  _listOfTextParts.Add(segment);
    //  ExtractPunctuations(segmentTextTrimmed, startPosition, segment);
    //  ExtractWords(segmentTextTrimmed, startPosition, segment);
    //  //_sbAllText.Append(segmentTextTrimmed);
    //}


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

        var parts = startQuery[1..].Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Distinct()
            .ToArray();
        var newQuery = "";
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

    //private void ExtractPunctuations(string fromText, int textStartPosition, TextTerm segment)
    //{
    //  int punctuationPosition = 0;
    //  Regex re = new Regex(_par.GetPunctuationsRegex()); //ყველა პუნქტუაციის ნიშანი
    //  string[] strTestParts = re.Split(fromText);
    //  for (int i = 1; i < strTestParts.Length; i += 2)
    //  {
    //    //CreatePunctuation(strTestParts[i], strTestParts[i + 1], textStartPosition + punctuationPosition);
    //    if (strTestParts[i].Length > 0)
    //    {
    //      punctuationPosition += strTestParts[i - 1].Length;
    //      _listOfTextParts.Add(new PunctuationTerm(strTestParts[i], _batchPart.BpId, _url.UrlId,
    //        textStartPosition + punctuationPosition, segment));
    //      punctuationPosition += strTestParts[i].Length;
    //    }
    //  }
    //}

    //V3
    //private void ExtractWords(string fromText, int textStartPosition, TextTerm segment)
    //{
    //  int wordPosition = 0;
    //  Regex re = new Regex(_par.GetWordDelimiterRegex()); //ყველა ის პუნქტუაციის ნიშანი, რომელიც არ შეიძლება აღიქმებოდეს სიტყვის ნაწილად
    //  string[] strTestParts = re.Split(fromText);
    //  if (strTestParts.Length < 3)
    //    CreateWord(strTestParts[0], _url.UrlId, textStartPosition, segment);
    //  else
    //  {
    //    for (int i = 2; i < strTestParts.Length; i += 3)
    //    {
    //      CreateWord(strTestParts[i - 2], _url.UrlId, textStartPosition + wordPosition, segment);
    //      wordPosition += strTestParts[i - 2].Length;
    //      wordPosition += strTestParts[i - 1].Length;
    //    }
    //    CreateWord(strTestParts[^1], _url.UrlId, textStartPosition + wordPosition, segment);
    //  }
    //}

    //private void CreateWord(string context, int urlId, int startPosition, TextTerm segment)
    //{
    //  if (context.Length <= 0)
    //    return;
    //  WordTerm word = new WordTerm(context, _batchPart.BpId, urlId, startPosition, segment, _par);
    //  _listOfTextParts.Add(word);
    //  //if (word.WordCategoryId < 4)
    //  //  _hasInterestWords = true;
    //}
}