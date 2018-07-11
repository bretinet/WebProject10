<%
    Function GetTextFromUrl(url)

  Dim oXMLHTTP
  Dim strStatusTest

  Set oXMLHTTP = CreateObject("MSXML2.ServerXMLHTTP.3.0")

  oXMLHTTP.Open "GET", url, False
  oXMLHTTP.setRequestHeader "Cookie", "cookietest=" + Session.SessionID
  oXMLHTTP.Send

  If oXMLHTTP.Status = 200 Then

        GetTextFromUrl = oXMLHTTP.responseText
        Response.Cookies("SessionCookie") = GetTextFromUrl
        Response.Cookies("SessionCookie").Expires = DateAdd("n", 30, Now())


        Response.Redirect("default.asp")
  End If

End Function


    
    %>

<%
    If Request.Form("UserId") = "S" then
        Session("LogOn") = True
  
        
        GetTextFromUrl("https://localhost:44334/adding.ashx?")
    End if 
     
%>

<!--Response.Redirect("adding.ashx?"+Session.SessionID)-->
<!--Response.Redirect("default.asp")-->


