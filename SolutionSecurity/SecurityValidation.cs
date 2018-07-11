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
            context.BeginRequest += Context_BeginRequest;
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (context.Request.CurrentExecutionFilePath == "/adding.ashx")
            {
                const string cookieTest = "cookietest";

                var cookie = context.Request.Cookies[cookieTest];

                if (cookie != null)
                {
                    var value = cookie.Value;
                    var encrypedValue = SecurityEncryption.Encrypt(value);

                    cookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(cookie);

                    context.Application.Lock();
                    context.Application[value] = encrypedValue;
                    context.Application.UnLock();

                    context.Response.Write(encrypedValue);
                    context.Response.End();
                }
            }

            if (context.Request.CurrentExecutionFilePath == "/removing.ashx")
            {
                const string cookieTest = "cookietest";

                var cookie = context.Request.Cookies[cookieTest];

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

                    //context.Response.Write(encrypedValue);
                    //context.Response.End();
                }
            }
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
            if (context.Request.CurrentExecutionFilePath == "/webform2.aspx")
            {
                return;
            }

            var sessionCookie = context.Request.Cookies[SessionCookieName];

            if (sessionCookie?.Value == null)
            {
                context.Response.Redirect("login.asp");
            }

            //if (sessionCookie?.Value != null)
            //{
            sessionCookieValue = HttpUtility.UrlDecode(sessionCookie?.Value);
            var decryptedCookieValue = SecurityEncryption.Decrypt(sessionCookieValue);

            var applicationCookie = context.Application[decryptedCookieValue];

            if (applicationCookie == null || applicationCookie.ToString() != sessionCookieValue)
            {
                context.Response.Redirect("login.asp");
            }
            //}


            //var securityCookie = context.Request.Cookies[SecurityCookieName];
            //if (securityCookie != null && securityCookie.Value != null && securityCookie.Value.Length > 5)
            //{
            //    SecurityCookieValue = securityCookie.Value.Substring(0, securityCookie.Value.Length - 5);
            //}

            //if (sessionCookieValue == null || SecurityCookieValue == null|| sessionCookieValue != SecurityCookieValue)
            //{
            //    context.Response.Redirect("~/login.asp");
            //}
        }
    }
}
