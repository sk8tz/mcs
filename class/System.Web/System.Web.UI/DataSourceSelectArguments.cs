﻿//
// System.Web.UI.DataSourceSelectArguments.cs
//
// Author:
//   Sanjay Gupta <gsanjay@novell.com>
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Web.UI
{
	public sealed class DataSourceSelectArguments
	{
		string sortExpression = string.Empty;
		int startingRowIndex = 0;
		int maxRows = 0;
		bool isEmpty;
		bool getTotalRowCount = false;
		int totalRowCount = -1;
		DataSourceCapabilities dsc = DataSourceCapabilities.None;

		public static readonly DataSourceSelectArguments Empty = new DataSourceSelectArguments ();

		public DataSourceSelectArguments ()
		{
			this.isEmpty = true;
		}

		public DataSourceSelectArguments (string sortExpression)
		{
			this.sortExpression = sortExpression;
			this.isEmpty = false;
		}

		public DataSourceSelectArguments (int startingRowIndex, int maxRows)
		{
			this.startingRowIndex = startingRowIndex;
			this.maxRows = maxRows;
			this.isEmpty = false;
		}

		public DataSourceSelectArguments (string sortExpression, int startingRowIndex, int maxRows)
		{
			this.sortExpression = sortExpression; 
			this.startingRowIndex = startingRowIndex;
			this.maxRows = maxRows;
			this.isEmpty = false;
		}

		public void AddSupportedCapabilities (DataSourceCapabilities srcCapabilities)
		{
			this.dsc = this.dsc | srcCapabilities;
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			if (!(obj is DataSourceSelectArguments))
				return false;
			return false;
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return sortExpression.GetHashCode ();
		}

		public void RaiseUnsupportedCapabilitiesError (DataSourceView view)
		{
			view.RaiseUnsupportedCapabilityError (this.dsc);
		}

		public bool IsEmpty {
			get { return this.isEmpty; }
		}

		public int MaximumRows {
			get { return this.MaximumRows; }
			set {
				this.maxRows = value;
				this.isEmpty = false;
			}
		}

		public bool RetrieveTotalRowCount  {
			get { return this.getTotalRowCount; }
			set { this.getTotalRowCount = value; }
		}

		public string SortExpression {
			get { return this.sortExpression; }
			set {
				this.sortExpression = value;
				this.isEmpty = false;
			}
		}

		public int StartRowIndex {
			get { return this.startingRowIndex; }
			set {
				this.startingRowIndex = value;
				this.isEmpty = false;
			}
		}

		public int TotalRowCount {
			get { return this.totalRowCount; }
			set {
				this.totalRowCount = value;
				this.isEmpty = false;
			}
		}
	}
}
#endif
