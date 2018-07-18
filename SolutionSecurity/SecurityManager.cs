using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace SolutionSecurity
{
    internal class SecurityManager
    {
        internal const string SessionCookieName = "SessionCookie";
        internal const string LogOutUrlAppSetting = "LogOutUrl";
        internal const string AspNetSessionId = "ASP.NET_SessionId";

        internal const string DefaultuUrlAppSetting = "DefaultUrl";

        internal const string AddingSession = "Adding.ashx";
        internal const string CheckingSession = "Checking.ashx";
        internal const string RemovingSession = "Removing.ashx";
        
        internal static void RemoveHttpCookie(string cookieName)
        {
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        internal static void SendRedirectResponse(string configurationSetting)
        {
            var returnUrl = WebConfigurationManager.AppSettings[configurationSetting];

            if (string.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Current.Response.End();
            }

            HttpContext.Current.Response.Redirect(returnUrl);
        }
    }
}
