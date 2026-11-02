using System.ComponentModel.DataAnnotations.Schema;

namespace BRMDatabase.Tables;

 public class LabelledSegment
 {
     

     public int ID { get; set; }

  
     


     public DateTime StartOffset { get; set; }

     public DateTime EndOffset { get; set; } 

     [XmlAttribute("Comment")]
     public string Comment { get; set; } = "";

     [XmlAttribute("AutoID")]
     public string AutoID { get; set; } = string.Empty;

     [XmlAttribute("AutoIdProb")]
     public double AutoIdProb { get; set; } = 0.0d;

    //public int SessionID { get; set; } = 0;

    //public virtual RecordingSession? Session { get; set; } = null;

    public int RecordingID { get; set; } = 0;

    public virtual Recording? Recording { get; set; } = null;

    public virtual ICollection<IdedBat> IdedBats { get; set; } = new List<IdedBat>();

    public virtual ICollection<Call> Calls { get; set; } = new List<Call>();

    //////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Not Mapped
    /// 

    [NotMapped]
    private List<BatSummary> _batSummaryList = new List<BatSummary>();

    [NotMapped]
    /// <summary>
    /// Gets or sets the bat summaries for the current  
    /// </summary>
    public List<BatSummary> BatSummaryList
    {
        get
        {
            if (!_batSummaryList.Any()) GetSegBatSummary();
            return  _batSummaryList;
        }

        set {  _batSummaryList = value; }
    }

    [NotMapped]
    /// <summary>
    /// Gets a display string containing the segment start time, end time, duration, and comment.
    /// </summary>
    public string SegmentTextForDisplay
    {
        get
        {
            string text =
                $"{ StartOffsetTimeSpan.TotalSeconds:0.00}s - { EndOffsetTimeSpan.TotalSeconds:0.00}s = {( EndOffsetTimeSpan -  StartOffsetTimeSpan).TotalSeconds:0.00}s; { Comment}";
            return text;
        }
    }

    [NotMapped]
    /// <summary>
    /// Gets or sets the end offset as a <see cref="System.TimeSpan"/> value.
    /// </summary>
    /// <remarks>Represents only the time-of-day portion of the underlying end offset. Setting
    /// this value updates the underlying offset using a zero-date base.</remarks>
    [XmlAttribute("EndOffset")]
    public TimeSpan EndOffsetTimeSpan
    {
        get { return  EndOffset.TimeOfDay; }
        set {  EndOffset = new DateTime() + value; }
    }

    [NotMapped]
    /// <summary>
    /// Gets or sets the segment start offset as a time-of-day value.
    /// </summary>
    /// <remarks>When set, the value is applied as an offset from midnight and the date
    /// portion is not preserved.</remarks>
    [XmlAttribute("StartOffset")]
    public TimeSpan StartOffsetTimeSpan
    {
        get { return  StartOffset.TimeOfDay; }
        set {  StartOffset = new DateTime() + value; }
    }




    //////////////////////////////////////////////////////////////////////////////
    /// Functions
    /// 




    

    /// <summary>
    /// Gets bat summaries for the current segment, using cached summaries unless refresh is forced.
    /// </summary>
    /// <remarks>If identified bats are not already loaded, they are retrieved asynchronously
    /// before summaries are created.</remarks>
    /// <param name="force">true to ignore cached summaries and rebuild the summary list; otherwise, returns cached summaries when
    /// available.</param>
    /// <returns>A task that represents the asynchronous operation. The task summary contains a list of bat summaries for
    /// identified bats in the segment, or an empty list when no identified bats are available.</returns>
    public List<BatSummary> GetSegBatSummary(bool force = false)
    {
        if ( _batSummaryList?.Any() ?? false)
        {
            if (!force)
            {
                return  BatSummaryList;
            }
        }

        //var summary=DBAccess.GetBatSummaryForSegment(ID).Result;
        if (! IdedBats?.Any() ?? false)
        {
             IdedBats = DBAccessMS.GetIdedBatsForSegment( ID);
            if (! IdedBats?.Any() ?? false)
            {
                 BatSummaryList = new List<BatSummary>();
                return new List<BatSummary>();
            }
        }

        // if we get here there are bats in the IdedBats list
        var list = new List<BatSummary>();
        foreach (var bat in  IdedBats)
        {
            BatSummary batSummary = new BatSummary();
            batSummary.BatDuration =  EndOffsetTimeSpan -  StartOffsetTimeSpan;
            batSummary.BatName = bat.Name;
            batSummary.NumSegments = 1;
            batSummary.Passes = LabelledSegment.ToPasses(batSummary.BatDuration);
            batSummary.ByAutoId = bat.ByAutoId;
            list.Add(batSummary);
        }
        BatSummaryList = list;
        return list;

    }

    /// <summary>
    /// Analyses the comment for any bat names and sets the IdedBats list in this segment
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>

    public async Task<(List<IdedBat> batList, string moddedDescription)> GetDescribedBatsAsync(bool force = true)
    {
        if ( IdedBats?.Any() ?? false)
        {
            if (!force)
            {
                return ( IdedBats.ToList(),  Comment);
            }
        }

        //// Replaced Debug.WriteLine("GetDescribedBatsAsync");
        var matchingBats = new List<IdedBat>();
        var bracketed = "";
        var result = new List<IdedBat>();
        var moddedDescription =  Comment;
        //// Replaced Debug.WriteLine($"\tget bats from <{moddedDescription}>");
        if (string.IsNullOrWhiteSpace( Comment))
        {
            var nobat = new IdedBat();
            nobat.ByAutoId = false;
            matchingBats.Add(nobat);

             IdedBats = matchingBats;
            return ( IdedBats.ToList(), moddedDescription);
        }



        string description =  Comment;
        string autoDescription = "";

        var len =  Comment.IndexOf('{');
        if (len >= 0)
        {
            bracketed =  Comment.Substring(len).Trim();
            description = description.Substring(0, len);
        } // we have removed all text from { to the end to eliminate bracketed text

        var autoIDstart = description.IndexOf("(Auto");
        var autoEnd = description.IndexOf(")");
        if (autoIDstart >= 0 && autoEnd >= 0)
        {
            var autoLength = autoEnd - autoIDstart;
            autoDescription = description.Substring(autoIDstart, autoLength);
            description = description.Substring(0, autoIDstart);
        } // we have separated the main description and the first AuotID (bracketed) section

        var manualIds = await  GetIdedBatsAsync(description, AutoID: false);
        result.AddRange(manualIds.batList);
        var autoIds = await  GetIdedBatsAsync(autoDescription, AutoID: true);
        result.AddRange(autoIds.batList);

        moddedDescription = manualIds.moddedDescription + autoDescription + bracketed;
        return (result, moddedDescription);

    }

    /// <summary>
    /// Locates the described bats in the string by tag or by name or by latin name (genus+species)
    /// </summary>
    /// <param name="description"></param>
    /// <param name="AutoID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task<(IEnumerable<IdedBat> batList, string moddedDescription)> GetIdedBatsAsync(string description,
        bool AutoID)
    {
        //// Replaced Debug.WriteLine("GetIdedBatsAsync");
        string moddedDescription = description;
        List<IdedBat> result = new List<IdedBat>();
        List<BatTag> containedTags = new List<BatTag>();
        containedTags =
            DBAccessMS.GetContainedTags(description); // returns contained tags sorted longest to shortest
        string udescription = description.ToUpper();
        foreach (var tag in containedTags)
        {
            string desc = description;
            if ( IsUpperCase(tag.Tag))
            {
                desc = udescription;
            }

            var loc = desc.IndexOf(tag.Tag, 0, desc.Length, StringComparison.OrdinalIgnoreCase);
            if (loc >= 0)
            {
                //// Replaced Debug.WriteLine($"found {tag.Tag} in {desc}");
                var bat = DBAccessMS.GetBatByID(tag.BatID);
                var iBat = new IdedBat();
                iBat.Name = (bat?.Name) ?? "";
                iBat.ByAutoId = AutoID;
                result.Add(iBat);
                moddedDescription = moddedDescription.Replace(tag.Tag, iBat.Name);
                if (description.Length > tag.Tag.Length)
                {
                    //// Replaced Debug.WriteLine($"d=<{description}> t=<{tag.Tag}> loc={loc} tagLength={tag.Tag.Length}" );
                    description = description.Substring(0, loc) + description.Substring(loc + tag.Tag.Length - 1);
                }
                else
                {
                    description = "";
                }

                udescription = description.ToUpper();
                //udescription = description.Substring(0, loc) + description.Substring(loc + tag.Tag.Length-1);
                //// Replaced Debug.WriteLine($"\tfound bat {iBat.Name}");

                continue;
            }

        }

        return (result, moddedDescription);
    }

    /// <summary>
    /// Determines whether a string is equal to its uppercase form.
    /// </summary>
    /// <remarks>Calling this method with `null` throws a `NullReferenceException`.</remarks>
    /// <param name="s">The string to evaluate.</param>
    /// <returns>true if `s` is equal to `s` converted to uppercase using the current culture; otherwise, false.</returns>
    private bool IsUpperCase(string s)
    {
        if (s == s.ToUpper()) return true;
        return false;
    }

    /// <summary>
    /// Returns the fully qualified file name for the recording containing this segment
    /// does not check for the existence of the file or the path
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> GetFile()
    {
        string result = "";

        result =  Recording.RecordingNameFull;


        return result;
    }

    /// <summary>
    /// Saves this segment of the specified recording to a new file created for the purpose, and returns the path and name of the new file
    /// </summary>
    /// <param name="recording"></param>
    /// <returns>the fully qualified name of the new file</returns>
    public async Task<string> Save(Recording recording)
    {
        if (recording == null || recording.ID !=  RecordingID)
        {
            // Replaced Debug.WriteLine($"Labelled Save: getting recording with ID {RecordingID}");
            recording =  Recording;
        }

        if (recording == null)
        {
            // Replaced Debug.WriteLine($"Labelled Save: no recording with ID {RecordingID}");
            return "";
        }

        string folder = "";
        try
        {
            // Replaced Debug.WriteLine($"Labelled Save: getting file for recording ID {RecordingID}");
            folder = recording.RecordingNameFull;
            if (!File.Exists(folder))
            {
                // Replaced Debug.WriteLine($"Labelled Save: no file {folder}");
                return "";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
               $"Labelled Save: exception getting file for recording ID { RecordingID}: {ex.Message}");
            return "";
        }

        // Replaced Debug.WriteLine($"Labelled Save: copying segment {ID} from file {folder}");
        var destination = await  CopyFile(folder);
        return destination ?? "";

    }

    /// <summary>
    /// given a fully qualified .wav file name, creates a destination directory and copies the current segment
    /// to a new file in that directory.  Returns the fully qualified file name of the new file
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> CopyFile(string? folder)
    {
        string newFQFile = "";
        string? path = Path.GetDirectoryName(folder ?? "") ?? "";
        string? fileName = Path.GetFileName(folder);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length < 5)
        {
            // Replaced Debug.WriteLine($"Labelled CopyFile: invalid file name {fileName}");
            return "";
        }

        if (string.IsNullOrWhiteSpace(path)) return "";

#if MACCATALYST
//TODO reinstall MauiLib reference to SecurityScopedBookmarks
        Foundation.NSUrl? url = null;
        // A bookmark for an ancestor grants access to this directory and its contents.
        //url = MauiLib1.SecurityScopedBookmarks.TryRestoreNearestAncestorFolderBookmark(path);
        if (url == null)
        {
            return "";
        }

        bool accessGranted = url.StartAccessingSecurityScopedResource();
        if (!accessGranted)
        {
            return "";
        }
#endif

        try
        {
            string newFolder = Path.Combine(path, fileName.Substring(0, fileName.Length - 4));
            string baseFolder = newFolder;

            // Replaced Debug.WriteLine($"[Labelled CopyFile] Creating directory: {newFolder}");
            if (!Directory.Exists(newFolder))
            {
                Directory.CreateDirectory(newFolder);
            }

            string newFileName =
                $"{fileName.Substring(0, fileName.Length - 4)}_{(int) StartOffsetTimeSpan.TotalSeconds}.wav";
            newFQFile = Path.Combine(newFolder, newFileName);
            // Replaced Debug.WriteLine($"Labelled CopyFile: creating file {newFQFile}");
            if (File.Exists(newFQFile))

            {
                var bakFile = Path.ChangeExtension(newFQFile, ".bak");
                if (File.Exists(bakFile)) File.Delete(bakFile);
                File.Copy(newFQFile, bakFile);
                File.Delete(newFQFile);
            }

            // Replaced Debug.WriteLine($"Labelled CopyFile: extracting segment {ID} to file {newFQFile}");
            await  ExtractWavSegmentAsync(folder, newFQFile,  StartOffsetTimeSpan,  EndOffsetTimeSpan);
            if (File.Exists(newFQFile))
            {
                // Replaced Debug.WriteLine($"Labelled CopyFile: created file {newFQFile}");
                return (newFQFile);
            }
            else
            {
                return "";
            }


        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Labelled CopyFile] Error: {ex.GetType().Name} - {ex.Message}");
            Debug.WriteLine($"[Labelled CopyFile] StackTrace: {ex.StackTrace}");
        }
        finally
        {
#if MACCATALYST
            // Always release the security-scoped resource access
            if (url != null)
            {
                url.StopAccessingSecurityScopedResource();
                // Replaced Debug.WriteLine($"[Labelled CopyFile] Security-scoped resource access released");
            }
#endif
        }
        return newFQFile;
    }

    /// <summary>
    /// Copies source file 'folder' to new file 'newFQFile' starting at startOffsetTimeSpan and ending at endOffsetTimeSpan
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="newFQFile"></param>
    /// <param name="startOffsetTimeSpan"></param>
    /// <param name="endOffsetTimeSpan"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task ExtractWavSegmentAsync(string? folder, string newFQFile, TimeSpan startOffsetTimeSpan, TimeSpan endOffsetTimeSpan)
    {
        if (!string.IsNullOrWhiteSpace(folder) && File.Exists(folder))
        {
            try
            {
                // Replaced Debug.WriteLine($"[ExtractWavSegmentAsync] Extracting segment from {folder} to {newFQFile}");
                await LabelledSegment.CopyWavSegmentAsync(folder, newFQFile, startOffsetTimeSpan, endOffsetTimeSpan);
                // Replaced Debug.WriteLine($"[ExtractWavSegmentAsync] Segment extracted successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExtractWavSegmentAsync] Error: {ex.Message}");
                throw;
            }
        }
        else
        {
            // Replaced Debug.WriteLine($"[ExtractWavSegmentAsync] File does not exist: {folder}");
        }
    }

    /// <summary>
    /// Inserts an autoID summary into the comment and updates the database record
    /// </summary>
    /// <param name="summary"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void InsertSummary(string summary)
    {
        string subcomment = "";
        if (string.IsNullOrWhiteSpace(summary))
        {
            return;
        }
        if ( Comment.Contains("{"))
        {
            var len =  Comment.IndexOf('{');
            subcomment =  Comment.Substring(len).Trim();
             Comment =  Comment.Substring(0, len).Trim();

        }
        if ( Comment.Contains("("))
        {
            var blen =  Comment.IndexOf('(');
             Comment =  Comment.Substring(0, blen).Trim();
        }
         Comment = $"{ Comment} (AutoId: {summary}){subcomment}";
        DBAccessMS.UpdateLabelledSegment(this);
    }

    /// <summary>
    /// For each ided bat, adds a 4 letter code for the bat and adds it to the string
    /// that is returned seperated by commas
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GetIdedBatsString()
    {
        string result = "";

        foreach (var bat in  IdedBats ?? new List<IdedBat>())
        {

            string batCode = bat.GetLabel();
            if (!result.Trim().EndsWith(',')) result += ", ";
            result += batCode;

        }
        return result;
    }

    /// <summary>
    /// Asynchronously copies a segment of a WAV file from start time to end time without using NAudio
    /// </summary>
    /// <param name="inFile">Source WAV file path</param>
    /// <param name="outFile">Destination WAV file path</param>
    /// <param name="start">Start time offset</param>
    /// <param name="end">End time offset</param>
    public static async Task CopyWavSegmentAsync(string inFile, string outFile, TimeSpan start, TimeSpan end)
    {
        if (!File.Exists(inFile))
        {
            // Replaced Debug.WriteLine($"[CopyWavSegmentAsync] Source file not found: {inFile}");
            throw new FileNotFoundException($"Source file not found: {inFile}");
        }

        try
        {
            await Task.Run(() => LabelledSegment.CopyWavSegment(inFile, outFile, start, end));
            // Replaced Debug.WriteLine($"[CopyWavSegmentAsync] Successfully created segment: {outFile}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CopyWavSegmentAsync] Error copying WAV segment: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Converts a duration to a pass count using 5-second intervals, rounded to the nearest interval.
    /// </summary>
    /// <remarks>Rounding is calculated as <c>floor((TotalSeconds + 2.5) / 5.0)</c>.</remarks>
    /// <param name="duration">The duration to convert.</param>
    /// <returns>The number of 5-second passes represented by the duration; positive durations less than one rounded pass
    /// return 1.</returns>
    public static int ToPasses(TimeSpan duration)
    {
        int result = 0;
        var secs = duration.TotalSeconds;
        result = (int)Math.Floor((secs + 2.5d) / 5.0d);
        if (result < 1 && secs > 0.0d) result = 1;

        return result;
    }

    /// <summary>
    /// Copies a segment of a WAV file by directly manipulating the WAV format structure
    /// </summary>
    public static void CopyWavSegment(string inFile, string outFile, TimeSpan start, TimeSpan end)
    {
        using (FileStream inputStream = File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (FileStream outputStream = File.Create(outFile))
        using (BinaryReader reader = new BinaryReader(inputStream))
        using (BinaryWriter writer = new BinaryWriter(outputStream))
        {
            // Validate and read RIFF header
            if (new string(reader.ReadChars(4)) != "RIFF")
                throw new InvalidOperationException("Invalid WAV file: missing RIFF header");

            int riffSize = reader.ReadInt32();
            if (new string(reader.ReadChars(4)) != "WAVE")
                throw new InvalidOperationException("Invalid WAV file: missing WAVE header");

            // Parse chunks to find fmt and data
            int sampleRate = 0;
            int channels = 0;
            int bytesPerSample = 0;
            long dataStartPos = 0;
            int dataSize = 0;

            while (inputStream.Position < inputStream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunkId == "fmt ")
                {
                    short audioFormat = reader.ReadInt16();
                    if (audioFormat != 1) // PCM only
                        throw new InvalidOperationException("Only PCM WAV files are supported");

                    channels = reader.ReadInt16();
                    sampleRate = reader.ReadInt32();
                    int byteRate = reader.ReadInt32();
                    short blockAlign = reader.ReadInt16();
                    short bitsPerSample = reader.ReadInt16();
                    bytesPerSample = bitsPerSample / 8;

                    inputStream.Seek(chunkSize - 16, SeekOrigin.Current);
                }
                else if (chunkId == "data")
                {
                    dataStartPos = inputStream.Position;
                    dataSize = chunkSize;
                    break;
                }
                else
                {
                    inputStream.Seek(chunkSize, SeekOrigin.Current);
                }
            }

            if (sampleRate == 0)
                throw new InvalidOperationException("Invalid WAV file: missing fmt chunk");
            if (dataStartPos == 0)
                throw new InvalidOperationException("Invalid WAV file: missing data chunk");

            // Calculate byte positions based on time offsets
            int bytesPerSecond = sampleRate * channels * bytesPerSample;
            int startBytes = (int)(start.TotalSeconds * bytesPerSecond);
            int endBytes = (int)(end.TotalSeconds * bytesPerSecond);

            // Align to sample boundary (important for audio quality)
            int sampleBlockSize = channels * bytesPerSample;
            startBytes = startBytes - startBytes % sampleBlockSize;
            endBytes = endBytes - endBytes % sampleBlockSize;

            int segmentSize = Math.Max(0, endBytes - startBytes);

            // Write output WAV header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + segmentSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // Write fmt chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(bytesPerSecond);
            writer.Write((short)sampleBlockSize);
            writer.Write((short)(bytesPerSample * 8));

            // Write data chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(segmentSize);

            // Copy audio segment
            inputStream.Seek(dataStartPos + startBytes, SeekOrigin.Begin);
            byte[] buffer = new byte[4096];
            int bytesRemaining = segmentSize;

            while (bytesRemaining > 0)
            {
                int bytesToRead = Math.Min(bytesRemaining, buffer.Length);
                int bytesRead = inputStream.Read(buffer, 0, bytesToRead);
                if (bytesRead <= 0) break;

                writer.Write(buffer, 0, bytesRead);
                bytesRemaining -= bytesRead;
            }
        }
    }


}
 