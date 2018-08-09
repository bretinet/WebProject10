using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ICEProject
{
    public partial class ApplicationControl : System.Web.UI.Page
    {
        List<KeyValuePair<string, string>> items;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsCallback)
            {
                //Application["Probe1"] = "Test1";
                //Application["Probe2"] = "Test2";

                items = new List<KeyValuePair<string, string>>();

                foreach (var item in Context.Application.AllKeys)
                {
                    var v = new KeyValuePair<string, string>(item, Context.Application[item].ToString());

                    items.Add(v);
                }

                GridView1.DataSource = items;
                GridView1.DataBind();
            }
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteRow")
            {
                var index = Convert.ToInt32(e.CommandArgument);

                if (index > Application.Count)
                {
                    return;
                }

                var ff = Application[index];

                Application.RemoveAt(index);
                if (items == null)
                {
                    items = new List<KeyValuePair<string, string>>();
                }
                else
                {
                    items.Clear();
                }


                foreach (var item in Context.Application.AllKeys)
                {
                    var v = new KeyValuePair<string, string>(item, Context.Application[item].ToString());

                    items.Add(v);
                }

                GridView1.DataSource = items;
                GridView1.DataBind();
            }
        }
    }


}