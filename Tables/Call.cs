
using System.ComponentModel.DataAnnotations.Schema;

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

    [XmlAttribute("KneeFrequency")]
    public double KneeFrequency { get; set; } = 0.0d;

    [XmlAttribute("KneeFrequencyVariation")]
    public double KneeFrequencyVariation { get; set; } = 0.0d;

    [XmlAttribute("HeelFrequency")]
    public double HeelFrequency { get; set; } = 0.0d;

    [XmlAttribute("HeelFrequencyVariation")]
    public double HeelFrequencyVariation { get; set; } = 0.0d;

    public int? batID { get; set; } = 0;

    public int? SegmentID { get; set; } = 0;

    public virtual Bat? Bat { get; set; } = null;

    public virtual LabelledSegment? Segment { get; set; } = null;

    /////////////////////////////////////////////////////////////////////////////
    /// Not Mapped
    /// 

    [NotMapped]
    public string FMax { get => $"{StartFrequency:F2} +/- {StartFrequencyVariation:F2}"; }

    [NotMapped]
    public string FPeak { get => $"{PeakFrequency:F2} +/- {PeakFrequencyVariation:F2}"; }

    [NotMapped]
    public string FLow { get => $"{EndFrequency:F2} +/- {EndFrequencyVariation:F2}"; }

    [NotMapped]
    public string Interval { get =>$"{PulseInterval:F2} +/- {PulseIntervalVariation:F2}"; }

    [NotMapped]
    public string Duration { get => $"{PulseDuration:F2} +/- {PulseDurationVariation:F2}"; }
}
