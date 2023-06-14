namespace LibRobotsTextGramParser
{
  public static class RobotsTextGrammarFabric
  {

    //JSON sample
//    @"
//Program => Statement Program | Statement
//Statement => FunctionCall Semicolon | AssignmentExpression Semicolon
//FunctionCall => Identifier OpenBracket CloseBracket | Identifier OpenBracket ArgumentList CloseBracket
//ArgumentList => AdditiveExpression Comma ArgumentList | AdditiveExpression
//AssignmentExpression => ExpressionName Equals AdditiveExpression
//AdditiveExpression => MultiplicativeExpression PlusMinus AdditiveExpression | MultiplicativeExpression
//MultiplicativeExpression => UnaryExpression DivMulMod MultiplicativeExpression | UnaryExpression
//UnaryExpression => PlusMinus Expression | Expression
//Expression => String | ExpressionName | FunctionCall | TRUE | FALSE | Number
//ValueExpression => Equals AdditiveExpression
//ExpressionName => Identifier Dot Identifier
//Semicolon => ;
//Identifier => [A-Za-z_]\w*
//OpenBracket => \(
//CloseBracket => \)
//Comma => ,
//Equals => =
//String => \""[^\""]*\""
//Dot => \.
//PlusMinus => [\+\-]
//DivMulMod => [/\*%]
//TRUE => true
//FALSE => false
//Number => IntNumber Frac Exp | IntNumber Frac | IntNumber Exp | IntNumber
//IntNumber => [0-9]*
//Frac => [\.][0-9]+
//Exp => [eE][-+]?[0-9]+
//", "Program");

//FIXME https://developers.google.com/search/docs/advanced/robots/robots_txt
//y => *(x) კონსტრუქცია იცვლება y => x y | x
//თუ x რადენიმე ნაწილისგან შედგება, მაშინ ჯერ უნდა გაკეთდეს x => zzz 

    private const string RobotsTextAbnf = @"robotstxt => grouper robotstxt | grouper
grouper => group | EOL
group => startgroupline startgrouplines ruleLines sitemapLines
startgrouplines => startgroupliner startgrouplines | startgroupliner
startgrouplinesr => groupStartLine | EOL
ruleLines => ruleLiner ruleLines | ruleLiner
ruleLiner => rule | EOL
sitemapLines => sitemapLiner sitemapLines | sitemapLiner
sitemapLiner => sitemapLine | EOL
groupStartLine => WSS user-agent WSS : WSS product-token EOL
rule => WSS command WSS : WSS pattern EOL
sitemapLine => WSS sitemap WSS : WSS URL EOL
product-token = identifier | \*
command => allow | disallow
pattern => path-pattern | WSS
path-pattern => \/ | UTF8NoctlChars
identifier => [\-A-Z_a-z]
comment => # CommentPartes
CommentPartes => CommentPart CommentPartes | CommentPart
CommentPart => UTF8-char-noctl | WS | #
url => https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)
EOL => WSS comment NL | WSS NL
NL => CR | LF |  CR LF
CR => %x0D
LF => %x0A
WSS => WS WSS | WS
WS => [\x20\x09]
UTF8NoctlChars => UTF8-char-noctl UTF8NoctlChars | UTF8-char-noctl
UTF8-char-noctl => UTF8-1-noctl | UTF8-2 | UTF8-3 | UTF8-4
UTF8-1-noctl => \x21 | \x22 | [\x24-\x7F]
UTF8-2 => [\xC2-\xDF] UTF8-tail
UTF8-3 => \xE0 [\xA0-\xBF] UTF8-tail | [\xE1-\xEC] UTF8-tail UTF8-tail | \xED [\x80-\x9F] UTF8-tail | [\xEE-\xEF UTF8-tail UTF8-tail
UTF8-4 => \xF0 [\x90-\xBF] UTF8-tail UTF8-tail | [\xF1-\xF3] UTF8-tail UTF8-tail UTF8-tail | \xF4 [\x80-\x8F] UTF8-tail UTF8-tail
UTF8-tail => [\x80-\xBF]
";

    private static Grammar _grammarMemo;

    public static Grammar GetGrammar()
    {
      return _grammarMemo ??= new Grammar(RobotsTextAbnf, "");
    }
  }
}
