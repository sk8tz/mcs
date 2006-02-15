<%@ Page Language="c#" AutoEventWireup="false" Codebehind="ListItem_Selected.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_WebControls.ListItem_Selected" %>
<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>ListItem_Selected</title>
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
			<asp:CheckBoxList id="CheckBoxList1" runat="server">
				<asp:ListItem Value="Item1" Selected="True">Item1</asp:ListItem>
				<asp:ListItem Value="Item2">Item2</asp:ListItem>
			</asp:CheckBoxList>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="64px" Width="88px" Description="selected - set">
					<asp:CheckBoxList id="CheckBoxList2" runat="server">
						<asp:ListItem Value="Item1">Item1</asp:ListItem>
						<asp:ListItem Value="Item2">Item2</asp:ListItem>
					</asp:CheckBoxList>
				</cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
