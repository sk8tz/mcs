// 
// System.Web.Services.Description.PortCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class PortCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal PortCollection (Service service)
			: base (service)
		{
		}

		#endregion

		#region Properties

		public Port this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (Port) List[index]; 
			}
                        set { List [index] = value; }
		}

		public Port this [string name] {
			get { return this [IndexOf ((Port) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (Port port) 
		{
			Insert (Count, port);
			return (Count - 1);
		}

		public bool Contains (Port port)
		{
			return List.Contains (port);
		}

		public void CopyTo (Port[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is Port))
				throw new InvalidCastException ();

			return ((Port) value).Name;
		}

		public int IndexOf (Port port)
		{
			return List.IndexOf (port);
		}

		public void Insert (int index, Port port)
		{
			List.Insert (index, port);
		}
	
		public void Remove (Port port)
		{
			List.Remove (port);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((Port) value).SetParent ((Service) parent);
		}
			
		#endregion // Methods
	}
}
