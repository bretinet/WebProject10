<%
    If Session("LogOn") = True then
        Response.Write (Session.SessionID)
    else
        Response.Redirect("login.asp")
    End if
    %>