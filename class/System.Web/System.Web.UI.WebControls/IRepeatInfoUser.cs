/**
 * Namespace: System.Web.UI.WebControls
 * Interface: IRepeatInfoUser
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public interface IRepeatInfoUser
	{
		bool HasFooter { get; }
		bool HasHeader { get; }
		bool HasSeparators { get; }
		int  RepeatedItemCount { get; }
		void RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer);
		Style GetItemStyle(ListItemType itemType, int repeatIndex);
	}
}
