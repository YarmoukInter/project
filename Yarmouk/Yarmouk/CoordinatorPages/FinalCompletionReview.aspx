<%@ Page Title="Final Completion Review" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="FinalCompletionReview.aspx.cs" Inherits="Yarmouk.CoordinatorPages.FinalCompletionReview" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/FinalCompletionReview.css?v=20261021-4") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/FinalCompletionReview.js?v=review-button-1") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<div class="co-final-review" data-final-review-page></div>
</asp:Content>
