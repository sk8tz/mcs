//
// System.Web.UI.LiteralControl.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;

namespace System.Web.UI
{
        public class LiteralControl : Control
        {
                private string _text = String.Empty;
                public LiteralControl() {}
                public LiteralControl(string text)
                {
                        _text = text;
                }
                public virtual string Text
                {
                        get
                        {
                                return _text;
                        }
                        set
                        {
                                _text = value;
                        }
                }
                public override Render(HtmlTextWriter writer)
                {
                        writer.Write(_text);
                }
        }
}
