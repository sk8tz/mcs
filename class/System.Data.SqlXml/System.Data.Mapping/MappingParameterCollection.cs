//
// System.Data.Mapping.MappingParameterCollection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

using System.Data.SqlXml;
using System.Collections;

namespace System.Data.Mapping {
        public class MappingParameterCollection : ReadOnlyCollectionBase
        {
		#region Properties
	
		[MonoTODO]
		public MappingParameter this [int index] {
			get { throw new NotImplementedException (); }
		}
		
		#endregion // Properties

		#region Methods

		[MonoTODO]
		public bool Contains (MappingParameter dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (MappingParameter[] array, int index)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
        }
}

#endif // NET_2_0
