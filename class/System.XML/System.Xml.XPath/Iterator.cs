//
// System.Xml.XPath.BaseIterator
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

namespace System.Xml.XPath
{
/*
	internal abstract class XPathNodeIterator : ICloneable
	{
		private int _count;

		#region Constructor

		protected XPathNodeIterator () {}

		#endregion

		#region Properties

		public virtual int Count
		{
			get
			{
				if (_count == 0)
				{
					// compute and cache the count
					XPathNodeIterator tmp = Clone ();
					while (tmp.MoveNext ())
						;
					_count = tmp.CurrentPosition;
				}
				return _count;
			}
		}

		public abstract XPathNavigator Current { get; }

		public abstract int CurrentPosition { get; }

		#endregion

		#region Methods

		public abstract XPathNodeIterator Clone ();

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		public abstract bool MoveNext ();

		#endregion
	}
*/

	internal abstract class BaseIterator : XPathNodeIterator
	{
		private XsltContext _context;

		internal BaseIterator (BaseIterator other)
		{
			_context = other._context;
		}
		internal BaseIterator (XsltContext context)
		{
			_context = context;
		}

		public XsltContext Context { get { return _context; } }
		
		public override string ToString ()
		{
			return Current.NodeType.ToString () + "[" + CurrentPosition + "] : " + Current.Name + " = " + Current.Value;
		}
	}

	internal class UnionIterator : BaseIterator
	{
		protected ArrayList _iters = new ArrayList ();
		protected int _pos;
		protected int _index;

		public UnionIterator (BaseIterator iter ) : base (iter) {}
		protected UnionIterator (UnionIterator other) : base (other)
		{
			foreach (object obj in other._iters)
				_iters.Add (obj);
			_pos = other._pos;
			_index = other._index;
		}
		public override XPathNodeIterator Clone () { return new UnionIterator (this); }

		public void Add (BaseIterator iter)
		{
			_iters.Add (iter);
		}

		public override bool MoveNext ()
		{
			while (_index < _iters.Count)
			{
				BaseIterator iter = (BaseIterator) _iters [_index];
				if (iter.MoveNext ())
				{
					_pos ++;
					return true;
				}
				_index ++;
			}
			return false;
		}
		public override XPathNavigator Current
		{
			get
			{
				if (_index >= _iters.Count)
					return null;
				BaseIterator iter = (BaseIterator) _iters [_index];
				return iter.Current;
			}
		}
		public override int CurrentPosition { get { return _pos; }}
	}

	internal abstract class SimpleIterator : BaseIterator
	{
		protected XPathNavigator _nav;
		protected int _pos;

		public SimpleIterator (BaseIterator iter) : base (iter)
		{
			_nav = iter.Current.Clone ();
		}
		protected SimpleIterator (SimpleIterator other) : base (other)
		{
			_nav = other._nav.Clone ();
			_pos = other._pos;
		}
		public SimpleIterator (XPathNavigator nav, XsltContext context) : base (context)
		{
			_nav = nav.Clone ();
		}

		public override XPathNavigator Current { get { return _nav; }}
		public override int CurrentPosition { get { return _pos; }}
	}

	internal class SelfIterator : SimpleIterator
	{
		public SelfIterator (BaseIterator iter) : base (iter) {}
		public SelfIterator (XPathNavigator nav, XsltContext context) : base (nav, context) {}
		protected SelfIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new SelfIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				_pos = 1;
				return true;
			}
			return false;
		}
	}

	internal class ParentIterator : SimpleIterator
	{
		public ParentIterator (BaseIterator iter) : base (iter) {}
		protected ParentIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new ParentIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0 && _nav.MoveToParent ())
			{
				_pos = 1;
				return true;
			}
			return false;
		}
	}

	internal class ChildIterator : SimpleIterator
	{
		public ChildIterator (BaseIterator iter) : base (iter) {}
		protected ChildIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new ChildIterator (this); }
		public override bool MoveNext ()
		{
			bool fSuccess = (_pos == 0) ? _nav.MoveToFirstChild () : _nav.MoveToNext ();
			if (fSuccess)
				_pos ++;
			return fSuccess;
		}
	}

	internal class FollowingSiblingIterator : SimpleIterator
	{
		public FollowingSiblingIterator (BaseIterator iter) : base (iter) {}
		protected FollowingSiblingIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new FollowingSiblingIterator (this); }
		public override bool MoveNext ()
		{
			if (_nav.MoveToNext ())
			{
				_pos ++;
				return true;
			}
			return false;
		}
	}

	internal class PrecedingSiblingIterator : SimpleIterator
	{
		public PrecedingSiblingIterator (BaseIterator iter) : base (iter) {}
		protected PrecedingSiblingIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new PrecedingSiblingIterator (this); }
		public override bool MoveNext ()
		{
			if (_nav.MoveToPrevious ())
			{
				_pos ++;
				return true;
			}
			return false;
		}
	}

	internal class AncestorIterator : SimpleIterator
	{
		public AncestorIterator (BaseIterator iter) : base (iter) {}
		protected AncestorIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new AncestorIterator (this); }
		public override bool MoveNext ()
		{
			if (_nav.MoveToParent ())
			{
				_pos ++;
				return true;
			}
			return false;
		}
	}

	internal class AncestorOrSelfIterator : UnionIterator
	{
		public AncestorOrSelfIterator (BaseIterator iter) : base (iter)
		{
			Add (new SelfIterator (iter));
			Add (new AncestorIterator (iter));
		}
		protected AncestorOrSelfIterator (UnionIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new AncestorOrSelfIterator (this); }
	}

	internal class DescendantIterator : SimpleIterator
	{
		protected int _depth;
		public DescendantIterator (BaseIterator iter) : base (iter) {}
		protected DescendantIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new DescendantIterator (this); }
		[MonoTODO]
		public override bool MoveNext ()
		{
			if (_nav.MoveToFirstChild ())
			{
				_depth ++;
				_pos ++;
				return true;
			}
			while (_depth != 0)
			{
				if (_nav.MoveToNext ())
				{
					_pos ++;
					return true;
				}
				if (!_nav.MoveToParent ())	// should NEVER fail!
					throw new Exception ("unexpected depth");	// TODO: better message
				_depth --;
			}
			return false;
		}
	}

	internal class DescendantOrSelfIterator : UnionIterator
	{
		public DescendantOrSelfIterator (BaseIterator iter) : base (iter)
		{
			Add (new SelfIterator (iter));
			Add (new DescendantIterator (iter));
		}
		protected DescendantOrSelfIterator (UnionIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new DescendantOrSelfIterator (this); }
	}

	internal class FollowingIterator : SimpleIterator
	{
		public FollowingIterator (BaseIterator iter) : base (iter) {}
		protected FollowingIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new FollowingIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				if (_nav.MoveToNext ())
				{
					_pos ++;
					return true;
				}
			}
			else
			{
				if (_nav.MoveToFirstChild ())
				{
					_pos ++;
					return true;
				}
				do
				{
					if (_nav.MoveToNext ())
					{
						_pos ++;
						return true;
					}
				}
				while (_nav.MoveToParent ());
			}
			return false;
		}
	}

	internal class PrecedingIterator : SimpleIterator
	{
		public PrecedingIterator (BaseIterator iter) : base (iter) {}
		protected PrecedingIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new PrecedingIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				if (_nav.MoveToPrevious ())
				{
					_pos ++;
					return true;
				}
			}
			else
			{
				if (_nav.MoveToFirstChild ())
				{
					while (_nav.MoveToNext ())
						;
					_pos ++;
					return true;
				}
				do
				{
					if (_nav.MoveToPrevious ())
					{
						_pos ++;
						return true;
					}
				}
				while (_nav.MoveToParent ());
			}
			return false;
		}
	}

	internal class NamespaceIterator : SimpleIterator
	{
		public NamespaceIterator (BaseIterator iter) : base (iter) {}
		protected NamespaceIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new NamespaceIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				if (_nav.MoveToFirstNamespace ())
				{
					_pos ++;
					return true;
				}
			}
			else if (_nav.MoveToNextNamespace ())
			{
				_pos ++;
				return true;
			}
			return false;
		}
	}

	internal class AttributeIterator : SimpleIterator
	{
		public AttributeIterator (BaseIterator iter) : base (iter) {}
		protected AttributeIterator (SimpleIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new AttributeIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				if (_nav.MoveToFirstAttribute ())
				{
					_pos += 1;
					return true;
				}
			}
			else if (_nav.MoveToNextAttribute ())
			{
				_pos ++;
				return true;
			}
			return false;			
		}
	}

	internal class AxisIterator : BaseIterator
	{
		protected BaseIterator _iter;
		protected NodeTest _test;
		protected int _pos;

		public AxisIterator (BaseIterator iter, NodeTest test) : base (iter)
		{
			_iter = iter;
			_test = test;
		}

		protected AxisIterator (AxisIterator other) : base (other)
		{
			_iter = (BaseIterator) other._iter.Clone ();
			_test = other._test;
			_pos = other._pos;
		}
		public override XPathNodeIterator Clone () { return new AxisIterator (this); }

		public override bool MoveNext ()
		{
			while (_iter.MoveNext ())
			{
				if (_test.Match (Context, Current))
				{
					_pos ++;
					return true;
				}
			}
			return false;
		}
		public override XPathNavigator Current { get { return _iter.Current; }}
		public override int CurrentPosition { get { return _pos; }}
	}

	internal class SlashIterator : BaseIterator
	{
		protected BaseIterator _iterLeft;
		protected BaseIterator _iterRight;
		protected Expression _expr;
		protected int _pos;

		public SlashIterator (BaseIterator iter, Expression expr) : base (iter)
		{
			_iterLeft = iter;
			_expr = expr;
		}

		protected SlashIterator (SlashIterator other) : base (other)
		{
			_iterLeft = (BaseIterator) other._iterLeft.Clone ();
			_iterRight = (BaseIterator) other._iterRight.Clone ();
			_expr = other._expr;
			_pos = other._pos;
		}
		public override XPathNodeIterator Clone () { return new SlashIterator (this); }

		public override bool MoveNext ()
		{
			while (_iterRight == null || !_iterRight.MoveNext ())
			{
				if (!_iterLeft.MoveNext ())
					return false;
				_iterRight = _expr.EvaluateNodeSet (_iterLeft);
			}
			_pos ++;
			return true;
		}
		public override XPathNavigator Current { get { return _iterRight.Current; }}
		public override int CurrentPosition { get { return _pos; }}
	}
	internal class PredicateIterator : BaseIterator
	{
		protected BaseIterator _iter;
		protected Expression [] _preds;
		protected int _pos;

		public PredicateIterator (BaseIterator iter, Expression [] preds) : base (iter)
		{
			_iter = iter;
			_preds = preds;
		}

		protected PredicateIterator (PredicateIterator other) : base (other)
		{
			_iter = (BaseIterator) other._iter.Clone ();
			_preds = other._preds;
			_pos = other._pos;
		}
		public override XPathNodeIterator Clone () { return new PredicateIterator (this); }

		public override bool MoveNext ()
		{
			while (_iter.MoveNext ())
			{
				bool fTrue = true;
				foreach (Expression pred in _preds)
				{
					if (!pred.EvaluateBoolean ((BaseIterator) _iter.Clone ()))
					{
						fTrue = false;
						break;
					}
				}
				if (fTrue)
					return true;
			}
			return false;
		}
		public override XPathNavigator Current { get { return _iter.Current; }}
		public override int CurrentPosition { get { return _pos; }}
	}
	internal class ArrayListIterator : BaseIterator
	{
		protected ArrayList _rgNodes;
		protected int _pos;

		public ArrayListIterator (BaseIterator iter, ArrayList rgNodes) : base (iter)
		{
			_rgNodes = rgNodes;
		}

		protected ArrayListIterator (ArrayListIterator other) : base (other)
		{
			_rgNodes = other._rgNodes;
		}
		public override XPathNodeIterator Clone () { return new ArrayListIterator (this); }

		public override bool MoveNext ()
		{
			if (_pos >= _rgNodes.Count)
				return false;
			_pos++;
			return true;
		}
		public override XPathNavigator Current { get { return (XPathNavigator) _rgNodes [_pos - 1]; }}
		public override int CurrentPosition { get { return _pos; }}
	}
}
