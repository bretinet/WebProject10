using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace SolutionSecurity
{
    public class SecurityValidation : IHttpModule
    {
        //private const string SecurityCookieName = "TempSession";
        private const string SessionCookieName = "SessionCookie";
        private const string AspNetSessionId = "ASP.NET_SessionId";
        private const string LogOutUrlAppSettings = "LogOutUrl";
        private const string CheckingUrl = "Checking.aspx";
        private const string TrueValue = "True";


        public void Dispose()
        {
            //throw new NotImplementedException();
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
            const string cookieTempName = "CookieTemp";
            var cookieTemp = HttpContext.Current.Response.Cookies[cookieTempName];
            if (cookieTemp == null) return;
            cookieTemp.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(cookieTemp);
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (!WebConfigurationManager.AppSettings["SecurityActivation"]
                .Equals(TrueValue, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            /*
            var context = ((HttpApplication)sender).Context;

            if (context.Request.CurrentExecutionFilePath == "/adding.ashx")
            {
                const string cookieTest = "cookietest";

                var cookie = context.Request.Cookies[cookieTest];

                if (cookie != null)
                {
                    var value = cookie.Value;
                    var encrypedValue = SecurityEncryption.Encrypt(value);

                    cookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(cookie);

                    context.Application.Lock();
                    context.Application[value] = encrypedValue;
                    context.Application.UnLock();

                    context.Response.Write(encrypedValue);
                    context.Response.End();
                }
            }

            if (context.Request.CurrentExecutionFilePath == "/removing.ashx")
            {
                const string cookieTest = "cookietest";

                var cookie = context.Request.Cookies[cookieTest];

                if (cookie != null)
                {
                    var value = cookie.Value;


                    cookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(cookie);

                    var sessionCookie = context.Request.Cookies[SessionCookieName];

                    if (sessionCookie != null)
                    {
                        var sessionCookieValue = HttpUtility.UrlDecode(sessionCookie.Value);
                        var applicationCookie = context.Application[value];
                        if (applicationCookie.ToString() == sessionCookieValue)
                        {
                            context.Application.Lock();
                            context.Application.Remove(value);
                            context.Application.UnLock();
                        }

                        sessionCookie.Expires = DateTime.Now.AddDays(-1);
                        context.Response.Cookies.Add(sessionCookie);

                        var dd = context.Request.Cookies["ASP.NET_SessionID"];
                        if (dd != null)
                        {
                            dd.Expires = DateTime.Now.AddDays(-1);
                            context.Response.Cookies.Add(dd);
                        }
                    }


                    context.Response.Redirect("default.asp");

                    //context.Response.Write(encrypedValue);
                    //context.Response.End();
                }
            }*/

            //try
            //{

            var defaultUrl = WebConfigurationManager.AppSettings["DefaultUrl"];

            var context = ((HttpApplication)sender).Context;

            //if (IsAllowedPage(context))
            //{
            //    return;
            //}

            if (context.Request.RawUrl.EndsWith("Adding.ashx", StringComparison.CurrentCultureIgnoreCase))
            {
                const string cookieTest = "CookieTemp";

                var cookie = context.Request.Cookies[cookieTest];

                if (cookie != null)
                {
                    var value = cookie.Value;
                    var encrypedValue = SecurityEncryption.Encrypt(value);

                    cookie.Expires = DateTime.Now.AddDays(-1);
                    context.Response.Cookies.Add(cookie);

                    context.Application.Lock();
                    context.Application[value] = encrypedValue;
                    context.Application.UnLock();

                    context.Response.Write(encrypedValue);
                    context.Response.End();
                }
            }

            if (context.Request.RawUrl.EndsWith(CheckingUrl, StringComparison.CurrentCultureIgnoreCase))
            {
                //context.Response.ContentType = "text/plain";
                //context.Response.Write("Hello World");

                //var sessionEncryptedCookieValue = context.Request.Form["encryptedValue"];
                var sessionDecriptedCookieValue = context.Request.Form["decryptedValue"];
                context.Response.ContentType = "text/plain";
                //context.Response.Write("This is plain text");
                //if (sessionCookieValue == null)
                //{
                //    context.Response.Write("False");
                //    context.Response.End();
                //}

                //string decryptedCookieValue = null;
                //try
                //{
                ////var sss1 = HttpUtility.UrlDecode(sessionCookieValue1);
                //decryptedCookieValue = SecurityEncryption.Decrypt(sessionEncryptedCookieValue);

                //var sss2 = HttpUtility.UrlDecode(sessionEncryptedCookieValue);
                //decryptedCookieValue = SecurityEncryption.Decrypt(sss2);



                var application = HttpContext.Current.Application[sessionDecriptedCookieValue];
                if (application != null)
                {
                    context.Response.Write($"{application}{sessionDecriptedCookieValue}");
                }

                context.Response.End();
                //}
                //catch
                //{
                //    context.Response.Write("False");
                //    context.Response.End();
                //}
            }


            if (context.Request.RawUrl.EndsWith("Removing.ashx", StringComparison.CurrentCultureIgnoreCase))
            {

                var sessionCookie = context.Request.Cookies[SessionCookieName];

                var sessionCookieValue2 = sessionCookie?.Value;
                RemoveHttpCookie(SessionCookieName);

                if (sessionCookie == null || sessionCookieValue2 == null)
                {
                    GenerateContextResponse();
                }

                var ggg = HttpUtility.UrlDecode(sessionCookieValue2);
                var decryptedSessionValue = SecurityEncryption.Decrypt(ggg);

                context.Application.Lock();
                context.Application.Remove(decryptedSessionValue);
                context.Application.UnLock();

                RemoveHttpCookie(AspNetSessionId);

                GenerateContextResponse();
            }


            if (IsAllowedPage(context))
            {
                return;
            }

            //No cookie Response
            var sessionCookie1 = HttpContext.Current.Request.Cookies[SessionCookieName];
            if (sessionCookie1?.Value == null)
            {
                GenerateContextResponse();
            }

            ///Check for session and application
            if (sessionCookie1 != null)
            {
                string decryptedCookieValue = null;
                var urlDecodeCookieValue = HttpUtility.UrlDecode(sessionCookie1.Value);
                try
                {
                    decryptedCookieValue = SecurityEncryption.Decrypt(urlDecodeCookieValue);
                }
                catch
                {
                    GenerateContextResponse();
                }

                if (HttpContext.Current.Request.ApplicationPath != null && HttpContext.Current.Request.ApplicationPath.Equals("/"))
                {

                    /////Checking for a single session
                    if (decryptedCookieValue == null || HttpContext.Current.Application[decryptedCookieValue] == null ||
                        HttpContext.Current.Application[decryptedCookieValue].ToString() != urlDecodeCookieValue)
                    {
                        //RemoveHttpCookie(SessionCookieName);
                        GenerateContextResponse();
                    }
                    return;
                }
                else if (HttpContext.Current.Request.ApplicationPath != null && IsAllowedApplication())
                {
                    ////Testing plugins
                    // if (HttpContext.Current.Request.ApplicationPath != null && !HttpContext.Current.Request.ApplicationPath.Equals("/"))
                    {
                        var returnedCredentials = GetApplicationParentSession(decryptedCookieValue);

                        if (returnedCredentials == null ||
                            returnedCredentials != $"{urlDecodeCookieValue}{decryptedCookieValue}")
                        {
                            //RemoveHttpCookie(SessionCookieName);
                            GenerateContextResponse();
                        }
                        return;
                    }
                }
                GenerateContextResponse();
            }


            ////

            //if (decryptedCookieValue == null)
            //{
            //    context.Response.Write("False");
            //    context.Response.End();
            //}
            //else
            //{
            //    var applicationCookieValue = context.Application[decryptedCookieValue];
            //    if (applicationCookieValue == null)
            //    {
            //        context.Response.Write("False");
            //        context.Response.End();
            //    }
            //    context.Response.Write($"{applicationCookieValue}{decryptedCookieValue}");
            //}
            //context.Response.End();




            //var ddd = GetCookie2(context);
            //var fff = GetApplicationSession2(context, ddd.Item1);

            //if (!HasCookieValue(context, SessionCookieName, checkEncryptedValue: true))
            //{
            //    context.Response.Redirect(defaultUrl);
            //}


            //var sessionTicket = GetSessionTicket(context);
            //var applicationTicket = GetApplicationTicket(context);

            //if (!ValidateTickets(sessionTicket, applicationTicket))
            //{
            //    context.Response.Redirect(defaultUrl);
            //}
        }

        private bool ValidateTickets(string sesssion, string application)
        {
            if (string.IsNullOrEmpty(sesssion) || string.IsNullOrEmpty(application))
            {
                return false;
            }

            return sesssion == application;
        }

        private static bool IsAllowedPage(HttpContext context)
        {
            var allowedPages = WebConfigurationManager.AppSettings["AllowedPages"];

            if (allowedPages == null)
            {
                return false;
            }

            var allowedPagesList = allowedPages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var currentpage = context.Request.AppRelativeCurrentExecutionFilePath?.Replace("~/", "");

            return currentpage!= null && allowedPagesList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
        }

        private static bool IsAllowedApplication()
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

        private static bool HasCookieValue(HttpContext context, string cookieName, bool checkEncryptedValue = false)
        {
            var sessionCookie = context.Request.Cookies[cookieName];

            var isNull = sessionCookie?.Value == null;
            if (isNull)
            {
                return false;
            }

            if (!checkEncryptedValue)
            {
                return true;
            }

            try
            {
                var sessionCookieValue = HttpUtility.UrlDecode(sessionCookie.Value);
                var decryptedCookieValue = SecurityEncryption.Decrypt(sessionCookieValue);

                return decryptedCookieValue != null;
            }
            catch
            {
                return false;
            }

        }

        private string GetCookieValue(HttpContext context, string cookieName, bool isEncrypted)
        {
            try
            {
                var sessionCookie = context.Request.Cookies[cookieName];
                var hasCookieValue = HasCookieValue(context, cookieName, isEncrypted);

                if (!hasCookieValue)
                {
                    return null;
                }

                var sessionCookieUrlDecoded = HttpUtility.UrlDecode(sessionCookie?.Value);
                if (!isEncrypted)
                {
                    return sessionCookieUrlDecoded;
                }

                var decryptedValue = SecurityEncryption.Decrypt(sessionCookieUrlDecoded);
                return decryptedValue;
            }
            catch
            {
                return null;
            }


        }

        private string GetSessionTicket(HttpContext context)
        {
            var decryptedCookieValue = GetCookieValue(context, SessionCookieName, true);
            var encryptedCookieValue = GetCookieValue(context, SessionCookieName, false);

            return encryptedCookieValue != null && decryptedCookieValue != null
                ? $"{encryptedCookieValue}{decryptedCookieValue}" :
            null;
        }

        private string GetApplicationTicket(HttpContext context)
        {
            var decryptedCookieValue = GetCookieValue(context, SessionCookieName, true);
            var encryptedCookieValue = GetCookieValue(context, SessionCookieName, false);


            if (context.Request.ApplicationPath != null && context.Request.ApplicationPath.Equals("/"))
            {

                var applicationCookieValue = context.Application[decryptedCookieValue];

                return applicationCookieValue != null ? $"{applicationCookieValue}{decryptedCookieValue}" : null;
            }

            if (IsAllowedApplication())
            {

                var applicationCookieValue = GetApplicationParentSession(decryptedCookieValue);

                return !string.IsNullOrEmpty(applicationCookieValue) && !applicationCookieValue.Contains("False")
                    ? applicationCookieValue
                    : null;
            }

            return null;

        }

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
                var postData = "decryptedValue=" + decrypted;
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

        private void GenerateContextResponse()
        {
            var logOutUrl = WebConfigurationManager.AppSettings[LogOutUrlAppSettings];
            if (string.IsNullOrEmpty(logOutUrl))
            {
                HttpContext.Current.Response.End();
            }

            HttpContext.Current.Response.Redirect(logOutUrl);
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
            //var context = ((HttpApplication)sender).Context;
            //var securityCookie = context.Request.Cookies[SecurityCookieName];
            //if (securityCookie != null)
            //{
            //    securityCookie.Expires = DateTime.Now.AddDays(-1);
            //    context.Response.Cookies.Add(securityCookie);
            //}


        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {
            /*
            try
            {


                string sessionCookieValue = null;
                string SecurityCookieValue = string.Empty;

                var context = ((HttpApplication) sender).Context;
                if (context.Request.CurrentExecutionFilePath == "/ApplicationControl.aspx" ||
                    context.Request.CurrentExecutionFilePath == "/adding.ashx")
                {
                    return;
                }

                var sessionCookie = context.Request.Cookies[SessionCookieName];

                if (sessionCookie?.Value == null)
                {
                    context.Response.Redirect("login.asp",false);
                    context.Response.End();
                }

                //if (sessionCookie?.Value != null)
                //{
                sessionCookieValue = HttpUtility.UrlDecode(sessionCookie?.Value);
                var decryptedCookieValue = SecurityEncryption.Decrypt(sessionCookieValue);

                var applicationCookie = context.Application[decryptedCookieValue];

                if (applicationCookie == null || applicationCookie.ToString() != sessionCookieValue)
                {
                    context.Response.Redirect("login2.asp");
                }
            }
            catch (Exception ex)
            {
                var context = ((HttpApplication)sender).Context;
                context.Response.Redirect("login3.asp");
            }*/


            //var securityCookie = context.Request.Cookies[SecurityCookieName];
            //if (securityCookie != null && securityCookie.Value != null && securityCookie.Value.Length > 5)
            //{
            //    SecurityCookieValue = securityCookie.Value.Substring(0, securityCookie.Value.Length - 5);
            //}

            //if (sessionCookieValue == null || SecurityCookieValue == null|| sessionCookieValue != SecurityCookieValue)
            //{
            //    context.Response.Redirect("~/login.asp");
            //}
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
