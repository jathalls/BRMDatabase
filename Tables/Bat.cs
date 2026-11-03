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
            { type += $"{call.CallType};"; }
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
                if (!string.IsNullOrEmpty(flow)) flow += " / ";
                flow += $"{call.EndFrequency}+/-{call.EndFrequencyVariation:F2}";
            }
            return flow;
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////
    /// Functions
    /// 



}