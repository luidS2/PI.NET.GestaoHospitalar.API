using PI.GestaoHospitalar.Core.ExceptionCore;
using PI.GestaoHospitalar.Core.Results;
using Elastic.Apm;
using Elastic.Apm.DiagnosticSource;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Resilience
{
    public class CircuitBreaker
    {
        private readonly int _attempts;
        private readonly int _durationOfBreack;
        private readonly string _nameBackEnd;
        public CircuitBreaker(string nameBackEnd, int attempts, int durationOfBreack)
        {
            _attempts = attempts;
            _durationOfBreack = durationOfBreack;
            _nameBackEnd = nameBackEnd;
        }

        public async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
        {
            int attempt = 0;
            Exception exception = null;
            var initialDate = DateTime.Now;
            do
            {
                try
                {
                    Agent.Subscribe(new HttpDiagnosticsSubscriber());
                    return await Agent.Tracer.CaptureTransaction(_nameBackEnd, "BackGroundService", async () =>
                    {
                        return await action();
                    });
                }
                catch (Exception ex)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(_durationOfBreack));
                    attempt++;
                    exception = ex;
                }
            } while (attempt < _attempts);
            throw new CircuitBreakerException(exception?.ToDetailedString(), 590, _attempts, _durationOfBreack, initialDate);            
        }
    }
}
