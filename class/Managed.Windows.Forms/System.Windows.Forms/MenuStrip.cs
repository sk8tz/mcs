//
// MenuStrip.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0

using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class MenuStrip : ToolStrip
	{
		private bool can_overflow;
		private bool stretch;

		public MenuStrip () : base ()
		{
			this.GripStyle = ToolStripGripStyle.Hidden;
			this.stretch = true;
		}

		#region Public Properties
		[DefaultValue (false)]
		public bool CanOverflow
		{
			get { return this.can_overflow; }
			set { this.can_overflow = value; }
		}

		[DefaultValue (ToolStripGripStyle.Hidden)]
		public ToolStripGripStyle GripStyle
		{
			get { return base.GripStyle; }
			set { base.GripStyle = value; }
		}

		[DefaultValue (false)]
		public bool ShowItemToolTips
		{
			get { return base.ShowItemToolTips; }
			set { base.ShowItemToolTips = value; }
		}

		[MonoTODO ()]
		[DefaultValue (true)]
		public bool Stretch
		{
			get { return this.stretch; }
			set { this.stretch = value; }
		}
		#endregion

		#region Protected Properties
		protected override Padding DefaultGripMargin { get { return new Padding (2, 2, 0, 2); } }
		protected override Padding DefaultPadding { get { return new Padding (6, 2, 0, 2); } }
		protected override bool DefaultShowItemToolTips { get { return false; } }
		protected override Size DefaultSize { get { return new Size (200, 24); } }
		#endregion

		#region Protected Methods
		protected internal override ToolStripItem CreateDefaultItem (string text, Image image, EventHandler onClick)
		{
			return new ToolStripMenuItem (text, image, onClick);
		}

		protected virtual void OnMenuActivate (EventArgs e)
		{
			if (MenuActivate != null) MenuActivate (this, e);
		}

		protected virtual void OnMenuDeactivate (EventArgs e)
		{
			if (MenuDeactivate != null) MenuDeactivate (this, e);
		}

		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			return base.ProcessCmdKey (ref msg, keyData);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		#endregion

		#region Public Events
		public event EventHandler MenuActivate;
		public event EventHandler MenuDeactivate;
		#endregion
		
		#region Internal Methods
		internal void FireMenuActivate ()
		{
			// The tracker lets us know when the form is clicked or loses focus
			ToolStripMenuTracker.AppClicked += new EventHandler (ToolStripMenuTracker_AppClicked);
			ToolStripMenuTracker.AppFocusChange += new EventHandler (ToolStripMenuTracker_AppFocusChange);
			
			this.OnMenuActivate (EventArgs.Empty);
		}

		internal void FireMenuDeactivate ()
		{
			// Detach from the tracker
			ToolStripMenuTracker.AppClicked -= new EventHandler (ToolStripMenuTracker_AppClicked); ;
			ToolStripMenuTracker.AppFocusChange -= new EventHandler (ToolStripMenuTracker_AppFocusChange);
		
			this.OnMenuDeactivate (EventArgs.Empty);
		}

		private void ToolStripMenuTracker_AppFocusChange (object sender, EventArgs e)
		{
			this.HideMenus (true, ToolStripDropDownCloseReason.AppFocusChange);
		}

		private void ToolStripMenuTracker_AppClicked (object sender, EventArgs e)
		{
			this.HideMenus (true, ToolStripDropDownCloseReason.AppClicked);
		}
		#endregion
	}
}
#endif
