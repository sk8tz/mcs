//
// XslNumber.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;

namespace Mono.Xml.Xsl.Operations {
	public class XslNumber : XslCompiledElement {
		
		// <xsl:number
		//   level = "single" | "multiple" | "any"
		XslNumberingLevel level;
		//   count = pattern
		XPathExpression count;
		//   from = pattern
		XPathExpression from;
		//   value = number-expression
		XPathExpression value;
		//   format = { string }
		XslAvt format;
		//   lang = { nmtoken }
		XslAvt lang;
		//   letter-value = { "alphabetic" | "traditional" }
		XslAvt letterValue;
		//   grouping-separator = { char }
		XslAvt groupingSeparator;
		//   grouping-size = { number } />
		XslAvt groupingSize;
		
		public XslNumber (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			switch (c.GetAttribute ("level"))
			{
			case "single":
				level = XslNumberingLevel.Single;
				break;
			case "multiple":
				level = XslNumberingLevel.Multiple;
				break;
			case "any":
				level = XslNumberingLevel.Any;
				break;
			case null:
			case "":
			default:
				level = XslNumberingLevel.Single; // single == default
				break;
			}
			
			count = c.CompilePattern (c.GetAttribute ("count"));
			from = c.CompilePattern (c.GetAttribute ("from"));
			value = c.CompileExpression (c.GetAttribute ("value"));
			
			if (value != null && value.ReturnType != XPathResultType.Number && value.ReturnType != XPathResultType.Any)
				throw new Exception ("The expression for attribute 'value' must return a number");
			
			format = c.ParseAvtAttribute ("format");
			lang = c.ParseAvtAttribute ("lang");
			letterValue = c.ParseAvtAttribute ("letter-value");
			groupingSeparator = c.ParseAvtAttribute ("grouping-separator");
			groupingSize = c.ParseAvtAttribute ("grouping-size");
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			p.Out.WriteString (GetFormat (p));
		}
		
		XslNumberFormatter GetNumberFormatter (XslTransformProcessor p)
		{
			string format = "1. ";
			string lang = null;
			string letterValue = null;
			char groupingSeparator = '\0';
			int groupingSize = 0;
			
			if (this.format != null)
				format = this.format.Evaluate (p);
			
			if (this.lang != null)
				lang = this.lang.Evaluate (p);
			
			if (this.letterValue != null)
				letterValue = this.letterValue.Evaluate (p);
			
			if (this.groupingSeparator != null)
				groupingSeparator = this.groupingSeparator.Evaluate (p) [0];
			
			if (this.groupingSize != null)
				groupingSize = int.Parse (this.groupingSize.Evaluate (p));
			
			return new XslNumberFormatter (format, lang, letterValue, groupingSeparator, groupingSize);
		}
		
		string GetFormat (XslTransformProcessor p)
		{
			XslNumberFormatter nf = GetNumberFormatter (p);
			
			if (this.value != null)
				return nf.Format ((int)p.EvaluateNumber (this.value)); // TODO: Correct rounding
			
			switch (this.level) {
				case XslNumberingLevel.Single:
					return nf.Format (NumberSingle (p));
				case XslNumberingLevel.Multiple:
					throw new NotImplementedException ();
				case XslNumberingLevel.Any:
					return nf.Format (NumberAny (p));
				default:
					throw new Exception ("Should not get here");
			}
		}
		int NumberAny (XslTransformProcessor p)
		{
			int i = 0;
			XPathNavigator n = p.CurrentNode.Clone ();
			do {
				do {
					if (MatchesCount (n, p)) i++;
					if (MatchesFrom (n, p)) return i;
				} while (n.MoveToPrevious ());
			} while (n.MoveToParent ());
			return 0;
		}
		int NumberSingle (XslTransformProcessor p)
		{
			XPathNavigator n = p.CurrentNode.Clone ();
		
			while (!MatchesCount (n, p)) {
				if (from != null && MatchesFrom (n, p))
					return 0;
				
				if (!n.MoveToParent ())
					return 0;
			}
			
			if (from != null) {
				XPathNavigator tmp = n.Clone ();
				if (MatchesFrom (tmp, p))
					// Was not desc of closest matches from
					return 0;
				
				bool found = false;
				while (tmp.MoveToParent ())
					if (MatchesFrom (tmp, p)) {
						found = true; break;
					}
				if (!found)
					// not desc of matches from
					return 0;
			}
			
			int i = 1;
				
			while (n.MoveToPrevious ()) {
				if (MatchesCount (n, p)) i++;
			}
				
			return i;
		}
		
		bool MatchesCount (XPathNavigator item, XslTransformProcessor p)
		{
			if (count == null)
				return item.LocalName == p.CurrentNode.LocalName &&
					item.NamespaceURI == p.CurrentNode.NamespaceURI;
			else
				return item.Matches (count);
		}
		
		bool MatchesFrom (XPathNavigator item, XslTransformProcessor p)
		{
			if (from == null)
				return item.NodeType == XPathNodeType.Root;
			else
				return item.Matches (from);
		}
		
		class XslNumberFormatter {
			string firstSep = "", lastSep = "";
			ArrayList fmtList = new ArrayList ();
			
			public XslNumberFormatter (string format, string lang, string letterValue, char groupingSeparator, int groupingSize)
			{
				// We dont do any i18n now, so we ignore lang and letterValue.
				if (format == null || format == "")
					fmtList.Add (FormatItem.GetItem (null, "1", groupingSeparator, groupingSize));
				else {
					NumberFormatterScanner s = new NumberFormatterScanner (format);
					
					string sep, itm;
					
					sep = s.Advance (false);
					itm = s.Advance (true);
					
					if (itm == null) {
						lastSep = sep;
						fmtList.Add (FormatItem.GetItem (null, "1", groupingSeparator, groupingSize));
					} else {
						firstSep = sep;
						sep = null;
					
						while (itm != null) {
							fmtList.Add (FormatItem.GetItem (sep, itm, groupingSeparator, groupingSize));
							sep = s.Advance (false);
							itm = s.Advance (true);
						}
						
						lastSep = sep;
					}
				}
			}
			
			public int NumbersNeeded {
				get { return fmtList.Count; }
			}
			
			// return the format for a single value, ie, if using Single or Any
			public string Format (int value)
			{
				StringBuilder b = new StringBuilder ();
				if (firstSep != null) b.Append (firstSep);
				((FormatItem)fmtList [0]).Format (b, value);
				if (lastSep != null) b.Append (lastSep);
				
				return b.ToString ();
			}
			
			// format for an array of numbers.
			public string Format (int [] values)
			{
				throw new NotImplementedException ();
			}
			
			class NumberFormatterScanner {
				int pos = 0, len;
				string fmt;
				
				public NumberFormatterScanner (string fmt) {
					this.fmt = fmt;
					len = fmt.Length;
				}
				
				public string Advance (bool alphaNum)
				{
					int start = pos;
					while ((pos < len) && (char.IsLetterOrDigit (fmt, pos) == alphaNum))
						pos++;
					
					if (pos == start)
						return null;
					else
						return fmt.Substring (start, pos - start);
				}
			}
			
			abstract class FormatItem {
				public readonly string sep;
				public FormatItem (string sep)
				{
					this.sep = sep;
				}
				
				public abstract void Format (StringBuilder b, int num);
					
				public static FormatItem GetItem (string sep, string item, char gpSep, int gpSize)
				{
					switch (item [0])
					{
						case '0': case '1':
							return new DigitItem (sep, item.Length, gpSep, gpSize);
						case 'a':
							return new AlphaItem (sep, false);
						case 'A':
							return new AlphaItem (sep, true);
						case 'i':
							return new RomanItem (sep, false);
						case 'I':
							return new RomanItem (sep, true);
						
						default:
							throw new Exception ();
					}
				}
			}
			
			class AlphaItem : FormatItem {
				bool uc;
				public AlphaItem (string sep, bool uc) : base (sep)
				{
					this.uc = uc;
				}
				
				public override void Format (StringBuilder b, int num)
				{
					throw new NotImplementedException ();
				}
			}
			
			class RomanItem : FormatItem {
				bool uc;
				public RomanItem (string sep, bool uc) : base (sep)
				{
					this.uc = uc;
				}
				
				public override void Format (StringBuilder b, int num)
				{
					throw new NotImplementedException ();
				}
			}
			
			class DigitItem : FormatItem {
				int len, gpSize;
				char gpSep;
				
				public DigitItem (string sep, int len, char gpSep, int gpSize) : base (sep)
				{
					this.len = len;
					this.gpSep = gpSep;
					this.gpSize = gpSize;
				}
				
				public override void Format (StringBuilder b, int num)
				{
					// TODO Formatting
					b.Append (num);
				}
			}
		}
	}
	
	public enum XslNumberingLevel
	{
		Single,
		Multiple,
		Any
	}
}
