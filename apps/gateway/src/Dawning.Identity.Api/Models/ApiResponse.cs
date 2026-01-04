using System.Collections.Generic;

namespace Dawning.Identity.Api.Models
{
    /// <summary>
    /// Unified API response format
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Business status code (20000 indicates success)
        /// </summary>
        public int Code { get; set; } = 20000;

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = "Success";

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Success response
        /// </summary>
        public static ApiResponse<T> Success(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Code = 20000,
                Message = message,
                Data = data,
            };
        }

        /// <summary>
        /// Success response (with pagination) - Returns data structure with pagination info
        /// </summary>
        public static ApiResponse<object> SuccessPaged<TItem>(
            IEnumerable<TItem> items,
            int current,
            int pageSize,
            long total,
            string message = "Success"
        )
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
                        total,
                    },
                },
            };
        }

        /// <summary>
        /// Error response
        /// </summary>
        public static ApiResponse<T> Error(int code, string message, T? data = default)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = data,
            };
        }
    }

    /// <summary>
    /// API response without data
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// Generates an ApiResponse object representing success, without data.
        /// </summary>
        /// <param name="message">Response message, default is "Success"</param>
        /// <returns>Returns an ApiResponse instance representing success</returns>
        public static ApiResponse Success(string message = "Success")
        {
            return new ApiResponse
            {
                Code = 20000,
                Message = message,
                Data = null,
            };
        }

        /// <summary>
        /// Generates an ApiResponse object representing error, without data.
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="message">Error message</param>
        /// <returns>Returns an ApiResponse instance representing error</returns>
        public static ApiResponse Error(int code, string message)
        {
            return new ApiResponse
            {
                Code = code,
                Message = message,
                Data = null,
            };
        }
    }
}
