using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BRMDatabase.Tables
{
    public static class RecordingExt
    {

        /// <summary>
        /// Backing stor for the Recortding class
        /// </summary>
        private static readonly ConditionalWeakTable<Recording, RecData> RecStore = new();

        /// <summary>
        /// Represents BAT-related record data.
        /// </summary>
        /// <remarks>Contains the BAT summary text used by the record.</remarks>
        class RecData
        {
            /// <summary>
            /// Stores the BAT summary text.
            /// </summary>
            public string _batSummaryString = "";

            public List<BatSummary> _batSummaryList = new List<BatSummary>();

        }

        /// <summary>
        /// Provides an extension context for <see cref="Recording"/>.
        /// </summary>
        /// <param name="recording">The recording instance the extension members apply to.</param>
        extension(Recording recording)
        {
            /// <summary>
            /// Gets or sets the recording start time as a time of day.
            /// </summary>
            /// <remarks>Setting the value updates <c>RecordingStartTime</c> by combining
            /// <c>RecordingDate.Date</c> with the specified time.</remarks>
            public TimeSpan RecordingStartTimeSpan
            {
                get => recording.RecordingStartTime.TimeOfDay;
                set => recording.RecordingStartTime = recording.RecordingDate.Date + value;
            }

            /// <summary>
            /// Gets or sets the recording end time as a time of day.
            /// </summary>
            /// <remarks>Setting the value combines the specified time with the recording date to
            /// update the recording end date and time.</remarks>
            public TimeSpan RecordingEndTimeSpan
            {
                get=>recording.RecordingEndTime.TimeOfDay;
                set=>recording.RecordingEndTime= recording.RecordingDate.Date + value;
            }

            /// <summary>
            /// Gets the duration of the recording.
            /// </summary>
            public TimeSpan RecordingDuration
            {
                get => recording.RecordingEndTimeSpan - recording.RecordingStartTimeSpan;
            }

            /// <summary>
            /// Gets the number of labeled segments in the recording.
            /// </summary>
            public int NumberOfSegments { get => recording.LabelledSegments?.Count ?? 0; }

            /// <summary>
            /// Gets the recording date as a short date string.
            /// </summary>
            /// <remarks>Formatting is culture-sensitive and uses the current culture's short date
            /// pattern.</remarks>
            public string RecordingDateOnly { get => recording.RecordingDate.ToShortDateString(); }

            public string BatSummaryString
            {
                get
                {
                    var summary = RecStore.GetOrCreateValue(recording)._batSummaryString;
                    if(recording.NumberOfSegments>0 && string.IsNullOrWhiteSpace(summary))
                    {
                        summary = recording.GetRecBatSummary(Force: true);
                        
                    }
                    return RecStore.GetOrCreateValue(recording)._batSummaryString;
                }

                set
                {
                    RecStore.GetOrCreateValue(recording)._batSummaryString = value;
                }
            }

            /// <summary>
            /// Gets or sets the collection of bat summaries for the current recording.
            /// </summary>
            /// <remarks>The collection is stored in the recording state store and is created on first
            /// access if it does not already exist.</remarks>
            public List<BatSummary> BatSummaryList
            {
                get
                {
                    return RecStore.GetOrCreateValue(recording)._batSummaryList;
                }
                set
                {
                    RecStore.GetOrCreateValue(recording)._batSummaryList = value;
                }
            }

            /// <summary>
            /// Bat details associated with the recording.
            /// </summary>
            public string RecordingBats
            {
                get => recording.GetBatDetailListAsString();
            }

            /// <summary>
            /// Gets the full path to the recording file by combining the session's original file path and the recording
            /// name.
            /// </summary>
            public string RecordingNameFull { get => Path.Combine(recording.Session.OriginalFilePath, recording.RecordingName); }

            /// <summary>
            /// Builds a newline-separated list of bat summary entries formatted as "{BatName} -
            /// {Passes}/{NumSegments}".
            /// </summary>
            /// <remarks>If the bat summary list is null or empty, recording summaries are loaded
            /// before formatting.</remarks>
            /// <returns>A string containing one formatted line per bat summary entry. Uses "No Name" for missing names and 0 for
            /// missing numeric values, and always ends with a newline.</returns>
            private string GetBatDetailListAsString()
            {
                string result = "";

                if (!(recording.BatSummaryList?.Any() ?? false))
                {
                    recording.GetRecBatSummary();
                }
                foreach (BatSummary bat in recording.BatSummaryList ?? new List<BatSummary>())
                {
                    result += $"{bat?.BatName??"No Name"} - {bat?.Passes??0}/{bat?.NumSegments??0}{Environment.NewLine}";
                }

                return result.Trim() + Environment.NewLine; ;
            }

            

            /// <summary>
            /// Gets a formatted batting summary for the current recording.
            /// </summary>
            /// <remarks>Returns the existing summary text when available; otherwise retrieves summary
            /// entries, converts each entry to its string representation, and combines them into a single trimmed
            /// string.</remarks>
            /// <returns>A task that represents the asynchronous operation. The task result contains the batting summary text,
            /// with individual summary entries separated by new lines and leading/trailing whitespace removed.</returns>
            public  string? GetRecBatSummary(bool Force=false)
            {
                string result = "";
                if (!Force)
                {
                    result = recording.BatSummaryString;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        return result.Trim();
                    }
                }
                
                recording.BatSummaryList = recording.GetRecBatSummaries();

                //result = "";
                foreach (var summary in recording.BatSummaryList)
                {
                    result += summary.ToString() + "\n";
                }
                // Replaced Debug.WriteLine($"\t\tsummary={result}");
                recording.BatSummaryString = result.Trim();
                return recording.BatSummaryString;

            }

            /// <summary>
            /// Aggregates bat summaries from all labeled segments, condenses the combined results, and updates the
            /// recording summary list.
            /// </summary>
            /// <remarks>Returns immediately when no labeled segments are available. Safely skips null
            /// segment summary lists during aggregation.</remarks>
            private async void GetSummariesAsync()
            {
                List<BatSummary> batSummaryList = new List<BatSummary>();
                if (recording.LabelledSegments.Count <= 0)
                {
                    return;
                }
                foreach (var seg in recording.LabelledSegments)
                {
                    batSummaryList.AddRange(seg?.BatSummaryList ?? new List<BatSummary>());
                }
                batSummaryList = Recording.CondenseBatSummaries(batSummaryList);
                recording.BatSummaryList = batSummaryList;

            }

            /// <summary>
            /// Builds and stores the recording-level bat summaries from all labeled segments, optionally reusing
            /// existing data.
            /// </summary>
            /// <remarks>If labeled segments are not loaded, they are refreshed before aggregation.
            /// The operation runs asynchronously as fire-and-forget because the signature is `async void`.</remarks>
            /// <param name="force">Rebuilds summaries even when a summary list is already available; otherwise, returns without changes
            /// when summaries exist.</param>
            public  List<BatSummary> GetRecBatSummaries(bool force = true)
            {
                if (recording.BatSummaryList?.Any() ?? false)
                {
                    if (!force)
                        return recording.BatSummaryList;
                }
                List<BatSummary> batSummaryList = new List<BatSummary>();
                if ((recording.LabelledSegments?.Count??0) <= 0)
                {
                    recording.RefreshLabelledSegments();
                }
                foreach (var seg in recording.LabelledSegments)
                {
                    batSummaryList.AddRange(seg?.BatSummaryList ?? new List<BatSummary>());
                }
                recording.BatSummaryList = Recording.CondenseBatSummaries(batSummaryList);
                return recording.BatSummaryList;
            }

            /// <summary>
            /// Refreshes the recording’s labeled segments from storage, updates segment bat summaries, and aggregates
            /// those summaries into the recording.
            /// </summary>
            /// <remarks>This operation updates internal recording state, including `LabelledSegments`
            /// and `BatSummaryList`.</remarks>
            /// <returns>A new list containing the refreshed labeled segments for the recording.</returns>
            public  List<LabelledSegment> RefreshLabelledSegments()
            {
                var ls =  DBAccessMS.GetSegmentsForRecording(recording.ID);// includes IdedBats
                recording.LabelledSegments = ls;
                for (int i = 0; i < ls.Count; i++)
                {

                    ls[i].GetSegBatSummary();
                    
                    recording.BatSummaryList.AddRange(ls[i].BatSummaryList);
                }
                recording.LabelledSegments = ls ?? new List<LabelledSegment>();

                return recording.LabelledSegments.ToList();
            }

            /// <summary>
            /// Combines bat summaries into one summary per bat name by aggregating non-auto-identified segments.
            /// </summary>
            /// <remarks>Distinct bat names are derived from all input entries. Bat names that appear
            /// only in auto-identified entries are returned with zero-valued aggregates.</remarks>
            /// <param name="batSummaryList">The bat summaries to condense. Only entries where ByAutoId is false are included in aggregate totals.</param>
            /// <returns>A list containing one BatSummary per distinct bat name, with total duration, segment count, and passes
            /// calculated from non-auto-identified entries.</returns>
            public static List<BatSummary> CondenseBatSummaries(List<BatSummary> batSummaryList)
            {
                var result = new List<BatSummary>();
                var uniqueBats = batSummaryList.Select(bs => bs.BatName).Distinct();
                foreach (var bat in uniqueBats)
                {
                    var summary = new BatSummary();
                    summary.BatName = bat;
                    summary.BatDuration = TimeSpan.FromMilliseconds(batSummaryList.Where(bs => bs.BatName == bat && !bs.ByAutoId).Select(bs => bs.BatDuration.TotalMilliseconds).Sum());
                    summary.NumSegments = batSummaryList.Where(bs => bs.BatName == bat && !bs.ByAutoId).Count();
                    summary.Passes = batSummaryList.Where(bs => bs.BatName == bat && !bs.ByAutoId).Select(b => b.Passes).Sum();
                    summary.ByAutoId = false;
                    result.Add(summary);
                }
                return result;
            }

            /// <summary>
            /// Given a filename with path finds all the labelled segments for this recording and adds
            /// them to the LabelledSegments List.  Initially looks for .txt sidecar file for the current
            /// sound file.  Does NOT look at metadata for IDs, those are dealt with elsewhere
            /// </summary>
            /// <param name="wavFile"></param>
            /// <exception cref="NotImplementedException"></exception>
            internal async Task UpdateLabelledSegmentsAsync(FileInfo wavFile, DateTime fileStart, DateTime fileEnd, string note)
            {
                //// Replaced Debug.WriteLine("UpdateLabelledSegmentAsync");
                string path = Path.GetDirectoryName(wavFile.FullName) ?? "";
                string file = wavFile.Name;
                string fqTextFile = Path.Combine(path ?? "", Path.ChangeExtension(file, ".txt"));

                if (File.Exists(fqTextFile))
                {
                    await recording.CreateLabelledSegmentsAsync(fqTextFile);
                    //// Replaced Debug.WriteLine($"\tCreated {this.LabelledSegments.Count} segments from {fqTextFile}");

                }
                else
                {
                    // assume one labelled segment per file
                    //var segment = new LabelledSegment();
                    //segment.StartOffset = new DateTime();
                    //segment.EndOffset = segment.StartOffset + (fileEnd - fileStart);
                    //segment.Comment = note;
                    var segment = await Recording.ProcessLabelledSegmentAsync(note);
                    if (segment != null)
                    {
                        await segment.GetSegBatSummaryAsync();
                        recording.LabelledSegments.Add(segment);
                    }
                    //// Replaced Debug.WriteLine($"\tnew labelled segment from {segment.Comment}");
                }
            }

            /// <summary>
            ///     Processes the labelled segment. Accepts a processed segment comment line consisting
            ///     of a start offset, end offset, duration and comment string and generates a new
            ///     Labelled segment instance and BatSegmentLink instances for each bat represented in
            ///     the Labelled segment. The instances are merged into a single instance of
            ///     CombinedSegmentAndBatPasses to be returned. If the line to be processed is not in the
            ///     correct format then an instance containing an empty LabelledSegment instance and an
            ///     empty List of ExtendedBatPasses. The comment section is checked for the presence of a
            ///     call parameter string and if present new Call is created and populated.
            /// </summary>
            /// <param name="processedLine">
            ///     The processed line.
            /// </param>
            /// <param name="bats">
            ///     The bats.
            /// </param>
            /// <returns>
            /// </returns>
            /// <exception cref="System.NotImplementedException">
            /// </exception>
            public static async Task<LabelledSegment?> ProcessLabelledSegmentAsync(string processedLine)
            {
                //// Replaced Debug.WriteLine("ProcessLabelledSegmentAsync");
                LabelledSegment? segment = null;
                DateTime start = new DateTime();
                DateTime end = new DateTime();
                string comment = "";
                try
                {
                    var parts = processedLine.Split(new char[] { ' ', '\t' }, 3);
                    if (parts.Length > 0)
                    {
                        if (float.TryParse(parts[0], out float value))
                        {
                            start += TimeSpan.FromSeconds(value > 0.0f ? value : 0.0f);
                        }
                        else
                        {
                            if (TimeSpan.TryParse(parts[0], out TimeSpan tsValue))
                            {
                                start += tsValue;
                            }
                        }
                    }
                    if (parts.Length > 1)
                    {
                        if (float.TryParse(parts[1], out float value))
                        {
                            end += TimeSpan.FromSeconds(value > 0.0f ? value : 0.0f);
                        }
                        else
                        {
                            if (TimeSpan.TryParse(parts[1], out TimeSpan tsValue))
                            {
                                end += tsValue;
                            }
                        }
                    }
                    for (int i = 2; i < parts.Length; i++)
                    {
                        comment += parts[i];
                    }
                    segment = new LabelledSegment();
                    segment.StartOffset = start;
                    segment.EndOffset = end;
                    segment.Comment = comment;
                    // Replaced Debug.WriteLine($"ProcessLabelledSegment {comment}");
                    var idedBatList = await segment.GetDescribedBatsAsync();
                    segment.IdedBats = idedBatList.batList;
                    segment.Comment = idedBatList.moddedDescription;
                    


                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERR in ProcessLabelledSegmentAsync {ex.Message}");
                }

                return segment;
            }

            /// <summary>
            /// Goes through a sidecar text file creating a labelled segment for each line and adding the
            /// LabelledSegment to the list.
            /// </summary>
            /// <param name="fqTextFile"></param>
            /// <exception cref="NotImplementedException"></exception>
            private async Task CreateLabelledSegmentsAsync(string fqTextFile)
            {
                //// Replaced Debug.WriteLine("CreateLabelledSegmentsAsync");
                var lines = File.ReadAllLines(fqTextFile);
                for (int i = 0; i < lines.Count(); i++)
                {
                    string line = lines[i];
                    if (line.StartsWith(@"\")) continue;
                    if (i < lines.Count() - 1 && lines[i + 1].StartsWith(@"\"))
                    {
                        line = line + lines[i + 1].Substring(1);
                    }
                    var modline = Regex.Replace(line, @"[Ss][Tt][Aa][Rr][Tt]", "0.0");
                    modline = Regex.Replace(modline, @"[Ee][Nn][Dd]", ((decimal)recording.RecordingDuration.TotalSeconds).ToString());

                    var segment = await ProcessLabelledSegmentAsync(modline);
                    if (segment != null)
                    {
                        await segment.GetSegBatSummaryAsync();
                        recording.LabelledSegments.Add(segment);
                        //// Replaced Debug.WriteLine($"Added segment {segment.Comment} with {segment.IdedBats.Count} ids");
                    }
                }

            }

            /// <summary>
            /// Parses a modulation line, resolves bat tags against stored bats, and produces a normalized line with
            /// matched tags replaced by bat names.
            /// </summary>
            /// <remarks>Performs an asynchronous bat lookup and evaluates tags ordered by tag length
            /// before matching.</remarks>
            /// <param name="modline">Modulation text to parse, including optional auto-identification tokens in parentheses and optional
            /// trailing notes in braces.</param>
            /// <param name="segId">Segment identifier to associate with each identified bat.</param>
            /// <returns>A tuple containing the identified bats and the rebuilt modulation line. The first item is the list of
            /// identified bats with segment assignment and identification source, and the second item is the updated
            /// modulation line with original parenthesized and braced parts appended.</returns>
            internal async Task<(List<IdedBat>, string newmodline)> GetIdedBats(string modline, int segId)
            {
                string newmodline = "";
                string autoIds = "";
                string fullAutoIds = "";
                string braces = "";
                List<IdedBat> result = new List<IdedBat>();
                int lbPos = modline.IndexOf('{');
                if (lbPos >= 0)
                { // remove subcomment in curly braces
                    braces = modline.Substring(lbPos);
                    modline = modline.Substring(0, lbPos);
                }
                lbPos = modline.IndexOf('(');
                if (lbPos >= 0)
                { // remove autoId in brackets
                    autoIds = modline.Substring(lbPos);
                    fullAutoIds = autoIds;
                    modline = modline.Substring(0, lbPos);
                }
                newmodline = modline;
                var bats = await DBAccessMS.GetAllBatsAsync();
                Dictionary<Bat, string> batTags = new Dictionary<Bat, string>();
                foreach (var bat in bats)
                {
                    foreach (var tag in bat.Tags ?? new List<BatTag>())
                    {
                        batTags.Add(bat, tag.Tag);
                    }
                }
                batTags = (from bt in batTags
                           orderby bt.Value.Length
                           select bt).ToDictionary();
                foreach (var tag in batTags)
                {
                    if (modline.Contains(tag.Value))
                    {
                        IdedBat idedBat = new IdedBat();
                        idedBat.Name = tag.Key.Name;
                        idedBat.ByAutoId = false;
                        idedBat.SegmentID = segId;
                        modline = modline.Replace(tag.Value, "");
                        newmodline = newmodline.Replace(tag.Value, tag.Key.Name);
                    }
                    if (autoIds.Contains(tag.Value))
                    {
                        IdedBat idedBat = new IdedBat();
                        idedBat.Name = tag.Key.Name;
                        idedBat.ByAutoId = true;
                        idedBat.SegmentID = segId;
                        autoIds = autoIds.Replace(tag.Value, "");
                    }
                }
                result = result.Distinct().ToList();
                newmodline += fullAutoIds + braces;




                return ((result, newmodline));
            }

            /// <summary>
            /// Gets the formatted text representation for display.
            /// </summary>
            public String DisplayText
            {
                get { return recording.CreateDisplayText(); }
            }

            /// <summary>
            /// Gets the display text color based on whether the recording file exists.
            /// </summary>
            /// <remarks>Writes a debug message when the recording file is not found.</remarks>
            public Color DisplayTextColor
            {
                get
                {
                    if (File.Exists(recording.RecordingNameFull ?? ""))
                    {
                        return Colors.Black;
                    }
                    else
                    {
                        Debug.WriteLine($"File not Found:- {recording.RecordingNameFull}");
                        return Colors.Red;
                    }
                }
            }

            /// <summary>
            /// A displayable form of the recording notes fo use in the header of the recordingsMDPage
            /// </summary>
            public object RecordingNotesForDisplay
            {
                get
                {
                    string notes = "";
                    notes += recording.RecordingName + Environment.NewLine;
                    notes += $"{recording.RecordingStartTime.ToShortTimeString()} for {recording.RecordingDuration.TotalSeconds:0.#}secs" + Environment.NewLine;
                    notes += recording.RecordingNotes;
                    return notes.Trim();
                }
            }

            /// <summary>
            /// Creates a multi-line display string containing the recording name, start date and time, duration in
            /// seconds, and each metadata label/value pair.
            /// </summary>
            /// <remarks>The output uses line breaks between entries and includes a trailing line
            /// break.</remarks>
            /// <returns>A formatted string with recording details on the first line followed by one line per metadata entry.</returns>
            public string CreateDisplayText()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{recording.RecordingName} {recording.RecordingDate.Date + recording.RecordingStartTime.TimeOfDay} For {recording.RecordingDuration.TotalSeconds}s");
                foreach (var meta in recording.Metas)
                {
                    sb.AppendLine($"{meta.Label}:- {meta.Value}");
                }
                return sb.ToString();
            }

            

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public async Task<string> GetFQFileName()
            {

                return recording.RecordingNameFull;
            }

            /// <summary>
            /// Returns a list, one item for each labelled segment, the items contain a string
            /// identifying all Ided bats in the segment and the start and end offsets in the
            /// segment
            /// </summary>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            internal async Task<List<LabelItem>> GetLabelList()
            {
                List<LabelItem> list = new List<LabelItem>();
                
                foreach (var segment in recording.LabelledSegments)
                {
                    var label = await segment.GetIdedBatsString();
                    double start = segment.StartOffsetTimeSpan.TotalSeconds;
                    double end = segment.EndOffsetTimeSpan.TotalSeconds;
                    LabelItem item = new LabelItem(label, start, end);
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        list.Add(item);
                    }
                }
                return list;

            }


        }
    }
}
