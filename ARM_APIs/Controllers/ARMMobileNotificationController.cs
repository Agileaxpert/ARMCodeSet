//using ARM_APIs.Interface;
//using ARMCommon.ActionFilter;
//using ARMCommon.Filter;
//using ARMCommon.Helpers;
//using ARMCommon.Interface;
//using ARMCommon.Model;
//using CsvHelper;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using NpgsqlTypes;
//using NPOI.POIFS.Crypt.Dsig;
//using NPOI.SS.Formula.Functions;
//using Oracle.ManagedDataAccess.Client;
//using Org.BouncyCastle.Ocsp;
//using System.Data;
//using Twilio.Base;

//namespace ARM_APIs.Controllers
//{
//    [Route("api/v{version:apiVersion}")]
//    [ApiVersion("1")]
//    //[Authorize]
//    // [ServiceFilter(typeof(ValidateSessionFilter))]
//    [ApiController]
//    public class ARMMobileNotificationController : Controller
//    {

//        private readonly IRedisHelper _redis;
//        private readonly IPostgresHelper _postGres;
//        private readonly IOracleHelper _oracle;
//        private readonly Utils _common;
//        private readonly IConfiguration _config;
//        public ARMMobileNotificationController(IRedisHelper redis, IPostgresHelper postGres, Utils common, IOracleHelper oracle, IConfiguration config)
//        {
//            _redis = redis;
//            _postGres = postGres;
//            _common = common;
//            _oracle = oracle;
//            _config = config;

//        }

//        [RequiredFieldsFilter("firebaseId", "ImeiNo", "ARMSessionId")]
//        [ServiceFilter(typeof(ApiResponseFilter))]
//        [HttpPost("ARMMobileNotification")]
//        public async Task<IActionResult> ARMMobileNotification(MobileNotification notification)
//        {
//            ARMResult result = new ARMResult();
//            SQLResult axnotification = new SQLResult();
//            SQLResult axmobilenotify = new SQLResult();
//            string SqlQuery = string.Empty;
//            string userName = string.Empty;
//            string firebaseid = string.Empty;
//            var loginuser = await _redis.HashGetAllDictAsync(notification.ARMSessionId);
//            if (loginuser == null)
//            {
//                return BadRequest("INVALIDSESSION");
//            }
//            firebaseid = notification.firebaseId;
//            userName = loginuser["USERNAME"];
//            string appName = loginuser["APPNAME"];
//            string sessId = notification.ARMSessionId;

//            Dictionary<string, string> config = await _common.GetDBConfigurations(appName);
//            string connectionString = config["ConnectionString"];
//            string dbType = config["DBType"];
//            SqlQuery = Constants_SQL.SELECT_AXMOBILENOTIFY.ToString();
//            string[] paramName = { "@username", "@firebaseid", "@appname" };
//            DbType[] paramType = { DbType.String, DbType.String, DbType.String };
//            object[] paramValue = { userName, firebaseid, appName.ToLower() };
//            IDbHelper dbHelper = DBHelper.CreateDbHelper(SqlQuery, dbType, connectionString, paramName, paramType, paramValue);
//            axnotification.data = await dbHelper.ExecuteQueryAsync(SqlQuery, connectionString, paramName, paramType, paramValue);

//            var Id = Guid.NewGuid();
//            notification.guid = Id;
//            if (axnotification.data.Rows.Count > 0)
//            {
//                SqlQuery = Constants_SQL.DELETE_AXMOBILENOTIFY.ToString();
//                await dbHelper.ExecuteQueryAsync(SqlQuery, connectionString, paramName, paramType, paramValue);

//                SqlQuery = Constants_SQL.INSERT_AXMOBILENOTIFY.ToString().Replace("$userName$", userName).Replace("$appName$", appName).Replace("$notificationguid$", notification.guid.ToString()).Replace("$notificationfirebaseId$", notification.firebaseId).Replace("$notificationImeiNo$", notification.ImeiNo).Replace("$notificationstatus$", notification.status);
//                await dbHelper.ExecuteQueryAsync(SqlQuery, connectionString, new string[] { }, new DbType[] { }, new object[] { });
//                return Ok("RECORDUPDATED");
//            }
//            else
//            {
//                SqlQuery = Constants_SQL.INSERT_AXMOBILENOTIFY.ToString().Replace("$userName$", userName).Replace("$appName$", appName).Replace("$notificationguid$", notification.guid.ToString()).Replace("$notificationfirebaseId$", notification.firebaseId).Replace("$notificationImeiNo$", notification.ImeiNo).Replace("$notificationstatus$", notification.status);
//                await dbHelper.ExecuteQueryAsync(SqlQuery, connectionString, new string[] { }, new DbType[] { }, new object[] { });
//                return Ok("RECORDINSERTED");
//            }
//        }

//        [ServiceFilter(typeof(ApiResponseFilter))]
//        [HttpPost("ARMDisableMobileNotificationForUser")]
//        public async Task<IActionResult> ARMDisableMobileNotificationForUser(string ARMSessionId)
//        {
//            string SqlQuery = string.Empty;
//            string userName = string.Empty;
//            var loginuser = await _redis.HashGetAllDictAsync(ARMSessionId);
//            if (loginuser == null)
//            {
//                return BadRequest("INVALIDSESSION");
//            }
//            userName = loginuser["USERNAME"];
//            string appName = loginuser["APPNAME"];

//            Dictionary<string, string> config = await _common.GetDBConfigurations(appName);
//            string connectionString = config["ConnectionString"];
//            string dbType = config["DBType"];
//            SqlQuery = Constants_SQL.DELETE_AXMOBILENOTIFYFORUSER.ToString();
//            string[] paramName = { "@username", "@appname" };
//            DbType[] paramType = { DbType.String, DbType.String };
//            object[] paramValue = { userName, appName.ToLower() };
//            IDbHelper dbHelper = DBHelper.CreateDbHelper(SqlQuery, dbType, connectionString, paramName, paramType, paramValue);
//            await dbHelper.ExecuteQueryAsync(SqlQuery, connectionString, paramName, paramType, paramValue);

//            return Ok($"Notification Disabled for user {userName}");
//        }
//    }
//}

