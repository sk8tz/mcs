//
// System.Resources.IResourceReader.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc. http://www.ximian.com
//

using System;
using System.Collections;

namespace System.Resources
{

	   public interface IResourceReader : IEnumerable, IDisposable
	   {
			 void Close();
			 new IDictionaryEnumerator GetEnumerator();
	   }
}
