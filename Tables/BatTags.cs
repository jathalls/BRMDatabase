namespace BRMDatabase.Tables;

public class BatTag
  {
     
      public int ID { get; set; }

   
      public string Tag {  get; set; }

 
      public int BatID { get; set; }

    public Bat? Bat { get; set; } = null;
  }
  