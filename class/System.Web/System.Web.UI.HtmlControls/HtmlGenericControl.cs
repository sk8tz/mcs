/
// System.Web.UI.HtmlControls.HtmlGenericControl.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls
{
        public class HtmlGenericControl : HtmlContainerControl
        {
                public HtmlContainerControl() : base(); {}
                public HtmlContainerControl(string tag) : base(tag) {}
        }
}
