using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BRMDatabase.Tables;


public class RecordingSession :INotifyPropertyChanged

{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string PropertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
    }

    public int ID { get; set; }

    [XmlAttribute("SessionTag")]
    public string SessionTag { get; set; } = string.Empty;

    public DateTime SessionStart { get; set; }

    public DateTime SessionEnd { get; set; }

    

    [XmlAttribute("Temp")]
    public decimal Temp { get; set; }

    [XmlAttribute("Equipment")]
    public string Equipment { get; set; }

    [XmlAttribute("Microphone")]
    public string microphone { get; set; }

    [XmlAttribute("Operator")]
    public string Operator { get; set; }

    [XmlAttribute("Location")]
    public string Location { get; set; }

    
    [XmlAttribute("LocationGPSLongitude")]
    public decimal LocationGPSLongitude { get; set; }

    [XmlAttribute("LocationGPSLatitude")]

    public decimal LocationGPSLatitude { get; set; }

    [XmlAttribute("SessionNotes")]
    public string SessionNotes { get; set; } = string.Empty;

    [XmlAttribute("OriginalFilePath")]
    public string OriginalFilePath { get; set; } = string.Empty;

    
    public DateTime Sunset{get;set;}

    [XmlAttribute("Weather")]
    public string Weather { get; set; } = string.Empty;

    public virtual ICollection<Recording> Recordings { get; set; } = new List<Recording>();

    public virtual  ICollection<LabelledSegment> Segments { get; set; } = new List<LabelledSegment>();

    public virtual ICollection<IdedBat> IdedBats { get; set; } = new List<IdedBat>();

}
