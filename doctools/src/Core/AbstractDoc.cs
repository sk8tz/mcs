// AbstractDoc.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections.Specialized;

namespace Mono.Doc.Core
{
	public abstract class AbstractDoc
	{
		protected string           name    = null;
		protected string           summary = null;
		protected string           remarks = null;
		protected string          language = null;
		protected StringCollection seeAlso = null;

		protected AbstractDoc()
		{
			seeAlso = new StringCollection();
		}

		public string Summary
		{
			get { return summary;  }
			set { summary = value; }
		}

		public string Remarks
		{
			get { return remarks;  }
			set { remarks = value; }
		}

		public string Name
		{
			get { return name;  }
			set { name = value; }
		}

		public StringCollection SeeAlso
		{
			get { return seeAlso; }
		}

		public string Language
		{
			get { return language;  }
			set { language = value; }
		}
	}
}
