// System.Net.NetConfig.cs
//
// Authors:
//    Jerome Laban (jlaban@wanadoo.fr)
//
//

using System;

namespace System.Net
{
	class NetConfig : ICloneable
	{
		internal bool ipv6Enabled = false;
		internal int MaxResponseHeadersLength = 64;

		internal NetConfig()
		{
		}

		object ICloneable.Clone()
		{
			return MemberwiseClone();
		}
	}
}

