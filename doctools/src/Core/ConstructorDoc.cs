// ConstructorDoc.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
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
using System.Reflection;
using System.Xml.Serialization;

namespace Mono.Doc.Core
{
	[XmlType(TypeName = "constructor")]
	public class ConstructorDoc : AbstractDoc
	{
		private ValueConstrainedArrayList exceptions = new ValueConstrainedArrayList(typeof(ExceptionDoc));
		private ValueConstrainedArrayList parameters = new ValueConstrainedArrayList(typeof(ParameterDoc));

		public ConstructorDoc(string name) : base(name)
		{
		}

		public ConstructorDoc() : this(string.Empty)
		{
		}

		public ConstructorDoc(ConstructorInfo cons, AssemblyLoader loader) : base(cons, loader)
		{
			// parameters
			foreach (ParameterInfo param in cons.GetParameters())
			{
				this.Parameters.Add(new ParameterDoc(param.Name, AbstractDoc.TODO));
			}
		}

		[XmlElement(ElementName = "exception", Type = typeof(ExceptionDoc))]
		public ValueConstrainedArrayList Exceptions
		{
			get { return this.exceptions; }
		}

		[XmlElement(ElementName = "param", Type = typeof(ParameterDoc))]
		public ValueConstrainedArrayList Parameters
		{
			get { return this.parameters; }
		}
	}
}
