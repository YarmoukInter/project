<%@ Page Title="Team" Language="C#" MasterPageFile="~/PublicPages/Section.Master" AutoEventWireup="true" CodeBehind="Team.aspx.cs" Inherits="Yarmouk.PublicPages.Team" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/PublicPages/Team.css") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/PublicPages/Team.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
</asp:Content>
