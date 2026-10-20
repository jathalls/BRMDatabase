namespace BRMDatabase.Tables;


public class Recording
{
    
    public int ID { get; set; } = 0;

    
    private string _recordingName = "";

    [XmlAttribute("RecordingName")]
    public string RecordingName 
    {
        get => _recordingName;
        set
        {
            if(value.StartsWith(@"\") || value.StartsWith(@"/"))
            {
                value = value.Substring(1);
            }
            _recordingName = value;
        }
    }

    public DateTime RecordingStartTime { get; set; }

    public DateTime RecordingEndTime { get; set; }

    [XmlAttribute("RecordingGPSLongitude")]
    public double RecordingGPSLongitude { get; set; } = 200.0d;

    [XmlAttribute("RecordingGPSLatitude")]
    public double RecordingGPSLatitude { get; set; } = 200.0d;

    [XmlAttribute("RecordingNotes")]
    public string RecordingNotes { get; set; } = string.Empty;

    [XmlAttribute("RecordingDate")]
    public DateTime RecordingDate
    {
        get { return _recordingDate; }
        set { _recordingDate = value; }
    }

    private DateTime _recordingDate = new DateTime();

    
    public int SessionID { get; set; } = 0;

    public RecordingSession Session { get; set; } = null;

    public ICollection<LabelledSegment> LabelledSegments= new List<LabelledSegment>();

    public ICollection<Meta> Metas=new List<Meta>();

    public ICollection<IdedBat> IdedBats { get; set; } = new List<IdedBat>();

}
