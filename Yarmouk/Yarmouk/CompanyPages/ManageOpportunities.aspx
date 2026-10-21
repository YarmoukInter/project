<%@ Page Title="ManageOpportunities" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageOpportunities.aspx.cs" Inherits="Yarmouk.CompanyPages.ManageOpportunities" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/ManageOpportunities.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/ManageOpportunities.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Manage Opportunities</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
