using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;

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




            context.Application.Lock();
            context.Application[cont.ToString()] = cont;
            context.Application.UnLock();
            context.Response.ContentType = "text/plain";
            context.Response.Write(cont + "RESPONSE");
            //context.Response.Redirect("default.asp");
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