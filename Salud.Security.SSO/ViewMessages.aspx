<%@ Page Title="" Language="C#" MasterPageFile="~/UI/MasterPage.Master" AutoEventWireup="true" CodeBehind="ViewMessages.aspx.cs" Inherits="Salud.Security.SSO.ViewMessages" %>

<asp:Content runat="server" ContentPlaceHolderID="ContentBodyPlaceHolder">
    <style>
        .ssoMessages H5
        {
            white-space: nowrap;
        }
        
        .ssoMessages TR:nth-child(2n)
        {
            background: #E2E2E2;
        }
        
        .ssoMessages TR:nth-child(2n-1)
        {
            background: #F2F2F2;
        }
        
        .ssoMessages TD
        {
            padding-right: 10px;
            padding-bottom: 10px;
        }
    </style>
    <asp:Repeater runat="server" ID="messages">
        <HeaderTemplate>
            <table class="ssoMessages">
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td style="vertical-align: middle">
                    <asp:Image runat="server" ID="imgTipo" OnDataBinding="ImageDataBind" Width="32" />
                </td>
                <td style="text-align: right">
                    <h5 style="color: Silver">
                        Fecha y hora</h5>
                    <asp:Label runat="server" Text='<%# Eval("date", "{0:d}") %>' Font-Bold="true"></asp:Label><br />
                    <asp:Label runat="server" Text='<%# Eval("date", "{0:HH:mm}") %>' Font-Bold="true"></asp:Label>
                </td>
                <td>
                    <h5 style="color: Silver">
                        Mensaje</h5>
                    <asp:Label runat="server" Text='<%# Eval("message") %>'></asp:Label>
                </td>
                <td>
                    <h5 style="color: Silver">
                        Notificado en ...</h5>
                    <asp:Image runat="server" Width="16" Visible='<%# Eval("intranet") %>' ToolTip="Intranet" Style="margin-right: 5px" ImageUrl="/Resources.Net/Iconos/intranet.png" />
                    <asp:Image runat="server" Width="16" Visible='<%# Eval("email") %>' ToolTip="Email" Style="margin-right: 5px" ImageUrl="/Resources.Net/Iconos/email.png" />
                    <asp:Image runat="server" Width="16" Visible='<%# Eval("sms") %>' ToolTip="Mensaje de texto (SMS)" Style="margin-right: 5px" ImageUrl="/Resources.Net/Iconos/mobile.png" />
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
</asp:Content>
