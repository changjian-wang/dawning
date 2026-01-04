using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models
{
    public class PagedData<T>
        where T : class, new()
    {
        /// <summary>
        /// Current page index
        /// </summary>
        public int PageIndex { get; init; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// Total number of records
        /// </summary>
        public long TotalCount { get; init; }

        /// <summary>
        /// Data list
        /// </summary>
        public IEnumerable<T> Items { get; init; } = new List<T>();
    }
}
