using System;

namespace ICEProject
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //var context = ((HttpApplication)sender).Context;

            //var cookie = context.Request.Cookies["TempSession"];


            //string cookie1 = string.Empty;
            //string cookie2 = string.Empty;

            //if (cookie != null && cookie.Value != null && cookie.Value.Length > 5)
            //{
            //    cookie1 = cookie.Value.Substring(0, cookie.Value.Length - 5);
            //}

            //var sessionCookie = context.Request.Cookies["Session"];
            //if (sessionCookie != null && sessionCookie.Value != null && sessionCookie.Value.Length > 4)
            //{
            //    cookie2 = sessionCookie.Value.Substring(0, sessionCookie.Value.Length - 4);
            //}



            ////var ttt = (HttpApplication)sender;

            ////va//r aaa = context.Request.Form["Sergi"];

            //if (cookie1 == string.Empty || cookie2 == string.Empty || cookie1 != cookie2)
            //{
            //    Response.Redirect("~/login.asp");
            //}


        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            //var context = ((HttpApplication)sender).Context;
            //var tempSession = context.Request.Cookies["TempSession"];
            //if (tempSession != null)
            //{
            //    tempSession.Expires = DateTime.Now.AddDays(-1);
            //    Response.Cookies.Add(tempSession);
            //}
        }

        //protected new void PreSendRequestHeaders()
        //{

        //    }
        //}

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
        private void Encrypt()
        {

        }

    }



}