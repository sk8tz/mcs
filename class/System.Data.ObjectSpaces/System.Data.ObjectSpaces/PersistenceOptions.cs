//
// System.Data.ObjectSpaces.PersistenceOptions.cs 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003-2004
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

namespace System.Data.ObjectSpaces {
        public class PersistenceOptions
        {
		#region Fields

		Depth depth;
		PersistenceErrorBehavior errorBehavior;
		static readonly PersistenceOptions DefaultPersistenceOptions = new PersistenceOptions ();

		#endregion // Fields

		#region Constructors

		public PersistenceOptions (Depth depth, PersistenceErrorBehavior errorBehavior)
		{
			this.depth = depth;
			this.errorBehavior = errorBehavior;
		}

		public PersistenceOptions (PersistenceErrorBehavior errorBehavior)
			: this (Depth.ObjectGraph, errorBehavior)
		{
		}

		public PersistenceOptions (Depth depth)
			: this (depth, PersistenceErrorBehavior.ThrowAtFirstError)
		{
		}

		public PersistenceOptions ()
			: this (Depth.ObjectGraph, PersistenceErrorBehavior.ThrowAtFirstError)
		{
		}

		#endregion // Constructors

		#region Properties

		public static PersistenceOptions Default {
			get { return DefaultPersistenceOptions; }
		}

		public Depth Depth {
			get { return depth; }
		}
		
		public PersistenceErrorBehavior ErrorBehavior {
			get { return errorBehavior; }
		}

		#endregion // Properties
        }
}

#endif
