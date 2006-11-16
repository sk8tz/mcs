//
// RegionInfo.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible(true)]
#endif
	[Serializable]
	public class RegionInfo
	{
		static RegionInfo currentRegion;

		// This property is not synchronized with CurrentCulture, so
		// we need to use bootstrap CurrentCulture LCID.
		public static RegionInfo CurrentRegion {
			get {
				if (currentRegion == null) {
					// make sure to fill BootstrapCultureID.
					CultureInfo ci = CultureInfo.CurrentCulture;
					// If current culture is invariant then region is not available.
					if (ci == null || CultureInfo.BootstrapCultureID == 0x7F)
						return null;
					currentRegion = new RegionInfo (CultureInfo.BootstrapCultureID);
				}
				return currentRegion;
			}
		}

		int regionId;
		string iso2Name;
		string iso3Name;
		string win3Name;
		string englishName;
		string currencySymbol;
		string isoCurrencySymbol;
		string currencyEnglishName;

		public RegionInfo (int lcid)
		{
			if (!construct_internal_region_from_lcid (lcid))
				throw new ArgumentException (
					String.Format ("Region ID {0} (0x{0:X4}) is not a " +
							"supported region.", lcid), "lcid");
		}

		public RegionInfo (string name)
		{
			if (name == null)
				throw new ArgumentNullException ();

			if (!construct_internal_region_from_name (name.ToUpperInvariant ()))
				throw new ArgumentException ("Region name " + name +
						" is not supported.", "name");
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool construct_internal_region_from_lcid (int lcid);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool construct_internal_region_from_name (string name);

#if NET_2_0
		[System.Runtime.InteropServices.ComVisible(false)]
		public virtual string CurrencyEnglishName {
			get { return currencyEnglishName; }
		}
#endif

		public virtual string CurrencySymbol {
			get { return currencySymbol; }
		}

		[MonoTODO ("DisplayName currently only returns the EnglishName")]
		public virtual string DisplayName {
			get { return englishName; }
		}

		public virtual string EnglishName {
			get { return englishName; }
		}

		public virtual bool IsMetric {
			get {
				switch (iso2Name) {
				case "US":
				case "UK":
					return false;
				default:
					return true;
				}
			}
		}

		public virtual string ISOCurrencySymbol {
			get { return isoCurrencySymbol; }
		}

#if NET_2_0
		[MonoTODO]
		[System.Runtime.InteropServices.ComVisible(false)]
		public virtual string NativeName {
			get { return DisplayName; }
		}

		[MonoTODO ("Not implemented")]
		public virtual string CurrencyNativeName {
			get { throw new NotImplementedException (); }
		}
#endif

		public virtual string Name {
			get { return iso2Name; }
		}

		public virtual string ThreeLetterISORegionName {
			get { return iso3Name; }
		}

		public virtual string ThreeLetterWindowsRegionName {
			get { return win3Name; }
		}
		
		public virtual string TwoLetterISORegionName {
			get { return iso2Name; }
		}

		//
		// methods

#if NET_2_0
#else
		public override bool Equals (object value)
		{
			RegionInfo other = value as RegionInfo;
			return other != null && regionId == other.regionId;
		}

		public override int GetHashCode () {
			return regionId;
		}
#endif

		public override string ToString ()
		{
			return Name;
		}
	}
}
