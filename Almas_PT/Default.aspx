<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RIMBA.Default" %>
<%@ Register src="~/UserControls/Message.ascx" tagname="Message" tagprefix="Custom" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
        
    <Custom:Message ID="message" runat="server" />
    
    <div class="page-title">
        Pending Items
    </div>

    <div class="main-links-container">
        <div class="main-links"><asp:LinkButton ID="lbtnSO" Text="Sales Order" OnClick="lbtnSO_Click" runat="server" /> (<asp:Label ID="lblSOCount" runat="server" />)</div>
        <div class="main-links"><asp:LinkButton ID="lbtnSI" Text="Sales Invoice" OnClick="lbtnSI_Click" runat="server" /> (<asp:Label ID="lblSICount" runat="server" />)</div>
        <div class="main-links"><asp:LinkButton ID="lbtnRO" Text="Permintaan Pembelian" OnClick="lbtnRO_Click" runat="server" /> (<asp:Label ID="lblROCount" runat="server" />)</div>
        <div class="main-links"><asp:LinkButton ID="lbtnPO" Text="Purchase Order" OnClick="lbtnPO_Click" runat="server" /> (<asp:Label ID="lblPOCount" runat="server" />)</div>
        <div class="main-links"><asp:LinkButton ID="lbtnPI" Text="Purchase Invoice" OnClick="lbtnPI_Click" runat="server" /> (<asp:Label ID="lblPICount" runat="server" />)</div>
        <div class="main-links"><asp:LinkButton ID="lbtnJO" Text="Order Produksi" OnClick="lbtnJO_Click" runat="server" /> (<asp:Label ID="lblJOCount" runat="server" />)</div>
    </div>

</asp:Content>
