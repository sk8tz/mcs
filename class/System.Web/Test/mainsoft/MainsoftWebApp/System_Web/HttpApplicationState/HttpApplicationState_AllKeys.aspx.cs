//
// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Vladimir Krasnov <vladimirk@mainsoft.com>
//
//
// Copyright (c) 2002-2005 Mainsoft Corporation.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web
{
	public class HttpApplicationState_AllKeys
		: GHTBaseWeb 
	{
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e) 
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() 
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		private void Page_Load(object sender, EventArgs e)
		{
			HtmlForm form1 = (HtmlForm) (HtmlForm)this.FindControl("Form1");
			this.GHTTestBegin(form1);
			this.GHTSubTestBegin("GHTSubTest1");
			try
			{
				this.Application.Clear();
				this.Application.Add("var1", "variable1");
				this.Application.Add("var2", "variable2");
				this.Application.Add("var3", "variable3");
				string[] textArray1 = new string[this.Application.Count + 1];
				string[] textArray4 = this.Application.AllKeys;
				for (int num2 = 0; num2 < textArray4.Length; num2++)
				{
					string text1 = textArray4[num2];
					this.GHTSubTestAddResult((string)("(\"" + text1 + "\") = " + this.Application[text1]));
				}
			}
			catch (Exception exception3)
			{
				// ProjectData.SetProjectError(exception3);
				Exception exception1 = exception3;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("GHTSubTest2");
			try
			{
				this.Application.Add("var4", "");
				this.Application.Add("", "variable5");
				string[] textArray2 = new string[3];
				string[] textArray3 = this.Application.AllKeys;
				for (int num1 = 0; num1 < textArray3.Length; num1++)
				{
					string text2 = textArray3[num1];
					this.GHTSubTestAddResult((string)("(\"" + text2 + "\") = " + this.Application[text2]));
				}
			}
			catch (Exception exception4)
			{
				// ProjectData.SetProjectError(exception4);
				Exception exception2 = exception4;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTTestEnd();
		}
	}
}
