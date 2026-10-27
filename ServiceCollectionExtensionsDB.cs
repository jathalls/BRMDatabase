using System;
using System.Collections.Generic;
using System.Text;

namespace BRMDatabase
{
    public static  class ServiceCollectionExtensionsDB
    {
        public static void AddDBServices(this IServiceCollection services)
        {
            _=services.AddDbContextFactory<RecordingDbContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseSqlite();
            });
        }
    }
}
