<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataGrid_SortCommand.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataGrid_SortCommand" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataGrid_SortCommand</title>
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
			<cc1:ghtsubtest id="Ghtsubtest1" runat="server" Width="553px" Height="120px">
				<asp:DataGrid id="DataGrid1" runat="server" BorderWidth="1" BackColor="#ffffcc" BorderColor="#000000"
					HeaderStyle-BackColor="#cccccc" FooterStyle-BackColor="#cccccc" PagerStyle-BackColor="#99cc33"></asp:DataGrid>
			</cc1:ghtsubtest>
			<cc1:ghtsubtest id="Ghtsubtest2" runat="server" Width="553px" Height="120px">
				<asp:DataGrid id="DataGrid2" runat="server" BorderWidth="1" BackColor="#ffffcc" BorderColor="#000000"
					HeaderStyle-BackColor="#cccccc" FooterStyle-BackColor="#cccccc" PagerStyle-BackColor="#99cc33"></asp:DataGrid>
			</cc1:ghtsubtest>
		</form>
	</body>
</HTML>
