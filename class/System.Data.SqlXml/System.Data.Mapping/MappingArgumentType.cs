//
// System.Data.Mapping.MappingArgumentType
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

namespace System.Data.Mapping {
        public enum MappingArgumentType 
        {
		NotRequired,
		Field,
		DomainField,
		Constant,
		MappingParameter
        }
}

#endif // NET_2_0
