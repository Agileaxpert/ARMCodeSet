using ARM_APIs.Interface;
using ARM_APIs.Model;
using ARMCommon.ActionFilter;
using ARMCommon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [Authorize]
    [ServiceFilter(typeof(ApiResponseFilter))]
    [ApiController]
    public class EntityController : ControllerBase
    {

        private readonly IEntityService _entity;
        public EntityController(IEntityService entity)
        {
            _entity = entity;
        }

        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetEntityListData")]
        public async Task<IActionResult> GetEntityListData(Entity entity)
        {
            SQLResult entityList;
            if (string.IsNullOrEmpty(entity.Filter))
                entityList = await _entity.GetEntityListData(entity);
            else
                entityList = await _entity.GetFilteredEntityListData(entity);

            var entityMetaData = new SQLResult();
            if (entity.MetaData == true)
            {
                entityMetaData = await _entity.GetEntityMetaData(entity);
            }

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityMetaData.error))
            {
                result.result.Add("message", entityMetaData.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else if (!string.IsNullOrEmpty(entityList.error))
            {
                result.result.Add("message", entityList.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                if (entityList.data?.Rows?.Count == 0 || (entityList.data?.Rows?.Count > 0 && (string.IsNullOrEmpty(entityList.data?.Rows[0][0]?.ToString()) || entityList.data?.Rows[0][0]?.ToString() == "[]")))
                    result.result.Add("count", 0);

                result.result.Add("list", entityList.data);

                if (entity.MetaData == true)
                {
                    result.result.Add("metadata", entityMetaData.data);
                }
                return Ok(result);
            }
        }

        //[AllowAnonymous]
        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetEntityMetaData")]
        public async Task<IActionResult> GetEntityMetaData(Entity entity)
        {

            var entityMetaData = await _entity.GetEntityMetaData(entity);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityMetaData.error))
            {
                result.result.Add("message", entityMetaData.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("metadata", entityMetaData.data);
                return Ok(result);
            }
        }

        //[AllowAnonymous]
        [ApiVersion("1")]
        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetEntityMetaDataV2")]
        public async Task<IActionResult> GetEntityMetaDataV2(Entity entity)
        {

            var entityMetaData = await _entity.GetEntityMetaDataV2(entity);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityMetaData.error))
            {
                result.result.Add("message", entityMetaData.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("metadata", entityMetaData.data);
                return Ok(result);
            }
        }

        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetEntityChartsData")]
        public async Task<IActionResult> GetEntityChartsData(EntityCharts entity)
        {
            var entityCharts = await _entity.GetEntityChartsData(entity);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityCharts.error))
            {
                result.result.Add("message", entityCharts.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");

                result.result.Add("charts", entityCharts.data);
                return Ok(result);
            }
        }

        [RequiredFieldsFilter("ARMSessionId", "TransId", "ChartMetaData")]
        [HttpPost("GetAnalyticsChartsData")]
        public async Task<IActionResult> GetAnalyticsChartsData(AnalyticsCharts charts)
        {
            var entityCharts = await _entity.GetAnalyticsChartsData(charts);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityCharts.error))
            {
                result.result.Add("message", entityCharts.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");

                result.result.Add("charts", entityCharts.data);
                return Ok(result);
            }
        }


        //[RequiredFieldsFilter("ARMSessionId","EntityName")]
        //[ServiceFilter(typeof(ApiResponseFilter))]
        //[HttpPost("GetEntityChartsMetaData")]
        //public async Task<IActionResult> GetEntityChartsMetaData(EntityCharts entity)
        //{
        //    var entityCharts = await _entity.GetEntityChartsMetaData(entity);

        //    ARMResult result = new ARMResult();
        //    if (!string.IsNullOrEmpty(entityCharts.error))
        //    {
        //        result.result.Add("message", entityCharts.error);
        //        result.result.Add("messagetype", "Custom");
        //        return BadRequest(result);
        //    }
        //    else
        //    {
        //        result.result.Add("message", "SUCCESS");
        //        result.result.Add("metadata", entityCharts.data);
        //        return Ok(result);
        //    }
        //}

        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetSubEntityListData")]
        public async Task<IActionResult> GetSubEntityListData(Entity entity)
        {

            var entityList = await _entity.GetSubEntityListData(entity);

            var entityMetaData = new SQLResult();
            if (entity.MetaData == true)
            {
                entityMetaData = await _entity.GetSubEntityMetaData(entity);
            }

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityMetaData.error))
            {
                result.result.Add("message", entityMetaData.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else if (!string.IsNullOrEmpty(entityList.error))
            {
                result.result.Add("message", entityList.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("list", entityList.data);
                if (entity.MetaData == true)
                {
                    result.result.Add("metadata", entityMetaData.data);
                }
                return Ok(result);
            }
        }

        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetSubEntityMetaData")]
        public async Task<IActionResult> GetSubEntityMetaData(Entity entity)
        {

            var entityMetaData = await _entity.GetSubEntityMetaData(entity);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityMetaData.error))
            {
                result.result.Add("message", entityMetaData.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("metadata", entityMetaData.data);
                return Ok(result);
            }
        }

        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetEntityFormMetaData")]
        public async Task<IActionResult> GetEntityFormMetaData(EntityForm entity)
        {
            var entityMData = await _entity.GetEntityFormMetaData(entity);

            //TO DO
            //var subEntityMData = new SQLResult();
            //if (entity.SubEntityMetaData == true) { 

            //}

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityMData.error))
            {
                result.result.Add("message", entityMData.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("metadata", entityMData.data);
                return Ok(result);
            }
        }

        [RequiredFieldsFilter("ARMSessionId", "EntityName")]
        [HttpPost("GetSubEntityChartsData")]
        public async Task<IActionResult> GetSubEntityChartsData(EntityCharts entity)
        {
            var entityCharts = await _entity.GetSubEntityChartsData(entity);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityCharts.error))
            {
                result.result.Add("message", entityCharts.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("charts", entityCharts.data);
                return Ok(result);
            }
        }

        [RequiredFieldsFilter("ARMSessionId")]
        [HttpPost("GetEntityList")]
        public async Task<IActionResult> GetEntityList(Entity entity)
        {
            var entityList = await _entity.GetEntityList(entity);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(entityList.error))
            {
                result.result.Add("message", entityList.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("list", entityList.data);
                return Ok(result);
            }
        }

        //[RequiredFieldsFilter("ARMSessionId","EntityName")]
        //[ServiceFilter(typeof(ApiResponseFilter))]
        //[HttpPost("GetSubEntityChartsMetaData")]
        //public async Task<IActionResult> GetSubEntityChartsMetaData(EntityCharts entity)
        //{
        //    var entityCharts = await _entity.GetSubEntityChartsMetaData(entity);

        //    ARMResult result = new ARMResult();
        //    if (!string.IsNullOrEmpty(entityCharts.error))
        //    {
        //        result.result.Add("message", entityCharts.error);
        //        result.result.Add("messagetype", "Custom");
        //        return BadRequest(result);
        //    }
        //    else
        //    {
        //        result.result.Add("message", "SUCCESS");
        //        result.result.Add("metadata", entityCharts.data);
        //        return Ok(result);
        //    }
        //}

        [RequiredFieldsFilter("ARMSessionId", "AppName", "UserName", "Page", "Properties")]
        [HttpPost("SetAnalyticsData")]
        public async Task<IActionResult> SetAnalyticsData(AnalyticsData analyticsData)
        {

            var apiResult = await _entity.SetAnalyticsData(analyticsData);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(apiResult.error))
            {
                result.result.Add("message", apiResult.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                return Ok(result);
            }
        }


        [RequiredFieldsFilter("ARMSessionId", "AppName", "UserName", "Page", "PropertiesList")]
        [HttpPost("GetAnalyticsData")]
        public async Task<IActionResult> GetAnalyticsData(AnalyticsData analyticsData)
        {

            var apiResult = await _entity.GetAnalyticsData(analyticsData);

            ARMResult result = new ARMResult();
            if (!string.IsNullOrEmpty(apiResult.error))
            {
                result.result.Add("message", apiResult.error);
                result.result.Add("messagetype", "Custom");
                return BadRequest(result);
            }
            else
            {
                result.result.Add("message", "SUCCESS");
                result.result.Add("data", apiResult.data);
                return Ok(result);
            }
        }

        [RequiredFieldsFilter("ARMSessionId", "AppName", "UserName", "Page", "PropertiesList")]
        [HttpPost("GetAnalyticsEntityData")]
        public async Task<IActionResult> GetAnalyticsEntityData(AnalyticsEntityInput analyticsInput)
        {
            AnalyticsEntityOutput analyticResult;
            if (string.IsNullOrEmpty(analyticsInput.TransId))
                analyticResult = await _entity.GetAnalyticsPageLoadData(analyticsInput);
            else
                analyticResult = await _entity.GetAnalyticsEntityData(analyticsInput);

            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", analyticResult);
            return Ok(result);
        }

        [AllowAnonymous]
        [RequiredFieldsFilter("ARMSessionId", "TransId")]
        [HttpPost("GetEntityListPageLoadData")]
        public async Task<IActionResult> GetEntityListPageLoadData(Entity entity)
        {
            var entityList = await _entity.GetEntityListPageLoadData(entity);
            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", entityList);
            return Ok(result);


        }
    }
}
