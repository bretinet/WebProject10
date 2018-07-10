using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ValidationProject
{
    public class Class1 : IHttpModule
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            var context = ((HttpApplication) context).Context;


        }
    }
}
