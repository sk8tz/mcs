//
// System.Windows.Forms.DataObject
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//
// (C) Ximian, Inc., 2002
//
using System.Runtime.InteropServices;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template. Nothing is implemented yet.
	//	Implements a basic data transfer mechanism.
	// </summary>
	[ClassInterface(ClassInterfaceType.None)]
	public class DataObject : IDataObject {

		//
		//  --- Constructors/Destructors
		//
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public DataObject() : base()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public DataObject(object data) : base()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public DataObject(string format, object data) : base()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual object GetData(string format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual object GetData(Type format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual object GetData(string format, bool autoConvert)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual bool GetDataPresent(string format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual bool GetDataPresent(Type format)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual bool GetDataPresent(string format, bool autoConvert)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual string[] GetFormats()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual string[] GetFormats(bool autoConvert)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(object data)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(string format, object data)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(Type format, object data)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.None)]
		public virtual void SetData(string format, bool autoConvert, object data)
		{
			throw new NotImplementedException ();
		}
	}
}
