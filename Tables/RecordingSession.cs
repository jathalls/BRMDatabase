using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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

    public int NumberOfRecordings { get=>Recordings?.Count ?? 0;}

    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// NotMapped
    /// </summary>

    [NotMapped]
    private string _batNames = "";

    [NotMapped]
    public string BatNames
    {
        get
        {
            var res = _batNames; ;
            if (string.IsNullOrWhiteSpace(res))
            {
                return GetBatNames();

            }
            else
            {
                return res;
            }
        }
        set => _batNames= value;
    }

    //public List<LabelledSegment> Segments { get => Recordings.SelectMany(r => r.LabelledSegments).ToList(); }

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Functions
    /// 



    /// </summary>
    /// <returns></returns>

    public string GetBatNames()
    {
        string result = "";

        var uniqueBats = GetIdedBats();
        foreach (var bat in uniqueBats)
        {
            result += bat.Name + Environment.NewLine;
        }

        BatNames = result;
        return result;
    }

    public  List<IdedBat> GetIdedBats ()
    {
        return Recordings.SelectMany(r => r.IdedBats).DistinctBy(b=>b.Name).ToList(); 
    }



}
