//
// System.Web.UI.Html32TextWriter.cs: Provides a HtmlTextWriter which writes HTML 3.2
//
// Authors:
// 	Matthijs ter Woord  [meddochat]  (meddochat@zonnet.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) Matthijs ter Woord, 2004
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Globalization;
using System.IO;

namespace System.Web.UI
{
	[MonoTODO ("Needs work and verification.")]
	public class Html32TextWriter : HtmlTextWriter
	{
		public Html32TextWriter (TextWriter writer) : base (writer)
		{
		}

		public Html32TextWriter (TextWriter writer, string tabString) : base (writer, tabString)
		{
		}


		public override void RenderBeginTag (HtmlTextWriterTag tagKey)
		{
                        base.RenderBeginTag (tagKey);
		}

		public override void RenderEndTag ()
		{
                        base.RenderEndTag ();
		}

		protected override string GetTagName (HtmlTextWriterTag tagKey)
		{
			if (tagKey == HtmlTextWriterTag.Unknown ||
			    !Enum.IsDefined (typeof (HtmlTextWriterTag), tagKey))
				return "";

			return tagKey.ToString ().ToLower (CultureInfo.InvariantCulture);
			/* The code below is here just in case we need to split things up
			switch (tagkey) {
			case HtmlTextWriterTag.Unknown:
				return "";
			case HtmlTextWriterTag.A:
				return "a";
			case HtmlTextWriterTag.Acronym:
				return "acronym";
			case HtmlTextWriterTag.Address:
				return "address";
			case HtmlTextWriterTag.Area:
				return "area";
			case HtmlTextWriterTag.B:
				return "b";
			case HtmlTextWriterTag.Base:
				return "base";
			case HtmlTextWriterTag.Basefont:
				return "basefont";
			case HtmlTextWriterTag.Bdo:
				return "bdo";
			case HtmlTextWriterTag.Bgsound:
				return "bgsound";
			case HtmlTextWriterTag.Big:
				return "big";
			case HtmlTextWriterTag.Blockquote:
				return "blockquote";
			case HtmlTextWriterTag.Body:
				return "body";
			case HtmlTextWriterTag.Br:
				return "br";
			case HtmlTextWriterTag.Button:
				return "button";
			case HtmlTextWriterTag.Caption:
				return "caption";
			case HtmlTextWriterTag.Center:
				return "center";
			case HtmlTextWriterTag.Cite:
				return "cite";
			case HtmlTextWriterTag.Code:
				return "code";
			case HtmlTextWriterTag.Col:
				return "col";
			case HtmlTextWriterTag.Colgroup:
				return "colgroup";
			case HtmlTextWriterTag.Dd:
				return "dd";
			case HtmlTextWriterTag.Del:
				return "del";
			case HtmlTextWriterTag.Dfn:
				return "dfn";
			case HtmlTextWriterTag.Dir:
				return "dir";
			case HtmlTextWriterTag.Div:
				return "table";
			case HtmlTextWriterTag.Dl:
				return "dl";
			case HtmlTextWriterTag.Dt:
				return "dt";
			case HtmlTextWriterTag.Em:
				return "em";
			case HtmlTextWriterTag.Embed:
				return "embed";
			case HtmlTextWriterTag.Fieldset:
				return "fieldset";
			case HtmlTextWriterTag.Font:
				return "font";
			case HtmlTextWriterTag.Form:
				return "form";
			case HtmlTextWriterTag.Frame:
				return "frame";
			case HtmlTextWriterTag.Frameset:
				return "frameset";
			case HtmlTextWriterTag.H1:
				return "h1";
			case HtmlTextWriterTag.H2:
				return "h2";
			case HtmlTextWriterTag.H3:
				return "h3";
			case HtmlTextWriterTag.H4:
				return "h4";
			case HtmlTextWriterTag.H5:
				return "h5";
			case HtmlTextWriterTag.H6:
				return "h6";
			case HtmlTextWriterTag.Head:
				return "head";
			case HtmlTextWriterTag.Hr:
				return "hr";
			case HtmlTextWriterTag.Html:
				return "html";
			case HtmlTextWriterTag.I:
				return "i";
			case HtmlTextWriterTag.Iframe:
				return "iframe";
			case HtmlTextWriterTag.Img:
				return "img";
			case HtmlTextWriterTag.Input:
				return "input";
			case HtmlTextWriterTag.Ins:
				return "ins";
			case HtmlTextWriterTag.Isindex:
				return "isindex";
			case HtmlTextWriterTag.Kbd:
				return "kbd";
			case HtmlTextWriterTag.Label:
				return "label";
			case HtmlTextWriterTag.Legend:
				return "legend";
			case HtmlTextWriterTag.Li:
				return "li";
			case HtmlTextWriterTag.Link:
				return "link";
			case HtmlTextWriterTag.Map:
				return "map";
			case HtmlTextWriterTag.Marquee:
				return "marquee";
			case HtmlTextWriterTag.Menu:
				return "menu";
			case HtmlTextWriterTag.Meta:
				return "meta";
			case HtmlTextWriterTag.Nobr:
				return "nobr";
			case HtmlTextWriterTag.Noframes:
				return "noframes";
			case HtmlTextWriterTag.Noscript:
				return "noscript";
			case HtmlTextWriterTag.Object:
				return "object";
			case HtmlTextWriterTag.Ol:
				return "ol";
			case HtmlTextWriterTag.Option:
				return "option";
			case HtmlTextWriterTag.P:
				return "p";
			case HtmlTextWriterTag.Param:
				return "param";
			case HtmlTextWriterTag.Pre:
				return "pre";
			case HtmlTextWriterTag.Q:
				return "q";
			case HtmlTextWriterTag.Rt:
				return "rt";
			case HtmlTextWriterTag.Ruby:
				return "ruby";
			case HtmlTextWriterTag.S:
				return "s";
			case HtmlTextWriterTag.Samp:
				return "samp";
			case HtmlTextWriterTag.Script:
				return "script";
			case HtmlTextWriterTag.Select:
				return "select";
			case HtmlTextWriterTag.Small:
				return "small";
			case HtmlTextWriterTag.Span:
				return "span";
			case HtmlTextWriterTag.Strike:
				return "strike";
			case HtmlTextWriterTag.Strong:
				return "strong";
			case HtmlTextWriterTag.Style:
				return "style";
			case HtmlTextWriterTag.Sub:
				return "sub";
			case HtmlTextWriterTag.Sup:
				return "sup";
			case HtmlTextWriterTag.Table:
				return "table";
			case HtmlTextWriterTag.Tbody:
				return "tbody";
			case HtmlTextWriterTag.Td:
				return "td";
			case HtmlTextWriterTag.Textarea:
				return "textarea";
			case HtmlTextWriterTag.Tfoot:
				return "tfoot";
			case HtmlTextWriterTag.Th:
				return "th";
			case HtmlTextWriterTag.Thead:
				return "thead";
			case HtmlTextWriterTag.Title:
				return "title";
			case HtmlTextWriterTag.Tr:
				return "tr";
			case HtmlTextWriterTag.Tt:
				return "tt";
			case HtmlTextWriterTag.U:
				return "u";
			case HtmlTextWriterTag.Ul:
				return "ul";
			case HtmlTextWriterTag.Var:
				return "var";
			case HtmlTextWriterTag.Wbr:
				return "wbr";
			case HtmlTextWriterTag.Xml:
				return "xml";
			default:
				return "";
			}
			*/
		}

		protected override bool OnStyleAttributeRender (string name, string value, HtmlTextWriterStyle key)
		{
                        return base.OnStyleAttributeRender (name, value, key);
		}

		protected override bool OnTagRender (string name, HtmlTextWriterTag key)
		{
                        return base.OnTagRender (name, key);
		}

		protected override string RenderAfterContent ()
		{
                        return base.RenderAfterContent ();
		}

		protected override string RenderAfterTag ()
		{
                        return base.RenderAfterTag ();
		}

		protected override string RenderBeforeContent ()
		{
                        return base.RenderBeforeContent ();
		}

		protected override string RenderBeforeTag ()
		{
                        return base.RenderBeforeTag ();
		}

		[MonoTODO]
		protected Stack FontStack {
			get {
				return null;
			}
		}
	}
}

