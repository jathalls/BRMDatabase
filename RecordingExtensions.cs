using System.ComponentModel;

namespace SqliteDbContextFactoryLib
{
    /// <summary>
    /// Extended RecordingSession with additional functionality that doesn't belong in the database
    /// </summary>
    public class RecordingSessionEx : RecordingSession
    {
        // Additional fields that don't need to be stored in DB
        public bool IsSelected { get; set; }
        public int TotalRecordingCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public string DisplayDate => SessionStart.ToString("yyyy-MM-dd");
        
        // Additional functionality
        public string GetSessionSummary()
        {
            return $"Session '{SessionTag}' with {Recordings?.Count ?? 0} recordings";
        }

        public bool HasRecordings()
        {
            return Recordings != null && Recordings.Count > 0;
        }

        public Recording? GetLatestRecording()
        {
            return Recordings?.OrderByDescending(r => r.RecordingDate).FirstOrDefault();
        }
    }

    /// <summary>
    /// Extended Recording with additional functionality that doesn't belong in the database
    /// </summary>
    public class RecordingEx : Recording
    {
        // Additional fields that don't need to be stored in DB
        public bool IsSelected { get; set; }
        public string FileSizeDisplay { get; set; } = string.Empty;
        public string DurationDisplay => FormatDuration((int)(RecordingEndTime-RecordingStartTime).TotalSeconds);
        public string DisplayDate => RecordingDate.ToString("yyyy-MM-dd HH:mm");

        public double Duration { get => (RecordingEndTime - RecordingStartTime).TotalSeconds; } 
        
        // Additional functionality
        public string GetRecordingDetails()
        {
            return $"Recording: {RecordingName} ({DurationDisplay})";
        }

        public bool IsRecent(int hours = 24)
        {
            return RecordingDate >= DateTime.UtcNow.AddHours(-hours);
        }

        private string FormatDuration(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.Hours > 0)
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }

    /// <summary>
    /// Extension methods for working with recording data
    /// </summary>
    public static class RecordingExtensions
    {
        /// <summary>
        /// Converts a Recording to RecordingEx with additional functionality
        /// </summary>
        public static RecordingEx ToExtendedRecording(this Recording recording)
        {
            return new RecordingEx
            {
                ID = recording.ID,
                RecordingName=recording.RecordingName,
                RecordingStartTime=recording.RecordingStartTime,
                RecordingEndTime=recording.RecordingEndTime,
                RecordingNotes=recording.RecordingNotes,
                SessionID = recording.SessionID,
                
                // Copy the navigation property if it exists
                Session = recording.Session,
                
                // Extended properties (not in DB)
                IsSelected = false,
                FileSizeDisplay = "Unknown",
            };
        }

        /// <summary>
        /// Converts a RecordingSession to RecordingSessionEx with additional functionality
        /// </summary>
        public static RecordingSessionEx ToExtendedSession(this RecordingSession session)
        {
            return new RecordingSessionEx
            {
                ID = session.ID,
                SessionTag= session.SessionTag,
                SessionNotes= session.SessionNotes,
                SessionStart= session.SessionStart,
                Recordings = session.Recordings,

                // Extended properties (not in DB)
                IsSelected = false,
                TotalRecordingCount = session.Recordings?.Count ?? 0,
                TotalDuration = TimeSpan.FromSeconds(session.Recordings?.Sum(r => (r?.RecordingEndTime-r?.RecordingStartTime)?.TotalSeconds)??0) ,
            };
        }

        /// <summary>
        /// Gets recordings grouped by date
        /// </summary>
        public static Dictionary<DateTime, List<RecordingEx>> GroupByDate(this IEnumerable<Recording> recordings)
        {
            return recordings
                .Select(r => r.ToExtendedRecording())
                .GroupBy(r => r.RecordingDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Calculates the total duration of all recordings in a session
        /// </summary>
        public static TimeSpan GetTotalDuration(this RecordingSession session)
        {
            return TimeSpan.FromSeconds(session.Recordings?.Sum(r => (r?.RecordingEndTime - r?.RecordingStartTime)?.TotalSeconds) ?? 0);
        }
    }
}
