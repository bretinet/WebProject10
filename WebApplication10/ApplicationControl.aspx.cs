using System;

namespace ICEProject
{
    public partial class ApplicationControl : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            foreach(var v in Context.Application.AllKeys)
            {
                Response.Write(v + "--" + Context.Application[v]);
            }
        }
    }
}