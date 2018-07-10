<!--#include file="validation.asp"-->
<!--#include file="header.asp"-->
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title></title>
</head>
<body>
    default 2 asp

    <%
for each x in Request.ServerVariables
  response.write(x &  Request.ServerVariables (x) & "<br>" )
next
%>

    <br />

    <% Response.Write Request.ServerVariables ("REMOTE_ADDR") %>
</body>
</html>