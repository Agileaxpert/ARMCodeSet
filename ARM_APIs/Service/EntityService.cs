using ARM_APIs.Interface;
using ARM_APIs.Model;
using ARMCommon.Helpers;
using ARMCommon.Interface;
using ARMCommon.Model;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;

namespace ARM_APIs.Services
{
    public class EntityService : IEntityService
    {
        private readonly DataContext _context;
        private readonly IRedisHelper _redis;
        private readonly IConfiguration _config;
        private readonly IPostgresHelper _postGres;
        private readonly Utils _common;

        public EntityService(DataContext context, IRedisHelper redis, IConfiguration configuration, IPostgresHelper postGres, Utils common)
        {
            _context = context;
            _redis = redis;
            _config = configuration;
            _postGres = postGres;
            _common = common;
        }

        //public async Task<SQLResult> GetEntityListData(Entity entity)
        //{
        //    Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
        //    string connectionString = config["ConnectionString"];
        //    string dbType = config["DBType"];

        //    string sql = Constants_SQL.GET_ENTITYDATA_ANALYTICS.ToString();
        //    if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
        //        sql = Constants_SQL.GET_ENTITYDATA_ANALYTICS_ORACLE.ToString();
        //    string[] paramName = { "@transid", "@fields", "@pagesize", "@pageno" };
        //    DbType[] paramType = { DbType.String, DbType.String, DbType.Int32, DbType.Int32 };
        //    object[] paramValue = { entity.TransId, entity.Fields, entity.PageSize, entity.PageNo };
        //    return await GetDataCustom(dbType, sql, connectionString, paramName, paramType, paramValue);

        //}

        public async Task<SQLResult> GetEntityListData(Entity entity)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = Constants_SQL.GET_ENTITYDATA_ANALYTICS.ToString();
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
                sql = Constants_SQL.GET_ENTITYDATA_ANALYTICS_ORACLE.ToString();
            string[] paramName = { "@transid", "@fields", "@pagesize", "@pageno", "@username" };
            DbType[] paramType = { DbType.String, DbType.String, DbType.Int32, DbType.Int32, DbType.String };
            object[] paramValue = { entity.TransId, entity.Fields, entity.PageSize, entity.PageNo, entity.UserName };

            //TO-DO - REMOVE after DAC in ORACLE
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                Array.Resize(ref paramName, paramName.Length - 1);
                Array.Resize(ref paramType, paramType.Length - 1);
                Array.Resize(ref paramValue, paramValue.Length - 1);
            };

            var dsResult = await GetDataSet(dbType, sql, connectionString, paramName, paramType, paramValue, entity.ViewFilters, entity.GlobalParams);
            return await GetJsonFromDataSet(dsResult);

        }
        //public async Task<SQLResult> GetFilteredEntityListData(Entity entity)
        //{

        //    Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
        //    string connectionString = config["ConnectionString"];
        //    string dbType = config["DBType"];

        //    string sql = Constants_SQL.GET_FILTERED_ENTITYDATA_ANALYTICS.ToString();
        //    if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
        //        sql = Constants_SQL.GET_FILTERED_ENTITYDATA_ANALYTICS_ORACLE.ToString();
        //    string[] paramName = { "@transid", "@fields", "@pagesize", "@pageno", "@filter" };
        //    DbType[] paramType = { DbType.String, DbType.String, DbType.Int32, DbType.Int32, DbType.String, };
        //    object[] paramValue = { entity.TransId, entity.Fields, entity.PageSize, entity.PageNo, entity.Filter };
        //    return await GetDataCustom(dbType, sql, connectionString, paramName, paramType, paramValue);

        //}

        public async Task<SQLResult> GetFilteredEntityListData(Entity entity)
        {

            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = Constants_SQL.GET_FILTERED_ENTITYDATA_ANALYTICS.ToString();
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
                sql = Constants_SQL.GET_FILTERED_ENTITYDATA_ANALYTICS_ORACLE.ToString();
            string[] paramName = { "@transid", "@fields", "@pagesize", "@pageno", "@filter", "@username" };
            DbType[] paramType = { DbType.String, DbType.String, DbType.Int32, DbType.Int32, DbType.String, DbType.String };
            object[] paramValue = { entity.TransId, entity.Fields, entity.PageSize, entity.PageNo, entity.Filter, entity.UserName };

            //TO-DO - REMOVE after DAC in ORACLE
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                Array.Resize(ref paramName, paramName.Length - 1);
                Array.Resize(ref paramType, paramType.Length - 1);
                Array.Resize(ref paramValue, paramValue.Length - 1);
            };

            var dsResult = await GetDataSet(dbType, sql, connectionString, paramName, paramType, paramValue, entity.ViewFilters, entity.GlobalParams);
            return await GetJsonFromDataSet(dsResult);

        }
        public async Task<SQLResult> GetEntityMetaDataFromDB(Entity entity)
        {

            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = Constants_SQL.GET_ENTITYMETADATA_ANALYTICS.ToString();
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
                sql = Constants_SQL.GET_ENTITYMETADATA_ANALYTICS_ORACLE.ToString();
            string[] paramName = { "@transid" };
            DbType[] paramType = { DbType.String };
            object[] paramValue = { entity.TransId };
            return await GetData(dbType, sql, connectionString, paramName, paramType, paramValue);

        }

        public async Task<SQLResult> GetEntityMetaDataV2(Entity entity)
        {
            var result = new SQLResult();
            var keyParams = new string[] { entity.AppName, "AX-UI-MetaData", entity.TransId };
            string rediskey = GenerateRedisKeyString(keyParams);
            var data = await _redis.StringGetAsync(rediskey);

            if (string.IsNullOrEmpty(data))
            {

                result = await GetEntityMetaData(entity);
                if (string.IsNullOrEmpty(result.error))
                {
                    result.data = ExcludeDBColumns(result.data);
                    data = JsonConvert.SerializeObject(result.data);
                    await _redis.StringSetAsync(rediskey, data);

                    return result;
                }
                else
                    return result;
            }

            result.data = JsonConvert.DeserializeObject<DataTable>(data);

            return result;
        }

        public async Task<SQLResult> GetEntityMetaData(Entity entity)
        {
            var result = new SQLResult();
            var keyParams = new string[] { entity.AppName, "AX-MetaData", entity.TransId };
            string rediskey = GenerateRedisKeyString(keyParams);
            var data = await _redis.StringGetAsync(rediskey);

            if (string.IsNullOrEmpty(data))
            {

                result = await GetEntityMetaDataFromDB(entity);
                if (string.IsNullOrEmpty(result.error))
                {
                    data = JsonConvert.SerializeObject(result.data);
                    await _redis.StringSetAsync(rediskey, data);
                    return result;
                }
                else
                    return result;
            }

            result.data = JsonConvert.DeserializeObject<DataTable>(data);

            return result;
        }

        //public async Task<SQLResult> GetEntityChartsData(EntityCharts entity)
        //{

        //    Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
        //    string connectionString = config["ConnectionString"];
        //    string dbType = config["DBType"];

        //    string sql = Constants_SQL.GET_ENTITYCHARTSDATA_ANALYTICS.ToString()
        //    if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
        //        sql = Constants_SQL.GET_ENTITYCHARTSDATA_ANALYTICS_ORACLE.ToString();
        //    string[] paramName = { "@transid", "@condition", "@criteria" };
        //    DbType[] paramType = { DbType.String, DbType.String, DbType.String };
        //    object[] paramValue = { entity.TransId, entity.Condition, entity.Criteria };
        //    var dsResult = await GetDataSet(dbType, sql, connectionString, paramName, paramType, paramValue);
        //    return await GetJsonFromDataSet(dsResult);

        //}
        //
        public async Task<SQLResult> GetEntityChartsData(EntityCharts entity)
        {

            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = "";
            if (entity.Condition.ToUpper() == "GENERAL")
                sql = Constants_SQL.GET_ENTITYCHARTSDATA_GENERAL_ANALYTICS.ToString();
            else
                sql = Constants_SQL.GET_ENTITYCHARTSDATA_CUSTOM_ANALYTICS.ToString();

            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                sql = Constants_SQL.GET_ENTITYCHARTSDATA_ANALYTICS_ORACLE.ToString();
            }

            string[] paramName = { "@transid", "@condition", "@criteria", "@username" };
            DbType[] paramType = { DbType.String, DbType.String, DbType.String, DbType.String };
            object[] paramValue = { entity.TransId, entity.Condition, entity.Criteria, entity.UserName };

            //TO-DO - REMOVE after DAC in ORACLE
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                Array.Resize(ref paramName, paramName.Length - 1);
                Array.Resize(ref paramType, paramType.Length - 1);
                Array.Resize(ref paramValue, paramValue.Length - 1);
            };
            var dsResult = await GetDataSet(dbType, sql, connectionString, paramName, paramType, paramValue, entity.ViewFilters);
            return await GetJsonFromDataSet(dsResult);

        }

        public async Task<SQLResult> GetAnalyticsChartsData(AnalyticsCharts charts)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(charts.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = "";
            var chartsMetaData = charts.ChartMetaData[0];
            string criteria = "";
            if (chartsMetaData.AggField.ToUpper() == "COUNT" && chartsMetaData.GroupField.ToUpper() == "ALL")
                sql = Constants_SQL.GET_ENTITYCHARTSDATA_GENERAL_ANALYTICS.ToString();
            else
            {
                sql = Constants_SQL.GET_ANALYTICSPAGECHARTSDATA_CUSTOM_ANALYTICS.ToString();

                //if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
                //{
                //    sql = Constants_SQL.GET_ENTITYCHARTSDATA_ANALYTICS_ORACLE.ToString(); //TO DO
                //}  

                var entity = new Entity();
                entity.AppName = charts.AppName;
                entity.TransId = charts.TransId;
                var metaData = await GetEntityMetaData(entity);

                var filteredRows = metaData.data.AsEnumerable()
                                       .Where(row => ((row.Field<string>("fldname") == chartsMetaData.AggField && row.Field<string>("ftransid") == chartsMetaData.AggTransId) || (row.Field<string>("fldname") == chartsMetaData.GroupField && row.Field<string>("ftransid") == chartsMetaData.GroupTransId)));

                Dictionary<string, FieldMetadata> metadataFlds = filteredRows.ToDictionary(row => row.Field<string>("fldname"), row => MapDataRowToMetadata(row));

                FieldMetadata aggFld;
                metadataFlds.TryGetValue(chartsMetaData.AggField, out aggFld);

                FieldMetadata groupFld;
                metadataFlds.TryGetValue(chartsMetaData.GroupField, out groupFld);

                string tableName;
                if (aggFld?.Dcname.ToLower() == "dc1")
                    tableName = aggFld.Tablename;
                else if (groupFld?.Dcname.ToLower() == "dc1")
                    tableName = groupFld.Tablename;
                else
                {
                    tableName = metaData.data.AsEnumerable()
                         .First(row => row.Field<string>("dcname") == "dc1")
                         .Field<string>("tablename");
                }

                if (aggFld is null && chartsMetaData.AggField.ToUpper() == "COUNT")
                {
                    aggFld = new FieldMetadata
                    {
                        Fldname = chartsMetaData.AggField,
                        Ftransid = chartsMetaData.AggTransId,
                        Tablename = tableName,
                        Dcname = "dc1",
                    };
                }

                if (groupFld is null && chartsMetaData.GroupField.ToUpper() == "ALL")
                {
                    groupFld = new FieldMetadata
                    {
                        Fldname = "",
                        Ftransid = chartsMetaData.GroupTransId,
                        Tablename = tableName,
                        Dcname = "dc1",
                    };
                }

                string aggGrpFldLink = "";
                if (chartsMetaData.AggTransId == chartsMetaData.GroupTransId && (aggFld.Dcname == groupFld.Dcname && groupFld.Dcname == "dc1"))
                {
                    aggGrpFldLink = "";
                }
                else
                {
                    aggGrpFldLink = $"{aggFld.Tablename}.{tableName}id={groupFld.Tablename}.{tableName}id";
                }

                /* aggfnc~grpfld_transid~groupfld~normalized~srctable~srcfld~allowempty~grpfld_tablename~aggfld_transid~aggfld~aggfld_tablename~grpfld_transid_AND_aggfld_transid_relation*/
                criteria = $"{chartsMetaData.AggFunc}~{groupFld.Ftransid}~{groupFld.Fldname}~{groupFld.Normalized}~{groupFld.Srctable}~{groupFld.Srcfield}~{groupFld.Allowempty}~{groupFld.Tablename}~{aggFld.Ftransid}~{aggFld.Fldname}~{aggFld.Tablename}~{aggGrpFldLink}";

            }

            string[] paramName = { "@transid", "@criteria", "@username" };
            DbType[] paramType = { DbType.String, DbType.String, DbType.String };
            object[] paramValue = { charts.TransId, criteria, charts.UserName };

            //TO-DO - REMOVE after DAC in ORACLE
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                Array.Resize(ref paramName, paramName.Length - 1);
                Array.Resize(ref paramType, paramType.Length - 1);
                Array.Resize(ref paramValue, paramValue.Length - 1);
            };
            var dsResult = await GetDataSet(dbType, sql, connectionString, paramName, paramType, paramValue, charts.ViewFilters);
            return await GetJsonFromDataSet(dsResult);

        }

        public async Task<SQLResult> GetEntityFormMetaData(EntityForm entity)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = Constants_SQL.GET_ENTITYFORMMETADATA.ToString();
            string[] paramName = { "@transid" };
            DbType[] paramType = { DbType.String };
            object[] paramValue = { entity.TransId };
            return await GetData(dbType, sql, connectionString, paramName, paramType, paramValue);
        }

        //public async Task<SQLResult> GetSubEntityListData(Entity entity)
        //{
        //    Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
        //    string connectionString = config["ConnectionString"];
        //    string dbType = config["DBType"];

        //    string sql = Constants_SQL.GET_SUBENTITYDATA_ANALYTICS.ToString();
        //    if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
        //        sql = Constants_SQL.GET_SUBENTITYDATA_ANALYTICS_ORACLE.ToString();
        //    string[] paramName = { "@transid", "@fields", "@recordid", "@pagesize", "@pageno" };
        //    DbType[] paramType = { DbType.String, DbType.String, DbType.Int64, DbType.Int32, DbType.Int32 };
        //    object[] paramValue = { entity.TransId, entity.Fields, entity.RecordId, entity.PageSize, entity.PageNo };
        //    return await GetDataCustom(dbType, sql, connectionString, paramName, paramType, paramValue);
        //}

        public async Task<SQLResult> GetSubEntityListData(Entity entity)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = Constants_SQL.GET_SUBENTITYDATA_ANALYTICS.ToString();

            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
                sql = Constants_SQL.GET_SUBENTITYDATA_ANALYTICS_ORACLE.ToString();
            string[] paramName = { "@transid", "@fields", "@pagesize", "@pageno" };
            DbType[] paramType = { DbType.String, DbType.String, DbType.Int32, DbType.Int32 };
            object[] paramValue = { entity.TransId, entity.Fields, entity.PageSize, entity.PageNo };

            var dsResult = await GetDataSet(dbType, sql, connectionString, paramName, paramType, paramValue, entity.ViewFilters);
            return await GetJsonFromDataSetForSubEntityListing(dsResult, Convert.ToInt32(entity.PageSize), Convert.ToInt32(entity.PageNo));
        }

        public async Task<SQLResult> GetSubEntityMetaData(Entity entity)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = Constants_SQL.GET_SUBENTITYMETADATA_ANALYTICS.ToString();
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
                sql = Constants_SQL.GET_SUBENTITYMETADATA_ANALYTICS_ORACLE.ToString();
            string[] paramName = { "@transid" };
            DbType[] paramType = { DbType.String };
            object[] paramValue = { entity.TransId };
            return await GetData(dbType, sql, connectionString, paramName, paramType, paramValue);
        }

        //public async Task<SQLResult> GetSubEntityChartsData(EntityCharts entity)
        //{
        //    Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
        //    string connectionString = config["ConnectionString"];
        //    string dbType = config["DBType"];

        //    //string sql = Constants_SQL.GET_SUBENTITYCHARTSDATA.ToString();
        //    string sql = Constants_SQL.GET_SUBENTITYCHARTSDATA_ANALYTICS.ToString();
        //    if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
        //        sql = Constants_SQL.GET_SUBENTITYCHARTSDATA_ANALYTICS_ORACLE.ToString();
        //    string[] paramName = { "@transid", "@recordid", "@keyvalue", "@condition", "@criteria" };
        //    DbType[] paramType = { DbType.String, DbType.Int64, DbType.String, DbType.String, DbType.String };
        //    object[] paramValue = { entity.TransId, entity.RecordId, entity.KeyValue, entity.Condition, entity.Criteria };
        //    return await GetDataCustom(dbType, sql, connectionString, paramName, paramType, paramValue);
        //}

        public async Task<SQLResult> GetSubEntityChartsData(EntityCharts entity)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];

            string sql = Constants_SQL.GET_SUBENTITYCHARTSDATA_ANALYTICS.ToString();
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
                sql = Constants_SQL.GET_SUBENTITYCHARTSDATA_ANALYTICS_ORACLE.ToString();

            string[] paramName = { "@transid", "@condition", "@criteria", "@username" };
            DbType[] paramType = { DbType.String, DbType.String, DbType.String, DbType.String };
            object[] paramValue = { entity.TransId, entity.Condition, entity.Criteria, entity.UserName };

            //TO-DO - REMOVE after DAC in ORACLE
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString())
            {
                Array.Resize(ref paramName, paramName.Length - 1);
                Array.Resize(ref paramType, paramType.Length - 1);
                Array.Resize(ref paramValue, paramValue.Length - 1);
            };

            var dsResult = await GetDataSet(dbType, sql, connectionString, paramName, paramType, paramValue);
            return await GetJsonFromDataSet(dsResult);
        }
        public async Task<SQLResult> GetEntityList(Entity entity)
        {
            Dictionary<string, string> config = await _common.GetDBConfigurations(entity.AppName);
            string connectionString = config["ConnectionString"];
            string dbType = config["DBType"];
            string sql = "";
            bool hasDefaultRole = entity.Roles.Split(',').Contains("default");
            string[] roles = entity.Roles.Split(',');
            string rolesList = string.Join(", ", roles.Where(word => !string.IsNullOrWhiteSpace(word)).Select(word => $"'{word}'"));

            if (string.IsNullOrEmpty(entity.EntityName))
                if (hasDefaultRole)
                    sql = Constants_SQL.GET_ALLENTITYLIST_ANALYTICS_DEFAULTROLE.ToString();
                else
                {
                    sql = Constants_SQL.GET_ALLENTITYLIST_ANALYTICS_OTHERROLES.ToString();
                    sql = sql.Replace("$OTHERROLES$", rolesList);
                }
            else
            {

                string[] entities = entity.EntityName.Split(',');
                string entityList = string.Join(", ", entities.Where(word => !string.IsNullOrWhiteSpace(word)).Select(word => $"'{word}'"));
                if (hasDefaultRole)
                    sql = Constants_SQL.GET_FILTERED_ENTITYLIST_ANALYTICS_DEFAULTROLE.ToString();
                else
                {
                    sql = Constants_SQL.GET_FILTERED_ENTITYLIST_ANALYTICS_OTHERROLES.ToString();
                    sql = sql.Replace("$OTHERROLES$", rolesList);
                }

                sql = sql.Replace("$ENTITYLIST$", entityList);
            }
            return await GetData(dbType, sql, connectionString);
        }


        private DataTable ConvertDataJsonNodesToLowerCase(DataTable dt)
        {
            dt.Columns["data_json"].ReadOnly = false;
            foreach (DataRow row in dt.Rows)
            {
                DataTable newDt = new DataTable();
                var dataList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(row["data_json"].ToString());
                foreach (var item in dataList)
                {
                    if (newDt.Columns.Count == 0)
                    {
                        // Create columns with lowercase names
                        foreach (var key in item.Keys)
                        {
                            newDt.Columns.Add(key.ToLower(), typeof(object));
                        }
                    }

                    // Add the data to the new DataTable
                    DataRow newRow = newDt.NewRow();
                    foreach (var key in item.Keys)
                    {
                        newRow[key.ToLower()] = item[key];
                    }
                    newDt.Rows.Add(newRow);
                }
                row["data_json"] = JsonConvert.SerializeObject(newDt);
            }

            return dt;

        }

        private async Task<SQLResult> GetData(string dbType, string sql, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null)
        {
            IDbHelper dbHelper = DBHelper.CreateDbHelper(dbType);
            return await dbHelper.ExecuteSQLAsync(sql, connectionString, paramName, paramType, paramValue);
        }

        private async Task<SQLResult> GetDataCustom(string dbType, string sql, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
        {
            IDbHelper dbHelper = DBHelper.CreateDbHelper(dbType);
            var dt = await dbHelper.ExecuteSQLAsync(sql, connectionString, paramName, paramType, paramValue);
            if (dbType.ToUpper() == Constants.DBTYPE.ORACLE.ToString() && string.IsNullOrEmpty(dt.error))
                dt.data = ConvertDataJsonNodesToLowerCase(dt.data);
            return dt;
        }

        private async Task<SQLDataSetResult> GetDataSet(string dbType, string sql, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue, Dictionary<string, string>? viewFilters = null, Dictionary<string, string>? globalParams = null)
        {
            IDbHelper dbHelper = DBHelper.CreateDbHelper(dbType);
            return await dbHelper.ExecuteSQLsFromSQLAsync(sql, connectionString, paramName, paramType, paramValue, viewFilters, globalParams);
        }

        private async Task<SQLResult> GetJsonFromDataSet(SQLDataSetResult dsResult)
        {
            var sqlResult = new SQLResult();
            if (dsResult.error != null)
            {
                sqlResult.error = dsResult.error;
            }
            else
            {

                List<Task<string>> jsonTasks = new List<Task<string>>();
                foreach (DataTable dt in dsResult.DataSet.Tables)
                {
                    jsonTasks.Add(Task.Run(() => JsonConvert.SerializeObject(dt)));
                }

                sqlResult.data.Columns.Add("data_json", typeof(string));

                string[] jsonResults = await Task.WhenAll(jsonTasks);
                foreach (string json in jsonResults)
                {
                    sqlResult.data.Rows.Add(json);
                }
            }

            return sqlResult;
        }

        private async Task<SQLResult> GetJsonFromDataSetForSubEntityListing(SQLDataSetResult dsResult, int pageSize, int pageNo)
        {
            var sqlResult = new SQLResult();
            if (dsResult.error != null)
            {
                sqlResult.error = dsResult.error;
            }
            else
            {

                List<Task<DataTable>> dtTasks = new List<Task<DataTable>>();
                foreach (DataTable dt in dsResult.DataSet.Tables)
                {
                    dtTasks.Add(Task.Run(() => MergeSubEntityData(dt)));
                }

                sqlResult.data.Columns.Add("modifiedon", typeof(string));
                sqlResult.data.Columns.Add("data_json", typeof(object));

                DataTable[] dtResults = await Task.WhenAll(dtTasks);
                foreach (DataTable dt in dtResults)
                {
                    sqlResult.data.Merge(dt);
                }

                sqlResult.data = GetPaginatedData(sqlResult.data, "modifiedon", false, pageSize, pageNo);
            }

            return sqlResult;
        }

        private DataTable MergeSubEntityData(DataTable dt)
        {
            var newDt = new DataTable();
            newDt.Columns.Add("modifiedon", typeof(string));
            newDt.Columns.Add("data_json", typeof(object));

            foreach (DataRow dr in dt.Rows)
            {
                DataRow row = newDt.NewRow();
                if (DateTime.TryParse(dr["modifiedon"].ToString(), out DateTime modifiedOn))
                {
                    row["modifiedon"] = modifiedOn.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }
                else
                {
                    row["modifiedon"] = dr["modifiedon"].ToString();
                }

                row["data_json"] = DataRowToJson(dr);
                newDt.Rows.Add(row);
            }

            return newDt;
        }

        private Dictionary<string, object> DataRowToJson(DataRow row)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn column in row.Table.Columns)
            {
                dict[column.ColumnName] = row[column];
            }
            return dict;
        }

        private static DataTable GetPaginatedData(DataTable dt, string sortBy, bool ascending, int pageSize, int pageNumber)
        {
            DataView dv = new DataView(dt);
            dv.Sort = sortBy + (ascending ? " ASC" : " DESC");
            DataTable sortedTable = dv.ToTable();

            var paginatedData = sortedTable.AsEnumerable()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            DataTable paginatedTable = new DataTable();
            paginatedTable.Columns.Add("data_json", typeof(object));

            foreach (var row in paginatedData)
            {
                DataRow newRow = paginatedTable.NewRow();
                newRow["data_json"] = row["data_json"];
                paginatedTable.Rows.Add(newRow);
            }

            return paginatedTable;
        }

        public async Task<APIResult> SetAnalyticsData(AnalyticsData analyticsData)
        {
            APIResult apiResult = new APIResult();
            string hashField = Convert.ToBoolean(analyticsData.All) ? "All" : analyticsData.UserName;

            string prefix = GetPagePrefix(analyticsData.Page);
            if (string.IsNullOrEmpty(prefix))
            {
                apiResult.error = "Invalid page type.";
                return apiResult;
            }

            try
            {
                foreach (var prop in analyticsData.Properties)
                {
                    string redisKey = GenerateRedisKeyString(new string[] { analyticsData.AppName, prefix, analyticsData.TransId, prop.Key });
                    await _redis.HashSetAsync(redisKey, $"{hashField}", prop.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                apiResult.error = $"Save failed. {ex.Message}.";
                return apiResult;
            }

            apiResult.data.Add("success", true);
            apiResult.data.Add("message", "Saved successfully");
            return apiResult;
        }


        public async Task<APIResult> GetAnalyticsData(AnalyticsData analyticsData)
        {
            APIResult apiResult = new APIResult();
            string prefix = GetPagePrefix(analyticsData.Page);
            if (string.IsNullOrEmpty(prefix))
            {
                apiResult.error = "Invalid page type.";
                return apiResult;
            }

            var resultDict = new Dictionary<string, Object>();
            try
            {
                foreach (var prop in analyticsData.PropertiesList)
                {
                    string redisKey = GenerateRedisKeyString(new string[] { analyticsData.AppName, prefix, analyticsData.TransId, prop });
                    var result = await _redis.HashGetAsync(redisKey, analyticsData.UserName);
                    if (result == null || string.IsNullOrEmpty(result))
                    {
                        result = await _redis.HashGetAsync(redisKey, "All");
                    }

                    resultDict.Add(prop, result);
                }
            }
            catch (Exception ex)
            {
                apiResult.error = $"Get failed. {ex.Message}.";
                return apiResult;
            }

            apiResult.data = resultDict;
            return apiResult;
        }

        public async Task<APIResult> GetAnalyticsProperties(string appName, string page, string transId, List<string> propertiesList, string userName)
        {
            APIResult apiResult = new APIResult();
            string prefix = GetPagePrefix(page);
            if (string.IsNullOrEmpty(prefix))
            {
                apiResult.error = "Invalid page type.";
                return apiResult;
            }

            var resultDict = new Dictionary<string, Object>();
            try
            {
                foreach (var prop in propertiesList)
                {
                    string redisKey = GenerateRedisKeyString(new string[] { appName, prefix, transId, prop });
                    var result = await _redis.HashGetAsync(redisKey, userName);
                    if (result == null || string.IsNullOrEmpty(result))
                    {
                        result = await _redis.HashGetAsync(redisKey, "All");
                    }

                    resultDict.Add(prop, result);
                }
            }
            catch (Exception ex)
            {
                apiResult.error = $"Get failed. {ex.Message}.";
                return apiResult;
            }

            apiResult.data = resultDict;
            return apiResult;
        }

        public string GetPagePrefix(string page)
        {
            string prefix = "";
            bool isValidAnalyticsPage = Enum.IsDefined(typeof(Constants.ANALYTICS_PAGES), page.ToUpper());
            if (isValidAnalyticsPage)
            {
                switch (page.ToUpper())
                {
                    case "ANALYTICS":
                        prefix = "AXA";
                        break;
                    case "ENTITY":
                        prefix = "AXE";
                        break;
                    case "ENTITY_FORM":
                        prefix = "AXEF";
                        break;
                    default:
                        break;
                }
            }

            return prefix;
        }

        private string GenerateRedisKeyString(string[] keyNodes)
        {
            string redisKey = "";
            if (keyNodes != null || keyNodes.Length > 0)
            {
                foreach (var keyNode in keyNodes)
                {
                    if (!string.IsNullOrEmpty(keyNode))
                        redisKey += $"-{keyNode}";
                }
            }
            if (redisKey.StartsWith("-"))
                redisKey = redisKey.Substring(1);
            return redisKey;
        }

        private DataTable ExcludeDBColumns(DataTable originalTable)
        {
            List<string> excludeCols = new List<string> { "props", "normalized", "srctable", "srcfield", "srctransid", "allowempty", "datacnd", "entityrelfld", "allowduplicate", "tablename" };
            DataTable filteredTable = new DataTable();

            foreach (DataColumn col in originalTable.Columns)
            {
                if (!excludeCols.Contains(col.ColumnName))
                {
                    filteredTable.Columns.Add(col.ColumnName, col.DataType);
                }
            }

            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow = filteredTable.NewRow();
                foreach (DataColumn col in filteredTable.Columns)
                {
                    newRow[col.ColumnName] = row[col.ColumnName];
                }
                filteredTable.Rows.Add(newRow);
            }

            return filteredTable;
        }
        public async Task<EntityListOutput> GetEntityListPageLoadData(Entity entity)
        {
            var result = new EntityListOutput();
            var metaData = await GetEntityMetaData(entity);
            var properties = await GetAnalyticsProperties(entity.AppName, entity.Page, entity.TransId, entity.PropertiesList, entity.UserName);
            var fieldsStr = "All";
            if (properties != null && properties.data != null && properties.data.Count > 0 && properties.data.ContainsKey("FIELDS"))
            {
                fieldsStr = properties.data["FIELDS"].ToString();
            }

            if (!string.IsNullOrEmpty(fieldsStr) && fieldsStr != "All")
            {
                entity.Fields = GetFieldString(entity.TransId, fieldsStr, metaData.data);
            }
            else
            {
                entity.Fields = "All";
            }


            SQLResult entityList;
            if (string.IsNullOrEmpty(entity.Filter))
                entityList = await GetEntityListData(entity);
            else
                entityList = await GetFilteredEntityListData(entity);

            result.TransId = entity.TransId;
            result.MetaData = metaData.data;
            result.Properties = properties.data;
            result.ListData = entityList.data;
            result.PageNo = entity.PageNo;
            result.PageSize = entity.PageSize;
            return result;
        }

        private string GetFieldString(string transId, string fieldsList, DataTable metaData)
        {
            fieldsList = fieldsList + ",";

            var result = string.Join("^",
            metaData.AsEnumerable()
                .Where(row => fieldsList.IndexOf(row.Field<string>("fldname") + ",") > -1 && row.Field<string>("ftransid") == transId)
                .GroupBy(row => row.Field<string>("tablename"))
                .Select(group => $"{group.Key}=" +
                    string.Join("|", group.Select(row => $"{row.Field<string>("fldname")}~{row.Field<string>("normalized")}~{row.Field<string>("srctable")}~{row.Field<string>("srcfield")}~{row.Field<string>("srcfield")}~{row.Field<string>("allowempty")}"))));

            return result;
        }

        public async Task<AnalyticsEntityOutput> GetAnalyticsPageLoadData(AnalyticsEntityInput analyticsInput)
        {
            var result = new AnalyticsEntityOutput();
            var analyticsData = new AnalyticsData();
            analyticsData.AppName = analyticsInput.AppName;
            analyticsData.SchemaName = analyticsInput.SchemaName;
            analyticsData.UserName = analyticsInput.UserName;
            analyticsData.Page = analyticsInput.Page;
            analyticsData.PropertiesList = new List<string>();
            analyticsData.PropertiesList.Add("Entities");
            var selectedEntites = await GetAnalyticsData(analyticsData); //Get Selected Entities
            var entity = new Entity();
            entity.AppName = analyticsInput.AppName;
            entity.Roles = analyticsInput.Roles;
            if (selectedEntites == null || selectedEntites.data == null || selectedEntites.data.Count == 0 || string.IsNullOrEmpty(selectedEntites.data["Entities"]?.ToString()))
            {
                //Get All Entities List                
                entity.EntityName = "";
                var allEntities = await GetEntityList(entity);
                result.AllEntitiesList = allEntities.data;
            }
            else
            {
                //Get Selected Entities List
                entity.EntityName = selectedEntites.data["Entities"].ToString();
                var selectedEntityList = await GetEntityList(entity);
                if (selectedEntityList == null || selectedEntityList.data == null || selectedEntityList.data.Rows.Count == 0)
                {
                    //Get All Entities List                
                    entity.EntityName = "";
                    var allEntities = await GetEntityList(entity);
                }
                else
                {
                    //Reorder the selected entities
                    var orderArray = entity.EntityName.Split(',');
                    selectedEntityList.data = selectedEntityList.data.AsEnumerable().OrderBy(row => Array.IndexOf(orderArray, row.Field<string>("name"))).CopyToDataTable();

                    result.SelectedEntities = string.Join(",", selectedEntityList.data.AsEnumerable().Select(item => item["name"].ToString()).ToList());
                    result.SelectedEntitiesList = selectedEntityList.data;

                    var transId = selectedEntityList.data.Rows[0][0].ToString();
                    entity.TransId = transId;
                    var metaData = await GetEntityMetaDataV2(entity);
                    result.MetaData = metaData.data;

                    analyticsData.PropertiesList = analyticsInput.PropertiesList;
                    analyticsData.TransId = transId;
                    var properties = await GetAnalyticsData(analyticsData); //Get Selected Entities
                    result.TransId = transId;
                    result.Properties = properties.data;
                }
            }
            return result;
        }

        public async Task<AnalyticsEntityOutput> GetAnalyticsEntityData(AnalyticsEntityInput analyticsInput)
        {
            var result = new AnalyticsEntityOutput();
            var analyticsData = new AnalyticsData();
            analyticsData.AppName = analyticsInput.AppName;
            analyticsData.SchemaName = analyticsInput.SchemaName;
            analyticsData.UserName = analyticsInput.UserName;
            analyticsData.Page = analyticsInput.Page;
            analyticsData.PropertiesList = analyticsInput.PropertiesList;
            analyticsData.TransId = analyticsInput.TransId;

            var properties = await GetAnalyticsData(analyticsData);
            result.TransId = analyticsInput.TransId;
            result.Properties = properties.data;

            var entity = new Entity();
            entity.AppName = analyticsInput.AppName;
            entity.Roles = analyticsInput.Roles;
            entity.TransId = analyticsInput.TransId;
            var metaData = await GetEntityMetaDataV2(entity);
            result.MetaData = metaData.data;

            return result;
        }

        public FieldMetadata MapDataRowToMetadata(DataRow row)
        {
            FieldMetadata metadata = new FieldMetadata();
            foreach (var prop in typeof(FieldMetadata).GetProperties())
            {
                if (row.Table.Columns.Contains(prop.Name))
                {
                    prop.SetValue(metadata, row[prop.Name]?.ToString());
                }
            }
            return metadata;
        }
    }
}
