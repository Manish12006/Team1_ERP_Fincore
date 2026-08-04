using Fincore.Application.DTO;

namespace Fincore.Infrastructure.CommonHelper
{
    public class ApiResponseHelper
    {
        public static ApiResponse<T> SuccessRes<T>(
            T data,
            string message = "Success",
            int? totalRecord = null,
            object? metadata = null)
        {
            return new ApiResponse<T>
            {
                success = true,
                message = message,
                data = data,
                metadata = metadata,
                totalNumberRecord = totalRecord
            };
        }

        public static ApiResponse<T> Failure<T>(
            string message,
            string errorCode,
            string details)
        {
            return new ApiResponse<T>
            {
                success = false,
                message = message,
                error = new ApiError
                {
                    Code = errorCode,
                    Details = details
                }
            };
        }

        internal static ApiResponse<T> Failure<T>(string v)
        {
            throw new NotImplementedException();
        }
    }
}