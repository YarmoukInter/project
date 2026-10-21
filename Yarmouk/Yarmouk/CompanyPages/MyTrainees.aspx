<%@ Page Title="MyTrainees" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" CodeBehind="MyTrainees.aspx.cs" Inherits="Yarmouk.CompanyPages.MyTrainees" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/MyTrainees.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/MyTrainees.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>My Trainees</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
