using Microsoft.EntityFrameworkCore;

namespace BRMDatabase
{
    /// <summary>
    /// Database context for recording sessions and recordings
    /// </summary>
    public class RecordingDbContext : DbContext
    {
        public RecordingDbContext(DbContextOptions<RecordingDbContext> options) : base(options)
        {
        }

        public DbSet<RecordingSession> RecordingSessions { get; set; }
        public DbSet<Recording> Recordings { get; set; }

        public DbSet<Bat> Bats { get; set; }

        public DbSet<BatTag> BatTags { get; set; }

        public DbSet<Call> Calls { get; set; }

        public DbSet<DbVersion> DbVersions { get; set; }

        public DbSet<IdedBat> IdedBats { get; set;  }

        public DbSet<LabelledSegment> LabelledSegments {get;set;}

        public DbSet<Meta> Metas { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure RecordingSession
            modelBuilder.Entity<RecordingSession>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.SessionTag).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SessionStart).IsRequired();
                entity.Property(e => e.SessionEnd);
                entity.Property(e => e.Temp);
                entity.Property(e=>e.Equipment).HasMaxLength(255);
                entity.Property(e => e.microphone).HasMaxLength(255);
                entity.Property(e=>e.Operator).HasMaxLength(255);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.LocationGPSLongitude).HasDefaultValue(0);
                entity.Property(e => e.LocationGPSLatitude).HasDefaultValue(53);
                entity.Property(e => e.SessionNotes).HasMaxLength(1000);
                entity.Property(e=>e.OriginalFilePath).HasMaxLength(500);
                entity.Property(e => e.Sunset);
                entity.Property(e => e.Weather).HasMaxLength(255);
                

            });

            // Configure Recording
            modelBuilder.Entity<Recording>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.RecordingName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RecordingStartTime).IsRequired();
                entity.Property(e => e.RecordingEndTime).IsRequired();
                entity.Property(e => e.RecordingGPSLongitude).HasDefaultValue(200);
                entity.Property(e => e.RecordingGPSLatitude).HasDefaultValue(200);
                entity.Property(e => e.RecordingNotes).HasMaxLength(2000);
                entity.Property(e => e.RecordingDate);
                
                
                // Configure foreign key relationship
                entity.HasOne(r => r.Session)
                      .WithMany(s => s.Recordings)
                      .HasForeignKey(r => r.SessionID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure indexes for performance
            modelBuilder.Entity<Recording>(entity =>
            {
                entity.HasIndex(e => e.SessionID);
                entity.HasIndex(e => e.RecordingDate);
            });

            //Configure Meta
            modelBuilder.Entity<Meta>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Label).HasMaxLength(255);
                entity.Property(e => e.Value).HasMaxLength(1000);
                entity.Property(e => e.Type).HasMaxLength(255);
               

                entity.HasOne(m => m.Recording)
                    .WithMany(r => r.Metas)
                    .HasForeignKey(m => m.RecordingID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure LabelledSegment
            modelBuilder.Entity<LabelledSegment>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.StartOffset);
                entity.Property(e => e.EndOffset);
                entity.Property(e => e.Comment).HasMaxLength(1000);
                entity.Property(e => e.AutoID).HasMaxLength(255);

                entity.HasOne(l => l.Recording)
                    .WithMany(r => r.LabelledSegments)
                    .HasForeignKey(l=> l.RecordingID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Session)
                    .WithMany(s => s.Segments)
                    .HasForeignKey(l=>l.SessionID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure IdedBats
            modelBuilder.Entity<IdedBat>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.ByAutoId).HasDefaultValue(false);

                entity.HasOne(ib => ib.Segment)
                    .WithMany(s => s.IdedBats)
                    .HasForeignKey(ib => ib.SegmentID)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ib => ib.Session)
                    .WithMany(s => s.IdedBats)
                    .HasForeignKey(ib => ib.SessionID)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ib => ib.Recording)
                    .WithMany(r => r.IdedBats)
                    .HasForeignKey(ib => ib.RecordingID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure Bats
            modelBuilder.Entity<Bat>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.BatGenus).HasMaxLength(50);
                entity.Property(e => e.BatSpecies).HasMaxLength(50);
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Label).HasMaxLength(10);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });

            modelBuilder.Entity<BatTag>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Tag).HasMaxLength(50);

                entity.HasOne(bt => bt.Bat)
                    .WithMany(b => b.Tags)
                    .HasForeignKey(bt => bt.BatID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configure Calls
            modelBuilder.Entity<Call>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.CallFunction).HasMaxLength(50);
                entity.Property(e => e.CallNotes).HasMaxLength(255);
                entity.Property(e => e.CallType).HasMaxLength(50);
                entity.Property(e => e.PeakFrequency);
                entity.Property(e => e.PeakFrequencyVariation);
                entity.Property(e => e.PulseInterval);
                entity.Property(e => e.PulseIntervalVariation);
                entity.Property(e => e.PulseDuration);
                entity.Property(e => e.PulseDurationVariation);
                entity.Property(e => e.StartFrequency);
                entity.Property(e => e.StartFrequencyVariation);
                entity.Property(e => e.EndFrequency);
                entity.Property(e => e.EndFrequencyVariation);
                entity.Property(e => e.FirstHalfPeak);
                entity.Property(e => e.FirstHalfPeakVariation);
                entity.Property(e => e.SecondHalfPeak);
                entity.Property(e => e.SecondHalfPeakVariation);
                entity.Property(e => e.ByAutoID).HasDefaultValue(false);

                entity.HasOne(c => c.Bat)
                    .WithMany(b => b.BatCalls)
                    .HasForeignKey(c => c.batID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Segment)
                    .WithMany(s => s.Calls)
                    .HasForeignKey(c => c.SegmentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //Configue DbVersion
            modelBuilder.Entity<DbVersion>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Version);
            });
        }
    }
    /*
    /// <summary>
    /// Base RecordingSession class (mapped to database)
    /// </summary>
    public class RecordingSession
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<Recording> Recordings { get; set; } = new List<Recording>();
    }*/

  /*  /// <summary>
    /// Base Recording class (mapped to database)
    /// </summary>
    public class Recording
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int Duration { get; set; } // in seconds
        public DateTime RecordingDate { get; set; } = DateTime.UtcNow;
        public int SessionId { get; set; }
        
        // Navigation property
        public RecordingSession Session { get; set; } = null!;
    }*/
}
