

using ARMCommon.Helpers;
using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net;

namespace ARMCommon.ActionFilter
{

    public class ApiResponseFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var constantsResult = new Constants_Result();
            ARMResult armResult;
            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value is ARMResult result)
                {
                    armResult = (ARMResult)objectResult.Value;
                    string message = result.result["message"].ToString();
                    if (result.result.ContainsKey("messagetype") && result.result["messagetype"]?.ToString() == "Custom")
                    {
                        armResult.result["message"] = message;
                    }
                    else
                    {
                        armResult.result["message"] = constantsResult.result[message].ToString();
                    }
                }
                else
                {
                    armResult = new ARMResult();
                    armResult.result.Add("message", constantsResult.result[objectResult.Value.ToString()].ToString());
                }

                // Check the HTTP status code of the response
                if (objectResult.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    armResult.result.Add("success", false);
                }
                else if (objectResult.StatusCode == (int)HttpStatusCode.OK)
                {
                    armResult.result.Add("success", true);

                }

                objectResult.Value = JsonConvert.SerializeObject(armResult, new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.Default
                });
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Do nothing
        }
    }


}
