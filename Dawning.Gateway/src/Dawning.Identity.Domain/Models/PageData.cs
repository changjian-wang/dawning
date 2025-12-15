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
        /// 当前页码
        /// </summary>
        public int PageIndex { get; init; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalCount { get; init; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public IEnumerable<T> Items { get; init; } = new List<T>();
    }
}
