// 
// System.Web.Services.Description.BindingCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class BindingCollection : ServiceDescriptionBaseCollection {
		
		#region Fields

		ServiceDescription serviceDescription;

		#endregion // Fields

		#region Constructors
	
		internal BindingCollection (ServiceDescription serviceDescription) 
			: base (serviceDescription)
		{
		}

		#endregion // Constructors

		#region Properties

		public Binding this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (Binding) List[index]; 
			}
                        set { List [index] = value; }
		}

		public Binding this [string name] {
			get { return (Binding) Table[name]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Binding binding) 
		{
			Insert (Count, binding);
			return (Count - 1);
		}

		public bool Contains (Binding binding)
		{
			return List.Contains (binding);
		}

		public void CopyTo (Binding[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is Binding))
				throw new InvalidCastException ();
			return ((Binding) value).Name;
		}

		public int IndexOf (Binding binding)
		{
			return List.IndexOf (binding);
		}

		public void Insert (int index, Binding binding)
		{
			List.Insert (index, binding);
		}
	
		public void Remove (Binding binding)
		{
			List.Remove (binding);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Binding) value).SetParent ((ServiceDescription) parent);
		}
			
		#endregion // Methods
	}
}
