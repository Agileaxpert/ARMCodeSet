using ARM_APIs.Interface;
using ARMCommon.ActionFilter;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ServiceFilter(typeof(ApiResponseFilter))]
    [ApiController]
    public class ARMAxpertController : ControllerBase
    {
        private readonly IARMLogin _login;
        private readonly IRedisHelper _redis;
        private readonly INotificationHelper _notification;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        public ARMAxpertController(IARMLogin login, IRedisHelper redis, INotificationHelper notification, IConfiguration config, ITokenService tokenService)
        {
            _login = login;
            _redis = redis;
            _notification = notification;
            _config = config;
            _tokenService = tokenService;
        }

        [Authorize]
        [RequiredFieldsFilter("ARMSessionId")]
        [HttpPost("ARMConnectToAxpert")]
        public ActionResult ARMConnectToAxpert(ARMAxpertConnect axpert)
        {
            if (_redis.KeyExists(axpert.ARMSessionId))
            {
                if (string.IsNullOrEmpty(axpert.AppName))
                {
                    axpert.AppName = _redis.HashGet(axpert.ARMSessionId, Constants.SESSION_DATA.APPNAME.ToString());
                }


                string redisHost = _config["AppConfig:AxpertRedisHost"];
                string redisPass = _config["AppConfig:AxpertRedisPassword"];
                string secret = Guid.NewGuid().ToString();
                var privateKey = _config["ARM_PrivateKey"];
                string hashedKey = MD5Hash(privateKey + secret);
                var axpertConnectObj = new
                {
                    privatekey = hashedKey,//Encryptedkey
                    secret = secret,
                    hguid = secret,
                    hdeviceid = secret,
                    project = axpert.AppName,
                    username = _redis.HashGet(axpert.ARMSessionId, Constants.SESSION_DATA.USERNAME.ToString()),
                    roles = _redis.HashGet(axpert.ARMSessionId, Constants.SESSION_DATA.USER_ROLES.ToString()),
                    lang = "english"
                };

                var axpertRedis = new AxpertRedisHelper(redisHost, redisPass);
                axpertRedis.StringSet($"AXPERT-{axpert.ARMSessionId}", JsonConvert.SerializeObject(axpertConnectObj), 0, true);
                axpertRedis.CloseConnection();

                ARMResult result = new ARMResult();
                result.result.Add("message", "AXPERTCONNECTIONESTABLISHED");
                return Ok(result);
            }
            else
            {
                ARMResult result = new ARMResult();
                result.result.Add("message", "INVALIDSESSION");
                return BadRequest(result);
            }


        }

        public string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }


        [Authorize]
        [RequiredFieldsFilter("ARMSessionId")]
        [HttpPost("ARMConnectToARM")]
        public ActionResult ARMConnectToARM(ARMAxpertConnect axpert)
        {
            if (_redis.KeyExists(axpert.ARMSessionId))
            {
                if (string.IsNullOrEmpty(axpert.AppName))
                {
                    axpert.AppName = _redis.HashGet(axpert.ARMSessionId, Constants.SESSION_DATA.APPNAME.ToString());
                }
                string redisHost = _config["AppConfig:AxpertRedisHost"];
                string redisPass = _config["AppConfig:AxpertRedisPassword"];
                string secret = Guid.NewGuid().ToString();
                var privateKey = _config["ARM_PrivateKey"];
                string hashedKey = MD5Hash(privateKey + secret);
                var axpertConnectObj = new
                {
                    privatekey = hashedKey,//Encryptedkey
                    secret = secret,
                    hguid = secret,
                    hdeviceid = secret,
                    project = axpert.AppName,
                    username = _redis.HashGet(axpert.ARMSessionId, Constants.SESSION_DATA.USERNAME.ToString()),
                    roles = _redis.HashGet(axpert.ARMSessionId, Constants.SESSION_DATA.USER_ROLES.ToString()),
                    lang = "english"
                };

                var axpertRedis = new AxpertRedisHelper(redisHost, redisPass);
                axpertRedis.StringSet($"AXPERT-{axpert.ARMSessionId}", JsonConvert.SerializeObject(axpertConnectObj), 0, true);
                axpertRedis.CloseConnection();
                ARMResult result = new ARMResult();
                result.result.Add("message", "AXPERTCONNECTIONESTABLISHED");
                return Ok(result);
            }
            else
            {
                ARMResult result = new ARMResult();
                result.result.Add("message", "INVALIDSESSION");
                return BadRequest(result);
            }


        }
    }
}
