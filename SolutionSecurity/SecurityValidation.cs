using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace SolutionSecurity
{
    public class SecurityValidation : IHttpModule
    {
        //private const string SessionCookieName = "SessionCookie";
        //private const string AspNetSessionId = "ASP.NET_SessionId";
        //private const string TemporalCookieName = "CookieTemp";

        private const string DecryptedValueForm = "decryptedValue";

        //private const string SecurityActivationAppSetting = "SecurityActivation";
        //private const string LogOutUrlAppSetting = "LogOutUrl";
        //private const string DefaultuUrlAppSetting = "DefaultUrl";
        private const string InternalApplicationRootAppSetting = "InternalAppRoot";
        //private const string AllowedPagesAppSetting = "AllowedPages";
        private const string AllowedApplicationsAppSetting = "AllowedApplications";

        //private const string AddingUrlPage = "Adding.ashx";
        //private const string CheckingUrlPage = "Checking.ashx";
        //private const string RemovingUrlPage = "Removing.ashx";
        //private const string TrueValue = "True";

        private static string CookieValue { get; set; }
        private static string UrldecodeCookieValue { get; set; }
        private static string DecryptedCookieValue { get; set; }

        //internal static ApplicationLevelRequestType ApplicationLevelRequest { get; set; }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
            context.AuthenticateRequest += Context_AuthenticateRequest;
        }

        public void Dispose()
        {
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (!SecurityManager.IsValidationSecurityActivated())
            {
                return;
            }

            if (SecurityManager.IsAdministrationPageRequest(SecurityManager.AddingSessionUrl))
            {
                AdministrationRequest.SendAddingSessionPageResponse();
            }

            if (SecurityManager.IsAdministrationPageRequest(SecurityManager.CheckingSessionUrl))
            {
                AdministrationRequest.SendCheckingApplicationPageResponse();
            }

            if (SecurityManager.IsAdministrationPageRequest(SecurityManager.RemovingSessionUrl))
            {
                AdministrationRequest.SendRemovingSessionPageResponse();
            }


            //if (IsRequestedPageInAllowedPagesList())
            //{
            //    return;
            //}

            //if (!IsValidSessionCookie(AdministrationPage.SessionCookieName))
            //{
            //    AdministrationPage.SendRedirectResponse(AdministrationPage.DefaultuUrlAppSetting);
            //}

            //if (!IsValidApplicationSession())
            //{
            //    AdministrationPage.SendRedirectResponse(AdministrationPage.DefaultuUrlAppSetting);
            //}
        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {           
            if (SecurityManager.IsRequestedPageInAllowedPageList())
            {
                return;
            }

            SecurityManager.ApplicationLevelRequest = SecurityManager.GetApplicationLevelRequest();

            if (!IsValidSessionCookie())
            {
                SecurityManager.SendRedirectResponse(ApplicationResponseType.defaultUrl);
            }

            if (!IsValidApplicationSession())
            {
                SecurityManager.SendRedirectResponse(ApplicationResponseType.defaultUrl);
            }
        }

        private static bool IsValidSessionCookie()
        {
            return !IsCookieSessionNullOrNullValue() && IsValidDecryptedSessionCookieValue();
        }

        private static bool IsCookieSessionNullOrNullValue()
        {           
            var cookie = HttpContext.Current.Request.Cookies[SecurityManager.SessionCookieName];
            CookieValue = cookie.Value;
            return cookie?.Value == null ?  true : false;
        }

        private static bool IsValidDecryptedSessionCookieValue()
        {
            //const string sessionCookieName = SecurityManager.SessionCookieName;

            //var sessionCookie = HttpContext.Current.Request.Cookies[sessionCookieName];
            //var urlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie?.Value);
            //var urlDecodeCookieValue = HttpUtility.UrlDecode(cookieValue2);
            UrldecodeCookieValue = HttpUtility.UrlDecode(CookieValue);
            //var decryptedCookieValue = GetDecryptedCookieValue(urlDecodeCookieValue);
            DecryptedCookieValue = GetDecryptedCookieValue(UrldecodeCookieValue);
            return DecryptedCookieValue != null;


            //if (decryptedCookieValue != null)
            //{
            //    DecryptedCookieValue = decryptedCookieValue;
            //    return true;
            //}
            //else
            //{
            //    DecryptedCookieValue = null;
            //    return false;
            //}
        }

        private static string GetDecryptedCookieValue(string urlDecodeCookieValue)
        {
            try
            {
                return SecurityEncryption.Decrypt(urlDecodeCookieValue);
            }
            catch
            {
                return null;
            }
        }


        private bool IsValidApplicationSession()
        {
            //var sessionCookie = HttpContext.Current.Request.Cookies[SecurityManager.SessionCookieName];
            //var urlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie?.Value);
            //var decryptedCookieValue = GetDecryptedCookieValue(urlDecodeCookieValue);

            if (IsRootLevelApplicationRequest() && IsValidRootLevelApplicationRequest(UrldecodeCookieValue, DecryptedCookieValue))
            {
                return true;
            }

            if (IsChildLevelApplicatonRequest())
            {
                var credentials = GetApplicationParentSessionCredentials(DecryptedCookieValue);
                if (IsValidChildLevelApplicationRequest(credentials, UrldecodeCookieValue, DecryptedCookieValue))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsRootLevelApplicationRequest()
        {
            var applicationPath = HttpContext.Current.Request.ApplicationPath;
            return applicationPath != null && applicationPath.Equals("/");
        }

        private static bool IsValidRootLevelApplicationRequest(string decodedCookieValue, string decryptedCookieValue)
        {
            return decryptedCookieValue != null &&
                HttpContext.Current.Application[decryptedCookieValue] != null &&
                HttpContext.Current.Application[decryptedCookieValue].ToString() == decodedCookieValue;
        }

        private static bool IsChildLevelApplicatonRequest()
        {
            return HttpContext.Current.Request.ApplicationPath != null &&
                    !HttpContext.Current.Request.ApplicationPath.Equals("/") &&
                    IsRequestApplicationInAllowedApplicationsList();
        }

        private static bool IsRequestApplicationInAllowedApplicationsList()
        {
            var allowedApplications = WebConfigurationManager.AppSettings[AllowedApplicationsAppSetting];

            if (allowedApplications == null)
            {
                return false;
            }

            var allowedApplicationList = allowedApplications.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var currentpage = HttpContext.Current.Request.ApplicationPath?.Replace("/", "");


            return currentpage != null && allowedApplicationList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
        }

        private string GetApplicationParentSessionCredentials(string decrypted)
        {
            var internalAppRoot = WebConfigurationManager.AppSettings[InternalApplicationRootAppSetting];
            try
            {
                var request = (HttpWebRequest)WebRequest.Create($"{internalAppRoot}{SecurityManager.CheckingSessionUrl}");
                var postData = $"{DecryptedValueForm}={decrypted}";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                if (response.GetResponseStream() != null)
                {
                    var reader = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    return !string.IsNullOrEmpty(reader) ? reader : null;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsValidChildLevelApplicationRequest(
                                                    string decodeDecryptedResponse,
                                                    string decodedCookieValue,
                                                    string decryptedCookieValue)
        {
            return decodeDecryptedResponse != null &&
                decodeDecryptedResponse == $"{decodedCookieValue}{decryptedCookieValue}";
        }
        //private static bool IsRequestedPageInAllowedPageList()
        //{
        //    var context = HttpContext.Current;
        //    var allowedPages = WebConfigurationManager.AppSettings[AllowedPagesAppSetting];

        //    if (allowedPages == null)
        //    {
        //        return false;
        //    }

        //    var allowedPagesList = allowedPages
        //                            .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
        //                            .Select(s => s.Trim())
        //                            .ToList();

        //    var currentpage = context.Request.AppRelativeCurrentExecutionFilePath?.Replace("~/", "");

        //    return currentpage != null && allowedPagesList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
        //}





       










    }
}
