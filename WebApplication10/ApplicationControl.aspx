<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ApplicationControl.aspx.cs" Inherits="ICEProject.ApplicationControl" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="width:700px; margin-left:auto; margin-right:auto; margin-top:50px">
            <div style="width:100%; height:40px; margin-bottom: 10px; background-color:#5D7B9D; font-weight:bolder; color:white; display:block; vertical-align:middle; text-align:center; font-size:large">Active Sessions</div>
            <asp:GridView ID="GridView1" runat="server" CellPadding="4" ForeColor="#333333" AutoGenerateColumns="False" OnRowCommand="GridView1_RowCommand" EmptyDataText="No active sessions">
                <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                <Columns>
                    <asp:BoundField DataField="Key" HeaderText="Key" >
                    <ItemStyle Width="300px" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Value" HeaderText="Value" >
                    <HeaderStyle Width="300px" />
                    </asp:BoundField>
                    <asp:ButtonField ButtonType="Button" CommandName="DeleteRow" Text="Remove" >
                    <HeaderStyle Width="100px" />
                    <ItemStyle HorizontalAlign="Right" />
                    </asp:ButtonField>
                </Columns>
                <EditRowStyle BackColor="#999999" />
                <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                <SortedAscendingCellStyle BackColor="#E9E7E2" />
                <SortedAscendingHeaderStyle BackColor="#506C8C" />
                <SortedDescendingCellStyle BackColor="#FFFDF8" />
                <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
            </asp:GridView>
        </div>
    </form>
</body>
</html>
