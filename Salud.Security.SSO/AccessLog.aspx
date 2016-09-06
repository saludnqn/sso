<%@ Page Title="" Language="C#" AutoEventWireup="true" MasterPageFile="~/UI/MasterPage.Master" CodeBehind="AccessLog.aspx.cs" Inherits="Salud.Security.SSO.AccessLog" %>

<asp:Content ID="Content5" ContentPlaceHolderID="ContentBodyPlaceHolder" runat="server">
    <style type="text/css">
        .gridPanel
        {
            max-height: 300px;
            overflow-y: scroll;
            overflow-x: hidden;
        }
    </style>
    <salud:PanelEx runat="server" CssClass="warningBox">Momentáneamente las consultas online están deshabilitadas.<br />Comuníquese al Departamento de Tecnologías de la Información (int. 889) para solicitar el reporte de accesos.</salud:PanelEx>
<%--
    <table class="fieldLayout" style="display: none">
        <tr class="fieldLayoutRow">
            <td class="fieldLayoutColumn">
                <telerik:RadAjaxManagerProxy ID="RadAjaxManagerProxy1" runat="server">
                    <AjaxSettings>
                        <telerik:AjaxSetting AjaxControlID="cbSoloModificaciones">
                            <UpdatedControls>
                                <telerik:AjaxUpdatedControl ControlID="gridAccessLog" />
                                <telerik:AjaxUpdatedControl ControlID="gridData" />
                            </UpdatedControls>
                        </telerik:AjaxSetting>
                        <telerik:AjaxSetting AjaxControlID="gridAccessLog">
                            <UpdatedControls>
                                <telerik:AjaxUpdatedControl ControlID="gridAccessLog" />
                                <telerik:AjaxUpdatedControl ControlID="gridData" />
                            </UpdatedControls>
                        </telerik:AjaxSetting>
                    </AjaxSettings>
                </telerik:RadAjaxManagerProxy>
                <h2>
                    Accesos registrados</h2>
                <div style="height: 30px">
                    <asp:CheckBox ID="cbSoloModificaciones" runat="server" CssClass="NullableCheckBox_TextRight" Text="Mostrar sólo accesos con modificaciones de datos" AutoPostBack="True" Checked="True" />
                </div>
                <div class="gridPanel">
                    <telerik:RadGrid ID="gridAccessLog" runat="server" DataSourceID="dsAccessLog" GridLines="None" OnSelectedIndexChanged="gridAccessLog_SelectedIndexChanged" AutoGenerateColumns="False" OnDataBound="gridAccessLog_DataBound" Width="500px">
                        <ClientSettings EnablePostBackOnRowClick="True" EnableRowHoverStyle="True">
                            <Selecting AllowRowSelect="True" />
                        </ClientSettings>
                        <MasterTableView DataKeyNames="id, timeIn, timeOut, userId" DataSourceID="dsAccessLog">
                            <NoRecordsTemplate>
                                No se encontró ningún acceso
                            </NoRecordsTemplate>
                            <CommandItemSettings ExportToPdfText="Export to Pdf"></CommandItemSettings>
                            <RowIndicatorColumn>
                                <HeaderStyle Width="20px"></HeaderStyle>
                            </RowIndicatorColumn>
                            <ExpandCollapseColumn>
                                <HeaderStyle Width="20px"></HeaderStyle>
                            </ExpandCollapseColumn>
                            <Columns>
                                <telerik:GridBoundColumn DataField="timeIn" DataType="System.DateTime" HeaderText="Fecha y Hora" SortExpression="timeIn" UniqueName="timeIn">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="moduleName" HeaderText="Módulo" SortExpression="moduleName" UniqueName="moduleName">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="userFullname" HeaderText="Usuario" ReadOnly="True" SortExpression="userFullname" UniqueName="userFullname">
                                </telerik:GridBoundColumn>
                            </Columns>
                        </MasterTableView>
                        <PagerStyle ShowPagerText="False" />
                        <HeaderContextMenu EnableImageSprites="True" CssClass="GridContextMenu GridContextMenu_Default">
                        </HeaderContextMenu>
                    </telerik:RadGrid>
                </div>
                <asp:SqlDataSource ID="dsAccessLog" runat="server" ConnectionString="<%$ ConnectionStrings:SSOMembershipProviderConnectionString %>" SelectCommand="SELECT * FROM dbo.SSO_GetAccessLogForKey(@applicationId, @key, @value, null, null) AS AccessLog
WHERE dbAuditRecords &gt;= @dbAuditRecords
ORDER BY timeIn" OnSelected="dsAccessLog_Selected">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="applicationId" QueryStringField="applicationId" />
                        <asp:QueryStringParameter DefaultValue="" Name="key" QueryStringField="key" />
                        <asp:QueryStringParameter DefaultValue="" Name="value" QueryStringField="value" />
                        <asp:ControlParameter ControlID="cbSoloModificaciones" Name="dbAuditRecords" PropertyName="Checked" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </td>
            <td class="fieldLayoutColumn">
                <h2>
                    Datos modificados</h2>
                <div style="height: 30px">
                </div>
                <div class="gridPanel">
                    <telerik:RadGrid ID="gridData" runat="server" DataSourceID="dsData" GridLines="None" AutoGenerateColumns="False" Width="500px">
                        <MasterTableView DataSourceID="dsData">
                            <NoRecordsTemplate>
                                El módulo indicado no realizó cambios en los datos relacionados
                            </NoRecordsTemplate>
                            <CommandItemSettings ExportToPdfText="Export to Pdf"></CommandItemSettings>
                            <RowIndicatorColumn>
                                <HeaderStyle Width="20px"></HeaderStyle>
                            </RowIndicatorColumn>
                            <ExpandCollapseColumn>
                                <HeaderStyle Width="20px"></HeaderStyle>
                            </ExpandCollapseColumn>
                            <Columns>
                                <telerik:GridBoundColumn DataField="OperationName" HeaderText="Operación" UniqueName="OperationName">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="TableName" HeaderText="Tabla" UniqueName="TableName">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="TableKeys" HeaderText="Claves" UniqueName="TableKeys">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="FieldName" HeaderText="Campo" UniqueName="FieldName">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="OldValue" HeaderText="Valor anterior" UniqueName="OldValue" HeaderStyle-Wrap="False">
                                </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="NewValue" HeaderStyle-Wrap="False" HeaderText="Nuevo valor" UniqueName="NewValue">
                                </telerik:GridBoundColumn>
                            </Columns>
                        </MasterTableView>
                        <HeaderContextMenu EnableImageSprites="True" CssClass="GridContextMenu GridContextMenu_Default">
                        </HeaderContextMenu>
                    </telerik:RadGrid>
                </div>
                <asp:SqlDataSource ID="dsData" runat="server" ConnectionString="<%$ ConnectionStrings:SSOMembershipProviderConnectionString %>" OnSelecting="dsData_Selecting" SelectCommand="exec sp_SSO_GetAuditDB @fromDate, @toDate, @userId ">
                    <SelectParameters>
                        <asp:Parameter Name="fromDate" />
                        <asp:Parameter Name="toDate" />
                        <asp:Parameter Name="userId" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </td>
        </tr>
    </table>
--%></asp:Content>
