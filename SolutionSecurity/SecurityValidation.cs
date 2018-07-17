using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace SolutionSecurity
{
    public class SecurityValidation : IHttpModule, IRequiresSessionState
    {
        private const string SecurityCookieName = "TempSession";
        private const string SessionCookieName = "SessionCookie";


        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += Context_AuthenticateRequest;
            context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
            context.BeginRequest += Context_BeginRequest;
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (WebConfigurationManager.AppSettings["SecurityActivation"] != "True")
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

            if (IsAllowedPage(context))
            {
                return;
            }

            //var ddd = GetCookie2(context);
            //var fff = GetApplicationSession2(context, ddd.Item1);
            

            if (!HasCookieValue(context, SessionCookieName, checkEncryptedValue: true))
            {
                context.Response.Redirect(defaultUrl);
            }


            var sessionTicket = GetSessionTicket(context);

            if (sessionTicket != null)
            {
                return;
            }

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

            var allowedPagesList = allowedPages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

            var currentpage = context.Request.AppRelativeCurrentExecutionFilePath?.Replace("~/", "");

            return allowedPagesList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
        }

        private static bool IsAllowedApplication(HttpContext context)
        {
            var allowedApplications = WebConfigurationManager.AppSettings["AllowedApplications"];

            if (allowedApplications == null)
            {
                return false;
            }

            var allowedApplicationList = allowedApplications.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

            var currentpage = context.Request.ApplicationPath?.Replace("/", "");

            return allowedApplicationList.Contains(currentpage, StringComparer.CurrentCultureIgnoreCase);
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

            var ttt = context.Request.FilePath;
            var ggg = ttt.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Select(s=>s.StartsWith("/")?s.Remove(0,1):s).ToList();
            
            //if (ggg.Count == 1)
            //{

            //}



            if (ggg.Count == 1)//context.Request.ApplicationPath != null && context.Request.ApplicationPath.Equals("/"))
            {

                var applicationCookieValue = context.Application[decryptedCookieValue];

                return applicationCookieValue != null ? $"{applicationCookieValue}{decryptedCookieValue}" : null;
            }

            if (ggg.Count >1 ) //&& ggg.Contains("ICE")) // IsAllowedApplication(context))
            {

                var applicationCookieValue = context.Application[decryptedCookieValue];
                if(applicationCookieValue != null)
                {
                    return applicationCookieValue != null ? $"{applicationCookieValue.ToString()}{decryptedCookieValue}" : null;
                }
                


                var applicationCookieValue2 = GetApplicationParentSession(encryptedCookieValue);

                return !string.IsNullOrEmpty(applicationCookieValue2) && !applicationCookieValue2.Contains("False")
                    ? applicationCookieValue2
                    : null;
            }

            return null;

        }

        private string GetApplicationParentSession(string encryptedValue)
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
                var request = (HttpWebRequest)WebRequest.Create(appRoot + "check.ashx");
                var postData = "code=" + encryptedValue;
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded"; // "application/xml; charset=utf-8";// "text/plain"; // application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                if (request.CookieContainer == null)
                {
                    request.CookieContainer = new CookieContainer();
                }
                Cookie c = new Cookie();
                c.Domain = appRoot.Replace("https://", "").Replace("http://", "").Split(':')[0];
                c.Name = "code1";
                c.Value = encryptedValue;

                request.CookieContainer.Add(c);// new Cookie("code", encryptedValue, ));
                                               //request.CookieContainer.Add(new Cookie("codetemp", encryptedValue, "Temp"));

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                return null;
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
