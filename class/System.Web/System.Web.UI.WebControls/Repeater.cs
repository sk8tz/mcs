/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Repeater
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  90%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class Repeater : Control, INamingContainer
	{
		private static readonly object ItemCommandEvent   = new object();
		private static readonly object ItemCreatedEvent   = new object();
		private static readonly object ItemDataBoundEvent = new object();
		
		private static readonly string ITEMCOUNT = "Repeater_Item_Count";

		private ITemplate alternatingItemTemplate;
		private ITemplate footerTemplate;
		private ITemplate headerTemplate;
		private ITemplate itemTemplate;
		private ITemplate separatorTemplate;
		private object    dataSource;

		private RepeaterItemCollection items;
		private ArrayList              itemsArrayList;

		public Repeater(): base()
		{
		}
		
		public event RepeaterCommandEventHandler ItemCommand
		{
			add
			{
				Events.AddHandler(ItemCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemCommandEvent, value);
			}
		}
		
		public event RepeaterItemEventHandler ItemCreated
		{
			add
			{
				Events.AddHandler(ItemCreatedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemCreatedEvent, value);
			}
		}
		
		public event RepeaterItemEventHandler ItemDataBound
		{
			add
			{
				Events.AddHandler(ItemDataBoundEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemDataBoundEvent, value);
			}
		}
		
		public virtual ITemplate AlternatingItemTemplate
		{
			get
			{
				return alternatingItemTemplate;
			}
			set
			{
				alternatingItemTemplate = value;
			}
		}
		
		public override ControlCollection Controls
		{
			get
			{
				EnsureChildControls();
				return Controls;
			}
		}
		
		public virtual string DataMember
		{
			get
			{
				object o = ViewState["DataMember"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["DataMember"] = value;
			}
		}
		
		public virtual object DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				dataSource = value;
			}
		}
		
		public virtual ITemplate FooterTemplate
		{
			get
			{
				return footerTemplate;
			}
			set
			{
				footerTemplate = value;
			}
		}
		
		public virtual ITemplate HeaderTemplate
		{
			get
			{
				return headerTemplate;
			}
			set
			{
				headerTemplate = value;
			}
		}
		
		public virtual RepeaterItemCollection Items
		{
			get
			{
				if(items != null)
				{
					if(itemsArrayList != null)
					{
						EnsureChildControls();
					}
					items = new RepeaterItemCollection(itemsArrayList);
				}
				return items;
			}
		}

		public virtual ITemplate ItemTemplate
		{
			get
			{
				return itemTemplate;
			}
			set
			{
				itemTemplate = value;
			}
		}

		public virtual ITemplate SeparatorTemplate
		{
			get
			{
				return separatorTemplate;
			}
			set
			{
				separatorTemplate = value;
			}
		}
		
		public override void DataBind()
		{
			OnDataBinding(EventArgs.Empty);
		}
		
		protected override void CreateChildControls()
		{
			Controls.Clear();
			if(ViewState[ITEMCOUNT] != null)
			{
				CreateControlHierarchy(false);
			} else
			{
				itemsArrayList = new ArrayList();
			}
			ClearChildViewState();
		}
		
		/// <summary>
		/// Undocumented
		/// </summary>
		[MonoTODO]
		protected void CreateControlHierarchy(bool useDataSource)
		{
			//TODO: Fille me up
			throw new NotImplementedException();
		}
		
		protected override bool OnBubbleEvent(object sender, EventArgs e)
		{
			bool retVal = false;
			if(e is RepeaterCommandEventArgs)
			{
				OnItemCommand((RepeaterCommandEventArgs)e);
				retVal = true;
			}
			return retVal;
		}
		
		protected override void OnDataBinding(EventArgs e)
		{
			OnDataBinding(e);
			Controls.Clear();
			ClearChildViewState();
			CreateControlHierarchy(true);
			ChildControlCreated = true;
		}
		
		protected virtual void OnItemCommand(RepeaterCommandEventArgs e)
		{
			if(Events != null)
			{
				RepeaterCommandEventHandler rceh = (RepeaterCommandEventHandler)(ViewState[ItemCommandEvent]);
				if(rceh != null)
				{
					rceh(this, e);
				}
			}
		}
		
		protected virtual void OnItemCreated(RepeaterCommandEventArgs e)
		{
			if(Events != null)
			{
				RepeaterCommandEventHandler rceh = (RepeaterCommandEventHandler)(ViewState[ItemCreatedEvent]);
				if(rceh != null)
				{
					rceh(this, e);
				}
			}
		}
		
		protected virtual void OnItemDataBound(RepeaterCommandEventArgs e)
		{
			if(Events != null)
			{
				RepeaterCommandEventHandler rceh = (RepeaterCommandEventHandler)(ViewState[ItemDataBoundEvent]);
				if(rceh != null)
				{
					rceh(this, e);
				}
			}
		}
	}
}
