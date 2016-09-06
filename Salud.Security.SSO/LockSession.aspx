<%@ Page Title="Sesión bloqueada" Language="C#" AutoEventWireup="true" MasterPageFile="~/UI/MasterPage.Master" CodeBehind="LockSession.aspx.cs" Inherits="Salud.Security.SSO.LockSession" %>

<%@ Import Namespace="Salud.Security.SSO" %>
<asp:Content ID="Content5" ContentPlaceHolderID="ContentBodyPlaceHolder" runat="server">
    <script type="text/javascript">
        function CloseSession() {
            var url = 'Logout.aspx';
            try {
                top.window.location = url;
            }
            catch (E) {
                window.location = url;
            }
        }

        function ChangeUser() {
            var url = 'Logout.aspx?url=<%= Server.UrlEncode(Request.QueryString["url"]) %>';
            try {
                top.window.location = url;
            }
            catch (E) {
                window.location = url;
            }
        }


        $(document).ready(function () {
            $find("<%= tbPassword.ClientID %>").focus();
        });
    </script>
    <div id="mainDiv" runat="server">
        <h1>
            Sesión bloqueada</h1>
        <br />
        <salud:PanelEx runat="server" ID="pnlWarningBox" CssClass="warningBox ssoMessageBox">
            Para tu seguridad las sesiones se bloquean después de algunos minutos de inactividad. <b>Esto ayuda a proteger los datos</b> con los que estabas trabajando.
        </salud:PanelEx>
        <salud:PanelEx runat="server" ID="pnlErrorBox" CssClass="errorBox ssoMessageBox" Visible="false">
            La contraseña ingresada es incorrecta.
        </salud:PanelEx>
        <h4>
            Nombre de usuario</h4>
        <telerik:RadTextBox ID="tbUsername" runat="server" Width="250px" ReadOnly="true">
        </telerik:RadTextBox>
        <h4>
            Contraseña
        </h4>
        <telerik:RadTextBox ID="tbPassword" runat="server" TextMode="Password" Width="250px">
        </telerik:RadTextBox>
        <br />
        <div class="ssoTipSeguridad">
            <div>
                Tip de seguridad</div>
            <asp:Label runat="server" ID="lblTipSeguridad" Text="lblTipSeguridad"></asp:Label>
        </div>
        <salud:PanelEx ID="PanelEx1" runat="server" CssClass="panelBotones">
            <div class="floatLeft">
                <salud:ButtonEx ID="btnContinuar" runat="server" CssClass="botonSeguridad" Text="Desbloquear" OnClick="btnContinuar_Click" />
            </div>
            <div class="floatRight">
                <salud:ButtonEx runat="server" CssClass="botonActualizar" 
                    Text="Cambiar usuario" OnClientClick="ChangeUser(); return false;" />
                <salud:ButtonEx runat="server" CssClass="botonCancelar" Text="Cerrar sesión" OnClientClick="CloseSession(); return false;" />
            </div>
        </salud:PanelEx>
    </div>
</asp:Content>
