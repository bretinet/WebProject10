using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web;

namespace SolutionSecurity
{
    internal class AdministrationRequest
    {

        private const string DecryptedValueForm = "decryptedValue";

        //private const string SecurityActivationAppSetting = "SecurityActivation";
        //private const string TrueValue = "True";

        private const string TemporalCookieName = "CookieTemp";

        //internal static bool IsValidationSecurityActivated()
        //{
        //    return WebConfigurationManager.AppSettings[SecurityActivationAppSetting]
        //        .Equals(TrueValue, StringComparison.CurrentCultureIgnoreCase);
        //}

        //internal static bool IsAdministrationPageRequest(string pageUrl)
        //{
        //    return HttpContext.Current.Request.RawUrl.EndsWith(pageUrl, StringComparison.CurrentCultureIgnoreCase);
        //}

        internal static void SendAddingSessionPageResponse()
        {
            var context = HttpContext.Current;
            var cookie = context.Request.Cookies[TemporalCookieName];

            if (cookie != null)
            {
                var value = cookie.Value;
                var encrypedValue = SecurityEncryption.Encrypt(value);

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
            var context = HttpContext.Current;
            var sessionDecriptedCookieValue = context.Request.Form[DecryptedValueForm];

            context.Response.ContentType = "text/plain";

            var application = HttpContext.Current.Application[sessionDecriptedCookieValue];

            if (application != null)
            {
                context.Response.Write($"{application}{sessionDecriptedCookieValue}");
            }

            context.Response.End();
        }

        internal static void SendRemovingSessionPageResponse()
        {
            var context = HttpContext.Current;
            var sessionCookie = context.Request.Cookies[SecurityManager.SessionCookieName];

            var sessionCookieValue = sessionCookie?.Value;
            SecurityManager.RemoveHttpCookie(SecurityManager.SessionCookieName);

            if (sessionCookie == null || sessionCookieValue == null)
            {
                SecurityManager.SendRedirectResponse(ApplicationResponseType.defaultUrl);
            }

            var decodedSessionCookieValue = HttpUtility.UrlDecode(sessionCookieValue);
            var decryptedSessionValue = SecurityEncryption.Decrypt(decodedSessionCookieValue);

            context.Application.Lock();
            context.Application.Remove(decryptedSessionValue);
            context.Application.UnLock();

            SecurityManager.RemoveHttpCookie(SecurityManager.AspNetSessionIdCookieName);

            SecurityManager.SendRedirectResponse(ApplicationResponseType.logOutUrl);
        }
    }
}
