using Wallet.Domain;

namespace Wallet.Application;

public class ServiceResponse<T>
{
    public T Data { get; set; }
    
    public string Message { get; set; }
    
    public bool IsSuccess { get; set; }
    
    public string ErrorCode { get; set; }
    

    public static ServiceResponse<T> Success(T data, string message = "")
    {
        return new ServiceResponse<T> { Data = data, IsSuccess = true, Message = message };
    }

    public static ServiceResponse<T> Error(string errorMessage, string errorCode = "")
    {
        return new ServiceResponse<T>
        {
            IsSuccess = false, Message = errorMessage, ErrorCode = errorCode, Data = default
        };
    }
}
