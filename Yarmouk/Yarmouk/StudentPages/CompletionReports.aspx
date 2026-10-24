<%@ Page Title="Completion reports" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="CompletionReports.aspx.cs" Inherits="Yarmouk.StudentPages.CompletionReports" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="completion-inbox" aria-labelledby="completion-inbox-title">
  <p class="completion-eyebrow">UNIVERSITY APPROVAL</p>
  <h1 id="completion-inbox-title">Completion reports</h1>
  <p>Your final internship grades and approved reports, delivered by the university coordinator.</p>
  <p data-completion-feedback role="status" aria-live="polite">Loading your reports…</p>
  <div data-completion-inbox></div>
</section>
</asp:Content>
