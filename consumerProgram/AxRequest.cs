using Program;

namespace Program
{
    public class AxRequest
    {
        public bool success { get; set; }
        public string? requestid { get; set; }
        public DateTime? requestreceivedtime { get; set; }
        public string? sourcefrom { get; set; }
        public string? requeststring { get; set; }
        public string? headers { get; set; }
        public string? @params { get; set; }
        public string? authz { get; set; }
        public string? contenttype { get; set; }
        public string? contentlength { get; set; }
        public string? host { get; set; }
        public string? url { get; set; }
        public string? endpoint { get; set; }
        public string? requestmethod { get; set; }
        public string? username { get; set; }
        public string? additionaldetails { get; set; }
        public string? sourcemachineip { get; set; }
        public string? apiname { get; set; }
        public string? responseid { get; set; }
        public DateTime? responsesenttime { get; set; }
        public string? responsestring { get; set; }
        public int statuscode { get; set; }

        public string? executiontime { get; set; }
        public string? errordetails { get; set; }
    }


}

public class TokenResult
{
    public string token { get; set; }
    public AxRequest RequestData { get; set; }
    public AxRequest PaymentRequestData { get; set; }
    public AxRequest PaymentProcessData { get; set; }

    public AxRequest DocumentUpload { get; set; }
}
