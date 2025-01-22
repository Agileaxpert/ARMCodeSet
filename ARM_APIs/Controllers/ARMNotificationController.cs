using ARM_APIs.Interface;
using ARMCommon.ActionFilter;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using ARMCommon.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Dynamic;
using ARMCommon.Filter;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ApiController]
    [ServiceFilter(typeof(ValidateSessionFilter))]
    public class ARMNotificationController : Controller
    {
        private readonly IARMNotificationService _notificationService;
        private readonly IRedisHelper _redis;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();


        public ARMNotificationController(IARMNotificationService notificationService, IHubContext<NotificationHub> hubContext, IEmailSender emailSender, DataContext context, IRedisHelper redis)
        {
            _notificationService = notificationService;
            _redis = redis;
            _hubContext = hubContext;



        }

        [RequiredFieldsFilter("firebaseId", "ImeiNo", "ARMSessionId")]
        [HttpPost("ARMMobileNotification")]
        public async Task<IActionResult> ARMMobileNotification(MobileNotification notification)
        {
            var result = await _notificationService.ProcessARMMobileNotification(notification);

            if (result.ToString() == Constants.RESULTS.ERROR.ToString())
            {
                return BadRequest("INVALIDDETAILS");
            }
            else if (result.ToString() == Constants.RESULTS.RECORDINSERTED.ToString())
            {
                return Ok("RECORDINSERTED");
            }
            else if (result.ToString() == Constants.RESULTS.RECORDUPDATED.ToString())
            {
                return Ok("RECORDUPDATED");
            }
            else
            {
                return BadRequest("Unexpected Error");
            }
        }


            [RequiredFieldsFilter("Project", "Message", "UserId")]
            [HttpPost("SendSignalR")]
            public async Task<IActionResult> SendSignalR(SendSignalRRequest request)
            {
                string connectionId = _redis.StringGet($"ARM-{request.Project}-{request.UserId}");
                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", request.Message);
                    return Ok("RECEIVEDNOTIFICATION");
                }
                else
                {
                    return BadRequest("INAVALIDUSERID");
                }
            }



            [RequiredFieldsFilter("ARMSessionId")]
            [HttpPost("ARMDisableMobileNotificationForUser")]
            public async Task<IActionResult> ARMDisableMobileNotificationForUser(string ARMSessionId)
            {
                var result = await _notificationService.DisableMobileNotificationForUserAsync(ARMSessionId);
                if (result)
                {
                    return Ok("NOTIFICATIONDISABLED");
                }
                else
                {
                    return BadRequest("INVALIDSESSION");
                }
            }



            [HttpPost]
            public async Task<IActionResult> Post(ARMNotify model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _notificationService.SendEmailNotification(model);
                return result;

            }
        }


    }


