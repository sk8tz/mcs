/**
 * Namespace: System.Web.UI.WebControls
 * Struct:    Unit
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  99%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Globalization;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public struct Unit
	{
		public static readonly Unit Empty = new Unit();
		
		private static int Min = -32768;
		private static int Max = +32767;
		
		private UnitType type;
		private double   val;
		
		public static Unit Parse(string s)
		{
			return new Unit(s);
		}
		
		public static Unit Parse(string s, CultureInfo culture)
		{
			return new Unit(s, culture);
		}
		
		public static Unit Percentage(double n)
		{
			return new Unit(n);
		}
		
		public static Unit Pixel(int n)
		{
			return new Unit(n);
		}
		
		public static Unit Point(int n)
		{
			return new Unit(n, UnitType.Point);
		}
		
		public static bool operator ==(Unit left, Unit right)
		{
			return (left.type == right.type && left.val == right.val);
		}
		
		public static bool operator !=(Unit left, Unit right)
		{
			return !(left == right);
		}
		
		public static implicit operator Unit(int n)
		{
			return new Unit(n);
		}
		
		public Unit(double value)
		{
			if(value < Min || value > Max)
			{
				return ArgumentOutOfRangeException();
			}
			val = value;
			type = UnitType.Pixel;
		}
		
		public Unit(int value)
		{
			if(value < Min || value > Max)
			{
				return ArgumentOutOfRangeException();
			}
			val = value;
			type = UnitType.Pixel;
		}
		
		public Unit(string value): this(value, CultureInfo.CurrentCulture)
		{
		}
		
		public Unit(double value, UnitType type)
		{
			if(value < Min || value > Max)
			{
				return ArgumentOutOfRangeException();
			}
			val = value;
			this.type = type;
		}
		
		public Unit(string value, CultureInfo culture): this(value, culture, UnitType.Pixel)
		{
		}
		
		internal Unit(string value, CultureInfo culture, UnitType defType)
		{
			if(value == null || value.Length == 0)
			{
				this.val = 0;
				this.type = UnitType.Pixel;
			}
			if(culture == null)
				culture = CultureInfo.CurrentCulture;
			string strVal = value.Trim().ToLower();
			char c;
			int start = -1;
			int current = 0;
			for(int i = 0; i < strVal.Length; i++)
			{
				c = strVal[i];
				if( (c >= '0' && c <= '9') || (c == '-' || c == '.' || c == ',') )
					start = i;
			}
			if(start == -1)
				throw new ArgumentException();
			if( (start + 1) < strVal.Length)
			{
				this.type = GetTypeFromString(strVal.Substring(start + 1).Trim());
			} else
			{
				this.type = defType;
			}
			try
			{
				if(type == UnitType.Pixel)
				{
					val = (double)((new In32Converter()).ConvertFromString(null, culture, strVal.Substring(0, start + 1)));
				} else
				{
					val = (double)((new SingleConverter()).ConvertFromString(null, culture, strVal.Substring(0, start + 1)));
				}
			} catch(Exception e)
			{
				throw new ArgumentOutOfRangeException();
			}
			if(val < Min || val > Max)
				throw new ArgumentOutOfRangeException();
		}
		
		private UnitType GetTypeFromString(string s)
		{
			if(s == null || s.Length == 0)
				return UnitType.Pixel;
			s = s.ToLower().Trim();
			string[] uTypes = {
				"px",
				"pt",
				"pc",
				"in",
				"mm",
				"cm",
				"%",
				"em",
				"ex"
			}
			int i = 0;
			foreach(string cType in uTypes)
			{
				if(s == uTypes[i])
					return (UnitType)(i + 1);
				i++;
			}
			return UnitType.Pixel;
		}
		
		private string GetStringFromPixel(UnitType ut)
		{
			string[] uTypes = {
				"px",
				"pt",
				"pc",
				"in",
				"mm",
				"cm",
				"%",
				"em",
				"ex"
			}
			if( !Enum.IsDefined(typeof(UnitType), ut) )
				return "px";
			return uTypes[ut - 1];
		}
		
		public bool IsEmpty
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		
		public UnitType Type
		{
			get
			{
				if(IsEmpty)
					return UnitType.Pixel;
				return type;
			}
		}
		
		public double Value
		{
			get
			{
				return val;
			}
		}
		
		public override bool Equals(object obj)
		{
			if(obj != null && obj is Unit)
			{
				Unit that = (Unit)obj;
				return ( this.type == that.type && this.val == that.val );
			}
			return false;
		}
		
		public override int GetHashCode()
		{
			return ( (type.GetHashCode() << 2) | (val.GetHashCode()) );
		}
		
		public override string ToString()
		{
			if(IsEmpty)
				return String.Empty;
			return ( val.ToString() + GetStringFromType(type) );
		}
		
		public override string ToString(CultureInfo culture)
		{
			if(IsEmpty)
				return String.Empty;
			return ( val.ToString(culture) + GetStringFromType(type) );
		}
	}
}
