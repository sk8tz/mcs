//
// System.Globalization.NumberFormatInfo.cs
//
// Author:
//   Derek Holden (dholden@draper.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// NumberFormatInfo. One can only assume it is the class gottten
// back from a GetFormat() method from an IFormatProvider / 
// IFormattable implementer. There are some discrepencies with the
// ECMA spec and the SDK docs, surprisingly. See my conversation
// with myself on it at:
// http://lists.ximian.com/archives/public/mono-list/2001-July/000794.html
// 
// Other than that this is totally ECMA compliant.
//

namespace System.Globalization {

	public sealed class NumberFormatInfo : ICloneable, IFormatProvider {
		private bool readOnly;
		
		// Currency Related Format Info
		private int currencyDecimalDigits;
		private string currencyDecimalSeparator;
		private string currencyGroupSeparator;
		private int[] currencyGroupSizes;
		private int currencyNegativePattern;
		private int currencyPositivePattern;
		private string currencySymbol;

		private string naNSymbol;
		private string negativeInfinitySymbol;
		private string negativeSign;

		// Number Related Format Info
		private int numberDecimalDigits;
		private string numberDecimalSeparator;
		private string numberGroupSeparator;
		private int[] numberGroupSizes;
		private int numberNegativePattern;

		// Percent Related Format Info
		private int percentDecimalDigits;
		private string percentDecimalSeparator;
		private string percentGroupSeparator;
		private int[] percentGroupSizes;
		private int percentNegativePattern;
		private int percentPositivePattern;
		private string percentSymbol;

		private string perMilleSymbol;
		private string positiveInfinitySymbol;
		private string positiveSign;

		public NumberFormatInfo ()
		{
			readOnly = false;

			// Currency Related Format Info
			currencyDecimalDigits =       2;
			currencyDecimalSeparator =    ".";
			currencyGroupSeparator =      ",";
			currencyGroupSizes =          new int[1] { 3 };
			currencyNegativePattern =     0;
			currencyPositivePattern =     0;
			currencySymbol =              "$";

			naNSymbol =                   "NaN";
			negativeInfinitySymbol =      "-Infinity";
			negativeSign =                "-";

			// Number Related Format Info
			numberDecimalDigits =         2;
			numberDecimalSeparator =      ".";
			numberGroupSeparator =        ",";
			numberGroupSizes =            new int[1] { 3 };
			numberNegativePattern =       0;

			// Percent Related Format Info
			percentDecimalDigits =        2;
			percentDecimalSeparator =     ".";
			percentGroupSeparator =       ",";
			percentGroupSizes =           new int[1] { 3 };
			percentNegativePattern =      0;
			percentPositivePattern =      0;
			percentSymbol=                "%";
			
			perMilleSymbol =              "\u2030";
			positiveInfinitySymbol =      "Infinity";
			positiveSign =                "+";
		}
				
		// =========== Currency Format Properties =========== //

		public int CurrencyDecimalDigits {
			get {
				return currencyDecimalDigits;
			}
			
			set {
				if (value < 0 || value > 99) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 99");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				currencyDecimalDigits = value;
			}
		}

		public string CurrencyDecimalSeparator {
			get {
				return currencyDecimalSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				currencyDecimalSeparator = value;
			}
		}


		public string CurrencyGroupSeparator {
			get {
				return currencyGroupSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				currencyGroupSeparator = value;
			}
		}

		public int[] CurrencyGroupSizes {
			get {
				return currencyGroupSizes;
			}
			
			set {
				if (value == null || value.Length == 0) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				// All elements except last need to be in range 1 - 9, last can be 0.
				int last = value.Length - 1;

				for (int i = 0; i < last; i++)
					if (value[i] < 1 || value[i] > 9)
						throw new ArgumentOutOfRangeException
						("One of the elements in the array specified is not between 1 and 9");

				if (value[last] < 0 || value[last] > 9)
					throw new ArgumentOutOfRangeException
					("Last element in the array specified is not between 0 and 9");
				
				currencyGroupSizes = (int[]) value.Clone();
			}
		}

		public int CurrencyNegativePattern {
			get {
				// See ECMA NumberFormatInfo page 8
				return currencyNegativePattern;
			}
			
			set {
				if (value < 0 || value > 15) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 15");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				currencyNegativePattern = value;
			}
		}

		public int CurrencyPostivePattern {
			get {
				// See ECMA NumberFormatInfo page 11 
				return currencyPositivePattern;
			}
			
			set {
				if (value < 0 || value > 3) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 3");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				currencyPositivePattern = value;
			}
		}

		public string CurrencySymbol {
			get {
				return currencySymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				currencySymbol = value;
			}
		}

		// =========== Static Read-Only Properties =========== //

		public static NumberFormatInfo CurrentInfo {
			get {
				// This should be culture specific
				NumberFormatInfo nfi = new NumberFormatInfo ();
				nfi.readOnly = true;
				return nfi;
			}		       
		}

		public static NumberFormatInfo InvariantInfo {
			get {
				// This uses invariant info, which is same as in the constructor
				NumberFormatInfo nfi = new NumberFormatInfo ();
				nfi.readOnly = true;
				return nfi;
			}		       
		}

		public bool IsReadOnly {
			get {
				return readOnly;
			}
		}



		public string NaNSymbol {
			get {
				return naNSymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				naNSymbol = value;
			}
		}
		
		public string NegativeInfinitySymbol {
			get {
				return negativeInfinitySymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				negativeInfinitySymbol = value;
			}
		}

		public string NegativeSign {
			get {
				return negativeSign;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				negativeSign = value;
			}
		}
		
		// =========== Number Format Properties =========== //

		public int NumberDecimalDigits {
			get {
				return numberDecimalDigits;
			}
			
			set {
				if (value < 0 || value > 99) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 99");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				numberDecimalDigits = value;
			}
		}		

		public string NumberDecimalSeparator {
			get {
				return numberDecimalSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				numberDecimalSeparator = value;
			}
		}


		public string NumberGroupSeparator {
			get {
				return numberGroupSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				numberGroupSeparator = value;
			}
		}

		public int[] NumberGroupSizes {
			get {
				return numberGroupSizes;
			}
			
			set {
				if (value == null || value.Length == 0) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				// All elements except last need to be in range 1 - 9, last can be 0.
				int last = value.Length - 1;

				for (int i = 0; i < last; i++)
					if (value[i] < 1 || value[i] > 9)
						throw new ArgumentOutOfRangeException
						("One of the elements in the array specified is not between 1 and 9");

				if (value[last] < 0 || value[last] > 9)
					throw new ArgumentOutOfRangeException
					("Last element in the array specified is not between 0 and 9");
				
				numberGroupSizes = (int[]) value.Clone();
			}
		}

		public int NumberNegativePattern {
			get {
				// See ECMA NumberFormatInfo page 27
				return numberNegativePattern;
			}
			
			set {
				if (value < 0 || value > 4) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 15");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				numberNegativePattern = value;
			}
		}

		// =========== Percent Format Properties =========== //

		public int PercentDecimalDigits {
			get {
				return percentDecimalDigits;
			}
			
			set {
				if (value < 0 || value > 99) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 99");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				percentDecimalDigits = value;
			}
		}

		public string PercentDecimalSeparator {
			get {
				return percentDecimalSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				percentDecimalSeparator = value;
			}
		}


		public string PercentGroupSeparator {
			get {
				return percentGroupSeparator;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				percentGroupSeparator = value;
			}
		}

		public int[] PercentGroupSizes {
			get {
				return percentGroupSizes;
			}
			
			set {
				if (value == null || value.Length == 0) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				// All elements except last need to be in range 1 - 9, last can be 0.
				int last = value.Length - 1;

				for (int i = 0; i < last; i++)
					if (value[i] < 1 || value[i] > 9)
						throw new ArgumentOutOfRangeException
						("One of the elements in the array specified is not between 1 and 9");

				if (value[last] < 0 || value[last] > 9)
					throw new ArgumentOutOfRangeException
					("Last element in the array specified is not between 0 and 9");
				
				percentGroupSizes = (int[]) value.Clone();
			}
		}

		public int PercentNegativePattern {
			get {
				// See ECMA NumberFormatInfo page 8
				return percentNegativePattern;
			}
			
			set {
				if (value < 0 || value > 2) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 15");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				percentNegativePattern = value;
			}
		}

		public int PercentPostivePattern {
			get {
				// See ECMA NumberFormatInfo page 11 
				return percentPositivePattern;
			}
			
			set {
				if (value < 0 || value > 2) 
					throw new ArgumentOutOfRangeException
					("The value specified for the property is less than 0 or greater than 3");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");

				percentPositivePattern = value;
			}
		}

		public string PercentSymbol {
			get {
				return percentSymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				percentSymbol = value;
			}
		}

		public string PerMilleSymbol {
			get {
				return perMilleSymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
				
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");
				
				perMilleSymbol = value;
			}
		}

		public string PositiveInfinitySymbol {
			get {
				return positiveInfinitySymbol;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				positiveInfinitySymbol = value;
			}
		}

		public string PositiveSign {
			get {
				return positiveSign;
			}
			
			set {
				if (value == null) 
					throw new ArgumentNullException
					("The value specified for the property is a null reference");
			
				if (readOnly)
					throw new InvalidOperationException
					("The current instance is read-only and a set operation was attempted");	
				
				positiveSign = value;
			}
		}

		public object GetFormat (Type formatType) 
		{
			// LAMESPEC: ECMA says we implement IFormatProvider, but doesn't define this
			//
			// From the .NET Framework SDK
			//
			// Parameters: formatType The Type of the formatting service required. 
			//
			// Return Value: The current instance of the NumberFormatInfo class, if formatType 
			// is the same as the type of the current instance; otherwise, a null reference
			//
			// Remarks: This method is invoked by the Format(String, IFormatProvider) method 			
			// supported by the base data types when this instance is passed as the 
			// IFormatProvider parameter. It implements IFormatProvider.GetFormat.

			if (formatType.Equals(this)) // LAMESPEC: Should this be IsInstanceOfType?
				return this;
			else return null;
		}
		
		public object Clone () 
		{
			NumberFormatInfo clone = new NumberFormatInfo ();

			clone.readOnly = this.readOnly;

			clone.currencyDecimalDigits = this.currencyDecimalDigits; 
			clone.currencyDecimalSeparator = String.Copy (this.currencyDecimalSeparator);
			clone.currencyGroupSeparator = String.Copy (this.currencyGroupSeparator);
			clone.currencyGroupSizes = (int[]) this.currencyGroupSizes.Clone();
			clone.currencyNegativePattern = this.currencyNegativePattern;
			clone.currencyPositivePattern = this.currencyPositivePattern;
			clone.currencySymbol = String.Copy (this.currencySymbol);

			clone.naNSymbol = String.Copy (this.naNSymbol);
			clone.negativeInfinitySymbol = String.Copy (this.negativeInfinitySymbol);
			clone.negativeSign = String.Copy (this.negativeSign);

			clone.numberDecimalDigits = this.numberDecimalDigits;
			clone.numberDecimalSeparator = String.Copy (this.numberDecimalSeparator);
			clone.numberGroupSeparator = String.Copy (this.numberGroupSeparator);
			clone.numberGroupSizes = (int[]) this.numberGroupSizes.Clone();
			clone.numberNegativePattern = this.numberNegativePattern;

			clone.percentDecimalDigits = this.percentDecimalDigits;
			clone.percentDecimalSeparator = String.Copy (this.percentDecimalSeparator);
			clone.percentGroupSeparator = String.Copy (this.percentGroupSeparator); 
			clone.percentGroupSizes = (int []) this.percentGroupSizes.Clone();
			clone.percentNegativePattern = this.percentNegativePattern;
			clone.percentPositivePattern = this.percentPositivePattern;
			clone.percentSymbol = String.Copy (this.percentSymbol);
			
			clone.perMilleSymbol = String.Copy (this.perMilleSymbol);
			clone.positiveInfinitySymbol = String.Copy (this.positiveInfinitySymbol);
			clone.positiveSign = String.Copy (this.positiveSign);
			
			return clone;
		}

		public static NumberFormatInfo ReadOnly (NumberFormatInfo nfi)
		{
			NumberFormatInfo copy = (NumberFormatInfo)nfi.Clone();
			copy.readOnly = true;
			return copy;
		}			
	}
}
