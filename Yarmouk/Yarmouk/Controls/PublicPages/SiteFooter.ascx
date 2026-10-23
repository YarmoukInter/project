<%@ Control Language="C#" %>
    <footer class="yu-footer" id="faculty-footer">
      <div class="yu-footer-main">
        <div class="yu-footer-identity">
          <a class="yu-footer-brand" href="<%= ResolveUrl("~/PublicPages/Home.aspx#top") %>" aria-label="Yarmouk University home"><img src="<%= ResolveUrl("~/Content/Images/yarmouk-logo-on-dark.svg") %>" alt="Yarmouk University" width="150" height="54" loading="lazy"></a>
          <h2>Faculty of Information Technology<br>and Computer Science</h2>
          <p>Yarmouk University · Irbid, Jordan</p>
          <a class="yu-footer-email" href="mailto:it.fac@yu.edu.jo">it.fac@yu.edu.jo <span aria-hidden="true">↗</span></a>
        </div>
        <nav class="yu-footer-explore" aria-label="University links">
          <h3>Explore</h3>
          <a href="<%= ResolveUrl("~/PublicPages/AboutUs.aspx") %>">About us</a>
          <a href="<%= ResolveUrl("~/PublicPages/ContactUs.aspx") %>">Contact us</a>
          <a href="https://it.yu.edu.jo/" target="_blank" rel="noopener noreferrer">Faculty website <span aria-hidden="true">↗</span></a>
          <a class="yu-footer-login" href="<%= ResolveUrl("~/PublicPages/Login") %>">Start your journey <span aria-hidden="true">↗</span></a>
        </nav>
        <div class="yu-footer-map">
          <div class="yu-footer-map-heading"><h3>Find the faculty</h3><a href="https://www.google.com/maps/search/?api=1&amp;query=Faculty+of+Information+Technology+and+Computer+Science+Yarmouk+University+Irbid" target="_blank" rel="noopener noreferrer">Directions <span aria-hidden="true">↗</span></a></div>
          <iframe title="Google Maps: Faculty of Information Technology and Computer Science, Yarmouk University, Irbid" src="https://maps.google.com/maps?q=Faculty%20of%20Information%20Technology%20and%20Computer%20Science%20Yarmouk%20University%20Irbid&amp;output=embed" width="480" height="176" loading="lazy" referrerpolicy="no-referrer-when-downgrade" allowfullscreen></iframe>
        </div>
      </div>
      <div class="yu-footer-bottom"><span>© <span data-footer-year>2026</span> Yarmouk University</span><nav aria-label="Footer utilities"><a href="<%= ResolveUrl("~/PublicPages/Home.aspx#faq") %>">FAQ</a><a href="<%= ResolveUrl("~/PublicPages/Home.aspx#top") %>">Back to top <span aria-hidden="true">↑</span></a></nav><span class="yu-footer-powered">Powered by</span></div>
    </footer>
