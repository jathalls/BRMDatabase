namespace BRMDatabase.Tables;

 public class LabelledSegment
 {
     

     public int ID { get; set; }

  
     public int RecordingID { get; set; } = 0;

    public Recording? Recording { get; set; } = null;


     public DateTime StartOffset { get; set; }

     public DateTime EndOffset { get; set; } 

     [XmlAttribute("Comment")]
     public string Comment { get; set; } = "";

     [XmlAttribute("AutoID")]
     public string AutoID { get; set; } = string.Empty;

     [XmlAttribute("AutoIdProb")]
     public double AutoIdProb { get; set; } = 0.0d;

    public int SessionID { get; set; } = -1;

    public RecordingSession? Session { get; set; } = null;

    public ICollection<IdedBat> IdedBats { get; set; } = new List<IdedBat>();

    public ICollection<Call> SegmentCalls { get; set; } = new List<Call>();


}
 