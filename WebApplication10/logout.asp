<%
    Function GetTextFromUrl(url)

  Dim oXMLHTTP
  Dim strStatusTest

  Set oXMLHTTP = CreateObject("MSXML2.ServerXMLHTTP.3.0")

  oXMLHTTP.Open "GET", url, False
  oXMLHTTP.setRequestHeader "Cookie", "cookietest=" + Session.SessionID
  oXMLHTTP.Send

    Session.Abandon
  If oXMLHTTP.Status = 200 Then

        GetTextFromUrl = oXMLHTTP.responseText
        Response.Cookies("SessionCookie") = GetTextFromUrl
        Response.Cookies("SessionCookie").Expires = DateAdd("n", -1, Now())


        
        Response.Redirect("default.asp")
  End If

End Function


    
    %>

<%

  
    dim currentSession
    currentSession = Session.SessionID
    Session.Abandon
    Response.Cookies("cookietest") = currentSession
     Response.Cookies("cookietest").Expires = DateAdd("s", 5, Now())
    Response.Redirect("removing.ashx")

    %>