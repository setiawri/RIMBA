using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RIMBA.RO
{
    public partial class Default : System.Web.UI.Page
    {
        protected const string PAGETOPIC = "Permintaan Pembelian";
        protected const string RELATIONNAME = "relation";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                //textbox set to readonly causes value not retained during postback. This is the fix
                Tools.retainTextboxData(Request, txtDate, txtApproveDate);
            }
            else
            {
                populatePage();
            }
        }

        private void populatePage()
        {
            //generate filter
            string filter = "";
            if (chkNeedApprovalOnly.Checked)
                filter = Tools.append(filter, "(RequestOrderLog.[Approved] = 0 AND RequestOrderLog.[Rejected] = 0) AND (RequestOrderLog.ConfirmedWarehouse = 1 OR RequestOrderLog.ConfirmedPPIC = 1)", " AND ");
            filter = Tools.append(filter, string.Format("(RequestOrderLog.RequestOrderID LIKE '%{0}%')", txtNo.Text.Trim()), " AND ");
            if (!string.IsNullOrWhiteSpace(txtDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, RequestOrder.Date, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");
            if (!string.IsNullOrWhiteSpace(txtApproveDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, RequestOrderLog.ValidatedDate, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtApproveDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");

            string sql = string.Format(@"
                SELECT RequestOrder.RequestOrderID, RequestOrder.Date, RequestOrder.UserName, RequestOrder.Note,
                    RequestOrderType.Description
                FROM [DWSystem].RequestOrder
                    LEFT OUTER JOIN [DWSystem].RequestOrderType ON RequestOrderType.TypeID = RequestOrder.TypeID
                WHERE RequestOrder.RequestOrderID IN (  SELECT RequestOrderLog.RequestOrderID 
                                                        FROM [DWSystem].RequestOrderLog 
                                                        WHERE {1}
                                                        ) 
                ORDER BY RequestOrder.RequestOrderID ASC;

                SELECT RequestOrderLog.RequestOrderID, RequestOrderLog.No, RequestOrderLog.ConfirmedWarehouse, RequestOrderLog.ConfirmedWarehouseBy, RequestOrderLog.ConfirmedWarehouseDate, 
                    RequestOrderLog.ConfirmedPPIC, RequestOrderLog.ConfirmedPPICBy, RequestOrderLog.ConfirmedPPICDate, {0} AS FormattedQuantity, ISNULL(RequestOrderLog.[Detail],'-') AS Detail,
                    RequestOrderLog.SalesOrderID, RequestOrderLog.SOInventoryID, SOInventory.InventoryName AS SOInventoryName,
                    RequestOrderLog.Approved, RequestOrderLog.Rejected,
                    Inventory.InventoryID, Inventory.InventoryName AS InventoryName
                FROM [DWSystem].RequestOrderLog
                    LEFT OUTER JOIN [DWSystem].RequestOrder ON RequestOrder.RequestOrderID = RequestOrderLog.RequestOrderID
                    LEFT OUTER JOIN [DWSystem].Inventory ON Inventory.InventoryID = RequestOrderLog.InventoryID
                    LEFT OUTER JOIN [DWSystem].Inventory SOInventory ON SOInventory.InventoryID = RequestOrderLog.SOInventoryID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U1 ON U1.UnitID = Inventory.UnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U2 ON U2.UnitID = Inventory.SecUnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U3 ON U3.UnitID = Inventory.ThirdUnitID
                WHERE {1}
                ORDER BY RequestOrderLog.[No] ASC
            ", DBUtil.getSQLToFormatQty("RequestOrderLog.Quantity"), filter);
            rptParent.DataSource = DBUtil.getData(sql, RELATIONNAME, "RequestOrderID");
            rptParent.DataBind();
            if (rptParent.Items.Count == 0)
                message.display("Search tidak menghasilkan data untuk ditampilkan. Silahkan rubah filter dan coba lagi.");
        }

        protected void lbtnClearNo_Click(object sender, EventArgs e)
        {
            txtNo.Text = "";
        }

        protected void lbtnClearApproveDate_Click(object sender, EventArgs e)
        {
            txtApproveDate.Text = "";
        }

        protected void lbtnClearDate_Click(object sender, EventArgs e)
        {
            txtDate.Text = "";
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            populatePage();
        }

        protected void btnApprove_Command(object sender, CommandEventArgs e)
        {
            try
            {
                string sql = @"
                        UPDATE [DWSystem].[RequestOrderLog] 
                        SET 
                            [ApprovedQuantity] = [Quantity],
                            [Approved] = 1,
                            [Rejected] = 0,
                            [StopRequested] = 0,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [No] = @ItemNo";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@ItemNo", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = Tools.getCookieData<string>(GlobalVariables.COOKIEDATA_USERNAME);

                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                populatePage();
                message.display(PAGETOPIC + " berhasil di approve");
            }
            catch (Exception ex) { message.error(PAGETOPIC + " gagal di approve. Hubungi administrator. Error: " + ex.Message); }
        }

        protected void btnReject_Command(object sender, CommandEventArgs e)
        {
            try
            {
                string sql = @"
                        UPDATE [DWSystem].[RequestOrderLog] 
                        SET 
                            [Cost] = NULL,
                            [ApprovedQuantity] = 0,
                            [Approved] = 0,
                            [Rejected] = 1,
                            [StopRequested] = 0,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [No] = @ItemNo";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@ItemNo", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = Tools.getCookieData<string>(GlobalVariables.COOKIEDATA_USERNAME);

                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                populatePage();
                message.display(PAGETOPIC + " berhasil di reject");
            }
            catch (Exception ex) { message.error(PAGETOPIC + " gagal di reject. Hubungi administrator. Error: " + ex.Message); }
        }

        protected void btnCancelValidation_Command(object sender, CommandEventArgs e)
        {
            try
            {
                string sql = "";
                bool isEligibleToCancel = false;

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                {
                    //confirm eligible to cancel
                    sql = @"
                        DECLARE @iCount as int

                        SELECT @iCount = COUNT(*) 
                        FROM [DWSystem].[RequestOrderLog] L 
                        WHERE 
                            [No] = @ItemNo 
                            AND (
                                [StopRequested] = 1 
                                OR [No] IN(
                                        SELECT [ROLogNo] 
                                        FROM [DWSystem].[RequestOrderLogTaken] 
                                        WHERE [ROLogNo] = @ItemNo
                                )
                            ) 

                        SELECT @iCount = @iCount + COUNT(*) 
                        FROM [DWSystem].[RequestOrderLog] 
                        WHERE [No] = @ItemNo AND [ConfirmedWarehouse] = 0 AND [ConfirmedPPIC] = 0 

                        IF @iCount > 0 
                            SELECT 0
                        ELSE
                            SELECT 1
                    ";
                    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@ItemNo", SqlDbType.VarChar).Value = e.CommandArgument.ToString();

                        if (sqlConnection.State != ConnectionState.Open) sqlConnection.Open();
                        isEligibleToCancel = Convert.ToBoolean(cmd.ExecuteScalar());
                    }

                    //cancel validation
                    if (!isEligibleToCancel)
                        message.error(PAGETOPIC + " tidak valid untuk di cancel.");
                    else
                    {
                        sql = @"
                            UPDATE [DWSystem].[RequestOrderLog] 
                            SET 
                                [Approved] = 0,
                                [Rejected] = 0,
                                [ValidatedDate] = NULL,
                                [ValidatedBy] = NULL,
                                [Web] = NULL
                            WHERE [No] = @ItemNo";
                        using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@ItemNo", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
                            cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = Tools.getCookieData<string>(GlobalVariables.COOKIEDATA_USERNAME);

                            if (sqlConnection.State != ConnectionState.Open)
                                sqlConnection.Open();
                            cmd.ExecuteNonQuery();
                        }
                        message.display(PAGETOPIC + " berhasil di cancel");
                    }
                }
                populatePage();
            }
            catch (Exception ex) { message.error(PAGETOPIC + " gagal cancel approve/reject. Hubungi administrator. Error: " + ex.Message); }
        }

        protected string formatChildColumn(string columnName)
        {
            return string.Format("['{0}']", columnName);
        }

    }
}