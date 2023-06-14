//using System.Linq;
//using DoCrawler.Models;

//namespace DoCrawler.TextParts
//{
//  public sealed class WordTerm : TextTerm
//  {
//    private readonly CrawlerParameters _par;
//    public readonly int WordCategoryId;

//    public WordTerm(string context, int batchPartId, int urlId, int startPosition, TextTerm segment, CrawlerParameters par)
//      : base(context, batchPartId, urlId, startPosition, "Word", segment)
//    {
//      _par = par;
//      WordCategoryId = GetWordCategoryId();
//    }


////wcID	wcKey	wcName
////1	MainABC	შეიცავს მხოლოდ ძირითადი ანბანის ასოებს
////2	MainABCExt	შეიცავს ძირითადი ანბანის ასოებს და დამატებით სიმბოლოებს
////3	AtLeastOneFromMainABC	შეიცავს ერთ მაინც ასოს ძირითადი ანბანიდან
////4	Digits	შეიცავს მხოლოდ არაბულ ციფრებს
////5	NoOneFromMainABC	არ შეიცავს არცერთ სიმბოლოს ძირითადი ანბანიდან და არ შედგება მხოლოდ ციფრებისაგან
//    private int GetWordCategoryId()
//    {
//      //თუ ტექსტი შეიცავს მხოლოდ ქართულ ასოებს, მაშინ ყოფილა პირველი კატეგორია
//      if (Context.All(c => _par.Alphabet.Contains(c)))
//        return 1;
//      //თუ ტექსტი შეიცავს მხოლოდ არაბულ ციფრებს, ყოფილა მე-4 კატეგორია
//      if (Context.All(char.IsDigit))
//        return 4;
//      //თუ ტექსტი საერთოდ არ შეიცავს ქართიულ ასოებს, მაშინ ყოფილა მე-5 კატეგორია
//      if (!Context.Any(c => _par.Alphabet.Contains(c)))
//        return 5;
//      //თუ ტექსტი შეიცავს მხოლოდ ქართულ ასოებს, ან მხოლოდ დამატებით სიმბოლოებს და არ შეიცავს დანარჩენ არაფერს
//      return Context.All(c => _par.Alphabet.Contains(c) || _par.ExtraSymbols.Contains(c)) ? 2 : 3;
//      //წინა ოთხი შემოწმების მერე
//    }


//    //public override void SaveToBase()
//    //{
//    //  DatabaseClient databaseClient = DatabaseFactory.GetDatabaseClient();
//    //  DbManager dbm = new DbManager();
//    //  int wordId = databaseClient.TrySaveWord(dbm, Context, WordCategoryId);
//    //  if (wordId != 0)
//    //    SaveToBaseTextPartsByUrLs(wordId);
//    //}


//  }
//}

