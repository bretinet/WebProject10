using System;
using System.Web;

namespace SolutionSecurity
{
    internal class AdministrationRequest
    {
        private const string DecryptedValueForm = "decryptedValue";
        private const string TemporalCookieName = "CookieTemp";


        internal static void SendAddingSessionPageResponse()
        {
            HttpContext context = HttpContext.Current;
            HttpCookie cookie = context.Request.Cookies[TemporalCookieName];

            if (cookie != null)
            {
                string value = cookie.Value;
                string encrypedValue = SecurityEncryption.Encrypt(value);

                cookie.Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(cookie);

                context.Application.Lock();
                context.Application[value] = encrypedValue;
                context.Application.UnLock();

                SecurityManager.RemoveHttpCookie(TemporalCookieName);

                context.Response.Write(encrypedValue);
            }
            context.Response.End();
        }

        internal static void SendCheckingApplicationPageResponse()
        {
            HttpContext context = HttpContext.Current;
            string sessionDecriptedCookieValue = context.Request.Form[DecryptedValueForm];

            context.Response.ContentType = "text/plain";

            object application = HttpContext.Current.Application[sessionDecriptedCookieValue];

            if (application != null)
            {
                context.Response.Write($"{application}{sessionDecriptedCookieValue}");
            }

            context.Response.End();
        }

        internal static void SendRemovingSessionPageResponse()
        {
            HttpContext context = HttpContext.Current;
            HttpCookie sessionCookie = context.Request.Cookies[SecurityManager.SessionCookieName];

            string sessionCookieValue = sessionCookie?.Value;
            SecurityManager.RemoveHttpCookie(SecurityManager.SessionCookieName);

            if (sessionCookie == null || sessionCookieValue == null)
            {
                SecurityManager.SendRedirectResponse(ApplicationResponseType.DefaultUrl);
            }

            string decodedSessionCookieValue = HttpUtility.UrlDecode(sessionCookieValue);
            string decryptedSessionValue = SecurityEncryption.Decrypt(decodedSessionCookieValue);

            context.Application.Lock();
            context.Application.Remove(decryptedSessionValue);
            context.Application.UnLock();

            SecurityManager.RemoveHttpCookie(SecurityManager.AspNetSessionIdCookieName);

            SecurityManager.SendRedirectResponse(ApplicationResponseType.LogOutUrl);
        }
    }
}
