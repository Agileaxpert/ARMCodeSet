using ARMCommon.Model;
using System.Data;

namespace ARMCommon.Interface
{
    public interface IDbHelper
    {
        Task<DataTable> ExecuteQueryAsync(string query, string connectionString);
        Task<DataTable> ExecuteQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue);
        Task<SQLResult> ExecuteQueryAsyncs(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue);
        Task<SQLResult> ExecuteSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null);

        Task<SQLDataSetResult> ExecuteSQLsFromSQLAsync(string query, string connectionString, string[] paramName = null, DbType[] paramType = null, object[] paramValue = null, Dictionary<string, string>? viewFilters = null, Dictionary<string, string>? globalParams = null);
       
        Task<SQLResult> ExecuteNonQueryAsync(string query, string connectionString, string[] paramName, DbType[] paramType, object[] paramValue);
        Task<DataTable> ExecuteQueryAsync(string query, string connectionString, DBParamsDetails paramsDetails);

       
    }


}
