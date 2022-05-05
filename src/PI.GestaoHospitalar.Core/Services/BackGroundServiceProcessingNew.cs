using PI.GestaoHospitalar.Core.Helpers;
using PI.GestaoHospitalar.Core.Kafka;
using PI.GestaoHospitalar.Core.Results;
using PI.GestaoHospitalar.Core.Util;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.CircuitBreaker;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Services
{
    /// <summary>
    /// Classe Para processamento em BackGround
    /// </summary>
    public abstract class BackGroundServiceProcessingNew : BackgroundService
    {
        protected Task _task;
        protected CancellationTokenSource _cancellationTokens = new CancellationTokenSource();
        protected string _baseUrl = "";
        protected int _circuitBreakerTentativas = 1;
        protected int _circuitBreakerTempoEspera = 1000;
        protected int _circuitBreakerTempoReabertura = 2000;
        private readonly string _nameBackEnd = "";
        protected readonly IMediator _mediator;
        private readonly string _localMachine = "";

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="iConfig"></param>
        public BackGroundServiceProcessingNew(IConfiguration iConfig, string nameBackEnd, string localMachine)
        {
            _baseUrl = iConfig.GetSection("UrlAPI").Value;

            // Validação do valor passado no circuitBreaker pois o valor deve ser maior que zero
            // Busca o valor do parâmetro forçando ele a ter o valor defult caso venha nulo
            int circuitBreakerTentativas = Convert.ToInt32(iConfig.GetSection("CircuitBreaker:Tentativas").Value);
            _circuitBreakerTentativas = (circuitBreakerTentativas == 0 ? 1 : circuitBreakerTentativas);

            // Busca o valor do parâmetro forçando ele a ter o valor defult caso venha nulo
            int circuitBreakerTempoEspera = Convert.ToInt32(iConfig.GetSection("CircuitBreaker:TempoEspera").Value);
            _circuitBreakerTempoEspera = (circuitBreakerTempoEspera == 0 ? 1000 : circuitBreakerTempoEspera);

            // Busca o valor do parâmetro forçando ele a ter o valor defult caso venha nulo
            int circuitBreakerTempoReabertura = Convert.ToInt32(iConfig.GetSection("CircuitBreaker:TempoReabertura").Value);
            _circuitBreakerTempoReabertura = (circuitBreakerTempoReabertura == 0 ? 2000 : circuitBreakerTempoReabertura);

            _nameBackEnd = nameBackEnd;
            _localMachine = localMachine;

        }
        public BackGroundServiceProcessingNew(IConfiguration iConfig, string nameBackEnd, IMediator mediator, string localMachine) : this(iConfig, nameBackEnd, localMachine)
        {
            _mediator = mediator;
        }
        public bool StatusServico()
        {
            //Uri baseAddress = novo Uri(_httpContextAccessor.HttpContext.Request.ur .Url.GetLeftPart(UriPartial.Authority));
            return !_cancellationTokens.IsCancellationRequested;
        }
        public async Task IniciarServico()
        {
            _cancellationTokens = new CancellationTokenSource();
            await this.StartAsync(_cancellationTokens.Token);
        }
        public async Task PararServico()
        {
            await this.StopAsync(_cancellationTokens.Token);
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            this._task = this.ExecuteAsync(this._cancellationTokens.Token);

            return this._task.IsCompleted ? this._task : Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (this._task != null)
            {
                //try
                //{
                this._cancellationTokens.Cancel();
                return this._task.IsCompleted ? this._task : Task.CompletedTask;
                //}
                //finally
                //{
                //    await Task.WhenAny(_task, Task.Delay(Timeout.Infinite, cancellationToken));
                //}
            }
            else
            {
                return base.StopAsync(cancellationToken);
            }
            //return Task.CompletedTask;
        }
        protected async Task<string> BuscarMensagem(KafkaConfig kafkaConfig)
        {
            try
            {
                var consumerWrapper = new ConsumerWrapperNew(kafkaConfig);
                return await consumerWrapper.ReadMessage();
            }
            catch
            {
                return "";
            }
        }

        protected async Task<string> BuscarMensagem(KafkaConfig kafkaConfig, string idAssinante)
        {
            try
            {
                var consumerWrapper = new ConsumerWrapperNew(kafkaConfig, idAssinante);
                return await consumerWrapper.ReadMessage();
            }
            catch
            {
                return "";
            }
        }

        private async Task<IList<string>> GetMessageKafka(ConsumerWrapperNew consumerWrapper, int timeOut)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            IList<string> mensagens = new List<string>();
            do
            {
                var mensagem = await consumerWrapper.ReadMessage();
                if (!string.IsNullOrEmpty(mensagem))
                    mensagens.Add(mensagem);
                else
                    break;

            } while (stopWatch.Elapsed.TotalSeconds <= timeOut);
            stopWatch.Stop();

            return mensagens;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kafkaConfig">Configuração do kafka</param>
        /// <param name="recurso">Endpoint a ser acionado</param>
        /// <param name="timeOut">timeOut em segundos</param>
        /// <param name="stoppingToken">CancellationToken</param>
        /// <returns></returns>
        public virtual async Task ProcessarBaseAsync<TDto>(KafkaConfig kafkaConfig, string recurso, int timeOut, CancellationToken stoppingToken)
            where TDto : class
        {
            while (!stoppingToken.IsCancellationRequested)
            {                
                await ProcessarBaseAsync<TDto>(kafkaConfig, recurso, timeOut, request => ProcessarAsync(kafkaConfig, recurso, request));                
            }
        }

        public virtual async Task ProcessarBaseAsync<TCommand, TDto>(KafkaConfig kafkaConfig, int timeOut, CancellationToken stoppingToken)
            where TDto : class
            where TCommand : class
        {
            try
            {
                _ = _mediator ?? throw new ArgumentException("Parameter cannot be null", nameof(_mediator));

                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessarBaseAsync<TDto>(kafkaConfig, "", timeOut, request => ProcessarAsync<TCommand, TDto>(kafkaConfig, request));
                }
            }
            catch (Exception ex)
            {
                await SafeLog.WriteLogMessage(kafkaConfig, _nameBackEnd, ex);
            }
        }

        private async Task ProcessarBaseAsync<TDto>(KafkaConfig kafkaConfig, string recurso, int timeOut, Func<object, Task<IResult>> processar)
            where TDto : class
        {
            try
            {
                using var consumerWrapper = new ConsumerWrapperNew(kafkaConfig);
                IList<string> mensagensKafka = null;
                mensagensKafka = await GetMessageKafka(consumerWrapper, timeOut);
                IResult result = null;
                if (mensagensKafka.Any())
                {
                    var stopWatch = Stopwatch.StartNew();
                    var requestTime = DateTime.Now;
                    var request = await ProcessarMensagem<TDto>(mensagensKafka, timeOut);

                    result = await processar(request);

                    stopWatch.Stop();

                    await ProcessarRetorno(kafkaConfig, recurso, request, mensagensKafka, requestTime, stopWatch.ElapsedMilliseconds, result);
                }
            }
            catch (Exception ex)
            {
                await SafeLog.WriteLogMessage(kafkaConfig, _nameBackEnd, ex);
            }
            finally
            {
                GC.Collect(2, GCCollectionMode.Default, true, true);
            }
        }        

        private async Task ProcessarRetorno(KafkaConfig kafkaConfig, string recurso, object request, IList<string> mensagensKafka, DateTime requestTime, long latency, IResult result)
        {
            KafkaDados dados = new KafkaDados
            {
                AbsoluteUri = $@"{_baseUrl}/{recurso}",
                NameBackEnd = _nameBackEnd,
                ResponseBody = await result.Parameters.SerializeAsync(),
                //RequestBody = await request.SerializeAsync(),
                StatusCode = result.StatusCode,
                RequestTime = requestTime,
                Latency = latency,
                Method = "POST",
                LocalMachineName = _localMachine
            };
            await SafeLog.WriteLogMessage(kafkaConfig, dados);
            await ProcessarClasse(kafkaConfig, recurso, mensagensKafka, result);
        }

        /// <summary>
        /// Processar status code
        /// </summary>
        /// <param name="kafkaConfig">Configuração do Kafka</param>
        /// <param name="recurso">EndPoint a ser acionado</param>
        /// <param name="mensagens">Mensagem do Kafka</param>
        /// <param name="result">Resultado da chamada</param>
        /// <returns></returns>
        private async Task ProcessarClasse(KafkaConfig kafkaConfig, string recurso, IList<string> mensagens, IResult result)
        {
            if (result.StatusCode < 200 || result.StatusCode >= 300)
            {
                if (result.StatusCode >= 400 && result.StatusCode < 500)
                {
                    await ProcessarClasse400(recurso, kafkaConfig.TopicoNome, mensagens, result);
                }
                else
                {
                    await ProcessarClasse500(recurso, kafkaConfig.TopicoNome, mensagens, result);
                }
            }
            else
            {
                await ProcessarClasse200(recurso, kafkaConfig.TopicoNome, mensagens, result);
            }
        }

        /// <summary>
        /// Processar status codes da classe 500
        /// </summary>
        /// <param name="recurso">Recurso do endpoint chamado</param>
        /// <param name="result">Resultado da requisição HTTP</param>
        /// <returns></returns>
        public virtual Task ProcessarClasse500(string recurso, string topicoNome, IList<string> mensagem, IResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processar status codes da classe 400
        /// </summary>
        /// <param name="recurso">Recurso do endpoint chamado</param>
        /// <param name="result">Resultado da requisição HTTP</param>
        /// <returns></returns>
        public virtual Task ProcessarClasse400(string recurso, string topicoNome, IList<string> mensagem, IResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processar status codes da classe 200
        /// </summary>
        /// <param name="recurso">Recurso do endpoint chamado</param>
        /// <param name="result">Resultado da requisição HTTP</param>
        /// <returns></returns>
        public virtual Task ProcessarClasse200(string recurso, string topicoNome, IList<string> mensagem, IResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processar mensagens do Kafka
        /// </summary>
        /// <param name="mensagem">Mensagem recebida do Kafka</param>
        /// <returns></returns>
        public async virtual Task<IList<TDto>> ProcessarMensagem<TDto>(IList<string> mensagens, bool isCdc)
            where TDto : class
        {
            IList<TDto> list = new List<TDto>();

            foreach (var mensagem in mensagens)
            {
                if (isCdc)
                {
                    list.Add(await ConverterMensagemCdc<TDto>(mensagem));
                }
                else
                {
                    list.Add(await ConverterMensagem<TDto>(mensagem));
                }

            }

            return list;
        }

        public async virtual Task<TDto> ProcessarMensagem<TDto>(string mensagem, bool isCdc)
            where TDto : class
        {
            if (isCdc)
            {
                return await ConverterMensagemCdc<TDto>(mensagem);
            }
            else
            {
                return await ConverterMensagem<TDto>(mensagem);
            }
        }

        public virtual async Task<object> ProcessarMensagem<TDto>(IList<string> mensagensKafka, int timeOut)
            where TDto : class
        {
            var isCdc = mensagensKafka.Any(s => s.Contains("\"payload\":{"));
            if (timeOut == 0)
            {
                return await ProcessarMensagem<TDto>(mensagensKafka.FirstOrDefault(), isCdc);
            }
            else
            {
                return await ProcessarMensagem<TDto>(mensagensKafka, isCdc);
            }
        }

        public virtual async Task<IResult> ProcessarAsync(KafkaConfig kafkaConfig, string recurso, object dtos)
        {
            var result = new Result(500, "Serviço indisponível.", "Serviço indisponível.");

            var client = new RestClient($@"{_baseUrl}/");
            var request = new RestRequest(recurso, Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
            var mensagem = JsonHelper.Serialize(dtos);
            request.AddJsonBody(mensagem);
            IRestResponse response = await client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == 0) //Status Code 500 Ou Status Code 0
            {
                result = new Result(500, response.Content, response.Content);
                //throw new WebException(mensagem);
            }
            else if (response.IsSuccessful)
            {
                result = new Result(Convert.ToInt32(response.StatusCode), "", response.Request.Body);
            }
            else if (Convert.ToInt32(response.StatusCode) >= 400 && Convert.ToInt32(response.StatusCode) < 500)
            {
                result = new Result(Convert.ToInt32(response.StatusCode), "", response.Request.Body);
            }

            return result;
        }

        public virtual async Task<IResult> ProcessarAsync<TCommand, TDto>(KafkaConfig kafkaConfig, object dtos)
            where TCommand : class
        {
            var result = await _mediator.Send((TCommand)Activator.CreateInstance(typeof(TCommand), dtos)) as IResult;
            return result;
        }

        public virtual async Task<TDto> ConverterMensagemCdc<TDto>(string mensagem)
            where TDto : class
        {
            string payload;
            var op = await mensagem.SelectTokenAsync("payload.op");
            if (op == "d")
            {
                payload = await mensagem.SelectTokenAsync("payload.before");
            }
            else
            {
                payload = await mensagem.SelectTokenAsync("payload.after");
            }

            payload = await payload.AddNewPropertyAsync("op", op);

            return await payload.DeserializeAsync<TDto>();
        }

        public virtual async Task<TDto> ConverterMensagem<TDto>(string mensagem)
            where TDto : class
        {
            return await mensagem.DeserializeAsync<TDto>();
        }

        protected async Task<IResult> ProcessarCircuitBreakerAsync(KafkaConfig kafkaConfig, string recurso, string mensagem,
            int circuitBreakerTentativas = 1, int circuitBreakerTempoEspera = 1000,
            int circuitBreakerTempoReabertura = 2000)
        {
            var retry = Policy.Handle<WebException>()
                .WaitAndRetryForever(attemp => TimeSpan.FromMilliseconds(circuitBreakerTempoEspera));

            var circuitBreaker = Policy.Handle<WebException>()
                .CircuitBreaker(circuitBreakerTentativas, TimeSpan.FromMilliseconds(circuitBreakerTempoReabertura),
                onBreak: null
                , onReset: (context) =>
                 {
                     Console.WriteLine($"Circuito saiu do estado de falha { DateTime.Now.ToString() }");
                 });

            var result = new Result(500, "Serviço indisponível.", "Serviço indisponível.");

            await retry.Execute(async () =>
            {
                if (circuitBreaker.CircuitState != CircuitState.Open)
                {
                    await circuitBreaker.Execute(async () =>
                    {
                        var client = new RestClient($@"{_baseUrl}/");
                        var request = new RestRequest(recurso, Method.POST)
                        {
                            RequestFormat = DataFormat.Json
                        };
                        request.AddJsonBody(mensagem);
                        IRestResponse response = await client.ExecuteAsync(request);

                        if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == 0) //Status Code 500 Ou Status Code 0
                        {
                            result = new Result(500, response.Content, response.Content);
                            //throw new WebException(mensagem);
                        }
                        else if (response.IsSuccessful)
                        {
                            result = new Result(Convert.ToInt32(response.StatusCode), "", response.Request.Body);
                        }
                        else if (Convert.ToInt32(response.StatusCode) >= 400 && Convert.ToInt32(response.StatusCode) < 500)
                        {
                            result = new Result(Convert.ToInt32(response.StatusCode), "", response.Request.Body);
                        }
                    });
                }
            });

            return result;
        }
    }
}
