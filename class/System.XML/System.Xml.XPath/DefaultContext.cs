//
// System.Xml.XPath.DefaultContext & support classes
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
//
using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;

namespace System.Xml.XPath
{
	internal class XPathFunctions
	{
		public static bool ToBoolean (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is bool)
				return (bool) arg;
			if (arg is double)
			{
				double dArg = (double) arg;
				return (dArg != 0.0 && !double.IsNaN (dArg));
			}
			if (arg is string)
				return ((string) arg).Length != 0;
			if (arg is BaseIterator)
			{
				BaseIterator iter = (BaseIterator) arg;
				return iter.MoveNext ();
			}
			throw new ArgumentException ();
		}
		[MonoTODO]
		public static string ToString (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is string)
				return (string) arg;
			if (arg is bool)
				return ((bool) arg) ? "true" : "false";
			if (arg is double)
				return ((double) arg).ToString ("R", System.Globalization.NumberFormatInfo.InvariantInfo);
			if (arg is BaseIterator)
			{
				BaseIterator iter = (BaseIterator) arg;
				if (!iter.MoveNext ())
					return "";
				return iter.Current.Value;
			}
			throw new ArgumentException ();
		}
		[MonoTODO]
		public static double ToNumber (object arg)
		{
			if (arg == null)
				throw new ArgumentNullException ();
			if (arg is string)
			{
				try
				{
					return XmlConvert.ToDouble ((string) arg);	// TODO: spec? convert string to number
				}
				catch (System.FormatException)
				{
					return double.NaN;
				}
			}
			if (arg is BaseIterator)
				arg = ToString (arg);	// follow on
			if (arg is double)
				return (double) arg;
			if (arg is bool)
				return Convert.ToDouble ((bool) arg);
			throw new ArgumentException ();
		}
	}

	internal abstract class XPathFunction : Expression
	{
		public XPathFunction (FunctionArguments args) {}
	}


	internal class XPathFunctionLast : XPathFunction
	{
		public XPathFunctionLast (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("last takes 0 args");
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		public override object Evaluate (BaseIterator iter)
		{
			return (double) iter.Count;
		}
	}


	internal class XPathFunctionPosition : XPathFunction
	{
		public XPathFunctionPosition (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("position takes 0 args");
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		public override object Evaluate (BaseIterator iter)
		{
			return (double) iter.CurrentPosition;
		}
	}


	internal class XPathFunctionCount : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionCount (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("count takes 1 arg");
			
			arg0 = args.Arg;
		}

		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			return (double) arg0.EvaluateNodeSet (iter).Count;
		}
		
		public override bool EvaluateBoolean (BaseIterator iter)
		{
			if (arg0.GetReturnType (iter) == XPathResultType.NodeSet)
				return arg0.EvaluateBoolean (iter);
			
			return arg0.EvaluateNodeSet (iter).MoveNext ();
		}
	}


	internal class XPathFunctionId : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionId (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("id takes 1 arg");
			
			arg0 = args.Arg;
		}
		
		private static char [] rgchWhitespace = {' ', '\t', '\r', '\n'};
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}

		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			String strArgs;
			object val = arg0.Evaluate (iter);
			
			BaseIterator valItr = val as BaseIterator;
			if (valItr != null)
			{
				strArgs = "";
				while (!valItr.MoveNext ())
					strArgs += valItr.Current.Value + " ";
			}
			else
				strArgs = XPathFunctions.ToString (val);
			
			XPathNavigator n = iter.Current.Clone ();
			ArrayList rgNodes = new ArrayList ();
			foreach (string strArg in strArgs.Split (rgchWhitespace))
			{
				if (n.MoveToId (strArg))
					rgNodes.Add (n.Clone ());
			}
			return new EnumeratorIterator (iter, rgNodes.GetEnumerator ());
		}
	}


	internal class XPathFunctionLocalName : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionLocalName (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("local-name takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.LocalName;
			
			BaseIterator argNs = arg0.EvaluateNodeSet (iter);
			if (argNs == null || !argNs.MoveNext ())
				return "";
			return argNs.Current.LocalName;
		}
	}


	internal class XPathFunctionNamespaceUri : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionNamespaceUri (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("namespace-uri takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.NamespaceURI;
			
			BaseIterator argNs = arg0.EvaluateNodeSet (iter);
			if (argNs == null || !argNs.MoveNext ())
				return "";
			return argNs.Current.NamespaceURI;
		}
	}


	internal class XPathFunctionName : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionName (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("name takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.Name;
			
			BaseIterator argNs = arg0.EvaluateNodeSet (iter);
			if (argNs == null || !argNs.MoveNext ())
				return "";
			return argNs.Current.Name;
		}
	}


	internal class XPathFunctionString : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionString (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("boolean takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return iter.Current.Value;
			return arg0.EvaluateString (iter);
		}
	}


	internal class XPathFunctionConcat : XPathFunction
	{
		ArrayList rgs;
		
		public XPathFunctionConcat (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null)
				throw new XPathException ("concat takes 2 or more args");
			
			args.ToArrayList (rgs = new ArrayList ());
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			StringBuilder sb = new StringBuilder ();
			
			int len = rgs.Count;
			for (int i = 0; i < len; i++)
				sb.Append (((Expression)rgs[i]).EvaluateString (iter));
			
			return sb.ToString ();
		}
	}


	internal class XPathFunctionStartsWith : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionStartsWith (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("starts-with takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			return arg0.EvaluateString (iter).StartsWith (arg1.EvaluateString (iter));
		}
	}


	internal class XPathFunctionContains : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionContains (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("contains takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			return arg0.EvaluateString (iter).IndexOf (arg1.EvaluateString (iter)) != -1;
		}
	}


	internal class XPathFunctionSubstringBefore : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionSubstringBefore (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("substring-before takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			string str1 = arg0.EvaluateString (iter);
			string str2 = arg1.EvaluateString (iter);
			int ich = str1.IndexOf (str2);
			if (ich <= 0)
				return "";
			return str1.Substring (0, ich);
		}
	}


	internal class XPathFunctionSubstringAfter : XPathFunction
	{
		Expression arg0, arg1;
		
		public XPathFunctionSubstringAfter (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail != null)
				throw new XPathException ("substring-after takes 2 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			string str1 = arg0.EvaluateString (iter);
			string str2 = arg1.EvaluateString (iter);
			int ich = str1.IndexOf (str2);
			if (ich < 0)
				return "";
			return str1.Substring (ich + str2.Length);
		}
	}


	internal class XPathFunctionSubstring : XPathFunction
	{
		Expression arg0, arg1, arg2;
		
		public XPathFunctionSubstring (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || (args.Tail.Tail != null && args.Tail.Tail.Tail != null))
				throw new XPathException ("substring takes 2 or 3 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			if (args.Tail.Tail != null)
				arg2= args.Tail.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			// TODO: check this, what the hell were they smoking?
			string str = arg0.EvaluateString (iter);
			double ich = Math.Round (arg1.EvaluateNumber (iter)) - 1;
			if (Double.IsNaN (ich) || ich >= (double) str.Length)
				return "";

			if (arg2 == null)
			{
				if (ich < 0)
					ich = 0.0;
				return str.Substring ((int) ich);
			}
			else
			{
				double cch = Math.Round (arg2.EvaluateNumber (iter));
				if (Double.IsNaN (cch))
					return "";
				if (ich < 0.0 || cch < 0.0) 
				{
					cch = ich + cch;
					if (cch <= 0.0)
						return "";
					ich = 0.0;
				}
				double cchMax = (double) str.Length - ich;
				if (cch > cchMax)
					cch = cchMax;
				return str.Substring ((int) ich, (int) cch);
			}
		}
	}


	internal class XPathFunctionStringLength : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionStringLength (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("string-length takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			string str;
			if (arg0 != null)
				str = arg0.EvaluateString (iter);
			else
				str = iter.Current.Value;
			return (double) str.Length;
		}
	}


	internal class XPathFunctionNormalizeSpace : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionNormalizeSpace (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("string-length takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			string str;
			if (arg0 == null)
				str = arg0.EvaluateString (iter);
			else
				str = iter.Current.Value;
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			bool fSpace = false;
			foreach (char ch in str)
			{
				if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
				{
					fSpace = true;
				}
				else
				{
					if (fSpace)
					{
						fSpace = false;
						if (sb.Length > 0)
							sb.Append (' ');
					}
					sb.Append (ch);
				}
			}
			return sb.ToString ();
		}
	}


	internal class XPathFunctionTranslate : XPathFunction
	{
		Expression arg0, arg1, arg2;
		
		public XPathFunctionTranslate (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail == null || args.Tail.Tail == null || args.Tail.Tail.Tail != null)
				throw new XPathException ("translate takes 3 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			arg2= args.Tail.Tail.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			throw new NotImplementedException ();
		}
	}


	internal class XPathFunctionBoolean : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionBoolean (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("boolean takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return XPathFunctions.ToBoolean (iter.Current.Value);
			return arg0.EvaluateBoolean (iter);
		}
	}


	internal class XPathFunctionNot : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionNot (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("not takes one arg");
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object Evaluate (BaseIterator iter)
		{
			return !arg0.EvaluateBoolean (iter);
		}
	}


	internal class XPathFunctionTrue : XPathFunction
	{
		public XPathFunctionTrue (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("true takes 0 args");
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object Evaluate (BaseIterator iter)
		{
			return true;
		}
	}


	internal class XPathFunctionFalse : XPathFunction
	{
		public XPathFunctionFalse (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("false takes 0 args");
		}
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object Evaluate (BaseIterator iter)
		{
			return false;
		}
	}


	internal class XPathFunctionLang : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionLang (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("lang takes one arg");
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object Evaluate (BaseIterator iter)
		{
			string lang = arg0.EvaluateString (iter).ToLower ();
			string actualLang = iter.Current.XmlLang.ToLower ();
			
			return lang == actualLang || lang == (actualLang.Split ('-')[0]);
		}
	}


	internal class XPathFunctionNumber : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionNumber (FunctionArguments args) : base (args)
		{
			if (args != null) {
				arg0 = args.Arg;
				if (args.Tail != null)
					throw new XPathException ("number takes 1 or zero args");
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		public override object Evaluate (BaseIterator iter)
		{
			if (arg0 == null)
				return XPathFunctions.ToNumber (iter.Current.Value);
			return arg0.EvaluateNumber (iter);
		}
	}


	internal class XPathFunctionSum : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionSum (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("sum takes one arg");
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			throw new NotImplementedException ();
		}
	}


	internal class XPathFunctionFloor : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionFloor (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("floor takes one arg");
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		public override object Evaluate (BaseIterator iter)
		{
			return Math.Floor (arg0.EvaluateNumber (iter));
		}
	}


	internal class XPathFunctionCeil : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionCeil (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("ceil takes one arg");
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		public override object Evaluate (BaseIterator iter)
		{
			return Math.Ceiling (arg0.EvaluateNumber (iter));
		}
	}


	internal class XPathFunctionRound : XPathFunction
	{
		Expression arg0;
		
		public XPathFunctionRound (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("round takes one arg");
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}

		public override object Evaluate (BaseIterator iter)
		{
			double arg = arg0.EvaluateNumber (iter);
			if (arg < -0.5 || arg > 0)
				return Math.Floor (arg + 0.5);
			return Math.Round (arg);
		}
	}
}
