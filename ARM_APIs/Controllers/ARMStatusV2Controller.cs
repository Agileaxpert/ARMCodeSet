using ARM_APIs.Interface;
using ARMCommon.ActionFilter;
using ARMCommon.Helpers;
using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("2")]
    [ApiController]
    public class ARMStatusV2Controller : Controller
    {
        private readonly DataContext _context;
        private readonly Utils _common;
        private readonly IARMAppStatusV2 _appstatus;

        public ARMStatusV2Controller(DataContext context, Utils common, IARMAppStatusV2 armService)
        {
            _context = context;
            _common = common;
            _appstatus = armService;
        }
        
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpGet("ARMAppStatusV2")]
        [MapToApiVersion("2.0")]
        public async Task<ARMResult> ARMAppStatusV2()
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = await System.IO.File.ReadAllTextAsync(appSettingsPath);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            string connectionString = configuration.GetConnectionString("WebApiDatabase");
            string redisIP = configuration["AppConfig:RedisHost"];
            string redisPassword = configuration["AppConfig:RedisPassword"];
            string rabbitmqIP = configuration["AppConfig:RMQIP"];
            string axpertredisIP = configuration["AppConfig:AxpertRedisHost"];
            string axpertredisPassword = configuration["AppConfig:AxpertRedisPassword"];


            string armredisdetails =  await _appstatus.TestRedisConnection(redisIP, redisPassword);
            string axpertredisdetails = await _appstatus.TestAxpertRedisConnection(axpertredisIP, axpertredisPassword);
            string rabbitmqdetails = await _appstatus.TestRabbitmqConnection(rabbitmqIP);
            string testdbconnectionstring = await _appstatus.TestDatabaseConnectionString(connectionString);

            var appList = _context.ARMApps.Select(u => u.AppName).ToList();
            var results = new Dictionary<string, string>();

            foreach (var appName in appList)
            {
                string appConnectionString = await _common.GetDBConfiguration(appName);
                string result = await _appstatus.TestAppDatabaseConnection(appConnectionString,appName);
                results.Add(appName, result);
            }

            ARMResult result1 = new ARMResult();
            result1.result.Add("message", "SUCCESS");
            result1.result.Add("DBConnectionString", testdbconnectionstring);
            result1.result.Add("ARMRedisDetails", armredisdetails);
            result1.result.Add("AxpertRedisDetails" , axpertredisdetails);
            result1.result.Add("RabbitMQDetails", rabbitmqdetails);
            result1.result.Add("ARMAPPList", results);
            return result1;
            
        }

    }

}

