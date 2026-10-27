using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BRMDatabase.Tables
{
    


    public static class RecordingSessionExt
    {

        private static readonly ConditionalWeakTable<RecordingSession, SessionData> SessionStore = new();

        class SessionData
        {
            public string BatNames = "";
        }

        extension(RecordingSession session)
        {
            /// <summary>
            /// readony - gets the number of recordings in this session
            /// </summary>
            public int NumberOfRecordings
            {  get 
                {
                    return session.Recordings?.Count() ?? 0;
                    
                }
            }

            public string BatNames
            {
                get
                {
                    var res= SessionStore.GetOrCreateValue(session).BatNames;
                    if (string.IsNullOrWhiteSpace(res))
                    { 
                        return session.GetBatNames();

                    }
                    else
                    {
                        return res;
                    }
                }
                set=>SessionStore.GetOrCreateValue(session).BatNames= value;
            }

            

            public string GetBatNames()
            {
                string result = "";
                if (session.IdedBats != null)
                {
                    var uniqueBats = session.IdedBats.DistinctBy(b => b.Name);
                    foreach(var bat in uniqueBats)
                    {
                        result += bat.Name + Environment.NewLine;
                    }
                }
                session.BatNames = result;
                return result;
            }
        }
    }
}
