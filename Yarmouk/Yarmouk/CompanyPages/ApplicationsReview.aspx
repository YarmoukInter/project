<%@ Page Title="ApplicationsReview" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" CodeBehind="ApplicationsReview.aspx.cs" Inherits="Yarmouk.CompanyPages.ApplicationsReview" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/ApplicationsReview.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/ApplicationsReview.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Applications Review</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
