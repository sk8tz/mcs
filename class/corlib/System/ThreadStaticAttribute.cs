//
// System.ThreadStaticAttribute.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[AttributeUsage (AttributeTargets.Field, Inherited=false)]
	[Serializable]
	public class ThreadStaticAttribute : Attribute
	{
		// Constructors
		public ThreadStaticAttribute ()
			: base ()
		{
		}
	}
}
