<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title></title>
</head>
<body>
    <!--#include file="validation.asp"-->
    <!--#include file="header.asp"-->
    <% 
        
          
          
          Response.Write(SessionID) %>
        <!--#include file="TempCookie.asp" -->
<!--          Response.Cookies("TempSession") = Session.SessionID  + "BBBBB"
          Response.Cookies("TempSession".Expires = DateAdd("s", 5, Now())
          Response.Cookies("TempSession").Secure = true
           
         %>-->
    <form action="WebForm1.aspx" method="post">
        <input type="hidden" value="<% Session.SessionID %>" />
        <input type="submit" value="Go" />
    </form>
    <iframe src="http://localhost/ICEPermissions2018/Default.aspx" width="100%"></iframe>
</body>
</html>
