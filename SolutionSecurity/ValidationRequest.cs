using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web;

namespace SolutionSecurity
{
    internal class ValidationRequest
    {

        private const string DecryptedValueForm = "decryptedValue";

        private const string SecurityActivationAppSetting = "SecurityActivation";
        private const string TrueValue = "True";

        private const string TemporalCookieName = "CookieTemp";

        private const string AddingUrl = "Adding.ashx";

        //internal bool IsRequestAuthorized()
        //{

        //}

        internal static bool IsValidationSecurityActivated()
        {
            return WebConfigurationManager.AppSettings[SecurityActivationAppSetting]
                .Equals(TrueValue, StringComparison.CurrentCultureIgnoreCase);
        }


        internal static bool IsAdministrationPageRequest(string pageUrl)
        {
            return HttpContext.Current.Request.RawUrl.EndsWith(pageUrl, StringComparison.CurrentCultureIgnoreCase);
        }

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

                AdministrationPage.RemoveHttpCookie(TemporalCookieName);

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

        internal static void SendRemovingSessionAdministrationPageResponse()
        {
            var context = HttpContext.Current;
            var sessionCookie = context.Request.Cookies[AdministrationPage.SessionCookieName];

            var sessionCookieValue2 = sessionCookie?.Value;
            AdministrationPage.RemoveHttpCookie(AdministrationPage.SessionCookieName);

            if (sessionCookie == null || sessionCookieValue2 == null)
            {
                AdministrationPage.SendRedirectResponse(AdministrationPage.DefaultuUrlAppSetting);
            }

            var decodedSessionCookieValue = HttpUtility.UrlDecode(sessionCookieValue2);
            var decryptedSessionValue = SecurityEncryption.Decrypt(decodedSessionCookieValue);

            context.Application.Lock();
            context.Application.Remove(decryptedSessionValue);
            context.Application.UnLock();

            AdministrationPage.RemoveHttpCookie(AdministrationPage.AspNetSessionId);

            AdministrationPage.SendRedirectResponse(AdministrationPage.LogOutUrlAppSetting);
        }

        //private static void RemoveHttpCookie(string cookieName)
        //{
        //    var cookie = HttpContext.Current.Request.Cookies[cookieName];
        //    if (cookie != null)
        //    {
        //        cookie.Expires = DateTime.Now.AddDays(-1);
        //        HttpContext.Current.Response.Cookies.Add(cookie);
        //    }
        //}
    }
}
