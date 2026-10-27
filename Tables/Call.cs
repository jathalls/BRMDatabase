
namespace BRMDatabase.Tables;


public class Call
{
 
    public int ID { get; set; }

    [XmlAttribute("CallFunction")]
    public string CallFunction { get; set; } = string.Empty;

    [XmlAttribute("CallNotes")]
    public string CallNotes { get; set; } = string.Empty;

    [XmlAttribute("CallType")]
    public string CallType { get; set; } = string.Empty;

    

    [XmlAttribute("PeakFrequency")]
    public double PeakFrequency { get; set; } = 0.0d;

    [XmlAttribute("PeakFrequencyVariation")]
    public double PeakFrequencyVariation { get; set; } = 0.0d;

    [XmlAttribute("PulseInterval")]
    public double PulseInterval { get; set; } = 0.0d;

    [XmlAttribute("PulseIntervalVariation")]
    public double PulseIntervalVariation { get; set; } = 0.0d;

    [XmlAttribute("PulseDuration")]
    public double PulseDuration { get; set; } = 0.0d;

    [XmlAttribute("PulseDurationVariation")]
    public double PulseDurationVariation { get; set; } = 0.0d;

    [XmlAttribute("StartFrequency")]
    public double StartFrequency { get; set; } = 0.0d;

    [XmlAttribute("StartFrequencyVariation")]
    public double StartFrequencyVariation { get; set; } = 0.0d;

    [XmlAttribute("EndFrequency")]
    public double EndFrequency { get; set; } = 0.0d;

    [XmlAttribute("EndFrequencyVariation")]
    public double EndFrequencyVariation { get; set; } = 0.0d;

    [XmlAttribute("FirstHalfPeak")]
    public double FirstHalfPeak { get; set; } = 0.0d;

    [XmlAttribute("FirstHalfPeakVariation")]
    public double FirstHalfPeakVariation { get; set; } = 0.0d;

    [XmlAttribute("SecondHalfPeak")]
    public double SecondHalfPeak { get; set; } = 0.0d;

    [XmlAttribute("SecondHalfPeakVariation")]
    public double SecondHalfPeakVariation { get; set; } = 0.0d;


    [XmlAttribute("ByAutoID")]
    public bool ByAutoID { get; set; } = false;
 
    public int batID { get; set; } = -1;
  
    public int SegmentID { get; set; } = -1;

    public virtual Bat? Bat { get; set; } = null;

    public virtual LabelledSegment? Segment { get; set; } = null;
}
