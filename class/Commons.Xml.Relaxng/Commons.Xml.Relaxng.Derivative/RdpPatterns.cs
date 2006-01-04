//
// Commons.Xml.Relaxng.Derivative.RdpPatterns.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
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

using System;
using System.Collections;
using System.Xml;
using Commons.Xml.Relaxng;

using LabelList = System.Collections.Hashtable;


namespace Commons.Xml.Relaxng.Derivative
{
	public delegate RdpPattern RdpApplyAfterHandler (RdpPattern p);

	// abstract Pattern. Note that in README the classes in this namespace
	// is explicitly written as not for public use (and hidden in monodoc
	// for now).
	public abstract class RdpPattern
	{
		internal bool nullableComputed;
		internal bool isNullable;
		Hashtable patternPool;

		internal string debug ()
		{
			return RdpUtil.DebugRdpPattern (this, new Hashtable ());
		}

		public abstract RelaxngPatternType PatternType { get; }

		public abstract RdpContentType ContentType { get; }

		private Hashtable setupTable (Type type, RdpPattern p)
		{
			if (patternPool == null) // could be null for RdpElement etc.
				patternPool = new Hashtable ();

			Hashtable typePool = (Hashtable) patternPool [type];
			if (typePool == null) {
				typePool = new Hashtable ();
				patternPool [type] = typePool;
			}
			Hashtable pTable = (Hashtable) typePool [p];
			if (pTable == null) {
				pTable = new Hashtable ();
				typePool [p] = pTable;
			}
			return pTable;
		}

		internal RdpFlip MakeFlip (RdpBinaryFunction func, RdpPattern p)
		{
			if (patternPool == null) // could be null for RdpElement etc.
				patternPool = new Hashtable ();

			// Though this method takes function argument, all
			// p1 callers have different pattern types, so we don't
			// have to distinguish tables by func.

			Hashtable table = patternPool [func] as Hashtable;
			if (table == null) {
				table = new Hashtable ();
				patternPool [func] = table;
			}
			RdpFlip f = table [p] as RdpFlip;
			if (f != null)
				return f;
			f = new RdpFlip (func, p);
			table [p] = f;
			return f;
		}

		public RdpChoice MakeChoiceLeaf (RdpPattern p)
		{
			if (patternPool == null) // could be null for RdpElement etc.
				patternPool = new Hashtable ();
			Hashtable leaves = (Hashtable) patternPool [typeof (RdpEmpty)];
			if (leaves == null) {
				leaves = new Hashtable ();
				patternPool [typeof (RdpEmpty)] = leaves;
			}
			RdpChoice leaf = leaves [p] as RdpChoice;
			if (leaf == null) {
				leaf = new RdpChoice (RdpEmpty.Instance, p);
				leaf.setInternTable (patternPool);
				leaves [p] = leaf;
			}
			return leaf;
		}

		public RdpPattern MakeChoice (RdpPattern p1, RdpPattern p2)
		{
			if (p1.PatternType == RelaxngPatternType.NotAllowed)
				return p2;
			if (p2.PatternType == RelaxngPatternType.NotAllowed)
				return p1;
			// choice-leaves support
			if (p1.PatternType == RelaxngPatternType.Empty)
				return MakeChoiceLeaf (p2);
			if (p2.PatternType == RelaxngPatternType.Empty)
				return MakeChoiceLeaf (p1);

			if (p1.GetHashCode () > p2.GetHashCode ()) {
				RdpPattern tmp = p1;
				p1 = p2;
				p2 = tmp;
			}

			Hashtable p1Table = setupTable (typeof (RdpChoice), p1);
			if (p1Table [p2] == null) {
				RdpChoice c = new RdpChoice (p1, p2);
				c.setInternTable (this.patternPool);
				p1Table [p2] = c;
			}
			return (RdpChoice) p1Table [p2];
		}

		public RdpPattern MakeGroup (RdpPattern p1, RdpPattern p2)
		{
			if (p1.PatternType == RelaxngPatternType.Empty)
				return p2;

			Hashtable p1Table = setupTable (typeof (RdpGroup), p1);
			if (p1Table [p2] == null) {
				RdpGroup g = new RdpGroup (p1, p2);
				g.setInternTable (this.patternPool);
				p1Table [p2] = g;
			}
			return (RdpGroup) p1Table [p2];
		}

		public RdpInterleave MakeInterleave (RdpPattern p1, RdpPattern p2)
		{
			if (p1.GetHashCode () > p2.GetHashCode ()) {
				RdpPattern tmp = p1;
				p1 = p2;
				p2 = tmp;
			}

			Hashtable p1Table = setupTable (typeof (RdpInterleave), p1);
			if (p1Table [p2] == null) {
				RdpInterleave i = new RdpInterleave (p1, p2);
				i.setInternTable (this.patternPool);
				p1Table [p2] = i;
			}
			return (RdpInterleave) p1Table [p2];
		}

		public RdpAfter MakeAfter (RdpPattern p1, RdpPattern p2)
		{
			Hashtable p1Table = setupTable (typeof (RdpAfter), p1);
			if (p1Table [p2] == null) {
				RdpAfter a = new RdpAfter (p1, p2);
				a.setInternTable (this.patternPool);
				p1Table [p2] = a;
			}
			return (RdpAfter) p1Table [p2];
		}

		public RdpOneOrMore MakeOneOrMore (RdpPattern p)
		{
			if (patternPool == null) // could be null for RdpElement etc.
				patternPool = new Hashtable ();

			Hashtable pTable = (Hashtable) patternPool [typeof (RdpOneOrMore)];
			if (pTable == null) {
				pTable = new Hashtable ();
				patternPool [typeof (RdpOneOrMore)] = pTable;
			}
			if (pTable [p] == null) {
				RdpOneOrMore oom = new RdpOneOrMore (p);
				oom.setInternTable (patternPool);
				pTable [p] = oom;
			}
			return (RdpOneOrMore) pTable [p];
		}

		internal void setInternTable (Hashtable ht)
		{
			if (this.patternPool != null)
				return;
			this.patternPool = ht;

			Hashtable pt = ht [GetType ()] as Hashtable;
			if (pt == null) {
				pt = new Hashtable ();
				ht [GetType ()] = pt;
			}

			RdpAbstractSingleContent single =
				this as RdpAbstractSingleContent;
			if (single != null) {
				if (pt [single.Child] == null) {
					pt [single.Child] = this;
					single.Child.setInternTable (ht);
				}
				return;
			}

			RdpAbstractBinary binary =
				this as RdpAbstractBinary;
			if (binary != null) {
				Hashtable lTable = setupTable (GetType (), binary.LValue);
				if (lTable [binary.RValue] == null) {
					lTable [binary.RValue] = this;
					binary.LValue.setInternTable (ht);
					binary.RValue.setInternTable (ht);
				}
				return;
			}

			// For rest patterns, only check recursively, without pooling.
			RdpAttribute attr = this as RdpAttribute;
			if (attr != null) {
				attr.Children.setInternTable (ht);
				return;
			}
			RdpElement el = this as RdpElement;
			if (el != null) {
				el.Children.setInternTable (ht);
				return;
			}
			RdpDataExcept dex= this as RdpDataExcept;
			if (dex != null) {
				dex.Except.setInternTable (ht);
				return;
			}

			switch (PatternType) {
			case RelaxngPatternType.Empty:
			case RelaxngPatternType.NotAllowed:
			case RelaxngPatternType.Text:
			case RelaxngPatternType.Data:
			case RelaxngPatternType.Value:
				return;
			}

#if REPLACE_IN_ADVANCE
			throw new InvalidOperationException ();
#endif
		}

		internal abstract void MarkReachableDefs ();

		internal abstract void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept);

		// This method is to detect text pattern inside interleave child.
		internal abstract bool ContainsText ();

		internal virtual RdpPattern ExpandRef (Hashtable defs)
		{
			return this;
		}

		internal virtual RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			return this;
		}

		public abstract bool Nullable { get; }

		// fills QName collection
		public void GetLabels (LabelList elements, LabelList attributes)
		{
			GetLabels (elements, attributes, false);
		}

		public abstract void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass);

		internal void AddNameLabel (LabelList names, RdpNameClass nc)
		{
			RdpName name = nc as RdpName;
			if (name != null) {
				XmlQualifiedName qname = new XmlQualifiedName (
					name.LocalName, name.NamespaceURI);
				names [qname] = qname;
				return;
			}
			RdpNameClassChoice choice = nc as RdpNameClassChoice;
			if (choice != null) {
				AddNameLabel (names, choice.LValue);
				AddNameLabel (names, choice.RValue);
				return;
			}
			// For NsName and AnyName, do nothing.
		}

		#region Derivative
		public virtual RdpPattern TextDeriv (string s, XmlReader reader)
		{
			// This is an extension to JJC algorithm.
			// Whitespace text are allowed except for Data and Value
			// (their TextDeriv are overridden)
			//
			// FIXME: taking stripChildrenDeriv which we don't
			// implement into consideration, it should not be done.
			return Util.IsWhitespace (s) ? this : RdpNotAllowed.Instance;
//			return RdpNotAllowed.Instance;
		}

		internal virtual RdpPattern TextOnlyDeriv ()
		{
			return this;
		}

		// FIXME: this should not require argument string
		internal virtual RdpPattern MixedTextDeriv (string s)
		{
			return Util.IsWhitespace (s) ? this : RdpNotAllowed.Instance;
//			return RdpNotAllowed.Instance;
		}

/*
		public RdpPattern ChildDeriv (RdpChildNode child)
		{
			RdpTextChild tc = child as RdpTextChild;
//			RdpElementChild ec = child as RdpElementChild;
			if (tc != null) {
				return TextDeriv (tc.Text);
			} else {
				// complex stuff;-(
				return StartTagOpenDeriv (ec.LocalName, ec.NamespaceURI)
						.AttsDeriv (ec.Attributes)
						.StartTagCloseDeriv ()
					.ChildrenDeriv (ec.ChildNodes)
					.EndTagDeriv ();
			}
		}
*/

		public RdpPattern ListDeriv (string [] list, int index, XmlReader reader)
		{
			return listDerivInternal (list, 0, reader);
		}

		private RdpPattern listDerivInternal (string [] list, int start, XmlReader reader)
		{
			if (list.Length <= start)
				return this;
			else
				return this.TextDeriv (list [start], reader).listDerivInternal (list, start + 1, reader);
		}

		// Choice(this, p)
		public virtual RdpPattern Choice (RdpPattern p)
		{
			if (p is RdpNotAllowed)
				return this;
			else if (this is RdpNotAllowed)
				return p;
			else
				return MakeChoice (this, p);
		}

		// Group(this, p)
		public virtual RdpPattern Group (RdpPattern p)
		{
			if (p is RdpNotAllowed || this is RdpNotAllowed)
				return RdpNotAllowed.Instance;
			else if (p is RdpEmpty)
				return this;
			else if (this is RdpEmpty)
				return p;
			else
				return MakeGroup (this, p);
		}

		// Interleave(this, p)
		public virtual RdpPattern Interleave (RdpPattern p)
		{
			if (p is RdpNotAllowed || this is RdpNotAllowed)
				return RdpNotAllowed.Instance;
			else if (p is RdpEmpty)
				return this;
			else if (this is RdpEmpty)
				return p;
			else
				return MakeInterleave (this, p);
		}

		// After(this, p)
		public virtual RdpPattern After (RdpPattern p)
		{
			if (this is RdpNotAllowed || p is RdpNotAllowed)
				return RdpNotAllowed.Instance;
			else
				return MakeAfter (this, p);
		}


		// applyAfter((f, p1=this), p2)
		public virtual RdpPattern ApplyAfter (RdpApplyAfterHandler h)
		{
			return RdpNotAllowed.Instance;
		}

		// startTagOpenDeriv (this, qname)
		// startTagOpenDeriv _ qn = NotAllowed (default)
		public virtual RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			return RdpNotAllowed.Instance;
		}

		// attDeriv(ctx, this, att)
		// attDeriv _ _ _ = NotAllowed
		public virtual RdpPattern AttDeriv (string name, string ns, string value, XmlReader reader)
		{
			return RdpNotAllowed.Instance;
		}

		public bool ValueMatch (string s, XmlReader reader)
		{
			return TextDeriv (s, reader).Nullable;
		}

		public virtual RdpPattern StartTagCloseDeriv ()
		{
			return this;
		}

		/*
		public RdpPattern ChildrenDeriv (RdpChildNodes children)
		{
			return childrenDerivInternal (children, 0);
		}

		RdpPattern childrenDerivInternal (RdpChildNodes children, int start)
		{
			if (children.Count == 0) {
				// childrenDeriv cx p []
				RdpChildNodes c = new RdpChildNodes ();
				c.Add (new RdpTextChild (String.Empty));
				return ChildrenDeriv (c);
			} else if (children.Count == 1 && children [0] is RdpTextChild) {
				// childrenDeriv cx p [(TextNode s)]
				RdpTextChild tc = children [0] as RdpTextChild;
				RdpPattern p1 = ChildDeriv (tc);
				return RdpUtil.Whitespace (tc.Text) ?
					RdpUtil.Choice (this, p1) : p1;
			} else
				// childrenDeriv cx p children
				return stripChildrenDerivInternal (children, start);
		}

		RdpPattern stripChildrenDerivInternal (RdpChildNodes children, int start)
		{
			if (children.Count == start)
				return this;
			else {
				RdpChildNode firstChild =
					children [start] as RdpChildNode;
				RdpPattern p =
					(firstChild.IsNonWhitespaceText) ?
					this : ChildDeriv (firstChild);
				return p.childrenDerivInternal (children, start + 1);
			}
		}
		*/

		public RdpPattern OneOrMore ()
		{
			if (PatternType == RelaxngPatternType.NotAllowed)
				return RdpNotAllowed.Instance;
			else
				return MakeOneOrMore (this);
		}

		public virtual RdpPattern EndTagDeriv ()
		{
			return RdpNotAllowed.Instance;
		}
		#endregion
	}

	// Empty
	public class RdpEmpty : RdpPattern
	{
		public RdpEmpty () {}
		static RdpEmpty ()
		{
			instance = new RdpEmpty ();
		}

		public override bool Nullable {
			get { return true; }
		}

		static RdpEmpty instance;
		public static RdpEmpty Instance {
			get { return instance; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Empty; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Empty; }
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			// do nothing
		}

		internal override void MarkReachableDefs () 
		{
			// do nothing
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			if (dataExcept)
				throw new RelaxngException ("empty cannot appear under except of a data pattern.");
		}

		internal override bool ContainsText()
		{
			return false;
		}
	}

	// NotAllowed
	public class RdpNotAllowed : RdpPattern
	{
		public RdpNotAllowed () {}
		static RdpNotAllowed ()
		{
			instance = new RdpNotAllowed ();
		}

		static RdpNotAllowed instance;
		public static RdpNotAllowed Instance {
			get { return instance; }
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RdpPattern ApplyAfter (RdpApplyAfterHandler h)
		{
			return RdpNotAllowed.Instance;
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.NotAllowed; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Empty; }
		}

		internal override void MarkReachableDefs () 
		{
			// do nothing
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			// do nothing
		}

		internal override bool ContainsText()
		{
			return false;
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			// FIXME: Supposed to clear something here?
		}
	}

	// Text
	public class RdpText : RdpPattern
	{
		static RdpText instance;
		public static RdpText Instance {
			get { return instance; }
		}

		public RdpText () {}
		static RdpText ()
		{
			instance = new RdpText ();
		}

		public override bool Nullable {
			get { return true; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Text; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Complex; }
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			return this;
		}

		internal override RdpPattern MixedTextDeriv (string s)
		{
			return this;
		}

		internal override void MarkReachableDefs () 
		{
			// do nothing
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			if (list)
				throw new RelaxngException ("text is not allowed under a list.");
			if (dataExcept)
				throw new RelaxngException ("text is not allowed under except of a list.");
		}

		internal override bool ContainsText()
		{
			return true;
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			// do nothing
		}
	}

	// AbstractBinary
	public abstract class RdpAbstractBinary : RdpPattern
	{
		public RdpAbstractBinary (RdpPattern l, RdpPattern r)
		{
			this.l = l;
			this.r = r;
		}

		RdpPattern l;
		public RdpPattern LValue {
			get { return l; }
			set { l = value; }
		}

		RdpPattern r;
		public RdpPattern RValue {
			get { return r; }
			set { r = value; }
		}

		RdpContentType computedContentType = RdpContentType.Invalid;
		public override RdpContentType ContentType {
			get {
				if (computedContentType == RdpContentType.Invalid) {
					if (l.ContentType == RdpContentType.Empty)
						computedContentType = r.ContentType;
					else if (r.ContentType == RdpContentType.Empty)
						computedContentType = l.ContentType;
					else if ((l.ContentType & RdpContentType.Simple) != 0 || ((r.ContentType & RdpContentType.Simple) != 0))
						throw new RelaxngException ("The content type of this group is invalid.");
					else
						computedContentType = RdpContentType.Complex;
				}
				return computedContentType;
			}
		}

		bool expanded;
		internal override RdpPattern ExpandRef (Hashtable defs)
		{
			if (!expanded) {
				l = l.ExpandRef (defs);
				r = r.ExpandRef (defs);
			}
			return this;
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (LValue.PatternType == RelaxngPatternType.NotAllowed ||
				RValue.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else if (LValue.PatternType == RelaxngPatternType.Empty) {
				result = true;
				return RValue.ReduceEmptyAndNotAllowed (ref result, visited);
			} else if (RValue.PatternType == RelaxngPatternType.Empty) {
				result = true;
				return LValue.ReduceEmptyAndNotAllowed (ref result, visited);
			} else {
				LValue = LValue.ReduceEmptyAndNotAllowed (ref result, visited);
				RValue = RValue.ReduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		internal override void MarkReachableDefs () 
		{
			l.MarkReachableDefs ();
			r.MarkReachableDefs ();
		}

		internal override bool ContainsText()
		{
			return l.ContainsText () || r.ContainsText ();
		}

		// 7.3 (group/interleave attribute names) and
		// part of 7.4 (interleave element names)
		// FIXME: Actually it should be done against the correct
		// simplified grammar, expanding all refs.
		internal void CheckNameOverlap (bool checkElements)
		{
			if (RdpUtil.NamesOverlap (LValue, RValue, checkElements))
				throw new RelaxngException ("Duplicate attributes inside a group or an interleave is not allowed.");
			return;
		}
	}

	// Choice
	public class RdpChoice : RdpAbstractBinary
	{
		public RdpChoice (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get {
				if (!nullableComputed) {
					isNullable =
						LValue.Nullable || RValue.Nullable;
					nullableComputed = true;
				}
				return isNullable;
			}
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Choice; }
		}

		RdpContentType computedContentType = RdpContentType.Invalid;
		public override RdpContentType ContentType {
			get {
				if (computedContentType == RdpContentType.Invalid) {
					if (LValue.ContentType == RdpContentType.Simple ||
						RValue.ContentType == RdpContentType.Simple)
						computedContentType = RdpContentType.Simple;
					else
						computedContentType = base.ContentType;
				}
				return computedContentType;
			}
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			LValue.GetLabels (elements, attributes, collectNameClass);
			RValue.GetLabels (elements, attributes, collectNameClass);
		}


		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (LValue.PatternType == RelaxngPatternType.NotAllowed &&
				RValue.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else if (LValue.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return RValue.ReduceEmptyAndNotAllowed (ref result, visited);
			} else if (RValue.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return LValue.ReduceEmptyAndNotAllowed (ref result, visited);
			} else if (LValue.PatternType == RelaxngPatternType.Empty &&
				RValue.PatternType == RelaxngPatternType.Empty) {
				result = true;
				return RdpEmpty.Instance;
			} else if (RValue.PatternType == RelaxngPatternType.Empty) {
				result = true;
				RValue = LValue.ReduceEmptyAndNotAllowed (ref result, visited);
				LValue = RdpEmpty.Instance;
				return this;
			} else {
				LValue = LValue.ReduceEmptyAndNotAllowed (ref result, visited);
				RValue = RValue.ReduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			return LValue.TextDeriv (s, reader).Choice (RValue.TextDeriv (s, reader));
		}

		internal override RdpPattern TextOnlyDeriv ()
		{
			return MakeChoice (LValue.TextOnlyDeriv (),
				RValue.TextOnlyDeriv ());
		}

		internal override RdpPattern MixedTextDeriv (string s)
		{
			return MakeChoice (LValue.MixedTextDeriv (s),
				RValue.MixedTextDeriv (s));
		}

		public override RdpPattern ApplyAfter (RdpApplyAfterHandler handler)
		{
//			return handler (LValue).Choice (handler (RValue));
			return LValue.ApplyAfter (handler).Choice (RValue.ApplyAfter (handler));
		}

		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
#if UseStatic
			return RdpUtil.Choice (
				RdpUtil.StartTagOpenDeriv (LValue, qname),
				RdpUtil.StartTagOpenDeriv (RValue, qname));
#else
			RdpPattern lDeriv = LValue.StartTagOpenDeriv (name, ns);
			return lDeriv.Choice (RValue.StartTagOpenDeriv (name, ns));
#endif
		}

		// attDeriv cx (Choice p1 p2) att =
		//  choice (attDeriv cx p1 att) (attDeriv cx p2 att)
		public override RdpPattern AttDeriv (string name, string ns, string value, XmlReader reader)
		{
			return LValue.AttDeriv (name, ns, value, reader)
				.Choice (RValue.AttDeriv (name, ns, value, reader));
		}

		// startTagCloseDeriv (Choice p1 p2) =
		//  choice (startTagCloseDeriv p1) (startTagCloseDeriv p2)
		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ()
				.Choice (RValue.StartTagCloseDeriv ());
		}

		public override RdpPattern EndTagDeriv ()
		{
			return LValue.EndTagDeriv ().Choice (RValue.EndTagDeriv ());
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			LValue.CheckConstraints (attribute, oneOrMore, oneOrMoreGroup, oneOrMoreInterleave, list, dataExcept);
			RValue.CheckConstraints (attribute, oneOrMore, oneOrMoreGroup, oneOrMoreInterleave, list, dataExcept);
		}
	}

	// Interleave
	public class RdpInterleave : RdpAbstractBinary
	{
		public RdpInterleave (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get {
				if (!nullableComputed) {
					isNullable =
						LValue.Nullable && RValue.Nullable;
					nullableComputed = true;
				}
				return isNullable;
			}
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			LValue.GetLabels (elements, attributes, collectNameClass);
			RValue.GetLabels (elements, attributes, collectNameClass);
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (LValue.PatternType == RelaxngPatternType.NotAllowed ||
				RValue.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else {
				LValue = LValue.ReduceEmptyAndNotAllowed (ref result, visited);
				RValue = RValue.ReduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			return LValue.TextDeriv (s, reader).Interleave (RValue)
				.Choice (LValue.Interleave (RValue.TextDeriv (s, reader)));
		}

		internal override RdpPattern TextOnlyDeriv ()
		{
			return MakeInterleave (LValue.TextOnlyDeriv (),
				RValue.TextOnlyDeriv ());
		}

		internal override RdpPattern MixedTextDeriv (string s)
		{
			return MakeChoice (
				MakeInterleave (LValue.MixedTextDeriv (s), RValue),
				MakeInterleave (LValue, RValue.MixedTextDeriv (s)));
		}

		// => choice (applyAfter (flip interleave p2) (startTagOpenDeriv p1 qn)) (applyAfter (interleave p1) (startTagOpenDeriv p2 qn)
		// => p1.startTagOpenDeriv(qn).applyAfter (flip interleave p2).choice (p2.startTagOpenDeriv(qn).applyAfter (interleave p1) )
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern handledL = LValue.StartTagOpenDeriv (name, ns);
			RdpPattern handledR = RValue.StartTagOpenDeriv (name, ns);
			RdpFlip flipL = MakeFlip (RdpUtil.InterleaveFunction, RValue);
			RdpPattern choiceL = handledL.ApplyAfter (new RdpApplyAfterHandler (flipL.Apply));
			RdpPattern choiceR = handledR.ApplyAfter (new RdpApplyAfterHandler (LValue.Interleave));
			return choiceL.Choice (choiceR);
		}

		// attDeriv cx (Interleave p1 p2) att =
		//  choice (interleave (attDeriv cx p1 att) p2)
		//         (interleave p1 (attDeriv cx p2 att))
		public override RdpPattern AttDeriv (string name, string ns, string value, XmlReader reader)
		{
			return LValue.AttDeriv (name, ns, value, reader)
				.Interleave (RValue)
				.Choice (LValue.Interleave (
					RValue.AttDeriv (name, ns, value, reader)));
		}

		// startTagCloseDeriv (Interleave p1 p2) =
		//  interleave (startTagCloseDeriv p1) (startTagCloseDeriv p2)
		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ()
				.Interleave (RValue.StartTagCloseDeriv ());
		}

		/*
		// FIXME: This is not specified in James Clark's algorithm, so
		// this may raise unstable behaviour!!
		// I think this is right but not confident.
		
		// ... then I reminded to include it.
		public override RdpPattern EndTagDeriv ()
		{
			return LValue.Nullable ? RValue : RdpNotAllowed.Instance;
		}
		*/

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Interleave; }
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			if (list)
				throw new RelaxngException ("interleave is not allowed under a list.");
			if (dataExcept)
				throw new RelaxngException ("interleave is not allowed under except of a data.");

			// 7.1
			LValue.CheckConstraints (attribute, oneOrMore, oneOrMoreGroup, oneOrMore, list, dataExcept);
			RValue.CheckConstraints (attribute, oneOrMore, oneOrMoreGroup, oneOrMore, list, dataExcept);

			// unique name analysis - 7.3 and part of 7.4
			CheckNameOverlap (true);

			// (2) text/text prohibited
			if (LValue.ContainsText () && RValue.ContainsText ())
				throw new RelaxngException ("Both branches of the interleave contains a text pattern.");
		}
	}

	// Group
	public class RdpGroup : RdpAbstractBinary
	{
		public RdpGroup (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get {
				if (!nullableComputed) {
					isNullable =
						LValue.Nullable && RValue.Nullable;
					nullableComputed = true;
				}
				return isNullable;
			}
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			LValue.GetLabels (elements, attributes, collectNameClass);
			if (LValue.Nullable)
				RValue.GetLabels (elements, attributes, collectNameClass);
			else
				RValue.GetLabels (null, attributes, collectNameClass);
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			RdpPattern p = LValue.TextDeriv (s, reader).Group (RValue);
			return LValue.Nullable ?
				p.Choice (RValue.TextDeriv(s, reader)) : p;
		}

		internal override RdpPattern TextOnlyDeriv ()
		{
			return MakeGroup (LValue.TextOnlyDeriv (),
				RValue.TextOnlyDeriv ());
		}

		internal override RdpPattern MixedTextDeriv (string s)
		{
			RdpPattern p = MakeGroup (
				LValue.MixedTextDeriv (s), RValue);
			return LValue.Nullable ?
				MakeChoice (p, RValue.MixedTextDeriv (s)) : p;
		}

		// startTagOpenDeriv (Group p1 p2) qn =
		//  let x = applyAfter (flip group p2) (startTagOpenDeriv p1 qn)
		//  in if nullable p1 then
		//       choice x (startTagOpenDeriv p2 qn)
		//     else
		//       x
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern handled = LValue.StartTagOpenDeriv (name, ns);
			RdpFlip f = MakeFlip (RdpUtil.GroupFunction, RValue);
			RdpPattern x = handled.ApplyAfter (new RdpApplyAfterHandler (f.Apply));
			if (LValue.Nullable)
				return x.Choice (RValue.StartTagOpenDeriv (name, ns));
			else
				return x;
		}

		// attDeriv cx (Group p1 p2) att =
		//  choice (group (attDeriv cx p1 att) p2)
		//         (group p1 (attDeriv cx p2 att))
		public override RdpPattern AttDeriv (string name, string ns, string value, XmlReader reader)
		{
			return LValue.AttDeriv (name, ns, value, reader).Group (RValue)
				.Choice (LValue.Group (
					RValue.AttDeriv (name, ns, value, reader)));
		}

		// startTagCloseDeriv (Group p1 p2) =
		//  group (startTagCloseDeriv p1) (startTagCloseDeriv p2)
		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ()
				.Group (RValue.StartTagCloseDeriv ());
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Group; }
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			if (dataExcept)
				throw new RelaxngException ("interleave is not allowed under except of a data.");

			LValue.CheckConstraints (attribute, oneOrMore, oneOrMore, oneOrMoreInterleave, list, dataExcept);
			RValue.CheckConstraints (attribute, oneOrMore, oneOrMore, oneOrMoreInterleave, list, dataExcept);

			// 7.3
			CheckNameOverlap (false);
		}
	}

	public abstract class RdpAbstractSingleContent : RdpPattern
	{
		RdpPattern child;
		bool isExpanded;

		internal override RdpPattern ExpandRef (Hashtable defs)
		{
			if (!isExpanded)
				child = child.ExpandRef (defs);
			return this;
		}

		public RdpAbstractSingleContent (RdpPattern p)
		{
			this.child = p;
		}

		public RdpPattern Child {
			get { return child; }
			set { child = value; }
		}

		internal override void MarkReachableDefs () 
		{
			child.MarkReachableDefs ();
		}

		internal override bool ContainsText()
		{
			return child.ContainsText ();
		}
	}

	// OneOrMore
	public class RdpOneOrMore : RdpAbstractSingleContent
	{
		public RdpOneOrMore (RdpPattern p) : base (p)
		{
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.OneOrMore; }
		}

		public override RdpContentType ContentType {
			get {
				if (Child.ContentType == RdpContentType.Simple)
					throw new RelaxngException ("Invalid content type was found.");
				return Child.ContentType;
			}
		}

		public override bool Nullable {
			get { return Child.Nullable; }
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			Child.GetLabels (elements, attributes, collectNameClass);
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (Child.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else if (Child.PatternType == RelaxngPatternType.Empty)
				return RdpEmpty.Instance;
			else {
				Child = Child.ReduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			return Child.TextDeriv (s, reader).Group (Choice (RdpEmpty.Instance));
		}

		internal override RdpPattern TextOnlyDeriv ()
		{
			return MakeOneOrMore (Child.TextOnlyDeriv ());
		}

		internal override RdpPattern MixedTextDeriv (string s)
		{
			return MakeGroup (Child.MixedTextDeriv (s),
				MakeChoice (this, RdpEmpty.Instance));
		}

		// attDeriv cx (OneOrMore p) att =
		//  group (attDeriv cx p att) (choice (OneOrMore p) Empty)
		public override RdpPattern AttDeriv (string name, string ns, string value, XmlReader reader)
		{
#if UseStatic
			return RdpUtil.Group (
				RdpUtil.AttDeriv (ctx, children, att),
				RdpUtil.Choice (RdpUtil.OneOrMore (children), RdpEmpty.Instance));
#else
			return Child.AttDeriv (name, ns, value, reader)
				.Group (Choice (RdpEmpty.Instance));
#endif
		}

		// startTagOpenDeriv (OneOrMore p) qn =
		//  applyAfter (flip group (choice (OneOrMore p) Empty))
		//             (startTagOpenDeriv p qn)
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern rest = RdpEmpty.Instance.Choice (Child.OneOrMore ());
			RdpPattern handled = Child.StartTagOpenDeriv (name, ns);
			RdpFlip f = MakeFlip (RdpUtil.GroupFunction, rest);
			return handled.ApplyAfter (new RdpApplyAfterHandler (f.Apply));
		}

		// startTagCloseDeriv (OneOrMore p) =
		//  oneOrMore (startTagCloseDeriv p)
		public override RdpPattern StartTagCloseDeriv ()
		{
#if UseStatic
			return RdpUtil.OneOrMore (
				RdpUtil.StartTagCloseDeriv (children));
#else
			return Child.StartTagCloseDeriv ().OneOrMore ();
#endif
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			if (dataExcept)
				throw new RelaxngException ("oneOrMore is not allowed under except of a data.");
			this.Child.CheckConstraints (attribute, true, oneOrMoreGroup, oneOrMoreInterleave, list, dataExcept);
		}
	}

	// List
	public class RdpList : RdpAbstractSingleContent
	{
		public RdpList (RdpPattern p) : base (p)
		{
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (Child.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else {
				Child = Child.ReduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

/*
		// This is not written in James Clark's derivative algorithm
		// ( http://www.thaiopensource.com/relaxng/derivative.html ),
		// but it looks required.
		public override bool Nullable {
			get { return this.Child.Nullable; }
		}
		
		// ... but it also causes different error:
		// <list><group><data .../><data .../></group></list>
*/
		public override bool Nullable {
			get { return false; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.List; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Simple; }
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			Child.GetLabels (elements, attributes, collectNameClass);
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			RdpPattern p = Child.ListDeriv (Util.NormalizeWhitespace (s).Split (RdpUtil.WhitespaceChars), 0, reader);			
			if (p.Nullable)
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			if (list)
				throw new RelaxngException ("list is not allowed uner another list.");
			if (dataExcept)
				throw new RelaxngException ("list is not allowed under except of a data.");
			this.Child.CheckConstraints (attribute, oneOrMore, oneOrMoreGroup, oneOrMoreInterleave, true, dataExcept);
		}
	}

	// Data
	public class RdpData : RdpPattern
	{
		public RdpData (RdpDatatype dt)
		{
			this.dt = dt;
		}

		RdpDatatype dt;
		public RdpDatatype Datatype {
			get { return dt; }
		}

		// This is not written in James Clark's derivative algorithm
		// ( http://www.thaiopensource.com/relaxng/derivative.html ),
		// but it looks required.
		public override bool Nullable {
			get {
				if (dt.NamespaceURI.Length == 0)
					return true;
				return false; 
			}
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Data; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Simple; }
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			// do nothing.
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			if (dt.IsAllowed (s, reader))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
		}

		internal override void MarkReachableDefs () 
		{
			// do nothing
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			// do nothing
		}

		internal override bool ContainsText()
		{
			return false;
		}
	}

	// DataExcept
	public class RdpDataExcept : RdpData
	{
		public RdpDataExcept (RdpDatatype dt, RdpPattern except)
			: base (dt)
		{
			this.except = except;
		}

		RdpPattern except;
		public RdpPattern Except {
			get { return except; }
			set { except = value; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.DataExcept; }
		}

		public override RdpContentType ContentType {
			get {
				RdpContentType c = except.ContentType; // conformance required for except pattern.
				return RdpContentType.Simple;
			}
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (except.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return new RdpData (this.Datatype);
			} else {
				except = except.ReduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			if (Datatype.IsAllowed (s, reader) && !except.TextDeriv (s, reader).Nullable)
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept)
		{
			this.except.CheckConstraints (attribute, oneOrMore, oneOrMoreGroup, oneOrMoreInterleave, list, true);
		}

		internal override bool ContainsText()
		{
			return except.ContainsText ();
		}
	}

	// Value
	public class RdpValue : RdpPattern
	{
		public RdpValue (RdpDatatype dt, string value)
		{
			this.dt = dt;
			this.value = value;
		}

		RdpDatatype dt;
		public RdpDatatype Datatype {
			get { return dt; }
		}

		string value;
		public string Value {
			get { return value; }
		}

		// This is not written in James Clark's derivative algorithm
		// ( http://www.thaiopensource.com/relaxng/derivative.html ),
		// but it looks required.
		public override bool Nullable {
			get {
				if (dt.NamespaceURI.Length == 0 && value.Length == 0)
					return true;
				return false; 
			}
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Value; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Simple; }
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			// do nothing
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			if (dt.IsTypeEqual (value, s, reader))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
		}

		internal override void MarkReachableDefs () 
		{
			// do nothing
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept) 
		{
			// nothing to be checked
		}

		internal override bool ContainsText()
		{
			return false;
		}
	}

	// Attribute
	public class RdpAttribute : RdpPattern
	{
		public RdpAttribute (RdpNameClass nameClass, RdpPattern p)
		{
			this.nameClass = nameClass;
			this.children = p;
		}

		RdpNameClass nameClass;
		public RdpNameClass NameClass {
			get { return nameClass; }
		}

		RdpPattern children;
		public RdpPattern Children {
			get { return children; }
			set { children = value; }
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Attribute; }
		}

		public override RdpContentType ContentType {
			get { return RdpContentType.Empty; }
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			if (attributes != null) {
				if (collectNameClass)
					attributes [NameClass] = NameClass;
				else
					AddNameLabel (attributes, NameClass);
			}
		}

		bool isExpanded;
		internal override RdpPattern ExpandRef (Hashtable defs)
		{
			if (!isExpanded) {
				isExpanded = true;
				children = children.ExpandRef (defs);
			}
			return this;
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (children.PatternType == RelaxngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else {
				children = children.ReduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		// attDeriv cx (Attribute nc p) (AttributeNode qn s) =
		//  if contains nc qn && valueMatch cx p s then Empty else NotAllowed
		public override RdpPattern AttDeriv (string name, string ns, string value, XmlReader reader)
		{
			// If value is null, then does not check ValueMatch.
#if UseStatic
			if (RdpUtil.Contains (this.nameClass, att.QName)
				&& (value == null || RdpUtil.ValueMatch (ctx, this.children, att.Value)))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
#else
			if (nameClass.Contains (name, ns) &&
				(value == null || children.ValueMatch (value, reader)))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
#endif
		}

		// startTagCloseDeriv (Attribute _ _) = NotAllowed
		public override RdpPattern StartTagCloseDeriv ()
		{
			return RdpNotAllowed.Instance;
		}

		internal override void MarkReachableDefs () 
		{
			children.MarkReachableDefs ();
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept) 
		{
			// 7.1.1 and 7.1.2
			if (attribute || oneOrMoreGroup || oneOrMoreInterleave || list || dataExcept)
				throw new RelaxngException ("Not allowed attribute occurence was specified in the pattern.");

			// latter part of 7.3
			if (!oneOrMore && NameClass.HasInfiniteName)
				throw new RelaxngException ("Attributes that has an infinite name class must be repeatable.");

			this.Children.CheckConstraints (true, oneOrMore, false, false, false, false);
		}

		internal override bool ContainsText()
		{
			// This method is to detect text pattern inside interleave child.
			// return children.ContainsText ();
			return false;
		}
	}

	// Element
	public class RdpElement : RdpPattern
	{
		public RdpElement (RdpNameClass nameClass, RdpPattern p)
		{
			this.nameClass = nameClass;
			this.children = p;
		}

		RdpNameClass nameClass;
		public RdpNameClass NameClass {
			get { return nameClass; }
		}

		RdpPattern children;
		public RdpPattern Children {
			get { return children; }
			set { children = value; }
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.Element; }
		}

		bool contentTypeCheckDone;
		public override RdpContentType ContentType {
			get {
				if (!contentTypeCheckDone) {
					contentTypeCheckDone = true;
					RdpContentType ct = children.ContentType; // conformance required.
				}
				return RdpContentType.Complex;
			}
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			if (elements != null) {
				if (collectNameClass)
					elements [NameClass] = NameClass;
				else
					AddNameLabel (elements, NameClass);
			}
		}


		bool isExpanded;
		short expanding; // FIXME: It is totally not required, but there is
		// some bugs in simplification and without it it causes infinite loop.
		internal override RdpPattern ExpandRef (Hashtable defs)
		{
			if (!isExpanded) {
				isExpanded = true;
				if (expanding == 100)
					throw new RelaxngException (String.Format ("Invalid recursion was found. Name is {0}", nameClass));
				expanding++;
				children = children.ExpandRef (defs);
				expanding--;
			}
			return this;
		}

		internal override RdpPattern ReduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			children = children.ReduceEmptyAndNotAllowed (ref result, visited);
			return this;
		}

		internal override RdpPattern TextOnlyDeriv ()
		{
			return RdpNotAllowed.Instance;
		}

		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
#if UseStatic
			if (RdpUtil.Contains (this.nameClass, qname))
				return RdpUtil.After (this.Children, RdpEmpty.Instance);
			else
				return RdpNotAllowed.Instance;
#else
			return nameClass.Contains (name, ns) ?
				children.After (RdpEmpty.Instance) :
				RdpNotAllowed.Instance;
#endif
		}

		internal override void MarkReachableDefs () 
		{
			children.MarkReachableDefs ();
		}

		bool constraintsChecked;
		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept) 
		{
			if (constraintsChecked)
				return;
			constraintsChecked = true;
			if (attribute || list || dataExcept)
				throw new RelaxngException ("Not allowed element occurence was specified in the pattern.");
			this.Children.CheckConstraints (false, oneOrMore, oneOrMoreGroup, oneOrMoreInterleave, false, false);
		}

		internal override bool ContainsText()
		{
			return children.ContainsText ();
		}
	}

	// After
	public class RdpAfter : RdpAbstractBinary
	{
		public RdpAfter (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get { return false; }
		}

		public override void GetLabels (LabelList elements, LabelList attributes, bool collectNameClass)
		{
			LValue.GetLabels (elements, attributes, collectNameClass);
		}

		public override RdpPattern TextDeriv (string s, XmlReader reader)
		{
			return LValue.TextDeriv (s, reader).After (RValue);
		}

		internal override RdpPattern TextOnlyDeriv ()
		{
			return MakeAfter (LValue.TextOnlyDeriv (), RValue);
		}

		internal override RdpPattern MixedTextDeriv (string s)
		{
			return MakeAfter (LValue.MixedTextDeriv (s), RValue);
		}

		// startTagOpenDeriv (After p1 p2) qn =
		//   applyAfter (flip after p2) (startTagOpenDeriv p1 qn)
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern handled = LValue.StartTagOpenDeriv (name, ns);
			RdpFlip f = MakeFlip (RdpUtil.AfterFunction, RValue);
			return handled.ApplyAfter (new RdpApplyAfterHandler (
				f.Apply));
		}

		public override RdpPattern ApplyAfter (RdpApplyAfterHandler handler)
		{
			return LValue.After (handler (RValue));
		}

		// attDeriv cx (After p1 p2) att =
		//  after (attDeriv cx p1 att) p2
		public override RdpPattern AttDeriv (string name, string ns, string value, XmlReader reader)
		{
			return LValue.AttDeriv (name, ns, value, reader).After (RValue);
		}

		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ().After (RValue);
		}

		public override RdpPattern EndTagDeriv ()
		{
			return LValue.Nullable ? RValue : RdpNotAllowed.Instance;
		}

		public override RelaxngPatternType PatternType {
			get { return RelaxngPatternType.After; }
		}

		internal override void MarkReachableDefs () 
		{
			throw new InvalidOperationException ();
		}

		internal override void CheckConstraints (bool attribute, bool oneOrMore, bool oneOrMoreGroup, bool oneOrMoreInterleave, bool list, bool dataExcept) 
		{
			throw new InvalidOperationException ();
		}

		internal override bool ContainsText ()
		{
			throw new InvalidOperationException ();
		}
	}
}

