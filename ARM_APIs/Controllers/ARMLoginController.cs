using ARM_APIs.Interface;
using ARMCommon.ActionFilter;
using ARMCommon.Filter;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ARMCommon.Helpers.Constants;

namespace ARM_APIs.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1")]
    [ApiController]
    public class ARMLoginController : ControllerBase
    {
        private readonly IARMLogin _login;
        private readonly IRedisHelper _redis;
        private readonly INotificationHelper _notification;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        public readonly Utils _utils;

        public ARMLoginController(IARMLogin login, IRedisHelper redis, INotificationHelper notification, IConfiguration config, ITokenService tokenService, Utils utils)
        {
            _login = login;
            _redis = redis;
            _notification = notification;
            _config = config;
            _tokenService = tokenService;
            _utils = utils;
        }

        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpGet("ARMAppStatus")]
        [MapToApiVersion("1.0")]
        public ActionResult ARMAppStatus()
        {
            return Ok("APPISRUNNING");
        }

        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpGet("ARMTest")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> ARMTest()
        {
            string appname = "agiledemo";
            string message = await _utils.GetDBConfiguration(appname);
            ARMResult result = new ARMResult();
            result.result.Add("message", "SUCCESS");
            result.result.Add("data", message);
            return Ok(result);
        }


        private static ARMResult successfulloginresult(string messagecode, string token, string sessionid)
        {
            ARMResult result = new ARMResult();
            result.result.Add("message", messagecode);
            result.result.Add("token", token);
            result.result.Add("sessionid", sessionid);
            return result;
        }


        #region ARMSignIn
        [AllowAnonymous]
        [RequiredFieldsFilter("appname", "username", "usergroup", "password")]
        [ServiceFilter(typeof(LoggingActionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpPost("ARMSignin")]
        public async Task<IActionResult> ARMSignin(ARMLoginUser loginuser)
        {
            if (!_login.AppExists(loginuser.appname))
            {
                return BadRequest("APPNOTEXIST");
            }

            var userGroup = await _login.GetUserGroup(loginuser.usergroup);
            if (userGroup == null)
            {
                return BadRequest("USERGROUPNOTEXIST");
            }

            string sessionId = $"{Constants.REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
            var signinUserDetails = await _login.SigninUser(loginuser, userGroup, sessionId);


            if (signinUserDetails.ToString() == Constants.RESULTS.NO_RECORDS.ToString())
            {
                return BadRequest("INVALIDCREDENTIAL");
            }
            else if (signinUserDetails.ToString() == Constants.RESULTS.NO_TOKEN.ToString())
            {
                return BadRequest("TOKENGENERATIONFAILED");
            }
            else
            {
                if (loginuser.isfirsttime)
                {
                    var result = SuccessfulLoginResults("CHANGEPASSWORD", loginuser.token, sessionId, loginuser.isfirsttime);
                    return Ok(result);
                }
                else
                {
                    var result = SuccessfulLoginResults("SignIn", loginuser.token, sessionId, loginuser.isfirsttime);
                    return Ok(result);
                }

            }
        }

        private static ARMResult SuccessfulLoginResults(string messagecode, string token, string sessionId, bool isFirstTime)
        {
            ARMResult result = new ARMResult();
            result.result.Add("message", messagecode);
            result.result.Add("token", token);
            result.result.Add("sessionid", sessionId);
            result.result.Add("ChangePassword", isFirstTime);
            return result;
        }

        #endregion


        #region ARMAddValidateUsers
        [AllowAnonymous]
        [RequiredFieldsFilter("appname", "usergroup", "password")]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpPost("ARMAddUser")]
        public async Task<IActionResult> ARMAddUser(ARMUser user)
        {
            if (string.IsNullOrEmpty(user.email))
            {
                user.email = "";
            }

            if (!_login.AppExists(user.appname))
            {
                return BadRequest("APPNOTEXIST");
            }

            if (_login.UserEmailExists(user.email, user.appname))
            {
                return BadRequest("EMAILEXISTS");
            }

            var userGroup = await _login.GetUserGroup(user.usergroup);
            if (userGroup == null)
            {
                return BadRequest("USERGROUPNOTEXIST");
            }

            string otp = _notification.GenerateOTP();
            string regId = Guid.NewGuid().ToString();
            if (userGroup.GroupType.ToUpper() == Constants.GROUPTYPES.EXTERNAL.ToString())
            {
                if (string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.username))
                {
                    return BadRequest("REQUIREDFIELDMISS");
                }

                if (_login.UserExists(user.username, user.appname, user.usergroup))
                {
                    return BadRequest("ALREADYREGISTERED");
                }

                await _login.SaveUserRegistrationDetails(user, userGroup, otp, regId);
            }
            else if (userGroup.GroupType.ToUpper() == Constants.GROUPTYPES.INTERNAL.ToString())
            {
                if (string.IsNullOrEmpty(user.userid))
                {
                    return BadRequest("USERIDMISSING");
                }

                var internalUser = await _login.GetInternalUserDetails(userGroup, user.userid);
                if (internalUser == Constants.RESULTS.NO_RECORDS.ToString())
                {
                    return BadRequest("INTERNALAUTHFAIED");
                }
                var internalUserDetails = JsonConvert.DeserializeObject<Dictionary<string, string>>(internalUser);
                user.email = internalUserDetails["email"].ToString();
                user.username = internalUserDetails["username"].ToString();
                user.mobileno = internalUserDetails["mobile"].ToString();

                if (_login.UserExists(user.username, user.appname, user.usergroup))
                {
                    return BadRequest("ALREADYREGISTERED");
                }

                if (string.IsNullOrEmpty(user.email) || string.IsNullOrEmpty(user.username))
                {
                    return BadRequest("USERNAME/EMAIL_MISSING");
                }

                await _login.SaveUserRegistrationDetails(user, userGroup, otp, regId);


            }
            else if (userGroup.GroupType.ToUpper() == Constants.GROUPTYPES.POWER.ToString())
            {
                return BadRequest("NOREGISTRATIONFORPOWERUSERS");
            }

            var response = await _notification.SendEmailOTP(user.email, otp,user.username);

            if (response == null)
            {
                return BadRequest("ERRORINSENDINGOTP");
            }
            else
            {
                var result = _utils.GetOTPResult("OTPSENT", regId);
                return Ok(result);
            }
        }

        [AllowAnonymous]
        [RequiredFieldsFilter("regid", "otp")]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpPost("ARMValidateAddUser")]
        public async Task<IActionResult> ARMValidateAddUser(ARMValidateUser user)
        {
            string key = $"{Constants.REDIS_PREFIX.ARMADDUSER.ToString()}_{user.regid}";
            string registrationData = await _redis.StringGetAsync(key);

            if (!string.IsNullOrEmpty(registrationData))
            {
                var registrationDetails = JsonConvert.DeserializeObject(registrationData).ToString();
                var userDetails = JObject.Parse(registrationDetails);
                string otp = userDetails["otp"].ToString();
                int totalAttempt = ((int)userDetails["otpattemptsleft"]);

                if (_login.UserExists(userDetails["username"]?.ToString(), userDetails["appname"]?.ToString(), userDetails["usergroup"]?.ToString()))
                {
                    return BadRequest("ALREADYREGISTERED");
                }

                if (totalAttempt > 0)
                {
                    ARMResult resultData;
                    if (user.otp == otp)
                    {
                        var userAdded = await _login.AddUser(userDetails["appname"]?.ToString(), userDetails["username"]?.ToString(), userDetails["password"]?.ToString(), userDetails["email"]?.ToString(), userDetails["usergroup"]?.ToString(), userDetails["mobileno"]?.ToString(), Guid.Parse(userDetails["usergroupid"].ToString()), key);
                        if (userAdded)
                        {
                            return Ok("USERREGISTERED");
                        }
                        else
                        {
                            return BadRequest("USERREGISTRATIONFAILED");
                        }

                    }
                    else
                    {
                        userDetails["otpattemptsleft"] = totalAttempt - 1;
                        await _redis.StringSetAsync(key, JsonConvert.SerializeObject(userDetails));
                        var result = _utils.OTPFailureResult("OTPUNMATCHED", totalAttempt);
                        return BadRequest(result);
                    }
                }
                else
                {
                    return BadRequest("NOATTEMPTSLEFT");
                }

            }
            else
            {
                return BadRequest("NOREGISTRATIONDATAFOUND");
            }

        }
        #endregion

        #region ARMConnectFromAxpert
        [AllowAnonymous]
        [RequiredFieldsFilter("Key", "AxSessionId", "User", "AppName")]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpPost("ARMConnectFromAxpert")]
        public async Task<IActionResult> ARMConnectFromAxpert(ARMAxpertConnect axpert)
        {
            var validationResult = await _login.ValidateAxpertConnect(axpert);
            if (validationResult.ToString() != Constants.RESULTS.ERROR.ToString())
            {
                var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                     axpert.User, axpert.User, axpert.User);

                ARMResult resultData = new ARMResult();
                resultData.result.Add("message", "CONNECTIONESTABLISHED");
                resultData.result.Add("connectionid", validationResult);
                resultData.result.Add("token", generatedToken);
                return Ok(resultData);
            }
            else
            {
                return BadRequest("ARMAUTHFAILED");
            }
        }
        #endregion

        #region ARMSSOSignIn

        [AllowAnonymous]
        [RequiredFieldsFilter("appname", "usergroup", "userid")]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpPost("ARMSigninSSO")]
        public async Task<IActionResult> ARMSigninSSO(ARMLoginSSO user)
        {
            var userGroup = await _login.GetUserGroup(user.usergroup);

            if (userGroup.GroupType.ToLower() != Constants.GROUPTYPES.EXTERNAL.ToString().ToLower())
            {
                return BadRequest("SSOFOREXTERNALONLY");
            }
            if (_login.SSOUserExists(user.userid, user.appname))
            {
                if (await _login.ValidateGoogleSSO(user.ssodetails["token"], user.userid, user.ssodetails["id"]))
                {
                    var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                     user.userid, user.usergroup, user.usergroupid.ToString());
                    string sessionId = $"{Constants.REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
                    await _login.StoreSessionValues(sessionId, string.Join(",", userGroup.Roles), user.userid, user.appname, user.usergroup, userGroup.GroupType, generatedToken);
                    var result = successfulloginresult("SignIn", generatedToken, sessionId);
                    return Ok(result);
                }
                else
                {
                    return BadRequest("INVALIDCREDENTIALORTOKEN");
                }

            }

            else
            {
                ARMResult result = new ARMResult();
                result.result.Add("message", "USERNOTEXISTS");
                result.result.Add("doregister", true);
                return BadRequest(result);
            }


        }


        [AllowAnonymous]
        [HttpPost("ARMAddUserSSO")]
        public async Task<IActionResult> ARMAddUserSSO(ARMLoginSSO user)
        {
            if (!_login.AppExists(user.appname))
            {
                return BadRequest("APPNOTEXIST");
            }

            if (_login.UserEmailExists(user.email, user.appname))
            {
                return BadRequest("EMAILEXISTS");
            }


            var userGroup = await _login.GetUserGroup(user.usergroup);
            if (userGroup == null)
            {
                return BadRequest("USERGROUPNOTEXIST");
            }

            if (userGroup.GroupType.ToLower() != Constants.GROUPTYPES.EXTERNAL.ToString().ToLower())
            {
                return BadRequest("SSOFOREXTERNALONLY");
            }

            if (_login.UserExists(user.username, user.appname, user.usergroup))
            {
                return BadRequest("ALREADYREGISTERED");
            }
            if (await _login.ValidateGoogleSSO(user.ssodetails["token"], user.email, user.ssodetails["id"]))
            {
                var userAdded = await _login.AddUser(user.appname, user.username, user.password, user.email, user.usergroup, user.mobile, Guid.Parse(userGroup.ID.ToString()));
                if (userAdded)
                {
                    string sessionId = $"{Constants.REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
                    var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                user.usergroupid.ToString(), user.usergroup, user.usergroupid.ToString());
                    await _login.StoreSessionValues(sessionId, string.Join(",", userGroup.Roles), user.username, user.appname, user.usergroup, userGroup.GroupType, generatedToken);
                    var result = successfulloginresult("SignIn", generatedToken, sessionId);
                    return Ok(result);
                }
                else
                {
                    return BadRequest("USERREGISTRATIONFAILED");
                }

            }
            else
            {
                return BadRequest("INVALIDCREDENTIALORTOKEN");
            }




        }
        #endregion

        #region ARMBIOMETRIC
        [AllowAnonymous]
        [RequiredFieldsFilter("appname", "usergroup", "deviceid", "biometricType")]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpPost("ARMBiometricSignIn")]
        public async Task<IActionResult> ARMBiometricSignIn(UserDevice user)
        {
            //ARMResult result = new ARMResult();

            if (!_login.AppExists(user.appname))
            {
                return BadRequest("APPNOTEXIST");
            }
            var userGroup = await _login.GetUserGroup(user.usergroup);
            if (userGroup == null)
            {
                return BadRequest("USERGROUPNOTEXIST");
            }
            var userRoleslist = userGroup.Roles;
            if (user.biometricType.ToLower() != Constants.BIOMETRIC.FINGERPRINT.ToString().ToLower() && user.biometricType.ToLower() != Constants.BIOMETRIC.FACIALRECOGNIZATION.ToString().ToLower())
            {

                return BadRequest("INVALIDBIOMETRIC");
            };


            if (!_login.BiometricEnabled(user.appname))
            {
                return BadRequest("BIOMETRICDISABLED");
            }

            if (string.IsNullOrEmpty(user.password))
            {
                var userDevices = await _login.GetUserDevices(user.username, user.appname, user.deviceid);
                if (userDevices == null)
                {
                    return BadRequest("INVALIDDEVICEID");
                }

                if (userGroup.GroupType.ToLower() == GROUPTYPES.POWER.ToString().ToLower())
                {
                    var powerusers = await _login.ValidatePowerUsers(user.username, user.appname);
                    userRoleslist = await _login.GetPowerUserRoleList(user.username, user.appname);
                    if (powerusers.Rows == null || powerusers.Rows.Count == 0)
                    {
                        //userRoleslist = (List<string>?)await _login.GetPowerUserRoleList(user.username, user.appname);
                        return BadRequest("NOPOWERUSERFOUND");
                    }

                    else
                    {
                        var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                      user.deviceid, user.usergroup, user.usergroupid.ToString());
                        string sessionId = $"{Constants.REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
                        await _login.StoreSessionValues(sessionId, string.Join(",", userRoleslist), user.username, user.appname, user.usergroup, userGroup.GroupType, generatedToken);
                        var result = successfulloginresult("SignIn", generatedToken, sessionId);
                        return Ok(result);
                    }

                }
                else
                {
                    if (!_login.UserExists(user.username, user.appname, user.usergroup))
                    {
                        return BadRequest("INTERNALEXTERNALUSERNOTEXISTS");
                    }
                    else
                    {
                        var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                       user.deviceid, user.usergroup, user.usergroupid.ToString());
                        string sessionId = $"{Constants.REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
                        await _login.StoreSessionValues(sessionId, string.Join(",", userGroup.Roles), user.username, user.appname, user.usergroup, userGroup.GroupType, generatedToken);
                        var result = successfulloginresult("SignIn", generatedToken, sessionId);
                        return Ok(result);

                    }
                }


            }

            else
            {
                if (userGroup.GroupType.ToLower() == GROUPTYPES.POWER.ToString().ToLower())
                {
                    var powerUser = await _login.ValidatePowerUserWithPassword(user.username, user.password, user.appname);
                    userRoleslist = await _login.GetPowerUserRoleList(user.username, user.appname);
                    if (powerUser.Rows == null || powerUser.Rows.Count == 0)
                    {
                        return BadRequest("NOPOWERUSERFOUND");
                    }

                }

                else
                {
                    var armUsers = await _login.GetARMUsers(user.appname, user.username, user.password, user.usergroup);
                    if (armUsers == null)
                    {
                        return BadRequest("INVALIDCREDENTIAL");
                    }
                }


                var userDevices = await _login.GetUserDevices(user.username.ToString(), user.appname.ToString(), user.deviceid.ToString());

                if (userDevices != null)
                {
                    var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                   user.deviceid, user.usergroup, user.usergroupid.ToString());
                    string sessionId = $"{Constants.REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
                    await _login.StoreSessionValues(sessionId, string.Join(",", userRoleslist), user.username, user.appname, user.usergroup, userGroup.GroupType, generatedToken);
                    var result = successfulloginresult("SignIn", generatedToken, sessionId);
                    return Ok(result);
                }

                if (_login.DeviceExists(user.deviceid))
                {
                    await _login.RemoveARMUserDevices(user.deviceid);
                }
                var id = Guid.NewGuid();
                user.id = id;

                var userAdded = await _login.AddARMDevices(user);

                if (userAdded)
                {
                    var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
              user.deviceid, user.usergroup, user.usergroupid.ToString());
                    string sessionId = $"{Constants.REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
                    await _login.StoreSessionValues(sessionId, string.Join(",", userRoleslist), user.username, user.appname, user.usergroup, userGroup.GroupType, generatedToken);
                    var result = successfulloginresult("SignIn", generatedToken, sessionId);
                    return Ok(result);
                }
                else
                {
                    return BadRequest("DEVICEREGISTRATIONFAILED");
                }
            }

        }

        #endregion


        #region ARMSignOut
        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [HttpPost("ARMSignOut")]
        public async Task<IActionResult> ARMSignOut(ARMSession sessionid)
        {
            await _redis.KeyDeleteAsync(sessionid.ARMSessionId);
            return Ok("USERSIGNOUT");
        }

        #endregion

        #region ARMChangePassword
        [RequiredFieldsFilter("CurrentPassword", "UpdatedPassword", "ARMSessionId")]
        [ServiceFilter(typeof(ValidateSessionFilter))]
        [ServiceFilter(typeof(ApiResponseFilter))]
        [Authorize]
        [HttpPost("ARMChangePassword")]
        public async Task<IActionResult> ARMChangePassword(UpdatePassword loginuser)
        {
            if (loginuser.CurrentPassword == loginuser.UpdatedPassword)
            {
                return BadRequest("PASSWORDSHOULDBEDIFFERENT");
            }

            var result = await _login.ChangePassword(loginuser.ARMSessionId, loginuser.CurrentPassword, loginuser.UpdatedPassword);

            if (result.ToString() == Constants.RESULTS.INVALIDPASSWORD.ToString() || result.ToString() == Constants.RESULTS.ERROR.ToString())
            {
                return BadRequest("INCORRECTPASSWORD");
            }

            else if (result.ToString() == Constants.RESULTS.ERROR.ToString())
            {
                return Ok("PASSWORDUPDATIONFAILED");
            }
            else
            {
                return Ok("PASSWORDUPDATED");
            }


        }




        #endregion



       

        [RequiredFieldsFilter("email", "username", "usergroup", "appname")]
        [HttpPost("ARMForgotPassword")]
        [ServiceFilter(typeof(ApiResponseFilter))]

        public async Task<IActionResult> ARMForgotPassword(ARMForgetPassword forgetPassword)
        {

            if (!_login.AppExists(forgetPassword.appname))
            {
                return BadRequest("APPNOTEXIST");
            }

            var userGroup = await _login.GetUserGroup(forgetPassword.usergroup);
            if (userGroup == null)
            {
                return BadRequest("USERGROUPNOTEXIST");
            }

            var validationResult = await _login.ForgotPassword(forgetPassword.email, forgetPassword.username, forgetPassword.usergroup, forgetPassword.appname);

            if (validationResult.ToString() == Constants.RESULTS.SUCCESS.ToString())
            {
                return Ok("FORGOTPASSWORDUPDATED");
            }
            else if (validationResult.ToString() == Constants.RESULTS.INVALIDINTERNALENTERNALUSER.ToString() || validationResult.ToString() == Constants.RESULTS.INVALIDPOWERUSER.ToString())
            {
                return BadRequest("INVAILDDETAILS");
            }
            else
            {
                return Ok("FORGOTPASSWORDUPDATED");
            }
        }


    }
}
