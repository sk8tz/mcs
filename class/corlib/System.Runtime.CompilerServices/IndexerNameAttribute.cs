//
// System.Runtime.CompilerServices.IndexerNameAttributecs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.CompilerServices {

	[AttributeUsage(AttributeTargets.Property, Inherited=false)] [Serializable]
	public sealed class IndexerNameAttribute : Attribute {
		public IndexerNameAttribute (string indexer_name)
		{
		}
	}
}
