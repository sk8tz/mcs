// 
// System.Web.Services.Discovery.ExcludePathInfo.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//
// Copyright (C) Dave Bettin, 2002
//

using System.Xml.Serialization;

namespace System.Web.Services.Discovery {
	public sealed class ExcludePathInfo {
		
		#region Fields
		
		private string path;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ExcludePathInfo () 
		{
			throw new NotImplementedException ();
		}
		
		public ExcludePathInfo (string path) : this() 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties
		
		[XmlAttribute("path")]
		public string Path {
			get { return path; }
			set { path = value; }
		}
		
		#endregion // Properties
	}
}
