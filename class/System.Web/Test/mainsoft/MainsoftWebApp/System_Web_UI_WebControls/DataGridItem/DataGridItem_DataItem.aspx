<%@ Page Language="c#" AutoEventWireup="false" Codebehind="DataGridItem_DataItem.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.DataGridItem_DataItem" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DataGridItem_DataItem</title>
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
				<asp:DataGrid id="DataGrid1" runat="server"></asp:DataGrid>
			</cc1:GHTSubTest>
			<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="120px" Width="553px">
				<asp:DataGrid id="DataGrid2" runat="server"></asp:DataGrid>
			</cc1:GHTSubTest>
		</form>
	</body>
</HTML>
