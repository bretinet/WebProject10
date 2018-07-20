using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace ICEProject
{
    public partial class ApplicationControl : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            //foreach(var v in Context.Application.AllKeys)
            //{
            //    Response.Write(v + "--" + Context.Application[v]);
            //}

            HtmlTable table1 = new HtmlTable();
            // Set the table's formatting-related properties.
            table1.Border = 1;
            table1.CellPadding = 3;
            table1.CellSpacing = 3;
            table1.BorderColor = "red";

            // Start adding content to the table.
            HtmlTableRow row;
            HtmlTableCell cell;
            for (int i = 0; i < Context.Application.Count; i++)
            {
                // Create a new row and set its background color.
                row = new HtmlTableRow();
                row.BgColor = (i % 2 == 0 ? "lightyellow" : "lightcyan");
                //for (int j = 0; j <= 3; j++)
                {
                    // Create a cell and set its text.
                    cell = new HtmlTableCell();
                    cell.InnerHtml = Context.Application.Keys[i];
                        
                    row.Cells.Add(cell);

                    cell = new HtmlTableCell();
                    cell.InnerHtml = Context.Application.Get(i).ToString();

                    row.Cells.Add(cell);

                    cell = new HtmlTableCell();
                    cell.Controls.Add(new Button());
                    row.Cells.Add(cell);
                }

                // Add the row to the table.
                table1.Rows.Add(row);
            }

            // Add the table to the page.
            this.Controls.Add(table1);
        }
    }
}