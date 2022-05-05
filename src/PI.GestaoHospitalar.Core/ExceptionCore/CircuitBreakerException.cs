using System;

namespace PI.GestaoHospitalar.Core.ExceptionCore
{
    public class CircuitBreakerException : BaseException
    {
        public int Attempts { get; set; }
        public int DurationOfBreack { get; set; }
        public DateTime InitialDate { get; set; }
        public CircuitBreakerException(string message, int statusCode, int attempts, int durationOfBreack, DateTime initialDate) :
            base(message, statusCode)
        {
            Attempts = attempts;
            DurationOfBreack = durationOfBreack;
            InitialDate = initialDate;
        }
    }
}
