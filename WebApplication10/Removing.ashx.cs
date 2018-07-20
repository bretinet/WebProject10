using System;
using System.Web;
using System.Web.Configuration;

namespace ICEProject
{    public class Removing : IHttpHandler
    {
        private const string SessionCookieName = "SessionCookie";
        private const string CookieTempName = "CookieTemp";
        private const string AspNetCookieSessionName = "ASP.NET_SessionID";
        private const string LogOutUrlAppSettings = "LogOutUrl";

        public void ProcessRequest(HttpContext context)
        {
            var tempCookie = context.Request.Cookies[CookieTempName];
            var sessionCookie = context.Request.Cookies[SessionCookieName];

            if (tempCookie?.Value == null)
            {
                if (tempCookie != null)
                {
                    tempCookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(tempCookie);
                }
                if (sessionCookie != null)
                {
                    sessionCookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(sessionCookie);
                }
                GenerateContextResponse(context);
            }

            var tempCookieValue = tempCookie.Value;
            tempCookie.Expires = DateTime.Now.AddDays(-1);
            context.Response.Cookies.Add(tempCookie);

            if (sessionCookie?.Value == null)
            {
                if (sessionCookie != null)
                {
                    sessionCookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(sessionCookie);
                }
                GenerateContextResponse(context);
            }

            //var sessionCookieValue = sessionCookie.Value;
            //sessionCookie.Expires = DateTime.Now.AddDays(-1);
            //context.Response.Cookies.Add(sessionCookie);

            var applicationCookieValue = context.Application[tempCookieValue];

            if (applicationCookieValue != null)
            {
                var sessionUrlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie.Value);

                if (applicationCookieValue.ToString() == sessionUrlDecodeCookieValue)
                {
                    context.Application.Lock();
                    context.Application.Remove(tempCookieValue);
                    context.Application.UnLock();
                }
            }

            var aspNetSessionCookie = context.Request.Cookies[AspNetCookieSessionName];
            if (aspNetSessionCookie != null)
            {
                aspNetSessionCookie.Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(aspNetSessionCookie);
            }
            //context.Response.End();
            //context.Response.Redirect("logon2.asp");
            GenerateContextResponse(context);
        }

        private void GenerateContextResponse(HttpContext context)
        {
            var logOutUrl = WebConfigurationManager.AppSettings[LogOutUrlAppSettings];
            if (string.IsNullOrEmpty(logOutUrl))
            {
                context.Response.End();
            }

            context.Response.Redirect(logOutUrl);
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}