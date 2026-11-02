using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
namespace BRMDatabase.Tables;


 public class IdedBat
 {
 
     public int ID { get; set; }


     [XmlAttribute("BatName")]
     public string Name { get; set; } = "";


    
     public bool ByAutoId { get; set; } = false;


     public int SegmentID { get; set; } = -1;

   
    //public int RecordingID { get; set; } = -1;

   
    //public int SessionID { get; set;  } = -1;

    public virtual  LabelledSegment? Segment { get; set; } = null;

    //public virtual  Recording? Recording { get; set;  } = null;

    //public virtual RecordingSession? Session { get; set; } = null;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// NotMapped
    /// 

    [NotMapped]
    /// <summary>
    /// Gets or sets the `ByAutoId` XML attribute as a string flag for automatic ID matching.
    /// </summary>
    /// <remarks>Setting the value to `"TRUE"` (case-insensitive) stores `true`; any other
    /// value stores `false`.</remarks>
    [XmlAttribute("ByAutoId")]
    public string ByAutoIdString
    {
        get { return ByAutoId.ToString(); }
        set
        {
            if (value.ToUpper() == "TRUE") { ByAutoId = true; }
            else { ByAutoId = false; }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Functions
    /// 

    /// <summary>
    /// Asynchronously gets a four-letter identification code for the current bat from existing tags, or builds
    /// one from genus and species when no valid tag is available.
    /// </summary>
    /// <remarks>Uses the first tag that is exactly four characters and already uppercase. If
    /// none is found, the code is generated from the first three characters of genus and species, then
    /// converted to uppercase.</remarks>
    /// <returns>A four-letter uppercase code, or the same code wrapped in parentheses when automatic identification is
    /// enabled.</returns>
    internal async Task<string?> Get4LetterCode()
    {
        string result = "";
        //if (idedBat.ByAutoId) { return ""; }

        var bat = DBAccessMS.GetNamedBat(Name ?? "");
        var tagList = DBAccessMS.GetBatTags(bat.ID);
        string? code = (from tag in tagList ?? new List<BatTag>()
                        where (tag?.Tag?.Length ?? 0) == 4 && (tag?.Tag?.ToUpper() ?? "") == (tag?.Tag ?? " ")
                        select (tag?.Tag ?? "")).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(code))
        {
            code = (bat.BatGenus.Substring(0, 3) + bat.BatSpecies.Substring(0, 3)).ToUpper();

        }
        if (ByAutoId) code = $"({code})";
        return code;

    }

    public string GetLabel()
    {
        
        if (ByAutoId) { return ""; }

        var bat = DBAccessMS.GetNamedBat(Name);

        return bat?.Label??"";
    }

}
