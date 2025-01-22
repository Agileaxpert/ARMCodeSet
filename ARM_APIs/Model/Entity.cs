using System.Data;

namespace ARM_APIs.Model
{
    public class Entity
    {
        public string? Page { get; set; }
        public string? ARMSessionId { get; set; }
        public string? AppName { get; set; }
        public string? EntityName { get; set; }
        public int? PageNo { get; set; }
        public int? PageSize { get; set; }
        public Dictionary<string, string>? GlobalParams { get; set; }
        public bool? MetaData { get; set; }
        public string? TransId { get; set; }
        public string? KeyValue { get; set; }
        public string? Filter { get; set; }
        public string? Fields { get; set; }
        public string? UserName { get; set; }
        public Int64? RecordId { get; set; }
        public Dictionary<string, string>? ViewFilters { get; set; }
        public string? Roles { get; set; }
        public List<string>? PropertiesList { get; set; }
    }

    public class EntityCharts
    {
        public string? ARMSessionId { get; set; }
        public string? AppName { get; set; }
        public string? EntityName { get; set; }
        public string? TransId { get; set; }
        public string? Condition { get; set; }
        public string? Criteria { get; set; }
        public string? KeyValue { get; set; }
        public Int64? RecordId { get; set; }
        public string? UserName { get; set; }
        public Dictionary<string, string>? ViewFilters { get; set; }
    }

    public class EntityForm
    {
        public string? ARMSessionId { get; set; }
        public string? AppName { get; set; }
        public string? EntityName { get; set; }
        public string? TransId { get; set; }
        public string? KeyValue { get; set; }
        public string? UserName { get; set; }
        public bool? SubEntityMetaData { get; set; }
    }

    public class AnalyticsData
    {
        public string? ARMSessionId { get; set; }
        public string? SchemaName { get; set; }
        public string AppName { get; set; }
        public string Page { get; set; }
        public string? TransId { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public List<string>? PropertiesList { get; set; }
        public bool? All { get; set; }
        public string UserName { get; set; }
    }


    public class AnalyticsEntityInput
    {
        public string? ARMSessionId { get; set; }
        public string? SchemaName { get; set; }
        public string AppName { get; set; }
        public string Page { get; set; }
        public string? TransId { get; set; }
        public List<string>? PropertiesList { get; set; }
        public string UserName { get; set; }
        public string Roles { get; set; }
    }

    public class AnalyticsEntityOutput
    {

        public string? TransId { get; set; }
        public string? SelectedEntities { get; set; }
        public DataTable? SelectedEntitiesList { get; set; }
        public DataTable? AllEntitiesList { get; set; }
        public DataTable? MetaData { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public string? Error { get; set; }
    }

    public class AnalyticsCharts
    {
        public string? ARMSessionId { get; set; }
        public string? AppName { get; set; }
        public string? Page { get; set; }
        public string? TransId { get; set; }
        public string? UserName { get; set; }
        public List<ChartMetaData>? ChartMetaData { get; set; }
        public Dictionary<string, string>? ViewFilters { get; set; }
    }

    public class ChartMetaData
    {
        public string? AggField { get; set; }
        public string? GroupField { get; set; }
        public string? AggTransId { get; set; }
        public string? GroupTransId { get; set; }
        public string? AggFunc { get; set; }
    }

    public class EntityListOutput
    {
        public string? TransId { get; set; }
        public DataTable? ListData { get; set; }
        public DataTable? MetaData { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public string? Error { get; set; }
        public int? PageNo { get; set; }
        public int? PageSize { get; set; }
    }


}

