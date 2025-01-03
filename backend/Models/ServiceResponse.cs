using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ServiceResponse<T> CreateSuccess(T data, string message)
        {
            return new ServiceResponse<T> { Success = true, Message = message, Data = data };
        }

        public static ServiceResponse<T> CreateFailure(string message)
        {
            return new ServiceResponse<T> { Success = false, Message = message, Data = default };
        }
    }
}