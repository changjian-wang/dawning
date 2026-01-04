using System.Collections.Generic;

namespace Dawning.Identity.Domain.Models
{
    /// <summary>
    /// Cursor paged data
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class CursorPagedData<T>
        where T : class, new()
    {
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage { get; init; }

        /// <summary>
        /// Next page cursor (Timestamp of the last record)
        /// </summary>
        public long? NextCursor { get; init; }

        /// <summary>
        /// Data list
        /// </summary>
        public List<T> Items { get; init; } = new List<T>();
    }
}
