using System;
using System.Web;

namespace SolutionSecurity
{
    public class SecurityValidation : IHttpModule
    {
        private const string SecurityCookieName = "TempSession";
        private const string SessionCookieName = "SessionCookie";


        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += Context_AuthenticateRequest;
            context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
        }

        private void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            var securityCookie = context.Request.Cookies[SecurityCookieName];
            if (securityCookie != null)
            {
                securityCookie.Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(securityCookie);
            }
        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {
            string sessionCookieValue = null;
            string SecurityCookieValue = string.Empty;

            var context = ((HttpApplication)sender).Context;

            var sessionCookie = context.Request.Cookies [SessionCookieName];

            if (sessionCookie != null && sessionCookie.Value != null && sessionCookie.Value.Length > 4)
            {
                sessionCookieValue = sessionCookie.Value.Substring(0, sessionCookie.Value.Length - 4);
            }


            var securityCookie = context.Request.Cookies[SecurityCookieName];
            if (securityCookie != null && securityCookie.Value != null && securityCookie.Value.Length > 5)
            {
                SecurityCookieValue = securityCookie.Value.Substring(0, securityCookie.Value.Length - 5);
            }

            //if (sessionCookieValue == null || SecurityCookieValue == null|| sessionCookieValue != SecurityCookieValue)
            //{
            //    context.Response.Redirect("~/login.asp");
            //}
        }
    }
}
