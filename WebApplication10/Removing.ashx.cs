using System;
using System.Web;

namespace ICEProject
{
    /// <summary>
    /// Summary description for Removing
    /// </summary>
    public class Removing : IHttpHandler
    {
        private const string SessionCookieName = "SessionCookie";
        private const string CookieTempName = "CookieTemp";
        private const string AspNetCookieSessionName = "ASP.NET_SessionID";

        public void ProcessRequest(HttpContext context)
        {
            var tempCookie = context.Request.Cookies[CookieTempName];
            var sessionCookie = context.Request.Cookies[SessionCookieName];

            if (tempCookie == null || tempCookie.Value == null)
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
                context.Response.Redirect("logon2.asp");
                //context.Response.End();
            }

            var tempCookieValue = tempCookie.Value;
            tempCookie.Expires = DateTime.Now.AddDays(-1);
            context.Response.Cookies.Add(tempCookie);

            if (sessionCookie == null || sessionCookie.Value == null)
            {
                if (sessionCookie != null)
                {
                    sessionCookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(sessionCookie);
                }
                //context.Response.End();
                context.Response.Redirect("logon2.asp");
            }

            var sessionCookieValue = sessionCookie.Value;
            sessionCookie.Expires = DateTime.Now.AddDays(-1);
            context.Response.Cookies.Add(sessionCookie);

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
            context.Response.Redirect("logon2.asp");
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