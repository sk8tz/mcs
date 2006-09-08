//
// System.Web.UI.WebControls.ControlParameter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI.WebControls {

	[DefaultPropertyAttribute ("ControlID")]
	public class ControlParameter : Parameter {

		public ControlParameter () : base ()
		{
		}

		protected ControlParameter (ControlParameter original) : base (original)
		{
			this.ControlID = original.ControlID;
			this.PropertyName = original.PropertyName;
		}
		
		public ControlParameter (string name, string controlID) : base (name)
		{
			ControlID = controlID;
		}
		
		public ControlParameter (string name, string controlID, string propertyName) : base (name)
		{
			ControlID = controlID;
			PropertyName = propertyName;
		}
		
		public ControlParameter (string name, TypeCode type, string controlID, string propertyName) : base (name, type)
		{
			ControlID = controlID;
			PropertyName = propertyName;
		}
		
		protected override Parameter Clone ()
		{
			return new ControlParameter (this);
		}
		
		protected override object Evaluate (HttpContext ctx, Control control)
		{
			if (control == null) return null;
			if (control.Page == null) return null;
			
			Control c = null, namingContainer = control.NamingContainer; 
			
			while (namingContainer != null) {
				c = namingContainer.FindControl(ControlID);
				if (c != null)
					break;
				namingContainer = namingContainer.NamingContainer;
			}
			if (c == null) throw new HttpException ("Control '" + ControlID + "' not found.");
			
			PropertyInfo prop = c.GetType().GetProperty (PropertyName);
			if (prop == null) throw new HttpException ("Property '" + PropertyName + "' not found in type '" + c.GetType() + "'.");
			
			return prop.GetValue (c, null);
		}
		
		[WebCategoryAttribute ("Control")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		[TypeConverterAttribute (typeof (ControlIDConverter))]
		[DefaultValueAttribute ("")]
		[IDReferencePropertyAttribute (typeof(System.Web.UI.Control))]
		public string ControlID {
			get { return ViewState.GetString ("ControlID", ""); }
			set {
				if (ControlID != value) {
					ViewState ["ControlID"] = value;
					OnParameterChanged ();
				}
			}
		}
		
		[DefaultValueAttribute ("")]
		[TypeConverterAttribute (typeof (ControlPropertyNameConverter))]
		[WebCategoryAttribute ("Control")]
		public string PropertyName {
			get { return ViewState.GetString ("PropertyName", ""); }
			set {
				
				if (PropertyName != value) {
					ViewState ["PropertyName"] = value;
					OnParameterChanged ();
				}
			}
		}
	}
}
#endif

