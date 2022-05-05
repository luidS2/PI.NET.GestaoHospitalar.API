using System;

namespace PI.GestaoHospitalar.Core.ExceptionCore
{
    public class BaseException : Exception
    {
        public int StatusCode { get; set; }

        public BaseException(string message, int statusCode) :
            base(message)
        {
            StatusCode = statusCode;
        }
    }
}
