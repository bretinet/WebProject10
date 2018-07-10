using System;
using System.Collections.Generic;
using System.Linq;
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
            context.Application.Lock();
            context.Application[cont.ToString()] = cont;
            context.Application.UnLock();
            context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");
            context.Response.Redirect("default.asp");
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