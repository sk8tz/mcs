//
// System.Diagnostics.Debug.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	public sealed class Debug {

		private Debug () {}

		public static bool AutoFlush {
			get {return TraceImpl.AutoFlush;}
			set {TraceImpl.AutoFlush = value;}
		}

		public static int IndentLevel {
			get {return TraceImpl.IndentLevel;}
			set {TraceImpl.IndentLevel = value;}
		}

		public static int IndentSize {
			get {return TraceImpl.IndentSize;}
			set {TraceImpl.IndentSize = value;}
		}

		public static TraceListenerCollection Listeners {
			get {return TraceImpl.Listeners;}
		}

		[Conditional("DEBUG")]
		public static void Assert (bool condition)
		{
			TraceImpl.Assert (condition);
		}

		[Conditional("DEBUG")]
		public static void Assert (bool condition, string message)
		{
			TraceImpl.Assert (condition, message);
		}

		[Conditional("DEBUG")]
		public static void Assert (bool condition, string message, 
			string detailMessage)
		{
			TraceImpl.Assert (condition, message, detailMessage);
		}

		[Conditional("DEBUG")]
		public static void Close ()
		{
			TraceImpl.Close ();
		}

		[Conditional("DEBUG")]
		public static void Fail (string message)
		{
			TraceImpl.Fail (message);
		}

		[Conditional("DEBUG")]
		public static void Fail (string message, string detailMessage)
		{
			TraceImpl.Fail (message, detailMessage);
		}

		[Conditional("DEBUG")]
		public static void Flush ()
		{
			TraceImpl.Flush ();
		}

		[Conditional("DEBUG")]
		public static void Indent ()
		{
			TraceImpl.Indent ();
		}

		[Conditional("DEBUG")]
		public static void Unindent ()
		{
			TraceImpl.Unindent ();
		}

		[Conditional("DEBUG")]
		public static void Write (object value)
		{
			TraceImpl.Write (value);
		}

		[Conditional("DEBUG")]
		public static void Write (string message)
		{
			TraceImpl.Write (message);
		}

		[Conditional("DEBUG")]
		public static void Write (object value, string category)
		{
			TraceImpl.Write (value, category);
		}

		[Conditional("DEBUG")]
		public static void Write (string message, string category)
		{
			TraceImpl.Write (message, category);
		}

		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, object value)
		{
			TraceImpl.WriteIf (condition, value);
		}

		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, string message)
		{
			TraceImpl.WriteIf (condition, message);
		}

		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, object value, 
			string category)
		{
			TraceImpl.WriteIf (condition, value, category);
		}

		[Conditional("DEBUG")]
		public static void WriteIf (bool condition, string message, 
			string category)
		{
			TraceImpl.WriteIf (condition, message, category);
		}

		[Conditional("DEBUG")]
		public static void WriteLine (object value)
		{
			TraceImpl.WriteLine (value);
		}

		[Conditional("DEBUG")]
		public static void WriteLine (string message)
		{
			TraceImpl.WriteLine (message);
		}

		[Conditional("DEBUG")]
		public static void WriteLine (object value, string category)
		{
			TraceImpl.WriteLine (value, category);
		}

		[Conditional("DEBUG")]
		public static void WriteLine (string message, string category)
		{
			TraceImpl.WriteLine (message, category);
		}

		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, object value)
		{
			TraceImpl.WriteLineIf (condition, value);
		}

		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, string message)
		{
			TraceImpl.WriteLineIf (condition, message);
		}

		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, object value, 
			string category)
		{
			TraceImpl.WriteLineIf (condition, value, category);
		}

		[Conditional("DEBUG")]
		public static void WriteLineIf (bool condition, string message, 
			string category)
		{
			TraceImpl.WriteLineIf (condition, message, category);
		}
	}
}

