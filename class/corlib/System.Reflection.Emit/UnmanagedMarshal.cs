
//
// System.Reflection.Emit/UnmanagedMarshal.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001-2002 Ximian, Inc.  http://www.ximian.com
//

using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System;

namespace System.Reflection.Emit {

	[Serializable]
	public sealed class UnmanagedMarshal {
		private int count;
		private UnmanagedType t;
		private UnmanagedType tbase;
		
		private UnmanagedMarshal (UnmanagedType maint, int cnt) {
			count = cnt;
			t = maint;
			tbase = maint;
		}
		private UnmanagedMarshal (UnmanagedType maint, UnmanagedType elemt) {
			count = 0;
			t = maint;
			tbase = elemt;
		}
		
		public UnmanagedType BaseType {
			get {
				if (t == UnmanagedType.LPArray || t == UnmanagedType.SafeArray)
					throw new ArgumentException ();
				return tbase;
			}
		}

		public int ElementCount {
			get {return count;}
		}

		public UnmanagedType GetUnmanagedType {
			get {return t;}
		}

		public Guid IIDGuid {
			get {return Guid.Empty;}
		}

		public static UnmanagedMarshal DefineByValArray( int elemCount) {
			return new UnmanagedMarshal (UnmanagedType.ByValArray, elemCount);
		}

		public static UnmanagedMarshal DefineByValTStr( int elemCount) {
			return new UnmanagedMarshal (UnmanagedType.ByValTStr, elemCount);
		}

		public static UnmanagedMarshal DefineLPArray( UnmanagedType elemType) {
			return new UnmanagedMarshal (UnmanagedType.LPArray, elemType);
		}

		public static UnmanagedMarshal DefineSafeArray( UnmanagedType elemType) {
			return new UnmanagedMarshal (UnmanagedType.SafeArray, elemType);
		}

		public static UnmanagedMarshal DefineUnmanagedMarshal( UnmanagedType unmanagedType) {
			return new UnmanagedMarshal (unmanagedType, unmanagedType);
		}
		
	}
}
