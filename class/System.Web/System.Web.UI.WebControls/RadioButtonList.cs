/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RadioButtonList
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  95%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class RadioButtonList : ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler
	{
		private bool selectionIndexChanged;
		private int  tabIndex;
		
		public RadioButtonList(): base()
		{
			selectionIndexChanged = false;
		}
		
		public virtual int CellPadding
		{
			get
			{
				if(ControlStyleCreated)
					return (int)(((TableStyle)ControlStyle).CellPadding);
			}
			set
			{
				((TableStyle)ControlStyle).CellPadding = value;
			}
		}
		
		public virtual int CellSpacing
		{
			get
			{
				if(ControlStyleCreated)
					return (int)(((TableStyle)ControlStyle).CellSpacing);
			}
			set
			{
				((TableStyle)ControlStyle).CellSpacing = value;
			}
		}
		
		public virtual int RepeatColumns
		{
			get
			{
				object o = ViewState["RepeatColumns"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException("value");
				ViewState["RepeatColumns"] = value;
			}
		}
		
		public virtual RepeatDirection RepeatDirection
		{
			get
			{
				object o = ViewState["RepeatDirection"];
				if(o != null)
					return (RepeatDirection)o;
				return RepeatDirection.Vertical;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatDirection), value))
					throw new ArgumentException();
				ViewState["RepeatDirection"] = value;
			}
		}
		
		public virtual RepeatLayout RepeatLayout
		{
			get
			{
				object o = ViewState["RepeatLayout"];
				if(o != null)
					return (RepeatLayout)o;
				return RepeatLayout.Table;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatLayout), value))
					throw new ArgumentException();
				ViewState["RepeatLayout"] = value;
			}
		}
		
		public virtual TextAlign TextAlign
		{
			get
			{
				object o = ViewState["TextAlign"];
				if(o != null)
					return (TextAlign)o;
				return TextAlign.Right;
			}
			set
			{
				if(!Enum.IsDefined(typeof(TextAlign), value))
					throw new ArgumentException();
				ViewState["TextAlign"] = value;
			}
		}
		
		protected override Style CreateControlStyle()
		{
			return new TableStyle(ViewState);
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			RepeatInfo info = new RepeatInfo();
			Style cStyle = (ControlStyleCreated ? ControlStyle : null);
			bool dirty = false;
			tabIndex = TabIndex;
			if(tabIndex != 0)
			{
				dirty = !ViewState.IsItemDirty("TabIndex");
				TabIndex = 0;
			}
			info.RepeatColumns = RepeatColumns;
			info.RepeatDirection = RepeatDirection;
			info.RenderRepeater(writer, this, cStyle, this);
			if(tabIndex != 0)
			{
				TabIndex = tabIndex;
			}
			if(dirty)
			{
				ViewState.SetItemDirty("TabIndex", false);
			}
		}
		
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string value = postCollection[postDataKey];
			for(int i=0; i < Items.Count; i++)
			{
				if(Items[i].Value == value)
				{
					if(i != SelectedIndex)
					{
						SelectedIndex = i;
					}
					return true;
				}
			}
			return false;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			if(selectionIndexChanged)
				OnSelectedIndexChanged(EventArgs.Empty);
		}
		
		Style IRepeatInfoUser.GetItemStyle(valuetype System.Web.UI.WebControls.ListItemType itemType, int repeatIndex)
		{
			return null;
		}
		
		[MonoTODO("RadioButtonList_RenderItem")]
		void IRepeatInfoUser.RenderItem(valuetype System.Web.UI.WebControls.ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			throw new NotImplementedException();
		}
		
		bool IRepeatInfoUser.HasFooter
		{
			get
			{
				return false;
			}
		}
		
		bool IRepeatInfoUser.HasHeader
		{
			get
			{
				return false;
			}
		}
		
		bool IRepeatInfoUser.HasSeparators
		{
			get
			{
				return false;
			}
		}
		
		int IRepeatInfoUser.RepeatedItemCount
		{
			get
			{
				return Items.Count;
			}
		}
	}
}
