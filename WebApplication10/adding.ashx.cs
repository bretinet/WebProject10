using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using SolutionSecurity;

namespace WebApplication10
{
    /// <summary>
    /// Summary description for adding
    /// </summary>
    public class adding : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {



            var cont = context.Request.QueryString;


            //var v = new SecurityEncr




            //context.Application.Lock();
            //context.Application[cont.ToString()] = cont;
            //context.Application.UnLock();
            context.Response.ContentType = "text/plain";
            //context.Response.Write(cont + "RESPONSE");
            //context.Response.Redirect("default.asp");



            //var value = cookie.Value;
            var encrypedValue = SecurityEncryption.Encrypt(cont.ToString());

            //cookie.Expires = DateTime.Now.AddDays(-1);
            //context.Response.Cookies.Add(cookie);

            context.Application.Lock();
            context.Application[cont.ToString()] = encrypedValue;
            context.Application.UnLock();

            var cookie = new HttpCookie("SessionCookie", encrypedValue);
            cookie.Expires = DateTime.Now.AddMinutes(30);
            cookie.Secure = true;
            context.Response.Cookies.Add(cookie);



            //context.Response.Write(encrypedValue);
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