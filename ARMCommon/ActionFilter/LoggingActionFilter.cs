
using ARMCommon.Helpers;
using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace ARMCommon.ActionFilter
{
    public class LoggingActionFilter  : IAsyncResultFilter
    {
        private readonly IRabbitMQProducer _iMessageProducer;
        private readonly IConfiguration _config;

        public LoggingActionFilter(IRabbitMQProducer iMessageProducer, IConfiguration config)
        {

            _iMessageProducer = iMessageProducer;
            _config = config;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            try
            {
                 bool IsLoggingEnabled = _config.GetValue<bool>("Islogging");
                //bool IsLoggingEnabled = true;
                if (IsLoggingEnabled)
                {
                    context.HttpContext.Request.EnableBuffering();
                    context.HttpContext.Request.Body.Position = 0;
                    var originalstream = context.HttpContext.Response.Body;
                    MemoryStream requestBody = new MemoryStream();
                    await context.HttpContext.Request.Body.CopyToAsync(requestBody);
                    requestBody.Seek(0, SeekOrigin.Begin);
                    string requesttext = await new StreamReader(requestBody).ReadToEndAsync();
                    requestBody.Seek(0, SeekOrigin.Begin);

                    var tempstreame = new MemoryStream();
                    context.HttpContext.Response.Body = tempstreame;
                    await next.Invoke();
                    context.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                    string responsetext = await new StreamReader(context.HttpContext.Response.Body).ReadToEndAsync();
                    context.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                    await context.HttpContext.Response.Body.CopyToAsync(originalstream);
                    var messageObjToLog = new
                    {
                        path = context.HttpContext.Request.Path,
                        Querystringvalue = context.HttpContext.Request.QueryString.Value,
                        requestBody = requesttext,
                        ResponseBody = responsetext
                    };
               
                    _iMessageProducer.SendMessages(JsonConvert.SerializeObject(messageObjToLog), "logQueue");

                }
                else {
                    var result = await next();
                }
                
            }
            catch (Exception ex)
            {
                LogException(context, ex);
            }
        }


        private async Task LogException(ResultExecutingContext context, Exception ex)
        {
            ExceptionModel exceptionModel = new ExceptionModel();
            context.HttpContext.Request.EnableBuffering();
            context.HttpContext.Request.Body.Position = 0;
            MemoryStream requestBody = new MemoryStream();
            await context.HttpContext.Request.Body.CopyToAsync(requestBody);
            requestBody.Seek(0, SeekOrigin.Begin);
            string requesttext = await new StreamReader(requestBody).ReadToEndAsync();
            requestBody.Seek(0, SeekOrigin.Begin);

            exceptionModel.HttpMethod = context.HttpContext.Request.Method;
            exceptionModel.Requestbody = requesttext;
            exceptionModel.QueryStringValue = context.HttpContext.Request.QueryString.Value;
            exceptionModel.ExceptionMessage = ex.Message;
            exceptionModel.ExceptionStackSTrace = ex.StackTrace;
            exceptionModel.InnerExceptionMessage = ex.InnerException?.Message;
            exceptionModel.InnerExceptionStackTrace = ex.InnerException?.StackTrace;
            _iMessageProducer.SendMessage(exceptionModel, "logQueue");
            // Log.Error("Exception in Application. " + JsonConvert.SerializeObject(exceptionModel));
        }

    }
}
