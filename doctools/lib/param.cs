// param.cs - Mono Documentation Lib
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

using System;

namespace Mono.Util.MonoDoc.Lib {

	public class DocParam {

		string name;
		
		public DocParam ()
		{
		}

		public string Name
		{
			get {return name;}
			set {name = value;}
		}
	}
}
