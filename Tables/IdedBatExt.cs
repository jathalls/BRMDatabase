using System.Runtime.CompilerServices;

namespace BRMDatabase.Tables
{
    /// <summary>
    /// Provides extension data support for <see cref="IdedBat"/> instances.
    /// </summary>
    /// <remarks>Associates per-instance state by using a <see cref="ConditionalWeakTable{TKey, TValue}"/>,
    /// allowing related data to be collected when the key instance is no longer referenced.</remarks>
    internal static class IdedBatExt
    {
        /// <summary>
        /// Stores per-instance <see cref="IdedBatData"/> associated with <see cref="IdedBat"/> keys.
        /// </summary>
        /// <remarks>Uses weak key references so associated entries are eligible for collection when a key
        /// is no longer reachable.</remarks>
        private static readonly ConditionalWeakTable<IdedBat, IdedBatData> IdedBatStore = new();

        /// <summary>
        /// Represents battery data associated with a unique identifier.
        /// </summary>
        class IdedBatData
        {


        }

        /// <summary>
        /// Provides extension members for <see cref="IdedBat"/> instances.
        /// </summary>
        /// <param name="idedBat">The <see cref="IdedBat"/> instance to extend.</param>
        extension(IdedBat idedBat)
        {
            /// <summary>
            /// Gets or sets the `ByAutoId` XML attribute as a string flag for automatic ID matching.
            /// </summary>
            /// <remarks>Setting the value to `"TRUE"` (case-insensitive) stores `true`; any other
            /// value stores `false`.</remarks>
            [XmlAttribute("ByAutoId")]
            public string ByAutoIdString
            {
                get { return idedBat.ByAutoId.ToString(); }
                set
                {
                    if (value.ToUpper() == "TRUE") { idedBat.ByAutoId = true; }
                    else { idedBat.ByAutoId = false; }
                }
            }

            /// <summary>
            /// Asynchronously gets a four-letter identification code for the current bat from existing tags, or builds
            /// one from genus and species when no valid tag is available.
            /// </summary>
            /// <remarks>Uses the first tag that is exactly four characters and already uppercase. If
            /// none is found, the code is generated from the first three characters of genus and species, then
            /// converted to uppercase.</remarks>
            /// <returns>A four-letter uppercase code, or the same code wrapped in parentheses when automatic identification is
            /// enabled.</returns>
            internal async Task<string?> Get4LetterCode()
            {
                string result = "";
                //if (idedBat.ByAutoId) { return ""; }

                var bat = await DBAccessMS.GetNamedBat(idedBat.Name);
                var tagList = await DBAccessMS.GetBatTagsAsync(bat.ID);
                string? code = (from tag in tagList ?? new List<BatTag>()
                                where (tag?.Tag?.Length ?? 0) == 4 && (tag?.Tag?.ToUpper() ?? "") == (tag?.Tag ?? " ")
                                select (tag?.Tag ?? "")).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(code))
                {
                    code = (bat.BatGenus.Substring(0, 3) + bat.BatSpecies.Substring(0, 3)).ToUpper();
                    
                }
                if (idedBat.ByAutoId) code = $"({code})";
                return code;

            }

            internal async Task<string> GetLabel()
            {
                string result = "";
                if (idedBat.ByAutoId) { return ""; }

                var bat = await DBAccessMS.GetNamedBat(idedBat.Name);

                return bat.Label;
            }
        }
    }
}
