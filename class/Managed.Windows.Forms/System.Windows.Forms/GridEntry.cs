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
// Copyright (c) 2004-2008 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//      Ivan N. Zlatev	  (contact@i-nz.net)
//

// NOT COMPLETE

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms.PropertyGridInternal
{
	internal class GridEntry : GridItem, ITypeDescriptorContext
	{
		#region Local Variables
		private PropertyGrid property_grid;
		private bool expanded;
		private GridItemCollection grid_items;
		private GridItem parent;
		private PropertyDescriptor[] property_descriptors;
		private int top;
		private Rectangle plus_minus_bounds;
		#endregion	// Local Variables

		#region  Contructors
		protected GridEntry (PropertyGrid propertyGrid, GridEntry parent)
		{
			if (propertyGrid == null)
				throw new ArgumentNullException ("propertyGrid");
			property_grid = propertyGrid;
			plus_minus_bounds = new Rectangle (0,0,0,0);
			top = -1;
			grid_items = new GridItemCollection ();
			expanded = false;
			this.parent = parent;
		}

		// Cannot use one PropertyDescriptor for all owners, because the
		// propertydescriptors might have different Invokees. Check
		// ReflectionPropertyDescriptor.GetInvokee and how it's used.
		//
		public GridEntry (PropertyGrid propertyGrid, PropertyDescriptor[] properties, 
				  GridEntry parent) : this (propertyGrid, parent) 
		{
			if (properties == null || properties.Length == 0)
				throw new ArgumentNullException ("prop_desc");
			property_descriptors = properties;
		}
		#endregion	// Constructors


		public override bool Expandable {
			get { return grid_items.Count > 0; }
		}

		public override bool Expanded {
			get { return expanded; }
			set {
				if (expanded != value) {
					if (value)
						property_grid.OnExpandItem (this);
					else
						property_grid.OnCollapseItem (this);
					expanded = value;
				}
			}
		}

		public override GridItemCollection GridItems {
			get { return grid_items; }
		}

		public override GridItemType GridItemType {
			get { return GridItemType.Property; }
		}

		public override string Label {
			get { return PropertyDescriptor.Name; }
		}

		public override GridItem Parent {
			get { return parent; }
		}

		public GridEntry ParentEntry {
			get { 
				if (parent != null && parent.GridItemType == GridItemType.Category)
					return parent.Parent as GridEntry;
				return parent as GridEntry; 
			}
		}

		public override PropertyDescriptor PropertyDescriptor {
			get { return property_descriptors != null ? property_descriptors[0] : null; }
		}

		public PropertyDescriptor[] PropertyDescriptors {
			get { return property_descriptors; }
		}

		public object PropertyOwner {
			get { 
				object[] owners = PropertyOwners;
				if (owners != null)
					return owners[0];
				return null;
			}
		}

		public object[] PropertyOwners {
			get { 
				if (ParentEntry != null)
					return ParentEntry.Values;
				return null;
			}
		}

		// true if the value is the same among all properties
		public bool HasMergedValue {
			get {
				if (!IsMerged)
					return false;

				object[] values = this.Values;
				for (int i=0; i+1 < values.Length; i++) {
					if (!Object.Equals (values[i], values[i+1]))
						return false;
				}
				return true;
			}
		}

		public virtual bool IsMerged {
			get { return (PropertyDescriptors != null && PropertyDescriptors.Length > 1); }
		}

		// If IsMerged this will return all values for all properties in all owners
		public virtual object[] Values {
			get {
				if (PropertyDescriptor == null || this.PropertyOwners == null)
					return null;
				if (IsMerged) {
					object[] owners = this.PropertyOwners;
					PropertyDescriptor[] properties = PropertyDescriptors;
					object[] values = new object[owners.Length];
					for (int i=0; i < owners.Length; i++)
						values[i] = properties[i].GetValue (owners[i]);
					return values;
				} else {
					return new object[] { this.Value };
				}
			}
		}

		// Returns the first value for the first propertyowner and propertydescriptor
		//
		public override object Value {
			get {
				if (PropertyDescriptor == null || PropertyOwner == null)
					return null;

				return PropertyDescriptor.GetValue (PropertyOwner);
			}
		}

		public string ValueText {
			get { return ConvertToString (this.Value); }
		}

		public override bool Select ()
		{
			property_grid.SelectedGridItem = this;
			return true;
		}

		#region ITypeDescriptorContext
		void ITypeDescriptorContext.OnComponentChanged () 
		{
		}

		bool ITypeDescriptorContext.OnComponentChanging () 
		{
			return false;
		}

		IContainer ITypeDescriptorContext.Container {
			get {
				if (PropertyOwner == null)
					return null;

				IComponent component = property_grid.SelectedObject as IComponent;
				if (component != null && component.Site != null)
					return component.Site.Container;
				return null;
			}
		}

		object ITypeDescriptorContext.Instance {
			get { return PropertyOwner; }
		}

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor {
			get { return PropertyDescriptor; }
		}
		#endregion

		#region IServiceProvider Members

		object IServiceProvider.GetService (Type serviceType) {
			IComponent selectedComponent = property_grid.SelectedObject as IComponent;
			if (selectedComponent != null && selectedComponent.Site != null)
				return selectedComponent.Site.GetService (serviceType);
			return null;
		}

		#endregion

		internal int Top {
			get { return top; }
			set {
				if (top != value)
					top = value;
			}
		}

		internal Rectangle PlusMinusBounds {
			get { return plus_minus_bounds; }
			set { plus_minus_bounds = value; }
		}

		public void SetParent (GridItem parent)
		{
			this.parent = parent;
		}

		public ICollection AcceptedValues {
			get {
				if (PropertyDescriptor != null && PropertyDescriptor.Converter != null &&
				    PropertyDescriptor.Converter.GetStandardValuesSupported ()) {
					ArrayList values = new ArrayList ();
					string stringVal = null;
					ICollection standardValues = PropertyDescriptor.Converter.GetStandardValues ();
					if (standardValues != null) {
						foreach (object value in standardValues) {
							stringVal = ConvertToString (value);
							if (stringVal != null)
								values.Add (stringVal);
						}
					}
					return values.Count > 0 ? values : null;
				}
				return null;
			}
		}

		private string ConvertToString (object value)
		{
			if (value is string)
				return (string)value;

			if (PropertyDescriptor != null && PropertyDescriptor.Converter != null &&
			    PropertyDescriptor.Converter.CanConvertTo (typeof (string))) {
				try {
					return PropertyDescriptor.Converter.ConvertToString (value);
				} catch {
					// XXX: Happens too often...
					// property_grid.ShowError ("Property value of '" + property_descriptor.Name + "' is not convertible to string.");
					return null;
				}
			}

			return null;
		}

		public bool HasCustomEditor {
			get { return GetEditor() != null; }
		}

		public UITypeEditorEditStyle EditorStyle {
			get {
				UITypeEditor editor = GetEditor ();
				if (editor != null)
					return editor.GetEditStyle ((ITypeDescriptorContext)this);
				return UITypeEditorEditStyle.None;
			}
		}

		public bool EditValue (IWindowsFormsEditorService service)
		{
			if (service == null)
				throw new ArgumentNullException ("service");

			IServiceContainer parent = ((ITypeDescriptorContext)this).GetService (typeof (IServiceContainer)) as IServiceContainer;
			ServiceContainer container = null;

			if (parent != null)
				container = new ServiceContainer (parent);
			else
				container = new ServiceContainer ();

			container.AddService (typeof (IWindowsFormsEditorService), service);

			UITypeEditor editor = GetEditor ();
			if (editor != null) {
				string error = null;
				bool success = false;
				try {
					object value = editor.EditValue ((ITypeDescriptorContext)this,
									 container,
									 this.Value);
					success = SetValue (value, out error);
				} catch (Exception e) {
					error = e.Message;
				}
				if (!success && error != null)
					property_grid.ShowError (error);
			}
			return false;
		}

		private UITypeEditor GetEditor ()
		{
			if (PropertyDescriptor != null) {
				try { // can happen, because we are missing some editors
					return PropertyDescriptor.GetEditor (typeof (UITypeEditor)) as UITypeEditor;
				} catch {
					// property_grid.ShowError ("Unable to load UITypeEditor for property '" + PropertyDescriptor.Name + "'.");
				}
			}
			return null;
		}

		public bool ToggleValue ()
		{
			if (IsReadOnly || (IsMerged && !HasMergedValue))
				return false;

			bool success = false;
			string error = null;
			if (PropertyDescriptor.PropertyType == typeof(bool))
				success = SetValue (!(bool)this.Value, out error);
			else if (PropertyDescriptor.Converter != null && 
				 PropertyDescriptor.Converter.GetStandardValuesSupported ()) {
				TypeConverter.StandardValuesCollection values = 
					(TypeConverter.StandardValuesCollection) PropertyDescriptor.Converter.GetStandardValues();
				for (int i = 0; i < values.Count; i++) {
					if (this.Value.Equals (values[i])){
						if (i < values.Count-1)
							success = SetValue (values[i+1], out error);
						else
							success = SetValue (values[0], out error);
						break;
					}
				}
			}
			if (!success && error != null)
				property_grid.ShowError (error);
			return success;
		}

		public bool SetValue (object value, out string error)
		{
			error = null;
			if (this.IsReadOnly)
				return false;

			if (SetValueCore (value, out error)) {
				property_grid.OnPropertyValueChangedInternal (this, this.Value);
				return true;
			}
			return false;
		}

		protected virtual bool SetValueCore (object value, out string error)
		{
			error = null;

			TypeConverter converter = PropertyDescriptor.Converter;
			// if the new value is not of the same type try to convert it
			if (value != null && 
			    this.Value != null && value.GetType () != this.Value.GetType ()) {
				if (converter != null &&
				    converter.CanConvertFrom (value.GetType ())) {
					try {
						value = converter.ConvertFrom (value);
					} catch {
						string valueText = ConvertToString (value);
						if (valueText != null) {
							error = "Property value '" + valueText + "' of '" + 
								PropertyDescriptor.Name + "' is not convertible to type '" +
								this.Value.GetType ().Name + "'";

						} else {
							error = "Property value of '" + 
								PropertyDescriptor.Name + "' is not convertible to type '" +
								this.Value.GetType ().Name + "'";
						}
						return false;
					}
				}
			}

			bool changed = false;
			bool current_changed = false;
			object[] propertyOwners = this.PropertyOwners;
			PropertyDescriptor[] properties = PropertyDescriptors;
			for (int i=0; i < propertyOwners.Length; i++) {
				object currentVal = properties[i].GetValue (propertyOwners[i]);
				current_changed = false;
				if (!Object.Equals (currentVal, value)) {
					if (this.ShouldCreateParentInstance) {
						Hashtable updatedParentProperties = new Hashtable ();
						PropertyDescriptorCollection parentProperties = TypeDescriptor.GetProperties (propertyOwners[i]);
						foreach (PropertyDescriptor property in parentProperties) {
							if (property.Name == properties[i].Name)
								updatedParentProperties[property.Name] = value;
							else
								updatedParentProperties[property.Name] = property.GetValue (propertyOwners[i]);
						}
						object updatedParentValue = this.ParentEntry.PropertyDescriptor.Converter.CreateInstance (updatedParentProperties);
						if (updatedParentValue != null)
							current_changed = this.ParentEntry.SetValueCore (updatedParentValue, out error);
					} else {
						try {
							properties[i].SetValue (propertyOwners[i], value);
						} catch {
							// MS seems to swallow this
							// 
							// string valueText = ConvertToString (value);
							// if (valueText != null)
							// 	error = "Property value '" + valueText + "' of '" + properties[i].Name + "' is invalid.";
							// else
							// 	error = "Property value of '" + properties[i].Name + "' is invalid.";
							// return false;
						}

						if (IsValueType (this.ParentEntry)) 
							current_changed = ParentEntry.SetValueCore (propertyOwners[i], out error);
						else
							current_changed = Object.Equals (properties[i].GetValue (propertyOwners[i]), value);
					}
					// restore original value if doesn't get set
					if (!current_changed && !PropertyDescriptor.IsReadOnly)
						properties[i].SetValue (propertyOwners[i], currentVal);
				}
				if (current_changed)
					changed = true;
			}
			return changed;
		}

		private bool IsValueType (GridEntry item)
		{
			if (item != null && item.PropertyDescriptor != null && 
			    (item.PropertyDescriptor.PropertyType.IsValueType ||
			     item.PropertyDescriptor.PropertyType.IsPrimitive))
				return true;
			return false;
		}

		public bool ResetValue ()
		{
			if (IsResetable) {
				object[] owners = this.PropertyOwners;
				PropertyDescriptor[] properties = PropertyDescriptors;
				for (int i=0; i < owners.Length; i++) {
					properties[i].ResetValue (owners[i]);
					if (IsValueType (this.ParentEntry)) {
						string error = null;
						if (!ParentEntry.SetValueCore (owners[i], out error) && error != null)
							property_grid.ShowError (error);
					}
				}
				property_grid.OnPropertyValueChangedInternal (this, this.Value);
				return true;
			}
			return false;
		}

		public bool HasDefaultValue {
			get {
				if (PropertyDescriptor != null && 
				    PropertyDescriptor.Attributes[typeof (DefaultValueAttribute)] != null)
				    return true;
				return false;
			}
		}

		// Determines if the current value can be reset
		//
		public virtual bool IsResetable {
			get { return (!IsReadOnly && PropertyDescriptor.CanResetValue (PropertyOwner)); }

		}

		// If false the entry can be modified only by the means of a predefined values
		// and not such inputed by the user.
		//
		public virtual bool IsEditable {
			get {
				if (PropertyDescriptor == null)
					return true;
				else if (PropertyDescriptor.PropertyType.IsArray)
					return false;
				else if (PropertyDescriptor.IsReadOnly && this.ShouldCreateParentInstance)
					return true;
				else if (PropertyDescriptor.Converter == null)
					return false;
				else if (PropertyDescriptor.Converter.GetStandardValuesSupported () &&
					 PropertyDescriptor.Converter.GetStandardValuesExclusive ())
					return false;
				else
					return true;
			}
		}

		// If true the the entry cannot be modified at all
		//
		public virtual bool IsReadOnly {
			get {
				// if (PropertyDescriptor != null) {
				// 	Console.WriteLine ("=== [" + PropertyDescriptor.Name + "]");
				// 	Console.WriteLine ("PropertyDescriptor.IsReadOnly: " + PropertyDescriptor.IsReadOnly);
				// 	Console.WriteLine ("ShouldCreateParentInstance: " + this.ShouldCreateParentInstance);
				// 	Console.WriteLine ("HasCustomEditor: " + HasCustomEditor);
				// 	Console.WriteLine ("Converter != null: " + (PropertyDescriptor.Converter != null).ToString());
				// 	Console.WriteLine ("Converter.GetStandardValuesSupported: " + PropertyDescriptor.Converter.GetStandardValuesSupported ().ToString ());
				// 	Console.WriteLine ("CanConvertFrom (string): " + PropertyDescriptor.Converter.CanConvertFrom ((ITypeDescriptorContext)this, typeof (string)));
				// 	Console.WriteLine ("IsArray: " + PropertyDescriptor.PropertyType.IsArray.ToString ());
				// }
				if (PropertyDescriptor == null || PropertyOwner == null ||
				    (PropertyDescriptor.IsReadOnly && !this.ShouldCreateParentInstance))
					return true;
				else if (!HasCustomEditor && PropertyDescriptor.Converter == null)
					return true;
				else if (PropertyDescriptor.Converter != null &&
					 !PropertyDescriptor.Converter.GetStandardValuesSupported () &&
					 !PropertyDescriptor.Converter.CanConvertFrom ((ITypeDescriptorContext)this,
										       typeof (string)) &&
					 !HasCustomEditor) {
					return true;
				} else if (PropertyDescriptor.PropertyType.IsArray && !HasCustomEditor)
					return true;
				else
					return false;
			}
		}

		// This is a way to set readonly properties (e.g properties without a setter).
		// The way it works is that if CreateInstance is supported by the parent's converter  
		// it gets passed a list of properties and their values which it uses to create an 
		// instance (e.g by passing them to the ctor of that object type).
		// 
		// This is used for e.g Font
		//
		public virtual bool ShouldCreateParentInstance {
			get {
				if (this.ParentEntry != null && ParentEntry.PropertyDescriptor != null) {
					TypeConverter parentConverter = Parent.PropertyDescriptor.Converter;
					if (parentConverter != null && parentConverter.GetCreateInstanceSupported ((ITypeDescriptorContext)this))
						return true;
				}
				return false;
			}
		}

		public virtual bool PaintValueSupported {
			get {
				UITypeEditor editor = GetEditor ();
				if (editor != null)
					return editor.GetPaintValueSupported ();
				return false;
			}
		}

		public virtual void PaintValue (Graphics gfx, Rectangle rect)
		{
			UITypeEditor editor = GetEditor ();
			if (editor != null) {
				try {
					editor.PaintValue (this.Value, gfx, rect);
				} catch {
					// TODO
				}
			}
		}

	}
}
