//namespace DoCrawler.TextParts
//{
//  public sealed class SegmentTerm : TextTerm
//  {

//    public SegmentTerm(string context, int batId, int urlId, int startPosition)
//      : base(context, batId, urlId, startPosition, "Segment")
//    {
//    }

//    //public override void SaveToBase()
//    //{
//    //  int segmentId = TrySaveSegment();
//    //  if (segmentId != 0)
//    //    SaveToBaseTextPartsByUrLs(segmentId);
//    //}


//    //private int TrySaveSegment()
//    //{
//    //  DatabaseClient databaseClient = DatabaseFactory.GetDatabaseClient();
//    //  DbManager dbm = new DbManager();

//    //  int segmentHashCode = Context.GetHashCode();
//    //  int segmentId = databaseClient.GetSegmentId(dbm, Context, segmentHashCode);

//    //  if (segmentId == 0)
//    //    segmentId = databaseClient.AddSegment(dbm, Context, segmentHashCode);
//    //  return segmentId;
//    //}


//  }
//}

