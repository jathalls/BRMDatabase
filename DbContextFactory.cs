using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRMDatabase
{
    public class DbContextFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the DbContextFactory class
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file</param>
        public DbContextFactory(string databasePath)
        {
            if (string.IsNullOrEmpty(databasePath))
                throw new ArgumentException("Database path cannot be null or empty", nameof(databasePath));

            _connectionString = $"Data Source={databasePath}";
        }

        /// <summary>
        /// Creates a new instance of the DbContext
        /// </summary>
        /// <typeparam name="TContext">The type of DbContext to create</typeparam>
        /// <returns>DbContext instance</returns>
        public TContext CreateContext<TContext>() where TContext : DbContext, new()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseSqlite(_connectionString);

            return new TContext();
        }

        /// <summary>
        /// Configures services for dependency injection
        /// </summary>
        /// <typeparam name="TContext">The type of DbContext to register</typeparam>
        /// <param name="services">Service collection to configure</param>
        public void ConfigureServices<TContext>(IServiceCollection services) where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
                options.UseSqlite(_connectionString));
        }
    }
}
