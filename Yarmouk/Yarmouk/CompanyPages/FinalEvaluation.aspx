<%@ Page Title="FinalEvaluation" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" CodeBehind="FinalEvaluation.aspx.cs" Inherits="Yarmouk.CompanyPages.FinalEvaluation" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/FinalEvaluation.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/FinalEvaluation.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Final Evaluation</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
