namespace ARMCommon.Model
{
    public class ARMAxpertConnect
    {
        public string? Key { get; set; }
        public string? AxSessionId { get; set; }
        public string? ARMSessionId { get; set; }
        public string? User { get; set; }
        public string? AppName { get; set; }
        public string? Password { get; set; }
        public string? Transid { get; set; }
        public string? Field { get; set; }
        public List<string>? Datasources { get; set; }

        public Dictionary<string, string>? SqlParams { get; set; }
        public ARMAxpertConnect(string axSessionId, string armSessionId)
        {
            AxSessionId = axSessionId;
            ARMSessionId = armSessionId;
        }
    }
}
