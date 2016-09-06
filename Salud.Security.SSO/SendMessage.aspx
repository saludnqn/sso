<%@ Page Title="Enviar notificaciones" Language="C#" MasterPageFile="~/UI/MasterPage.Master" AutoEventWireup="true" CodeBehind="SendMessage.aspx.cs" Inherits="Salud.Security.SSO.SendMessage" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentBodyPlaceHolder" runat="server">
    <style>
        .targetItem SPAN
        {
            display: inline-block;
            max-width: 350px;
        }
        .targetItem BUTTON
        {
            visibility: hidden;
        }
        
        .targetItem:hover BUTTON
        {
            visibility: visible;
        }
    </style>
    <asp:Panel runat="server" ID="pnlEnviar">
        <table class="fieldLayout" style="min-width: 600px">
            <tr>
                <td style="width: 1px">
                    <h3>
                        Seleccione un usuario ...</h3>
                    <telerik:radcombobox runat="server" id="cbUser" width="250px" allowcustomtext="True" autopostback="True" dropdownwidth="500px" enableloadondemand="True" onitemsrequested="cbUser_ItemsRequested" showdropdownontextboxclick="False" onselectedindexchanged="cbUser_SelectedIndexChanged">
                </telerik:radcombobox>
                    <h3>
                        ... o un rol</h3>
                    <telerik:radcombobox runat="server" id="cbRole" width="250px" allowcustomtext="True" autopostback="True" dropdownwidth="500px" enableloadondemand="True" onitemsrequested="cbRole_ItemsRequested" showdropdownontextboxclick="False" onselectedindexchanged="cbRole_SelectedIndexChanged">
                </telerik:radcombobox>
                </td>
                <td>
                    <h3>
                        Destinatarios</h3>
                    <telerik:radlistview runat="server" id="lvTargets">
                    <ItemTemplate>
                        <div class="targetItem">
                            <asp:Label runat="server" ID="lblName" Text='<%# Eval("Name") %>'></asp:Label>
                            <salud:ButtonEx runat="server" ID="btnDelete" CssClass="botoncitoEliminar" CommandArgument='<%# String.Format("{0}_{1}", (int)Eval("TargetType"), Eval("Target")) %>' OnClick="btnDelete_Click" />
                        </div>
                    </ItemTemplate>
                </telerik:radlistview>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <h3>
                        Mensaje</h3>
                    <div style="display: inline-block">
                        <div class="floatLeft">
                            <h4>
                                Asunto <span class="optionalTag">(Opcional)</span></h4>
                            <telerik:radtextbox id="tbSubject" runat="server" width="250px"></telerik:radtextbox>
                            <h4>
                                Tipo</h4>
                            <telerik:radcombobox id="cbType" runat="server" width="250px">
                            <Items>
                                <telerik:RadComboBoxItem runat="server" Selected="True" Text="Mensaje" Value="1" />
                                <telerik:RadComboBoxItem runat="server" Text="Advertencia" Value="2" />
                                <telerik:RadComboBoxItem runat="server" Text="Mensaje prioritario" Value="3" />
                            </Items>
                        </telerik:radcombobox>
                        </div>
                        <div class="floatLeft" style="margin-left: 20px">
                            <h4>
                                Vencimiento <span class="optionalTag">(Opcional)</span></h4>
                            <telerik:raddatepicker id="dpExpiresOn" runat="server">
                        </telerik:raddatepicker>
                        </div>
                    </div>
                    <h4>
                        Notificación</h4>
                    <asp:CheckBox runat="server" ID="cbIntranet" Text="Intranet&nbsp;&nbsp;" Checked="true" />
                    <asp:CheckBox runat="server" ID="cbEmail" Text="Email&nbsp;&nbsp;" />
                    <asp:CheckBox runat="server" ID="cbSMS" Text="SMS" AutoPostBack="true" OnCheckedChanged="cbSMS_CheckedChanged" />
                    <asp:Panel runat="server" ID="pnlSMS" class="warningBox" Visible="False">
                        <b>Limitaciones de los mensajes SMS</b><br />
                        &nbsp;&nbsp; · Se omite el campo &#39;Asunto&#39;<br />
                        &nbsp;&nbsp; · Sonenviados en texto plano (sin formato)<br />
                        &nbsp;&nbsp; · Tienen un tamaño máximo de 140 caracteres</asp:Panel>
                    <h4>
                        Texto</h4>
                    <telerik:radeditor runat="server" id="editor" editmodes="Design" height="100px" width="100%" onload="editor_Load">
                    <Tools>
                        <telerik:EditorToolGroup Tag="Formatting">
                            <telerik:EditorTool Name="Bold" />
                            <telerik:EditorTool Name="Italic" />
                            <telerik:EditorTool Name="Underline" />
                        </telerik:EditorToolGroup>
                    </Tools>
                    <Content>
                    </Content>
                </telerik:radeditor>
                    <salud:panelex runat="server" cssclass="buttonBox fixed">
                    <div class="floatRight">
                        <salud:ButtonEx runat="server" CssClass="botonEmail" Text="Enviar" ID="btnSendMessage" OnClick="btnSendMessage_Click" DisableFormOnClick="True" />
                        <salud:ButtonEx runat="server" CssClass="botonCancelar" Text="Cancelar" OnClientClick="popupClose(); return false;" />
                    </div>
                </salud:panelex>
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel runat="server" ID="pnlResultados" Visible="false">
        <h3>
            Se produjeron errores en el reenvío del mensaje</h3>
        <telerik:radgrid runat="server" id="grid"></telerik:radgrid>
        <salud:panelex runat="server" cssclass="buttonBox fixed">
            <div class="floatRight">
                <salud:ButtonEx runat="server" CssClass="botonContinuar" Text="Reenviar mensaje" ID="btnReintentar" onclick="btnReintentar_Click" DisableFormOnClick="True" />
                <salud:ButtonEx runat="server" CssClass="botonCancelar" Text="Cancelar" OnClientClick="popupClose(); return false;" />
            </div>
        </salud:panelex>
    </asp:Panel>
</asp:Content>
