// 
// System.Web.Services.Description.ServiceDescriptionBaseCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Web.Services;

namespace System.Web.Services.Description {
	public abstract class ServiceDescriptionBaseCollection : CollectionBase {
		
		#region Fields

		Hashtable table;

		#endregion // Fields

		#region Constructors
	
		protected internal ServiceDescriptionBaseCollection ()
		{
			table = new Hashtable ();
		}

		#endregion // Constructors

		#region Properties

		protected virtual IDictionary Table {
			get { return table; }
		}

		#endregion // Properties

		#region Methods

		protected virtual string GetKey (object value) 
		{
			return null; // per .NET documentation
		}

		[MonoTODO]
		protected override void OnClear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnInsertComplete (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnRemove (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSet (int index, object oldValue, object newValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void SetParent (object value, object parent)
		{
			throw new NotImplementedException ();
		}
			
		#endregion // Methods
	}
}
