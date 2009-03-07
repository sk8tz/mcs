//
// support.cs: Support routines to work around the fact that System.Reflection.Emit
// can not introspect types that are being constructed
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// Copyright 2001 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2008 Novell, Inc
//

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;
using System.Globalization;

namespace Mono.CSharp {

#if GMCS_SOURCE
	public class ReflectionConstraints : GenericConstraints
	{
		GenericParameterAttributes attrs;
		Type base_type;
		Type class_constraint;
		Type[] iface_constraints;
		string name;

		public static GenericConstraints GetConstraints (Type t)
		{
			Type [] constraints = t.GetGenericParameterConstraints ();
			GenericParameterAttributes attrs = t.GenericParameterAttributes;
			if (constraints.Length == 0 && attrs == GenericParameterAttributes.None)
				return null;
			return new ReflectionConstraints (t.Name, constraints, attrs);
		}

		private ReflectionConstraints (string name, Type [] constraints, GenericParameterAttributes attrs)
		{
			this.name = name;
			this.attrs = attrs;

			if ((constraints.Length > 0) && !constraints [0].IsInterface) {
				class_constraint = constraints [0];
				iface_constraints = new Type [constraints.Length - 1];
				Array.Copy (constraints, 1, iface_constraints, 0, constraints.Length - 1);
			} else
				iface_constraints = constraints;

			if (HasValueTypeConstraint)
				base_type = TypeManager.value_type;
			else if (class_constraint != null)
				base_type = class_constraint;
			else
				base_type = TypeManager.object_type;
		}

		public override string TypeParameter {
			get { return name; }
		}

		public override GenericParameterAttributes Attributes {
			get { return attrs; }
		}

		public override Type ClassConstraint {
			get { return class_constraint; }
		}

		public override Type EffectiveBaseClass {
			get { return base_type; }
		}

		public override Type[] InterfaceConstraints {
			get { return iface_constraints; }
		}
	}
#endif

	class PtrHashtable : Hashtable {
		sealed class PtrComparer : IComparer
#if NET_2_0
			, IEqualityComparer
#endif
		{
			private PtrComparer () {}

			public static PtrComparer Instance = new PtrComparer ();

			public int Compare (object x, object y)
			{
				if (x == y)
					return 0;
				else
					return 1;
			}
#if NET_2_0
			bool IEqualityComparer.Equals (object x, object y)
			{
				return x == y;
			}
			
			int IEqualityComparer.GetHashCode (object obj)
			{
				return obj.GetHashCode ();
			}
#endif
			
		}

#if NET_2_0
		public PtrHashtable () : base (PtrComparer.Instance) {}
#else
		public PtrHashtable () 
		{
			comparer = PtrComparer.Instance;
		}
#endif

#if MS_COMPATIBLE
		//
		// Workaround System.InvalidOperationException for enums
		//
		protected override int GetHash (object key)
		{
			TypeBuilder tb = key as TypeBuilder;
			if (tb != null && tb.BaseType == TypeManager.enum_type)
				key = tb.BaseType;

			return base.GetHash (key);
		}
#endif
	}

	/*
	 * Hashtable whose keys are character arrays with the same length
	 */
	class CharArrayHashtable : Hashtable {
		sealed class ArrComparer : IComparer {
			private int len;

			public ArrComparer (int len) {
				this.len = len;
			}

			public int Compare (object x, object y)
			{
				char[] a = (char[])x;
				char[] b = (char[])y;

				for (int i = 0; i < len; ++i)
					if (a [i] != b [i])
						return 1;
				return 0;
			}
		}

		private int len;

		protected override int GetHash (Object key)
		{
			char[] arr = (char[])key;
			int h = 0;

			for (int i = 0; i < len; ++i)
				h = (h << 5) - h + arr [i];

			return h;
		}

		public CharArrayHashtable (int len)
		{
			this.len = len;
			comparer = new ArrComparer (len);
		}
	}

	struct Pair {
		public object First;
		public object Second;

		public Pair (object f, object s)
		{
			First = f;
			Second = s;
		}
	}

	public class Accessors {
		public Accessor get_or_add;
		public Accessor set_or_remove;

		// was 'set' declared before 'get'?  was 'remove' declared before 'add'?
		public bool declared_in_reverse;

		public Accessors (Accessor get_or_add, Accessor set_or_remove)
		{
			this.get_or_add = get_or_add;
			this.set_or_remove = set_or_remove;
		}
	}

	/// <summary>
	///   This is an arbitrarily seekable StreamReader wrapper.
	///
	///   It uses a self-tuning buffer to cache the seekable data,
	///   but if the seek is too far, it may read the underly
	///   stream all over from the beginning.
	/// </summary>
	public class SeekableStreamReader
	{
		const int default_average_read_length = 1024;
		const int buffer_read_length_spans = 3;

		TextReader reader;
		Stream stream;
		Encoding encoding;

		char[] buffer;
		int average_read_length;
		int buffer_start;       // in chars
		int char_count;         // count buffer[] valid characters
		int pos;                // index into buffer[]

		void ResetStream (int read_length_inc)
		{
			average_read_length += read_length_inc;
			stream.Position = 0;
			reader = new StreamReader (stream, encoding, true);
			buffer = new char [average_read_length * buffer_read_length_spans];
			buffer_start = char_count = pos = 0;
		}

		public SeekableStreamReader (Stream stream, Encoding encoding)
		{
			this.stream = stream;
			this.encoding = encoding;

			ResetStream (default_average_read_length);
		}

		/// <remarks>
		///   This value corresponds to the current position in a stream of characters.
		///   The StreamReader hides its manipulation of the underlying byte stream and all
		///   character set/decoding issues.  Thus, we cannot use this position to guess at
		///   the corresponding position in the underlying byte stream even though there is
		///   a correlation between them.
		/// </remarks>
		public int Position {
			get { return buffer_start + pos; }

			set {
				// If the lookahead was too small, re-read from the beginning.  Increase the buffer size while we're at it
				if (value < buffer_start)
					ResetStream (average_read_length / 2);

				while (value > buffer_start + char_count) {
					pos = char_count;
					if (!ReadBuffer ())
						throw new InternalErrorException ("Seek beyond end of file: " + (buffer_start + char_count - value));
				}

				pos = value - buffer_start;
			}
		}

		private bool ReadBuffer ()
		{
			int slack = buffer.Length - char_count;
			if (slack <= average_read_length / 2) {
				// shift the buffer to make room for average_read_length number of characters
				int shift = average_read_length - slack;
				Array.Copy (buffer, shift, buffer, 0, char_count - shift);
				pos -= shift;
				char_count -= shift;
				buffer_start += shift;
				slack += shift;		// slack == average_read_length
			}

			char_count += reader.Read (buffer, char_count, slack);

			return pos < char_count;
		}

		public int Peek ()
		{
			if ((pos >= char_count) && !ReadBuffer ())
				return -1;

			return buffer [pos];
		}

		public int Read ()
		{
			if ((pos >= char_count) && !ReadBuffer ())
				return -1;

			return buffer [pos++];
		}
	}

	public class DoubleHash {
		const int DEFAULT_INITIAL_BUCKETS = 100;

		public DoubleHash () : this (DEFAULT_INITIAL_BUCKETS) {}

		public DoubleHash (int size)
		{
			count = size;
			buckets = new Entry [size];
		}

		int count;
		Entry [] buckets;
		int size = 0;

		class Entry {
			public object key1;
			public object key2;
			public int hash;
			public object value;
			public Entry next;

			public Entry (object key1, object key2, int hash, object value, Entry next)
			{
				this.key1 = key1;
				this.key2 = key2;
				this.hash = hash;
				this.next = next;
				this.value = value;
			}
		}

		public bool Lookup (object a, object b, out object res)
		{
			int h = (a.GetHashCode () ^ b.GetHashCode ()) & 0x7FFFFFFF;

			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.key1.Equals (a) && e.key2.Equals (b)) {
					res = e.value;
					return true;
				}
			}
			res = null;
			return false;
		}

		public void Insert (object a, object b, object value)
		{
			// Is it an existing one?

			int h = (a.GetHashCode () ^ b.GetHashCode ()) & 0x7FFFFFFF;

			for (Entry e = buckets [h % count]; e != null; e = e.next) {
				if (e.hash == h && e.key1.Equals (a) && e.key2.Equals (b))
					e.value = value;
			}

			int bucket = h % count;
			buckets [bucket] = new Entry (a, b, h, value, buckets [bucket]);

			// Grow whenever we double in size
			if (size++ == count) {
				count <<= 1;
				count ++;

				Entry [] newBuckets = new Entry [count];
				foreach (Entry root in buckets) {
					Entry e = root;
					while (e != null) {
						int newLoc = e.hash % count;
						Entry n = e.next;
						e.next = newBuckets [newLoc];
						newBuckets [newLoc] = e;
						e = n;
					}
				}

				buckets = newBuckets;
			}
		}
	}

	class PartialMethodDefinitionInfo : MethodInfo
	{
		MethodOrOperator mc;
		MethodAttributes attrs;

		public PartialMethodDefinitionInfo (MethodOrOperator mc)
		{
			this.mc = mc;
			if ((mc.ModFlags & Modifiers.STATIC) != 0)
				attrs = MethodAttributes.Static;
		}

		public override MethodInfo GetBaseDefinition ()
		{
			throw new NotImplementedException ();
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes
		{
			get { throw new NotImplementedException (); }
		}

		public override MethodAttributes Attributes
		{
			get { return attrs; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			throw new NotImplementedException ();
		}

		public override ParameterInfo [] GetParameters ()
		{
			throw new NotImplementedException ();
		}

		public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object [] parameters, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		public override RuntimeMethodHandle MethodHandle
		{
			get { throw new NotImplementedException (); }
		}

		public override Type DeclaringType
		{
			get { return mc.Parent.TypeBuilder; }
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override Type ReturnType {
			get {
				return mc.MemberType;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public override string Name
		{
			get { return mc.Name; }
		}

		public override Type ReflectedType
		{
			get { throw new NotImplementedException (); }
		}
	}

	public class UnixUtils {
		[System.Runtime.InteropServices.DllImport ("libc", EntryPoint="isatty")]
		extern static int _isatty (int fd);
			
		public static bool isatty (int fd)
		{
			try {
				return _isatty (fd) == 1;
			} catch {
				return false;
			}
		}
	}
}
