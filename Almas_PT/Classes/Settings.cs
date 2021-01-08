using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RIMBA
{
    public class Settings
    {
        public static string ConnectionString
        {
            get
            {
                if (System.Environment.MachineName == "RQ-ASUS101")
                    return Tools.getConnectionString("connDBLocal");
                else
                    return Tools.getConnectionString("connDBLive");
            }
        }

        public static string CompanyName
        {
            get { return Tools.getAppSettingsValue("CompanyName"); }
        }
    }
}