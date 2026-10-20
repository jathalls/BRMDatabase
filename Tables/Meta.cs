namespace BRMDatabase.Tables;

public class Meta
{
    
    public int ID { get; set; }

    [XmlAttribute("Label")]
    public string Label { get; set; } = string.Empty;

    [XmlAttribute("Value")]
    public string Value { get; set; }

    [XmlAttribute("Type")]
    public string Type {  get; set; }

    
    public int RecordingID { get; set; }

    public Recording? Recording { get; set; } = null;
}
