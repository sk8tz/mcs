
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
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridLinkButton
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish_mono@lycos.com
 * Contact: <gvaish_mono@lycos.com>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	class DataGridLinkButton : LinkButton
	{
		public DataGridLinkButton() : base()
		{
		}

		protected override void Render(HtmlTextWriter writer)
		{
			SetForeColor();
			base.Render(writer);
		}

		private void SetForeColor()
		{
			if(!ControlStyle.IsSet(System.Web.UI.WebControls.Style.FORECOLOR))
			{
				Control ctrl = this;
				int level = 0;
				while(level < 3)
				{
					ctrl = ctrl.Parent;
					Color foreColor = ((WebControl)ctrl).ForeColor;
					if(foreColor != Color.Empty)
					{
						ForeColor = foreColor;
						return;
					}
					level++;
				}
			}
		}
	}
}
