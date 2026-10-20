using System.Xml.Serialization;
namespace BRMDatabase.Tables;


 public class IdedBat
 {
 
     public int ID { get; set; }


     [XmlAttribute("BatName")]
     public string Name { get; set; } = "";


    
     public bool ByAutoId { get; set; } = false;


     public int SegmentID { get; set; } = -1;

   
    public int RecordingID { get; set; } = -1;

   
    public int SessionID { get; set;  } = -1;

    public LabelledSegment? Segment { get; set; } = null;

    public Recording? Recording { get; set;  } = null;

    public RecordingSession? Session { get; set; } = null;

    
 }
