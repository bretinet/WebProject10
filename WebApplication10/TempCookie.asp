<%
    If Session("LogOn") = True then
        
          Response.Cookies("TempSession") = Session.SessionID  + "BBBBB"
          Response.Cookies("TempSession").Expires = DateAdd("s", 5, Now())
          Response.Cookies("TempSession").Secure = true
    else
        Response.Redirect("login.asp")
    End if
    %>