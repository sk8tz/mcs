//
// System.Reflection.AssemblyFlagsAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyFlagsAttribute : Attribute
	{
		// Field
		private uint flags;
		
		// Constructor
		[CLSCompliant (false)]
		public AssemblyFlagsAttribute (uint flags)
		{
			this.flags = flags;
		}

		// Property
 		[CLSCompliant (false)]
		public uint Flags
		{
			get { return flags; }
		}
	}
}
