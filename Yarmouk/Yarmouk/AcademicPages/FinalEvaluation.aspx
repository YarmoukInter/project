<%@ Page Title="FinalEvaluation" Language="C#" MasterPageFile="~/AcademicPages/Section.Master" AutoEventWireup="true" CodeBehind="FinalEvaluation.aspx.cs" Inherits="Yarmouk.AcademicPages.FinalEvaluation" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/AcademicPages/FinalEvaluation.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/AcademicPages/FinalEvaluation.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Final Evaluation</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
