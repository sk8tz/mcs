//
// System.Globalization.CompareInfo
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc. 2002
//

using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
	[Serializable]
	public class CompareInfo : IDeserializationCallback
	{
		// Keep in synch with MonoCompareInfo in the runtime. 
		private int culture;
		[NonSerialized]
		private string icu_name;
		[NonSerialized]
		private IntPtr ICU_collator;
		private int win32LCID;	// Unused, but MS.NET serializes this
		
		/* Hide the .ctor() */
		CompareInfo() {}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_compareinfo (string locale);
		
		internal CompareInfo (CultureInfo ci)
		{
			this.culture = ci.LCID;
			this.icu_name = ci.IcuName;
			this.construct_compareinfo (icu_name);
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void free_internal_collator ();
		
		~CompareInfo ()
		{
			free_internal_collator ();
		}
		
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_compare (string str1, int offset1,
						     int length1, string str2,
						     int offset2, int length2,
						     CompareOptions options);

		public virtual int Compare (string string1, string string2)
		{
			/* Short cuts... */
			if(string1.Length == 0) {
				if(string2.Length == 0) {
					return(0);
				} else {
					return(-1);
				}
			} else if(string2.Length == 0) {
				return(1);
			}

			return(internal_compare (string1, 0, string1.Length,
						 string2, 0, string2.Length,
						 CompareOptions.None));
		}

		public virtual int Compare (string string1, string string2,
					    CompareOptions options)
		{
			/* Short cuts... */
			if(string1.Length == 0) {
				if(string2.Length == 0) {
					return(0);
				} else {
					return(-1);
				}
			} else if(string2.Length == 0) {
				return(1);
			}

			return(internal_compare (string1, 0, string1.Length,
						 string2, 0, string2.Length,
						 options));
		}

		public virtual int Compare (string string1, int offset1,
					    string string2, int offset2)
		{
			/* Not in the spec, but ms does these short
			 * cuts before checking the offsets (breaking
			 * the offset >= string length specified check
			 * in the process...)
			 */
			if(string1.Length == 0 ||
			   offset1 == string1.Length) {
				if(string2.Length == 0 ||
				   offset2 == string2.Length) {
					return(0);
				} else {
					return(-1);
				}
			} else if(string2.Length == 0 ||
				  offset2 == string2.Length) {
				return(1);
			}

			if(offset1 < 0 || offset2 < 0) {
				throw new ArgumentOutOfRangeException ("Offsets must not be less than zero");
			}
			
			if(offset1 > string1.Length) {
				throw new ArgumentOutOfRangeException ("Offset1 is greater than or equal to the length of string1");
			}
			
			if(offset2 > string2.Length) {
				throw new ArgumentOutOfRangeException ("Offset2 is greater than or equal to the length of string2");
			}
			
			return(internal_compare (string1, offset1,
						 string1.Length-offset1,
						 string2, offset2,
						 string2.Length-offset2,
						 CompareOptions.None));
		}

		public virtual int Compare (string string1, int offset1,
					    string string2, int offset2,
					    CompareOptions options)
		{
			/* Not in the spec, but ms does these short
			 * cuts before checking the offsets (breaking
			 * the offset >= string length specified check
			 * in the process...)
			 */
			if(string1.Length == 0 ||
			   offset1 == string1.Length) {
				if(string2.Length == 0 ||
				   offset2 == string2.Length) {
					return(0);
				} else {
					return(-1);
				}
			} else if(string2.Length == 0 ||
				  offset2 == string2.Length) {
				return(1);
			}

			if(offset1 < 0 || offset2 < 0) {
				throw new ArgumentOutOfRangeException ("Offsets must not be less than zero");
			}
			
			if(offset1 > string1.Length) {
				throw new ArgumentOutOfRangeException ("Offset1 is greater than or equal to the length of string1");
			}
			
			if(offset2 > string2.Length) {
				throw new ArgumentOutOfRangeException ("Offset2 is greater than or equal to the length of string2");
			}
			
			return(internal_compare (string1, offset1,
						 string1.Length-offset1,
						 string2, offset2,
						 string2.Length-offset1,
						 options));
		}

		public virtual int Compare (string string1, int offset1,
					    int length1, string string2,
					    int offset2, int length2)
		{
			/* Not in the spec, but ms does these short
			 * cuts before checking the offsets (breaking
			 * the offset >= string length specified check
			 * in the process...)
			 */
			if(string1.Length == 0 ||
			   offset1 == string1.Length ||
			   length1 == 0) {
				if(string2.Length == 0 ||
				   offset2 == string2.Length ||
				   length2 == 0) {
					return(0);
				} else {
					return(-1);
				}
			} else if(string2.Length == 0 ||
				  offset2 == string2.Length ||
				  length2 == 0) {
				return(1);
			}

			if(offset1 < 0 || length1 < 0 ||
			   offset2 < 0 || length2 < 0) {
				throw new ArgumentOutOfRangeException ("Offsets and lengths must not be less than zero");
			}
			
			if(offset1 > string1.Length) {
				throw new ArgumentOutOfRangeException ("Offset1 is greater than or equal to the length of string1");
			}
			
			if(offset2 > string2.Length) {
				throw new ArgumentOutOfRangeException ("Offset2 is greater than or equal to the length of string2");
			}
			
			if(length1 > string1.Length-offset1) {
				throw new ArgumentOutOfRangeException ("Length1 is greater than the number of characters from offset1 to the end of string1");
			}
			
			if(length2 > string2.Length-offset2) {
				throw new ArgumentOutOfRangeException ("Length2 is greater than the number of characters from offset2 to the end of string2");
			}
			
			return(internal_compare (string1, offset1, length1,
						 string2, offset2, length2,
						 CompareOptions.None));
		}

		public virtual int Compare (string string1, int offset1,
					    int length1, string string2,
					    int offset2, int length2,
					    CompareOptions options)
		{
			/* Not in the spec, but ms does these short
			 * cuts before checking the offsets (breaking
			 * the offset >= string length specified check
			 * in the process...)
			 */
			if(string1.Length == 0 ||
			   offset1 == string1.Length ||
			   length1 == 0) {
				if(string2.Length == 0 ||
				   offset2 == string2.Length ||
				   length2 == 0) {
					return(0);
				} else {
					return(-1);
				}
			} else if(string2.Length == 0 ||
				  offset2 == string2.Length ||
				  length2 == 0) {
				return(1);
			}

			if(offset1 < 0 || length1 < 0 ||
			   offset2 < 0 || length2 < 0) {
				throw new ArgumentOutOfRangeException ("Offsets and lengths must not be less than zero");
			}
			
			if(offset1 > string1.Length) {
				throw new ArgumentOutOfRangeException ("Offset1 is greater than or equal to the length of string1");
			}
			
			if(offset2 > string2.Length) {
				throw new ArgumentOutOfRangeException ("Offset2 is greater than or equal to the length of string2");
			}
			
			if(length1 > string1.Length-offset1) {
				throw new ArgumentOutOfRangeException ("Length1 is greater than the number of characters from offset1 to the end of string1");
			}
			
			if(length2 > string2.Length-offset2) {
				throw new ArgumentOutOfRangeException ("Length2 is greater than the number of characters from offset2 to the end of string2");
			}
			
			return(internal_compare (string1, offset1, length1,
						 string2, offset2, length2,
						 options));
		}

		public override bool Equals(object value)
		{
			CompareInfo other=value as CompareInfo;
			if(other==null) {
				return(false);
			}
			
			return(other.culture==culture);
		}

		public static CompareInfo GetCompareInfo(int culture)
		{
			return(new CultureInfo (culture).CompareInfo);
		}

		public static CompareInfo GetCompareInfo(string name)
		{
			if(name == null) {
				throw new ArgumentNullException("name");
			}
			return(new CultureInfo (name).CompareInfo);
		}

		public static CompareInfo GetCompareInfo(int culture,
							 Assembly assembly)
		{
			/* The assembly parameter is supposedly there
			 * to allow some sort of compare algorithm
			 * versioning.
			 */
			if(assembly == null) {
				throw new ArgumentNullException("assembly");
			}
			if(assembly!=typeof (Object).Module.Assembly) {
				throw new ArgumentException ("Assembly is an invalid type");
			}
			return(GetCompareInfo (culture));
		}

		public static CompareInfo GetCompareInfo(string name,
							 Assembly assembly)
		{
			/* The assembly parameter is supposedly there
			 * to allow some sort of compare algorithm
			 * versioning.
			 */
			if(name == null) {
				throw new ArgumentNullException("name");
			}
			if(assembly == null) {
				throw new ArgumentNullException("assembly");
			}
			if(assembly!=typeof (Object).Module.Assembly) {
				throw new ArgumentException ("Assembly is an invalid type");
			}
			return(GetCompareInfo (name));
		}

		public override int GetHashCode()
		{
			return(LCID);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void assign_sortkey (object key, string source,
						    CompareOptions options);
		
		public virtual SortKey GetSortKey(string source)
		{
			return(GetSortKey (source, CompareOptions.None));
		}

		public virtual SortKey GetSortKey(string source,
						  CompareOptions options)
		{
			SortKey key=new SortKey (culture, source, options);

			/* Need to do the icall here instead of in the
			 * SortKey constructor, as we need access to
			 * this instance's collator.
			 */
			assign_sortkey (key, source, options);
			
			return(key);
		}

		public virtual int IndexOf (string source, char value)
		{
			return(IndexOf (source, value, 0, source.Length,
					CompareOptions.None));
		}

		public virtual int IndexOf (string source, string value)
		{
			return(IndexOf (source, value, 0, source.Length,
					CompareOptions.None));
		}

		public virtual int IndexOf (string source, char value,
					    CompareOptions options)
		{
			return(IndexOf (source, value, 0, source.Length,
					options));
		}

		public virtual int IndexOf (string source, char value,
					    int startIndex)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex,
					CompareOptions.None));
		}
		
		public virtual int IndexOf (string source, string value,
					    CompareOptions options)
		{
			return(IndexOf (source, value, 0, source.Length,
					options));
		}

		public virtual int IndexOf (string source, string value,
					    int startIndex)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex,
					CompareOptions.None));
		}

		public virtual int IndexOf (string source, char value,
					    int startIndex,
					    CompareOptions options)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex, options));
		}

		public virtual int IndexOf (string source, char value,
					    int startIndex, int count)
		{
			return IndexOf (source, value, startIndex, count,
					CompareOptions.None);
		}

		public virtual int IndexOf (string source, string value,
					    int startIndex,
					    CompareOptions options)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex, options));
		}

		public virtual int IndexOf (string source, string value,
					    int startIndex, int count)
		{
			return(IndexOf (source, value, startIndex, count,
					CompareOptions.None));
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_index (string source, int sindex,
						   int count, char value,
						   CompareOptions options,
						   bool first);
		
		public virtual int IndexOf (string source, char value,
					    int startIndex, int count,
					    CompareOptions options)
		{
			if(source==null) {
				throw new ArgumentNullException ("source");
			}
			if(startIndex<0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count<0 || (source.Length - startIndex) < count) {
				throw new ArgumentOutOfRangeException ("count");
			}
			if((options & CompareOptions.StringSort)!=0) {
				throw new ArgumentException ("StringSort is not a valid CompareOption for this method");
			}
			
			if(count==0) {
				return(-1);
			}

			if((options & CompareOptions.Ordinal)!=0) {
				for(int pos=startIndex;
				    pos < startIndex + count;
				    pos++) {
					if(source[pos]==value) {
						return(pos);
					}
				}
				return(-1);
			} else {
				return (internal_index (source, startIndex,
							count, value, options,
							true));
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_index (string source, int sindex,
						   int count, string value,
						   CompareOptions options,
						   bool first);
		
		public virtual int IndexOf (string source, string value,
					    int startIndex, int count,
					    CompareOptions options)
		{
			if(source==null) {
				throw new ArgumentNullException ("source");
			}
			if(value==null) {
				throw new ArgumentNullException ("value");
			}
			if(startIndex<0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count<0 || (source.Length - startIndex) < count) {
				throw new ArgumentOutOfRangeException ("count");
			}
			if(count==0) {
				return(-1);
			}

			return (internal_index (source, startIndex, count,
						value, options, true));
		}

		public virtual bool IsPrefix(string source, string prefix)
		{
			return(IsPrefix (source, prefix, CompareOptions.None));
		}

		public virtual bool IsPrefix(string source, string prefix,
					     CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(prefix == null) {
				throw new ArgumentNullException("prefix");
			}

			if(source.Length < prefix.Length) {
				return(false);
			} else {
				return(Compare (source, 0, prefix.Length,
						prefix, 0, prefix.Length,
						options)==0);
			}
		}

		public virtual bool IsSuffix(string source, string suffix)
		{
			return(IsSuffix (source, suffix, CompareOptions.None));
		}

		public virtual bool IsSuffix(string source, string suffix,
					     CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(suffix == null) {
				throw new ArgumentNullException("suffix");
			}

			if(source.Length < suffix.Length) {
				return(false);
			} else {
				return(Compare (source,
						source.Length - suffix.Length,
						suffix.Length, suffix, 0,
						suffix.Length, options)==0);
			}
		}

		public virtual int LastIndexOf(string source, char value)
		{
			return(LastIndexOf (source, value, 0, source.Length,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, string value)
		{
			return(LastIndexOf (source, value, 0, source.Length,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, char value,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, 0, source.Length,
					    options));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex)
		{
			return(LastIndexOf (source, value, startIndex,
					    source.Length - startIndex,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, string value,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, 0, source.Length,
					    options));
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex)
		{
			return(LastIndexOf (source, value, startIndex,
					    source.Length - startIndex,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, startIndex,
					    source.Length - startIndex,
					    options));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex, int count)
		{
			return(LastIndexOf (source, value, startIndex, count,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, startIndex,
					    source.Length - startIndex,
					    options));
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex, int count)
		{
			return(LastIndexOf (source, value, startIndex, count,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex, int count,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(startIndex < 0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count < 0 || (startIndex - count) < -1) {
				throw new ArgumentOutOfRangeException("count");
			}
			if((options & CompareOptions.StringSort)!=0) {
				throw new ArgumentException ("StringSort is not a valid CompareOption for this method");
			}
			
			if(count==0) {
				return(-1);
			}

			if((options & CompareOptions.Ordinal)!=0) {
				for(int pos=startIndex;
				    pos > startIndex - count;
				    pos--) {
					if(source[pos]==value) {
						return(pos);
					}
				}
				return(-1);
			} else {
				return (internal_index (source, startIndex,
							count, value, options,
							false));
			}
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex, int count,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(value == null) {
				throw new ArgumentNullException("value");
			}
			if(startIndex < 0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count < 0 || (startIndex - count) < -1) {
				throw new ArgumentOutOfRangeException("count");
			}
			if(count == 0) {
				return(-1);
			}

			int valuelen=value.Length;
			if(valuelen==0) {
				return(0);
			}

			return(internal_index (source, startIndex, count,
					       value, options, false));
		}

		public override string ToString()
		{
			return("CompareInfo - "+culture);
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
			/* This will build the ICU collator, and store
			 * the pointer in ICU_collator
			 */
			try {
				this.construct_compareinfo (icu_name);
			} catch {
				ICU_collator=IntPtr.Zero;
			}
		}

		/* LAMESPEC: not mentioned in the spec, but corcompare
		 * shows it.  Some documentation about what it does
		 * would be nice.
		 */
		public int LCID
		{
			get {
				return(culture);
			}
		}
	}
}
