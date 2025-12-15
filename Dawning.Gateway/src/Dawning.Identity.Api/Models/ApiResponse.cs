using System.Collections.Generic;

namespace Dawning.Identity.Api.Models
{
    /// <summary>
    /// 统一API响应格式
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 业务状态码（20000表示成功）
        /// </summary>
        public int Code { get; set; } = 20000;

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; } = "Success";

        /// <summary>
        /// 响应数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 成功响应
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Code = 20000,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 成功响应（带分页）- 返回包含分页信息的数据结构
        /// </summary>
        public static ApiResponse<object> SuccessPaged<TItem>(IEnumerable<TItem> items, int current, int pageSize, long total, string message = "Success")
        {
            return new ApiResponse<object>
            {
                Code = 20000,
                Message = message,
                Data = new
                {
                    list = items,
                    pagination = new
                    {
                        current,
                        pageSize,
                        total
                    }
                }
            };
        }

        /// <summary>
        /// 错误响应
        /// </summary>
        public static ApiResponse<T> Error(int code, string message, T? data = default)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = data
            };
        }
    }

    /// <summary>
    /// 无数据的API响应
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// 生成一个表示成功响应的ApiResponse对象，不包含数据。
        /// </summary>
        /// <param name="message">响应消息，默认为"Success"</param>
        /// <returns>返回一个表示成功的ApiResponse实例</returns>
        public static ApiResponse Success(string message = "Success")
        {
            return new ApiResponse
            {
                Code = 20000,
                Message = message,
                Data = null
            };
        }

        /// <summary>
        /// 生成一个表示错误响应的ApiResponse对象，不包含数据。
        /// </summary>
        /// <param name="code">错误代码</param>
        /// <param name="message">错误消息</param>
        /// <returns>返回一个表示错误的ApiResponse实例</returns>
        public static ApiResponse Error(int code, string message)
        {
            return new ApiResponse
            {
                Code = code,
                Message = message,
                Data = null
            };
        }
    }
}
