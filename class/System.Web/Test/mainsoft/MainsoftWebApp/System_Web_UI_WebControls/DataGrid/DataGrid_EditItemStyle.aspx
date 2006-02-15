<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataGrid_EditItemStyle.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataGrid_EditItemStyle" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataGrid_EditItemStyle</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script LANGUAGE="JavaScript">
        function ScriptTest()
        {
            var theform;
		    if (window.navigator.appName.toLowerCase().indexOf("netscape") > -1) {
			    theform = document.forms["Form1"];
		    }
		    else {
			    theform = document.Form1;
		    }
        }
		</script>
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="120px" Width="553px">
				<asp:DataGrid id="DataGrid1" runat="server">
				<EditItemStyle></EditItemStyle>
				</asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="120px" Width="553px">
				<asp:DataGrid id="DataGrid2" runat="server">
				<EditItemStyle VerticalAlign="Bottom" HorizontalAlign="Center" Wrap="True" BackColor="blue"></EditItemStyle>
				</asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="120px" Width="553px">
				<asp:DataGrid id="DataGrid3" runat="server">
				<EditItemStyle VerticalAlign="Top" HorizontalAlign="RiGhT" Height="50" Wrap="false" BackColor="#ffff00"></EditItemStyle>
				</asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="120px" Width="553px">
				<asp:DataGrid id="DataGrid4" runat="server">
				<EditItemStyle VerticalAlign="Middle" HorizontalAlign="Center" Height="50" Wrap="false" BackColor="#ffff00"></EditItemStyle>
				<Columns>
					<asp:EditCommandColumn EditText="Edit" UpdateText="Update"></asp:EditCommandColumn>
				</Columns>
				</asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest5" runat="server" Height="120px" Width="553px">
				<asp:DataGrid id="DataGrid5" runat="server">
				<Columns>
					<asp:EditCommandColumn EditText="Edit" UpdateText="Update"></asp:EditCommandColumn>
				</Columns>
				</asp:DataGrid>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
