//
// System.ComponentModel.PropertyDescriptor.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;

namespace System.ComponentModel {

	public abstract class PropertyDescriptor : MemberDescriptor {

		public PropertyDescriptor (MemberDescriptor reference)
			: base (reference)
		{
		}

		public PropertyDescriptor (MemberDescriptor reference, Attribute [] attrs)
			: base (reference, attrs)
		{
		}

		public PropertyDescriptor (string name, Attribute [] attrs)
			: base (name, attrs)
		{
		}

		public abstract Type ComponentType { get; }

		[MonoTODO]
		public virtual TypeConverter Converter {
			get {
				// FIXME: Implement me.
				
				return null;
			}
		}

		public virtual bool IsLocalizable {
			get {
				foreach (Attribute attr in AttributeArray){
					if (attr is LocalizableAttribute){
						return ((LocalizableAttribute) attr).IsLocalizable;
					}
				}

				return false;
			}
		}

		public abstract bool IsReadOnly { get; }

		public abstract Type PropertyType { get; }

		public DesignerSerializationVisibility SerializationVisibility {
			get {
				foreach (Attribute attr in AttributeArray){
					if (attr is DesignerSerializationVisibilityAttribute){
						DesignerSerializationVisibilityAttribute a;

						a = (DesignerSerializationVisibilityAttribute) attr;

						return a.Visibility;
					}
				}

				//
				// Is this a good default if we cant find the property?
				//
				return DesignerSerializationVisibility.Hidden;
			}
		}

		Hashtable notifiers;

		public virtual void AddValueChanged (object component, EventHandler handler)
		{
			EventHandler component_notifiers;
			
			if (component == null)
				throw new ArgumentNullException ("component");

			if (handler == null)
				throw new ArgumentNullException ("handler");

			if (notifiers == null)
				notifiers = new Hashtable ();

			component_notifiers = (EventHandler) notifiers [component];

			if (component_notifiers != null)
				component_notifiers += handler;
			else
				notifiers [component] = handler;
		}
		
		protected virtual void OnValueChanged (object component, EventArgs e)
		{
			if (notifiers == null)
				return;

			EventHandler component_notifiers = (EventHandler) notifiers [component];

			if (component_notifiers == null)
				return;

			component_notifiers (component, e);
		}
	}
}
