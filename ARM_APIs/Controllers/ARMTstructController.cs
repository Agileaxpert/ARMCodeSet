using Microsoft.AspNetCore.Mvc;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using ARM_APIs.Interface;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using ARM_APIs.Model;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Security.Cryptography;
using ARMCommon.ActionFilter;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ServiceFilter(typeof(ApiResponseFilter))]
    [ApiController]
    public class ARMTstructController : ControllerBase
    {  
        private readonly IARMTstruct _tstruct;
        public ARMTstructController( IARMTstruct tstruct)
        { 
            _tstruct = tstruct;
        }


        //[AllowAnonymous]
        //[HttpPost("ARMGetTstructSqlData")]
        //public async Task<IActionResult> ARMGetTstructSqlData(ARMAxpertConnect axpert)
        //{
        //    ARMResult result;
        //    if (string.IsNullOrEmpty(axpert.ARMSessionId) || string.IsNullOrEmpty(axpert.AxSessionId))
        //    {
        //        result = new ARMResult(false, "Required fields (Connection details) is missing in the input.");
        //        return BadRequest(JsonConvert.SerializeObject(result));
        //    }

        //    var validConnection = await _tstruct.IsValidAxpertConnection(axpert);
        //    if (validConnection)
        //    {
        //        var activeTasks = await _tstruct.GetActiveTasksList(axpert.ARMSessionId);
        //        if (activeTasks.ToString() == Constants.RESULTS.NO_RECORDS.ToString())
        //        {
        //            result = new ARMResult(false, "Process data is not available. Please try again.");
        //            return BadRequest(JsonConvert.SerializeObject(result));
        //        }

        //        result = new ARMResult();
        //        result.result.Add("success", true);
        //        result.result.Add("data", JsonConvert.SerializeObject(activeTasks));
        //        return Ok(JsonConvert.SerializeObject(result));
        //    }
        //    else
        //    {
        //        result = new ARMResult(false, "Invalid Session. Please try again.");
        //        return BadRequest(JsonConvert.SerializeObject(result));
        //    }
        //}

        [AllowAnonymous]
        [RequiredFieldsFilter("Datasources")]
        [HttpPost("ARMPrepareTstructSqls")]
        public async Task<IActionResult> ARMPrepareTstructSqls(ARMAxpertConnect axpert)
        { var validConnection = await _tstruct.IsValidAxpertConnection(axpert);
            if (validConnection)
            {
                Dictionary<string,string> results = new Dictionary<string,string>();
                foreach (var datasource in axpert.Datasources)
                {
                    var paramList = await _tstruct.GetTstructSQLQuery(datasource.Split(".")[0], datasource.Split(".")[1] , axpert.ARMSessionId);
                    results.Add(datasource, paramList.Split("~~")[1]);
                }

                ARMResult result = new ARMResult();
                result.result.Add("message", "SUCCESS");
                result.result.Add("data", JsonConvert.SerializeObject(results));
                return Ok(result);
            }
            else
            {
                return BadRequest("INVALIDSESSION");
            }
        }
    }
}
