<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RIMBA.SI.Default" %>
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

    <div class="sales-order">
        <div class="container-daftar">
            <div class="daftar">
                <table border="0">
                    <asp:Repeater ID="rptParent"  runat="server">
                        <ItemTemplate>
                            <tr><td>
                                <div class="item">
                                    <div class="info oneline">
                                        <div class="col1">
                                            <div class="text-bold"><%# Eval("SalesInvoiceID") %></div>
                                        </div>
                                        <div class="col2">
                                            <div>Customer: <%# Eval("CustomerName") %></div>
                                        </div>
                                    </div>
                                    <div class="info oneline">
                                        <div class="col1">
                                            <div>Date: <%# string.Format("{0:dd MMM yyyy}",Eval("Date")) %></div>
                                        </div>
                                        <div class="col2">
                                            <div>Sales: <%# Eval("SalesName") %></div>
                                        </div>
                                    </div>
                                    <div class="info oneline">
                                        <div class="col1">
                                            <div>&nbsp;</div>
                                        </div>
                                        <div class="col2">
                                            <div>Operator: <%# Eval("UserName") %></div>
                                        </div>
                                    </div>
                                    <div class="children-container">
                                        <asp:Repeater ID="rptChild" DataSource='<%# ((System.Data.DataRowView)Container.DataItem).Row.GetChildRows(RELATIONNAME) %>' OnItemCommand="rptChild_ItemCommand" runat="server">
                                            <HeaderTemplate>
                                                <div class="child-header oneline">
                                                    <div class="kode">Kode</div>
                                                    <div class="nama-barang">Nama Barang</div>
                                                    <div class="quantity">Jumlah</div>
                                                    <div class="harga">Harga</div>
                                                    <div class="discount">Disc</div>
                                                    <div class="total">Subtotal</div>
                                                </div>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <a name='<%# Eval("[No]") %>' />
                                                <asp:Label ID="ID" Text='<%# Eval("[No]") %>' Visible="false" runat="server" />
                                                <div class="child oneline">
                                                    <div class="kode"><%# Eval("['InventoryID']") %> </div>
                                                    <div class="nama-barang"><%# Eval("['InventoryName']") %> </div>
                                                    <div class="quantity"><%# Eval("['FormattedQuantity']") %> </div>   
                                                    <div class="harga"><%# string.Format("{0:N2} /{1}", Eval("['Price']"), Eval("['HighestUnitCode']")) %> </div>   
                                                    <div class="discount"><%# string.Format("{0:N2}%", Eval("['Discount']")) %> </div>
                                                    <div class="total"><%# string.Format("{0:N2}", Eval("['Total']")) %> </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                    <div class="grand-total">TOTAL: <%# string.Format("{0} {1:N2}", Eval("CurrencySymbol"), Eval("SubTotal")) %></div>

                                    <div>
                                        <asp:Button id="btnCancelApprove" Text="Cancel Approval" CssClass="rounded-button cancel-validation" OnCommand="btnCancelValidation_Command" CommandArgument='<%# Eval("SalesInvoiceID") %>' Visible='<%# Convert.ToBoolean(Eval("Approved")) == true %>' runat="server" />
                                        <asp:Button id="btnCancelReject" Text="Cancel Reject" CssClass="rounded-button cancel-validation" OnCommand="btnCancelValidation_Command" CommandArgument='<%# Eval("SalesInvoiceID") %>' Visible='<%# Convert.ToBoolean(Eval("Rejected")) == true %>' runat="server" />
                                        <asp:Button id="btnApprove" Text="Approve" CssClass="rounded-button" OnCommand="btnApprove_Command" CommandArgument='<%# Eval("SalesInvoiceID") %>' Visible='<%# Convert.ToBoolean(Eval("Approved")) == false && Convert.ToBoolean(Eval("Rejected")) == false %>' runat="server" />
                                        <asp:Button id="btnReject" Text="Reject" CssClass="rounded-button" OnCommand="btnReject_Command" CommandArgument='<%# Eval("SalesInvoiceID") %>' Visible='<%# Convert.ToBoolean(Eval("Approved")) == false && Convert.ToBoolean(Eval("Rejected")) == false %>' runat="server" />
                                         <%--<%# Eval("['ValidatedDate']") %>--%>
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
