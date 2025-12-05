using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawning.Identity.Domain.Models
{
    public class PagedData<T> where T : class, new()
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

    /// <summary>
    /// 游标分页数据（用于大数据集场景）
    /// </summary>
    public class CursorPagedData<T> where T : class, new()
    {
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage { get; init; }

        /// <summary>
        /// 下一页的游标值
        /// </summary>
        public object? NextCursor { get; init; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public IEnumerable<T> Items { get; init; } = new List<T>();
    }
}
