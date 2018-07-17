using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SolutionSecurity;

namespace ICEProject
{
    /// <summary>
    /// Summary description for Check
    /// </summary>
    public class Check : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            //context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");

            var sessionCookieValue = context.Request.Form["code"];
            context.Response.ContentType = "text/plain";

            var session2 = context.Request.Cookies["code1"];

            //context.Response.Write("This is plain text");
            if (sessionCookieValue == null)
            {
                context.Response.Write("False");
                context.Response.End();
            }

            string decryptedCookieValue = null;
            try
            {               
                
                decryptedCookieValue = SecurityEncryption.Decrypt(session2.Value);
            }
            catch
            {
                //context.Response.Write("False");
                //context.Response.End();
            }
            
            if (decryptedCookieValue == null)
            {
                try
                {
var v = HttpUtility.UrlDecode(session2.Value);
                decryptedCookieValue = SecurityEncryption.Decrypt(v);
                }
                catch { }
                
            }
            

            if (decryptedCookieValue == null)
            {
                context.Response.Write("False");
                context.Response.End();
            }
            else
            {
                var applicationCookieValue = context.Application[decryptedCookieValue];
                if (applicationCookieValue == null)
                {
                    context.Response.Write("False");
                    context.Response.End();
                }
                context.Response.Write($"{applicationCookieValue}{decryptedCookieValue}");
            }
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