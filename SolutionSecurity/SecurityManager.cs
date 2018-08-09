using System;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace SolutionSecurity
{
    internal class SecurityManager
    {
        internal const string SessionCookieName = "SessionCookie";
        internal const string AspNetSessionIdCookieName = "ASP.NET_SessionId";

        private const string SecurityActivationAppSetting = "SecurityActivation";
        internal const string DefaultRootUrlAppSetting = "DefaultRootUrl";
        internal const string DefaultChildUrlAppSetting = "DefaultChildUrl";
        internal const string LogOutUrlAppSetting = "LogOutUrl";
        internal const string LogOutChildUrlAppSetting = "LogOutChildUrl";

        private const string AllowedPagesAppSetting = "AllowedPages";

        internal const string AddingSessionUrl = "Adding.ashx";
        internal const string CheckingSessionUrl = "Checking.ashx";
        internal const string RemovingSessionUrl = "Removing.ashx";

        private const string TrueValue = "True";

        internal static ApplicationLevelRequestType? ApplicationLevelRequest { get; set; }

        internal static void RemoveHttpCookie(string cookieName)
        {
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        internal static void SendRedirectResponse(ApplicationResponseType responseType)
        {
            string responseAppSetting = GetResponseByLevelRequest(responseType);

            string returnUrl = WebConfigurationManager.AppSettings[responseAppSetting];

            if (string.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Current.Response.End();
            }

            HttpContext.Current.Response.Redirect(returnUrl);
        }

        internal static string GetResponseByLevelRequest(ApplicationResponseType responseType)
        {
            if (ApplicationLevelRequest == null)
            {
                ApplicationLevelRequest = GetApplicationLevelRequest();
            }

            if (ApplicationLevelRequest == ApplicationLevelRequestType.RootapplicationLevel)
            {
                return responseType == ApplicationResponseType.LogOutUrl ? LogOutUrlAppSetting : DefaultRootUrlAppSetting;
            }

            if (ApplicationLevelRequest == ApplicationLevelRequestType.ChildApplicationLevel)
            {
                return responseType == ApplicationResponseType.LogOutUrl ? LogOutChildUrlAppSetting : DefaultChildUrlAppSetting;
            }
            return DefaultRootUrlAppSetting;
        }

        internal static bool IsSecurityActivated()
        {
            return WebConfigurationManager
                .AppSettings[SecurityActivationAppSetting]
                .Equals(TrueValue, StringComparison.CurrentCultureIgnoreCase);
        }

        internal static bool IsRequestToSecurity(string pageUrl)
        {
            return HttpContext.Current.Request.RawUrl.EndsWith(
                pageUrl, 
                StringComparison.CurrentCultureIgnoreCase);
        }

        internal static bool IsRequestInAllowedPages()
        {
            var allowedPages = WebConfigurationManager.AppSettings[AllowedPagesAppSetting];

            if (allowedPages == null)
            {
                return false;
            }

            var allowedPagesList = allowedPages
                                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .ToList();

            var currentpage = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath?.Replace("~/", "");

            return currentpage != null && allowedPagesList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
        }

        internal static ApplicationLevelRequestType GetApplicationLevelRequest()
        {
            var applicationPath = HttpContext.Current.Request.ApplicationPath;

            if (applicationPath == null)
            {
                return ApplicationLevelRequestType.None;
            }

            if (applicationPath.Equals("/"))
            {
                return ApplicationLevelRequestType.RootapplicationLevel;
            }
            else
            {
                return ApplicationLevelRequestType.ChildApplicationLevel;
            }
        }
    }

    internal enum ApplicationLevelRequestType
    {
        None,
        RootapplicationLevel,
        ChildApplicationLevel
    }


    internal enum ApplicationResponseType
    {
        DefaultUrl,
        LogOutUrl
    }
}
