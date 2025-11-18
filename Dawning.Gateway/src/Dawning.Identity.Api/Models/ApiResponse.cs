namespace Dawning.Identity.Api.Models
{
    /// <summary>
    /// 统一API响应格式
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 业务状态码
        /// </summary>
        public int Code { get; set; } = 20000;

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int Status { get; set; } = 200;

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Msg { get; set; } = "Success";

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
                Status = 200,
                Msg = message,
                Data = data
            };
        }

        /// <summary>
        /// 错误响应
        /// </summary>
        public static ApiResponse<T> Error(int code, string message, T data = default)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Status = 400,
                Msg = message,
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
                Status = 200,
                Msg = message,
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
                Status = 400,
                Msg = message,
                Data = null
            };
        }
    }
}
