//
// System.Web.UI.WebControls.MenuItemTemplateContainer.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public sealed class MenuItemTemplateContainer: Control, IDataItemContainer, INamingContainer
	{
		object dataItem;
		int index;
		
		public MenuItemTemplateContainer (int itemIndex, MenuItem menuItem)
		{
			index = itemIndex;
			dataItem = menuItem;
		}
		
		protected override bool OnBubbleEvent (object source, EventArgs args)
		{
			CommandEventArgs command = args as CommandEventArgs;
			if (command == null)
				return false;

			MenuEventArgs menuArgs = new MenuEventArgs ((MenuItem) DataItem, source, command);
			RaiseBubbleEvent (this, menuArgs);
			return true;
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			base.Render (writer);
		}
		
		public object DataItem {
			get { return dataItem; }
			set { dataItem = value; }
		}
		
		public int ItemIndex {
			get { return index; }
		}
		
		int IDataItemContainer.DataItemIndex {
			get { return index; }
		}

		int IDataItemContainer.DisplayIndex {
			get { return index; }
		}
	}
}

#endif
