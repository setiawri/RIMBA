<%@ Page Title="Log in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="RIMBA.Login" %>
<%@ Register src="UserControls/Message.ascx" tagname="Message" tagprefix="Custom" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
    <Custom:Message ID="message" runat="server" />

    <div class="page-title">
        <%: Title %>
    </div>

    <div class="login-container">
        <hr class="style-three" />
        <div class="input-group">
            <asp:Label runat="server" AssociatedControlID="txtUsername">Username</asp:Label>
            <div class="input-textbox">
                <asp:TextBox runat="server" ID="txtUsername" CssClass="rounded-textbox" />
            </div>
        </div>
        <div class="input-group">
            <asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="col-md-2 control-label">Password</asp:Label>
            <div class="input-textbox">
                <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" CssClass="rounded-textbox" />
            </div>
        </div>
        <div class="input-group">
            <asp:Button runat="server" OnClick="btnLogin_OnClick" Text="Log in" CssClass="rounded-button" />
        </div>
    </div>

</asp:Content>
