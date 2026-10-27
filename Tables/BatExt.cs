using System.Runtime.CompilerServices;

namespace BRMDatabase.Tables
{
    /// <summary>
    /// Provides extension members and associated state management for <see cref="Bat"/> instances.
    /// </summary>
    /// <remarks>Uses a <see cref="ConditionalWeakTable{TKey, TValue}"/> to attach supplemental data to <see
    /// cref="Bat"/> instances without extending their lifetime.</remarks>
    public static class BatExt
    {
        /// <summary>
        /// Backing store for the Bat class
        /// </summary>
        private static readonly ConditionalWeakTable<Bat, BatData> BatStore = new();

        /// <summary>
        /// Represents data associated with a bat.
        /// </summary>
        class BatData
        {
            

        }

        /// <summary>
        /// Provides extension members for <see cref="Bat"/> instances.
        /// </summary>
        /// <param name="bat">The <see cref="Bat"/> instance to extend.</param>
        extension(Bat bat)
        {
            public string CallType
            {
                get
                {
                    string type = "";
                    foreach (var call in bat.BatCalls ?? new List<Call>())
                    { type += $"{call.CallType};"; }
                    return type;

                }
            }

        }
    }
}
