<%
    dim v
    v = Session.SessionID
    Session.Abandon
    
    Response.Redirect("removing.ashx?" + v)
    %>