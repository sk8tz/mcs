//
// System.Data.SqlXml.DBObjectException
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

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

#if NET_2_0

using System;
using System.Runtime.Serialization;

namespace System.Data.SqlXml {
        public class DBObjectException : Exception
        {
		#region Constructors

		public DBObjectException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public DBObjectException ()
			: base ("A Database Object Exception has occurred.")
		{
		}

		public DBObjectException (string res)
			: base (res)
		{
		}

		public DBObjectException (string resource, Exception exception)
			: base (resource, exception)
		{
		}

		#endregion // Constructors

		#region Properties
	
		[MonoTODO]
		public override string Message {
			get { throw new NotImplementedException(); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException();
		}

		#endregion // Methods
        }
}

#endif // NET_2_0
