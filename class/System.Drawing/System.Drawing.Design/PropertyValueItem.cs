// System.Drawing.Design.PropertyValueItem.cs
// 
// Author:
//     Alejandro S�nchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro S�nchez Acosta
// 

using System.Drawing;
using System.Drawing.Imaging;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace System.Drawing.Design
{
	public class PropertyValueUIItem
	{
		[MonoTODO]
		public PropertyValueUIItem (Image uiItemImage,
				      PropertyValueUIItemInvokeHandler handler,
				      string tooltip)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Image Image 
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual PropertyValueUIItemInvokeHandler InvokeHandler
		{
			get
			{
				throw new NotImplementedException ();
			}

			set
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual string ToolTip 
		{
			get
			{
				throw new NotImplementedException ();
			}

			set
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual void Reset()
		{
			throw new NotImplementedException ();
		}
	}
}
