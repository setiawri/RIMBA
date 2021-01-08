<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RIMBA.RO.Default" %>
<%@ Register src="~/UserControls/Message.ascx" tagname="Message" tagprefix="Custom" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="AJAX" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="page-title"><%= PAGETOPIC %></div>

    <Custom:Message ID="message" runat="server" />

    <table>
        <tr><td>            
            <div class="filter">
                <div class="title">FILTER</div>
                <div>            
                    <asp:UpdatePanel ID="upFilter" runat="server">
                        <ContentTemplate>
                            <table border="0">
                                    <tr><td class="label">Need Approval Only:&nbsp;</td>
                                        <td><asp:CheckBox ID="chkNeedApprovalOnly" Checked="true" runat="server" /></td>
                                    </tr>
                                    <tr><td class="label">No:&nbsp;</td>
                                        <td><asp:TextBox ID="txtNo" CssClass="rounded-textbox input-textbox" runat="server" /></td>
                                        <td><asp:LinkButton ID="lbtnClearNo" OnClick="lbtnClearNo_Click" Text="X" CssClass="clear-textbox" runat="server" /></td>
                                    </tr>
                                    <tr><td class="label">Date:&nbsp;</td>
                                        <td><asp:TextBox ID="txtDate" CssClass="rounded-textbox input-textbox" ReadOnly="true" runat="server" /></td>
                                        <td><asp:LinkButton ID="lbtnClearDate" OnClick="lbtnClearDate_Click" Text="X" CssClass="clear-textbox" runat="server" /></td>
                                        <td><asp:ImageButton ID="ibtnDate" ImageUrl="~/Images/Icons/Calendar_Button.png" CssClass="AJAXCalendarExtender" runat="server" />
                                            <AJAX:CalendarExtender ID="calDate" runat="server" 
                                                    Enabled="True" TargetControlID="txtDate" Format="dd/MM/yyyy"
                                                PopupButtonID="ibtnDate">
                                            </AJAX:CalendarExtender>
                                        </td>
                                    </tr>
                                    <tr><td class="label">Approve Date:&nbsp;</td>
                                        <td><asp:TextBox ID="txtApproveDate" CssClass="rounded-textbox input-textbox" ReadOnly="true" runat="server" /></td>
                                        <td><asp:LinkButton ID="lbtnClearApproveDate" OnClick="lbtnClearApproveDate_Click" Text="X" CssClass="clear-textbox" runat="server" /></td>
                                        <td><asp:ImageButton ID="ibtnApproveDate" ImageUrl="~/Images/Icons/Calendar_Button.png" CssClass="AJAXCalendarExtender" runat="server" />
                                            <AJAX:CalendarExtender ID="calApproveDate" runat="server" 
                                                    Enabled="True" TargetControlID="txtApproveDate" Format="dd/MM/yyyy"
                                                PopupButtonID="ibtnApproveDate">
                                            </AJAX:CalendarExtender>
                                        </td>
                                    </tr>
                            </table>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="submitbutton">
                   <asp:Button ID="btnFilter" Text="FILTER" OnClick="btnFilter_Click" CssClass="rounded-button" runat="server" />
                </div>
            </div>
            </td>
        </tr>
    </table>

    <div class="request-order">
        <div class="container-daftar">
            <div class="daftar">
                <table border="0">
                    <asp:Repeater ID="rptParent" runat="server">
                        <ItemTemplate>
                            <tr><td>
                                <div class="item">
                                    <div class="info oneline">
                                        <div class="col1">
                                            <div class="text-bold">No Request: <%# Eval("RequestOrderID") %></div>
                                            <div>Date: <%# string.Format("{0:dd MMM yyyy}",Eval("Date")) %></div>
                                        </div>
                                        <div class="col2">
                                            <div>Operator: <%# Eval("UserName") %></div>
                                            <div>Jenis: <%# Eval("Description") %></div>
                                        </div>
                                    </div>
                                    <div class="info">
                                        <div>Catatan: <%# Eval("Note") %></div>
                                    </div>
                                    <div class="children-container">
                                        <asp:Repeater ID="rptChild" DataSource='<%# ((System.Data.DataRowView)Container.DataItem).Row.GetChildRows(RELATIONNAME) %>' runat="server">
                                            <ItemTemplate>
                                                <div class="child">
                                                    <div>Kode: <%# Eval("['InventoryID']") %></div>
                                                    <div>Nama Barang: <%# Eval("['InventoryName']") %></div>
                                                    <div>Jumlah: <%# Eval("['FormattedQuantity']") %></div>
                                                    <asp:Panel id="pnlSalesOrder" Visible='<%# !String.IsNullOrWhiteSpace(Eval("[SalesOrderID]").ToString()) %>' runat="server">
                                                        <div>No Sales Order: <%# Eval("['SalesOrderID']") %></div>
                                                        <div>SO - Barang: (<%# Eval("['SOInventoryID']") %>) <%# Eval("['SOInventoryName']") %></div>
                                                    </asp:Panel>
                                                    <div>Keterangan: <%# Eval("['Detail']") %></div>
                                                    <div>Konfirmasi Manager: <%# Eval("['ConfirmedWarehouse']") != DBNull.Value ? (Boolean.Parse(Eval("['ConfirmedWarehouse']").ToString()) ? string.Format("{0} ({1:dd MMM yyyy})", Eval("['ConfirmedWarehouseBy']"), Eval("['ConfirmedWarehouseDate']")) : "No") : "No" %></div>
                                                    <div>Konfirmasi Director: <%# Eval("['ConfirmedPPIC']") != DBNull.Value ? (Boolean.Parse(Eval("['ConfirmedPPIC']").ToString()) ? string.Format("{0} ({1:dd MMM yyyy})", Eval("['ConfirmedPPICBy']"), Eval("['ConfirmedPPICDate']")) : "No") : "No" %></div>
                                                    <asp:Button id="btnApprove" Text="Approve" CssClass="rounded-button" OnCommand="btnApprove_Command" CommandArgument='<%# Eval(formatChildColumn("No")) %>' Visible='<%# Convert.ToBoolean(Eval("[Approved]")) == false && Convert.ToBoolean(Eval("[Rejected]")) == false %>' runat="server" />
                                                    <asp:Button id="btnReject" Text="Reject" CssClass="rounded-button" OnCommand="btnReject_Command" CommandArgument='<%# Eval(formatChildColumn("No")) %>' Visible='<%# Convert.ToBoolean(Eval("[Approved]")) == false && Convert.ToBoolean(Eval("[Rejected]")) == false %>' runat="server" />
                                                    <asp:Button id="btnCancelApprove" Text="Cancel Approval" CssClass="rounded-button cancel-validation" OnCommand="btnCancelValidation_Command" CommandArgument='<%# Eval("[No]") %>' Visible='<%# Convert.ToBoolean(Eval("[Approved]")) == true %>' runat="server" />
                                                    <asp:Button id="btnCancelReject" Text="Cancel Reject" CssClass="rounded-button cancel-validation" OnCommand="btnCancelValidation_Command" CommandArgument='<%# Eval("[No]") %>' Visible='<%# Convert.ToBoolean(Eval("[Rejected]")) == true %>' runat="server" />
                                                     <%--<%# Eval("['ValidatedDate']") %>--%>

                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
