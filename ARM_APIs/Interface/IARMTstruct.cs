using ARM_APIs.Model;
using ARMCommon.Model;
using System.Data;

namespace ARM_APIs.Interface
{
    public interface IARMTstruct
    {
        abstract Task<bool> IsValidAxpertConnection(ARMAxpertConnect axpert);
        abstract Task<string> GetTstructSQLQuery(string transId, string field, string sessionid);
        //abstract Task<object> GetTstructSQLData(ARMAxpertConnect axpert);
    }
}
