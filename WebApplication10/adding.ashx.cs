using System;
using System.Web;
using System.Web.SessionState;
using SolutionSecurity;

namespace ICEProject
{
    /// <summary>
    /// Summary description for adding
    /// </summary>
    public class adding : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {

            const string cookieTest = "Cookietemp";

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
            context.Response.Write("False");
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}