namespace ARMCommon.Helpers
{
    public class Constants
    {

        public const string KeyForAxpertToken = "202308231803";
        #region Redis-Prefixes
        public const string sqlPrefix = "ARMSQL-";
        public const string dataPrefix = "ARMDS-";
        public const string validityPrefix = "ARMDS-Validity-";
        public const string metaDataPrefix = "ARMMETADATA-";
        public const string xmlPrefix = "ARMXML-";
        public const string structPrefix = "ARMSTRUCT-";
        #endregion

        #region ARM-Prefixes
        public const string SuccessText = "Success";
        public const string ErrorText = "Error";
        public const string RegistrationSuccess = "User added successfully";
        public const string INVALIDDATASOURCE = "Required fields (Connection details/Datasources) is missing in the input.";
        public const string INVALIDSESSION = "Invalid Session. Please try again.";
        #endregion

        public enum GROUPTYPES
        {
            POWER,
            INTERNAL,
            EXTERNAL
        };
        public enum SOURCETYPE
        {
            SQL,
            APIDEFINITION
        };
        public enum Roles
        {
            DEFAULT

        }
        public enum AXTASKS
        {
            APPROVE,
            REJECT,
            RETURN,
            FORWARD,
            MAKE,
            CHECK,
            SEND,
            BULKAPPROVE,
            SKIP
        }

        public enum TASKTYPE
        {
            APPROVE,
            MAKE,
            CHECK            
        }
        public enum REDIS_PREFIX
        {
            ARMADDUSER,
            ARMSESSION,
            ARMRedisConfiguration,
            ARMPAGE,
            ARMUPDATEUSERDETAIL,
            AXINLINEFORM,
           



    }

    public enum DB_PREFIX
        {
            ARMConnectionString,

        }

        public enum AXPERT { 
            AXPERT_WEB_URL,
            AXPERT_SCRIPTS_URL
        }

        public enum SESSION_DATA
        {
            USER_ROLES,
            ARMTOKEN,
            USERNAME,
            AXPERT_SESSIONID,
            APPNAME,
            USERGROUP,
            GROUPTYPE,
            CONNECTED_TO_AXPERT

        }

        public enum BIOMETRIC
        {
            FINGERPRINT,
            FACIALRECOGNIZATION

        }
        public enum RESULTS
        {
            INVALIDPASSWORD,
            CHANGEPASSWORD,
            NO_RECORDS,
            NO_TOKEN,
            SUCCESS,
            ERROR,
            INVALID_AXSESSION,
            INCORRECTPASSWORD,
            SAMEPASSWORD,
            POWERUSERNOTALLOWED,
            INSERTED,
            NOKEYAVAILABLEINREDIS,
            INVALIDPOWERUSER,
            INVALIDINTERNALENTERNALUSER,
            RECORDINSERTED,
            RECORDUPDATED
        }
        public enum DBTYPE
        {
            ORACLE,
            POSTGRES
        }
        
        public enum HTTPMETHOD
        {
            HTTPPOST,
            HTTPGET
        }
        public enum KEY
        {
            METADATA
            

        }

        public enum ANALYTICS_PAGES
        {
            ANALYTICS,
            ENTITY,
            ENTITY_FORM
        }

    }
}
