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
        private const string AllowedPagesAppSetting = "AllowedPages";
        private const string AllowedApplicationsAppSetting = "AllowedApplications";

        //private const string AddingUrlPage = "Adding.ashx";
        //private const string CheckingUrlPage = "Checking.ashx";
        //private const string RemovingUrlPage = "Removing.ashx";
        //private const string TrueValue = "True";

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
            if (!ValidationRequest.IsValidationSecurityActivated())
            {
                return;
            }

            if (ValidationRequest.IsAdministrationPageRequest(SecurityManager.AddingSession))
            {
                ValidationRequest.SendAddingSessionPageResponse();
            }

            if (ValidationRequest.IsAdministrationPageRequest(SecurityManager.CheckingSession))
            {
                ValidationRequest.SendCheckingApplicationPageResponse();
            }

            if (ValidationRequest.IsAdministrationPageRequest(SecurityManager.RemovingSession))
            {
                ValidationRequest.SendRemovingSessionPageResponse();
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
            if (IsRequestedPageInAllowedPageList())
            {
                return;
            }

            if (!IsValidSessionCookie())
            {
                SecurityManager.SendRedirectResponse(SecurityManager.DefaultuUrlAppSetting);
            }

            if (!IsValidApplicationSession())
            {
                SecurityManager.SendRedirectResponse(SecurityManager.DefaultuUrlAppSetting);
            }
        }

        private static bool IsValidSessionCookie()
        {
            return !IsCookieSessionNullOrNullValue() && IsValidDecryptedSessionCookieValue();
        }

        private bool IsValidApplicationSession()
        {
            var sessionCookie = HttpContext.Current.Request.Cookies[SecurityManager.SessionCookieName];
            var urlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie?.Value);
            var decryptedCookieValue = GetDecryptedCookieValue(urlDecodeCookieValue);

            if (IsRootLevelApplicationRequest() && IsValidRootLevelApplicationRequest(urlDecodeCookieValue, decryptedCookieValue))
            {
                return true;
            }

            if (IsChildLevelApplicatonRequest())
            {
                var credentials = GetApplicationParentSessionCredentials(decryptedCookieValue);
                if (IsValidChildLevelApplicationRequest(credentials, urlDecodeCookieValue, decryptedCookieValue))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsValidDecryptedSessionCookieValue()
        {
            const string cookieName = SecurityManager.SessionCookieName;

            var sessionCookie = HttpContext.Current.Request.Cookies[cookieName];
            var urlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie?.Value);
            var decryptedCookieValue = GetDecryptedCookieValue(urlDecodeCookieValue);

            return decryptedCookieValue != null;
        }

        private static bool IsRequestedPageInAllowedPageList()
        {
            var context = HttpContext.Current;
            var allowedPages = WebConfigurationManager.AppSettings[AllowedPagesAppSetting];

            if (allowedPages == null)
            {
                return false;
            }

            var allowedPagesList = allowedPages
                                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .ToList();

            var currentpage = context.Request.AppRelativeCurrentExecutionFilePath?.Replace("~/", "");

            return currentpage != null && allowedPagesList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
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

        private string GetApplicationParentSessionCredentials(string decrypted)
        {
            var internalAppRoot = WebConfigurationManager.AppSettings[InternalApplicationRootAppSetting];
            try
            {
                var request = (HttpWebRequest)WebRequest.Create($"{internalAppRoot}{SecurityManager.CheckingSession}");
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

        private static bool IsCookieSessionNullOrNullValue()
        {
            const string cookieName = SecurityManager.SessionCookieName;
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            return cookie?.Value == null;
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

        private static bool IsValidChildLevelApplicationRequest(
                                                            string decodeDecryptedResponse,
                                                            string decodedCookieValue,
                                                            string decryptedCookieValue)
        {
            return decodeDecryptedResponse != null &&
                decodeDecryptedResponse == $"{decodedCookieValue}{decryptedCookieValue}";
        }
    }
}
