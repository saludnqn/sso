<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="Salud._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="/resources/jquery-1.4.2.min.js" type="text/javascript"></script>
    <script src="/resources/Salud.Applications.Shared.js" type="text/javascript"></script>
    <script src="/resources/Salud.Applications.Shared.Workflows.js" type="text/javascript"></script>
    <script src="/resources/Salud.Applications.Shared.Date.js" type="text/javascript"></script>
    <script src="/resources/Salud.Applications.Telerik.js" type="text/javascript"></script>
    <script src="/resources/Salud.Applications.JQuery.js" type="text/javascript"></script>
    <script src="/resources/Salud.Applications.Historias.js" type="text/javascript"></script>
    <script src="/resources/Salud.Security.SSO.js" type="text/javascript"></script>
    <style>          
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <script type="text/javascript">
            Salud.Security.SSO.Messages.Init();
        </script>
        <asp:TextBox ID="TextBox1" runat="server" Width="281px"></asp:TextBox>
        <br />
        <br />
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
        <asp:Button ID="Button2" runat="server" Text="Prevent Lock" OnClientClick="Salud.Security.SSO.PreventLock(); return false;" />
        <asp:Button ID="Button3" runat="server" Text="Init Check Lock" OnClientClick="Salud.Security.SSO.InitCheckLock(); return false;" />
        &nbsp;<asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
        <salud:ButtonEx ID="ButtonEx1" runat="server" Text='Hello &lt;img src="/resources/images/arrowDown16.gif" style="width: 16px; height: 16px" /&gt;' OnClick="ButtonEx1_Click" CommandArgument="Hola"></salud:ButtonEx>
        <p>
            &nbsp;</p>
        <asp:Image ID="Image1" runat="server" />
    </div>
    <telerik:RadScriptManager ID="RadScriptManager1" Runat="server">
    </telerik:RadScriptManager>
    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server">
    </telerik:RadAjaxManager>
    </form>
</body>
</html>
