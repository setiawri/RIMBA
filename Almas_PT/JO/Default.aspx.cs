using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RIMBA.JO
{
    public partial class Default : System.Web.UI.Page
    {
        protected const string PAGETOPIC = "Order Produksi";
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
                filter = Tools.append(filter, "ProductionOrder.[Approved] = 0 AND ProductionOrder.[Rejected] = 0", " AND ");
            filter = Tools.append(filter, string.Format("(ProductionOrder.ProductionOrderID LIKE '%{0}%')", txtNo.Text.Trim()), " AND ");
            if (!string.IsNullOrWhiteSpace(txtDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, ProductionOrder.Date, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");
            if (!string.IsNullOrWhiteSpace(txtApproveDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, ProductionOrder.ValidatedDate, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtApproveDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");

            string sql = string.Format(@"
                SELECT ProductionOrder.ProductionOrderID, ProductionOrder.Date, ProductionOrder.UserName, ProductionOrder.Approved, ProductionOrder.Rejected,
                    ProductionOrderItems.QuantityCount AS QuantityCount, InventoryPicture.Picture, InventoryPicture.Filename
                FROM [DWSystem].ProductionOrder
                    LEFT OUTER JOIN (
                            SELECT ProductionOrderLog.ProductionOrderID, SUM(ProductionOrderLog.Quantity) AS QuantityCount
                            FROM [DWSystem].ProductionOrderLog
                            GROUP BY ProductionOrderLog.ProductionOrderID
                        ) ProductionOrderItems ON ProductionOrderItems.ProductionOrderID = ProductionOrder.ProductionOrderID
                    LEFT OUTER JOIN (
                            SELECT ProductionOrderLog.ProductionOrderID, ProductionOrderLog.InventoryID, Inventory.Picture, Inventory.Filename, row_number() over (partition by ProductionOrderLog.ProductionOrderID order by ProductionOrderLog.InventoryID) AS RowNumber
                            FROM [DWSystem].ProductionOrderLog
                                LEFT OUTER JOIN [DWSystem].ProductionOrder ON ProductionOrder.ProductionOrderID = ProductionOrderLog.ProductionOrderID
                                LEFT OUTER JOIN [DWSystem].Inventory ON Inventory.InventoryID = ProductionOrderLog.InventoryID
                            WHERE ProductionOrder.[Approved] = 0 AND ProductionOrder.[Rejected] = 0 AND Inventory.Picture IS NOT NULL
                        ) InventoryPicture ON InventoryPicture.ProductionOrderID = ProductionOrder.ProductionOrderID AND RowNumber=1
                WHERE {1}
                ORDER BY ProductionOrder.[Date] ASC, ProductionOrder.[ProductionOrderID] ASC;
                
                SELECT ProductionOrderLog.ProductionOrderID, {0} AS FormattedQuantity, 
                    Inventory.InventoryID, Inventory.InventoryName
                FROM [DWSystem].ProductionOrderLog
                    LEFT OUTER JOIN [DWSystem].ProductionOrder ON ProductionOrder.ProductionOrderID = ProductionOrderLog.ProductionOrderID
                    LEFT OUTER JOIN [DWSystem].Inventory ON Inventory.InventoryID = ProductionOrderLog.InventoryID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U1 ON U1.UnitID = Inventory.UnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U2 ON U2.UnitID = Inventory.SecUnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U3 ON U3.UnitID = Inventory.ThirdUnitID
                WHERE {1}
                ORDER BY ProductionOrderLog.[No] ASC
            ", DBUtil.getSQLToFormatQty("ProductionOrderLog.Quantity"), filter);
            rptParent.DataSource = DBUtil.getData(sql, RELATIONNAME, "ProductionOrderID");
            rptParent.DataBind();
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
                        UPDATE [DWSystem].[ProductionOrder] 
                        SET 
                            [Approved] = 1,
                            [Rejected] = 0,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [ProductionOrderID] = @ProductionOrderID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@ProductionOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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
                        UPDATE [DWSystem].[ProductionOrder] 
                        SET 
                            [Approved] = 0,
                            [Rejected] = 1,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [ProductionOrderID] = @ProductionOrderID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@ProductionOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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

                        DECLARE @iCount as int, @isApproved as bit, @isRejected as bit

                        SELECT @isApproved = ProductionOrder.Approved
                        FROM [DWSystem].ProductionOrder

                        SELECT @isRejected = ProductionOrder.Rejected
                        FROM [DWSystem].ProductionOrder

                        IF @isApproved = 1
                        BEGIN
                            SELECT @iCount = COUNT(*) 
                            FROM 
                                [DWSystem].[ProductionOrder] PO
                                ,[DWSystem].[ProductionOrderLog] L 
                            WHERE PO.[ProductionOrderID] = L.[ProductionOrderID] 
                                AND PO.[ProductionOrderID] = @ProductionOrderID 
                                AND (
                                    PO.[Rejected] = 1 
                                    OR L.[StopProducted] = 1 
                                )

                            IF @iCount > 0 
                                SELECT 0
                            ELSE
                                SELECT 1
                        END
                        ELSE IF @isRejected = 1
                        BEGIN
                            
                            SELECT @iCount = COUNT(*) 
                            FROM 
                                [DWSystem].[ProductionOrder] PO
                                ,[DWSystem].[ProductionOrderLog] L 
                            WHERE 
                                PO.[ProductionOrderID] = L.[ProductionOrderID] 
                                AND PO.[ProductionOrderID] = @ProductionOrderID 
                                AND (
                                        PO.[Approved] = 1 
                                        OR L.[StopProducted] = 1 
                                        OR PO.[ProductionOrderID] IN (
                                                SELECT [ProductionOrderID] 
                                                FROM [DWSystem].[BBKProductionOrder] 
                                                WHERE 
                                                    [ProductionOrderID] = @ProductionOrderID
                                            ) 
                                        OR L.[ProductionOrderID] IN (
                                                SELECT [ProductionOrderID] 
                                                FROM [DWSystem].[BBMProductionOrder] 
                                                WHERE [ProductionOrderID] = @ProductionOrderID
                                            )
                                    ) 

                            IF @iCount > 0 
                                SELECT 0
                            ELSE
                                SELECT 1

                        END
                        ELSE
                            SELECT 0
                    ";
                    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@ProductionOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();

                        if (sqlConnection.State != ConnectionState.Open) sqlConnection.Open();
                        isEligibleToCancel = Convert.ToBoolean(cmd.ExecuteScalar());
                    }

                    //cancel validation
                    if (!isEligibleToCancel)
                        message.error(PAGETOPIC + " tidak valid untuk di cancel.");
                    else
                    {
                        sql = @"
                            UPDATE [DWSystem].[ProductionOrder] 
                            SET 
                                [Approved] = 0,
                                [Rejected] = 0,
                                [ValidatedDate] = NULL,
                                [ValidatedBy] = NULL,
                                [Web] = NULL
                            WHERE [ProductionOrderID] = @ProductionOrderID";
                        using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@ProductionOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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

        protected string getImageSource(object image, string filename)
        {
            if (image != DBNull.Value)
                return string.Format("data:image/{1};base64,{0}", Convert.ToBase64String((Byte[])image), filename.Substring(filename.LastIndexOf('.')));
            else
                return string.Empty;
        }
    }
}