<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Menutest.aspx.cs" Inherits="Salud.Menutest" %>

<%@ Register assembly="Telerik.Web.UI" namespace="Telerik.Web.UI" tagprefix="telerik" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
        OldValuesParameterFormatString="original_{0}" SelectMethod="GetMenuItems" 
        TypeName="Salud.Security.SSO.SSOMenuItems"></asp:ObjectDataSource>
    <telerik:RadScriptManager ID="RadScriptManager1" Runat="server">
    </telerik:RadScriptManager>
    <br />
    <telerik:RadMenu ID="RadMenu4" Runat="server" DataSourceID="ObjectDataSource1" 
        DataFieldID="ID" DataFieldParentID="ParentID" DataNavigateUrlField="Url" 
        DataTextField="Text" DataValueField="Value" 
        onitemdatabound="RadMenu4_ItemDataBound" Skin="Telerik"
       >
        <Items>
            <telerik:RadMenuItem runat="server" Text="Root RadMenuItem1">
            </telerik:RadMenuItem>
            <telerik:RadMenuItem runat="server" Text="Root RadMenuItem2">
                <Items>
                    <telerik:RadMenuItem runat="server" Text="Child RadMenuItem 1">
                    </telerik:RadMenuItem>
                    <telerik:RadMenuItem runat="server" Text="Child RadMenuItem 2">
                    </telerik:RadMenuItem>
                </Items>
            </telerik:RadMenuItem>
            <telerik:RadMenuItem runat="server" Text="Root RadMenuItem3">
            </telerik:RadMenuItem>
        </Items>
    </telerik:RadMenu>    
    </form>
</body>
</html>
