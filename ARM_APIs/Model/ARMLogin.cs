
using ARM_APIs.Interface;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using StackExchange.Redis;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using static ARMCommon.Helpers.Constants;

namespace ARM_APIs.Model
{
    public class ARMLogin : IARMLogin
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly IRedisHelper _redis;
        private readonly ITokenService _tokenService;
        private readonly IPostgresHelper _postGres;
        private readonly IAPI _api;
        private readonly Utils _common;
        private readonly INotificationHelper _notification;

        public ARMLogin(DataContext context, IConfiguration configuration, INotificationHelper notification, ITokenService tokenService, IRedisHelper redis, IPostgresHelper postGres, IAPI api, Utils common)
        {
            _context = context;
            _config = configuration;
            _tokenService = tokenService;
            _redis = redis;
            _postGres = postGres;
            _api = api;
            _common = common;
            _notification = notification;

        }
        public async Task<bool> AddUser(string appname, string username, string password, string email, string usergroup, string mobileno, Guid usergroupid, string registrationKey = "")
        {
            try
            {
                if (registrationKey != "") await _redis.KeyDeleteAsync(registrationKey);
                ARMUser newuser = new ARMUser();
                newuser.ID = Guid.NewGuid();
                newuser.usergroupid = usergroupid;
                newuser.appname = appname;
                newuser.username = username;
                newuser.password = password;
                newuser.email = email;
                newuser.usergroup = usergroup;
                newuser.mobileno = mobileno;
                newuser.isactive = true;
                _context.Add(newuser);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;


        }



        public async Task<object> SigninUser(ARMLoginUser loginUser, ARMUserGroup userGroup, string sessionId)
        {
            string? groupType = userGroup.GroupType;
            if (groupType.ToUpper() == GROUPTYPES.POWER.ToString())
            {
                var powerUserTable = await GetPowerUser(loginUser);

                if (powerUserTable == null)
                {
                    return RESULTS.ERROR;
                }
                else
                {
                    if (powerUserTable != null && powerUserTable.Rows.Count > 0)                    
                    {
                        foreach (DataRow row in powerUserTable.Rows)
                        {
                            if (row["isfirsttime"].ToString().ToUpper() != "F")
                            {
                                loginUser.isfirsttime = true;
                            }
                            else
                            {
                                loginUser.isfirsttime = false;
                            }
                        }

                        var userRoles = await GetPowerUserRoles(loginUser);
                        var userRoleslist = userRoles.AsEnumerable().Select(r => r["USERROLES"].ToString());
                        var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                            loginUser.username, loginUser.usergroup, userGroup.ID.ToString());

                        string allroles = String.Empty;
                        if (generatedToken != null)
                        {
                            if (!string.IsNullOrEmpty(loginUser.deviceid))
                            {
                                await UpdateARMDevices(loginUser.deviceid, loginUser.appname, loginUser.username);
                            }
                            loginUser.token = generatedToken;
                            await StoreSessionValues(sessionId, string.Join(",", userRoleslist), loginUser.username, loginUser.appname, loginUser.usergroup, groupType, generatedToken);
                        }
                        else
                        {
                            return RESULTS.NO_TOKEN;
                        }
                    }
                    else
                    {
                        return RESULTS.NO_RECORDS;
                    }
                }
            }
            else
            {
                var user = await _context.ARMUsers.FirstOrDefaultAsync(f => f.username.ToLower() == loginUser.username.ToLower() && f.password == loginUser.password && f.appname.ToLower() == loginUser.appname.ToLower() && f.usergroup.ToLower() == loginUser.usergroup.ToLower() && f.isactive);

                if (user.isfirsttime != "F")
                {
                    loginUser.isfirsttime = true;
                }

                var generatedToken = _tokenService.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(),
                                        loginUser.username, loginUser.usergroup, user.ID.ToString());

                if (generatedToken != null)
                {
                    if (!string.IsNullOrEmpty(loginUser.deviceid))
                    {
                        await UpdateARMDevices(loginUser.deviceid, loginUser.appname, loginUser.username);
                    }
                    loginUser.token = generatedToken;
                    await StoreSessionValues(sessionId, string.Join(",", userGroup.Roles), user.username, user.appname, user.usergroup, groupType, generatedToken);

                }
                else
                {
                    return RESULTS.NO_TOKEN;
                }

            }
            return loginUser;
        }
        private async Task<DataTable> GetPowerUser(ARMLoginUser loginUser)
        {
            string hashedPwd = MD5Hash(loginUser.password);
            Dictionary<string, string> config = await _common.GetDBConfigurations(loginUser.appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.GETPOWERUSER.ToString();
            string[] paramName = { "@username", "@password" };
            DbType[] paramType = { DbType.String, DbType.String};
            object[] paramValue = { loginUser.username.ToLower(), hashedPwd};
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            DataTable powerUserTable = await dbHelper.ExecuteQueryAsync(sql, connectionString, paramName, paramType, paramValue);
            return powerUserTable;
        }



        public async Task<SQLResult> GetPowerUsers(string currentPassword, string username, string appname)
        {
            string password = MD5Hash(currentPassword);
            Dictionary<string, string> config = await _common.GetDBConfigurations(appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.GETPOWERUSER.ToString();
            string[] paramName = { "@username", "@password" };
            DbType[] paramType = { DbType.String, DbType.String };
            object[] paramValue = { username, password };
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            return await dbHelper.ExecuteSQLAsync(sql, connectionString, paramName, paramType, paramValue);
        }
        public async Task<object> ChangePassword(string ARMSessionId, string currentPassword, string updatedpassword)
        {

            var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
            ARMLoginUser loginuserdetails = new ARMLoginUser();
            loginuserdetails.usergroup = dictSession["USERGROUP"];
            loginuserdetails.username = dictSession["USERNAME"];
            loginuserdetails.appname = dictSession["APPNAME"];

            if (loginuserdetails.usergroup.ToLower() == Constants.GROUPTYPES.POWER.ToString().ToLower())
            {
                string password = MD5Hash(currentPassword);
                Dictionary<string, string> config = await _common.GetDBConfigurations(loginuserdetails.appname);
                string connectionString = config["ConnectionString"];
                string dbType = config["DBType"];
                string sql = Constants_SQL.GETPOWERUSER.ToString();
                string[] paramName = { "@username", "@password" };
                DbType[] paramType = { DbType.String, DbType.String };
                object[] paramValue = { loginuserdetails.username, password };
                IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
                SQLResult powerUser = await dbHelper.ExecuteSQLAsync(sql, connectionString, paramName, paramType, paramValue);

                if (powerUser != null && powerUser.data != null && powerUser.data.Rows.Count > 0 && powerUser.data.Rows[0]["Password"].ToString() == password)
                {
                    string oldpassword = MD5Hash(currentPassword);
                    string newpassword = MD5Hash(updatedpassword);
                    //Dictionary<string, string> config1 = await _common.GetDBConfigurations(loginuserdetails.appname);
                    //string connectionString1 = config1["ConnectionString"];
                    string dbType1 = config["DBType"];
                    string sql1 = Constants_SQL.UPDATEPOWERUSERPASSWORD.ToString();
                    string[] paramName1 = { "@newpassword", "@oldpassword" , "@username", "@oldpassword"};
                    DbType[] paramType1 = { DbType.String, DbType.String, DbType.String, DbType.String };
                    object[] paramValue1 = { newpassword, oldpassword, loginuserdetails.username, oldpassword };
                   // IDbHelper dbHelper1 = DBHelper.CreateDbHelper(sql1, dbType1, connectionString1, paramName1, paramType1, paramValue1);
                    SQLResult result = await dbHelper.ExecuteNonQueryAsync(sql1, connectionString, paramName1, paramType1, paramValue1);

                    if (result != null && result.success)
                    {
                        return RESULTS.SUCCESS;
                    }
                    else
                    {
                        return RESULTS.ERROR;
                    }

                }
                else
                {
                    return RESULTS.INVALIDPASSWORD;

                }
            }
            else if (loginuserdetails.usergroup.ToLower() == Constants.GROUPTYPES.EXTERNAL.ToString().ToLower() ||
                     loginuserdetails.usergroup.ToLower() == Constants.GROUPTYPES.INTERNAL.ToString().ToLower())
            {
                ARMUser user = await GetARMUsers(loginuserdetails.appname, loginuserdetails.username, currentPassword, loginuserdetails.usergroup);

                if (user != null)
                {
                    string newPassword = updatedpassword;
                    user.password = newPassword;
                    _context.Update(user);
                    var updated = await _context.SaveChangesAsync();
                    if (updated > 0)
                    {
                        return RESULTS.SUCCESS;
                    }
                    else
                    {
                        return RESULTS.ERROR;
                    }
                }
                else
                {
                    return RESULTS.ERROR;
                }
            }
            else
            {
                loginuserdetails.password = updatedpassword;
                _context.Update(loginuserdetails);
                var updated = await _context.SaveChangesAsync();
                if (updated > 0)
                {
                    return RESULTS.SUCCESS;
                }
                else
                {
                    return RESULTS.INVALIDINTERNALENTERNALUSER;
                }
            }

            return ChangePassword;
        }


        public async Task<SQLResult> GetPowerUsersDetails(string username, string appname, string usergroup, string email)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.GETFORGETPASSWORDPOWERUSER.ToString();
            string[] paramName = { "@username","@email" };
            DbType[] paramType = { DbType.String,DbType.String };
            object[] paramValue = { username,email };
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            return await dbHelper.ExecuteSQLAsync(sql, connectionString, paramName, paramType, paramValue);
        }
        public async Task<ARMUser> GetARMUsersDetails(string appName, string username, string usergroup, string email)
        {
            return await _context.ARMUsers.FirstOrDefaultAsync(f => f.email.ToLower() == email.ToLower() && f.username.ToLower() == username.ToLower() && f.appname.ToLower() == appName.ToLower() && f.usergroup.ToLower() == usergroup.ToLower());
        }


        public async Task<SQLResult> UpdatePasswordDetails(string username, string appname,string email)
        {
            string otp = _notification.GenerateOTP();
            string MD5OTP = MD5Hash(otp);
            var response = await _notification.SendEmailOTP(email, otp, username);
            Dictionary<string, string> config = await _common.GetDBConfigurations(appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.UPDATEFORGOTPASSWORD.ToString();
            string[] paramName = { "@username", "@MD5OTP","@email" };
            DbType[] paramType = { DbType.String, DbType.String,DbType.String };
            object[] paramValue = { username, MD5OTP,email };
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            return await dbHelper.ExecuteSQLAsync(sql, connectionString, paramName, paramType, paramValue);
        }


        //Forget password

        public async Task<object> ForgotPassword(string email, string username, string usergroup, string appname)
        {
            if (usergroup.ToLower() == Constants.GROUPTYPES.POWER.ToString().ToLower())
            {
                SQLResult result = await GetPowerUsersDetails(username, appname, usergroup, email);

                if (result.data.Rows.Count > 0)
                {
                    string otp = _notification.GenerateOTP();
                    string MD5OTP = MD5Hash(otp);
                    var response = await _notification.SendEmailOTP(email, otp, username);
                    Dictionary<string, string> config = await _common.GetDBConfigurations(appname);
                    string connectionString = config["ConnectionString"];
                    string dbType = config["DBType"];
                    string sql = Constants_SQL.UPDATEFORGOTPASSWORD.ToString();
                    string[] paramName = { "@username", "@MD5OTP" };
                    DbType[] paramType = { DbType.String, DbType.String,DbType.String };
                    object[] paramValue = { username, MD5OTP,email };
                    IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
                    return await dbHelper.ExecuteSQLAsync(sql, connectionString, paramName, paramType, paramValue);
                }
                else
                {
                    return RESULTS.INVALIDPOWERUSER;
                }
            }
            else if (usergroup.ToLower() == Constants.GROUPTYPES.EXTERNAL.ToString().ToLower() ||
                     usergroup.ToLower() == Constants.GROUPTYPES.INTERNAL.ToString().ToLower())
                try
                {
                    {

                        ARMUser user = await GetARMUsersDetails(appname, username, usergroup, email);
                        if (user == null)
                        {
                            return RESULTS.INVALIDINTERNALENTERNALUSER;
                        }
                        else
                        {
                            string otp = _notification.GenerateOTP();
                            string MD5OTP = MD5Hash(otp);
                            var response = await _notification.SendEmailOTP(email, otp, username);
                            string newPassword = otp;
                            user.password = newPassword;
                            user.isfirsttime = "T";

                            _context.Update(user);
                            var updated = await _context.SaveChangesAsync();
                            if (updated > 0)
                            {
                                return RESULTS.SUCCESS;
                            }
                            else
                            {
                                return RESULTS.ERROR;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            return RESULTS.SUCCESS;

        }




 

    
        //private async Task<DataTable> GetPowerUserRoles(ARMLoginUser loginUser)
        //{
        //    string connectionString = await _common.GetDBConfiguration(loginUser.appname);
        //    string sql = Constants_SQL.GETPOWERUSERROLES.ToString();
        //    string[] paramName = { "@username" };
        //    NpgsqlDbType[] paramType = { NpgsqlDbType.Varchar };
        //    object[] paramValue = { loginUser.username.ToLower() };
        //    return await _postGres.ExecuteSql(sql, connectionString, paramName, paramType, paramValue);
        //}

        private async Task<DataTable> GetPowerUserRoles(ARMLoginUser loginUser)
        { 
            Dictionary<string, string> config = await _common.GetDBConfigurations(loginUser.appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.GETPOWERUSERROLES.ToString();
            string[] paramName = { "@username" };
            DbType[] paramType = { DbType.String };
            object[] paramValue = { loginUser.username.ToLower() };
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            return await dbHelper.ExecuteQueryAsync(sql, connectionString, paramName, paramType, paramValue);
        }
        public async Task<List<string>> GetPowerUserRoleList(string username, string appname)
        {
            var userRoles = await GetPowerUserRole(username, appname);
            List<string> userRoleslist = userRoles.AsEnumerable().Select(r => r["USERROLES"].ToString()).ToList();
            return userRoleslist;
        }
        private async Task<DataTable> GetPowerUserRole(string username, string appname)
        {
             //string connectionString = await _common.GetDBConfiguration(appname);
            string sql = Constants_SQL.GETPOWERUSERROLES.ToString();
            Dictionary<string, string> config = await _common.GetDBConfigurations(appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            //ParamsDetails roles = new ParamsDetails();

            //roles.ParamsNames = new List<ConnectionParamsList>();
            //roles.ParamsNames.Add(new ConnectionParamsList
            //{
            //    Name = "@username",
            //    Type = NpgsqlDbType.Varchar,
            //    Value = username.ToLower(),
            //});
            string[] paramName = { "@username" };
            DbType[] paramType = { DbType.String };
            object[] paramValue = { username.ToLower() };
            //return await _postGres.ExecuteSql(sql, connectionString, paramName, paramType, paramValue);
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            return await dbHelper.ExecuteQueryAsync(sql, connectionString, paramName, paramType, paramValue);
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
        public bool UserGroupExists(string userGroupName)
        {
            var userGroup = _context.ARMUserGroups.FirstOrDefault(p => p.Name.ToLower() == userGroupName.ToLower());
            if (userGroup == null)
            {
                return false;
            }
            return true;
        }
        public bool UserExists(string userName, string appName, string usergroup)
        {
            var user = _context.ARMUsers.FirstOrDefault(f => f.username.ToLower() == userName.ToLower() && f.appname.ToLower() == appName.ToLower() && f.usergroup.ToLower() == usergroup.ToLower());
            if (user == null)
            {
                return false;
            }
            return true;
        }
        public bool SSOUserExists(string userName, string appName)
        {
            var user = _context.ARMUsers.FirstOrDefault(f => f.email.ToLower() == userName.ToLower() && f.appname.ToLower() == appName.ToLower());
            if (user == null)
            {
                return false;
            }
            return true;
        }
        public bool UserEmailExists(string userEmail, string appname)
        {
            var user = _context.ARMUsers.FirstOrDefault(f => f.email.ToLower() == userEmail.ToLower() && f.appname.ToLower() == appname.ToLower());
            if (user == null)
            {
                return false;
            }
            return true;
        }
        public bool AppExists(string appName)
        {
            var app = _context.ARMApps.FirstOrDefault(p => p.AppName.ToLower() == appName.ToLower());
            if (app == null)
            {
                return false;
            }
            return true;
        }
        public async Task<ARMUserGroup> GetUserGroup(string userGroupName)
        {
            return await _context.ARMUserGroups.FirstOrDefaultAsync(p => p.Name.ToLower() == userGroupName.ToLower());

        }

        public async Task<bool> SaveUserRegistrationDetails(ARMUser user, ARMUserGroup userGroup, string otp, string regId)
        {
            var registrationDetails = new
            {
                otpattemptsleft = 3,
                otp = otp,
                appname = user.appname,
                username = user.username,
                email = user.email,
                password = user.password,
                usergroup = userGroup.Name,
                usergroupid = userGroup.ID,
                grouptype = userGroup.GroupType,
                mobileno = user.mobileno,
            };

            try
            {
                string key = $"{REDIS_PREFIX.ARMADDUSER.ToString()}_{regId}";
                await _redis.StringSetAsync(key, JsonConvert.SerializeObject(registrationDetails));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<string> GetInternalUserDetails(ARMUserGroup userGroup, string userId)
        {
            var validationResult = await _api.POSTData(userGroup.InternalAuthUrl, userGroup.InternalAuthRequest.Replace("{{userid}}", userId), "application/json");
            //var jObj = JObject.Parse(validationResult.result.GetType().GetProperty("message").GetValue(validationResult.result,null).ToString());
            var jObj = JObject.Parse(validationResult.result["message"].ToString());
            var rowData = jObj["result"][0]["result"]["row"][0];
            return JsonConvert.SerializeObject(rowData);

        }
        public async Task<string> ValidateAxpertConnect(ARMAxpertConnect axpert)
        {
            var privateKey = _config["ARM_PrivateKey"];
            string hashedKey = MD5Hash(privateKey + axpert.AxSessionId);
            if (hashedKey == axpert.Key)
            {
                string sessionId = $"{REDIS_PREFIX.ARMSESSION.ToString()}-{Guid.NewGuid().ToString()}";
                await _redis.HashSetAsync(sessionId, SESSION_DATA.USERNAME.ToString(), axpert.User);
                await _redis.HashSetAsync(sessionId, SESSION_DATA.AXPERT_SESSIONID.ToString(), axpert.AxSessionId);
                await _redis.HashSetAsync(sessionId, SESSION_DATA.APPNAME.ToString(), axpert.AppName);
                return sessionId;
            }
            return RESULTS.ERROR.ToString();
        }
        public async Task<bool> ValidateGoogleSSO(string ssodetailTokenId, string userid, string subId)
        {

            ARMResult resultData;
            string url = "https://oauth2.googleapis.com/tokeninfo?access_token={{acesstoken}}";
            string googleAuthUrl = url.Replace("{{acesstoken}}", ssodetailTokenId);
            var googleUrlResult = await _api.GetData(googleAuthUrl);
            try
            {
                var jObj = JObject.Parse(googleUrlResult?.result["message"]?.ToString());
                string email = jObj["email"].ToString();
                string sub = jObj["sub"].ToString();
                if (userid == jObj["email"]?.ToString() && subId == jObj["sub"].ToString())
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                resultData = new ARMResult(false, ex.Message);
                return false;
            }



        }
        public async Task<bool> AddSSOUser(ARMUser user)
        {
            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;


        }
        public bool BiometricEnabled(string appName)
        {
            var app = _context.ARMApps.FirstOrDefault(p => p.AppName.ToLower() == appName.ToLower());
            if (app == null)
            {
                return false;
            }
            if (app.EnableFingerPrint == false && app.EnablefacialRecognition == false)
            {
                return false;
            }
            return true;
        }
        public async Task<bool> AddARMDevices(UserDevice user)
        {
            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;


        }
        public async Task<bool> UpdateARMDevices(string deviceid, string appname, string username)
        {
            try
            {
                if (DeviceExists(deviceid))
                {
                    await RemoveARMUserDevices(deviceid);
                }
                UserDevice userdevice = new UserDevice();
                var id = Guid.NewGuid();
                userdevice.id = id;
                userdevice.deviceid = deviceid;
                userdevice.appname = appname;
                userdevice.username = username;
                var userAdded = await AddARMDevices(userdevice);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;


        }

        public bool DeviceExists(string deviceid)
        {
            var app = _context.ARMUserDevices.FirstOrDefault(p => p.deviceid.ToLower() == deviceid.ToLower());
            if (app == null)
            {
                return false;
            }
            return true;
        }

        public async Task<UserDevice> GetUserDevices(string username, string appname, string deviceid)
        {
            return await _context.ARMUserDevices.FirstOrDefaultAsync(p => p.username.ToLower() == username.ToLower() && p.appname == appname && p.deviceid == deviceid);
        }
        public async Task<DataTable> ValidatePowerUserWithPassword(string username, string password, string appname)
        {
            //string connectionString = _config["ConnectionStrings:AxpertDb"];
            //string connectionString = await _common.GetDBConfiguration(appname);
            Dictionary<string, string> config = await _common.GetDBConfigurations(appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string hashedPwd = MD5Hash(password);
            //string sql = "select username,password,active from axusers where lower(username) = @username and password = @password and active = 'T'";
            string sql = Constants_SQL.VALIDATEPOWERUSERSWITHPASSWORD.ToString();
            string[] paramName = { "@username", "@password" };
            DbType[] paramType = { DbType.String, DbType.String };
            object[] paramValue = { username.ToLower(), hashedPwd };
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            return await dbHelper.ExecuteQueryAsync(sql, connectionString, paramName, paramType, paramValue);
            //return await _postGres.ExecuteSql(sql, connectionString, paramName, paramType, paramValue);
        }

        public async Task<DataTable> ValidatePowerUsers(string username, string appname)
        {
            //string connectionString = _config["ConnectionStrings:AxpertDb"];
            Dictionary<string, string> config = await _common.GetDBConfigurations(appname);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = Constants_SQL.VALIDATEPOWERUSERS.ToString();
            string[] paramName = { "@username" };
            DbType[] paramType = { DbType.String };
            object[] paramValue = { username.ToLower() };
            //return await _postGres.ExecuteSql(sql, connectionString, paramName, paramType, paramValue);
            IDbHelper dbHelper = DBHelper.CreateDbHelper(sql, dbType, connectionString, paramName, paramType, paramValue);
            return await dbHelper.ExecuteQueryAsync(sql, connectionString, paramName, paramType, paramValue);
        }

        public async Task<bool> RemoveARMUserDevices(string deviceId)
        {
            try
            {
                var app = _context.ARMUserDevices.FirstOrDefault(p => p.deviceid.ToLower() == deviceId.ToLower());
                var userdevice = await _context.ARMUserDevices.FindAsync(app.id);
                _context.ARMUserDevices.Remove(userdevice);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;


        }

        public async Task<ARMUser> GetARMUsers(string appName, string username, string password, string usergroup)
        {
            return await _context.ARMUsers.FirstOrDefaultAsync(f => f.username.ToLower() == username.ToLower() && f.appname.ToLower() == appName.ToLower() && f.password == password && f.usergroup.ToLower() == usergroup.ToLower());


        }
        public async Task<Dictionary<string, string>> StoreSessionValues(string sessionId, string userRoleslist, string username, string appname, string usergroup, string grouptype, string token)
        {
            Dictionary<string, string> sessionValues = new Dictionary<string, string>();
            sessionValues.Add(SESSION_DATA.USER_ROLES.ToString(), string.Join(",", userRoleslist));
            sessionValues.Add(SESSION_DATA.USERNAME.ToString(), username);
            sessionValues.Add(SESSION_DATA.ARMTOKEN.ToString(), token);
            sessionValues.Add(SESSION_DATA.APPNAME.ToString(), appname);
            sessionValues.Add(SESSION_DATA.USERGROUP.ToString(), usergroup);
            if (sessionValues.Count > 0)
            {
                var hashEntries = sessionValues.Select(pair => new HashEntry(pair.Key, pair.Value)).ToArray();
                await _redis.HashSetEntriesAsync(sessionId, hashEntries);
                return sessionValues;
            }
            return sessionValues;

        }

        public async Task<object> ValidateAndUpdatePassword(string ARMSessionId, string currentPassword, string updatedpassword)
        {
            var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
            var loginuser = await GetARMUsers(dictSession["APPNAME"], dictSession["USERNAME"], currentPassword, dictSession["USERGROUP"]);
            if (loginuser == null)
            {
                return RESULTS.INCORRECTPASSWORD;
            }
            if (loginuser.usergroup.ToLower() == Constants.GROUPTYPES.POWER.ToString().ToLower())
            {
                return RESULTS.POWERUSERNOTALLOWED;
            }

            else

                loginuser.password = updatedpassword;
            _context.Update(loginuser);
            var updated = await _context.SaveChangesAsync();
            if (updated > 0)
            {
                return RESULTS.SUCCESS;
            }

            else
            {
                return RESULTS.ERROR;
            }


        }

       

        public async Task<ARMUser> GetUserProfileDetails(string ARMSessionId)
        {
            var dictSession = await _redis.HashGetAllDictAsync(ARMSessionId);
            var loginuser = await GetUserProfile(dictSession["APPNAME"], dictSession["USERNAME"], dictSession["USERGROUP"]);
            return loginuser;
        }


        private async Task<ARMUser> GetUserProfile(string appName, string username, string usergroup)
        {
            return await _context.ARMUsers.FirstOrDefaultAsync(f => f.username.ToLower() == username.ToLower() && f.appname.ToLower() == appName.ToLower() && f.usergroup.ToLower() == usergroup.ToLower());

        }
       

        public async Task<bool> ResetPassword(string email, string updatedpassword, string appname)
        {   
            var loginuser = await _context.ARMUsers.FirstOrDefaultAsync(p => p.email.ToLower() == email.ToLower() && p.appname.ToLower() == appname.ToLower());
            if (loginuser == null)
            {
                return false;
            }
            else

                loginuser.password = updatedpassword;
            _context.Update(loginuser);
            var updated = await _context.SaveChangesAsync();
            if (updated > 0)
            {
                return true;
            }

            else
            {
                return false;
            }


        }
 

    
    }

}
