//
// Mono.Data.TdsClient.Internal.TdsServerTypeInternal.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

namespace Mono.Data.TdsClient.Internal {
        internal enum TdsServerTypeInternal 
	{
		SqlServer, // use TDS version 4.2, 7.0, 8.0
		Sybase     // use TDS version 4,2, 5.0
	}
}
