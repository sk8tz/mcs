//
// System.Data.ProviderBase.DbParameterBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbParameterBase : DbParameter
	{
		#region Constructors
	
		[MonoTODO]
		protected DbParameterBase ()
		{
		}

		[MonoTODO]
		protected DbParameterBase (DbParameterBase source)
		{
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected object CoercedValue {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override ParameterDirection Direction {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsNullable {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int Offset {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string ParameterName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override byte Precision {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override byte Scale {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int Size {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string SourceColumn {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override DataRowVersion SourceVersion {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override object Value {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void CopyTo (DbParameter destination)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void PropertyChanging ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void ResetCoercedValue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void ResetScale ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetCoercedValue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool ShouldSerializePrecision ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool ShouldSerializeScale ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected bool ShouldSerializeSize ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual byte ValuePrecision (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual byte ValueScale (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int ValueSize (object value)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
