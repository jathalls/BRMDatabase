using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRMDatabase;


public class LabelItem
{
    public string idedBats { get; set; } = string.Empty;
    public double startOffset { get; set; } = 0.0;

    public double endOffset { get; set; } = 0.0;

    public LabelItem(string label, double startOffset, double endOffset)
    {
        idedBats = label;
        this.startOffset = startOffset;
        this.endOffset = endOffset;
        if (endOffset < startOffset)
        {
            this.startOffset = endOffset;
            this.endOffset = startOffset;
        }
    }

    /// <summary>
    /// returns true if there is an overlap between this time section and the 
    /// supplied start and end times
    /// </summary>
    /// <param name="startSecs"></param>
    /// <param name="endSecs"></param>
    /// <returns></returns>
    internal bool Overlaps(double startSecs, double endSecs)
    {
        if (endSecs < startSecs)
        {
            var tmp = startSecs;
            startSecs = endSecs;
            endSecs = tmp;
        }

        if (endSecs < startOffset) return false; // all of this is before the existing
        if(startSecs>endOffset) return false; // all after the existing
        return true;
        
    }

    internal bool Matches(double startSecs, double endSecs)
    {
        if (endSecs < startSecs)
        {
            var tmp = startSecs;
            startSecs = endSecs;
            endSecs = tmp;
        }

        if (startSecs==startOffset && endSecs == endOffset)
        {
            return true;
        }
        return false;
    }
}
