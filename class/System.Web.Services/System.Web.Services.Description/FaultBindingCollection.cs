// 
// System.Web.Services.Description.FaultBindingCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Description {
	public sealed class FaultBindingCollection : ServiceDescriptionBaseCollection {

		#region Fields

		OperationBinding operationBinding;

		#endregion // Fields

		#region Constructors

		internal FaultBindingCollection (OperationBinding operationBinding) 
		{
			this.operationBinding = operationBinding;
		}

		#endregion // Constructors

		#region Properties

		public FaultBinding this [int index] {
			get { 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
				return (FaultBinding) List[index]; 
			}
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		public FaultBinding this [string name] {
			get { return this[IndexOf ((FaultBinding) Table[name])]; }
		}

		#endregion // Properties

		#region Methods

		public int Add (FaultBinding bindingOperationFault) 
		{
			Insert (Count, bindingOperationFault);
			return (Count - 1);
		}

		public bool Contains (FaultBinding bindingOperationFault)
		{
			return List.Contains (bindingOperationFault);
		}

		public void CopyTo (FaultBinding[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		protected override string GetKey (object value) 
		{
			if (!(value is FaultBinding))
				throw new InvalidCastException ();

			return ((FaultBinding) value).Name;
		}

		public int IndexOf (FaultBinding bindingOperationFault)
		{
			return List.IndexOf (bindingOperationFault);
		}

		public void Insert (int index, FaultBinding bindingOperationFault)
		{
			SetParent (bindingOperationFault, operationBinding);
			Table [GetKey (bindingOperationFault)] = bindingOperationFault;
			List.Insert (index, bindingOperationFault);
		}
	
		public void Remove (FaultBinding bindingOperationFault)
		{
			Table.Remove (GetKey (bindingOperationFault));
			List.Remove (bindingOperationFault);
		}
			
		protected override void SetParent (object value, object parent)
		{
			((FaultBinding) value).SetParent ((OperationBinding) parent);	
		}
			
		#endregion // Methods
	}
}
