using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class ServiceResponse<T>
{
    public T Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }

    public static ServiceResponse<T> CreateSuccess(T data, string message)
    {
        return new ServiceResponse<T> { Data = data, Success = true, Message = message };
    }

    public static ServiceResponse<T> CreateFailure(string message)
    {
        return new ServiceResponse<T> { Data = default, Success = false, Message = message };
    }
}
}