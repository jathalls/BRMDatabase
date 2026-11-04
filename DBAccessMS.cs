using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace BRMDatabase
{
    public static class DBAccessMS
    {
#if DEBUG
        public static string defaultDatabase = Path.Combine(FileSystem.AppDataDirectory, "BRMDatabaseDebug.db");
        
        public static string workingDatabase = Path.Combine(FileSystem.AppDataDirectory, "BRMDatabaseDebug.db");
        
#else
        public static string defaultDatabase = Path.Combine(FileSystem.AppDataDirectory, "BRMDatabase.db");

        public static string workingDatabase = Path.Combine(FileSystem.AppDataDirectory, "BRMDatabase.db");
#endif
        public static RecordingContextFactory? factory = null;

        private static bool isBatListLoaded=false;
        public static void InitialiseDatabase(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName)) databaseName = defaultDatabase;
            workingDatabase = databaseName;

            factory = new RecordingContextFactory(workingDatabase);

            
        }

        public static void CloseConnection()
        {
            if(recordingDbContext is not null)
            {
                recordingDbContext.Dispose();
                recordingDbContext = null;
            }
        }

        public static RecordingDbContext? recordingDbContext { get; set; } = null;

        public static RecordingDbContext? GetContext()
        {
            if (recordingDbContext is not null) return recordingDbContext;

            if(factory is null) InitialiseDatabase(defaultDatabase);

            // Create context for direct usage
            var context = factory?.CreateContext();
            if(context is null)
            {
                if (File.Exists(workingDatabase))
                {
                    factory= new RecordingContextFactory(workingDatabase);
                    context= factory?.CreateContext();
                    context?.Database.EnsureCreated();
                }
                else
                {
                    if (File.Exists(defaultDatabase))
                    {
                        factory= new RecordingContextFactory(defaultDatabase);
                        context= factory?.CreateContext();
                        context?.Database.EnsureCreated();
                    }
                }
            }
            if (!(context?.Database.EnsureCreated()??false))
            {
                Debug.WriteLine($"Database exists");
            }


            if (!isBatListLoaded)
            {
                var bats = context?.Bats.ToList();
                if (!bats.Any())
                {
                    PreloadBatReference(context); // passes context to prevent a recursive call to getContext();
                    isBatListLoaded = true;
                }
            }

            recordingDbContext = context;
            return recordingDbContext;
        }

        public static void ReloadAllBats()
        {
            var context = GetContext();
            var bats = context?.Bats.ToList();
            foreach(var bat in bats)
            {
                context?.Bats.Remove(bat);
            }
            context?.SaveChanges();
            PreloadBatReference(context);
        }

        /// <summary>
        /// Loads the reference list of bats
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private static void PreloadBatReference(RecordingDbContext? context)
        {
            if (context is null) return;
            var xmlBats = GetBatReference();
            foreach(var batXL in xmlBats??new List<XElement>())
            {
                InsertBatElement(batXL, context);
            }
            
        }

        /// <summary>
        /// Adds an XML bat item tot he database
        /// </summary>
        /// <param name="batXL"></param>
        /// <param name="context"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void InsertBatElement(XElement batXL, RecordingDbContext context)
        {
            if (batXL is null) return;
            if (context is null) return;
            var batEx = ConvertXmlBat(batXL);
            if (batEx != null)
            {
                context.Bats.Add(batEx);
                context.SaveChanges();
            }

        }

        private static Bat ConvertXmlBat(XElement bat)
        {
            var newBat = new Bat();
            Bat? insertedBat = null;
            try
            {
                newBat.Name = (bat?.Descendants("BatCommonName")?.FirstOrDefault()?.Value) ?? "";
                newBat.BatGenus = (bat?.Descendants("BatGenus")?.FirstOrDefault()?.Value) ?? "";
                newBat.BatSpecies = (bat?.Descendants("BatSpecies")?.FirstOrDefault()?.Value) ?? "";

                if (bat?.Descendants("BatNotes")?.Any() ?? false)
                {

                    newBat.Notes = (bat?.Descendants("BatNotes")?.FirstOrDefault()?.Value) ?? "";
                }
                newBat.Label = (bat?.Descendants("Label")?.FirstOrDefault()?.Value) ?? "";


                //newBat.Notes = "";
                var parameters = "";

                var newTags = bat?.Descendants("BatTag") ?? Enumerable.Empty<XElement>();
                if (newTags != null && ((newTags?.Count()) ?? 0) > 0)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    var tagList = newTags.Select(tag => tag.Value).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
                    newBat.Tags = new List<BatTag>();
                    foreach (var tag in tagList)
                    {
                        var batTag = new BatTag();
                        batTag.Tag = tag;
                        newBat.Tags.Add(batTag);
                    }

                }

                var newCommonNames = bat?.Descendants("BatCommonName") ?? Enumerable.Empty<XElement>();
                if (((newCommonNames?.Count()) ?? 0) > 0) newBat.Name = (newCommonNames?.First().Value ?? "");
                newBat.BatCalls.Clear();

                var callData = bat?.Descendants("Call") ?? Enumerable.Empty<XElement>();
                if (((callData?.Count()) ?? 0) > 0)
                {
                    foreach (var call in callData ?? Enumerable.Empty<XElement>())
                    {
                        var dbCall = GetXmlCallParameters(call, parameters);
                        newBat.BatCalls.Add(dbCall);
                    }
                }




            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return newBat;
        }

        private static Call GetXmlCallParameters(XElement call, string parameters)
        {
            var dbCall = new Call();
            dbCall?.Segment = null;
            dbCall?.SegmentID = null;
            var mean = 0.0d;
            var variation = 0.0d;
            var xFstart = call.Descendants("fStart");
            if (((xFstart?.Count()) ?? 0) > 0)
            {
                parameters = xFstart?.FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(parameters))
                    if (MauiLib1.Tools.GetValuesAsMeanAndVariation(parameters, out mean, out variation))
                    {
                        dbCall.StartFrequency = mean;
                        dbCall.StartFrequencyVariation = variation;
                    }
            }

            mean = 0.0d;
            variation = 0.0d;
            var xFend = call.Descendants("fEnd");
            if ((xFend?.Any()??false))
            {
                parameters = xFend?.FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(parameters))
                    if (MauiLib1.Tools.GetValuesAsMeanAndVariation(parameters, out mean, out variation))
                    {
                        dbCall.EndFrequency = mean;
                        dbCall.EndFrequencyVariation = variation;
                    }
            }

            mean = 0.0d;
            variation = 0.0d;
            var xFpeak = call.Descendants("fPeak");
            if ((xFpeak?.Any() ?? false))
            {
                parameters = xFpeak?.FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(parameters))
                    if (MauiLib1.Tools.GetValuesAsMeanAndVariation(parameters, out mean, out variation))
                    {
                        dbCall.PeakFrequency = mean;
                        dbCall.PeakFrequencyVariation = variation;
                    }
            }

            mean = 0.0d;
            variation = 0.0d;
            var xInterval = call.Descendants("Interval");
            if ((xInterval?.Any() ?? false))
            {
                parameters = xInterval?.FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(parameters))
                    if (MauiLib1.Tools.GetValuesAsMeanAndVariation(parameters, out mean, out variation))
                    {
                        dbCall.PulseInterval = mean;
                        dbCall.PulseIntervalVariation = variation;
                    }
            }

            mean = 0.0d;
            variation = 0.0d;
            var xDuration = call.Descendants("Duration");
            if ((xDuration?.Any() ?? false))
            {
                parameters = xDuration?.FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(parameters))
                    if (MauiLib1.Tools.GetValuesAsMeanAndVariation(parameters, out mean, out variation))
                    {
                        dbCall.PulseDuration = mean;
                        dbCall.PulseDurationVariation = variation;
                    }
            }

            var xFunction = call.Descendants("Function");
            if ((xFunction?.Any() ?? false))
            {
                parameters = xFunction?.FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(parameters)) dbCall.CallFunction = parameters;
            }

            var xType = call.Descendants("Type");
            if ((xType?.Any() ?? false))
            {
                parameters = xType?.FirstOrDefault()?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(parameters)) dbCall.CallType = parameters;
            }

            var xComments = call.Descendants("Comments");
            if ((xComments?.Any() ?? false))
            {
                parameters = xComments?.FirstOrDefault()?.Value ?? "";

                if (!string.IsNullOrWhiteSpace(parameters)) dbCall.CallNotes = parameters;
            }

            return dbCall;
        }

        /// <summary>
        /// loads the XML bat reference file
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static List<XElement>? GetBatReference()
        {
            List<XElement> result = new List<XElement>();
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "BRM_2.Resources.Data.BatReferenceXMLFile.xml";
                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        return result;
                    }
                    var xData = XElement.Load(stream, LoadOptions.None);
                    result = xData.Descendants("Bat").ToList<XElement>();
                }
            }catch(Exception ex)
            {
                Debug.WriteLine($"Error reading BatReference {ex.Message},{ex.InnerException?.Message??""}");
            }
            return result;
        }

        public static List<RecordingSession> GetRecordingSessionsList()
        {
            List<RecordingSession> sessionList = new List<RecordingSession>();

            var context=GetContext();
            
                sessionList = (context?.RecordingSessions
                .Include(rs => rs.Recordings).ThenInclude(r => r.LabelledSegments).ThenInclude(ls => ls.IdedBats)
                .Include(rs => rs.Recordings).ThenInclude(r => r.LabelledSegments).ThenInclude(ls => ls.Calls)
                
                .Include(rs => rs.Recordings).ThenInclude(r => r.Metas)
                
                
                
                            .ToList())??new List<RecordingSession>();
            

                return sessionList;
        }

        /// <summary>
        /// Updates the specified labelled segment in the data context.
        /// </summary>
        /// <remarks>Marks the entity for update in the current context. Changes are persisted only when
        /// the context saves changes.</remarks>
        /// <param name="segment">The labelled segment entity to update.</param>
        public static void UpdateLabelledSegment(LabelledSegment segment)
        {
            var context=GetContext();
            {
                context.LabelledSegments.Update(segment);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Marks the specified recording entity for update in the current data context.
        /// </summary>
        /// <remarks>Uses a new context instance for the operation.</remarks>
        /// <param name="recording">The recording entity containing the values to update.</param>
        public static void UpdateRecording(Recording recording)
        {
            var context=GetContext();
            {
                context.Recordings.Update(recording);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Updates an existing recording session in the data context.
        /// </summary>
        /// <remarks>Marks the entity as modified in the current context; changes are persisted when the
        /// context is saved.</remarks>
        /// <param name="session">The recording session entity to update.</param>
        public static int UpdateRecordingSession(RecordingSession session)
        {
            var context=GetContext();
            {
                context?.RecordingSessions.Update(session);
                context?.SaveChanges();
            }
            return session.ID;
        }

        public static void DeleteRecordingSession(RecordingSession? sessionToDelete)
        {
            
            if(sessionToDelete!=null && sessionToDelete.ID>=0)
            {
                DeleteRecordingSession(sessionToDelete.ID);
            }
        }

        public static void DeleteRecordingSession(int sessionID)
        {
            var context = GetContext();
            var session = context?.RecordingSessions.Where(s => s.ID == sessionID).FirstOrDefault();
            context?.RecordingSessions.Remove(session);
            context?.SaveChanges();
        }

        public static Bat? GetBatByID(int batID)
        {
            Bat? result = null;
            var context=GetContext();
            {
                result = context?.Bats
                    .Include(b => b.Tags).Include(b => b.BatCalls)
                    .Where(b => b.ID == batID).FirstOrDefault();
            }
            return result;
        }

        public static List<BatTag> GetContainedTags(string description)
        {
            List<BatTag> result = new List<BatTag>();
            var context=GetContext();
            {
                string udescription = description.ToUpper();


                var allTags = context?.BatTags.ToList()??new List<BatTag>();
                var allBats = context?.Bats
                    .Include(b=>b.Tags).Include(b=>b.BatCalls).ToList()??new List<Bat>();
                
                foreach (var tag in allTags)
                {
                    if (tag.Tag == tag.Tag.ToUpper()) // all upper case tag, so match must  be case dependent
                    {
                        if (description.Contains(tag.Tag)) result.Add(tag);
                    }
                    else
                    { // mixed case tag, so do a case independent match - tag is already upper case so no need to modify
                        if (udescription.Contains(tag.Tag.ToUpper())) result.Add(tag);
                    }
                }
                //var tags = BatCol.Select(batEntry => batEntry.BatTags.Where(bt => udescription.Contains(bt.Tag.ToUpper())));
                //foreach (var tag in tags) { result.AddRange(tag.Select(t=>t.Tag)); }
                //var LatinNames = BatCol.Select(batEntry => batEntry.BatGenus + " " + batEntry.BatSpecies);
                //result.AddRange(LatinNames);
                //var CommonNames = BatCol.Select(batEntry => batEntry.Name);

                var final = new List<BatTag>();
                if (result.Any()) final.Add(result[0]);
                foreach (var res in result)
                {
                    for (int i = 0; i < final.Count; i++)
                    {
                        if (final[i].BatID == res.BatID)
                        {
                            if (res.Tag.Length > final[i].Tag.Length)
                            {
                                final[i] = res;
                                continue;
                            }
                        }
                        else
                        {
                            final.Add(res);
                        }
                    }
                }

                result = final;

                
                
            }
            return result;
        }


        public static Bat? GetBatByTag(string tag)
        {
            Bat? bat=null;

            var context=GetContext();
            {

                if (!string.IsNullOrWhiteSpace(tag))

                {
                    var batTag =  context?.BatTags.Where(t => t.Tag == tag).FirstOrDefault();

                    if (batTag != null)
                    {
                        bat = context?.Bats
                            .Include(b=>b.Tags)
                            .Include(b=>b.BatCalls)
                            .Where(b => b.ID == batTag.BatID).FirstOrDefault();

                    }
                }
            }





            return bat;
        }

        public static List<IdedBat> GetIdedBatsForSegment(int iD)
        {

            if (iD >= 0)
            {
                var context=GetContext();
                {
                    var bats =  context?.IdedBats.Where(ib => ib.SegmentID == iD).ToList()??new List<IdedBat>();
                    return bats;
                }
            }
            return new List<IdedBat>();
        }

        public static Bat? GetNamedBat(string name)
        {
            Bat? bat = null;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var context=GetContext();
                {
                    bat= context.Bats.Where(b=>b.Name == name).FirstOrDefault();
                }
            }
            return bat;
        }

        public static List<BatTag>? GetBatTags(int batId)
        {
            List<BatTag> tags=new List<BatTag>();
            if (batId >= 0)
            {
                var context=GetContext();
                {
                    tags = context?.BatTags.Where(bt => bt.BatID == batId).ToList()??new List<BatTag>();
                }
            }
            return tags;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recordingId"></param>
        /// <returns></returns>
        public static async Task<List<LabelledSegment>?> GetSegmentsForRecordingAsync(int recordingId)
        {
            var segList = new List<LabelledSegment>();
            if (recordingId >= 0)
            {
                var context=GetContext();
                {
                    segList = await context.LabelledSegments
                        .Include(ls=>ls.IdedBats)
                        .Include(ls=>ls.Calls)
                        .Where(ls => ls.RecordingID == recordingId).ToListAsync();
                }
            }
            return segList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recordingId"></param>
        /// <returns></returns>
        public static List<LabelledSegment> GetSegmentsForRecording(int recordingId)
        {
            var segList = new List<LabelledSegment>();
            if (recordingId >= 0)
            {
                var context=GetContext();
                {
                    segList = context?.LabelledSegments
                        .Include(l=>l.IdedBats)
                        .Include(l=>l.Calls)
                        .Where(ls => ls.RecordingID == recordingId)?.ToList()??segList;
                }
            }
            return segList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Bat>>? GetAllBatsAsync()
        {
            var context=GetContext();
            {
                return await context?.Bats?.Include(b => b.Tags)?.Include(b => b.BatCalls)?.ToListAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public static List<Recording>? GetRecordingsForSession(int sessionID)
        {
            var context = GetContext();
            return context?.Recordings
                .Include(r=>r.LabelledSegments).ThenInclude(ls=>ls.IdedBats)
                .Include(r=>r.LabelledSegments).ThenInclude(ls=>ls.Calls)
                
                .Include(r=>r.Metas)
                .Where(r=>r.SessionID == sessionID).ToList()??new List<Recording>();
        }

        public static RecordingSession? GetSessionForTag(string sessionTag)
        {
            var context = GetContext();
            var res = context?.RecordingSessions
                .Include(rs => rs.Recordings).ThenInclude(r => r.LabelledSegments).ThenInclude(ls => ls.IdedBats)
                .Include(rs => rs.Recordings).ThenInclude(r => r.LabelledSegments).ThenInclude(ls => ls.Calls)
                
                .Include(rs => rs.Recordings).ThenInclude(r => r.Metas)
               
                .Where(rs => rs.SessionTag == sessionTag).FirstOrDefault();
            return res;
        }

        public static RecordingSession? GetRecordingSession(int id)
        {
            var context = GetContext();
            return context?.RecordingSessions
                .Include(rs => rs.Recordings).ThenInclude(r => r.LabelledSegments).ThenInclude(ls => ls.IdedBats)
                .Include(rs => rs.Recordings).ThenInclude(r => r.LabelledSegments).ThenInclude(ls => ls.Calls)
                
                .Include(rs => rs.Recordings).ThenInclude(r => r.Metas)
               
                .Where(s => s.ID==id).FirstOrDefault();
        }

        public static List<Bat> GetAllBats()
        {
            var context = GetContext();
            return context?.Bats?
                .Include(b=>b.Tags)
                .Include(b=>b.BatCalls)
                .ToList()??new List<Bat>();
        }

        public static List<string> GetAllLocationsList()
        {
            var context = GetContext();
            return context?.RecordingSessions.Select(s => s.Location).ToList() ?? new List<String>();
        }

        public static IEnumerable<string> GetAllOperatorsList()
        {
            var context = GetContext();
            return context?.RecordingSessions.Select(s=>s.Operator).ToList() ?? new List<String>();
        }

        public static IEnumerable<string> GetAllEquipmentList()
        {
            var context = GetContext();
            return context?.RecordingSessions.Select(s=>s.Equipment).ToList() ?? new List<String>();
        }

        public static IEnumerable<string> GetAllMicrophonesList()
        {
            var context = GetContext();
            return context?.RecordingSessions.Select(s=>s.microphone).ToList() ?? new List<String>();
        }

        public static RecordingSession InsertXMLSession(string fileName)
        {
            RecordingSession session = new RecordingSession();


            try
            {
                XDocument doc = XDocument.Load(fileName);

                XElement? RecSessElement = doc?.Descendants("RecordingSession").FirstOrDefault();
                if (RecSessElement != null)
                {
                    session = InsertBRM1Session(RecSessElement); // gets a null return if there is no sessionTag
                    return session;
                }


                foreach (var element in doc.Root?.DescendantsAndSelf() ?? Enumerable.Empty<XElement>())
                {
                    element.Name = element.Name.LocalName;  // Keep local name, remove namespace
                    element.Attributes().Where(a => string.IsNullOrWhiteSpace(a.Value)).Remove();
                }

                using (var reader = doc?.Root?.CreateReader())
                {
                    if (reader != null)
                    {
                        var serializer = new XmlSerializer(typeof(RecordingSession));
                        session = (RecordingSession?)(serializer.Deserialize(reader)) ?? new RecordingSession();
                        reader.Close();
                    }
                }
                DBAccessMS.UpdateRecordingSession(session);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            return (session);
        }


        private static  RecordingSession? InsertBRM1Session(XElement? recSessElement)
        {
            RecordingSession? session = null;

            session = new RecordingSession();
            Debug.WriteLine(($"Importing a session from XML"));
            session.SessionTag = recSessElement?.Attribute("SessionTag")?.Value ?? "";
            if (string.IsNullOrWhiteSpace(session.SessionTag)) return null;

            var existingSession = DBAccessMS.GetSessionForTag(session.SessionTag);
            if (existingSession != null && existingSession.ID > 0) DBAccessMS.DeleteRecordingSession(existingSession);


            session.Location = recSessElement?.Attribute("Location")?.Value ?? "";
            if (DateTime.TryParse(recSessElement?.Attribute("SessionDate")?.Value ?? new DateTime().ToString(), CultureInfo.InvariantCulture, out DateTime sdt))
            {
                session.SessionStart = sdt;
            }

            if (DateTime.TryParse(recSessElement?.Attribute("EndDate")?.Value ?? new DateTime().ToString(), CultureInfo.InvariantCulture, out DateTime edt))
            {
                session.SessionEnd = edt;
            }

            var etstr = recSessElement?.Attribute("SessionEndTime")?.Value ?? "";
            if (!string.IsNullOrWhiteSpace(etstr))
            {
                try
                {
                    TimeSpan ets = XmlConvert.ToTimeSpan(etstr);
                    session.SessionEnd = session.SessionEnd.Date + ets;
                }
                catch (FormatException)
                {
                }
            }

            session.Weather = recSessElement?.Attribute("Weather")?.Value ??
                (recSessElement?.Attribute("Temp")?.Value ?? "");
            var tstr = recSessElement?.Attribute("Temp")?.Value ?? "";
            if (!string.IsNullOrWhiteSpace(tstr))
            {
                try { session.Temp = XmlConvert.ToDecimal(tstr); }
                catch (FormatException)
                {
                }
            }
            session.Equipment = recSessElement?.Attribute("Equipment")?.Value ?? "";
            session.microphone = recSessElement?.Attribute("Microphone")?.Value ?? "";
            session.Operator = recSessElement?.Attribute("Operator")?.Value ?? "";
            if (decimal.TryParse(recSessElement?.Attribute("LocationGPSLatitude")?.Value ?? "", out decimal latit))
            {
                session.LocationGPSLatitude = latit;
            }
            if (decimal.TryParse(recSessElement?.Attribute("LocationGPSLongitude")?.Value ?? "", out decimal longit))
            {
                session.LocationGPSLongitude = longit;
            }
            session.SessionNotes = recSessElement?.Attribute("SessionNotes")?.Value ?? "";
            session.OriginalFilePath = recSessElement?.Attribute("OriginalFilePath")?.Value ?? "";

            var ssstr = recSessElement?.Attribute("Sunset")?.Value ?? "";
            if (!string.IsNullOrWhiteSpace(etstr))
            {
                try
                {
                    TimeSpan sst = XmlConvert.ToTimeSpan(ssstr);
                    session.Sunset = session.SessionStart.Date + sst;
                }
                catch (FormatException)
                {
                    Debug.WriteLine($"Format Exception in sunset");
                }
            }

            if (string.IsNullOrWhiteSpace(session?.SessionTag ?? "")) { return null; }

            
            Debug.WriteLine($"\tAdd Recordings to session...");
            session.Recordings = GetXmlRecordings(recSessElement?.Descendants("Recording"));
            //var segs= session.Recordings.SelectMany(rec => rec.LabelledSegments).ToList();
            //session.Segments = segs;
            //var ibats= session.Recordings.SelectMany(rec => rec.IdedBats).Distinct().ToList();
            //session.IdedBats = ibats;
            
            Debug.WriteLine(session.SessionTag);
            try
            {
                //UpdateRecordingSession(session);
                var context = GetContext();
                context?.RecordingSessions.Update(session);
                context?.SaveChanges();
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return (session);


        }

        private static List<Recording> GetXmlRecordings(IEnumerable<XElement>? Recordings)
        {
            List<Recording> result = new List<Recording>();
            foreach (var rec in Recordings ?? Enumerable.Empty<XElement>())
            {
                Recording? recording = GetXmlRecording(rec);
                if (recording != null) result.Add(recording);
            }
            return result;
        }

        private static Recording? GetXmlRecording(XElement rec)
        {
            Recording? result = new Recording();
            try
            {
                Debug.WriteLine($"Importing Recording from XML");
                if (rec != null)
                {
                    result.RecordingName = rec.Attribute("RecordingName")?.Value ?? "";

                    var rsdstr = rec.Attribute("RecordingDate")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(rsdstr))
                    {
                        result.RecordingDate = XmlConvert.ToDateTime(rsdstr);
                    }

                    var rststr = rec.Attribute("RecordingStartTime")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(rststr))
                    {
                        result.RecordingStartTime = new DateTime() + XmlConvert.ToTimeSpan(rststr);
                        result.RecordingDate = result.RecordingDate.Date + result.RecordingStartTime.TimeOfDay;
                        result.RecordingStartTime = result.RecordingDate;
                    }

                    var resstr = rec.Attribute("RecordingEndTime")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(resstr))
                    {
                        result.RecordingEndTime = result.RecordingDate.Date + XmlConvert.ToTimeSpan(resstr);
                    }

                    var longitstr = rec.Attribute("RecordingGPSLongitude")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(longitstr))
                    {
                        result.RecordingGPSLongitude = XmlConvert.ToDouble(longitstr);
                    }
                    var latitstr = rec.Attribute("RecordingGPSLongitude")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(latitstr))
                    {
                        result.RecordingGPSLatitude = XmlConvert.ToDouble(latitstr);
                    }

                    result.RecordingNotes = rec.Attribute("RecordingNotes")?.Value ?? "";

                    //result.IdedBats = result.LabelledSegments.SelectMany(ls => ls.IdedBats).Distinct().ToList();

                }
            }
            catch (FormatException) { return null; }
           
            Debug.WriteLine("Add Metas to Recording...");
            result.Metas = GetXmlMetas(rec.Descendants("Meta"));
            Debug.WriteLine("Add Segments to recording...");
            result.LabelledSegments = GetXmlLabelledSegments(rec.Descendants("LabelledSegment"));
            Debug.WriteLine("\tCompleted recording");
            return result;
        }

        private static List<LabelledSegment> GetXmlLabelledSegments(IEnumerable<XElement> xmlSegments)
        {
            List<LabelledSegment> result = new List<LabelledSegment>();
            try
            {
                foreach (XElement xmlSeg in xmlSegments ?? Enumerable.Empty<XElement>())
                {
                    var seg = new LabelledSegment();
                    var sostr = xmlSeg?.Attribute("StartOffset")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(sostr))
                    {
                        seg.StartOffset = new DateTime() + XmlConvert.ToTimeSpan(sostr);
                    }
                    var eostr = xmlSeg?.Attribute("EndOffset")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(eostr))
                    {
                        seg.EndOffset = new DateTime() + XmlConvert.ToTimeSpan(eostr);
                    }
                    seg.Comment = xmlSeg?.Attribute("Comment")?.Value ?? "";
                    seg.AutoID = xmlSeg?.Attribute("AutoID")?.Value ?? "";
                    var probstr = xmlSeg?.Attribute("AutoIdProb")?.Value ?? "0";
                    if (!string.IsNullOrWhiteSpace(probstr))
                    {
                        seg.AutoIdProb = XmlConvert.ToDouble(probstr);
                    }
                    seg.Calls = GetXmlCalls(xmlSeg.Descendants("Call"));
                    seg.IdedBats = GetXmlBatIds(xmlSeg.Descendants("IdedBat"));
                    
                    result.Add(seg);
                }
            }
            catch (FormatException)
            {

            }
            return result;
        }

        private static List<IdedBat> GetXmlBatIds(IEnumerable<XElement> xmlIds)
        {
            var result = new List<IdedBat>();
            foreach (var id in xmlIds ?? Enumerable.Empty<XElement>())
            {
                var bat = new IdedBat();
                bat.Name = id?.Attribute("BatName")?.Value ?? "";
                bat.ByAutoId = (id?.Attribute("ByAutoId")?.Value ?? "") == "true";
                
                result.Add(bat);
            }
            return result;
        }

        private static List<Call> GetXmlCalls(IEnumerable<XElement> xmlCalls)
        {

            var result = new List<Call>();
            foreach (var Xcall in xmlCalls)
            {
                var call = new Call();
                call.ID = -1;
                call.CallNotes = Xcall?.Attribute("CallNotes")?.Value ?? "";
                call.ByAutoID = ((Xcall?.Attribute("ByAutoID")?.Value ?? "false") == "true");
                call.CallFunction = Xcall?.Attribute("CallFunction")?.Value ?? "";
                call.CallNotes = Xcall?.Attribute("CallNotes")?.Value ?? "";
                call.CallType = Xcall?.Attribute("CallType")?.Value ?? "";
                call.EndFrequency = XmlConvert.ToDouble(Xcall?.Attribute("EndFrequency")?.Value ?? "0");
                call.EndFrequencyVariation = XmlConvert.ToDouble(Xcall?.Attribute("EndFrequencyVariation")?.Value ?? "0");
                call.FirstHalfPeak = XmlConvert.ToDouble(Xcall?.Attribute("FirstHalfPeak")?.Value ?? "0");
                call.FirstHalfPeakVariation = XmlConvert.ToDouble(Xcall?.Attribute("FirstHalfPeakVariation")?.Value ?? "0");
                call.PeakFrequency = XmlConvert.ToDouble(Xcall?.Attribute("PeakFrequency")?.Value ?? "0");
                call.PeakFrequencyVariation = XmlConvert.ToDouble(Xcall?.Attribute("PeakFrequencyVariation")?.Value ?? "0");
                call.PulseDuration = XmlConvert.ToDouble(Xcall?.Attribute("PulseDuration")?.Value ?? "0");
                call.PulseDurationVariation = XmlConvert.ToDouble(Xcall?.Attribute("PulseDurationVariation")?.Value ?? "0");
                call.PulseInterval = XmlConvert.ToDouble(Xcall?.Attribute("PulseInterval")?.Value ?? "0");
                call.PulseIntervalVariation = XmlConvert.ToDouble(Xcall.Attribute("PulseIntervalVariation")?.Value ?? "0");
                call.SecondHalfPeak = XmlConvert.ToDouble(Xcall?.Attribute("SecondHalfPeak")?.Value ?? "0");
                call.SecondHalfPeakVariation = XmlConvert.ToDouble(Xcall?.Attribute("SecondHalfPeakVariation")?.Value ?? "0");
                call.StartFrequency = XmlConvert.ToDouble(Xcall?.Attribute("StartFrequency")?.Value ?? "0");
                call.StartFrequencyVariation = XmlConvert.ToDouble(Xcall?.Attribute("StartFrequencyVariation")?.Value ?? "0");
                result.Add(call);

               
            }
            return result;


        }

        private static List<Meta> GetXmlMetas(IEnumerable<XElement> xmlMetas)
        {
            List<Meta> result = new List<Meta>();
            foreach (var meta in xmlMetas ?? Enumerable.Empty<XElement>())
            {
                var newMeta = new Meta();
                newMeta.Label = meta?.Attribute("Label")?.Value ?? "";
                newMeta.Value = meta?.Attribute("Value")?.Value ?? "";
                newMeta.Type = meta?.Attribute("Type")?.Value ?? "";
                
                result.Add(newMeta);
            }
            return (result);
        }

        public static List<Call> GetCallsForSegment(int iD)
        {
            var context = GetContext();
            return (context?.LabelledSegments?
                .Include(l=>l.Calls)
                
                .Where(l => l.ID == iD).FirstOrDefault()?.Calls?.ToList()) ?? new List<Call>();

        }

        public static List<Meta> GetMetasForRecording(int iD)
        {
            var context = GetContext();
            return (context?.Recordings?
                .Include(r=>r.Metas)?.Where(r => r.ID == iD)?.FirstOrDefault()?.Metas?.ToList()) ?? new List<Meta>();
        }
    }
}
