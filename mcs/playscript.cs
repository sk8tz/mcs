//
// playscript.cs: PlayScript expressions and support
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or Apache License, Version 2.0
//
// Copyright 2013 Zynga Inc.
// Copyright 2013 Xamarin Inc
//

using System;
using System.Collections.Generic;
using Mono.CSharp;

#if STATIC
using IKVM.Reflection;
using IKVM.Reflection.Emit;
#else
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace Mono.PlayScript
{
	public abstract class PlayScriptExpression : Expression
	{
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ("Expression trees conversion not implemented in PlayScript");
		}
	}

	public abstract class CollectionInitialization : PlayScriptExpression
	{
		protected Expression ctor;
		protected TemporaryVariableReference instance;
		protected List<Invocation> inserts;

		protected CollectionInitialization (ArrayInitializer initializer)
		{
			this.Initializer = initializer;
			loc = Initializer.Location;
		}

		public ArrayInitializer Initializer { get; private set; }

		protected List<Invocation> ResolveInitializations (ResolveContext rc, Expression instance, MethodSpec pushMethod)
		{
			List<Invocation> all = new List<Invocation> (Initializer.Count);
			foreach (var expr in Initializer.Elements) {

				var call_args = new Arguments (1);
				call_args.Add (new Argument (expr));

				var mg = MethodGroupExpr.CreatePredefined (pushMethod, rc.CurrentType, loc);
				mg.InstanceExpression = instance;

				var inv = new Invocation (mg, call_args);
				inv.Resolve (rc);

				all.Add (inv);
			}

			return all;
		}

		public override void Emit (EmitContext ec)
		{
			if (instance != null) {
				instance.EmitAssign (ec, ctor);
				foreach (var insert in inserts)
					insert.EmitStatement (ec);

				instance.EmitLoad (ec);
			} else {
				ctor.Emit (ec);
			}
		}
	}

	public class ArrayCreation : CollectionInitialization
	{
		public ArrayCreation (ArrayInitializer initializer)
			: base (initializer)
		{
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = rc.Module.PlayscriptTypes.Array.Resolve ();
			if (type == null)
				return null;

			var count = Initializer.Elements == null ? 0 : Initializer.Count;

			var ctor_args = new Arguments (1);
			ctor_args.Add (new Argument (new IntLiteral (rc.BuiltinTypes, count, loc)));

			ctor = new New (new TypeExpression (type, loc), ctor_args, loc).Resolve (rc);

			if (count != 0) {
				instance = TemporaryVariableReference.Create (type, rc.CurrentBlock, loc);

				var push = rc.Module.PlayScriptMembers.ArrayPush.Resolve (loc);
				if (push == null)
					return null;

				inserts = ResolveInitializations (rc, instance, push);
			}

			eclass = ExprClass.Value;
			return this;
		}


		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	class BinaryOperators
	{
		public static Expression ResolveOperator (ResolveContext rc, Binary op, Expression left, Expression right)
		{
			string method, oper;
			switch (op.Oper) {
			case Binary.Operator.Equality:
				oper = "Equality";
				method = "Comparison";
				break;
			case Binary.Operator.Inequality:
				oper = "Inequality";
				method = "Comparison";
				break;
			case Binary.Operator.GreaterThan:
				oper = "GreaterThan";
				method = "Comparison";
				break;
			case Binary.Operator.GreaterThanOrEqual:
				oper = "GreaterThanOrEqual";
				method = "Comparison";
				break;
			case Binary.Operator.LessThan:
				oper = "LessThan";
				method = "Comparison";
				break;
			case Binary.Operator.LessThanOrEqual:
				oper = "LessThanOrEqual";
				method = "Comparison";
				break;
			default:
				throw new NotImplementedException ();
			}

			var loc = op.Location;

			var ps = new MemberAccess (new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "PlayScript", loc), "Runtime", loc);

			var args = new Arguments (3);
			args.Add (new Argument (new MemberAccess (new MemberAccess (ps, "BinaryOperator", loc), oper, loc)));
			args.Add (new Argument (left));
			args.Add (new Argument (right));


			//
			// ActionScript does not really care about types for this for example following cases are all valid
			// 1.0 == 1
			// "3" > null
			// We defer to runtime to do the complex coercion
			//
			return new Invocation (new MemberAccess (new TypeExpression (rc.Module.PlayscriptTypes.Operations.Resolve (), loc), method, loc), args).Resolve (rc);
		}
	}

	public class NewVector : CollectionInitialization
	{
		FullNamedExpression elementType;

		public NewVector (FullNamedExpression elementType, ArrayInitializer initializer, Location loc)
			: base (initializer)
		{
			this.elementType = elementType;
			this.loc = loc;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			var element = elementType.ResolveAsType (rc);
			type = rc.Module.PlayscriptTypes.Vector.Resolve ();
			if (type == null || element == null)
				return null;

			type = type.MakeGenericType (rc, new [] { element });

			var count = Initializer.Elements == null ? 0 : Initializer.Count;

			var ctor_args = new Arguments (1);
			ctor_args.Add (new Argument (new IntLiteral (rc.BuiltinTypes, count, loc)));

			ctor = new New (new TypeExpression (type, loc), ctor_args, loc).Resolve (rc);

			if (count != 0) {
				instance = TemporaryVariableReference.Create (type, rc.CurrentBlock, loc);

				var push = rc.Module.PlayScriptMembers.VectorPush.Resolve (loc);
				if (push == null)
					return null;

				push = MemberCache.GetMember (type, push);
	
				inserts = ResolveInitializations (rc, instance, push);
			}

			eclass = ExprClass.Value;
			return this;
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class UntypedTypeExpression : TypeExpr
	{
		public UntypedTypeExpression (Location loc)
		{
			this.loc = loc;
		}

		public override TypeSpec ResolveAsType (IMemberContext mc)
		{
			//
			// An untyped variable is not the same as a variable of type Object.
			// The key difference is that untyped variables can hold the special value
			// undefined, while a variable of type Object cannot hold that value.
			// Also any conversion is done at runtime.
			//
			return mc.Module.Compiler.BuiltinTypes.Dynamic;
		}
	}

	public class ObjectInitializer : PlayScriptExpression
	{
		public ObjectInitializer (List<Expression> initializer, Location loc)
		{
			Initializer = initializer;
			this.loc = loc;
		}

		public List<Expression> Initializer { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			var type = new TypeExpression (rc.Module.PlayscriptTypes.Object, loc);

			var expr = Initializer == null ?
				new New (type, null, loc) :
				new NewInitialize (type, null, new CollectionOrObjectInitializers (Initializer, loc), loc);
			
			return expr.Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class SuperBaseInitializer : CSharp.ConstructorBaseInitializer
	{
		public SuperBaseInitializer (Arguments args, Location loc)
			: base (args, loc)
		{
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			// TODO: PS1201: A super statement cannot occur after a this, super, return, or throw statement.

			return base.DoResolve (ec);
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class Delete : ExpressionStatement
	{
		public Delete (Expression expr, Location l)
		{
			this.Expression = expr;
			loc = l;
		}

		public Expression Expression { get; private set; }

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ("Expression trees conversion not implemented in PlayScript");
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			var expr = Expression.Resolve (rc);

			var dcma = expr as DynamicClassMemberAccess;
			if (dcma != null) {
				var ms = rc.Module.PlayScriptMembers.BinderDeleteProperty.Resolve (loc);
				if (ms == null)
					return null;

				var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
				var call_args = new Arguments (2);
				call_args.Add (new Argument (dcma.Instance));
				call_args.Add (dcma.Arguments [0]);

				return new Invocation (mg, call_args).Resolve (rc);
			}

			//
			// Fixed properties cannot be deleted but it can be used with delete operator
			//
			var pe = expr as PropertyExpr;
			if (pe != null) {
				rc.Report.WarningPlayScript (3601, loc, "The declared property `{0}' cannot be deleted. To free associated memory, set its value to null",
					pe.GetSignatureForError ());

				expr = new BoolConstant (rc.BuiltinTypes, false, loc);
				return expr.Resolve (rc);
			}

			Expression = expr;
			eclass = ExprClass.Value;
			type = rc.BuiltinTypes.Bool;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Expression.Emit (ec);

			// Always returns true
			ec.EmitInt (1);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Expression.Emit (ec);
			if (Expression.Type.Kind != MemberKind.Void)
				ec.Emit (OpCodes.Pop);
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class TypeOf : PlayScriptExpression
	{		
		public TypeOf (Expression expr, Location l)
		{
			Expression = expr;
			loc = l;
		}

		public Expression Expression { get; private set; }
				
		protected override Expression DoResolve (ResolveContext rc)
		{
			var expr = Expression.Resolve (rc);
			if (expr == null)
				return null;

			var ms = rc.Module.PlayScriptMembers.OperationsTypeof.Resolve (loc);
			if (ms == null)
				return null;

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (1);
			call_args.Add (new Argument (expr));

			return new Invocation (mg, call_args).Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class RestArrayParameter : Parameter
	{
		PredefinedAttribute attr;

		public RestArrayParameter (string name, Attributes attrs, Location loc)
			: base (null, name, Modifier.RestArray, attrs, loc)
		{
		}

		public override TypeSpec Resolve (IMemberContext rc, int index)
		{
			TypeExpression = new TypeExpression (rc.Module.PlayscriptTypes.Array.Resolve (), Location);
			attr = rc.Module.PlayscriptAttributes.RestArrayParameter;
			attr.Define ();

			return base.Resolve (rc, index);
		}

		public override void ApplyAttributes (MethodBuilder mb, ConstructorBuilder cb, int index, CSharp.PredefinedAttributes pa)
		{
			base.ApplyAttributes (mb, cb, index, pa);

			attr.EmitAttribute (builder);
		}
	}

	public class RegexLiteral : Constant, ILiteralConstant
	{
		readonly public string Regex;
		readonly public string Options;

		public RegexLiteral (string regex, string options, Location loc)
			: base (loc)
		{
			Regex = regex;
			Options = options ?? "";
		}

		public override bool IsLiteral {
			get { return true; }
		}

		public override object GetValue ()
		{
			return "/" + Regex + "/" + Options;
		}
		
		public override string GetValueAsLiteral ()
		{
			return GetValue () as String;
		}
		
		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			throw new NotSupportedException ();
		}
		
		public override bool IsDefaultValue {
			get {
				return Regex == null && Options == "";
			}
		}
		
		public override bool IsNegative {
			get {
				return false;
			}
		}
		
		public override bool IsNull {
			get {
				return IsDefaultValue;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			return null;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
/*
			if (rc.Target == Target.JavaScript) {
				type = rc.Module.PredefinedTypes.AsRegExp.Resolve();
				eclass = ExprClass.Value;
				return this;
			}
*/
			var args = new Arguments(2);
			args.Add (new Argument(new StringLiteral(rc.BuiltinTypes, Regex, this.Location)));
			args.Add (new Argument(new StringLiteral(rc.BuiltinTypes, Options, this.Location)));

			return new New(new TypeExpression(rc.Module.PlayscriptTypes.RegExp.Resolve(), this.Location), 
			               args, this.Location).Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
/*
		public override void EmitJs (JsEmitContext jec)
		{
			jec.Buf.Write (GetValue () as String, Location);
		}
*/
#if FULL_AST
		public char[] ParsedValue { get; set; }
#endif

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class XmlLiteral : Constant, ILiteralConstant
	{
		readonly public string Xml;

		public XmlLiteral (string xml, Location loc)
			: base (loc)
		{
			Xml = xml;
		}
		
		public override bool IsLiteral {
			get { return true; }
		}
		
		public override object GetValue ()
		{
			return Xml;
		}
		
		public override string GetValueAsLiteral ()
		{
			return GetValue () as String;
		}
		
		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}
		
		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			throw new NotSupportedException ();
		}
		
		public override bool IsDefaultValue {
			get {
				return Xml == null;
			}
		}
		
		public override bool IsNegative {
			get {
				return false;
			}
		}
		
		public override bool IsNull {
			get {
				return IsDefaultValue;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			return null;
		}
		
		protected override Expression DoResolve (ResolveContext rc)
		{
			var args = new Arguments(1);
			args.Add (new Argument(new StringLiteral(rc.BuiltinTypes, Xml, this.Location)));

			return new New(new TypeExpression(rc.Module.PlayscriptTypes.Xml.Resolve(), this.Location), 
			               args, this.Location).Resolve (rc);
		}
		
		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
/*		
		public override void EmitJs (JsEmitContext jec)
		{
			jec.Buf.Write (GetValue () as String, Location);
		}
*/		
#if FULL_AST
		public char[] ParsedValue { get; set; }
#endif
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class In : PlayScriptExpression
	{
		public In (Expression propertyExpr, Expression expression, Location loc)
		{
			this.PropertyExpression = propertyExpr;
			this.Expression = expression;
			this.loc = loc;
		}

		public Expression Expression { get; private set; }

		public Expression PropertyExpression { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			PropertyExpression = PropertyExpression.Resolve (rc);
			Expression = Expression.Resolve (rc);
			if (PropertyExpression == null || Expression == null)
				return null;

			if (Expression is MethodGroupExpr) {
				var res = new BoolConstant (rc.BuiltinTypes, false, Location);
				res.Resolve (rc);
				return res;
			}

			var ms = rc.Module.PlayScriptMembers.BinderHasProperty.Resolve (loc);
			if (ms == null)
				return null;

			var args = new Arguments (3);
			args.Add (new Argument (Expression));
			args.Add (new Argument (new CSharp.TypeOf (rc.CurrentType, loc)));
			args.Add (new Argument (PropertyExpression));

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			return new Invocation (mg, args).Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class AsLocalFunction : Statement {
		
		public string Name;
		public AnonymousMethodExpression MethodExpr;
		public BlockVariable VarDecl;

		public AsLocalFunction (Location loc, string name, AnonymousMethodExpression methodExpr, BlockVariable varDecl)
		{
			this.loc = loc;
			this.Name = name;
			this.MethodExpr = methodExpr;
			this.VarDecl = varDecl;
		}

		public override bool Resolve (BlockContext bc)
		{
			return true;
		}

		protected override void CloneTo (CloneContext clonectx, Statement t)
		{
			var target = (AsLocalFunction) t;

			target.Name = Name;
			target.MethodExpr = MethodExpr.Clone (clonectx) as AnonymousMethodExpression;
			target.VarDecl = VarDecl.Clone (clonectx) as BlockVariable;
		}

		protected override void DoEmit (EmitContext ec)
		{
		}

//		public override void EmitJs (JsEmitContext jec)
//		{
//			jec.Buf.Write ("delete ", Location);
//			Expr.EmitJs (jec);
//		}
//		
//		public override void EmitStatementJs (JsEmitContext jec)
//		{
//			jec.Buf.Write ("\t", Location);
//			EmitJs (jec);
//			jec.Buf.Write (";\n");
//		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new System.NotSupportedException ();
		}
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class UseNamespace : Statement
	{
		public UseNamespace (string ns, Location loc)
		{
			Namespace = ns;
			this.loc = loc;
		}

		public string Namespace { get; private set; }

		public override bool Resolve (BlockContext bc)
		{
			// TODO: Implement by adding the name to BlockContext namespaces list. Then when 
			// doing the namespace lookup get the list and do search with prefixes from the list
			// It looks like once the name is added (used) it's never removed even if the scope
			// is different
			return true;
		}
		
		public override bool ResolveUnreachable (BlockContext bc, bool warn)
		{
			return true;
		}
		
		public override void Emit (EmitContext ec)
		{
			// Nothing, not even sequence point
		}

		protected override void DoEmit (EmitContext ec)
		{
		}
		
		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
		}
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class AsNonAssignStatementExpression : Statement
	{
		public Expression expr;
		
		public AsNonAssignStatementExpression (Expression expr)
		{
			this.expr = expr;
		}
		
		public Expression Expr {
			get {
				return expr;
			}
		}

		public override bool Resolve (BlockContext bc)
		{
			if (!base.Resolve (bc))
				return false;

			expr = expr.Resolve (bc);

			return expr != null;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (!expr.IsSideEffectFree) {
				expr.EmitSideEffect (ec);
			}
		}
/*
		protected override void DoEmitJs (JsEmitContext jec) 
		{
			expr.EmitJs (jec);
		}
		
		public override void EmitJs (JsEmitContext jec)
		{
			DoEmitJs (jec);
		}
*/
		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			var t = target as AsNonAssignStatementExpression;
			t.expr = expr.Clone (clonectx);
		}
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	/// <summary>
	///   Implementation of the ActionScript E4X xml query.
	/// </summary>
	public class AsXmlQueryExpression : Expression
	{
		protected Expression expr;
		protected Expression query;
		
		public AsXmlQueryExpression (Expression expr, Expression query, Location l)
		{
			this.expr = expr;
			this.query = query;
			loc = l;
		}
		
		public Expression Expr {
			get {
				return expr;
			}
		}

		public Expression Query {
			get {
				return query;
			}
		}

		public override bool ContainsEmitWithAwait ()
		{
			throw new NotSupportedException ();
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}
		
		protected override Expression DoResolve (ResolveContext ec)
		{
			// TODO: Implement XML query expression.
			return null;
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			AsXmlQueryExpression target = (AsXmlQueryExpression) t;
			
			target.expr = expr.Clone (clonectx);
			target.query = query.Clone (clonectx);
		}
		
		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("Missing Resolve call");
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
		
	}

	public class SimpleName : CSharp.SimpleName
	{
		public SimpleName (string name, Location loc)
			: base (name, loc)
		{
		}

		// TODO: targs should be always null
		public SimpleName (string name, TypeArguments targs, Location loc)
			: base (name, targs, loc)
		{
		}

		public override Expression LookupNameExpression (ResolveContext rc, MemberLookupRestrictions restrictions)
		{
			int lookup_arity = Arity;
			bool errorMode = false;
			Expression e;
			Block current_block = rc.CurrentBlock;
			INamedBlockVariable variable = null;
			bool variable_found = false;

			//
			// Stage 1: binding to local variables or parameters
			//
			if (current_block != null && lookup_arity == 0) {
				if (current_block.ParametersBlock.TopBlock.GetLocalName (Name, current_block.Original, ref variable)) {
					if (!variable.IsDeclared) {
//						rc.Report.Warning (7156, 1, loc, "Use of local variable before declaration");
						if (variable is LocalVariable) {
							var locVar = variable as LocalVariable;
//							if (locVar.Type == null && locVar.TypeExpr != null) {
//								locVar.DeclFlags |= LocalVariable.Flags.AsIgnoreMultiple;
//								locVar.Type = locVar.TypeExpr.ResolveAsType (rc);
//							}
						}
						e = variable.CreateReferenceExpression (rc, loc);
						if (e != null) {
							if (Arity > 0)
								Error_TypeArgumentsCannotBeUsed (rc, "variable", Name, loc);

							return e;
						}
					} else {
						e = variable.CreateReferenceExpression (rc, loc);
						if (e != null) {
							if (Arity > 0)
								Error_TypeArgumentsCannotBeUsed (rc, "variable", Name, loc);

							return e;
						}
					}
				}
			}

				//
				// Stage 2: Lookup members if we are inside a type up to top level type for nested types
				//
				TypeSpec member_type = rc.CurrentType;
				for (; member_type != null; member_type = member_type.DeclaringType) {
					e = MemberLookup (rc, errorMode, member_type, Name, lookup_arity, restrictions, loc);
					if (e == null)
						continue;

					var me = e as MemberExpr;
					if (me == null) {
						// The name matches a type, defer to ResolveAsTypeStep
						if (e is TypeExpr)
							break;

						continue;
					}

					me = me.ResolveMemberAccess (rc, null, null);

					if (Arity > 0) {
						targs.Resolve (rc);
						me.SetTypeArguments (rc, targs);
					}

					return me;
				}

			// Stage 3: Global names lookup
			e = LookupGlobalName (rc, Name + "_fn", restrictions) ?? LookupGlobalName (rc, "<Globals>", restrictions);
			if (e != null)
				return e;

			//
			// Stage 3: Lookup nested types, namespaces and type parameters in the context
			//
			if ((restrictions & MemberLookupRestrictions.InvocableOnly) == 0 && !variable_found) {
				if (IsPossibleTypeOrNamespace (rc)) {
					if (variable != null) {
						rc.Report.SymbolRelatedToPreviousError (variable.Location, Name);
						rc.Report.Error (135, loc, "`{0}' conflicts with a declaration in a child block", Name);
					}

					var fne = ResolveAsTypeOrNamespace (rc);
					if (fne != null && (restrictions & MemberLookupRestrictions.PlayScriptConversion) == 0) {
						return new CSharp.TypeOf (fne, loc);
					}

					return fne;
				}
			}

			// TODO: Use C# rules too?

			// TODO: Handle errors
			throw new NotImplementedException ();
		}

		public override FullNamedExpression ResolveAsTypeOrNamespace (IMemberContext mc)
		{
			var fne = ResolveKnownTypes (mc);
			if (fne != null)
				return fne;

			return base.ResolveAsTypeOrNamespace (mc);
		}

		// TODO: Add ambiguity checks
		// PS1000: var:Number:Number = 0; is ambiguous between local variable and global type
		TypeExpression ResolveKnownTypes (IMemberContext mc)
		{
			var types = mc.Module.Compiler.BuiltinTypes;
			switch (Name) {
			case "Object":
				return new TypeExpression (mc.Module.PlayscriptTypes.Object, loc);
			case "Boolean":
				return new TypeExpression (types.Bool, loc);
			case "Number":
				return new TypeExpression (types.Double, loc);
			case "String":
				return new TypeExpression (types.String, loc);
			case "Function":
				return new TypeExpression (types.Delegate, loc);
			case "Class":
				return new TypeExpression (types.Type, loc);
			default:
				return null;
			}
		}

		Expression LookupGlobalName (ResolveContext rc, string name, MemberLookupRestrictions restrictions)
		{
			bool errorMode = false;

			FullNamedExpression fne = rc.LookupNamespaceOrType (name, 0, LookupMode.Normal, loc);
			if (fne == null || fne is Namespace) {
				return null;
			}

			TypeSpec member_type = fne.ResolveAsType (rc);
			if (member_type == null) {
				return null;
			}

			Expression e = MemberLookup (rc, errorMode, member_type, Name, Arity, restrictions, loc);
			if (e == null)
				return null;

			var me = e as MemberExpr;
			me = me.ResolveMemberAccess (rc, null, null);
/*
			if (Arity > 0) {
				targs.Resolve (rc);
				me.SetTypeArguments (rc, targs);
			}
*/

			return me;
		}
	}

	public class QualifiedMemberAccess : MemberAccess
	{
		public QualifiedMemberAccess (string namespaceName, string identifier, Location l)
			: base (null, identifier, l)
		{
			this.Namespace = namespaceName;
		}

		public string Namespace { get; private set; }

		public override Expression LookupNameExpression (ResolveContext rc, MemberLookupRestrictions restrictions)
		{
/*
			expr = rc.LookupNamespaceAlias (Namespace);
			if (expr == null) {
				// TODO: New error code
				rc.Module.Compiler.Report.ErrorPlayScript (9999, loc, "Namespace `{0}' not found", Namespace);
				return null;
			}
*/
			var expr_type = rc.CurrentType;
			var name = Namespace + "." + Name;
			var member_lookup = MemberLookup (rc, false, expr_type, name, 0, restrictions, loc);
			if (member_lookup != null)
				return member_lookup;

			// TODO: Implement correct rules for out of context namespaces
			throw new NotImplementedException ("Namespace global lookup");
		}
	}

	public class NamespaceMemberName : MemberName
	{
		public NamespaceMemberName (string namespaceName, string name, Location loc)
			: base (new MemberName (namespaceName, loc), name, loc)
		{
		}

		public override string LookupName {
			get {
				return Left.LookupName + "." + base.LookupName;
			}
		}

		public override string GetSignatureForError ()
		{
			return Left.GetSignatureForError () + "::" + Name;
		}
	}

	public class NamespaceField : FieldBase
	{
		class NamespaceInitializer : FieldInitializer
		{
			readonly NamespaceField field;

			public NamespaceInitializer (NamespaceField field, Expression value, Location loc)
				: base (field, value, loc)
			{
				this.field = field;
			}

			protected override ExpressionStatement ResolveInitializer (ResolveContext rc)
			{
				if (source == null)
					return null;

				source = source.Resolve (rc);
				if (source == null)
					return null;

				source = GetStringValue (rc, source);
				if (source == null) {
					rc.Report.ErrorPlayScript (1171, loc, "A namespace initializer must be either a literal string or another namespace.");
					return null;
				}

				var args = new Arguments (2);
				args.Add (new Argument (new NullLiteral (loc)));
				args.Add (new Argument (source));
				source = new New (new TypeExpression (field.MemberType, loc), args, loc);

				return base.ResolveInitializer (rc);
			}

			public static StringConstant GetStringValue (IMemberContext mc, Expression source)
			{
				var sc = source as StringConstant;
				if (sc != null)
					return sc;

				var fe = source as FieldExpr;
				if (fe == null)
					return null;

				var nf = fe.Spec as NamespaceFieldSpec;
				if (nf == null)
					return null;

				return new StringConstant (mc.Module.Compiler.BuiltinTypes, nf.GetValue (), Location.Null);
			}
		}

		const Modifiers AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public NamespaceField (TypeDefinition parent, Modifiers mod, MemberName name, Expression initializer, Attributes attrs)
			: base (parent, null, mod, AllowedModifiers, name, attrs)
		{
			Initializer = initializer;
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			ModFlags |= Modifiers.STATIC;

			FieldAttributes field_attr = FieldAttributes.InitOnly | ModifiersExtensions.FieldAttr (ModFlags);
			FieldBuilder = Parent.TypeBuilder.DefineField (GetFullName (MemberName), MemberType.GetMetaInfo (), field_attr);

			spec = new NamespaceFieldSpec (Parent.Definition, this, MemberType, FieldBuilder, ModFlags);
			Parent.MemberCache.AddMember (spec);

			if (initializer != null) {
				Parent.PartialContainer.RegisterFieldForInitialization (this,  new NamespaceInitializer (this, initializer, Location));
			}

			return true;
		}

		public override void Emit ()
		{
			base.Emit ();

			Module.PlayscriptAttributes.NamespaceField.EmitAttribute (FieldBuilder, GetValue ());
		}

		public string GetValue ()
		{
			if (initializer == null)
				return null;

			var sc = NamespaceInitializer.GetStringValue (this, initializer);
			if (sc == null)
				return null;

			return sc.Value;
		}

		protected override bool ResolveMemberType ()
		{
			member_type = Module.PlayscriptTypes.Namespace.Resolve ();
			return true;
		}
	}

	class NamespaceFieldSpec : FieldSpec
	{
		string value;

		public NamespaceFieldSpec (TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, FieldInfo info, Modifiers modifiers)
			: base (declaringType, definition, memberType, info, modifiers)
		{
		}

		public NamespaceFieldSpec (TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, FieldInfo info, Modifiers modifiers, string value)
			: this (declaringType, definition, memberType, info, modifiers)
		{
			this.value = value;
		}

		public string GetValue ()
		{
			var def = MemberDefinition as NamespaceField;
			if (def != null)
				return def.GetValue ();

			return value;
		}
	}

	interface IConstantProperty
	{
		Expression Initializer { get; }
	}

	class ImportedPropertyConstant : ImportedMemberDefinition, IConstantProperty
	{
		public ImportedPropertyConstant (MemberInfo member, TypeSpec type, MetadataImporter importer)
			: base (member, type, importer)
		{
		}

		public Expression Initializer { get; set; }
	}

	public class ConstantField : FieldBase
	{
		public class Property : CSharp.Property, IConstantProperty
		{
			public Property (TypeDefinition parent, FullNamedExpression type, Modifiers mod, MemberName name, Attributes attrs)
				: base (parent, type, mod, name, attrs)
			{
			}

			public Expression Initializer { get; set; }

			public override void Emit ()
			{
				var rc = new ResolveContext (this);
				rc.CurrentBlock = Get.Block;

				var init = Initializer.Resolve (rc);
				if (init != null) {
					init = CSharp.Convert.ImplicitConversionRequiredEnhanced (rc, init, member_type, Initializer.Location);
					if (init == null)
						return;
				}
					
				var c = init as Constant;
				if (c == null) {
					var set_field = new Field (Parent, new TypeExpression (Compiler.BuiltinTypes.Bool, Location), Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | (ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
						new MemberName ("<" + GetFullName (MemberName) + ">__SetField", Location), null);

					var lazy_field = new Field (Parent, type_expr, Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | (ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
						new MemberName ("<" + GetFullName (MemberName) + ">__LazyField", Location), null);

					set_field.Define ();
					lazy_field.Define ();

					Parent.AddField (set_field);
					Parent.AddField (lazy_field);

					// 
					// if (!SetField) {
					//   SetField = true;
					//   LazyField = Initializer;
					// }
					//
					var set_f_expr = new FieldExpr (set_field, Location);
					var lazy_f_expr = new FieldExpr (lazy_field, Location);
					if (!IsStatic) {
						set_f_expr.InstanceExpression = new CompilerGeneratedThis (CurrentType, Location);
						lazy_f_expr.InstanceExpression = new CompilerGeneratedThis (CurrentType, Location);
					}

					var expl = new ExplicitBlock (Get.Block, Location, Location);
					Get.Block.AddScopeStatement (new If (new Unary (Unary.Operator.LogicalNot, set_f_expr, Location), expl, Location));

					expl.AddStatement (new StatementExpression (new CompilerAssign (lazy_f_expr, init, Location)));
					Get.Block.AddStatement (new Return (lazy_f_expr, Location));

					Module.PlayscriptAttributes.ConstantField.EmitAttribute (PropertyBuilder);
				} else {
					//
					// Simple constant, just return a value
					//
					Get.Block.AddStatement (new Return (init, Location));

					//
					// Store compile time constant to attribute for easier import
					//
					Module.PlayscriptAttributes.ConstantField.EmitAttribute (this, PropertyBuilder, c);
				}

				base.Emit ();
			}
		}

		const Modifiers AllowedModifiers =
			Modifiers.STATIC |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public ConstantField (TypeDefinition parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base (parent, type, mod_flags, AllowedModifiers, name, attrs)
		{
		}

		public override bool Define ()
		{
			if (Initializer == null) {
				Report.WarningPlayScript (1111, Location, "The constant was not initialized.");
			}

			if (!base.Define ())
				return false;

			if (Parent is PackageGlobalContainer)
				ModFlags |= Modifiers.STATIC;

			var t = new TypeExpression (MemberType, TypeExpression.Location);
			var init = Initializer ?? new DefaultValueExpression (t, Location);

			var prop = new Property (Parent, t, ModFlags, MemberName, attributes);
			prop.Initializer = init;
			prop.Get = new Property.GetMethod (prop, 0, null, prop.Location);
			prop.Get.Block = new ToplevelBlock (Compiler, Location);

			if (!prop.Define ())
				return false;

			var idx = Parent.Members.IndexOf (this);
			Parent.Members[idx] = prop;

			if (declarators != null) {
				foreach (var d in declarators) {
					init = d.Initializer ?? new DefaultValueExpression (t, Location);

					prop = new Property (Parent, t, ModFlags, new MemberName (d.Name.Value, d.Name.Location), attributes);
					prop.Initializer = init;

					prop.Get = new Property.GetMethod (prop, 0, null, prop.Location);
					prop.Get.Block = new ToplevelBlock (Compiler, Location); ;

					prop.Define ();
					Parent.PartialContainer.Members.Add (prop);
				}
			}

			return true;
		}
	}

	public class FieldDeclarator : CSharp.FieldDeclarator
	{
		public FieldDeclarator (SimpleMemberName name, Expression initializer, FullNamedExpression typeExpr)
			: base (name, initializer)
		{
			this.TypeExpression = typeExpr;
		}

		public FieldDeclarator (SimpleMemberName name, Expression initializer)
			: base (name, initializer)
		{
		}

		public FullNamedExpression TypeExpression { get; private set; }

		public override FullNamedExpression GetFieldTypeExpression (FieldBase field)
		{
			return TypeExpression;
		}
	}

	public class BlockVariableDeclarator : CSharp.BlockVariableDeclarator
	{
		public BlockVariableDeclarator (LocalVariable li, Expression initializer, FullNamedExpression typeExpr)
			: base (li, initializer)
		{
			this.TypeExpression = typeExpr;
		}

		public BlockVariableDeclarator (LocalVariable li, Expression initializer)
			: base (li, initializer)
		{
		}

		public FullNamedExpression TypeExpression { get; private set; }
	}

	public class E4XIndexer : PlayScriptExpression
	{
		public enum Operator
		{
			Attribute,	// .@
			Namespace	// ::
		}

		readonly Operator oper;
		readonly Arguments args;
		Expression expr;

		public E4XIndexer (Operator oper, Expression expr, Arguments args, Location loc)
		{
			this.oper = oper;
			this.expr = expr;
			this.args = args;
			this.loc = loc;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			switch (oper) {
			case Operator.Attribute:
				return MakeInvocation ("attribute").Resolve (rc);
			case Operator.Namespace:
				return MakeInvocation ("namespace").Resolve (rc);
			}

			throw new NotImplementedException ();
		}

		Expression MakeInvocation (string method)
		{
			return new Invocation (new MemberAccess (expr, method, loc), args);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
	}

	public class E4XOperator : PlayScriptExpression
	{
		public enum Operator
		{
			Descendant,		// ..
			ChildAll,		// .*
			ChildAttribute,	// .@
			DescendantAll,	// ..*
			Namespace		// ::
		}

		readonly Operator oper;
		readonly string name;
		Expression expr;
		
		public E4XOperator (Operator oper, Expression expr, string name, Location loc)
		{
			this.oper = oper;
			this.expr = expr;
			this.name = name;
			this.loc = loc;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			switch (oper) {
			case Operator.ChildAll:
				return MakeInvocation (rc, "children").Resolve (rc);
			case Operator.DescendantAll:
				return MakeInvocation (rc, "descendants").Resolve (rc);
			case Operator.ChildAttribute:
				return MakeInvocation (rc, "attribute", name).Resolve (rc);
			case Operator.Descendant:
				return MakeInvocation (rc, "descendants", name).Resolve (rc);
			case Operator.Namespace:
				return MakeInvocation (rc, "namespace", name).Resolve (rc);
			}

			throw new NotImplementedException ();
		}

		Expression MakeInvocation (ResolveContext rc, string method, string arg = null)
		{
			Arguments args = null;
			if (arg != null) {
				args = new Arguments (1);
				args.Add (new Argument (new StringLiteral (rc.BuiltinTypes, arg, loc)));
			}

			return new Invocation (new MemberAccess (expr, method, loc), args);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
	}

	public enum AsForEachType
	{
		/// <summary>
		/// Generate a normal cs foreach statement.
		/// </summary>
//		CSharpForEach,
		/// <summary>
		/// Generate an PlayScript for (var a in collection) statement.  Yields keys.
		/// </summary>
		ForEachKey,
		/// <summary>
		/// Generate an PlayScript for each (var a in collection) statement.  Yields values.
		/// </summary>
		ForEachValue
	}

	public class Foreach : CSharp.Foreach
	{
		public Foreach (Expression type, LocalVariable var, Expression expr, Statement stmt, Block body, AsForEachType asType, Location l)
			: base (type, var, expr, stmt, body, l)
		{
//			asForEachType = asType;
		}

		public Foreach (FullNamedExpression varRef, Expression expr, Statement stmt, Block body, AsForEachType asType, Location l)
			: base (null, null, expr, stmt, body, l)
		{
//			this.varRef = varRef;
		}
	}

	public class UsingType : UsingNamespace
	{
		protected TypeSpec resolvedType;

		public UsingType (ATypeNameExpression expr, Location loc)
			: base (expr, loc)
		{
		}

		public override void Define (NamespaceContainer ctx)
		{
			resolved = NamespaceExpression.ResolveAsTypeOrNamespace (ctx);
			if (resolved != null) {
				resolvedType = resolved.ResolveAsType (ctx);
			}
		}

		public TypeSpec ResolvedType
		{
			get { return resolvedType; }
		}
	}

	class DynamicClassMemberAccess : PlayScriptExpression, IAssignMethod
	{
		Expression invocation;

		public DynamicClassMemberAccess (ElementAccess ea)
			: this (ea.Expr, ea.Arguments, ea.Location)
		{
		}

		public DynamicClassMemberAccess (Expression instance, Arguments args, Location loc)
		{
			this.Instance = instance;
			this.Arguments = args;
			this.loc = loc;
		}

		public Arguments Arguments { get; private set; }

		public Expression Instance { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			var ms = rc.Module.PlayScriptMembers.BinderGetMember.Resolve (loc);
			if (ms == null)
				return null;

			// TODO: Figure out what value = dc["a", "b"] is supposed to do

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (3);
			call_args.Add (new Argument (Instance));
			call_args.Add (new Argument (new CSharp.TypeOf (rc.CurrentType, loc)));
			call_args.Add (Arguments [0]);

			invocation = new Invocation (mg, call_args).Resolve (rc);
			if (invocation == null)
				return null;

			eclass = ExprClass.Variable;
			type = invocation.Type;
			return this;
		}

		public override Expression DoResolveLValue (ResolveContext rc, Expression rhs)
		{
			var ms = rc.Module.PlayScriptMembers.BinderSetMember.Resolve (loc);
			if (ms == null)
				return null;

			// TODO: Figure out what dc["a", "b"] = value is supposed to do

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (3);
			call_args.Add (new Argument (Instance));
			call_args.Add (new Argument (new CSharp.TypeOf (rc.CurrentType, loc)));
			call_args.Add (Arguments [0]);
			call_args.Add (new Argument (rhs));

			invocation = new Invocation (mg, call_args).Resolve (rc);

			eclass = ExprClass.Variable;
			type = rhs.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			invocation.Emit (ec);
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			if (leave_copy || isCompound)
				throw new NotImplementedException ();

			invocation.Emit (ec);
		}
	}

	class Package : NamespaceContainer
	{
		static readonly MemberName DefaultPackageName = new MemberName (PredefinedTypes.RootNamespace, Location.Null);
		TypeDefinition globals;

		private Package (MemberName name, NamespaceContainer parent)
			: base (name, parent)
		{
		}

		public static Package Create (MemberName name, NamespaceContainer parent)
		{
			if (name == null)
				return new Package (DefaultPackageName, parent);

			return new Package (name, parent) {
				Usings = new List<UsingNamespace> () {
					new UsingNamespace (new SimpleName (PredefinedTypes.RootNamespace, name.Location), name.Location)
				}
			};
		}

		public bool IsTopLevel {
			get {
				return MemberName == DefaultPackageName;
			}
		}

		public override string GetSignatureForError ()
		{
			if (IsTopLevel)
				return "";

			return base.GetSignatureForError ();
		}

		public TypeDefinition GetGlobalsTypeDefinition ()
		{
			if (globals == null) {
				globals = new PackageGlobalContainer (this);
				AddTypeContainer (globals);
			}

			return globals;
		}
	}

	class PackageGlobalContainer : CompilerGeneratedContainer
	{
		public PackageGlobalContainer (TypeContainer parent)
			: base (parent, new MemberName ("<Globals>"), Modifiers.PUBLIC | Modifiers.STATIC)
		{
		}
	}

	class PredefinedTypes
	{
		public readonly BuiltinTypeSpec Object;

		public readonly PredefinedType Vector;
		public readonly PredefinedType Array;
		public readonly PredefinedType Error;
		public readonly PredefinedType Function;
		public readonly PredefinedType RegExp;
		public readonly PredefinedType Xml;
		public readonly PredefinedType Namespace;

		public readonly PredefinedType Binder;
		public readonly PredefinedType Operations;

		//
		// The namespace used for the root package.
		//
		public const string RootNamespace = "_root";

		public PredefinedTypes (ModuleContainer module)
		{
			Object = new BuiltinTypeSpec ("Object", BuiltinTypeSpec.Type.Object);
			Object.SetDefinition (module.Compiler.BuiltinTypes.Object);
			Object.Modifiers |= Modifiers.DYNAMIC;
			// TODO: Add toString to MemberCache which will map to ToString

			Array = new PredefinedType (module, MemberKind.Class, RootNamespace, "Array");
			Vector = new PredefinedType (module, MemberKind.Class, RootNamespace, "Vector", 1);
			Error = new PredefinedType (module, MemberKind.Class, RootNamespace, "Error");
			Function = new PredefinedType (module, MemberKind.Class, RootNamespace, "Function");
			RegExp = new PredefinedType (module, MemberKind.Class, RootNamespace, "RegExp");
			Xml = new PredefinedType (module, MemberKind.Class, RootNamespace, "XML");
			Namespace = new PredefinedType (module, MemberKind.Class, RootNamespace, "Namespace");

			Binder = new PredefinedType (module, MemberKind.Class, "PlayScript.Runtime", "Binder");
			Operations = new PredefinedType (module, MemberKind.Class, "PlayScript.Runtime", "Operations");

			// Define types which also used for comparisons early
			Array.Define ();
		}
	}

	class PredefinedMembers
	{
		public readonly PredefinedMember<MethodSpec> ArrayPush;
		public readonly PredefinedMember<MethodSpec> VectorPush;
		public readonly PredefinedMember<MethodSpec> BinderGetMember;
		public readonly PredefinedMember<MethodSpec> BinderSetMember;
		public readonly PredefinedMember<MethodSpec> BinderHasProperty;
		public readonly PredefinedMember<MethodSpec> BinderDeleteProperty;
		public readonly PredefinedMember<MethodSpec> OperationsTypeof;

		public PredefinedMembers (ModuleContainer module)
		{
			var types = module.PredefinedTypes;
			var btypes = module.Compiler.BuiltinTypes;
			var ptypes = module.PlayscriptTypes;

			var tp = new TypeParameter (0, new MemberName ("T"), null, null, Variance.None);

			ArrayPush = new PredefinedMember<MethodSpec> (module, ptypes.Array, "push", btypes.Object);
			VectorPush = new PredefinedMember<MethodSpec> (module, ptypes.Vector, "push", new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null));
			BinderGetMember = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "GetMember", btypes.Object, btypes.Type, btypes.Object);
			BinderSetMember = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "SetMember", btypes.Object, btypes.Type, btypes.Object, btypes.Object);
			BinderDeleteProperty = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "DeleteProperty", btypes.Object, btypes.Object);
			BinderHasProperty = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "HasProperty", btypes.Object, btypes.Type, btypes.Object);
			OperationsTypeof = new PredefinedMember<MethodSpec> (module, ptypes.Operations, "Typeof", btypes.Object);
		}
	}

	class PredefinedAttributes
	{
		public class PredefinedConstantAttribute : PredefinedAttribute
		{
			PredefinedMember<MethodSpec> ctor_definition;

			public PredefinedConstantAttribute (ModuleContainer module, string ns, string name)
				: base (module, ns, name)
			{
			}

			public void EmitAttribute (IMemberContext mc, PropertyBuilder builder, Constant constant)
			{
				if (ctor_definition == null) {
					if (!Define ())
						return;

					ctor_definition = new PredefinedMember<MethodSpec> (module, type, CSharp.MemberFilter.Constructor (
						ParametersCompiled.CreateFullyResolved (module.Compiler.BuiltinTypes.Object)));
				}

				var ctor = ctor_definition.Get ();
				if (ctor == null)
					return;

				AttributeEncoder encoder = new AttributeEncoder ();
				encoder.Encode (constant.Type);
				constant.EncodeAttributeValue (mc, encoder, ctor.Parameters.Types [0]);
				encoder.EncodeEmptyNamedArguments ();

				builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), encoder.ToArray ());
			}
		}

		public class PredefinedNamespaceAttribute : PredefinedAttribute
		{
			PredefinedMember<MethodSpec> ctor_definition;

			public PredefinedNamespaceAttribute (ModuleContainer module, string ns, string name)
				: base (module, ns, name)
			{
			}

			public void EmitAttribute (FieldBuilder builder, string value)
			{
				if (ctor_definition == null) {
					if (!Define ())
						return;

					ctor_definition = new PredefinedMember<MethodSpec> (module, type, CSharp.MemberFilter.Constructor (
						ParametersCompiled.CreateFullyResolved (module.Compiler.BuiltinTypes.String)));
				}

				var ctor = ctor_definition.Get ();
				if (ctor == null)
					return;

				AttributeEncoder encoder = new AttributeEncoder ();
				encoder.Encode (value);
				encoder.EncodeEmptyNamedArguments ();

				builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), encoder.ToArray ());
			}
		}

		public readonly PredefinedConstantAttribute ConstantField;
		public readonly PredefinedNamespaceAttribute NamespaceField;
		public readonly PredefinedAttribute DynamicClass;
		public readonly PredefinedAttribute PlayScript;
		public readonly PredefinedAttribute RestArrayParameter;

		public PredefinedAttributes (ModuleContainer module)
		{
			var ns = "PlayScript.Runtime.CompilerServices";
			ConstantField = new PredefinedConstantAttribute (module, ns, "ConstantFieldAttribute");
			NamespaceField = new PredefinedNamespaceAttribute (module, ns, "NamespaceFieldAttribute");
			DynamicClass = new PredefinedAttribute (module, ns, "DynamicClassAttribute");
			PlayScript = new PredefinedAttribute (module, ns, "PlayScriptAttribute");
			RestArrayParameter = new PredefinedAttribute (module, ns, "RestArrayParameterAttribute");
		}
	}

	static class Convert
	{
		public static Expression ImplicitConversion (Expression expr, TypeSpec target)
		{
			Expression e;

			e = ImplicitNumericConversion (expr, expr.Type, target);
			if (e != null)
				return e;

			return null;
		}

		static Expression ImplicitNumericConversion (Expression expr, TypeSpec expr_type, TypeSpec target_type)
		{
			switch (expr_type.BuiltinType) {
			case BuiltinTypeSpec.Type.Int:
				//
				// From int to uint
				//
				switch (target_type.BuiltinType) {
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U4);
				}

				break;

			case BuiltinTypeSpec.Type.UInt:
				//
				// From uint to int
				//
				switch (target_type.BuiltinType) {
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I4);
				}

				break;
			}

			return null;
		}
	}

	sealed class ErrorMessage : AbstractMessage
	{
		public ErrorMessage (int code, Location loc, string message, List<string> extraInfo)
			: base (code, loc, message, extraInfo)
		{
		}

		public ErrorMessage (AbstractMessage aMsg)
			: base (aMsg)
		{
		}

		public override bool IsWarning {
			get {
				return false;
			}
		}

		public override string LanguagePrefix {
			get {
				return "PS";
			}
		}

		public override string MessageType {
			get {
				return "error";
			}
		}
	}

	sealed class WarningMessage : AbstractMessage
	{
		public WarningMessage (int code, Location loc, string message, List<string> extra_info)
			: base (code, loc, message, extra_info)
		{
		}

		public override bool IsWarning {
			get {
				return true;
			}
		}

		public override string MessageType {
			get {
				return "warning";
			}
		}

		public override string LanguagePrefix {
			get {
				return "PS";
			}
		}
	}
}
