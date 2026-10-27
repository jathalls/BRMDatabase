using System;
using System.Collections.Generic;
using System.Text;

namespace BRMDatabase.Interfaces
{
    public interface IDatabasePathProvider
    {
        string DatabasePath { get; set; }
    }
}
