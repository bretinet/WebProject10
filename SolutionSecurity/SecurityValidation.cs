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
        private const string SessionCookieName = "SessionCookie";
        private const string AspNetSessionId = "ASP.NET_SessionId";
        private const string TemporalCookieName = "CookieTemp";

        private const string DecryptedValueForm = "decryptedValue";

        private const string SecurityActivationAppSetting = "SecurityActivation";
        private const string LogOutUrlAppSetting = "LogOutUrl";
        private const string DefaultuUrlAppSetting = "DefaultUrl";
        private const string AllowedPagesAppSetting = "AllowedPages";

        private const string AddingUrl = "Adding.ashx";
        private const string CheckingUrl = "Checking.ashx";
        private const string RemovingUrl = "Removing.ashx";
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
            if (!WebConfigurationManager.AppSettings[SecurityActivationAppSetting]
                .Equals(TrueValue, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }


           // var defaultUrl = WebConfigurationManager.AppSettings[DefaultuUrlAppSetting];

            var context = ((HttpApplication)sender).Context;


            if (context.Request.RawUrl.EndsWith(AddingUrl, StringComparison.CurrentCultureIgnoreCase))
            {
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

                    RemoveHttpCookie(TemporalCookieName);

                    context.Response.Write(encrypedValue);
                }
                context.Response.End();
            }

            if (context.Request.RawUrl.EndsWith(CheckingUrl, StringComparison.CurrentCultureIgnoreCase))
            {
                var sessionDecriptedCookieValue = context.Request.Form[DecryptedValueForm];

                context.Response.ContentType = "text/plain";

                var application = HttpContext.Current.Application[sessionDecriptedCookieValue];

                if (application != null)
                {
                    context.Response.Write($"{application}{sessionDecriptedCookieValue}");
                }

                context.Response.End();
            }


            if (context.Request.RawUrl.EndsWith(RemovingUrl, StringComparison.CurrentCultureIgnoreCase))
            {

                var sessionCookie = context.Request.Cookies[SessionCookieName];

                var sessionCookieValue2 = sessionCookie?.Value;
                RemoveHttpCookie(SessionCookieName);

                if (sessionCookie == null || sessionCookieValue2 == null)
                {
                    GenerateContextResponse(DefaultuUrlAppSetting);
                }

                var decodedSessionCookieValue = HttpUtility.UrlDecode(sessionCookieValue2);
                var decryptedSessionValue = SecurityEncryption.Decrypt(decodedSessionCookieValue);

                context.Application.Lock();
                context.Application.Remove(decryptedSessionValue);
                context.Application.UnLock();

                RemoveHttpCookie(AspNetSessionId);

                GenerateContextResponse(LogOutUrlAppSetting);
            }


            if (IsRequestPageInAllowedPagesList())
            {
                return;
            }

            //No cookie Response
            var sessionCookie1 = HttpContext.Current.Request.Cookies[SessionCookieName];
            if (sessionCookie1?.Value == null)
            {
                GenerateContextResponse(DefaultuUrlAppSetting);
            }

            ///Check for session and application
            if (sessionCookie1 != null)
            {
                string decryptedCookieValue = null;
                string urlDecodeCookieValue = null;
                
                try
                {
                    urlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie1.Value);
                    decryptedCookieValue = SecurityEncryption.Decrypt(urlDecodeCookieValue);
                }
                catch
                {
                    GenerateContextResponse(DefaultuUrlAppSetting);
                }

                if (HttpContext.Current.Request.ApplicationPath != null && HttpContext.Current.Request.ApplicationPath.Equals("/"))
                {

                    /////Checking for a single session
                    if (decryptedCookieValue == null || HttpContext.Current.Application[decryptedCookieValue] == null ||
                        HttpContext.Current.Application[decryptedCookieValue].ToString() != urlDecodeCookieValue)
                    {
                        //RemoveHttpCookie(SessionCookieName);
                        GenerateContextResponse(DefaultuUrlAppSetting);
                    }
                    return;
                }
                else if (HttpContext.Current.Request.ApplicationPath != null && IsRequestApplicationInAllowedApplicationsList())
                {
                    {
                        var returnedCredentials = GetApplicationParentSession(decryptedCookieValue);

                        if (returnedCredentials == null ||
                            returnedCredentials != $"{urlDecodeCookieValue}{decryptedCookieValue}")
                        {
                            //RemoveHttpCookie(SessionCookieName);
                            GenerateContextResponse(DefaultuUrlAppSetting);
                        }
                        return;
                    }
                }
                GenerateContextResponse(DefaultuUrlAppSetting);
            }
        }

        //private bool ValidateTickets(string sesssion, string application)
        //{
        //    if (string.IsNullOrEmpty(sesssion) || string.IsNullOrEmpty(application))
        //    {
        //        return false;
        //    }

        //    return sesssion == application;
        //}

        private static bool IsRequestPageInAllowedPagesList()
        {
            var context = HttpContext.Current;
            var allowedPages = WebConfigurationManager.AppSettings["AllowedPages"];

            if (allowedPages == null)
            {
                return false;
            }

            var allowedPagesList = allowedPages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var currentpage = context.Request.AppRelativeCurrentExecutionFilePath?.Replace("~/", "");

            return currentpage!= null && allowedPagesList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
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

        //private static bool HasCookieValue(HttpContext context, string cookieName, bool checkEncryptedValue = false)
        //{
        //    var sessionCookie = context.Request.Cookies[cookieName];

        //    var isNull = sessionCookie?.Value == null;
        //    if (isNull)
        //    {
        //        return false;
        //    }

        //    if (!checkEncryptedValue)
        //    {
        //        return true;
        //    }

        //    try
        //    {
        //        var sessionCookieValue = HttpUtility.UrlDecode(sessionCookie.Value);
        //        var decryptedCookieValue = SecurityEncryption.Decrypt(sessionCookieValue);

        //        return decryptedCookieValue != null;
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //}

        //private string GetCookieValue(HttpContext context, string cookieName, bool isEncrypted)
        //{
        //    try
        //    {
        //        var sessionCookie = context.Request.Cookies[cookieName];
        //        var hasCookieValue = HasCookieValue(context, cookieName, isEncrypted);

        //        if (!hasCookieValue)
        //        {
        //            return null;
        //        }

        //        var sessionCookieUrlDecoded = HttpUtility.UrlDecode(sessionCookie?.Value);
        //        if (!isEncrypted)
        //        {
        //            return sessionCookieUrlDecoded;
        //        }

        //        var decryptedValue = SecurityEncryption.Decrypt(sessionCookieUrlDecoded);
        //        return decryptedValue;
        //    }
        //    catch
        //    {
        //        return null;
        //    }


        //}

        //private string GetSessionTicket(HttpContext context)
        //{
        //    var decryptedCookieValue = GetCookieValue(context, SessionCookieName, true);
        //    var encryptedCookieValue = GetCookieValue(context, SessionCookieName, false);

        //    return encryptedCookieValue != null && decryptedCookieValue != null
        //        ? $"{encryptedCookieValue}{decryptedCookieValue}" :
        //    null;
        //}

        //private string GetApplicationTicket(HttpContext context)
        //{
        //    var decryptedCookieValue = GetCookieValue(context, SessionCookieName, true);
        //    var encryptedCookieValue = GetCookieValue(context, SessionCookieName, false);


        //    if (context.Request.ApplicationPath != null && context.Request.ApplicationPath.Equals("/"))
        //    {

        //        var applicationCookieValue = context.Application[decryptedCookieValue];

        //        return applicationCookieValue != null ? $"{applicationCookieValue}{decryptedCookieValue}" : null;
        //    }

        //    if (IsAllowedApplication())
        //    {

        //        var applicationCookieValue = GetApplicationParentSession(decryptedCookieValue);

        //        return !string.IsNullOrEmpty(applicationCookieValue) && !applicationCookieValue.Contains("False")
        //            ? applicationCookieValue
        //            : null;
        //    }

        //    return null;

        //}

        private string GetApplicationParentSession(string decrypted)
        {
            var appRoot = WebConfigurationManager.AppSettings["InternalAppRoot"];
            try
            {
                //var httpClient = new HttpClient();
                //var content = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair<string, string>("code", encryptedValue)
                //});

                //return httpClient.PostAsync(appRoot + "/check.ashx", content).Result.Content.ReadAsStringAsync().Result;
                ;
                var request = (HttpWebRequest)WebRequest.Create(appRoot + CheckingUrl);
                var postData = $"{DecryptedValueForm}={decrypted}"; // "decryptedValue=" + decrypted;
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
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private void GenerateContextResponse(string configurationSetting)
        {
            var returnUrl = WebConfigurationManager.AppSettings[configurationSetting];
            if (string.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Current.Response.End();
            }

            HttpContext.Current.Response.Redirect(returnUrl);
        }

        private void RemoveHttpCookie(string cookieName)
        {
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        private static void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {
        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {
           
        }

        /*
        private Tuple<string, string> GetCookie2(HttpContext context)
        {
            var cookie = context.Request.Cookies[SessionCookieName];
            if (cookie?.Value == null)
            {
                return null;
            }

            try
            {
                var urlDecodeCookieValue = HttpUtility.UrlDecode(cookie.Value);
                var decryptedCookieValue = SecurityEncryption.Decrypt(urlDecodeCookieValue);

                return decryptedCookieValue != null ? new Tuple<string, string>(urlDecodeCookieValue, decryptedCookieValue) : null;
            }
            catch
            {
                return null;
            }

        }

        private Tuple<string, string> GetApplicationSession2(HttpContext context, string aplicationId)
        {
            var applicationIdValue = context.Application[aplicationId];

            if (applicationIdValue == null)
            {
                return null;
            }

            return new Tuple<string, string>(aplicationId, applicationIdValue.ToString());
        }


    */
    }
}
