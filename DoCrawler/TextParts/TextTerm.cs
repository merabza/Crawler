//using System.Collections.Generic;

//namespace DoCrawler.TextParts;

//public sealed class TextTerm // : IForBase
//{
//    protected string Context;
//    private readonly int _urlId;
//    private readonly int _batchPartId;
//    private readonly int _startPosition;
//    private readonly int _endPosition;
//    private readonly int _tptId; //TextPartTypeID
//    private readonly TextTerm _parentTextPart;

//    private readonly List<TextTerm> _textPartsList = new();
//    //private int pVirtualID;

//    //private string Context { get { return _context; } }
//    //public int StartPosition { get { return _startPosition; } }
//    public int ReallId { get; private set; }

//    protected TextTerm(string context, int batchPartId, int urlId, int startPosition, string partName,
//        TextTerm parentTextPart = null)
//    {
//        Context = context;
//        _batchPartId = batchPartId;
//        _urlId = urlId;
//        _startPosition = startPosition;
//        _endPosition = startPosition + context.Length;
//        //_tptId = CrowlerMasterData.Instance.GetTextPartTypeIdFor(partName);
//        _parentTextPart = parentTextPart;
//    }

//    internal void AddContextLine(TextTerm textPart)
//    {
//        if (Context != "")
//            Context += "\r\n";
//        Context += textPart.Context;
//        AddChildTextPart(textPart);
//    }

//    private void AddChildTextPart(TextTerm textPart)
//    {
//        _textPartsList.Add(textPart);
//    }

//    protected int GetSortId()
//    {
//        return 0;
//    }

////    protected void SaveToBaseTextPartsByUrLs(int textPartRecordId)
////    {
////      DbManager dbm = new DbManager();
////      try
////      {
////        dbm.Open();
////        dbm.AddParameter("@urlID", _urlId);
////        dbm.AddParameter("@tptID", _tptId);
////        dbm.AddParameter("@tpID", textPartRecordId);
////        dbm.AddParameter("@tpuStartPosition", _startPosition);
////        dbm.AddParameter("@tpuEndPosition", _endPosition);
////        dbm.AddParameter("@batID", _batId);
////        ReallId = (int) dbm.ExecuteScalar<decimal>(
////          @"INSERT INTO TextPartsByURLs (urlID, tptID, tpID, tpuStartPosition, tpuEndPosition, batID) 
////VALUES (@urlID, @tptID, @tpID, @tpuStartPosition, @tpuEndPosition, @batID)
////SELECT SCOPE_IDENTITY() as iden");


////      }
////      catch (Exception ex)
////      {
////        Loger.Instance.LogMessage(ex);
////      }
////      finally
////      {
////        dbm.Close();
////      }

////      if (_parentTextPart != null)
////      {
////        TextPartRelation textPartRelation = new TextPartRelation(_urlId, _parentTextPart, this);
////        textPartRelation.SaveToBase();
////      }

////      foreach (TextPart tp in _textPartsList)
////      {
////        TextPartRelation textPartRelation = new TextPartRelation(_urlId, this, tp);
////        textPartRelation.SaveToBase();
////      }
////    }


//    public virtual void SaveToBase()
//    {

//    }

//}

