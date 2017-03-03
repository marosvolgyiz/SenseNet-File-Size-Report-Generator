using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNFileSizeReportGenerator
{
    class Config
    {        
        public static NameValueCollection AppSetting
        {
            get
            {
                return ConfigurationManager.AppSettings;
            }
        }        
        public static string[] EmailList
        {
            get
            {
                return (AppSetting["EmailList"] ?? string.Empty).Trim().Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        public static string[] Extensions
        {
            get
            {
                return (AppSetting["Extensions"] ?? string.Empty).Trim().Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string[] ContentTypes
        {
            get
            {
                return (AppSetting["ContentTypes"] ?? string.Empty).Trim().Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        public static string[] ExtensionsBlackList
        {
            get
            {
                return (AppSetting["ExtensionsBlackList"] ?? string.Empty).Trim().Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        public static string NetworkPath
        {
            get
            {
                return (AppSetting["NetworkPath"] ?? string.Empty).Trim();
            }
        }

        public static double ItemsCountInIteration
        {
            get
            {
                return double.Parse((AppSetting["ItemsCountInIteration"] ?? "1000").Trim());
            }
        }
        public static string InTree
        {
            get
            {
                return (AppSetting["InTree"] ?? "/Root").Trim();
            }
        }
        public static bool SendByMail
        {
            get
            {
                if (EmailList.Length == 0)
                {
                    return false;
                }
                return (AppSetting["SendByMail"] ?? string.Empty).Trim().ToLower()=="true";
            }
        }
        public static bool ExcludePreviews
        {
            get
            {
                return (AppSetting["ExcludePreviews"] ?? string.Empty).Trim().ToLower() == "true";
            }
        }
        public static bool UploadNetworkPath
        {
            get
            {
                if (string.IsNullOrEmpty(NetworkPath))
                {
                    return false;
                }
                return (AppSetting["UploadNetworkPath"] ?? string.Empty).Trim().ToLower() == "true";
            }
        }
        public static string ConnectionString
        {
            get
            {
                return (System.Configuration.ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString + ";Connection Timeout=" + int.Parse(AppSetting["SQLConnectionTimeoutInSeconds"]));
            }
        }
        public static string SMTP
        {
            get
            {
                return (AppSetting["SMTP"] ?? string.Empty).Trim();
            }
        }
        public static string SMTPUser
        {
            get
            {
                return (AppSetting["SMTPUser"] ?? string.Empty).Trim();
            }
        }
        public static string SMTPPassword
        {
            get
            {
                return (AppSetting["SMTPPassword"] ?? string.Empty).Trim();
            }
        }
        public static string EmailFrom
        {
            get
            {
                return (AppSetting["EmailFrom"] ?? string.Empty).Trim();
            }
        }
        public static string SubjectPrefix
        {
            get
            {
                return (AppSetting["SubjectPrefix"] ?? string.Empty).Trim();
            }
        }
        public static string DatabaseName
        {
            get
            {
                return new SqlConnectionStringBuilder(System.Configuration.ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString).InitialCatalog;
            }
        }
        public static string ServerName
        {
            get
            {
                return new SqlConnectionStringBuilder(System.Configuration.ConfigurationManager.ConnectionStrings["SnCrMsSql"].ConnectionString).DataSource;
            }
        }
        public static int SQLCommandTimeoutInSeconds
        {
            get
            {
                return int.Parse(AppSetting["SQLCommandTimeoutInSeconds"]);
            }
        }
    }
}
