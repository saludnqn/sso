<%@ Page Title="Opciones de usuario" Language="C#" MasterPageFile="~/UI/MasterPage.Master" AutoEventWireup="true" CodeBehind="Options.aspx.cs" Inherits="Salud.Security.SSO.Options" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentBodyPlaceHolder" runat="server">

    <style>
        #ssoPasswordChecks LI.valid {
            background: url('Resources/passwordValid.png') no-repeat left center;
            background-size: 16px 16px;
            padding-left: 24px;
        }

        #ssoPasswordChecks LI.invalid {
            background: url('Resources/passwordInvalid.png') no-repeat left center;
            background-size: 16px 16px;
            padding-left: 24px;
            font-weight: bold;
        }

        .skin-slidedeck {
            width: 900px;
            height: 300px;
            max-width: 900px;
            max-height: 300px;
            overflow: hidden;
        }

        #slidedeck_frame .acciones DIV {
            float: left;
            width: 200px;
            height: 200px;
            padding: 30px 10px 10px 10px;
            box-shadow: 0 0 4px rgba(0, 0, 0, 0.2);
            text-align: center;
            margin-right: 10px;
        }

            #slidedeck_frame .acciones DIV IMG, #slidedeck_frame .acciones DIV H3 {
                opacity: 0.5;
            }

            #slidedeck_frame .acciones DIV:hover {
                cursor: pointer;
                background: #E1E1E1;
            }
    </style>
    <script type="text/javascript">
                
        function TestPasswordStrength(event) {
            this.setStyle = function (li, isValid) {
                if (isValid)
                    li.removeClass("invalid").removeClass("colorRojo_Font").addClass("valid").addClass("colorVerde_Font");
                else
                    li.removeClass("valid").removeClass("colorVerde_Font").addClass("invalid").addClass("colorRojo_Font");
            }

            var holder = $("#ssoPasswordChecks");
            var value = $($get("<%= tbNewPassword.ClientID %>")).val();
            /*if (event && event.which >= 49 && event.which <= 121) {
            value += String.fromCharCode(event.which)
            }*/

            this.setStyle($(".chars", holder), value.length >= 6);
            this.setStyle($(".digits", holder), new RegExp("[0-9]", "g").test(value));
            this.setStyle($(".uppercase", holder), new RegExp("[A-Z]", "g").test(value));
            this.setStyle($(".lowercase", holder), new RegExp("[a-z]", "g").test(value));
        }

        $(document).ready(function () {
            if ($find("<%= tbPassword.ClientID %>"))
                $find("<%= tbPassword.ClientID %>").focus();
        });
    </script>

    <telerik:RadScriptBlock runat="server" Visible="false" ID="scripts">
        <script type="text/javascript">
            $(document).ready(function () {
                // Prepara eventos
                var tbNewPassword = $($get("<%= tbNewPassword.ClientID %>"));
                $("INPUT", tbNewPassword.parent()).keyup(function (event) {
                    TestPasswordStrength(event);
                });

                // Prepara el tooltip con las indicaciones de la contraseña
                tbNewPassword.tipTip({
                    activation: "focus",
                    defaultPosition: "right",
                    keepAlive: true,
                    delay: 0,
                    tipHolder: "ssoPasswordStrength_holder",
                    content: $("#ssoPasswordStrength").html(),
                    enter: function () {
                        window.setTimeout(function () {
                            TestPasswordStrength();
                        }, 50);
                    }
                });
            });
        </script>
    </telerik:RadScriptBlock>
    <h1>Opciones de usuario</h1>
    <br />
    <salud:PanelEx runat="server" ID="pnlWarningBox" CssClass="warningBox ssoMessageBox" Visible="false">
        <asp:Label runat="server" ID="lblWarning"></asp:Label>
    </salud:PanelEx>
    <asp:Panel runat="server" ID="pnlLogin" DefaultButton="btnContinuar">
        <h4>Para continuar operando ingrese su contraseña actual</h4>
        <telerik:RadTextBox ID="tbPassword" runat="server" TextMode="Password" Width="250px">
        </telerik:RadTextBox>
        <salud:PanelEx runat="server" CssClass="panelBotones noWrap">
            <salud:ButtonEx runat="server" ID="btnContinuar" CssClass="botonSeguridad" Text="Continuar" OnClick="btnContinuar_Click" />
            <salud:ButtonEx runat="server" CssClass="botonVolver" Text="Volver" OnClientClick="window.history.go(-1); return false" />
        </salud:PanelEx>
    </asp:Panel>
    
    <asp:Panel runat="server" ID="pnlOptions" Visible="false" DefaultButton="btnGuardar">
        <div id="slidedeck_frame" class="skin-slidedeck">
                                                                   
                        <h3>Bloqueo de sesión</h3>
                        <h4>Indica el tiempo de bloqueo automático para proteger su sesión</h4>
                        <telerik:RadComboBox runat="server" ID="cbTiempoBloqueo" Width="350px">
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Text="No bloquear (¡no recomendado!)" Value="0" />
                                <telerik:RadComboBoxItem runat="server" Text="Bloquear luego de 5 minutos de inactividad" Value="300" />
                                <telerik:RadComboBoxItem runat="server" Text="Bloquear luego de 10 minutos de inactividad" Value="600" />
                                <telerik:RadComboBoxItem runat="server" Text="Bloquear luego de 15 minutos de inactividad" Value="900" />
                                <telerik:RadComboBoxItem runat="server" Text="Bloquear luego de 30 minutos de inactividad" Value="900" />
                                <telerik:RadComboBoxItem runat="server" Text="Bloquear luego de 45 minutos de inactividad" Value="1200" />
                                <telerik:RadComboBoxItem runat="server" Text="Bloquear luego de 60 minutos de inactividad" Value="1500" />
                            </Items>
                        </telerik:RadComboBox>                    
                        
                        <br />
                        <br />                               

                        <h3>Cambiar contraseña</h3>
                        <h4>Ingrese a continuación una nueva palabra clave.</h4>
                        <telerik:RadTextBox ID="tbNewPassword" runat="server" TextMode="Password" Width="250px">
                        </telerik:RadTextBox>
                        <script id="ssoPasswordStrength" type="text/x-jquery-tmpl">
                            Para que la contraseña sea válida debe tener como mínimo ...
                        <ul id="ssoPasswordChecks">
                            <li class="chars invalid colorRojo_Font">Seis o más caracteres</li>
                            <li class="uppercase invalid colorRojo_Font">Una letra mayúscula</li>
                            <li class="lowercase invalid colorRojo_Font">Una letra minúscula</li>
                            <li class="digits invalid colorRojo_Font">Un dígito</li>
                        </ul>
                        </script>
                        <h4>Reingrese la nueva contraseña</h4>
                        <telerik:RadTextBox ID="tbNewPasswordConfirmation" runat="server" TextMode="Password" Width="250px">
                        </telerik:RadTextBox>                                                
                    
                        <salud:PanelEx ID="PanelBotones" runat="server" CssClass="panelBotones">
                            <div class="floatLeft" style="margin-right: 10px">
                                <salud:ButtonEx runat="server" ID="btnGuardar" CssClass="botonSeguridad" Text="Guardar" OnClick="btnGuardar_Click" />
                                <salud:ButtonEx runat="server" ID="btnCancelar" CssClass="botonCancelar" Text="Cancelar" OnClick="btnCancelar_Click"/>
                            </div>
                            <asp:Panel runat="server" ID="pnlPasswordOk" CssClass="floatLeft colorVerde_Font" Font-Bold="true" Visible="false">
                                ¡Los datos fueron cambiados correctamente!
                            </asp:Panel>
                            <asp:Panel runat="server" ID="pnlPasswordError" CssClass="floatLeft colorRojo_Font" Font-Bold="true" Visible="false">
                                Los datos no pudieron cambiarse
                            </asp:Panel>
                        </salud:PanelEx>
        </div>                        
    </asp:Panel>
</asp:Content>
