namespace BRMDatabase.Tables;

 //[Table("BatTable")]
 public class Bat
 {

     public int ID { get; set; }
     public string BatGenus { get; set; }
     public string BatSpecies { get; set; }

 
     public string Name { get; set; } = string.Empty;

     public string Label { get; set; } = string.Empty;
     public string Notes { get; set; }

    public virtual ICollection<BatTag> Tags { get; set; } = new List<BatTag>();

    public virtual ICollection<Call> BatCalls { get; set; } = new List<Call>();
 }