namespace BRMDatabase.Utilities
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
        public static void InitialiseDatabase(string databaseName)
        {
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

        internal static RecordingDbContext? GetContext()
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
            recordingDbContext = context;
            return recordingDbContext;
        }

        public static List<RecordingSession> GetRecordingSessionsList()
        {
            List<RecordingSession> sessionList = new List<RecordingSession>();

            var context=GetContext();
            {
                sessionList = (context?.RecordingSessions
                            .Include(s => s.Recordings).ThenInclude(r => r.Metas).Include(r => r.IdedBats)

                            .Include(s => s.Segments).ThenInclude(l => l.Calls).Include(l => l.IdedBats)
                            .Include(s => s.IdedBats)
                            .ToList())??new List<RecordingSession>();
            }

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
        public static void UpdateRecordingSession(RecordingSession session)
        {
            var context=GetContext();
            {
                context.RecordingSessions.Update(session);
                context.SaveChanges();
            }
        }

        internal static async Task<Bat?> GetBatByIDAsync(int batID)
        {
            Bat? result = null;
            var context=GetContext();
            {
                result = context.Bats.Where(b => b.ID == batID).FirstOrDefault();
            }
            return result;
        }

        internal static async Task<List<BatTag>> GetContainedTagsAsync(string description)
        {
            List<BatTag> result = new List<BatTag>();
            var context=GetContext();
            {
                string udescription = description.ToUpper();


                var allTags = await context.BatTags.ToListAsync();
                var allBats = await context.Bats.ToListAsync();
                foreach (var bat in allBats)
                {
                    var tag = new BatTag();
                    tag.BatID = bat.ID;
                    tag.Tag = bat.Name;
                    allTags.Add(tag);
                }
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

                //var batIDs = result.Select(r => r.BatID).Distinct();
                //result = result.OrderByDescending(x => x.Tag.Length).ToList();
                //// Replaced Debug.WriteLine($"In {description}:-");
                foreach (var item in result)
                {
                    // Replaced Debug.Write($"{item.Tag}, ");

                }
                //// Replaced Debug.WriteLine("");
                
            }
            return result;
        }


        internal static async Task<Bat?> GetBatByTagAsync(string tag)
        {
            Bat? bat=null;

            var context=GetContext();
            {

                if (!string.IsNullOrWhiteSpace(tag))

                {
                    var batTag = await context.BatTags.Where(t => t.Tag.Contains(tag)).FirstOrDefaultAsync();

                    if (batTag != null)
                    {
                        bat = context.Bats.Where(b => b.ID == batTag.ID).FirstOrDefault();

                    }
                }
            }





            return bat;
        }

        internal static async Task<List<IdedBat>>? GetIdedBatsForSegmentAsync(int iD)
        {

            if (iD >= 0)
            {
                var context=GetContext();
                {
                    var bats = await context.IdedBats.Where(ib => ib.SegmentID == iD).ToListAsync();
                    return bats;
                }
            }
            return new List<IdedBat>();
        }

        internal static async Task<Bat?> GetNamedBat(string name)
        {
            Bat? bat = null;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var context=GetContext();
                {
                    bat=await context.Bats.Where(b=>b.Name == name).FirstOrDefaultAsync();
                }
            }
            return bat;
        }

        internal static async Task<List<BatTag>?> GetBatTagsAsync(int batId)
        {
            List<BatTag> tags=new List<BatTag>();
            if (batId >= 0)
            {
                var context=GetContext();
                {
                    tags = await context.BatTags.Where(bt => bt.BatID == batId).ToListAsync();
                }
            }
            return tags;
        }

        internal static async Task<List<LabelledSegment>?> GetSegmentsForRecordingAsync(int recordingId)
        {
            var segList = new List<LabelledSegment>();
            if (recordingId >= 0)
            {
                var context=GetContext();
                {
                    segList = await context.LabelledSegments.Where(ls => ls.RecordingID == recordingId).ToListAsync();
                }
            }
            return segList;
        }

        internal static List<LabelledSegment> GetSegmentsForRecording(int recordingId)
        {
            var segList = new List<LabelledSegment>();
            if (recordingId >= 0)
            {
                var context=GetContext();
                {
                    segList = context.LabelledSegments.Where(ls => ls.RecordingID == recordingId)?.ToList()??segList;
                }
            }
            return segList;
        }

        internal static async Task<List<Bat>> GetAllBatsAsync()
        {
            var context=GetContext();
            {
                return await context.Bats.ToListAsync();
            }
        }
    }
}
