using ARM_APIs.Interface;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using StackExchange.Redis;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ARM_APIs.Model
{
    public class ARMTstruct : IARMTstruct
    {
        private readonly IRedisHelper _redis;
        private readonly IPostgresHelper _postGres;
        private readonly IConfiguration _config;
        private readonly Utils _common;

        public ARMTstruct(IRedisHelper redis, IPostgresHelper postGres, IConfiguration config, Utils common)
        {
            _redis = redis;
            _postGres = postGres;
            _config = config;
            _common = common;   
        }

        public async Task<bool> IsValidAxpertConnection(ARMAxpertConnect axpert)
        {
            var axSession = await _redis.HashGetAsync(axpert.ARMSessionId, Constants.SESSION_DATA.AXPERT_SESSIONID.ToString());
            if (axSession == axpert.AxSessionId)
            {
                return true;
            }
            return false;
        }
        public async Task<string> GetTstructSQLQuery(string transId, string field , string sessionid)
        {
            //string connectionString = await _common.GetDBConfigurationBySessionId(sessionid);
            //string selectSql = $"select fldsql  from axpflds a where modeofentry in('accept','select') and lower(tstruct) = @transid and lower(fname) = @field";
            var loginuser = await _redis.HashGetAllDictAsync(sessionid);
            Dictionary<string, string> config = await _common.GetDBConfigurations(loginuser["APPNAME"]);
            string selectSql = Constants_SQL.GETTSTRUCTSQLQUERY.ToString();
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string[] paramName = { "@transid", "@field" };
            object[] paramValue = { transId.ToLower(), field.ToLower() };
            DbType[] paramType = { DbType.String, DbType.String };

            var sql = "";
            //var dt = await _postGres.ExecuteSql(selectSql, connectionString, paramName, paramType, paramValue);
            IDbHelper dbHelper = DBHelper.CreateDbHelper(selectSql, dbType, connectionString, paramName, paramType, paramValue);
            var dt = await dbHelper.ExecuteQueryAsync(selectSql, connectionString, paramName, paramType, paramValue);
            if (dt.Rows.Count > 0)
            {
                sql = dt.Rows[0]["fldsql"].ToString();
            }

            if (string.IsNullOrEmpty(sql))
            {
                return Constants.RESULTS.NO_RECORDS.ToString();
            }
            var paramsList = GetParametersFromSQL(sql);
            await _redis.HashSetAsync(transId.ToUpper(), field.ToUpper(), sql);
            await _redis.HashSetAsync(transId.ToUpper(), $"{field.ToUpper()}-PARAMS", paramsList);
            return $"{sql}~~{paramsList}";
        }

        private string GetParametersFromSQL(string sql)
        {
            var regex = new Regex(@"[:?:](?<Parameter>[\S]+)");
            var matchCollection = regex.Matches(sql);
            var result = matchCollection.Cast<Match>().Select(x => x.Groups["Parameter"].Value).ToList<string>();
            return string.Join(",", result);
        }
        //public async Task<object> GetTstructSQLData(string transId, string field, Dictionary<string, string> sqlParams = null)
        //{
        //    var sql = await _redis.HashGetAsync(transId.ToUpper(), field.ToUpper());
        //    if (string.IsNullOrEmpty(sql))
        //    {
        //        sql = await GetTstructSQLQuery(transId, field);

        //        if (string.IsNullOrEmpty(sql))
        //        {
        //            return Constants.RESULTS.NO_RECORDS.ToString();
        //        }

        //        await _redis.HashSetAsync(transId.ToUpper(), field.ToUpper(), sql);

        //    }
        //}

        private ParamsDetails GetSQLParams(Dictionary<string, string> sqlParams)
        {
            ParamsDetails parameters = new ParamsDetails();
            parameters.ParamsNames = new List<ConnectionParamsList>();

            foreach (var sqlParam in sqlParams)
            {
                parameters.ParamsNames.Add(new ConnectionParamsList
                {
                    Name = "@"+ sqlParam.Key.Split("~")[0],
                    Type = GetNpgsqlDbType(sqlParam.Key.Split("~")[1]),
                    Value = sqlParam.Value
                });
            }
            return parameters;
        }

        public static NpgsqlDbType GetNpgsqlDbType(string type)
        {
            switch (type)
            {
                case "number":
                    return NpgsqlDbType.Integer;
                case "text":
                    return NpgsqlDbType.Varchar;
                case "largetext":
                    return NpgsqlDbType.Text;
                case "select":
                    return NpgsqlDbType.Varchar;
                case "date":
                    return NpgsqlDbType.Date;
                case "timestamp":
                    return NpgsqlDbType.Timestamp;
                case "boolean":
                    return NpgsqlDbType.Boolean;
                case "float":
                    return NpgsqlDbType.Double;
                case "numeric":
                    return NpgsqlDbType.Numeric;
                case "uuid":
                    return NpgsqlDbType.Uuid;
                case "bytea":
                    return NpgsqlDbType.Bytea;
                case "json":
                    return NpgsqlDbType.Json;
                case "jsonb":
                    return NpgsqlDbType.Jsonb;
                case "default":
                    return NpgsqlDbType.Varchar;
            }
            return NpgsqlDbType.Varchar;
        }
    }

   
}
