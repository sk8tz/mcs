// Mainsoft.Web.Administration - Site administration utility
// Authors:
//  Klain Yoni <yonik@mainsoft.com>
//
// Mainsoft.Web.Administration - Site administration utility
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
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


using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Mainsoft.Web.Administration
{
	public partial class ManageUser : System.Web.UI.Page
	{
		public string User_name
		{
			get { return ViewState["User"] == null ? String.Empty : (string) ViewState["User"]; }
			set { ViewState["User"] = value; }
		}

		protected void Page_Load (object sender, EventArgs e)
		{
			
		}

		protected override void OnPreRender (EventArgs e)
		{
			if (IsPostBack) {
				Roles_gv.DataBind ();
				if (mv.ActiveViewIndex == 1) {
					((Button) Master.FindControl ("Back")).Visible = false;
				}
				else {
					((Button) Master.FindControl ("Back")).Visible = true;
				}
			}
			base.OnPreRender (e);
		}

		public void CheckBox_CheckedChanged (object sender, EventArgs e)
		{
			string user_name = ((GridCheckBox) sender).User;
			MembershipUser user = Membership.GetUser (user_name);
			if (((GridCheckBox) sender).Checked) {
				user.IsApproved = true;
				Membership.UpdateUser (user);
			}
			else {
				user.IsApproved = false;
				Membership.UpdateUser (user);
			}
		}

		protected void gridbtn_click (object sender, EventArgs e)
		{
			srch.User = ((GridButton) sender).User;
		}

		protected void Roles_Changed (object sender, EventArgs e)
		{
			String user_name = (string) ViewState["User_name"];
			if (((CheckBox) sender).Checked) {
				Roles.AddUserToRole (user_name, ((CheckBox) sender).Text);
			}
			else {
				Roles.RemoveUserFromRole (user_name, ((CheckBox) sender).Text);
			}
		}
		
		protected void Click_No (object sender, EventArgs e)
		{
			mv.ActiveViewIndex = 0;
		}

		protected void Click_Yes (object sender, EventArgs e)
		{
			RolesDS.DeleteUser (User_name);
			//Roles_gv.DataBind ();
			mv.ActiveViewIndex = 0;
		}

		protected void Delete_Click (object sender, EventArgs e)
		{
			User_name = ((GridButton) sender).User;
			mv.ActiveViewIndex = 1;
		}
	}
}
