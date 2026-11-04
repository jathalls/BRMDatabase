using ScottPlot;
using System.ComponentModel.DataAnnotations.Schema;

namespace BRMDatabase.Tables;

 //[Table("BatTable")]
 public class Bat
 {

     public int ID { get; set; }
     public string BatGenus { get; set; }
     public string BatSpecies { get; set; }

 
     public string Name { get; set; } = string.Empty;

     public string Label { get; set; } = string.Empty;
    public string Notes { get; set; } = "";

    public virtual ICollection<BatTag> Tags { get; set; } = new List<BatTag>();

    public virtual ICollection<Call> BatCalls { get; set; } = new List<Call>();

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// NotMapped
    /// 
    [NotMapped]
    private BatTag _tag = new BatTag();

    [NotMapped]
    public BatTag Tag { get => _tag; set => _tag = value; }

    [NotMapped]
    public string CallType
    {
        get
        {
            string type = "";
            foreach (var call in BatCalls ?? new List<Call>())
            {
                if (!string.IsNullOrWhiteSpace(type)) type += Environment.NewLine;
                type += $"{call.CallType};"; 
            }
            return type;

        }
    }

    [NotMapped]
    public string FLow
    {
        get
        {
            string flow = "";
            foreach(Call call in BatCalls?.ToList()??new List<Call>())
            {
                if (!string.IsNullOrEmpty(flow)) flow += Environment.NewLine;
                flow += $"{call.EndFrequency:F2}+/-{call.EndFrequencyVariation:F2}";
            }
            return flow;
        }
    }

    public string FHigh
    {
        get
        {
            string fhigh = "";
            foreach (Call call in BatCalls?.ToList() ?? new List<Call>())
            {
                if (!string.IsNullOrEmpty(fhigh)) fhigh += Environment.NewLine;
                fhigh += $"{call.StartFrequency:F2}+/-{call.StartFrequencyVariation:F2}";
            }
            return fhigh;
        }
    }

    public string FPeak
    {
        get
        {
            string fpk = "";
            foreach (Call call in BatCalls?.ToList() ?? new List<Call>())
            {
                if (!string.IsNullOrEmpty(fpk)) fpk += Environment.NewLine;
                fpk += $"{call.PeakFrequency:F2}+/-{call.PeakFrequencyVariation:F2}";
            }
            return fpk;
        }
    }

    public string Interval
    {
        get
        {
            string ival = "";
            foreach (Call call in BatCalls?.ToList() ?? new List<Call>())
            {
                if (!string.IsNullOrEmpty(ival)) ival += Environment.NewLine;
                ival += $"{call.PulseInterval:F2}+/-{call.PulseIntervalVariation:F2}";
            }
            return ival;
        }
    }

    public string Duration
    {
        get
        {
            string dur = "";
            foreach (Call call in BatCalls?.ToList() ?? new List<Call>())
            {
                if (!string.IsNullOrEmpty(dur)) dur += Environment.NewLine;
                dur += $"{call.PulseDuration:F2}+/-{call.PulseDurationVariation:F2}";
            }
            return dur;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /// Functions
    /// 



}