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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
	public abstract class ListControl : Control
	{
		private object data_source;
		private BindingMemberInfo value_member;
		private string display_member;
		private CurrencyManager data_manager;
#if NET_2_0
		private bool formatting_enabled;
#endif

		protected ListControl ()
		{			
			data_source = null;
			value_member = new BindingMemberInfo (string.Empty);
			display_member = string.Empty;
			data_manager = null;
			SetStyle (ControlStyles.StandardClick | ControlStyles.UserPaint
#if NET_2_0
				| ControlStyles.UseTextForAccessibility
#endif
				, false);
		}

		#region Events
		static object DataSourceChangedEvent = new object ();
		static object DisplayMemberChangedEvent = new object ();
		static object SelectedValueChangedEvent = new object ();
		static object ValueMemberChangedEvent = new object ();

		public event EventHandler DataSourceChanged {
			add { Events.AddHandler (DataSourceChangedEvent, value); }
			remove { Events.RemoveHandler (DataSourceChangedEvent, value); }
		}

		public event EventHandler DisplayMemberChanged {
			add { Events.AddHandler (DisplayMemberChangedEvent, value); }
			remove { Events.RemoveHandler (DisplayMemberChangedEvent, value); }
		}

		public event EventHandler SelectedValueChanged {
			add { Events.AddHandler (SelectedValueChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedValueChangedEvent, value); }
		}

		public event EventHandler ValueMemberChanged {
			add { Events.AddHandler (ValueMemberChangedEvent, value); }
			remove { Events.RemoveHandler (ValueMemberChangedEvent, value); }
		}

		#endregion // Events

		#region .NET 2.0 Public Properties
#if NET_2_0
		[DefaultValue (false)]
		public bool FormattingEnabled {
			get { return formatting_enabled; }
			set { formatting_enabled = value; }
		}
#endif
		#endregion

		#region Public Properties

		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
#if NET_2_0
		[AttributeProvider (typeof (IListSource))]
#else
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, " + Consts.AssemblySystem_Design)]
#endif
		public object DataSource {
			get { return data_source; }
			set {
				if (data_source == value)
					return;

				if (value == null)
					display_member = String.Empty;
				else if (!(value is IList || value is IListSource))
					throw new Exception ("Complex DataBinding accepts as a data source " +
							     "either an IList or an IListSource");

				data_source = value;
				ConnectToDataSource ();
				OnDataSourceChanged (EventArgs.Empty);
			}
		}

		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, " + Consts.AssemblySystem_Design)]
		public string DisplayMember {
			get { 
				return display_member;
			}
			set {
				if (value == null)
					value = String.Empty;

				if (display_member == value) {
					return;
				}

				display_member = value;
				ConnectToDataSource ();
				OnDisplayMemberChanged (EventArgs.Empty);
			}
		}

		public abstract int SelectedIndex {
			get;
			set;
		}

		[Bindable(BindableSupport.Yes)]
		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object SelectedValue {
			get {
				if (data_manager == null) {
					return null;
				}				
				
				object item = data_manager [SelectedIndex];
				object fil = FilterItemOnProperty (item, ValueMember);
				return fil;
			}
			set {
				if (value == null)
					return;

				if (value is string) {
					string valueString = value as string;
					if (valueString == String.Empty)
						return;
				}

				if (data_manager != null) {
					
					PropertyDescriptorCollection col = data_manager.GetItemProperties ();
					PropertyDescriptor prop = col.Find (ValueMember, true);
										
					for (int i = 0; i < data_manager.Count; i++) {
						if (value.Equals (prop.GetValue (data_manager [i]))) {
						 	SelectedIndex = i;
						 	return;
						}
					}
					SelectedIndex = -1;
					
				}
			}
		}

		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		public string ValueMember  {
			get { return value_member.BindingMember; }
			set {
				BindingMemberInfo new_value = new BindingMemberInfo (value);
				
				if (value_member.Equals (new_value)) {
					return;
				}
				
				value_member = new_value;
				
				if (display_member == string.Empty) {
					DisplayMember = value_member.BindingMember;
				}
				
				ConnectToDataSource ();
				OnValueMemberChanged (EventArgs.Empty);
			}
		}

#if NET_2_0
		protected virtual
#endif
		bool AllowSelection {
			get { return true; }
		}

		#endregion Public Properties

		#region Public Methods

		protected object FilterItemOnProperty (object item)
		{
			return FilterItemOnProperty (item, string.Empty);
		}

		protected object FilterItemOnProperty (object item, string field)
		{
			if (item == null)
				return null;

			if (field == null || field == string.Empty)
				return item;

			PropertyDescriptor prop = null;

			if (data_manager != null) {
				PropertyDescriptorCollection col = data_manager.GetItemProperties ();
				prop = col.Find (field, true);
			} else {
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (item);
				prop = properties.Find (field, true);
			}
			
			if (prop == null)
				return item;
			
			return prop.GetValue (item);
		}

		public string GetItemText (object item)
		{
			object fil = FilterItemOnProperty (item, DisplayMember);
			if (fil != null)
				return fil.ToString ();

			return item.ToString ();
		}

		protected CurrencyManager DataManager {
			get { return data_manager; }
		}

		// Used only by ListBox to avoid to break Listbox's member signature
		protected override bool IsInputKey (Keys keyData)
		{
			switch (keyData) {
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
			case Keys.Right:
			case Keys.Left:
			case Keys.End:
			case Keys.Home:
			case Keys.ControlKey:
			case Keys.Space:
			case Keys.ShiftKey:
				return true;

			default:
				return false;
			}
		}

		protected override void OnBindingContextChanged (EventArgs e)
		{
			base.OnBindingContextChanged (e);
			ConnectToDataSource ();

			if (DataManager != null) {
				SetItemsCore (DataManager.List);
				if (AllowSelection)
					SelectedIndex = DataManager.Position;
			}
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DataSourceChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDisplayMemberChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DisplayMemberChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			if (data_manager == null)
				return;
			if (data_manager.Position == SelectedIndex)
				return;
			data_manager.Position = SelectedIndex;
		}

		protected virtual void OnSelectedValueChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectedValueChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnValueMemberChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ValueMemberChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected abstract void RefreshItem (int index);
		
#if NET_2_0
		protected virtual void RefreshItems ()
		{
		}
#endif

		protected virtual void SetItemCore (int index,	object value)
		{

		}

		protected abstract void SetItemsCore (IList items);
		
		#endregion Public Methods
		
		#region Private Methods

		internal void BindDataItems ()
		{
			if (data_manager != null) {
				SetItemsCore (data_manager.List);
			}
		}

		private void ConnectToDataSource ()
		{
			if (data_source == null) {
				data_manager = null;
				return;
			}

			if (BindingContext == null) {
				return;
			}
			
			data_manager = (CurrencyManager) BindingContext [data_source];
			data_manager.PositionChanged += new EventHandler (OnPositionChanged);
			data_manager.ItemChanged += new ItemChangedEventHandler (OnItemChanged);
		}		

		private void OnItemChanged (object sender, ItemChangedEventArgs e)
		{
			/* if the list has changed, tell our subclass to re-bind */
			if (e.Index == -1)
				SetItemsCore (data_manager.List);
			else
				RefreshItem (e.Index);
		}

		private void OnPositionChanged (object sender, EventArgs e)
		{
			if (AllowSelection)
				SelectedIndex = data_manager.Position;
		}

		#endregion Private Methods
	}

}

