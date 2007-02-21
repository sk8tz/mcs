//
// System.Data.Odbc.OdbcMetaDataColumnNames
//
// Author: Nidhi Rawal (rawalnidhi_rawal@yahoo.com)
//  
//  
//  
//
// Copyright (C) Brian Ritchie, 2007
// Copyright (C) Daniel Morgan, 2007
//

//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;

namespace System.Data.Odbc
{
	public static class OdbcMetaDataColumnNames
	{
		#region Fields

		public static readonly string BooleanFalseLiteral;
		public static readonly string BooleanTrueLiteral;
		public static readonly string SQLType;

		#endregion

		#region Methods
        
		public static bool ReferenceEquals(Object o1, Object o2)
		{
			return o1 == o2;
		}

		#endregion
	}
}
