//
// System.ContextBoundObject.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[Serializable]
	public abstract class ContextBoundObject : MarshalByRefObject
	{
		protected ContextBoundObject ()
		{
		}
	}
}
