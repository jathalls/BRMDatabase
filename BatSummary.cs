namespace BRMDatabase;

public class BatSummary
{
    public BatSummary() { }

    public BatSummary(LabelledSegment segment) { }

    public string BatName { get; set; } = "";

    public TimeSpan BatDuration { get; set; }=new TimeSpan();

    public bool ByAutoId { get; set; } = false;

    public int NumSegments { get; set; } = 0;

    public int Passes { get; set; }

    public double TotalDuration { get => BatDuration.TotalSeconds; }

    public double AvgDuration { get => TotalDuration / NumSegments; }

    public override String ToString()
    {
        string result;
         result=$"({BatName} - {Passes} passes in {NumSegments} Total:- {BatDuration.TotalSeconds:0.00}s)";
        
        return result ;
    }
}
