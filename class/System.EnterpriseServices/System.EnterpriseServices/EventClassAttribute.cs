// 
// System.EnterpriseServices.EventClassAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	[ComVisible(false)]
	public sealed class EventClassAttribute : Attribute {

		#region Fields

		bool allowInProcSubscribers;
		bool fireInParallel;
		string publisherFilter;

		#endregion // Fields

		#region Constructors

		public EventClassAttribute ()
		{
			allowInProcSubscribers = true;
			fireInParallel = false;
			publisherFilter = null;
		}

		#endregion // Constructors

		#region Properties

		public bool AllowInprocSubscribers {
			get { return allowInProcSubscribers; }
			set { allowInProcSubscribers = value; }
		}

		public bool FireInParallel {
			get { return fireInParallel; }
			set { fireInParallel = value; }
		}

		public string PublisherFilter {
			get { return publisherFilter; }
			set { publisherFilter = value; }
		}

		#endregion // Properties
	}
}
