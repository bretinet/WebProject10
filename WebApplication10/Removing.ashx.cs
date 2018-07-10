using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication10
{
    /// <summary>
    /// Summary description for Removing
    /// </summary>
    public class Removing : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var cont = context.Request.QueryString;
            //context.Application.Lock();
            context.Application.Remove(cont.ToString());
           // context.Application.UnLock();
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