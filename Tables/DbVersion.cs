using System;
using System.Collections.Generic;
using System.Text;

namespace BRMDatabase.Tables;


public class DbVersion
{
   
    public int ID { get; set; }

    public int Version { get; set; } = -1;
}
