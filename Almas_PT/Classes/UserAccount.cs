using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Security;
using System.Data;
using System.Data.SqlClient;

namespace RIMBA
{
    public class UserAccount
    {
        private const int SALT_LENGTH = 10;
        public const string PASSWORD_REQUIREMENTS = "Password must be at least 6 characters";

        private const string COL_DB_USERNAME = "UserName";
        private const string COL_DB_PASSWORD = "Password";

        private const string COL_PURCHASEORDERVALIDATION = "PurchasingOrderValidation";
        private const string COL_JOBORDERVALIDATION = "ProductionOrderValidation";
        private const string COL_REQUESTORDERVALIDATION = "RequestValidation";
        private const string COL_SALESORDERVALIDATION = "SalesOrderValidation";
        private const string COL_SALESINVOICEVALIDATION = "SellingValidation";
        private const string COL_PURCHASEINVOICEVALIDATION = "PurchasingValidation";

        /*******************************************************************************************************/
        #region PUBLIC VARIABLES

        public string Username = "";
        private string HashedPassword = "";
        public bool CanApprovePO = false;
        public bool CanApproveRO = false;
        public bool CanApproveJO = false;
        public bool CanApproveSO = false;
        public bool CanApproveSI = false;
        public bool CanApprovePI = false;

        public static bool IsAuthenticated
        {
            get
            {
                if (!HttpContext.Current.Request.IsAuthenticated)
                {
                    FormsAuthentication.SignOut();
                    return false;
                }

                return true;
            }
        }

        #endregion
        /*******************************************************************************************************/
        #region CONSTRUCTOR METHODS
        
        public UserAccount(string username)
        {
            DataRow row = get(username) ?? null;
            if(row != null)
            {
                Username = username;
                HashedPassword = Tools.parseData<string>(row, COL_DB_PASSWORD);
                CanApproveRO = Tools.parseData<bool>(row, COL_REQUESTORDERVALIDATION);
                CanApprovePO = Tools.parseData<bool>(row, COL_PURCHASEORDERVALIDATION);
                CanApprovePI = Tools.parseData<bool>(row, COL_PURCHASEINVOICEVALIDATION);
                CanApproveJO = Tools.parseData<bool>(row, COL_JOBORDERVALIDATION);
                CanApproveSO = Tools.parseData<bool>(row, COL_SALESORDERVALIDATION);
                CanApproveSI = Tools.parseData<bool>(row, COL_SALESINVOICEVALIDATION);
            }
        }

        #endregion
        /*******************************************************************************************************/

        private DataRow get(string username)
        {
            DataTable datatable = new DataTable();
            using (SqlConnection conn = new SqlConnection(Settings.ConnectionString))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format(@"
	                SELECT [DWSystem].[Operator].*,
                            ISNULL(OperatorPrivilegeTransaction.[{1}],0) AS {1},
                            ISNULL(OperatorPrivilegeProduction.[{2}],0) AS {2},
                            ISNULL(OperatorPrivilegeProduction.[{3}],0) AS {3},           
                            ISNULL(OperatorPrivilegeTransaction.[{4}],0) AS {4},           
                            ISNULL(OperatorPrivilegeInternalOffice.[{5}],0) AS {5},           
                            ISNULL(OperatorPrivilegeInternalOffice.[{6}],0) AS {6}                  
                    FROM [DWSystem].[Operator] 
                        LEFT OUTER JOIN [DWSystem].[OperatorPrivilegeTransaction] ON OperatorPrivilegeTransaction.{0} = @{0}
                        LEFT OUTER JOIN [DWSystem].[OperatorPrivilegeProduction] ON OperatorPrivilegeProduction.{0} = @{0}
                        LEFT OUTER JOIN [DWSystem].[OperatorPrivilegeInternalOffice] ON OperatorPrivilegeInternalOffice.{0} = @{0}
                    WHERE [DWSystem].[Operator].{0} = @{0}
                ", COL_DB_USERNAME, 
                 COL_PURCHASEORDERVALIDATION,
                 COL_REQUESTORDERVALIDATION,
                 COL_JOBORDERVALIDATION,
                 COL_SALESORDERVALIDATION,
                 COL_SALESINVOICEVALIDATION,
                 COL_PURCHASEINVOICEVALIDATION);
                cmd.Parameters.Add("@" + COL_DB_USERNAME, SqlDbType.VarChar).Value = username;

                datatable = DBUtil.getData(cmd);
            }

            if (datatable.Rows.Count == 0)
                return null;

            return datatable.Rows[0];
        }
        
        public bool isCorrectPassword(string password)
        {
            return hashPassword(password) == HashedPassword;
        }

        public void redirectToOriginalPage()
        {
            string strUserData = Tools.buildCookieData(Username, CanApproveRO.ToString(), CanApprovePO.ToString(), CanApproveJO.ToString(), CanApproveSO.ToString(), CanApproveSI.ToString(), CanApprovePI.ToString());
            FormsAuthenticationTicket objTicket = new FormsAuthenticationTicket(1,
                        Username,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(1 * 30),
                        false,
                        strUserData,
                        FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(objTicket);

            // Create the cookie.
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
        }

        private string hashPassword(string password)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
        }

    }
}