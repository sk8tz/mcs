// 
// System.Web.Services.Description.PortType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class PortType : DocumentableItem {

		#region Fields

		string name;
		OperationCollection operations;
		ServiceDescription serviceDescription;

		#endregion // Fields

		#region Constructors
		
		public PortType ()
		{
			name = String.Empty;
			operations = new OperationCollection (this);
			serviceDescription = null;
		}
		
		#endregion // Constructors

		#region Properties

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public OperationCollection Operations {
			get { return operations; }
		}
	
		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}
		
		#endregion // Properties

		#region Methods

		internal void SetParent (ServiceDescription serviceDescription)
		{
			this.serviceDescription = serviceDescription;
		}

		#endregion // Methods
	}
}
