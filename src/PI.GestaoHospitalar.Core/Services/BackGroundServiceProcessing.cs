using PI.GestaoHospitalar.Core.ExceptionCore;
using PI.GestaoHospitalar.Core.Extensions.Tasks;
using PI.GestaoHospitalar.Core.Helpers;
using PI.GestaoHospitalar.Core.Kafka;
using PI.GestaoHospitalar.Core.Resilience;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Services
{
    /// <summary>
    /// Classe Para processamento em BackGround
    /// </summary>
    public abstract class BackGroundServiceProcessing : BackgroundService
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
        private readonly CircuitBreaker _circuitBreaker;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="iConfig"></param>
        public BackGroundServiceProcessing(IConfiguration iConfig, string nameBackEnd, string localMachine)
        {
            _baseUrl = iConfig.GetSection("UrlAPI").Value;

            // Validação do valor passado no circuitBreaker pois o valor deve ser maior que zero
            // Busca o valor do parâmetro forçando ele a ter o valor defult caso venha nulo            
            int.TryParse(iConfig.GetSection("CircuitBreaker:Tentativas").Value, out int circuitBreakerTentativas);
            _circuitBreakerTentativas = circuitBreakerTentativas == 0 ? 5 : circuitBreakerTentativas;

            // Busca o valor do parâmetro forçando ele a ter o valor defult caso venha nulo
            int.TryParse(iConfig.GetSection("CircuitBreaker:TempoEspera").Value, out int circuitBreakerTempoEspera);
            _circuitBreakerTempoEspera = circuitBreakerTempoEspera == 0 ? 20000 : circuitBreakerTempoEspera;

            // Busca o valor do parâmetro forçando ele a ter o valor defult caso venha nulo            
            int.TryParse(iConfig.GetSection("CircuitBreaker:TempoReabertura").Value, out int circuitBreakerTempoReabertura);
            _circuitBreakerTempoReabertura = circuitBreakerTempoReabertura == 0 ? 2000 : circuitBreakerTempoReabertura;

            _nameBackEnd = nameBackEnd;
            _localMachine = localMachine;

            _circuitBreaker = new CircuitBreaker(nameBackEnd,_circuitBreakerTentativas, _circuitBreakerTempoEspera);

        }
        public BackGroundServiceProcessing(IConfiguration iConfig, string nameBackEnd, IMediator mediator, string localMachine) : this(iConfig, nameBackEnd, localMachine)
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

        private async Task<IList<string>> GetMessageKafka(ConsumerWrapper consumerWrapper, int timeOut)
        {
            Stopwatch stopWatch = new Stopwatch();
            IList<string> mensagens = new List<string>();
            if (timeOut > 0)
            {
                do
                {
                    var mensagem = await consumerWrapper.ReadMessage();
                    if (!string.IsNullOrEmpty(mensagem))
                    {
                        mensagens.Add(mensagem);
                        if (mensagens.Count == 1)
                        {
                            stopWatch.Start();
                        }
                    }

                } while (stopWatch.Elapsed.TotalSeconds <= timeOut);
                stopWatch.Stop();
            }
            else
            {
                var mensagem = await consumerWrapper.ReadMessage();
                if (!string.IsNullOrEmpty(mensagem))
                {
                    mensagens.Add(mensagem);
                }
            }

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
            using var consumerWrapper = new ConsumerWrapper(kafkaConfig);
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessarBaseAsync<TDto>(consumerWrapper, kafkaConfig, recurso, timeOut, request => ProcessarAsync(kafkaConfig, recurso, request));
            }
        }

        public virtual async Task ProcessarBaseAsync<TCommand, TDto>(KafkaConfig kafkaConfig, int timeOut, CancellationToken stoppingToken)
            where TDto : class
            where TCommand : class
        {
            try
            {
                _ = _mediator ?? throw new ArgumentException("Parameter cannot be null", nameof(_mediator));

                using var consumerWrapper = new ConsumerWrapper(kafkaConfig);
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessarBaseAsync<TDto>(consumerWrapper, kafkaConfig, "", timeOut, request => ProcessarAsync<TCommand, TDto>(kafkaConfig, request));
                }
            }
            catch (Exception ex)
            {
                await SafeLog.WriteLogMessage(kafkaConfig, _nameBackEnd, _localMachine, ex);
            }
        }

        private async Task ProcessarBaseAsync<TDto>(ConsumerWrapper consumerWrapper, KafkaConfig kafkaConfig, string recurso, int timeOut, Func<object, Task<IResult>> processar)
            where TDto : class
        {
            IList<string> mensagensKafka = null;
            IResult result = null;
            var requestTime = DateTime.Now;
            var stopWatch = Stopwatch.StartNew();
            try
            {
                mensagensKafka = await GetMessageKafka(consumerWrapper, timeOut);
                if (mensagensKafka.Any())
                {
                    stopWatch = Stopwatch.StartNew();
                    requestTime = DateTime.Now;
                    var request = await ProcessarMensagem<TDto>(mensagensKafka, timeOut);

                    result = await _circuitBreaker.ExecuteAsync(() =>
                    {
                        return ProcessarClasse(kafkaConfig, recurso, mensagensKafka, processar(request));
                    });

                    await ProcessarRetorno(kafkaConfig, recurso, mensagensKafka, requestTime, stopWatch.ElapsedMilliseconds, result);
                }
            }
            catch (CircuitBreakerException ex)
            {
                result = new Result(ex.StatusCode, $"Falha no Circuite Break número de tentativas: {ex.Attempts} - Duração entre as tentativas: {TimeSpan.FromMilliseconds(ex.DurationOfBreack).Seconds}s Data Hora de início:{ex.InitialDate}", null);
                await ProcessarErro(kafkaConfig, recurso, result, mensagensKafka, requestTime, stopWatch.ElapsedMilliseconds, ex);
                await ProcessarClasse500(recurso, kafkaConfig, mensagensKafka, result);
            }
            catch (Exception ex)
            {
                await ProcessarErro(kafkaConfig, recurso, result, mensagensKafka, requestTime, stopWatch.ElapsedMilliseconds, ex);
            }
            finally
            {
                GC.Collect(2, GCCollectionMode.Default, true, true);
            }
        }

        private async Task<IResult> ProcessarClasse(KafkaConfig kafkaConfig, string recurso, IList<string> mensagens, Task<IResult> request)
        {
            var result = await request;

            await ProcessarClasse(kafkaConfig, recurso, mensagens, result);

            return result;
        }

        public async Task ProcessarErro(KafkaConfig kafkaConfig, string recurso, IResult result, IList<string> mensagensKafka, DateTime requestTime, long latency, string stackTrace)
        {
            if (mensagensKafka != null && mensagensKafka.Count > 0)
            {
                await mensagensKafka.ParallelForEachAsync(async item =>
                {
                    await ProcessarErro(kafkaConfig, recurso, result, item, requestTime, latency, stackTrace);
                }, 20);
            }
            else
            {
                await ProcessarErro(kafkaConfig, recurso, result, "", requestTime, latency, stackTrace);
            }
        }

        public async Task ProcessarErro(KafkaConfig kafkaConfig, string recurso, IResult result, string mensagemKafka, DateTime requestTime, long latency, string stackTrace)
        {
            try
            {
                string responseBody = "";
                int statusCode = 555;
                try
                {
                    if (result != null)
                    {
                        statusCode = result.StatusCode;
                        if (result.Parameters != null)
                        {
                            responseBody = JsonSerializer.Serialize(result.Parameters);
                        }
                        else
                        {
                            responseBody = result.Message;
                        }
                        
                    }
                }
                catch { }
                KafkaDados dados = new KafkaDados
                {
                    AbsoluteUri = $@"{_baseUrl}/{recurso}",
                    NameBackEnd = _nameBackEnd,
                    ResponseBody = responseBody,
                    RequestBody = mensagemKafka,
                    RequestTime = requestTime,
                    Latency = latency,
                    Method = "POST",
                    LocalMachineName = _localMachine,
                    StatusCode = statusCode,
                    StackTrace = stackTrace,
                    TopicName = kafkaConfig.TopicoNome,
                    GroupName = kafkaConfig.GrupoNome
                };

                await Task.WhenAll(new Task[]
                {
                    Task.Run(async ()=>
                    {
                        if (!string.IsNullOrEmpty(mensagemKafka))
                        {
                            await GravaTopicoErro(kafkaConfig, dados);
                        }
                    }),
                    Task.Run(async ()=>
                    {
                        await GravaTopicoLogs(kafkaConfig, dados);
                    }),
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToDetailedString());
            }
        }

        private async Task GravaTopicoLogs(KafkaConfig kafkaConfig, KafkaDados dados)
        {
            try
            {
                dados.StatusCode = 555;
                await SafeLog.WriteLogMessage(kafkaConfig, dados);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToDetailedString());
            }
        }

        private async Task GravaTopicoErro(KafkaConfig kafkaConfig, KafkaDados dados)
        {
            try
            {
                await Task.WhenAll(new Task[]
                {
                    Task.Run(async ()=>
                    {
                        await SafeLog.WriteMessage(kafkaConfig, await JsonHelper.SerializeAsync(dados), kafkaConfig.TopicoErro);
                    }),
                    Task.Run(async ()=>
                    {
                       await SafeLog.WriteMessage(kafkaConfig, await JsonHelper.SerializeAsync(dados), "PI.GestaoHospitalar.Core.erros");
                    }),
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToDetailedString());
            }
        }

        private async Task ProcessarErro(KafkaConfig kafkaConfig, string recurso, IResult result, IList<string> mensagensKafka, DateTime requestTime, long latency, Exception ex)
        {
            await ProcessarErro(kafkaConfig, recurso, result, mensagensKafka, requestTime, latency, ex.ToDetailedString());
        }

        public async Task ProcessarRetorno(KafkaConfig kafkaConfig, string recurso, IList<string> mensagensKafka, DateTime requestTime, long latency, IResult result)
        {
            string responseBody = "";
            try
            {
                responseBody = JsonSerializer.Serialize(result.Parameters);
            }
            catch { }

            if (mensagensKafka != null && mensagensKafka.Count > 0)
            {
                await mensagensKafka.ParallelForEachAsync(async item =>
                {
                    try
                    {
                        KafkaDados dados = new KafkaDados
                        {
                            AbsoluteUri = $@"{_baseUrl}/{recurso}",
                            NameBackEnd = _nameBackEnd,
                            ResponseBody = responseBody,
                            RequestBody = item,
                            RequestTime = requestTime,
                            Latency = latency,
                            Method = "POST",
                            LocalMachineName = _localMachine,
                            StatusCode = result.StatusCode,
                            TopicName = kafkaConfig.TopicoNome,
                            GroupName = kafkaConfig.GrupoNome,
                            StackTrace = result.Message
                        };
                        await SafeLog.WriteLogMessage(kafkaConfig, dados);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToDetailedString());
                    }
                }, 20);
            }
            else
            {
                KafkaDados dados = new KafkaDados
                {
                    AbsoluteUri = $@"{_baseUrl}/{recurso}",
                    NameBackEnd = _nameBackEnd,
                    ResponseBody = responseBody,
                    RequestBody = "",
                    RequestTime = requestTime,
                    Latency = latency,
                    Method = "POST",
                    LocalMachineName = _localMachine,
                    StatusCode = result.StatusCode,
                    TopicName = kafkaConfig.TopicoNome,
                    GroupName = kafkaConfig.GrupoNome,
                    StackTrace = result.Message
                };
                await SafeLog.WriteLogMessage(kafkaConfig, dados);
            }
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
                    await ProcessarErro(kafkaConfig, recurso, result, mensagens, DateTime.Now, 0, result.Message);
                    await ProcessarClasse400(recurso, kafkaConfig, mensagens, result);
                }
                else
                {
                    throw new Exception(result.Message, new Exception(JsonSerializer.Serialize(result.Parameters)));
                }
            }
            else
            {
                await ProcessarClasse200(recurso, kafkaConfig, mensagens, result);
            }
        }

        /// <summary>
        /// Processar status codes da classe 500
        /// </summary>
        /// <param name="recurso">Recurso do endpoint chamado</param>
        /// <param name="result">Resultado da requisição HTTP</param>
        /// <returns></returns>
        public virtual Task ProcessarClasse500(string recurso, KafkaConfig kafkaConfig, IList<string> mensagem, IResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processar status codes da classe 400
        /// </summary>
        /// <param name="recurso">Recurso do endpoint chamado</param>
        /// <param name="result">Resultado da requisição HTTP</param>
        /// <returns></returns>
        public virtual Task ProcessarClasse400(string recurso, KafkaConfig kafkaConfig, IList<string> mensagem, IResult result)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processar status codes da classe 200
        /// </summary>
        /// <param name="recurso">Recurso do endpoint chamado</param>
        /// <param name="result">Resultado da requisição HTTP</param>
        /// <returns></returns>
        public virtual Task ProcessarClasse200(string recurso, KafkaConfig kafkaConfig, IList<string> mensagem, IResult result)
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
            if (!isCdc)
                isCdc = mensagensKafka.Any(s => s.Contains("\\\"source\\\":{"));
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
            string token = "";
            if (mensagem.Contains("\"payload\":{"))
                token = "payload.";

            var op = await mensagem.SelectTokenAsync($"{token}op");

            if (op == "d")
            {
                payload = await mensagem.SelectTokenAsync($"{token}before");
            }
            else
            {
                payload = await mensagem.SelectTokenAsync($"{token}after");
            }

            payload = await payload.AddNewPropertyAsync("op", op);
            payload = await payload.AddNewPropertyAsync("updatedate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffff"));

            return await payload.DeserializeAsync<TDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public virtual async Task<TDto> ConverterMensagem<TDto>(string mensagem)
            where TDto : class
        {
            return await mensagem.DeserializeAsync<TDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
