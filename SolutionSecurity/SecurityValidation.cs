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
        //private const string SessionCookieName = "SessionCookie";
        //private const string AspNetSessionId = "ASP.NET_SessionId";
        private const string TemporalCookieName = "CookieTemp";

        private const string DecryptedValueForm = "decryptedValue";

        private const string SecurityActivationAppSetting = "SecurityActivation";
        //private const string LogOutUrlAppSetting = "LogOutUrl";
        //private const string DefaultuUrlAppSetting = "DefaultUrl";
        private const string AllowedPagesAppSetting = "AllowedPages";

        //private const string AddingUrlPage = "Adding.ashx";
        //private const string CheckingUrlPage = "Checking.ashx";
        //private const string RemovingUrlPage = "Removing.ashx";
        private const string TrueValue = "True";

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += Context_AuthenticateRequest;
            context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
            context.BeginRequest += Context_BeginRequest;
            context.PreSendRequestHeaders += Context_PreSendRequestHeaders;
        }

        private void Context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            var cookieTemp = HttpContext.Current.Response.Cookies[TemporalCookieName];
            if (cookieTemp == null) return;

            cookieTemp.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(cookieTemp);
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            //if (!WebConfigurationManager.AppSettings[SecurityActivationAppSetting]
            //    .Equals(TrueValue, StringComparison.CurrentCultureIgnoreCase))
            //{
            //    return;
            //}

            if (!ValidationRequest.IsValidationSecurityActivated())
            {
                return;
            }


            // var defaultUrl = WebConfigurationManager.AppSettings[DefaultuUrlAppSetting];

            //var context = ((HttpApplication)sender).Context;


            //if (context.Request.RawUrl.EndsWith(AddingUrl, StringComparison.CurrentCultureIgnoreCase))
            if (ValidationRequest.IsAdministrationPageRequest(AdministrationPage.AddingSession))
            {
                //var cookie = context.Request.Cookies[TemporalCookieName];

                //if (cookie != null)
                //{
                //    var value = cookie.Value;
                //    var encrypedValue = SecurityEncryption.Encrypt(value);

                //    cookie.Expires = DateTime.Now.AddDays(-1);
                //    context.Response.Cookies.Add(cookie);

                //    context.Application.Lock();
                //    context.Application[value] = encrypedValue;
                //    context.Application.UnLock();

                //    RemoveHttpCookie(TemporalCookieName);

                //    context.Response.Write(encrypedValue);
                //}
                //context.Response.End();
                ValidationRequest.SendAddingSessionPageResponse();
            }

            //if (context.Request.RawUrl.EndsWith(CheckingUrl, StringComparison.CurrentCultureIgnoreCase))
            if (ValidationRequest.IsAdministrationPageRequest(AdministrationPage.CheckingApplicationSession))
            {
                //var sessionDecriptedCookieValue = context.Request.Form[DecryptedValueForm];

                //context.Response.ContentType = "text/plain";

                //var application = HttpContext.Current.Application[sessionDecriptedCookieValue];

                //if (application != null)
                //{
                //    context.Response.Write($"{application}{sessionDecriptedCookieValue}");
                //}

                //context.Response.End();
                ValidationRequest.SendCheckingApplicationPageResponse();
            }


            //if (context.Request.RawUrl.EndsWith(RemovingUrl, StringComparison.CurrentCultureIgnoreCase))
            if (ValidationRequest.IsAdministrationPageRequest(AdministrationPage.RemovingSession))
            {

                //var sessionCookie = context.Request.Cookies[SessionCookieName];

                //var sessionCookieValue2 = sessionCookie?.Value;
                //RemoveHttpCookie(SessionCookieName);

                //if (sessionCookie == null || sessionCookieValue2 == null)
                //{
                //    GenerateContextResponse(DefaultuUrlAppSetting);
                //}

                //var decodedSessionCookieValue = HttpUtility.UrlDecode(sessionCookieValue2);
                //var decryptedSessionValue = SecurityEncryption.Decrypt(decodedSessionCookieValue);

                //context.Application.Lock();
                //context.Application.Remove(decryptedSessionValue);
                //context.Application.UnLock();

                //RemoveHttpCookie(AspNetSessionId);

                //GenerateContextResponse(LogOutUrlAppSetting);

                ValidationRequest.SendRemovingSessionAdministrationPageResponse();
            }


            if (IsRequestedPageInAllowedPagesList())
            {
                return;
            }

            //No cookie Response
            //var sessionCookie1 = HttpContext.Current.Request.Cookies[SessionCookieName];
            //if (sessionCookie1?.Value == null)
            //{
            //    GenerateContextResponse(DefaultuUrlAppSetting);
            //}
            if (IsCookieSessionNullOrNullValue(AdministrationPage.SessionCookieName))
            {
                AdministrationPage.SendRedirectResponse(AdministrationPage.DefaultuUrlAppSetting);
            }


            ///Check for session and application
            //if (sessionCookie1 != null)
            //{
            var sessionCookie = HttpContext.Current.Request.Cookies[AdministrationPage.SessionCookieName];

            string decryptedCookieValue = null;
            string urlDecodeCookieValue = null;

            try
            {
                urlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie.Value);
                decryptedCookieValue = SecurityEncryption.Decrypt(urlDecodeCookieValue);
            }
            catch
            {
                AdministrationPage.SendRedirectResponse(AdministrationPage.DefaultuUrlAppSetting);
            }

            //if (HttpContext.Current.Request.ApplicationPath != null && HttpContext.Current.Request.ApplicationPath.Equals("/"))
            if (IsRootLevelRequest())
            {

                //if (decryptedCookieValue == null || HttpContext.Current.Application[decryptedCookieValue] == null ||
                //    HttpContext.Current.Application[decryptedCookieValue].ToString() != urlDecodeCookieValue)
                if (IsValidRootLevelRequest(urlDecodeCookieValue, decryptedCookieValue))
                {
                    return;
                }
                //AdministrationPage.SendRedirectResponse(DefaultuUrlAppSetting);
            }
            //else if (HttpContext.Current.Request.ApplicationPath != null && IsRequestApplicationInAllowedApplicationsList())
            else if (IsApplicatonLevelRequest())
            {
                //{
                var returnedCredentials = GetApplicationParentSession(decryptedCookieValue);

                //if (returnedCredentials == null ||
                //    returnedCredentials != $"{urlDecodeCookieValue}{decryptedCookieValue}")
                if (IsValidApplicationLevelRequest(returnedCredentials, urlDecodeCookieValue, decryptedCookieValue))
                {
                    return;
                }
                //AdministrationPage.SendRedirectResponse(DefaultuUrlAppSetting);
                //}
            }
            AdministrationPage.SendRedirectResponse(AdministrationPage.DefaultuUrlAppSetting);
            //}
        }


        private static bool IsRequestedPageInAllowedPagesList()
        {
            var context = HttpContext.Current;
            var allowedPages = WebConfigurationManager.AppSettings["AllowedPages"];

            if (allowedPages == null)
            {
                return false;
            }

            var allowedPagesList = allowedPages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var currentpage = context.Request.AppRelativeCurrentExecutionFilePath?.Replace("~/", "");

            return currentpage != null && allowedPagesList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
        }

        private static bool IsRequestApplicationInAllowedApplicationsList()
        {
            var allowedApplications = WebConfigurationManager.AppSettings["AllowedApplications"];

            if (allowedApplications == null)
            {
                return false;
            }

            var allowedApplicationList = allowedApplications.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var currentpage = HttpContext.Current.Request.ApplicationPath?.Replace("/", "");


            return currentpage != null && allowedApplicationList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
        }



        private string GetApplicationParentSession(string decrypted)
        {
            var internalAppRoot = WebConfigurationManager.AppSettings["InternalAppRoot"];
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(internalAppRoot + AdministrationPage.CheckingApplicationSession);
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

        //private void SendRedirectResponse(string configurationSetting)
        //{
        //    var returnUrl = WebConfigurationManager.AppSettings[configurationSetting];

        //    if (string.IsNullOrEmpty(returnUrl))
        //    {
        //        HttpContext.Current.Response.End();
        //    }

        //    HttpContext.Current.Response.Redirect(returnUrl);
        //}

        private void RemoveHttpCookie(string cookieName)
        {
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        private bool IsCookieSessionNullOrNullValue(string cookieName)
        {
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            return cookie?.Value == null;
        }

        private bool IsRootLevelRequest()
        {
            return HttpContext.Current.Request.ApplicationPath != null && HttpContext.Current.Request.ApplicationPath.Equals("/");
        }

        private bool IsValidRootLevelRequest(string decodedCookieValue, string decryptedCookieValue)
        {
            return decryptedCookieValue != null &&
                HttpContext.Current.Application[decryptedCookieValue] != null &&
                HttpContext.Current.Application[decryptedCookieValue].ToString() == decodedCookieValue;
        }

        private bool IsApplicatonLevelRequest()
        {
            return HttpContext.Current.Request.ApplicationPath != null && HttpContext.Current.Request.ApplicationPath.Equals("/");
        }

        private bool IsValidApplicationLevelRequest(string DecodeDecryptedResponse, string decodedCookieValue, string decryptedCookieValue)
        {
            return DecodeDecryptedResponse != null && DecodeDecryptedResponse == $"{decodedCookieValue}{decryptedCookieValue}";
        }

        private static void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {

        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {

        }
    }
}
