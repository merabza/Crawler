using GramParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibRobotsTextGramParser
{
  public class Grammar
  {
    private readonly string _pStartSymbol;
    private readonly Dictionary<string, List<OneAlternative>> _g = new();
    private string _tokenizer;
    private readonly ParseTree _fail = new(true);


    public Grammar(string description, string startSymbol, string whitespace = "\\s*")
    {
      string pDescription = description;
      _pStartSymbol = startSymbol;
      //"""Convert a description to a grammar.  Each line is a rule for a
      //non-terminal symbol; it looks like this:
      //გრამატიკის აღწერის სტრიქონი უნდა დაკონვერტირდეს გრამატიკის სტრუქტურაში
      //ყოველი სტრიქონი წარმოადგენს წესს არატრემინალური სიმბოლოსათვის
      //ის გამოიყურება მიახლოებით ასე:
      //    Symbol =>  A1 A2 ... | B1 B2 ... | C1 C2 ...
      //where the right-hand side is one or more alternatives, separated by
      //the '|' sign.  
      //სადაც მარჯვენა ნაწილი არის ერთი ან მეტი ალტერნატივა, გამოყოფილი სიმბოლოთი '|'
      //Each alternative is a sequence of atoms, separated by spaces.
      //ყოველი ალტერნატივა არის ატომების თანმიმდევრობა, რომელიც გამოყოფილია ჰარი სიმბოლოთი.
      //An atom is either a symbol on some left-hand side, or it is a regular expression that will be passed to re.match to match a token.
      //ატომი არის სიმბოლო, რომელიც შეიძლება შეგვხვდეს წესების მარცხენა ნაწილში, ან უნდა იყოს რეგულარული გამოსახულება, 
      //რომელიც საშუალებას იძლევა გამოვყოთ მინიმალური ტერმინალური სიმბოლო.
      //Notation for *, +, or ? not allowed in a rule alternative (but ok within a token). 
      //
      //
      //Use '\' to continue long lines.  
      //
      //You must include spaces or tabs around '=>' and '|'. That's within the grammar description itself.
      //აუცილებელია პრობელების აქეთ-იქიდან ჩასმა სიმბოლოებისათვის '=>' და '|'. ასეა განსაზღვრული თვითონ გრამატიკის აღწერაში
      //The grammar that gets defined allows whitespace between tokens by default;
      //გრამატიკას ტოკენებს შორის უნდა ჰქონდეს პრობელები
      //specify '' as the second argument to grammar() to disallow this (or supply any regular expression to describe allowable whitespace between tokens)."""
      //
      //G = {' ': whitespace}
      List<OneAlternative> whitespaceAlternativeList = new List<OneAlternative>
      {
        new(whitespace, false)
      };
      _g.Add("whitespace", whitespaceAlternativeList);
      //description = description.replace('\t', ' ') # no tabs!
      pDescription = pDescription.Replace('\t', ' ');
      //for line in split(description, '\n'):
      string[] descLines = pDescription.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string[] hs in descLines.Select(str => str.Split(new[] { " => " }, 2, StringSplitOptions.None)))
      {
        _g.Add(hs[0], new List<OneAlternative>());
        //alternatives = split(rhs, ' | ')
        string[] strAlternatives = hs[1].Split(new[] { " | " }, StringSplitOptions.None);
        //G[lhs] = tuple(map(split, alternatives))
        OneAlternative[] alternatives = new OneAlternative[strAlternatives.Length];
        for (int a = 0; a < strAlternatives.Length; a++)
          alternatives[a] = new OneAlternative(strAlternatives[a]);
        _g[hs[0]].AddRange(alternatives);
      }
      //return G

    }

    //def parse(start_symbol, text, grammar):
    public ParseTree Parse(string text, string startSymbol = "" )
    {
      string myStartSymbol = startSymbol;
      if (myStartSymbol == "")
        myStartSymbol = _pStartSymbol;
      //    """Example call: parse('Exp', '3*x + b', G).
      //    Returns a (tree, remainder) pair. If remainder is '', it parsed the whole
      //    string. Failure iff remainder is None. This is a deterministic PEG parser,
      //    so rule order (left-to-right) matters. Do 'E => T op E | T', putting the
      //    longest parse first; don't do 'E => T | T op E'
      //    Also, no left recursion allowed: don't do 'E => E op T'"""

      _tokenizer = "^" + _g["whitespace"][0][0] + "({0})";
      //    # Body of parse:
      return MemoParseAtom(myStartSymbol, text);
    }


    //    def parse_sequence(sequence, text):
    private ParseTree ParseSequence(OneAlternative sequence, string text)
    {
      //result = []
      ParseTree result = new ParseTree();
      string currentText = text;
      //for atom in sequence:
      for (int i = 0; i < sequence.Count; i++)
      {
        //tree, text = parse_atom(atom, text)
        ParseTree atomParseResult = MemoParseAtom(sequence[i], currentText);
        //if text is None: return Fail
        if (atomParseResult.IsFail)
          return _fail;
        //result.append(tree)
        currentText = atomParseResult.Text;
        result.Append(atomParseResult);
      }
      //return result, text
      result.Text = currentText;
      return result;
    }

    private readonly Dictionary<string, Dictionary<string, ParseTree>> _memo = new();

    private ParseTree MemoParseAtom(string atom, string text)
    {
      if (_memo.ContainsKey(atom) && _memo[atom].ContainsKey(text))
        return _memo[atom][text];
      ParseTree result = ParseAtom(atom, text);
      if (!_memo.ContainsKey(atom))
        _memo.Add(atom, new Dictionary<string, ParseTree>());
      _memo[atom].Add(text, result);
      return result;
    }

    //    @memo
    //    def parse_atom(atom, text):
    private ParseTree ParseAtom(string atom, string text)
    {
      //if atom in grammar:  # Non-Terminal: tuple of alternatives
      if (_g.ContainsKey(atom))
      {
        //for alternative in grammar[atom]:
        foreach (OneAlternative alternative in _g[atom])
        {
          //tree, rem = parse_sequence(alternative, text)
          ParseTree tree = ParseSequence(alternative, text);
          //if rem is not None: return [atom]+tree, rem  
          if (!tree.IsFail)
          {
            //clsParseTree AtomTree = new clsParseTree(new clsParseTree(Atom));
            ParseTree atomTree = new ParseTree(new GrammarToken(atom));
            atomTree.Concatenate(tree);
            atomTree.Text = tree.Text;
            return atomTree;
          }
        }
        //return Fail
        return _fail;
      }
      //else:  # Terminal: match characters against start of text
      //Console.WriteLine("Text");
      //Console.WriteLine(Text);
      //Console.WriteLine("Atom");
      //Console.WriteLine(Atom);
      //Console.WriteLine("string.Format(tokenizer, Atom)");
      //Console.WriteLine(string.Format(tokenizer, Atom));
      //    m = re.match(tokenizer % atom, text)
      Regex re = new Regex(string.Format(_tokenizer, atom));
      Match m = re.Match(text);
      //    return Fail if (not m) else (m.group(1), text[m.end():])
      //Console.WriteLine("m");
      //Console.WriteLine(m.Success.ToString());
      if (!m.Success)
        return _fail;
      //Match m = re.Match(Text);
      ParseTree terminalTree = new ParseTree(new GrammarToken(m.Groups[1].Value)) {Text = text.Substring(m.Length)};
      return terminalTree;
    }


  }
}
