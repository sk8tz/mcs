// AssemblyLoader.cs
// John Barnette (jbarn@httcb.net)
// Adam Treat    (manyoso@yahoo.com)
//
// Copyright (c) 2002 John Barnette
// Copyright (c) 2002 Adam Treat
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
using System.IO;
using System.Reflection;

namespace Mono.Doc.Core
{
	public class AssemblyLoader
	{
		// cannot instantiate this class
		private AssemblyLoader()
		{
		}

		public static Assembly Load(string fileName)
		{
			// from Adam's code in mcs/doctools/docstub.cs
			if (!File.Exists(fileName))
			{
				throw new ApplicationException("Cannot find assembly file: " + fileName);
			}

			FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, (int)fs.Length);
			fs.Close();

			return Assembly.Load(buffer);
		}
	}
}
