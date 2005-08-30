//
// System.Web.UI.WebControls.Menu.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

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
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.Handlers;
using System.Collections.Specialized;
using System.IO;

namespace System.Web.UI.WebControls
{
	[DefaultEvent ("MenuItemClick")]
	[ControlValueProperty ("SelectedValue")]
	[Designer ("System.Web.UI.Design.WebControls.MenuDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class Menu : HierarchicalDataBoundControl, IPostBackEventHandler, INamingContainer
	{
		MenuItemStyle dynamicMenuItemStyle;
		SubMenuStyle dynamicMenuStyle;
		MenuItemStyle dynamicSelectedStyle;
		MenuItemStyle staticMenuItemStyle;
		SubMenuStyle staticMenuStyle;
		MenuItemStyle staticSelectedStyle;
		Style staticHoverStyle;
		Style dynamicHoverStyle;

		MenuItemStyleCollection levelMenuItemStyles;
		MenuItemStyleCollection levelSelectedStyles;
		SubMenuStyleCollection levelSubMenuStyles;
		ITemplate staticItemTemplate;
		ITemplate dynamicItemTemplate;
		
		MenuItemCollection items;
		MenuItemBindingCollection dataBindings;
		MenuItem selectedItem;
		string selectedItemPath;
		Hashtable bindings;
		ArrayList dynamicMenus;
		
		private static readonly object MenuItemClickEvent = new object();
		private static readonly object MenuItemDataBoundEvent = new object();
		
		public static readonly string MenuItemClickCommandName = "Click";
		
		public event MenuEventHandler MenuItemClick {
			add { Events.AddHandler (MenuItemClickEvent, value); }
			remove { Events.RemoveHandler (MenuItemClickEvent, value); }
		}
		
		public event MenuEventHandler MenuItemDataBound {
			add { Events.AddHandler (MenuItemDataBoundEvent, value); }
			remove { Events.RemoveHandler (MenuItemDataBoundEvent, value); }
		}
		
		protected virtual void OnMenuItemClick (MenuEventArgs e)
		{
			if (Events != null) {
				MenuEventHandler eh = (MenuEventHandler) Events [MenuItemClickEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnMenuItemDataBound (MenuEventArgs e)
		{
			if (Events != null) {
				MenuEventHandler eh = (MenuEventHandler) Events [MenuItemDataBoundEvent];
				if (eh != null) eh (this, e);
			}
		}

	    [DefaultValueAttribute (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [EditorAttribute ("System.Web.UI.Design.WebControls.MenuBindingsEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	    [MergablePropertyAttribute (false)]
		public MenuItemBindingCollection DataBindings {
			get {
				if (dataBindings == null) {
					dataBindings = new MenuItemBindingCollection ();
					if (IsTrackingViewState)
						((IStateManager)dataBindings).TrackViewState();
				}
				return dataBindings;
			}
		}

		[DefaultValue (500)]
		[ThemeableAttribute (false)]
		public int DisappearAfter {
			get {
				object o = ViewState ["DisappearAfter"];
				if (o != null) return (int)o;
				return 500;
			}
			set {
				ViewState["DisappearAfter"] = value;
			}
		}

		[ThemeableAttribute (true)]
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DynamicBottomSeparatorImageUrl {
			get {
				object o = ViewState ["dbsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["dbsiu"] = value;
			}
		}

	    [DefaultValueAttribute ("")]
		public string DynamicItemFormatString {
			get {
				object o = ViewState ["DynamicItemFormatString"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["DynamicItemFormatString"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DynamicTopSeparatorImageUrl {
			get {
				object o = ViewState ["dtsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["dtsiu"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string StaticBottomSeparatorImageUrl {
			get {
				object o = ViewState ["sbsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["sbsiu"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string StaticTopSeparatorImageUrl {
			get {
				object o = ViewState ["stsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["stsiu"] = value;
			}
		}

		[DefaultValue (Orientation.Vertical)]
		public Orientation Orientation {
			get {
				object o = ViewState ["Orientation"];
				if (o != null) return (Orientation) o;
				return Orientation.Vertical;
			}
			set {
				ViewState["Orientation"] = value;
			}
		}

		[DefaultValue (1)]
		[ThemeableAttribute (true)]
		public int StaticDisplayLevels {
			get {
				object o = ViewState ["StaticDisplayLevels"];
				if (o != null) return (int)o;
				return 1;
			}
			set {
				if (value < 1) throw new ArgumentOutOfRangeException ();
				ViewState["StaticDisplayLevels"] = value;
			}
		}

	    [DefaultValueAttribute ("")]
		public string StaticItemFormatString {
			get {
				object o = ViewState ["StaticItemFormatString"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["StaticItemFormatString"] = value;
			}
		}

		[DefaultValue ("16px")]
		[ThemeableAttribute (true)]
		public Unit StaticSubMenuIndent {
			get {
				object o = ViewState ["StaticSubMenuIndent"];
				if (o != null) return (Unit)o;
				return new Unit (16);
			}
			set {
				ViewState["StaticSubMenuIndent"] = value;
			}
		}

		[ThemeableAttribute (true)]
		[DefaultValue (3)]
		public int MaximumDynamicDisplayLevels {
			get {
				object o = ViewState ["MaximumDynamicDisplayLevels"];
				if (o != null) return (int)o;
				return 3;
			}
			set {
				if (value < 0) throw new ArgumentOutOfRangeException ();
				ViewState["MaximumDynamicDisplayLevels"] = value;
			}
		}

		[DefaultValue (0)]
		public int DynamicVerticalOffset {
			get {
				object o = ViewState ["DynamicVerticalOffset"];
				if (o != null) return (int)o;
				return 0;
			}
			set {
				ViewState["DynamicVerticalOffset"] = value;
			}
		}

		[DefaultValue (0)]
		public int DynamicHorizontalOffset {
			get {
				object o = ViewState ["DynamicHorizontalOffset"];
				if (o != null) return (int)o;
				return 0;
			}
			set {
				ViewState["DynamicHorizontalOffset"] = value;
			}
		}

		[DefaultValue (true)]
		public bool DynamicEnableDefaultPopOutImage {
			get {
				object o = ViewState ["dedpoi"];
				if (o != null) return (bool)o;
				return true;
			}
			set {
				ViewState["dedpoi"] = value;
			}
		}

		[DefaultValue (true)]
		public bool StaticEnableDefaultPopOutImage {
			get {
				object o = ViewState ["sedpoi"];
				if (o != null) return (bool)o;
				return true;
			}
			set {
				ViewState["sedpoi"] = value;
			}
		}

	    [DefaultValueAttribute (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.MenuItemCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	    [MergablePropertyAttribute (false)]
		public MenuItemCollection Items {
			get {
				if (items == null) {
					items = new MenuItemCollection (this);
					if (IsTrackingViewState)
						((IStateManager)items).TrackViewState();
				}
				return items;
			}
		}

		[DefaultValue ('/')]
		public char PathSeparator {
			get {
				object o = ViewState ["PathSeparator"];
				if(o != null) return (char)o;
				return '/';
			}
			set {
				ViewState ["PathSeparator"] = value;
			}
		}

		[DefaultValue (false)]
		public bool ItemWrap {
			get {
				object o = ViewState ["ItemWrap"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["ItemWrap"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle DynamicMenuItemStyle {
			get {
				if (dynamicMenuItemStyle == null) {
					dynamicMenuItemStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						dynamicMenuItemStyle.TrackViewState();
				}
				return dynamicMenuItemStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle DynamicSelectedStyle {
			get {
				if (dynamicSelectedStyle == null) {
					dynamicSelectedStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						dynamicSelectedStyle.TrackViewState();
				}
				return dynamicSelectedStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SubMenuStyle DynamicMenuStyle {
			get {
				if (dynamicMenuStyle == null) {
					dynamicMenuStyle = new SubMenuStyle ();
					if (IsTrackingViewState)
						dynamicMenuStyle.TrackViewState();
				}
				return dynamicMenuStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle StaticMenuItemStyle {
			get {
				if (staticMenuItemStyle == null) {
					staticMenuItemStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						staticMenuItemStyle.TrackViewState();
				}
				return staticMenuItemStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle StaticSelectedStyle {
			get {
				if (staticSelectedStyle == null) {
					staticSelectedStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						staticSelectedStyle.TrackViewState();
				}
				return staticSelectedStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SubMenuStyle StaticMenuStyle {
			get {
				if (staticMenuStyle == null) {
					staticMenuStyle = new SubMenuStyle ();
					if (IsTrackingViewState)
						staticMenuStyle.TrackViewState();
				}
				return staticMenuStyle;
			}
		}

		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Editor ("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public MenuItemStyleCollection LevelMenuItemStyles {
			get {
				if (levelMenuItemStyles == null) {
					levelMenuItemStyles = new MenuItemStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelMenuItemStyles).TrackViewState();
				}
				return levelMenuItemStyles;
			}
		}

		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Editor ("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public MenuItemStyleCollection LevelSelectedStyles {
			get {
				if (levelSelectedStyles == null) {
					levelSelectedStyles = new MenuItemStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelSelectedStyles).TrackViewState();
				}
				return levelSelectedStyles;
			}
		}

		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Editor ("System.Web.UI.Design.WebControls.SubMenuStyleCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public SubMenuStyleCollection LevelSubMenuStyles {
			get {
				if (levelSubMenuStyles == null) {
					levelSubMenuStyles = new SubMenuStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelSubMenuStyles).TrackViewState();
				}
				return levelSubMenuStyles;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Style DynamicHoverStyle {
			get {
				if (dynamicHoverStyle == null) {
					dynamicHoverStyle = new Style ();
					if (IsTrackingViewState)
						dynamicHoverStyle.TrackViewState();
				}
				return dynamicHoverStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Style StaticHoverStyle {
			get {
				if (staticHoverStyle == null) {
					staticHoverStyle = new Style ();
					if (IsTrackingViewState)
						staticHoverStyle.TrackViewState();
				}
				return staticHoverStyle;
			}
		}
		
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string ScrollDownImageUrl {
			get {
				object o = ViewState ["sdiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["sdiu"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string ScrollUpImageUrl {
			get {
				object o = ViewState ["suiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["suiu"] = value;
			}
		}

		[Localizable (true)]
		public string ScrollDownText {
			get {
				object o = ViewState ["ScrollDownText"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState["ScrollDownText"] = value;
			}
		}

		[Localizable (true)]
		public string ScrollUpText {
			get {
				object o = ViewState ["ScrollUpText"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState["ScrollUpText"] = value;
			}
		}

		[MonoTODO]
		public string DynamicPopOutImageTextFormatString 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DynamicPopOutImageUrl {
			get {
				object o = ViewState ["dpoiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["dpoiu"] = value;
			}
		}

		[MonoTODO]
		public string StaticPopOutImageTextFormatString
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string StaticPopOutImageUrl {
			get {
				object o = ViewState ["spoiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["spoiu"] = value;
			}
		}

		[DefaultValue ("")]
		public string Target {
			get {
				object o = ViewState ["Target"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState["Target"] = value;
			}
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(MenuItemTemplateContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate StaticItemTemplate {
			get { return staticItemTemplate; }
			set { staticItemTemplate = value; }
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(MenuItemTemplateContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
	    [Browsable (false)]
		public ITemplate DynamicItemTemplate {
			get { return dynamicItemTemplate; }
			set { dynamicItemTemplate = value; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public MenuItem SelectedItem {
			get {
				if (selectedItem == null && selectedItemPath != null) {
					selectedItem = FindItemByPos (selectedItemPath);
				}
				
				return selectedItem;
			}
		}

		[Browsable (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedValue {
			get { return selectedItem != null ? selectedItem.Value : null; }
		}

		[MonoTODO]
		[Localizable (true)]
		public string SkipLinkText 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		

		internal void SetSelectedItem (MenuItem item)
		{
			if (selectedItem == item) return;
			selectedItem = item;
			selectedItemPath = item.Path;
		}
		
		public MenuItem FindItem (string valuePath)
		{
			if (valuePath == null) throw new ArgumentNullException ("valuePath");
			string[] path = valuePath.Split (PathSeparator);
			int n = 0;
			MenuItemCollection col = Items;
			bool foundBranch = true;
			while (col.Count > 0 && foundBranch) {
				foundBranch = false;
				foreach (MenuItem item in col) {
					if (item.Value == path [n]) {
						if (++n == path.Length) return item;
						col = item.ChildItems;
						foundBranch = true;
						break;
					}
				}
			}
			return null;
		}
		
		string GetBindingKey (string dataMember, int depth)
		{
			return dataMember + " " + depth;
		}
		
		internal MenuItemBinding FindBindingForItem (string type, int depth)
		{
			if (bindings == null) return null;

			MenuItemBinding bin = (MenuItemBinding) bindings [GetBindingKey (type, depth)];
			if (bin != null) return bin;
			
			bin = (MenuItemBinding) bindings [GetBindingKey (type, -1)];
			if (bin != null) return bin;
			
			bin = (MenuItemBinding) bindings [GetBindingKey ("", depth)];
			if (bin != null) return bin;
			
			bin = (MenuItemBinding) bindings [GetBindingKey ("", -1)];
			return bin;
		}
		
		protected internal override void PerformDataBinding ()
		{
			base.PerformDataBinding ();
			HierarchicalDataSourceView data = GetData ("");
			IHierarchicalEnumerable e = data.Select ();
			foreach (object obj in e) {
				IHierarchyData hdata = e.GetHierarchyData (obj);
				MenuItem item = new MenuItem ();
				item.Bind (hdata);
				Items.Add (item);
				OnMenuItemDataBound (new MenuEventArgs (item));
			}
		}
		
		protected void SetItemDataBound (MenuItem node, bool dataBound)
		{
			node.SetDataBound (dataBound);
		}
		
		protected void SetItemDataPath (MenuItem node, string dataPath)
		{
			node.SetDataPath (dataPath);
		}
		
		protected void SetItemDataItem (MenuItem node, object dataItem)
		{
			node.SetDataItem (dataItem);
		}
		
		protected internal virtual void RaisePostBackEvent (string eventArgument)
		{
			MenuItem item = FindItemByPos (eventArgument);
			if (item == null) return;
			item.Selected = true;
			OnMenuItemClick (new MenuEventArgs (item));
		}

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
		
		MenuItem FindItemByPos (string path)
		{
			string[] indexes = path.Split ('_');
			MenuItem item = null;
			
			foreach (string index in indexes) {
				int i = int.Parse (index);
				if (item == null) {
					if (i >= Items.Count) return null;
					item = Items [i];
				} else {
					if (i >= item.ChildItems.Count) return null;
					item = item.ChildItems [i];
				}
			}
			return item;
		}
		
		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
		}
		
		protected override void TrackViewState()
		{
			EnsureDataBound ();
			
			base.TrackViewState();
			if (dataBindings != null) {
				((IStateManager)dataBindings).TrackViewState ();
			}
			if (items != null) {
				((IStateManager)items).TrackViewState();
			}
			if (dynamicMenuItemStyle != null)
				dynamicMenuItemStyle.TrackViewState ();
			if (dynamicMenuStyle != null)
				dynamicMenuStyle.TrackViewState ();
			if (levelMenuItemStyles != null)
				((IStateManager)levelMenuItemStyles).TrackViewState();
			if (levelSelectedStyles != null)
				((IStateManager)levelSelectedStyles).TrackViewState();
			if (levelSubMenuStyles != null)
				((IStateManager)levelSubMenuStyles).TrackViewState();
			if (dynamicSelectedStyle != null)
				dynamicSelectedStyle.TrackViewState();
			if (staticMenuItemStyle != null)
				staticMenuItemStyle.TrackViewState ();
			if (staticMenuStyle != null)
				staticMenuStyle.TrackViewState ();
			if (staticSelectedStyle != null)
				staticSelectedStyle.TrackViewState();
			if (staticHoverStyle != null)
				staticHoverStyle.TrackViewState();
			if (dynamicHoverStyle != null)
				dynamicHoverStyle.TrackViewState();
		}

		protected override object SaveViewState()
		{
			object[] states = new object [14];
			states[0] = base.SaveViewState ();
			states[1] = dataBindings == null ? null : ((IStateManager)dataBindings).SaveViewState();
			states[2] = items == null ? null : ((IStateManager)items).SaveViewState();
			states[3] = dynamicMenuItemStyle == null ? null : dynamicMenuItemStyle.SaveViewState();
			states[4] = dynamicMenuStyle == null ? null : dynamicMenuStyle.SaveViewState();
			states[5] = levelMenuItemStyles == null ? null : ((IStateManager)levelMenuItemStyles).SaveViewState();
			states[6] = levelSelectedStyles == null ? null : ((IStateManager)levelSelectedStyles).SaveViewState();
			states[7] = dynamicSelectedStyle == null ? null : dynamicSelectedStyle.SaveViewState();
			states[8] = (staticMenuItemStyle == null ? null : staticMenuItemStyle.SaveViewState());
			states[9] = staticMenuStyle == null ? null : staticMenuStyle.SaveViewState();
			states[10] = staticSelectedStyle == null ? null : staticSelectedStyle.SaveViewState();
			states[11] = staticHoverStyle == null ? null : staticHoverStyle.SaveViewState();
			states[12] = dynamicHoverStyle == null ? null : dynamicHoverStyle.SaveViewState();
			states[13] = levelSubMenuStyles == null ? null : ((IStateManager)levelSubMenuStyles).SaveViewState();

			for (int i = states.Length - 1; i >= 0; i--) {
				if (states [i] != null)
					return states;
			}

			return null;
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object [] states = (object []) savedState;
			base.LoadViewState (states[0]);
			
			if (states[1] != null)
				((IStateManager)DataBindings).LoadViewState(states[1]);
			if (states[2] != null)
				((IStateManager)Items).LoadViewState(states[2]);
			if (states[3] != null)
				DynamicMenuItemStyle.LoadViewState (states[3]);
			if (states[4] != null)
				DynamicMenuStyle.LoadViewState (states[4]);
			if (states[5] != null)
				((IStateManager)LevelMenuItemStyles).LoadViewState(states[5]);
			if (states[6] != null)
				((IStateManager)LevelSelectedStyles).LoadViewState(states[6]);
			if (states[7] != null)
				DynamicSelectedStyle.LoadViewState (states[7]);
			if (states[8] != null)
				StaticMenuItemStyle.LoadViewState (states[8]);
			if (states[9] != null)
				StaticMenuStyle.LoadViewState (states[9]);
			if (states[10] != null)
				StaticSelectedStyle.LoadViewState (states[10]);
			if (states[11] != null)
				StaticHoverStyle.LoadViewState (states[11]);
			if (states[12] != null)
				DynamicHoverStyle.LoadViewState (states[12]);
			if (states[13] != null)
				((IStateManager)LevelSubMenuStyles).LoadViewState(states[13]);
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}
		
		protected internal override void LoadControlState (object ob)
		{
			if (ob == null) return;
			object[] state = (object[]) ob;
			base.LoadControlState (state[0]);
			selectedItemPath = state[1] as string;
		}
		
		protected internal override object SaveControlState ()
		{
			object bstate = base.SaveControlState ();
			object mstate = selectedItemPath;
			
			if (bstate != null || mstate != null)
				return new object[] { bstate, mstate };
			else
				return null;
		}
		
		protected internal override void CreateChildControls ()
		{
			base.CreateChildControls ();
		}
		
		protected override void EnsureDataBound ()
		{
			base.EnsureDataBound ();
		}

		[MonoTODO]
		protected override IDictionary GetDesignModeState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void SetDesignModeState (IDictionary data)
		{
			throw new NotImplementedException ();
		}
				
		public override ControlCollection Controls {
			get { return base.Controls; }
		}
		
		public sealed override void DataBind ()
		{
			base.DataBind ();
		}
		
		[MonoTODO]
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnDataBinding (EventArgs e)
		{
			throw new NotImplementedException ();
		}
		
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			
			if (!Page.ClientScript.IsClientScriptIncludeRegistered (typeof(Menu), "Menu.js")) {
				string url = Page.ClientScript.GetWebResourceUrl (typeof(Menu), "Menu.js");
				Page.ClientScript.RegisterClientScriptInclude (typeof(Menu), "Menu.js", url);
			}
			
			string cmenu = ClientID + "_data";
			string script = string.Format ("var {0} = new Object ();\n", cmenu);
			script += string.Format ("{0}.disappearAfter = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (DisappearAfter));
			script += string.Format ("{0}.vertical = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (Orientation == Orientation.Vertical));
			if (DynamicHorizontalOffset != 0)
				script += string.Format ("{0}.dho = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (DynamicHorizontalOffset));
			if (DynamicVerticalOffset != 0)
				script += string.Format ("{0}.dvo = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (DynamicVerticalOffset));
				
			// The order in which styles are defined matters when more than one class
			// is assigned to an element
			
			if (dynamicMenuStyle != null)
				RegisterItemStyle (dynamicMenuStyle);
			if (staticMenuStyle != null)
				RegisterItemStyle (staticMenuStyle);
		
			if (staticMenuItemStyle != null)
				RegisterItemStyle (staticMenuItemStyle);

			if (dynamicMenuItemStyle != null)
				RegisterItemStyle (dynamicMenuItemStyle);

			if (levelSubMenuStyles != null)
				foreach (Style style in levelSubMenuStyles)
					RegisterItemStyle (style);

			if (levelMenuItemStyles != null)
				foreach (Style style in levelMenuItemStyles)
					RegisterItemStyle (style);

			if (staticSelectedStyle != null)
				RegisterItemStyle (staticSelectedStyle);
			if (dynamicSelectedStyle != null)
				RegisterItemStyle (dynamicSelectedStyle);
				
			if (levelSelectedStyles != null)
				foreach (Style style in levelSelectedStyles)
					RegisterItemStyle (style);
			
			if (dynamicHoverStyle != null)
				RegisterItemStyle (dynamicHoverStyle);
			if (staticHoverStyle != null)
				RegisterItemStyle (staticHoverStyle);

			if (staticHoverStyle != null)
				script += string.Format ("{0}.staticHover = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (staticHoverStyle.RegisteredCssClass));
			if (dynamicHoverStyle != null)
				script += string.Format ("{0}.dynamicHover = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (dynamicHoverStyle.RegisteredCssClass));
			
			Page.ClientScript.RegisterStartupScript (typeof(Menu), ClientID, script, true);

			if (dataBindings != null && dataBindings.Count > 0) {
				bindings = new Hashtable ();
				foreach (TreeNodeBinding bin in dataBindings) {
					string key = GetBindingKey (bin.DataMember, bin.Depth);
					bindings [key] = bin;
				}
			}
			else
				bindings = null;
		}
		
		void RegisterItemStyle (Style baseStyle)
		{
			Page.Header.StyleSheet.RegisterStyle (baseStyle, this);
			Style ts = new Style ();
			ts.CopyTextStylesFrom (baseStyle);
			Page.Header.StyleSheet.CreateStyleRule (ts, "." + baseStyle.RegisteredCssClass + " A", this);
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			base.Render (writer);
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			RenderMenuBeginTagAttributes (writer, false, 0);
		}
		
		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			base.RenderBeginTag (writer);
		}
		
		public override void RenderEndTag (HtmlTextWriter writer)
		{
			base.RenderEndTag (writer);
			
			// Render dynamic menus outside the main control tag
			for (int n=0; n<dynamicMenus.Count; n++) {
				MenuItem item = (MenuItem) dynamicMenus [n];
				RenderDynamicMenu (writer, item);
			}
			dynamicMenus = null;
		}
		
		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			dynamicMenus = new ArrayList ();
			RenderMenuBody (writer, Items, Orientation == Orientation.Vertical, false);
		}
		
		void RenderDynamicMenu (HtmlTextWriter writer, MenuItem item)
		{
			if (dynamicMenuStyle != null)
				writer.AddAttribute ("class", dynamicMenuStyle.RegisteredCssClass);
			
			writer.AddStyleAttribute ("visibility", "hidden");
			writer.AddStyleAttribute ("position", "absolute");
			writer.AddStyleAttribute ("left", "0px");
			writer.AddStyleAttribute ("top", "0px");
			writer.AddAttribute ("id", GetItemClientId (item, "s"));
			writer.RenderBeginTag (HtmlTextWriterTag.Div);

			// Up button
			writer.AddAttribute ("id", GetItemClientId (item, "cu"));
			writer.AddStyleAttribute ("display", "block");
			writer.AddStyleAttribute ("text-align", "center");
			writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverScrollBtn ('{0}','{1}','{2}')", ClientID, item.Path, "u"));
			writer.AddAttribute ("onmouseout", string.Format ("javascript:Menu_OutScrollBtn ('{0}','{1}','{2}')", ClientID, item.Path, "u"));
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			
			string src = ScrollUpImageUrl != "" ? ScrollUpImageUrl : Page.ClientScript.GetWebResourceUrl (typeof(Menu), "arrow_up.gif");
			writer.AddAttribute ("src", src);
			writer.AddAttribute ("alt", ScrollUpText);
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();	// IMG
			
			writer.RenderEndTag ();	// DIV scroll button
		
			writer.AddAttribute ("id", GetItemClientId (item, "cb"));	// Scroll container
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			writer.AddAttribute ("id", GetItemClientId (item, "cc"));	// Content
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			
			RenderMenu (writer, item.ChildItems, true, true, item.Depth + 1);
			
			writer.RenderEndTag ();	// DIV Content
			writer.RenderEndTag ();	// DIV Scroll container

			// Down button
			writer.AddAttribute ("id", GetItemClientId (item, "cd"));
			writer.AddStyleAttribute ("display", "block");
			writer.AddStyleAttribute ("text-align", "center");
			writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverScrollBtn ('{0}','{1}','{2}')", ClientID, item.Path, "d"));
			writer.AddAttribute ("onmouseout", string.Format ("javascript:Menu_OutScrollBtn ('{0}','{1}','{2}')", ClientID, item.Path, "d"));
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			
			src = ScrollDownImageUrl != "" ? ScrollDownImageUrl : Page.ClientScript.GetWebResourceUrl (typeof(Menu), "arrow_down.gif");
			writer.AddAttribute ("src", src);
			writer.AddAttribute ("alt", ScrollDownText);
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();	// IMG
			
			writer.RenderEndTag ();	// DIV scroll button
			
			writer.RenderEndTag ();	// DIV menu
		}
		
		void RenderMenuBeginTagAttributes (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			writer.AddAttribute ("cellpadding", "0");
			writer.AddAttribute ("cellspacing", "0");

			string cls = string.Empty;
			
			if (!dynamic && staticMenuStyle != null)
				cls += staticMenuStyle.RegisteredCssClass + " ";
				
			if (levelSubMenuStyles != null && menuLevel < levelSubMenuStyles.Count)
				cls += levelSubMenuStyles [menuLevel].RegisteredCssClass;
			
			if (cls.Length != 0)
				writer.AddAttribute ("class", cls);
		}
		
		void RenderMenu (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, int menuLevel)
		{
			RenderMenuBeginTag (writer, dynamic, menuLevel);
			RenderMenuBody (writer, items, vertical, dynamic);
			RenderMenuEndTag (writer);
		}
		
		void RenderMenuBeginTag (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			RenderMenuBeginTagAttributes (writer, dynamic, menuLevel);
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
		}
		
		void RenderMenuEndTag (HtmlTextWriter writer)
		{
			writer.RenderEndTag ();
		}
		
		void RenderMenuBody (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic)
		{
			if (!vertical) writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			for (int n=0; n<items.Count; n++) {
				MenuItem item = items [n];
				if (n > 0) {
					Unit itemSpacing = GetItemSpacing (item, dynamic);
					if (itemSpacing != Unit.Empty) {
						if (vertical) {
							writer.AddAttribute ("height", itemSpacing.ToString());
							writer.RenderBeginTag (HtmlTextWriterTag.Tr);
							writer.RenderEndTag ();
						} else {
							writer.AddAttribute ("width", itemSpacing.ToString());
							writer.RenderBeginTag (HtmlTextWriterTag.Td);
							writer.RenderEndTag ();
						}
					}
				}
				RenderMenuItem (writer, item);
			}
			
			if (!vertical) writer.RenderEndTag ();	// TR
		}
		
		void RenderMenuItem (HtmlTextWriter writer, MenuItem item)
		{
			bool displayChildren = (item.Depth + 1 < StaticDisplayLevels + MaximumDynamicDisplayLevels);
			bool dynamicChildren = displayChildren && (item.Depth + 1 >= StaticDisplayLevels) && item.ChildItems.Count > 0;
			bool isDynamicItem = item.Depth + 1 > StaticDisplayLevels;
			bool vertical = (Orientation == Orientation.Vertical) || isDynamicItem;

			if (vertical)
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			Style itemStyle = null;
			if (levelMenuItemStyles != null && item.Depth < levelMenuItemStyles.Count)
				itemStyle = levelMenuItemStyles [item.Depth];
			else if (isDynamicItem) {
				if (dynamicMenuItemStyle != null)
					itemStyle = dynamicMenuItemStyle;
			} else {
				if (staticMenuItemStyle != null)
					itemStyle = staticMenuItemStyle;
			}
			
			Style selectedStyle = null;
			if (item == SelectedItem) {
				if (levelSelectedStyles != null && item.Depth < levelSelectedStyles.Count)
					selectedStyle = levelSelectedStyles [item.Depth];
				else if (isDynamicItem) {
					if (dynamicSelectedStyle != null)
						selectedStyle = dynamicSelectedStyle;
				} else {
					if (staticSelectedStyle != null)
						selectedStyle = staticSelectedStyle;
				}
			}
			
			string cls = "";
			if (itemStyle != null) cls += itemStyle.RegisteredCssClass + " ";
			if (selectedStyle != null) cls += selectedStyle.RegisteredCssClass + " ";
			if (cls != "")
				writer.AddAttribute ("class", cls);
			
			string parentId = isDynamicItem ? "'" + item.Parent.Path + "'" : "null";
			if (dynamicChildren) {
				writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverItem ('{0}','{1}',{2})", ClientID, item.Path, parentId));
				writer.AddAttribute ("onmouseout", string.Format ("javascript:Menu_OutItem ('{0}','{1}')", ClientID, item.Path));
			} else if (isDynamicItem) {
				writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverDynamicLeafItem ('{0}','{1}',{2})", ClientID, item.Path, parentId));
				writer.AddAttribute ("onmouseout", string.Format ("javascript:Menu_OutItem ('{0}','{1}',{2})", ClientID, item.Path, parentId));
			} else {
				writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverStaticLeafItem ('{0}','{1}')", ClientID, item.Path));
				writer.AddAttribute ("onmouseout", string.Format ("javascript:Menu_OutItem ('{0}','{1}')", ClientID, item.Path));
			}
			
			writer.AddAttribute ("id", GetItemClientId (item, "i"));
			
			writer.RenderBeginTag (HtmlTextWriterTag.Td);

			// Top separator image

			if (isDynamicItem && DynamicTopSeparatorImageUrl != "") {
				writer.AddAttribute ("src", DynamicTopSeparatorImageUrl);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			} else  if (!isDynamicItem && StaticTopSeparatorImageUrl != "") {
				writer.AddAttribute ("src", StaticTopSeparatorImageUrl);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			}
			
			// Menu item box
			
			writer.AddAttribute ("cellpadding", "0");
			writer.AddAttribute ("cellspacing", "0");
			writer.AddAttribute ("width", "100%");
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			if (item.Depth > 0 && !isDynamicItem) {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.AddStyleAttribute ("width", StaticSubMenuIndent.ToString ());
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
				writer.RenderEndTag ();	// DIV
				writer.RenderEndTag ();	// TD
			}
			
			if (item.ImageUrl != "") {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				RenderItemHref (writer, item);
				writer.RenderBeginTag (HtmlTextWriterTag.A);
				writer.AddAttribute ("src", item.ImageUrl);
				writer.AddAttribute ("border", "0");
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
				writer.RenderEndTag ();	// A
				writer.RenderEndTag ();	// TD
			}
			
			// Menu item text
			
			writer.AddAttribute ("width", "100%");
			if (!ItemWrap)
				writer.AddAttribute ("nowrap", "nowrap");
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			
			RenderItemHref (writer, item);
			writer.AddStyleAttribute ("text-decoration", "none");
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			RenderItemContent (writer, item, isDynamicItem);
			writer.RenderEndTag ();	// A
			
			writer.RenderEndTag ();	// TD
			
			// Popup image
			
			if (dynamicChildren) {
				string popOutImage = GetPopOutImage (item, isDynamicItem);
				if (popOutImage != null)
				{
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
					writer.AddAttribute ("src", popOutImage);
					writer.AddAttribute ("border", "0");
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();	// IMG
					writer.RenderEndTag ();	// TD
				}
			}
			
			writer.RenderEndTag ();	// TR
			writer.RenderEndTag ();	// TABLE
			
			// Bottom separator image
				
			string separatorImg = item.SeparatorImageUrl;
			if (separatorImg.Length == 0) { 
				if (isDynamicItem) separatorImg = DynamicBottomSeparatorImageUrl;
				else separatorImg = StaticBottomSeparatorImageUrl;
			}
			if (separatorImg.Length > 0) {
				writer.AddAttribute ("src", separatorImg);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			}
				
			// Submenu
				
			if (vertical) {
				if (displayChildren) {
					if (dynamicChildren) dynamicMenus.Add (item);
					else {
						writer.AddAttribute ("width", "100%");
						RenderMenu (writer, item.ChildItems, true, false, item.Depth + 1);
					}
				}
				
				writer.RenderEndTag ();	// TD
				writer.RenderEndTag ();	// TR
			} else {
				writer.RenderEndTag ();	// TD
				
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				if (displayChildren) {
					if (dynamicChildren) dynamicMenus.Add (item);
					else RenderMenu (writer, item.ChildItems, false, false, item.Depth + 1);
				}
				writer.RenderEndTag ();	// TD
			}
		}
		
		void RenderItemContent (HtmlTextWriter writer, MenuItem item, bool isDynamicItem)
		{
			if (isDynamicItem && dynamicItemTemplate != null) {
				MenuItemTemplateContainer cter = new MenuItemTemplateContainer (item.Index, item);
				dynamicItemTemplate.InstantiateIn (cter);
				cter.Render (writer);
			} else if (!isDynamicItem && staticItemTemplate != null) {
				MenuItemTemplateContainer cter = new MenuItemTemplateContainer (item.Index, item);
				staticItemTemplate.InstantiateIn (cter);
				cter.Render (writer);
			} else if (isDynamicItem && DynamicItemFormatString.Length > 0) {
				writer.Write (string.Format (DynamicItemFormatString, item.Text));
			} else if (!isDynamicItem && StaticItemFormatString.Length > 0) {
				writer.Write (string.Format (StaticItemFormatString, item.Text));
			} else {
				writer.Write (item.Text);
			}
			
		}
			
		Unit GetItemSpacing (MenuItem item, bool dynamic)
		{
			Unit itemSpacing;
			
			if (item.Selected) {
				if (levelSelectedStyles != null && item.Depth < levelSelectedStyles.Count) {
					itemSpacing = levelSelectedStyles [item.Depth].ItemSpacing;
					if (itemSpacing != Unit.Empty) return itemSpacing;
				}
				
				if (dynamic) itemSpacing = DynamicSelectedStyle.ItemSpacing;
				else itemSpacing = StaticSelectedStyle.ItemSpacing;
				if (itemSpacing != Unit.Empty) return itemSpacing;
			}
			
			if (levelMenuItemStyles != null && item.Depth < levelMenuItemStyles.Count) {
				itemSpacing = levelMenuItemStyles [item.Depth].ItemSpacing;
				if (itemSpacing != Unit.Empty) return itemSpacing;
			}
			
			if (dynamic) return DynamicMenuItemStyle.ItemSpacing;
			else return StaticMenuItemStyle.ItemSpacing;
		}
		
		string GetPopOutImage (MenuItem item, bool isDynamicItem)
		{
			if (item.PopOutImageUrl != "")
				return item.PopOutImageUrl;

			if (isDynamicItem) {
				if (DynamicPopOutImageUrl != "")
					return DynamicPopOutImageUrl;
				if (DynamicEnableDefaultPopOutImage)
					return AssemblyResourceLoader.GetResourceUrl (typeof(Menu), "arrow_plus.gif");
			} else {
				if (StaticPopOutImageUrl != "")
					return StaticPopOutImageUrl;
				if (StaticEnableDefaultPopOutImage)
					return AssemblyResourceLoader.GetResourceUrl (typeof(Menu), "arrow_plus.gif");
			}
			return null;
		}
			
		void RenderItemHref (HtmlTextWriter writer, MenuItem item)
		{
			if (!item.BranchEnabled) {
				writer.AddAttribute ("disabled", "true");
			}
			else if (!item.Selectable) {
				writer.AddAttribute ("href", "#");
				writer.AddStyleAttribute ("cursor", "text");
			}
			else if (item.NavigateUrl != "") {
				writer.AddAttribute ("href", item.NavigateUrl);
				if (item.Target != "")
					writer.AddAttribute ("target", item.Target);
				else if (Target != "")
					writer.AddAttribute ("target", Target);
			}
			else {
				writer.AddAttribute ("href", GetClientEvent (item));
			}

		}
		
		string GetItemClientId (MenuItem item, string sufix)
		{
			return ClientID + "_" + item.Path + sufix;
		}
							
		string GetClientEvent (MenuItem item)
		{
			return Page.ClientScript.GetPostBackClientHyperlink (this, item.Path);
		}
	}
}

#endif
