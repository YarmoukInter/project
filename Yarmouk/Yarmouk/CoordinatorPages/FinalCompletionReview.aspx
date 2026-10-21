<%@ Page Title="FinalCompletionReview" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="FinalCompletionReview.aspx.cs" Inherits="Yarmouk.CoordinatorPages.FinalCompletionReview" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/FinalCompletionReview.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/FinalCompletionReview.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Final Completion Review</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
