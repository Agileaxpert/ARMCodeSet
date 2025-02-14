using ARMCommon.Interface;
using ARMCommon.Model;
using Npgsql;
using NpgsqlTypes;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text.RegularExpressions;
using System.Text;
using ARMCommon.ActionFilter;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace ARMCommon.Helpers
{
    public class DBHelper
    {
        public class OracleHelpers : IDbHelper
        {
            private readonly string _query;
            private readonly string _connectionString;
            private readonly string[] _paramName;
            private readonly DbType[] _paramType;
            private readonly object[] _paramValue;


            public OracleHelpers()
            {
            }
            public OracleHelpers(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                _query = query;
                _connectionString = connectionString;
                _paramName = paramName;
                _paramType = paramType;
                _paramValue = paramValue;


            }

            private string ReplaceSqlParameters(string sql, Dictionary<string, string> parameters)
            {
                Regex regex = new Regex(@"\:\w+");

                // Use StringBuilder for efficient string manipulation
                StringBuilder sb = new StringBuilder(sql);

                // Find all parameter matches in the SQL string
                MatchCollection matches = regex.Matches(sql);

                // Iterate over matches in reverse order to avoid index issues
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match match = matches[i];
                    string paramKey = match.Value.Substring(1); // Remove leading ':'

                    // Check if the parameter key exists in the dictionary
                    if (parameters.ContainsKey(paramKey))
                    {
                        // Replace the parameter in SQL with its corresponding value
                        sb.Remove(match.Index, match.Length); // Remove original parameter
                        sb.Insert(match.Index, "'" + parameters[paramKey] + "'"); // Insert replacement value
                    }
                }

                return sb.ToString();
            }

            private void WriteDBLogs(string message)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string logFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "log");
                if (!Directory.Exists(logFolderPath))
                    Directory.CreateDirectory(logFolderPath);
                string filename = Path.Combine(logFolderPath, $"{timestamp}.txt");
                File.WriteAllText(filename, message);
            }

            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                DataTable dataTable = new DataTable();
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();
                    query = query.Replace("@", ":");
                    using (var cmd = new OracleCommand(query, connection))
                    {
                        if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                        {
                            return null;
                        }
                        for (int i = 0; i < paramName.Length; i++)
                        {
                            cmd.Parameters.Add(paramName[i], GetOracleDbType(paramType[i])).Value = paramValue[i];
                        }

                        try
                        {
                            cmd.Prepare();
                            var dr = await cmd.ExecuteReaderAsync();
                            dataTable.Load(dr);
                            ChangeColumnNamesToLowerCase(dataTable);
                            
                        }
                        catch (Exception ex)
                        {
                            dataTable.Rows.Add(ex.Message);
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
                return dataTable;
            }

            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString)
            {
                DataTable dataTable = new DataTable();
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();
                    query = query.Replace("@", ":");
                    using (var cmd = new OracleCommand(query, connection))
                    {
                        try
                        {
                            cmd.Prepare();
                            var dr = await cmd.ExecuteReaderAsync();
                            dataTable.Load(dr);
                        }
                        catch (Exception ex)
                        {
                            dataTable.Rows.Add(ex.Message);
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
                return dataTable;
            }

            public DataTable ExecuteQuery(string query, string connectionString)
            {
                DataTable dataTable = new DataTable();
                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    query = query.Replace("@", ":");
                    using (var cmd = new OracleCommand(query, connection))
                    {
                        try
                        {
                            cmd.Prepare();
                            var dr = cmd.ExecuteReader();
                            dataTable.Load(dr);
                        }
                        catch (Exception ex)
                        {
                            dataTable.Rows.Add(ex.Message);
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
                return dataTable;
            }

            public async Task<SQLResult> ExecuteSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null)
            {
                SQLResult sqlResult = new SQLResult();
                if (query.Trim().EndsWith(";"))
                    query = query.Trim().Substring(0, query.Trim().Length - 1);
                try
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        query = query.Replace("@", ":");
                        using (var cmd = new OracleCommand(query, connection))
                        {
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlResult.error = "Invalid params in SQL";
                                }
                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(paramName[i], GetOracleDbType(paramType[i])).Value = paramValue[i];
                                }

                            }
                            try
                            {
                                cmd.Prepare();
                                var dr = await cmd.ExecuteReaderAsync();
                                sqlResult.data.Load(dr);
                                sqlResult.data = ChangeColumnNamesToLowerCase(sqlResult.data);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;
            }


            public async Task<SQLResult> ExecuteStoredProcedureAsync(string procedureName,string connectionString,string[] paramName,DbType[] paramType,object[] paramValue)
            {
                SQLResult sqlResult = new SQLResult(); // Assuming SQLResult has 'data' as a DataTable and 'error' as a string

                try
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using (var cmd = new OracleCommand(procedureName, connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure; // Indicate it's a stored procedure

                            if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                            {
                                sqlResult.error = "Parameter arrays must have the same length.";
                                return sqlResult;
                            }

                            for (int i = 0; i < paramName.Length; i++)
                            {
                                cmd.Parameters.Add(paramName[i], GetOracleDbType(paramType[i])).Value = paramValue[i];
                            }

                            try
                            {
                                var dr = await cmd.ExecuteReaderAsync();
                                sqlResult.data.Load(dr); // Load data into SQLResult
                                sqlResult.data = ChangeColumnNamesToLowerCase(sqlResult.data);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;
            }



            //This function is used to get an SQL string from a SQL and then call that SQL string to get the result
            public async Task<SQLDataSetResult> ExecuteSQLsFromSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null, Dictionary<string, string>? viewFilters = null, Dictionary<string, string>? globalParams = null)
            {
                SQLDataSetResult sqlDataSetResult = new SQLDataSetResult();
                try
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        query = query.Replace("@", ":");
                        using (var cmd = new OracleCommand(query, connection))
                        {
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlDataSetResult.error = "Invalid params in SQL";
                                }

                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(paramName[i], GetOracleDbType(paramType[i])).Value = paramValue[i];
                                }
                            }

                            var dt = new DataTable();

                            try
                            {
                                cmd.Prepare();
                                var dr = await cmd.ExecuteReaderAsync();
                                dt.Load(dr);
                            }
                            catch (Exception ex)
                            {
                                sqlDataSetResult.error = ex.Message;
                            }

                            finally
                            {
                                await connection.CloseAsync();
                            }

                            if (dt != null && dt.Rows.Count > 0)
                            {

                                List<Task<SQLResult>> sqlTasks = new List<Task<SQLResult>>();

                                foreach (DataRow dataRow in dt.Rows)
                                {
                                    var sql = dataRow[0]?.ToString() ?? "";
                                    if (viewFilters != null && viewFilters.Count > 0)
                                    {
                                        sql = sql.Replace("--axp_filter", $" {viewFilters.Values.First()} ");
                                    }
                                    else
                                    {
                                        sql = sql.Replace("--axp_filter", "");
                                    }

                                    if (globalParams != null && globalParams.Count > 0)
                                    {
                                        sql = ReplaceSqlParameters(sql, globalParams);
                                    }

                                    if (!string.IsNullOrEmpty(sql))
                                    {
                                        sqlTasks.Add(ExecuteSQLAsync(sql, connectionString));
                                    }
                                }

                                var sqlResults = await Task.WhenAll(sqlTasks);

                                for (int i = 0; i < sqlResults.Length; i++)
                                {
                                    if (sqlResults[i].error == null)
                                    {
                                        sqlResults[i].data.TableName = $"Table{i}";
                                        sqlDataSetResult.DataSet.Tables.Add(sqlResults[i].data);
                                    }
                                    //else
                                    //{
                                    //    sqlDataSetResult.error = sqlResults[i].error;
                                    //    sqlDataSetResult.DataSet = new DataSet();
                                    //    break;
                                    //}

                                    //RETURN Error to front end
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    sqlDataSetResult.error = ex.ToString();
                }
                return sqlDataSetResult;
            }

            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString, DBParamsDetails paramsDetails)
            {

                DataTable dt = new DataTable();
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand(query, connection))
                    {
                        try
                        {
                            if (paramsDetails.ParamsNames.Count > 0)
                            {
                                foreach (var item in paramsDetails.ParamsNames)
                                {
                                    command.Parameters.Add(item.Name, GetOracleDbType((DbType)item.Type), item.Value, ParameterDirection.Input);
                                }
                            }
                            command.Prepare();
                            var dr = await command.ExecuteReaderAsync();
                            dt.Load(dr);
                        }
                        catch (Exception e)
                        {
                            dt.Rows.Add(e.Message);
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }

                }
                return dt;
            }

            public async Task<SQLResult> ExecuteQueryAsyncs(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                SQLResult result = new SQLResult();
                result.data = new DataTable();
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();
                    query = query.Replace("@", ":");
                    using (var cmd = new OracleCommand(query, connection))
                    {
                        if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                        {
                            return null;
                        }
                        for (int i = 0; i < paramName.Length; i++)
                        {
                            cmd.Parameters.Add(paramName[i], GetOracleDbType(paramType[i])).Value = paramValue[i];
                        }

                        try
                        {
                            cmd.Prepare();
                            var dr = await cmd.ExecuteReaderAsync();
                            result.data.Load(dr);
                            result.data = ChangeColumnNamesToLowerCase(result.data);
                        }
                        catch (Exception ex)
                        {
                            result.error = ex.Message;
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
                return result;
            }

            public async Task<SQLResult> ExecuteNonQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                SQLResult sqlResult = new SQLResult();
                if (query.Trim().EndsWith(";"))
                    query = query.Trim().Substring(0, query.Trim().Length - 1);
                try
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        query = query.Replace("@", ":");
                        using (var cmd = new OracleCommand(query, connection))
                        {
                            if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                            {
                                sqlResult.error = "Invalid params in SQL";
                            }
                            for (int i = 0; i < paramName.Length; i++)
                            {
                                cmd.Parameters.Add(paramName[i], GetOracleDbType(paramType[i])).Value = paramValue[i];
                            }

                            try
                            {
                                cmd.Prepare();
                                var result = await cmd.ExecuteNonQueryAsync();
                                sqlResult.success = (result == 1);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.ToString();
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;
            }


            private static OracleDbType GetOracleDbType(DbType type)
            {
                switch (type)
                {
                    case DbType.Int32:
                        return OracleDbType.Int32;
                    case DbType.Decimal:
                        return OracleDbType.Decimal;
                    case DbType.String:
                        return OracleDbType.Varchar2;
                    // add more type mappings here as needed
                    default:
                        throw new ArgumentException($"Unsupported parameter type: {type}");
                }
            }

            public DataTable ChangeColumnNamesToLowerCase(DataTable dt)
            {
                if (dt == null)
                {
                    throw new ArgumentNullException(nameof(dt), "DataTable cannot be null.");
                }

                foreach (DataColumn column in dt.Columns)
                {
                    column.ColumnName = column.ColumnName.ToLower();
                }

                return dt;
            }
        }

        public class PostgresHelper : IDbHelper
        {
            private readonly string _query;
            private readonly string _connectionString;
            private readonly string[] _paramName;
            private readonly DbType[] _paramType;
            private readonly object[] _paramValue;

            public PostgresHelper()
            {
            }
            public PostgresHelper(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                _query = query;
                _connectionString = connectionString;
                _paramName = paramName;
                _paramType = paramType;
                _paramValue = paramValue;
            }

            private string ReplaceSqlParameters(string sql, Dictionary<string, string> parameters)
            {
                Regex regex = new Regex(@"\:\w+");

                // Use StringBuilder for efficient string manipulation
                StringBuilder sb = new StringBuilder(sql);

                // Find all parameter matches in the SQL string
                MatchCollection matches = regex.Matches(sql);

                // Iterate over matches in reverse order to avoid index issues
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match match = matches[i];
                    string paramKey = match.Value.Substring(1); // Remove leading ':'

                    // Check if the parameter key exists in the dictionary
                    if (parameters.ContainsKey(paramKey))
                    {
                        // Replace the parameter in SQL with its corresponding value
                        sb.Remove(match.Index, match.Length); // Remove original parameter
                        sb.Insert(match.Index, "'" + parameters[paramKey] + "'"); // Insert replacement value
                    }
                }

                return sb.ToString();
            }

            private void WriteDBLogs(string message)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string logFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Log/SQL");
                if (!Directory.Exists(logFolderPath))
                    Directory.CreateDirectory(logFolderPath);
                string filename = Path.Combine(logFolderPath, $"{timestamp}.txt");
                File.WriteAllText(filename, message);
            }

            //public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            //{
            //    DataTable _dt = new DataTable();
            //    try
            //    {

            //        using (var connection = new NpgsqlConnection(connectionString))
            //        {
            //            await connection.OpenAsync();
            //            using (var cmd = new NpgsqlCommand(query, connection))
            //            {
            //                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
            //                {
            //                    return null;
            //                }

            //                for (int i = 0; i < paramName.Length; i++)
            //                {
            //                    cmd.Parameters.Add(paramName[i], GetNpgsqlDbType(paramType[i])).Value = paramValue[i];
            //                }

            //                cmd.Prepare();
            //                var dr = await cmd.ExecuteReaderAsync();
            //                _dt.Load(dr);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    { 
            //        _dt.Rows.Add(ex.Message);
            //    }

            //    return _dt;
            //}

            public async Task<SQLResult> ExecuteStoredProcedureAsync(string storedProcedure, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null)
            {
                SQLResult sqlResult = new SQLResult();

                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using (var cmd = new NpgsqlCommand(storedProcedure, connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlResult.error = "Invalid parameters in SQL.";
                                    return sqlResult;
                                }

                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(paramName[i], GetNpgsqlDbType(paramType[i])).Value = paramValue[i];
                                }
                            }

                            try
                            {
                                cmd.Prepare();
                                var dr = await cmd.ExecuteReaderAsync();
                                sqlResult.data.Load(dr);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;
            }

            public async Task<SQLResult> ExecuteSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null)
            {
                SQLResult sqlResult = new SQLResult();
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlResult.error = "Invalid params in SQL";
                                }

                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(paramName[i], GetNpgsqlDbType(paramType[i])).Value = paramValue[i];
                                }
                            }

                            try
                            {
                                cmd.Prepare();
                                var dr = await cmd.ExecuteReaderAsync();
                                sqlResult.data.Load(dr);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.Message;
                            }

                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }
                return sqlResult;
            }



            //This function is used to get an SQL string from a SQL and then call that SQL string to get the result
            public async Task<SQLDataSetResult> ExecuteSQLsFromSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null, Dictionary<string, string>? viewFilters = null, Dictionary<string, string>? globalParams = null)
            {
                SQLDataSetResult sqlDataSetResult = new SQLDataSetResult();
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlDataSetResult.error = "Invalid params in SQL";
                                }

                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(paramName[i], GetNpgsqlDbType(paramType[i])).Value = paramValue[i];
                                }
                            }

                            var dt = new DataTable();
                            try
                            {
                                cmd.Prepare();
                                var dr = await cmd.ExecuteReaderAsync();
                                dt.Load(dr);


                            }
                            catch (Exception ex)
                            {
                                sqlDataSetResult.error = ex.Message;
                            }

                            finally
                            {
                                await connection.CloseAsync();
                            }

                            if (dt != null && dt.Rows.Count > 0)
                            {

                                List<Task<SQLResult>> sqlTasks = new List<Task<SQLResult>>();

                                foreach (DataRow dataRow in dt.Rows)
                                {
                                    var sql = dataRow[0]?.ToString() ?? "";
                                    if (viewFilters != null && viewFilters.Count > 0)
                                    {
                                        sql = sql.Replace("--axp_filter", $" {viewFilters.Values.First()} ");
                                    }
                                    else
                                    {
                                        sql = sql.Replace("--axp_filter", "");
                                    }

                                    if (globalParams != null && globalParams.Count > 0)
                                    {
                                        sql = ReplaceSqlParameters(sql, globalParams);
                                    }

                                    if (!string.IsNullOrEmpty(sql))
                                    {
                                        sqlTasks.Add(ExecuteSQLAsync(sql, connectionString));
                                    }
                                }

                                var sqlResults = await Task.WhenAll(sqlTasks);

                                for (int i = 0; i < sqlResults.Length; i++)
                                {
                                    if (sqlResults[i].error == null)
                                    {
                                        sqlResults[i].data.TableName = $"Table{i}";
                                        sqlDataSetResult.DataSet.Tables.Add(sqlResults[i].data);
                                    }
                                    //else
                                    //{
                                    //    sqlDataSetResult.error = sqlResults[i].error;
                                    //    sqlDataSetResult.DataSet = new DataSet();
                                    //    break;
                                    //}
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    sqlDataSetResult.error = ex.ToString();
                }
                return sqlDataSetResult;
            }

            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString, DBParamsDetails paramsDetails)
            {

                DataTable dt = new DataTable();
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        try
                        {
                            if (paramsDetails.ParamsNames.Count > 0)
                            {
                                foreach (var item in paramsDetails.ParamsNames)
                                {
                                    command.Parameters.Add(item.Name, GetNpgsqlDbType((DbType)item.Type)).Value = item.Value;
                                }
                            }
                            command.Prepare();
                            var dr = await command.ExecuteReaderAsync();
                            dt.Load(dr);
                        }
                        catch (Exception e)
                        {
                            dt.Rows.Add(e.Message);
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }

                }
                return dt;
            }


            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString)
            {
                DataTable _dt = new DataTable();
                try
                {

                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            cmd.Prepare();
                            var dr = await cmd.ExecuteReaderAsync();
                            _dt.Load(dr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _dt.Rows.Add(ex.Message);
                }

                return _dt;
            }

            public DataTable ExecuteQuery(string query, string connectionString)
            {
                DataTable _dt = new DataTable();
                try
                {

                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            cmd.Prepare();
                            var dr = cmd.ExecuteReader();
                            _dt.Load(dr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _dt.Rows.Add(ex.Message);
                }

                return _dt;
            }


            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                DataTable _dt = new DataTable();
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();


                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                            {
                                throw new ArgumentException("Parameter arrays length mismatch.");
                            }

                            for (int i = 0; i < paramName.Length; i++)
                            {
                                cmd.Parameters.AddWithValue(paramName[i], GetNpgsqlDbType(paramType[i]), paramValue[i]);
                            }

                            if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                            {
                                cmd.Prepare();
                                using (var dr = await cmd.ExecuteReaderAsync())
                                {
                                    _dt.Load(dr);
                                }
                            }
                            else
                            {
                                await cmd.ExecuteNonQueryAsync();
                                _dt.Columns.Add("Message");
                                _dt.Rows.Add("Query executed successfully.");
                            }
                        }
                    }
                }
                catch (PostgresException ex) when (ex.SqlState == "23505")
                {
                    Console.WriteLine("Duplicate key value violates unique constraint. Returning empty DataTable.");
                    _dt.Columns.Add("Message");
                    _dt.Rows.Add("Duplicate key violation.");
                    return _dt;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error executing query: " + ex.Message);
                    _dt.Columns.Add("Message");
                    _dt.Rows.Add("Error: " + ex.Message);
                    return _dt;
                }

                return _dt;
            }



            public async Task<SQLResult> ExecuteQueryAsyncs(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {

                SQLResult result = new SQLResult();
                result.data = new DataTable();
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                            {
                                result.error = "Invalid parameters in SQL.";
                                return result;
                            }

                            for (int i = 0; i < paramName.Length; i++)
                            {
                                cmd.Parameters.Add(paramName[i], GetNpgsqlDbType(paramType[i])).Value = paramValue[i];
                            }

                            cmd.Prepare();
                            var dr = await cmd.ExecuteReaderAsync();
                            result.data.Load(dr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.error = ex.Message;
                }

                return result;
            }



            public async Task<SQLResult> ExecuteNonQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                SQLResult sqlResult = new SQLResult();
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        using (var cmd = new NpgsqlCommand(query, connection))
                        {
                            if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                            {
                                sqlResult.error = "Invalid params in SQL";
                            }

                            for (int i = 0; i < paramName.Length; i++)
                            {
                                cmd.Parameters.Add(paramName[i], GetNpgsqlDbType(paramType[i])).Value = paramValue[i];
                            }
                            try
                            {
                                cmd.Prepare();
                                var result = await cmd.ExecuteNonQueryAsync();
                                sqlResult.success = (result == 1);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.ToString();
                            }

                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }
                return sqlResult;
            }

            private static NpgsqlDbType GetNpgsqlDbType(DbType type)
            {
                switch (type)
                {
                    case DbType.Int32:
                        return NpgsqlDbType.Integer;
                    case DbType.Int64:
                        return NpgsqlDbType.Numeric;
                    case DbType.Decimal:
                        return NpgsqlDbType.Numeric;
                    case DbType.String:
                        return NpgsqlDbType.Varchar;
                    // add more type mappings here as needed
                    default:
                        throw new ArgumentException($"Unsupported parameter type: {type}");
                }
            }
        }
        public class MSSQLHelper : IDbHelper
        {
            private readonly string _query;
            private readonly string _connectionString;
            private readonly string[] _paramName;
            private readonly DbType[] _paramType;
            private readonly object[] _paramValue;

            public MSSQLHelper()
            {
            }

            private string ReplaceSqlParameters(string sql, Dictionary<string, string> parameters)
            {
                Regex regex = new Regex(@"\@\w+");

                // Use StringBuilder for efficient string manipulation
                StringBuilder sb = new StringBuilder(sql);

                // Find all parameter matches in the SQL string
                MatchCollection matches = regex.Matches(sql);

                // Iterate over matches in reverse order to avoid index issues
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match match = matches[i];
                    string paramKey = match.Value.Substring(1); // Remove leading '@'

                    // Check if the parameter key exists in the dictionary
                    if (parameters.ContainsKey(paramKey))
                    {
                        // Replace the parameter in SQL with its corresponding value
                        sb.Remove(match.Index, match.Length); // Remove original parameter
                        sb.Insert(match.Index, "'" + parameters[paramKey] + "'"); // Insert replacement value
                    }
                }

                return sb.ToString();
            }

            private void WriteDBLogs(string message)
            {
                return;
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string logFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "log");
                if (!Directory.Exists(logFolderPath))
                    Directory.CreateDirectory(logFolderPath);
                string filename = Path.Combine(logFolderPath, $"{timestamp}.txt");
                File.WriteAllText(filename, message);
            }

            public MSSQLHelper(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                _query = query;
                _connectionString = connectionString;
                _paramName = paramName;
                _paramType = paramType;
                _paramValue = paramValue;
            }

            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString)
            {
                DataTable dataTable = new DataTable();
                if (query.Trim().EndsWith(";"))
                    query = query.Trim().Substring(0, query.Trim().Length - 1);
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        if (query.Contains("$SCHEMA$"))
                        {
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            string schemaName = builder.UserID;
                            query = query.Replace("$SCHEMA$", schemaName);
                        }

                        using (var cmd = new SqlCommand(query, connection))
                        {
                            try
                            {
                                var dr = await cmd.ExecuteReaderAsync();
                                dataTable.Load(dr);
                                dataTable = ChangeColumnNamesToLowerCase(dataTable);
                            }
                            catch (Exception ex)
                            {
                                WriteDBLogs(JsonConvert.SerializeObject(ex));
                                dataTable.Rows.Add(ex.Message);
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteDBLogs(JsonConvert.SerializeObject(ex));
                    dataTable.Rows.Add(ex.Message);
                }

                return dataTable;
            }

            //public async Task<NonQueryResult> ExecuteNonQueryAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null)
            //{
            //    var result = new NonQueryResult();
            //    if (query.Trim().EndsWith(";"))
            //        query = query.Trim().Substring(0, query.Trim().Length - 1);
            //    try
            //    {
            //        using (var connection = new SqlConnection(connectionString))
            //        {
            //            await connection.OpenAsync();

            //            if (query.Contains("$SCHEMA$"))
            //            {
            //                var builder = new SqlConnectionStringBuilder(connectionString);
            //                string schemaName = builder.UserID;
            //                query = query.Replace("$SCHEMA$", schemaName);
            //            }

            //            using (var cmd = new SqlCommand(query, connection))
            //            {
            //                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
            //                {
            //                    result.error = "Invalid params in SQL";
            //                }

            //                for (int i = 0; i < paramName.Length; i++)
            //                {
            //                    cmd.Parameters.Add(new SqlParameter(paramName[i], GetSqlDbType(paramType[i])) { Value = paramValue[i] });
            //                }

            //                try
            //                {
            //                    result.count = await cmd.ExecuteNonQueryAsync();
            //                }
            //                catch (Exception ex)
            //                {
            //                    WriteDBLogs(JsonConvert.SerializeObject(ex));
            //                    result.error = ex.Message;
            //                }
            //                finally
            //                {
            //                    await connection.CloseAsync();
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        WriteDBLogs(JsonConvert.SerializeObject(ex));
            //        result.error = ex.Message;
            //    }

            //    return result;
            //}

            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                DataTable dataTable = new DataTable();
                if (query.Trim().EndsWith(";"))
                    query = query.Trim().Substring(0, query.Trim().Length - 1);
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        if (query.Contains("$SCHEMA$"))
                        {
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            string schemaName = builder.UserID;
                            query = query.Replace("$SCHEMA$", schemaName);
                        }

                        using (var cmd = new SqlCommand(query, connection))
                        {
                            if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                            {
                                return null;
                            }
                            for (int i = 0; i < paramName.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter(paramName[i], GetSqlDbType(paramType[i])) { Value = paramValue[i] });
                            }

                            try
                            {
                                var dr = await cmd.ExecuteReaderAsync();
                                dataTable.Load(dr);
                                dataTable = ChangeColumnNamesToLowerCase(dataTable);
                            }
                            catch (Exception ex)
                            {
                                WriteDBLogs(JsonConvert.SerializeObject(ex));
                                dataTable.Rows.Add(ex.Message);
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteDBLogs(JsonConvert.SerializeObject(ex));
                    dataTable.Rows.Add(ex.Message);
                }

                return dataTable;
            }

            public async Task<SQLResult> ExecuteSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null)
            {
                SQLResult sqlResult = new SQLResult();
                if (query.Trim().EndsWith(";"))
                    query = query.Trim().Substring(0, query.Trim().Length - 1);
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        if (query.Contains("$SCHEMA$"))
                        {
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            string schemaName = builder.UserID;
                            query = query.Replace("$SCHEMA$", schemaName);
                        }

                        using (var cmd = new SqlCommand(query, connection))
                        {
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlResult.error = "Invalid params in SQL";
                                }
                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(new SqlParameter(paramName[i], GetSqlDbType(paramType[i])) { Value = paramValue[i] });
                                }
                            }
                            try
                            {
                                var dr = await cmd.ExecuteReaderAsync();
                                sqlResult.data.Load(dr);
                                sqlResult.data = ChangeColumnNamesToLowerCase(sqlResult.data);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error += query;
                                sqlResult.error += JsonConvert.SerializeObject(paramName);
                                sqlResult.error += JsonConvert.SerializeObject(paramValue);
                                sqlResult.error = ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error += query;
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;
            }


            public async Task<SQLDataSetResult> ExecuteSQLsFromSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null, Dictionary<string, string>? viewFilters = null, Dictionary<string, string>? globalParams = null)
            {
                SQLDataSetResult sqlDataSetResult = new SQLDataSetResult();
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        if (query.Contains("$SCHEMA$"))
                        {
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            string schemaName = builder.UserID;
                            query = query.Replace("$SCHEMA$", schemaName);
                        }

                        using (var cmd = new SqlCommand(query, connection))
                        {
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlDataSetResult.error = "Invalid params in SQL";
                                }

                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(new SqlParameter(paramName[i], GetSqlDbType(paramType[i])) { Value = paramValue[i] });
                                }
                            }

                            var dt = new DataTable();

                            try
                            {
                                var dr = await cmd.ExecuteReaderAsync();
                                dt.Load(dr);
                            }
                            catch (Exception ex)
                            {
                                sqlDataSetResult.error += query;
                                sqlDataSetResult.error += JsonConvert.SerializeObject(paramName);
                                sqlDataSetResult.error += JsonConvert.SerializeObject(paramValue);
                                sqlDataSetResult.error += ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                List<Task<SQLResult>> sqlTasks = new List<Task<SQLResult>>();

                                foreach (DataRow dataRow in dt.Rows)
                                {
                                    var sql = dataRow[0]?.ToString() ?? "";
                                    if (viewFilters != null && viewFilters.Count > 0)
                                    {
                                        sql = sql.Replace("--axp_filter", $" {viewFilters.Values.First()} ");
                                    }
                                    else
                                    {
                                        sql = sql.Replace("--axp_filter", "");
                                    }

                                    if (globalParams != null && globalParams.Count > 0)
                                    {
                                        sql = ReplaceSqlParameters(sql, globalParams);
                                    }

                                    if (!string.IsNullOrEmpty(sql))
                                    {
                                        sqlTasks.Add(ExecuteSQLAsync(sql, connectionString));
                                    }
                                }

                                var sqlResults = await Task.WhenAll(sqlTasks);

                                for (int i = 0; i < sqlResults.Length; i++)
                                {
                                    if (string.IsNullOrEmpty(sqlResults[i].error))
                                    {
                                        sqlResults[i].data.TableName = $"Table{i}";
                                        sqlDataSetResult.DataSet.Tables.Add(sqlResults[i].data);
                                    }
                                    else
                                    {
                                        sqlDataSetResult.error += sqlResults[i].error;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlDataSetResult.error += query;
                    sqlDataSetResult.error = ex.ToString();
                }
                return sqlDataSetResult;
            }

            public async Task<DataTable> ExecuteQueryAsync(string query, string connectionString, DBParamsDetails paramsDetails)
            {
                DataTable dt = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    if (query.Contains("$SCHEMA$"))
                    {
                        var builder = new SqlConnectionStringBuilder(connectionString);
                        string schemaName = builder.UserID;
                        query = query.Replace("$SCHEMA$", schemaName);
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        try
                        {
                            if (paramsDetails.ParamsNames.Count > 0)
                            {
                                foreach (var item in paramsDetails.ParamsNames)
                                {
                                    command.Parameters.Add(new SqlParameter(item.Name, GetSqlDbType((DbType)item.Type)) { Value = item.Value });
                                }
                            }
                            var dr = await command.ExecuteReaderAsync();
                            dt.Load(dr);
                        }
                        catch (Exception e)
                        {
                            dt.Rows.Add(e.Message);
                        }
                        finally
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
                return dt;
            }

            public async Task<SQLResult> ExecuteStoredProcedureAsync(string storedProcedure, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null)
            {
                SQLResult sqlResult = new SQLResult();
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        if (storedProcedure.Contains("$SCHEMA$"))
                        {
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            string schemaName = builder.UserID;
                            storedProcedure = storedProcedure.Replace("$SCHEMA$", schemaName);

                        }
                        using (var cmd = new SqlCommand(storedProcedure, connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlResult.error = "Invalid params in SQL";
                                }
                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(new SqlParameter(paramName[i], GetSqlDbType(paramType[i])) { Value = paramValue[i] });
                                }
                            }
                            try
                            {
                                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                                {
                                    adapter.Fill(sqlResult.data);
                                }

                                //sqlResult.data = ChangeColumnNamesToLowerCase(sqlResult.data);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error += storedProcedure;
                                sqlResult.error += JsonConvert.SerializeObject(paramName);
                                sqlResult.error += JsonConvert.SerializeObject(paramValue);
                                sqlResult.error = ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error += storedProcedure;
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;
            }

            public DataTable ChangeColumnNamesToLowerCase(DataTable dt)
            {
                if (dt == null)
                {
                    throw new ArgumentNullException(nameof(dt), "DataTable cannot be null.");
                }

                foreach (DataColumn column in dt.Columns)
                {
                    column.ColumnName = column.ColumnName.ToLower();
                }

                return dt;
            }

            private static SqlDbType GetSqlDbType(DbType type)
            {
                switch (type)
                {
                    case DbType.Int32:
                        return SqlDbType.Int;
                    case DbType.Int64:
                        return SqlDbType.BigInt;
                    case DbType.Decimal:
                        return SqlDbType.Decimal;
                    case DbType.String:
                        return SqlDbType.VarChar;
                    case DbType.DateTime:
                        return SqlDbType.DateTime;
                    default:
                        throw new ArgumentException($"Unsupported parameter type: {type}");
                }
            }



            public async Task<SQLResult> ExecuteQueryAsyncs(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                SQLResult sqlResult = new SQLResult();

                if (query.Trim().EndsWith(";"))
                    query = query.Trim().Substring(0, query.Trim().Length - 1);

                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        if (query.Contains("$SCHEMA$"))
                        {
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            string schemaName = builder.UserID;
                            query = query.Replace("$SCHEMA$", schemaName);
                        }

                        using (var cmd = new SqlCommand(query, connection))
                        {
                            if (paramName != null)
                            {
                                if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                                {
                                    sqlResult.error = "Invalid params in SQL";
                                    return sqlResult;
                                }

                                for (int i = 0; i < paramName.Length; i++)
                                {
                                    cmd.Parameters.Add(new SqlParameter(paramName[i], GetSqlDbType(paramType[i])) { Value = paramValue[i] });
                                }
                            }

                            try
                            {
                                var dr = await cmd.ExecuteReaderAsync();
                                sqlResult.data.Load(dr);
                                sqlResult.data = ChangeColumnNamesToLowerCase(sqlResult.data);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;
            }

            public async Task<SQLResult> ExecuteNonQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
            {
                SQLResult sqlResult = new SQLResult();

                if (query.Trim().EndsWith(";"))
                    query = query.Trim().Substring(0, query.Trim().Length - 1);

                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        if (query.Contains("$SCHEMA$"))
                        {
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            string schemaName = builder.UserID;
                            query = query.Replace("$SCHEMA$", schemaName);
                        }

                        using (var cmd = new SqlCommand(query, connection))
                        {
                            // Check if the lengths of parameters match
                            if (paramName.Length != paramValue.Length || paramName.Length != paramType.Length)
                            {
                                sqlResult.error = "Invalid params in SQL";
                                return sqlResult;  // Return early with error
                            }

                            // Add parameters to the command
                            for (int i = 0; i < paramName.Length; i++)
                            {
                                cmd.Parameters.Add(new SqlParameter(paramName[i], GetSqlDbType(paramType[i])) { Value = paramValue[i] });
                            }

                            try
                            {
                                // Execute the non-query and set the count in SQLResult
                                sqlResult.success = await cmd.ExecuteNonQueryAsync() > 0;  // Set success flag based on result count
                            }
                            catch (Exception ex)
                            {
                                sqlResult.error = ex.Message;
                            }
                            finally
                            {
                                await connection.CloseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sqlResult.error = ex.ToString();
                }

                return sqlResult;  // Ensure this is returned
            }


        }



        public static IDbHelper CreateDbHelper(string query, string dbType, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue)
        {
            switch (dbType.ToLower())
            {
                case "oracle":
                    return new OracleHelpers(query, connectionString, paramName, paramType, paramValue);
                case "postgre":
                    return new PostgresHelper(query, connectionString, paramName, paramType, paramValue);
                case "mssql":
                    return new MSSQLHelper(query, connectionString, paramName, paramType, paramValue);
                default:
                    throw new ArgumentException("Invalid database type");
            }
        }

        public static IDbHelper CreateDbHelper(string dbType)
        {
            switch (dbType.ToLower())
            {
                case "oracle":
                    return new OracleHelpers();
                case "postgre":
                    return new PostgresHelper();
                case "mssql":
                   return new MSSQLHelper();
                default:
                    throw new ArgumentException("Invalid database type");
            }

        }




    }

}


