//
// System.ComponentModel.IComponent.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design.Serialization;

namespace System.ComponentModel
{
	[ComVisible (true),
		TypeConverter (typeof (System.ComponentModel.ComponentConverter)),
		Designer ("System.Windows.Forms.Design.ComponentDocumentDesigner, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (System.ComponentModel.Design.IRootDesigner)),
		RootDesignerSerializer ("System.ComponentModel.Design.Serialization.RootCodeDomSerializer, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true)]
	public interface IComponent : IDisposable
	{
		ISite Site { get; set; }
		event EventHandler Disposed;
	}
}

