using ARM_APIs.Interface;
using ARMCommon.ActionFilter;
using ARMCommon.Filter;
using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using ARM_APIs.Model;

namespace ARM_APIs.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("2")]
    [ServiceFilter(typeof(ValidateSessionFilter))]
    [ServiceFilter(typeof(ApiResponseFilter))]
    [ApiController]
    public class GetMenuControllerV2 : Controller
    {
        private readonly IARMenuV2 _menu;

        public GetMenuControllerV2(IARMenuV2 menu)
        {
            _menu = menu;
        }

        [RequiredFieldsFilter("ARMSessionId")]
        [HttpPost("ARMGetMenu")]
        public async Task<IActionResult> ARMGetMenu(ARMSession model)
        {
            var result = await _menu.GetMenu(model);
            if (result is not null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Error in Feteching Data"); 
            }
        }


        [RequiredFieldsFilter("ARMSessionId")]
        [HttpPost("ARMGetHomePageCards")]
        public async Task<IActionResult> ARMGetHomePageCards(ARMProcessFlowTask process)
        {
            var result = await _menu.GetHomePageCards(process);
            if (result is not null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Error in Fetching Data");
            }
        }
    }
}
