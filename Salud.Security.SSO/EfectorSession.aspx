<%@ Page Title="Ingresando al Efector" Language="C#" AutoEventWireup="true" MasterPageFile="UI/MasterPageLogin.Master" CodeBehind="EfectorSession.aspx.cs" Inherits="Salud.Security.SSO.EfectorSession" %>

<%@ Import Namespace="Salud.Security.SSO" %>
<asp:Content ID="Content5" ContentPlaceHolderID="ContentBodyPlaceHolder" runat="server">
<%--        <style type="text/css">
            select
            {
                background-color: rgb(225, 225, 225);
            }
        </style>--%>

    <div id="mainDiv" runat="server">
        
        <asp:Label ID="lblMensaje" runat="server" Text="Label" Visible="false"></asp:Label>
        

             <asp:Button ID="btnRegresar" runat="server" CssClass="botonSeguridad" Visible="false"
                    Style="width: 150px;height:35px;" Text="Regresar" BackColor="#02ab68"
                    OnClick="btnRegresar_Click" />
        <asp:Panel ID="Panel1" runat="server" CssClass="panelBotones1">
            <h1>Seleccione el efector con el que utilizará el sistema</h1>
        <br />

        <h4>Efector:</h4>
        <asp:DropDownList ID="ddlEfector" runat="server" Width="600px" Height="20px"></asp:DropDownList>
        <br />
            <br />
        <br />
        
            <div>
                <asp:Button ID="btnContinuar" runat="server" CssClass="botonSeguridad" 
                    Style="width: 150px;height:35px;" Text="Iniciar sesión" BackColor="#02ab68"
                    OnClick="btnContinuar_Click" />
            </div>
        </asp:Panel>
    </div>
</asp:Content>
