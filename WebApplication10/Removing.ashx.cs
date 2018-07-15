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
        private const string CookieTest = "cookietest";

        public void ProcessRequest(HttpContext context)
        {
            //// var cont = context.Request.QueryString;
            //// //context.Application.Lock();
            //// context.Application.Remove(cont.ToString());
            ////// context.Application.UnLock();

          

            var cookie = context.Request.Cookies[CookieTest];

            if (cookie != null)
            {
                var value = cookie.Value;


                cookie.Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(cookie);

                var sessionCookie = context.Request.Cookies[SessionCookieName];

                if (sessionCookie != null)
                {
                    var sessionCookieValue = HttpUtility.UrlDecode(sessionCookie.Value);
                    var applicationCookie = context.Application[value];
                    if (applicationCookie.ToString() == sessionCookieValue)
                    {
                        context.Application.Lock();
                        context.Application.Remove(value);
                        context.Application.UnLock();
                    }

                    sessionCookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(sessionCookie);

                    var dd = context.Request.Cookies["ASP.NET_SessionID"];
                    if (dd != null)
                    {
                        dd.Expires = DateTime.Now.AddDays(-1);
                        context.Response.Cookies.Add(dd);
                    }
                }


                context.Response.Redirect("default.asp");


            }
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