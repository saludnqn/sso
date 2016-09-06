<%@ Page Title="" Language="C#" MasterPageFile="UI/MasterPageLogin.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Salud.Security.SSO.Login" ValidateRequest="false" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="ContentBodyPlaceHolder">
    <telerik:RadCodeBlock runat="server">
        <style type="text/css">
            input[type="text"], input[type="password"]{
                background-color: #e1e1e1;
                height:16px;
                width:180px;
                border-top: solid 1px rgb(103, 133, 164);
                border-left: solid 1px rgb(103, 133, 164);
                font-family: 'Segoe UI';
                font-size: 14px;
            }

        </style>
        <script type="text/javascript">
            <%-- 
            function ssoSaveCookie() {
                if (!jQuery.cookie('ssoDontRememberUsername'))
                    jQuery.cookie('ssoLastUsername', $find("<%= tbUsername.ClientID %>").get_value(), { expires: 365 });
            }
            
            $(document).ready(function () {
                var checkbox = $("#<%= cbRememberUsername.ClientID %>");
                if (jQuery.cookie('ssoDontRememberUsername')) {
                    checkbox[0].checked = false;
                } else {
                    checkbox[0].checked = true;
               
                    var lastUsername = jQuery.cookie('ssoLastUsername');
                    if (lastUsername) {
                        $find("<%= tbUsername.ClientID %>").set_value(lastUsername);
                        $find("<%= tbPassword.ClientID %>").focus();
                    } else {
                        $find("<%= tbUsername.ClientID %>").focus();
                    }
                }

                checkbox.click(function () {
                    if (!$(this)[0].checked)
                        jQuery.cookie('ssoDontRememberUsername', true, { expires: 365 });
                    else
                        jQuery.cookie('ssoDontRememberUsername', null);
                });
            }); --%>
        </script>
    </telerik:RadCodeBlock>
    <h1>
        <asp:Label runat="server" ID="lblLoginCaption"></asp:Label></h1>
    <salud:PanelEx runat="server" ID="pnlWarningBox" CssClass="warningBox ssoMessageBox" Visible="false">
        <asp:Label runat="server" ID="lblWarning"></asp:Label>
    </salud:PanelEx>
    <salud:PanelEx runat="server" ID="pnlErrorBox" CssClass="errorBox ssoMessageBox" Visible="false">
        <asp:Label runat="server" ID="lblError"></asp:Label>
    </salud:PanelEx>
    <h4>Nombre de usuario</h4>
    <asp:TextBox runat="server" ID="tbUsername" Height="16px"></asp:TextBox>
    <%--<telerik:RadTextBox ID="tbUsername" runat="server" Width="250px">
    </telerik:RadTextBox>--%>
    <%--<div style="font-size: smaller">
        <asp:CheckBox runat="server" ID="cbRememberUsername" TabIndex="-1" />
        Recordar mi usuario en esta computadora
    </div>--%>
    <h4>Contraseña
    </h4>
    <asp:TextBox runat="server" ID="tbPassword" TextMode="Password" Height="16px"></asp:TextBox>
    <%--<telerik:RadTextBox ID="tbPassword" runat="server" TextMode="Password" Width="250px">
    </telerik:RadTextBox>--%>
    <br />
    <div class="ssoTipSeguridad">
        <div>
            Tip de seguridad
        </div>
        <asp:Label runat="server" ID="lblTipSeguridad" Text="lblTipSeguridad"></asp:Label>
    </div>


    <asp:Panel runat="server" CssClass="panelBotones">
        <div>
            <asp:Button ID="btnIniciarSesion" runat="server" CssClass="botonSeguridad" 
                Style="width: 150px;height:35px;" Text="Iniciar sesión" BackColor="#02ab68"
                OnClick="btnIniciarSesion_Click"/>
            <%--OnClientClick="ssoSaveCookie();"--%>
        </div>
    </asp:Panel>
    <%--<salud:PanelEx ID="Panelex1" runat="server" CssClass="panelBotones">
        <salud:ButtonEx ID="ButtonEx1" runat="server" CssClass="botonSeguridad" Style="width: 150px;" Text="Iniciar sesión" />
    </salud:PanelEx>--%>
</asp:Content>
