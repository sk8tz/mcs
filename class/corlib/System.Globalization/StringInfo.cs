//
// System.Globalization.StringInfo.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2004 Novell, Inc.
//

using System.Collections;

namespace System.Globalization {

	[Serializable]
	public class StringInfo {
		public StringInfo()
		{
		}

		public static string GetNextTextElement(string str)
		{
			if(str == null || str.Length == 0) {
				throw new ArgumentNullException("string is null");
			}
			return(GetNextTextElement (str, 0));
		}

		public static string GetNextTextElement(string str, int index)
		{
			if(str == null) {
				throw new ArgumentNullException("string is null");
			}

			if(index < 0 || index >= str.Length) {
				throw new ArgumentOutOfRangeException ("Index is not valid");
			}

			/* Find the next base character, surrogate
			 * pair or combining character sequence
			 */

			char ch = str[index];
			UnicodeCategory cat = char.GetUnicodeCategory (ch);

			if (cat == UnicodeCategory.Surrogate) {
				/* Check that it's a high surrogate
				 * followed by a low surrogate
				 */
				if (ch >= 0xD800 && ch <= 0xDBFF) {
					if ((index + 1) < str.Length &&
					    str[index + 1] >= 0xDC00 &&
					    str[index + 1] <= 0xDFFF) {
						/* A valid surrogate pair */
						return(str.Substring (index, 2));
					} else {
						/* High surrogate on its own */
						return(new String (ch, 1));
					}
				} else {
					/* Low surrogate on its own */
					return(new String (ch, 1));
				}
			} else {
				/* Look for a base character, which
				 * may or may not be followed by a
				 * series of combining characters
				 */

				if (cat == UnicodeCategory.NonSpacingMark ||
				    cat == UnicodeCategory.SpacingCombiningMark ||
				    cat == UnicodeCategory.EnclosingMark) {
					/* Not a base character */
					return(new String (ch, 1));
				}
				
				int count = 1;

				while (index + count < str.Length) {
					cat = char.GetUnicodeCategory (str[index + count]);
					if (cat != UnicodeCategory.NonSpacingMark &&
					    cat != UnicodeCategory.SpacingCombiningMark &&
					    cat != UnicodeCategory.EnclosingMark) {
						/* Finished the sequence */
						break;
					}
					count++;
				}

				return(str.Substring (index, count));
			}
		}

		public static TextElementEnumerator GetTextElementEnumerator(string str)
		{
			if(str == null || str.Length == 0) {
				throw new ArgumentNullException("string is null");
			}
			return(new TextElementEnumerator (str, 0));
		}

		public static TextElementEnumerator GetTextElementEnumerator(string str, int index)
		{
			if(str == null) {
				throw new ArgumentNullException("string is null");
			}

			if(index < 0 || index >= str.Length) {
				throw new ArgumentOutOfRangeException ("Index is not valid");
			}
			
			return(new TextElementEnumerator (str, index));
		}
		
		public static int[] ParseCombiningCharacters(string str)
		{
			if(str == null) {
				throw new ArgumentNullException("string is null");
			}

			ArrayList indices = new ArrayList (str.Length);
			TextElementEnumerator tee = GetTextElementEnumerator (str);

			tee.Reset ();
			while(tee.MoveNext ()) {
				indices.Add (tee.ElementIndex);
			}

			return((int[])indices.ToArray (typeof (int)));
		}
	}
}
