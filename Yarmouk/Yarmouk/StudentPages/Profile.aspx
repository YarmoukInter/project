<%@ Page Title="Profile" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="Yarmouk.StudentPages.Profile" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/StudentPages/Profile.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/Profile.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Profile</h1>
<p>Page layout placeholder. Add controls and events here.</p>
<div class="mb-2"><strong>Student ID:</strong> <asp:Label ID="lblStudentId" runat="server" /></div>
<div class="mb-2"><strong>Major:</strong> <asp:Label ID="lblMajor" runat="server" /></div>
</asp:Content>
