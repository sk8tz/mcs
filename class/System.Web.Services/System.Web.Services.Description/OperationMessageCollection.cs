// 
// System.Web.Services.Description.OperationMessageCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Description {
	public sealed class OperationMessageCollection : ServiceDescriptionBaseCollection {

		#region Constructors

		internal OperationMessageCollection (Operation operation)
		{
			parent = operation; 
		}

		#endregion // Constructors

		#region Properties

		public OperationFlow Flow {
			[MonoTODO ("Verify default return value.")]
			get { 
				switch (Count) {
				case 1: 
					if (this[0] is OperationInput)
						return OperationFlow.OneWay;
					else
						return OperationFlow.Notification;
					break;
				case 2:
					if (this[0] is OperationInput)
						return OperationFlow.RequestResponse;
					else
						return OperationFlow.SolicitResponse;
					break;
				}
				return OperationFlow.None; // .NET says default is SolicitResponse.  Verify this.
			}
		}

		public OperationInput Input {
			get { 
				foreach (object message in List)
					if (message is OperationInput)
						return (OperationInput) message;
				return null;
			}
		}
	
		public OperationMessage this [int index] {
			get { return (OperationMessage) List[index]; }
			set { List[index] = value; }
		}

		public OperationOutput Output {
			get { 
				foreach (object message in List)
					if (message is OperationOutput)
						return (OperationOutput) message;
				return null;
			}
		}

		#endregion // Properties

		#region Methods

		public int Add (OperationMessage operationMessage) 
		{
			Insert (Count, operationMessage);
			return (Count - 1);
		}

		public bool Contains (OperationMessage operationMessage)
		{
			return List.Contains (operationMessage);
		}

		public void CopyTo (OperationMessage[] array, int index) 
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (OperationMessage operationMessage)
		{
			return List.IndexOf (operationMessage);
		}

		public void Insert (int index, OperationMessage operationMessage)
		{
			List.Insert (index, operationMessage);
		}

		protected override void OnInsert (int index, object value)
		{
			if (Count > 2 || value.GetType () == this [0].GetType ())
				throw new InvalidOperationException ("The operation object can only contain one input and one output message.");
		}

		protected override void OnSet (int index, object oldValue, object newValue)
		{
			if (oldValue.GetType () != newValue.GetType ())
				throw new InvalidOperationException ("The message types of the old and new value are not the same.");
			base.OnSet (index, oldValue, newValue);
		}

		protected override void OnValidate (object value)
		{
			if (value == null)
				throw new NullReferenceException ("The message object is a null reference.");
			if (!(value is OperationInput || value is OperationOutput))
				throw new ArgumentException ("The message object is not an input or an output message.");
		}
	
		public void Remove (OperationMessage operationMessage)
		{
			List.Remove (operationMessage);
		}

		protected override void SetParent (object value, object parent)
		{
			((OperationMessage) value).SetParent ((Operation) parent);
		}
			
		#endregion // Methods
	}
}
