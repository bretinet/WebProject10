using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace SolutionSecurity
{
    public class SecurityValidation : IHttpModule
    {
        private const string DecryptedValueForm = "decryptedValue";
        private const string InternalApplicationRootAppSetting = "InternalAppRoot";
        private const string AllowedApplicationsAppSetting = "AllowedApplications";

        private static string CookieValue { get; set; }
        private static string UrldecodeCookieValue { get; set; }
        private static string DecryptedCookieValue { get; set; }


        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
            //context.AuthenticateRequest += Context_AuthenticateRequest;
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

            if (SecurityManager.IsRequestToInternalSecurityPage(SecurityManager.AddingSessionUrl))
            {
                AdministrationRequest.SendAddingSessionPageResponse();
            }

            if (SecurityManager.IsRequestToInternalSecurityPage(SecurityManager.CheckingSessionUrl))
            {
                AdministrationRequest.SendCheckingApplicationPageResponse();
            }

            if (SecurityManager.IsRequestToInternalSecurityPage(SecurityManager.RemovingSessionUrl))
            {
                AdministrationRequest.SendRemovingSessionPageResponse();
            }

            if (SecurityManager.IsRequestedPageInAllowedPageList())
            {
                return;
            }

            if (!IsValidSessionCookie())
            {
                SecurityManager.SendRedirectResponse(ApplicationResponseType.DefaultUrl);
            }

            if (!IsValidApplicationSession())
            {
                SecurityManager.SendRedirectResponse(ApplicationResponseType.DefaultUrl);
            }
        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {
            //if (SecurityManager.IsRequestedPageInAllowedPageList())
            //{
            //    return;
            //}

            //if (!IsValidSessionCookie())
            //{
            //    SecurityManager.SendRedirectResponse(ApplicationResponseType.defaultUrl);
            //}

            //if (!IsValidApplicationSession())
            //{
            //    SecurityManager.SendRedirectResponse(ApplicationResponseType.defaultUrl);
            //}
        }

        private static bool IsValidSessionCookie()
        {
            return !IsCookieSessionNullOrNullValue() && IsValidDecryptedSessionCookieValue();
        }

        private static bool IsCookieSessionNullOrNullValue()
        {
            var cookie = HttpContext.Current.Request.Cookies[SecurityManager.SessionCookieName];

            if (cookie?.Value != null)
            {
                CookieValue = cookie.Value;
                return false;
            }

            return true;
        }

        private static bool IsValidDecryptedSessionCookieValue()
        {
            UrldecodeCookieValue = HttpUtility.UrlDecode(CookieValue);
            DecryptedCookieValue = GetDecryptedCookieValue(UrldecodeCookieValue);

            return DecryptedCookieValue != null;
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
            ApplicationLevelRequestType applicationLevelRequest = SecurityManager.GetApplicationLevelRequest();

            if (applicationLevelRequest == ApplicationLevelRequestType.RootapplicationLevel &&
                IsValidRootLevelApplicationRequest(UrldecodeCookieValue, DecryptedCookieValue))
            {
                return true;
            }

            if (applicationLevelRequest == ApplicationLevelRequestType.ChildApplicationLevel && IsRequestApplicationInAllowedApplicationsList())
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
    }
}
