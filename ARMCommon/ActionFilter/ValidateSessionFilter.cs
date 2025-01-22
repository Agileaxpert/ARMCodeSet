using ARMCommon.Interface;
using ARMCommon.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ARMCommon.Filter
{

    public class ValidateSessionFilter : ActionFilterAttribute
    {
        ILogger _logger;
        private readonly IRedisHelper _redis;
        public ValidateSessionFilter(ILoggerFactory loggerFactory, IRedisHelper redis)
        {

            _logger = loggerFactory.CreateLogger<ValidateSessionFilter>();
            _redis = redis;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // do something before the action executes
            ARMResult result = new ARMResult();
            _logger.LogInformation($"Action '{context.ActionDescriptor.DisplayName}' executing");
            context.HttpContext.Request.EnableBuffering();
            context.HttpContext.Request.Body.Position = 0;
            MemoryStream requestBody = new MemoryStream();
            context.HttpContext.Request.Body.CopyToAsync(requestBody);
            requestBody.Seek(0, SeekOrigin.Begin);
            string requesttext = new StreamReader(requestBody).ReadToEnd();
            requestBody.Seek(0, SeekOrigin.Begin);
            string requestbody = requesttext;
            var jObject = JObject.Parse(requestbody);
            string sessionId = jObject["ARMSessionId"].ToString();
            if (!_redis.KeyExists(sessionId))
            {
                result.result.Add("success", false);
                result.result.Add("message", "SessionId is not valid");
                context.Result = new BadRequestObjectResult(JsonConvert.SerializeObject(result));

            }

        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
            _logger.LogInformation($"Action '{context.ActionDescriptor.DisplayName}' executed");
        }
    }
}

