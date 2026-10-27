using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BRMDatabase.Tables;


public class Recording : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string PropertyName = "") { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName)); }

    

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

    public DateTime _recordingDate = new DateTime();

    
    public int SessionID { get; set; } = 0;

    public virtual RecordingSession Session { get; set; } = null;

    public virtual ICollection<LabelledSegment> LabelledSegments { get; set; } = new List<LabelledSegment>();

    public virtual ICollection<Meta> Metas { get; set; } = new List<Meta>();

    public virtual ICollection<IdedBat> IdedBats { get; set; } = new List<IdedBat>();

}
