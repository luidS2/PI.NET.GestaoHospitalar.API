using PI.GestaoHospitalar.Core.ExceptionCore;
using PI.GestaoHospitalar.Core.Kafka;
using Confluent.Kafka;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.DiagnosticSource;
using Microsoft.AspNetCore.Http;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Middleware
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _nameBackEnd = "";
        private readonly string _servidorKafka = "";
        private readonly string _topicoLogs = "";
        private readonly string _localMachineName = "";

        public LogMiddleware(RequestDelegate next, string nameBackEnd, string servidorKafka, string topicoLogs, string localMachineName)
        {
            _next = next;
            _nameBackEnd = nameBackEnd;
            _servidorKafka = servidorKafka;
            _topicoLogs = topicoLogs;
            _localMachineName = localMachineName;
        }

        public async Task Invoke(HttpContext context)
        {
            using MemoryStream requestBodyStream = new MemoryStream();
            using MemoryStream responseBodyStream = new MemoryStream();
            Stream originalRequestBody = context.Request.Body;
            Stream originalResponseBody = context.Response.Body;
            var requestTime = DateTime.Now;
            string responseBody = "";
            string requestBodyText = "";
            string absoluteUri = "";
            var request = context.Request;
            var response = context.Response;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                await context.Request.Body.CopyToAsync(requestBodyStream);
                requestBodyStream.Seek(0, SeekOrigin.Begin);

                requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();

                requestBodyStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = requestBodyStream;

                context.Response.Body = responseBodyStream;

                absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), request.PathBase.ToUriComponent(), request.Path.ToUriComponent(), request.QueryString.ToUriComponent());

                Agent.Subscribe(new HttpDiagnosticsSubscriber());

                await Agent.Tracer.CaptureTransaction(_nameBackEnd, "Request", async () =>
                {
                    watch = Stopwatch.StartNew();
                    await _next(context);
                    watch.Stop();
                });

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                responseBody = new StreamReader(responseBodyStream).ReadToEnd();

                await SafeLog(absoluteUri,
                    requestTime,
                    watch.ElapsedMilliseconds,
                    response.StatusCode,
                    request.Method,
                    request.Path,
                    request.QueryString.ToString(),
                    requestBodyText,
                    responseBody);

                responseBodyStream.Seek(0, SeekOrigin.Begin);

                await responseBodyStream.CopyToAsync(originalResponseBody);
            }
            catch (Exception ex)
            {
                try
                {
                    await SafeLog(absoluteUri,
                                requestTime,
                                watch.ElapsedMilliseconds,
                                500,
                                request.Method,
                                request.Path,
                                request.QueryString.ToString(),
                                requestBodyText,
                                responseBody,
                                "",
                                ex.ToDetailedString());
                }
                catch (Exception sx)
                {
                    Console.WriteLine(sx.ToDetailedString());

                }

                byte[] data = Encoding.UTF8.GetBytes(ex.ToDetailedString());
                originalResponseBody.Write(data, 0, data.Length);
            }
            finally
            {
                context.Request.Body = originalRequestBody;
                context.Response.Body = originalResponseBody;
            }
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private async Task SafeLog(string absoluteUri,
                                DateTime requestTime,
                            long responseMillis,
                            int statusCode,
                            string method,
                            string path,
                            string queryString,
                            string requestBody,
                            string responseBody,
                            string complement = "",
                            string stackTrace = "")
        {
            try
            {
                var dados = new
                {
                    NameBackEnd = _nameBackEnd,
                    LocalMachineName = _localMachineName,
                    AbsoluteUri = absoluteUri,
                    RequestTime = requestTime,
                    Latency = responseMillis,
                    StatusCode = statusCode,
                    Method = method,
                    Path = path,
                    QueryString = queryString,
                    RequestPayload = string.Format("{0} bytes", requestBody.Length),
                    ResponsePayload = string.Format("{0} bytes", responseBody.Length),
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    Complement = complement,
                    StackTrace = stackTrace
                };

                KafkaConfig KafkaConfigTopicoResposta = new KafkaConfigPersonalizado();
                KafkaConfigTopicoResposta.GrupoNome = $"PI.GestaoHospitalar.Core.LogMiddleware.Group";
                KafkaConfigTopicoResposta.Servidor = _servidorKafka;
                //KafkaConfigTopicoResposta.TimeOut = 60000;
                KafkaConfigTopicoResposta.TopicoErro = $"PI.GestaoHospitalar.Core.LogMiddleware.Errors";
                KafkaConfigTopicoResposta.TopicoNome = _topicoLogs;
                var message = JsonSerializer.Serialize(dados);
                ProducerWrapper producerWrapper = new ProducerWrapper(KafkaConfigTopicoResposta);
                await Task.Run(() => producerWrapper.WriteMessage(message));

                //var message = JsonSerializer.Serialize(dados);
                //var config = new ProducerConfig(); //{ BootstrapServers = "172.16.201.16:9092" };                
                //var producer = new ProducerBuilder<Null, string>(config).Build();
                //producer.AddBrokers(_servidorKafka);

                //var sendResult =await producer
                //                    .ProduceAsync(_topicoLogs, new Message<Null, string> { Value = message });

                //Logger logger = LogManager.GetCurrentClassLogger();

                //await Task.Run(() =>
                //{
                //    logger.Info(new
                //    {
                //        NameBackEnd = _nameBackEnd,
                //        AbsoluteUri = absoluteUri,
                //        RequestTime = requestTime,
                //        Latency = responseMillis,
                //        StatusCode = statusCode,
                //        Method = method,
                //        Path = path,
                //        QueryString = queryString,
                //        RequestPayload = string.Format("{0} bytes", requestBody.Length),
                //        ResponsePayload = string.Format("{0} bytes", responseBody.Length),
                //        RequestBody = requestBody,
                //        ResponseBody = responseBody,
                //        Complement = complement
                //    });
                //});


                //var json = JsonSerializer.Serialize(new
                //{
                //    NameBackEnd = _nameBackEnd,
                //    AbsoluteUri = absoluteUri,
                //    RequestTime = requestTime,
                //    Latency = responseMillis,
                //    StatusCode = statusCode,
                //    Method = method,
                //    Path = path,
                //    QueryString = queryString,
                //    RequestPayload = string.Format("{0} bytes", requestBody.Length),
                //    ResponsePayload = string.Format("{0} bytes", responseBody.Length),
                //    RequestBody = requestBody,
                //    ResponseBody = responseBody,
                //    Complement = complement
                //});

                //LogEventInfo eventInfo = new LogEventInfo
                //{
                //    Level = LogLevel.Info,
                //    Properties = { { "NameBackEnd", _nameBackEnd } }
                //};

                //await Task.Run(() => {
                //    logger.Info(json);
                //});
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToDetailedString());
                //throw new Exception("Problema ao Logar no servidor Kafka", ex);
                //implementar e-mail aqui
            }
        }


    }
}
