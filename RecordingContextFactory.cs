using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BRMDatabase
{
    /// <summary>
    /// Factory for creating DbContext instances for recording database with SQLite
    /// </summary>
    public class RecordingContextFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the RecordingContextFactory class
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file</param>
        public RecordingContextFactory(string databasePath)
        {
            if (string.IsNullOrEmpty(databasePath))
                throw new ArgumentException("Database path cannot be null or empty", nameof(databasePath));
                
            _connectionString = $"Data Source={databasePath}";
        }

        /// <summary>
        /// Creates a new instance of the RecordingDbContext
        /// </summary>
        /// <returns>RecordingDbContext instance</returns>
        public RecordingDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<RecordingDbContext>();
            optionsBuilder.UseSqlite(_connectionString);
            optionsBuilder.UseLazyLoadingProxies();
            
            return new RecordingDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Configures services for dependency injection
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RecordingDbContext>(options =>
                options.UseSqlite(_connectionString).UseLazyLoadingProxies());
        }
    }
}
