//
// System.Collections.CaseInsensitiveHashCodeProvider
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Collections;
using System.Globalization;

namespace System.Collections {

	[Serializable]
	public class CaseInsensitiveHashCodeProvider : IHashCodeProvider {

		private static CaseInsensitiveHashCodeProvider singleton;


		// Class constructor

		static CaseInsensitiveHashCodeProvider ()
		{
			singleton=new CaseInsensitiveHashCodeProvider ();
		}



		// Public instance constructor

		public CaseInsensitiveHashCodeProvider ()
		{
		}

		[MonoTODO]
		public CaseInsensitiveHashCodeProvider (CultureInfo culture)
		{
			throw new NotImplementedException ();
		}



		//
		// Public static properties
		//

		public static CaseInsensitiveHashCodeProvider Default {
			get {
				return singleton;
			}
		}


		//
		// Instance methods
		//

		//
		// IHashCodeProvider
		//

		public int GetHashCode (object obj)
		{
			if (obj == null) {
				throw new ArgumentNullException ("obj is null");
			}

			string str = obj as string;

			if (str == null)
				return obj.GetHashCode ();

			int h = 0;
			char c;

			int length = str.Length;
			for (int i = 0;i<length;i++) {
				c = Char.ToLower (str [i]);
				h = h * 31 + c;
			}

			return h;
		}

	} // CaseInsensitiveHashCodeProvider
}

