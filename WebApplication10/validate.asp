<%
    If Request.Form("UserId") = "S" then
        Session("LogOn") = True
        Response.Cookies("SessionCookie") = Session.SessionID
        Response.Cookies("SessionCookie").Expires = DateAdd("n", 30, Now())
        Response.Cookies("SessionCookie").Secure = true
        
        Response.Redirect("adding.ashx?"+Session.SessionID)
    End if 
     
%>

<!--Response.Redirect("default.asp")-->