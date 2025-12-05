using System.Collections.Generic;

namespace Dawning.Identity.Domain.Models
{
    /// <summary>
    /// Cursor 分页数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
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
        /// 下一页游标（最后一条记录的 Timestamp）
        /// </summary>
        public long? NextCursor { get; init; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Items { get; init; } = new List<T>();
    }
}
