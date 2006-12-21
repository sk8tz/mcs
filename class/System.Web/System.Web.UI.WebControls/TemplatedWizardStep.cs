//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Web.UI.WebControls
{
	[ThemeableAttribute (true)]
	[BindableAttribute (false)]
	[PersistChildren (false)]
	[ParseChildren (true)]
	public class TemplatedWizardStep : WizardStepBase
	{
		private ITemplate _contentTemplate = null;
		private Control _contentTemplateContainer = null;
		private ITemplate _customNavigationTemplate = null;
		private Control _customNavigationTemplateContainer = null;

		[TemplateContainerAttribute (typeof (System.Web.UI.WebControls.Wizard))]
		public virtual ITemplate ContentTemplate
		{
			get { return _contentTemplate; }
			set { _contentTemplate = value; }
		}

		public Control ContentTemplateContainer
		{
			get { return _contentTemplateContainer; }
			internal set { _contentTemplateContainer = value; }
		}

		[TemplateContainerAttribute (typeof (System.Web.UI.WebControls.Wizard))]
		public virtual ITemplate CustomNavigationTemplate
		{
			get { return _customNavigationTemplate; }
			set { _customNavigationTemplate = value; }
		}

		[BindableAttribute (false)]
		public Control CustomNavigationTemplateContainer
		{
			get { return _customNavigationTemplateContainer; }
			internal set { _customNavigationTemplateContainer = value; }
		}

		[MonoTODO("Why override?")]
		public override string SkinID
		{
			get { return base.SkinID; }
			set { base.SkinID = value; }
		}
	}

	internal class BaseWizardContainer : Table, INamingContainer
	{
		internal BaseWizardContainer ()
		{
			SetBindingContainer (false);
			InitTable ();
		}

		internal void InstatiateTemplate (ITemplate template)
		{
			TableCell defaultCell = this.Rows [0].Cells [0];
			template.InstantiateIn (defaultCell);
		}

		private void InitTable ()
		{
			TableRow row = new TableRow ();
			TableCell cell = new TableCell ();

			cell.ControlStyle.Width = Unit.Percentage (100);
			cell.ControlStyle.Height = Unit.Percentage (100);

			row.Cells.Add (cell);

			this.ControlStyle.Width = Unit.Percentage (100);
			this.ControlStyle.Height = Unit.Percentage (100);
			this.CellPadding = 0;
			this.CellSpacing = 0;

			this.Rows.Add (row);
		}
	}

	internal class BaseWizardNavigationContainer : Control, INamingContainer
	{
		internal BaseWizardNavigationContainer ()
		{
			SetBindingContainer (false);
		}
	}
}

#endif
