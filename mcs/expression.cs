//
// expression.cs: Expression representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//
// Ideas:
//   Maybe we should make Resolve be an instance method that just calls
//   the virtual DoResolve function and checks conditions like the eclass
//   and type being set if a non-null value is returned.  For robustness
//   purposes.
//

namespace CIR {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;
	
	// <remarks>
	//   The ExprClass class contains the is used to pass the 
	//   classification of an expression (value, variable, namespace,
	//   type, method group, property access, event access, indexer access,
	//   nothing).
	// </remarks>
	public enum ExprClass {
		Invalid,
		
		Value,
		Variable,   // Every Variable should implement LValue
		Namespace,
		Type,
		MethodGroup,
		PropertyAccess,
		EventAccess,
		IndexerAccess,
		Nothing, 
	}

	// <remarks>
	//   Base class for expressions
	// </remarks>
	public abstract class Expression {
		protected ExprClass eclass;
		protected Type      type;
		
		public Type Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public ExprClass ExprClass {
			get {
				return eclass;
			}

			set {
				eclass = value;
			}
		}

		// <summary>
		//   Utility wrapper routine for Error, just to beautify the code
		// </summary>
		static protected void Error (int error, string s)
		{
			Report.Error (error, s);
		}

		static protected void Error (int error, Location loc, string s)
		{
			Report.Error (error, loc, s);
		}
		
		// <summary>
		//   Utility wrapper routine for Warning, just to beautify the code
		// </summary>
		static protected void Warning (int warning, string s)
		{
			Report.Warning (warning, s);
		}

		// <summary>
		//   Performs semantic analysis on the Expression
		// </summary>
		//
		// <remarks>
		//   The Resolve method is invoked to perform the semantic analysis
		//   on the node.
		//
		//   The return value is an expression (it can be the
		//   same expression in some cases) or a new
		//   expression that better represents this node.
		//   
		//   For example, optimizations of Unary (LiteralInt)
		//   would return a new LiteralInt with a negated
		//   value.
		//   
		//   If there is an error during semantic analysis,
		//   then an error should be reported (using Report)
		//   and a null value should be returned.
		//   
		//   There are two side effects expected from calling
		//   Resolve(): the the field variable "eclass" should
		//   be set to any value of the enumeration
		//   `ExprClass' and the type variable should be set
		//   to a valid type (this is the type of the
		//   expression).
		// </remarks>
		
		public abstract Expression DoResolve (EmitContext ec);


		//
		// Currently Resolve wraps DoResolve to perform sanity
		// checking and assertion checking on what we expect from Resolve
		//

		public Expression Resolve (EmitContext ec)
		{
			Expression e = DoResolve (ec);

			if (e != null){
				if (e is SimpleName)
					return e;

				if (e.ExprClass == ExprClass.Invalid)
					throw new Exception ("Expression " + e +
							     " ExprClass is Invalid after resolve");

				if (e.ExprClass != ExprClass.MethodGroup)
					if (e.type == null)
						throw new Exception ("Expression " + e +
								     " did not set its type after Resolve");

				if (e is LValue)
					e = ((LValue) e).LValueResolve (ec);
			}

			return e;
		}
		       
		// <summary>
		//   Emits the code for the expression
		// </summary>
		//
		// <remarks>
		// 
		//   The Emit method is invoked to generate the code
		//   for the expression.  
		//
		// </remarks>
		public abstract void Emit (EmitContext ec);
		
		// <summary>
		//   Protected constructor.  Only derivate types should
		//   be able to be created
		// </summary>

		protected Expression ()
		{
			eclass = ExprClass.Invalid;
			type = null;
		}

		// <summary>
		//   Returns a literalized version of a literal FieldInfo
		// </summary>
		static Expression Literalize (FieldInfo fi)
		{
			Type t = fi.FieldType;
			object v = fi.GetValue (fi);

			if (t == TypeManager.int32_type)
				return new IntLiteral ((int) v);
			else if (t == TypeManager.uint32_type)
				return new UIntLiteral ((uint) v);
			else if (t == TypeManager.int64_type)
				return new LongLiteral ((long) v);
			else if (t == TypeManager.uint64_type)
				return new ULongLiteral ((ulong) v);
			else if (t == TypeManager.float_type)
				return new FloatLiteral ((float) v);
			else if (t == TypeManager.double_type)
				return new DoubleLiteral ((double) v);
			else if (t == TypeManager.string_type)
				return new StringLiteral ((string) v);
			else if (t == TypeManager.short_type)
				return new IntLiteral ((int) ((short)v));
			else if (t == TypeManager.ushort_type)
				return new IntLiteral ((int) ((ushort)v));
			else if (t == TypeManager.sbyte_type)
				return new IntLiteral ((int) ((sbyte)v));
			else if (t == TypeManager.byte_type)
				return new IntLiteral ((int) ((byte)v));
			else if (t == TypeManager.char_type)
				return new IntLiteral ((int) ((char)v));
			else
				throw new Exception ("Unknown type for literal (" + v.GetType () +
						     "), details: " + fi);
		}

		// 
		// Returns a fully formed expression after a MemberLookup
		//
		static Expression ExprClassFromMemberInfo (EmitContext ec, MemberInfo mi, Location loc)
		{
			if (mi is EventInfo){
				return new EventExpr ((EventInfo) mi, loc);
			} else if (mi is FieldInfo){
				FieldInfo fi = (FieldInfo) mi;

				if (fi.IsLiteral){
					Expression e = Literalize (fi);
					e.Resolve (ec);

					return e;
				} else
					return new FieldExpr (fi);
			} else if (mi is PropertyInfo){
				return new PropertyExpr ((PropertyInfo) mi, loc);
			} else if (mi is Type)
				return new TypeExpr ((Type) mi);

			return null;
		}
		
		//
		// FIXME: Probably implement a cache for (t,name,current_access_set)?
		//
		// FIXME: We need to cope with access permissions here, or this wont
		// work!
		//
		// This code could use some optimizations, but we need to do some
		// measurements.  For example, we could use a delegate to `flag' when
		// something can not any longer be a method-group (because it is something
		// else).
		//
		// Return values:
		//     If the return value is an Array, then it is an array of
		//     MethodBases
		//   
		//     If the return value is an MemberInfo, it is anything, but a Method
		//
		//     null on error.
		//
		// FIXME: When calling MemberLookup inside an `Invocation', we should pass
		// the arguments here and have MemberLookup return only the methods that
		// match the argument count/type, unlike we are doing now (we delay this
		// decision).
		//
		// This is so we can catch correctly attempts to invoke instance methods
		// from a static body (scan for error 120 in ResolveSimpleName).
		//
		public static Expression MemberLookup (EmitContext ec, Type t, string name,
						       bool same_type, MemberTypes mt,
						       BindingFlags bf, Location loc)
		{
			if (same_type)
				bf |= BindingFlags.NonPublic;

			MemberInfo [] mi = ec.TypeContainer.RootContext.TypeManager.FindMembers (
				t, mt, bf, Type.FilterName, name);

			if (mi == null)
				return null;

			// FIXME : How does this wierd case arise ?
			if (mi.Length == 0)
				return null;
			
			if (mi.Length == 1 && !(mi [0] is MethodBase))
				return Expression.ExprClassFromMemberInfo (ec, mi [0], loc);
			
			for (int i = 0; i < mi.Length; i++)
				if (!(mi [i] is MethodBase)){
					Error (-5, "Do not know how to reproduce this case: " + 
					       "Methods and non-Method with the same name, " +
					       "report this please");

					for (i = 0; i < mi.Length; i++){
						Type tt = mi [i].GetType ();

						Console.WriteLine (i + ": " + mi [i]);
						while (tt != TypeManager.object_type){
							Console.WriteLine (tt);
							tt = tt.BaseType;
						}
					}
				}

			return new MethodGroupExpr (mi);
		}

		public const MemberTypes AllMemberTypes =
			MemberTypes.Constructor |
			MemberTypes.Event       |
			MemberTypes.Field       |
			MemberTypes.Method      |
			MemberTypes.NestedType  |
			MemberTypes.Property;
		
		public const BindingFlags AllBindingsFlags =
			BindingFlags.Public |
			BindingFlags.Static |
			BindingFlags.Instance;

		public static Expression MemberLookup (EmitContext ec, Type t, string name,
						       bool same_type, Location loc)
		{
			return MemberLookup (ec, t, name, same_type, AllMemberTypes, AllBindingsFlags, loc);
		}

		//
		// I am in general unhappy with this implementation.
		//
		// I need to revise this.
		//
//		  
//		  static public Expression ResolveMemberAccess (EmitContext ec, string name)
//		  {
//			  Expression left_e = null;
//			  int dot_pos = name.LastIndexOf (".");
//			  string left = name.Substring (0, dot_pos);
//			  string right = name.Substring (dot_pos + 1);
//			  Type t;
//
//			  if ((t = ec.TypeContainer.LookupType (left, false)) != null){
//				  Expression e;
//				  
//				  left_e = new TypeExpr (t);
//				  e = new MemberAccess (left_e, right);
//				  return e.Resolve (ec);
//			  } else {
//				  //
//				  // FIXME: IMplement:
//				  
//				  // Handle here:
//				  //    T.P  Static property access (P) on Type T.
//				  //    e.P  instance property access on instance e for P.
//				  //    p
//				  //
//			  }
//
//			  if (left_e == null){
//				  Error (246, "Can not find type or namespace `"+left+"'");
//				  return null;
//			  }
//
//			  switch (left_e.ExprClass){
//			  case ExprClass.Type:
//				  return  MemberLookup (ec,
//							left_e.Type, right,
//							left_e.Type == ec.TypeContainer.TypeBuilder);
//				  
//			  case ExprClass.Namespace:
//			  case ExprClass.PropertyAccess:
//			  case ExprClass.IndexerAccess:
//			  case ExprClass.Variable:
//			  case ExprClass.Value:
//			  case ExprClass.Nothing:
//			  case ExprClass.EventAccess:
//			  case ExprClass.MethodGroup:
//			  case ExprClass.Invalid:
//				  throw new Exception ("Should have got the " + left_e.ExprClass +
//						       " handled before");
//			  }
//			  
//			  return null;
//		  }
//
		static public Expression ImplicitReferenceConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (target_type == TypeManager.object_type) {
				if (expr_type.IsClass)
					return new EmptyCast (expr, target_type);
				if (expr_type.IsValueType)
					return new BoxedCast (expr);
			} else if (expr_type.IsSubclassOf (target_type)) {
				return new EmptyCast (expr, target_type);
			} else {
				// from any class-type S to any interface-type T.
				if (expr_type.IsClass && target_type.IsInterface) {
					Type [] interfaces = expr_type.FindInterfaces (Module.FilterTypeName,
										       target_type.FullName);
					if (interfaces != null)
						return new EmptyCast (expr, target_type);
				}	

				// from any interface type S to interface-type T.
				// FIXME : Is it right to use IsAssignableFrom ?
				if (expr_type.IsInterface && target_type.IsInterface)
					if (target_type.IsAssignableFrom (expr_type))
						return new EmptyCast (expr, target_type);
				
				
				// from an array-type S to an array-type of type T
				if (expr_type.IsArray && target_type.IsArray) {

					Console.WriteLine ("{0} -> {1}", expr_type, target_type);
					throw new Exception ("Implement array conversion");
					
				}
				
				// from an array-type to System.Array
				if (expr_type.IsArray && target_type.IsAssignableFrom (expr_type))
					return new EmptyCast (expr, target_type);
				
				// from any delegate type to System.Delegate
				if (expr_type.IsSubclassOf (TypeManager.delegate_type) &&
				    target_type == TypeManager.delegate_type)
					if (target_type.IsAssignableFrom (expr_type))
						return new EmptyCast (expr, target_type);
					
				// from any array-type or delegate type into System.ICloneable.
				if (expr_type.IsArray || expr_type.IsSubclassOf (TypeManager.delegate_type))
					if (target_type == TypeManager.icloneable_type)
						throw new Exception ("Implement conversion to System.ICloneable");
				
				// from the null type to any reference-type.
				if (expr is NullLiteral)
					return new EmptyCast (expr, target_type);

				return null;

			}
			
			return null;
		}

		// <summary>
		//   Handles expressions like this: decimal d; d = 1;
		//   and changes them into: decimal d; d = new System.Decimal (1);
		// </summary>
		static Expression InternalTypeConstructor (EmitContext ec, Expression expr, Type target)
		{
			ArrayList args = new ArrayList ();

			args.Add (new Argument (expr, Argument.AType.Expression));

			Expression ne = new New (target.FullName, args,
						 new Location (-1));

			return ne.Resolve (ec);
		}

		// <summary>
		//   Implicit Numeric Conversions.
		//
		//   expr is the expression to convert, returns a new expression of type
		//   target_type or null if an implicit conversion is not possible.
		//
		// </summary>
		static public Expression ImplicitNumericConversion (EmitContext ec, Expression expr,
								    Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			
			//
			// Attempt to do the implicit constant expression conversions

			if (expr is IntLiteral){
				Expression e;
				
				e = TryImplicitIntConversion (target_type, (IntLiteral) expr);
				if (e != null)
					return e;
			} else if (expr is LongLiteral){
				//
				// Try the implicit constant expression conversion
				// from long to ulong, instead of a nice routine,
				// we just inline it
				//
				if (((LongLiteral) expr).Value > 0)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
			}
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);

				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if (target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if (target_type == TypeManager.uint32_type)
					return new EmptyCast (expr, target_type);

				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)){
				//
				// From long/ulong to float, double
				//
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);	
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			}

			return null;
		}

		// <summary>
		//  Determines if a standard implicit conversion exists from
		//  expr_type to target_type
		// </summary>
		public static bool StandardConversionExists (Type expr_type, Type target_type)
		{
			if (expr_type == target_type)
				return true;

			// First numeric conversions 
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if ((target_type == TypeManager.int32_type) || 
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type)  ||
				    (target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
	
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if ((target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if ((target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)) {
				//
				// From long/ulong to float, double
				//
				if ((target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;

			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return true;
			}	
			
			// Next reference conversions

			if (target_type == TypeManager.object_type) {
				if ((expr_type.IsClass) ||
				    (expr_type.IsValueType))
					return true;
				
			} else if (expr_type.IsSubclassOf (target_type)) {
				return true;
				
			} else {
				// from any class-type S to any interface-type T.
				if (expr_type.IsClass && target_type.IsInterface)
					return true;
				
				// from any interface type S to interface-type T.
				// FIXME : Is it right to use IsAssignableFrom ?
				if (expr_type.IsInterface && target_type.IsInterface)
					if (target_type.IsAssignableFrom (expr_type))
						return true;
				
				// from an array-type S to an array-type of type T
				if (expr_type.IsArray && target_type.IsArray) 
					return true;
				
				// from an array-type to System.Array
				if (expr_type.IsArray && target_type.IsAssignableFrom (expr_type))
					return true;
				
				// from any delegate type to System.Delegate
				if (expr_type.IsSubclassOf (TypeManager.delegate_type) &&
				    target_type == TypeManager.delegate_type)
					if (target_type.IsAssignableFrom (expr_type))
						return true;
					
				// from any array-type or delegate type into System.ICloneable.
				if (expr_type.IsArray || expr_type.IsSubclassOf (TypeManager.delegate_type))
					if (target_type == TypeManager.icloneable_type)
						return true;
				
				// from the null type to any reference-type.
				// FIXME : How do we do this ?

			}

			return false;
		}
		
		// <summary>
		//  Finds "most encompassed type" according to the spec (13.4.2)
		//  amongst the methods in the MethodGroupExpr which convert from a
		//  type encompassing source_type
		// </summary>
		static Type FindMostEncompassedType (MethodGroupExpr me, Type source_type)
		{
			Type best = null;
			
			for (int i = me.Methods.Length; i > 0; ) {
				i--;

				MethodBase mb = me.Methods [i];
				ParameterData pd = Invocation.GetParameterData (mb);
				Type param_type = pd.ParameterType (0);

				if (StandardConversionExists (source_type, param_type)) {
					if (best == null)
						best = param_type;
					
					if (StandardConversionExists (param_type, best))
						best = param_type;
				}
			}

			return best;
		}
		
		// <summary>
		//  Finds "most encompassing type" according to the spec (13.4.2)
		//  amongst the methods in the MethodGroupExpr which convert to a
		//  type encompassed by target_type
		// </summary>
		static Type FindMostEncompassingType (MethodGroupExpr me, Type target)
		{
			Type best = null;
			
			for (int i = me.Methods.Length; i > 0; ) {
				i--;
				
				MethodInfo mi = (MethodInfo) me.Methods [i];
				Type ret_type = mi.ReturnType;
				
				if (StandardConversionExists (ret_type, target)) {
					if (best == null)
						best = ret_type;

					if (!StandardConversionExists (ret_type, best))
						best = ret_type;
				}
				
			}
			
			return best;

		}
		

		// <summary>
		//  User-defined Implicit conversions
		// </summary>
		static public Expression ImplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, false);
		}

		// <summary>
		//  User-defined Explicit conversions
		// </summary>
		static public Expression ExplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, true);
		}
		
		// <summary>
		//   User-defined conversions
		// </summary>
		static public Expression UserDefinedConversion (EmitContext ec, Expression source,
								Type target, Location loc,
								bool look_for_explicit)
		{
			Expression mg1 = null, mg2 = null, mg3 = null, mg4 = null;
			Expression mg5 = null, mg6 = null, mg7 = null, mg8 = null;
			Expression e;
			MethodBase method = null;
			Type source_type = source.Type;

			string op_name;
			
			// If we have a boolean type, we need to check for the True operator

			// FIXME : How does the False operator come into the picture ?
			// FIXME : This doesn't look complete and very correct !
			if (target == TypeManager.bool_type)
				op_name = "op_True";
			else
				op_name = "op_Implicit";
			
			mg1 = MemberLookup (ec, source_type, op_name, false, loc);

			if (source_type.BaseType != null)
				mg2 = MemberLookup (ec, source_type.BaseType, op_name, false, loc);
			
			mg3 = MemberLookup (ec, target, op_name, false, loc);

			if (target.BaseType != null)
				mg4 = MemberLookup (ec, target.BaseType, op_name, false, loc);

			MethodGroupExpr union1 = Invocation.MakeUnionSet (mg1, mg2);
			MethodGroupExpr union2 = Invocation.MakeUnionSet (mg3, mg4);

			MethodGroupExpr union3 = Invocation.MakeUnionSet (union1, union2);

			MethodGroupExpr union4 = null;

			if (look_for_explicit) {

				op_name = "op_Explicit";
				
				mg5 = MemberLookup (ec, source_type, op_name, false, loc);

				if (source_type.BaseType != null)
					mg6 = MemberLookup (ec, source_type.BaseType, op_name, false, loc);
				
				mg7 = MemberLookup (ec, target, op_name, false, loc);
				
				if (target.BaseType != null)
					mg8 = MemberLookup (ec, target.BaseType, op_name, false, loc);
				
				MethodGroupExpr union5 = Invocation.MakeUnionSet (mg5, mg6);
				MethodGroupExpr union6 = Invocation.MakeUnionSet (mg7, mg8);

				union4 = Invocation.MakeUnionSet (union5, union6);
			}
			
			MethodGroupExpr union = Invocation.MakeUnionSet (union3, union4);

			if (union != null) {

				Type most_specific_source, most_specific_target;

				most_specific_source = FindMostEncompassedType (union, source_type);
				if (most_specific_source == null)
					return null;

				most_specific_target = FindMostEncompassingType (union, target);
				if (most_specific_target == null) 
					return null;
				
				int count = 0;
				
				for (int i = union.Methods.Length; i > 0;) {
					i--;

					MethodBase mb = union.Methods [i];
					ParameterData pd = Invocation.GetParameterData (mb);
					MethodInfo mi = (MethodInfo) union.Methods [i];

					if (pd.ParameterType (0) == most_specific_source &&
					    mi.ReturnType == most_specific_target) {
						method = mb;
						count++;
					}
				}

				if (method == null || count > 1) {
					Report.Error (-11, loc, "Ambiguous user defined conversion");
					return null;
				}
				
				//
				// This will do the conversion to the best match that we
				// found.  Now we need to perform an implict standard conversion
				// if the best match was not the type that we were requested
				// by target.
				//
				if (look_for_explicit)
					source=ConvertExplicit (ec, source, most_specific_source, loc);
				else
					source = ConvertImplicitStandard (ec, source,
									  most_specific_source, loc);

				if (source == null)
					return null;
				
				e =  new UserCast ((MethodInfo) method, source);
				
				if (e.Type != target){
					if (!look_for_explicit)
						e = ConvertImplicitStandard (ec, e, target, loc);
					else
						e = ConvertExplicitStandard (ec, e, target, loc);

					return e;
				} else
					return e;
			}
			
			return null;
		}
		
		// <summary>
		//   Converts implicitly the resolved expression `expr' into the
		//   `target_type'.  It returns a new expression that can be used
		//   in a context that expects a `target_type'. 
		// </summary>
		static public Expression ConvertImplicit (EmitContext ec, Expression expr,
							  Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr_type == target_type)
				return expr;

			e = ImplicitNumericConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitReferenceConversion (expr, target_type);
			if (e != null)
				return e;

			e = ImplicitUserConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			if (target_type.IsSubclassOf (TypeManager.enum_type) && expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;

				if (i.Value == 0)
					return new EmptyCast (expr, target_type);
			}

			return null;
		}

		
		// <summary>
		//   Attempts to apply the `Standard Implicit
		//   Conversion' rules to the expression `expr' into
		//   the `target_type'.  It returns a new expression
		//   that can be used in a context that expects a
		//   `target_type'.
		//
		//   This is different from `ConvertImplicit' in that the
		//   user defined implicit conversions are excluded. 
		// </summary>
		static public Expression ConvertImplicitStandard (EmitContext ec, Expression expr,
								  Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr_type == target_type)
				return expr;

			e = ImplicitNumericConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitReferenceConversion (expr, target_type);
			if (e != null)
				return e;

			if (target_type.IsSubclassOf (TypeManager.enum_type) && expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;

				if (i.Value == 0)
					return new EmptyCast (expr, target_type);
			}
			return null;
		}
		// <summary>
		//   Attemps to perform an implict constant conversion of the IntLiteral
		//   into a different data type using casts (See Implicit Constant
		//   Expression Conversions)
		// </summary>
		static protected Expression TryImplicitIntConversion (Type target_type, IntLiteral il)
		{
			int value = il.Value;
			
			if (target_type == TypeManager.sbyte_type){
				if (value >= SByte.MinValue && value <= SByte.MaxValue)
					return il;
			} else if (target_type == TypeManager.byte_type){
				if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
					return il;
			} else if (target_type == TypeManager.short_type){
				if (value >= Int16.MinValue && value <= Int16.MaxValue)
					return il;
			} else if (target_type == TypeManager.ushort_type){
				if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
					return il;
			} else if (target_type == TypeManager.uint32_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint32
				//
				if (value >= 0)
					return il;
			} else if (target_type == TypeManager.uint64_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (value >= 0)
					return new OpcodeCast (il, target_type, OpCodes.Conv_I8);
			}

			return null;
		}

		// <summary>
		//   Attemptes to implicityly convert `target' into `type', using
		//   ConvertImplicit.  If there is no implicit conversion, then
		//   an error is signaled
		// </summary>
		static public Expression ConvertImplicitRequired (EmitContext ec, Expression target,
								  Type type, Location loc)
		{
			Expression e;
			
			e = ConvertImplicit (ec, target, type, loc);
			if (e != null)
				return e;
			
			string msg = "Can not convert implicitly from `"+
				TypeManager.CSharpName (target.Type) + "' to `" +
				TypeManager.CSharpName (type) + "'";

			Error (29, loc, msg);

			return null;
		}

		// <summary>
		//   Performs the explicit numeric conversions
		// </summary>
		static Expression ConvertNumericExplicit (EmitContext ec, Expression expr,
							  Type target_type)
		{
			Type expr_type = expr.Type;
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to sbyte and char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to sbyte, byte, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to sbyte, byte, short, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to sbyte, byte, short, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.uint32_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to sbyte, byte, short, ushort, int, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long to sbyte, byte, short, ushort, int, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.uint64_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to sbyte, byte, short, ushort, int, uint, long, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.int64_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to sbyte, byte, short
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
			} else if (expr_type == TypeManager.float_type){
				//
				// From float to sbyte, byte, short,
				// ushort, int, uint, long, ulong, char
				// or decimal
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.double_type){
				//
				// From double to byte, byte, short,
				// ushort, int, uint, long, ulong,
				// char, float or decimal
				//
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.char_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} 

			// decimal is taken care of by the op_Explicit methods.

			return null;
		}

		// <summary>
		//   Implements Explicit Reference conversions
		// </summary>
		static Expression ConvertReferenceExplicit (Expression source, Type target_type)
		{
			Type source_type = source.Type;
			bool target_is_value_type = target_type.IsValueType;
			
			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return new ClassCast (source, target_type);


			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (target_type.IsSubclassOf (source_type))
				return new ClassCast (source, target_type);

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (!target_type.IsSubclassOf (source_type))
					return new ClassCast (source, target_type);
			}
			    
			//
			// From any class type S to any interface T, provides S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed &&
			    !target_type.IsAssignableFrom (source_type))
				return new ClassCast (source, target_type);

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface &&
			    (!target_type.IsSealed || source_type.IsAssignableFrom (target_type)))
				return new ClassCast (source, target_type);

			//
			// FIXME: Implemet
			//

			// From an array typ eS with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsSubclassOf (TypeManager.array_type)){
				return new ClassCast (source, target_type);
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    target_type.IsSubclassOf (TypeManager.delegate_type))
				return new ClassCast (source, target_type);

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return new ClassCast (source, target_type);
			
			return null;
		}
		
		// <summary>
		//   Performs an explicit conversion of the expression `expr' whose
		//   type is expr.Type to `target_type'.
		// </summary>
		static public Expression ConvertExplicit (EmitContext ec, Expression expr,
							  Type target_type, Location loc)
		{
			Expression ne = ConvertImplicitStandard (ec, expr, target_type, loc);

			if (ne != null)
				return ne;

			ne = ConvertNumericExplicit (ec, expr, target_type);
			if (ne != null)
				return ne;

			ne = ConvertReferenceExplicit (expr, target_type);
			if (ne != null)
				return ne;

			ne = ExplicitUserConversion (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			Report.Error (30, loc, "Cannot convert type '" + TypeManager.CSharpName (expr.Type) + "' to '"
				      + TypeManager.CSharpName (target_type) + "'");
			return null;
		}

		// <summary>
		//   Same as ConverExplicit, only it doesn't include user defined conversions
		// </summary>
		static public Expression ConvertExplicitStandard (EmitContext ec, Expression expr,
								  Type target_type, Location l)
		{
			Expression ne = ConvertImplicitStandard (ec, expr, target_type, l);

			if (ne != null)
				return ne;

			ne = ConvertNumericExplicit (ec, expr, target_type);
			if (ne != null)
				return ne;

			ne = ConvertReferenceExplicit (expr, target_type);
			if (ne != null)
				return ne;

			Report.Error (30, l, "Cannot convert type '" +
				      TypeManager.CSharpName (expr.Type) + "' to '" + 
				      TypeManager.CSharpName (target_type) + "'");
			return null;
		}

		static string ExprClassName (ExprClass c)
		{
			switch (c){
			case ExprClass.Invalid:
				return "Invalid";
			case ExprClass.Value:
				return "value";
			case ExprClass.Variable:
				return "variable";
			case ExprClass.Namespace:
				return "namespace";
			case ExprClass.Type:
				return "type";
			case ExprClass.MethodGroup:
				return "method group";
			case ExprClass.PropertyAccess:
				return "property access";
			case ExprClass.EventAccess:
				return "event access";
			case ExprClass.IndexerAccess:
				return "indexer access";
			case ExprClass.Nothing:
				return "null";
			}
			throw new Exception ("Should not happen");
		}
		
		// <summary>
		//   Reports that we were expecting `expr' to be of class `expected'
		// </summary>
		protected void report118 (Location loc, Expression expr, string expected)
		{
			string kind = "Unknown";
			
			if (expr != null)
				kind = ExprClassName (expr.ExprClass);

			Error (118, loc, "Expression denotes a '" + kind +
			       "' where an " + expected + " was expected");
		}
	}

	// <summary>
	//   This is just a base class for expressions that can
	//   appear on statements (invocations, object creation,
	//   assignments, post/pre increment and decrement).  The idea
	//   being that they would support an extra Emition interface that
	//   does not leave a result on the stack.
	// </summary>

	public abstract class ExpressionStatement : Expression {

		// <summary>
		//   Requests the expression to be emitted in a `statement'
		//   context.  This means that no new value is left on the
		//   stack after invoking this method (constrasted with
		//   Emit that will always leave a value on the stack).
		// </summary>
		public abstract void EmitStatement (EmitContext ec);
	}

	// <summary>
	//   This kind of cast is used to encapsulate the child
	//   whose type is child.Type into an expression that is
	//   reported to return "return_type".  This is used to encapsulate
	//   expressions which have compatible types, but need to be dealt
	//   at higher levels with.
	//
	//   For example, a "byte" expression could be encapsulated in one
	//   of these as an "unsigned int".  The type for the expression
	//   would be "unsigned int".
	//
	// </summary>
	
	public class EmptyCast : Expression {
		protected Expression child;

		public EmptyCast (Expression child, Type return_type)
		{
			ExprClass = child.ExprClass;
			type = return_type;
			this.child = child;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
		}			
	}

	// <summary>
	//   This kind of cast is used to encapsulate Value Types in objects.
	//
	//   The effect of it is to box the value type emitted by the previous
	//   operation.
	// </summary>
	public class BoxedCast : EmptyCast {

		public BoxedCast (Expression expr)
			: base (expr, TypeManager.object_type)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (OpCodes.Box, child.Type);
		}
	}

	// <summary>
	//   This kind of cast is used to encapsulate a child expression
	//   that can be trivially converted to a target type using one or 
	//   two opcodes.  The opcodes are passed as arguments.
	// </summary>
	public class OpcodeCast : EmptyCast {
		OpCode op, op2;
		bool second_valid;
		
		public OpcodeCast (Expression child, Type return_type, OpCode op)
			: base (child, return_type)
			
		{
			this.op = op;
			second_valid = false;
		}

		public OpcodeCast (Expression child, Type return_type, OpCode op, OpCode op2)
			: base (child, return_type)
			
		{
			this.op = op;
			this.op2 = op2;
			second_valid = true;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (op);

			if (second_valid)
				ec.ig.Emit (op2);
		}			
		
	}

	// <summary>
	//   This kind of cast is used to encapsulate a child and cast it
	//   to the class requested
	// </summary>
	public class ClassCast : EmptyCast {
		public ClassCast (Expression child, Type return_type)
			: base (child, return_type)
			
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);

			ec.ig.Emit (OpCodes.Castclass, type);
		}			
		
	}
	
	// <summary>
	//   Unary expressions.  
	// </summary>
	//
	// <remarks>
	//   Unary implements unary expressions.   It derives from
	//   ExpressionStatement becuase the pre/post increment/decrement
	//   operators can be used in a statement context.
	// </remarks>
	public class Unary : ExpressionStatement {
		public enum Operator {
			Addition, Subtraction, Negate, BitComplement,
			Indirection, AddressOf, PreIncrement,
			PreDecrement, PostIncrement, PostDecrement
		}

		Operator   oper;
		Expression expr;
		ArrayList  Arguments;
		MethodBase method;
		Location   loc;
		
		public Unary (Operator op, Expression expr, Location loc)
		{
			this.oper = op;
			this.expr = expr;
			this.loc = loc;
		}

		public Expression Expr {
			get {
				return expr;
			}

			set {
				expr = value;
			}
		}

		public Operator Oper {
			get {
				return oper;
			}

			set {
				oper = value;
			}
		}

		// <summary>
		//   Returns a stringified representation of the Operator
		// </summary>
		string OperName ()
		{
			switch (oper){
			case Operator.Addition:
				return "+";
			case Operator.Subtraction:
				return "-";
			case Operator.Negate:
				return "!";
			case Operator.BitComplement:
				return "~";
			case Operator.AddressOf:
				return "&";
			case Operator.Indirection:
				return "*";
			case Operator.PreIncrement : case Operator.PostIncrement :
				return "++";
			case Operator.PreDecrement : case Operator.PostDecrement :
				return "--";
			}

			return oper.ToString ();
		}

		Expression ForceConversion (EmitContext ec, Expression expr, Type target_type)
		{
			if (expr.Type == target_type)
				return expr;

			return ConvertImplicit (ec, expr, target_type, new Location (-1));
		}

		void error23 (Type t)
		{
			Report.Error (
				23, loc, "Operator " + OperName () +
				" cannot be applied to operand of type `" +
				TypeManager.CSharpName (t) + "'");
		}

		// <summary>
		//   Returns whether an object of type `t' can be incremented
		//   or decremented with add/sub (ie, basically whether we can
		//   use pre-post incr-decr operations on it, but it is not a
		//   System.Decimal, which we test elsewhere)
		// </summary>
		static bool IsIncrementableNumber (Type t)
		{
			return (t == TypeManager.sbyte_type) ||
				(t == TypeManager.byte_type) ||
				(t == TypeManager.short_type) ||
				(t == TypeManager.ushort_type) ||
				(t == TypeManager.int32_type) ||
				(t == TypeManager.uint32_type) ||
				(t == TypeManager.int64_type) ||
				(t == TypeManager.uint64_type) ||
				(t == TypeManager.char_type) ||
				(t.IsSubclassOf (TypeManager.enum_type)) ||
				(t == TypeManager.float_type) ||
				(t == TypeManager.double_type);
		}
			
		Expression ResolveOperator (EmitContext ec)
		{
			Type expr_type = expr.Type;

			//
			// Step 1: Perform Operator Overload location
			//
			Expression mg;
			string op_name;
				
			if (oper == Operator.PostIncrement || oper == Operator.PreIncrement)
				op_name = "op_Increment";
			else if (oper == Operator.PostDecrement || oper == Operator.PreDecrement)
				op_name = "op_Decrement";
			else
				op_name = "op_" + oper;

			mg = MemberLookup (ec, expr_type, op_name, false, loc);
			
			if (mg == null && expr_type.BaseType != null)
				mg = MemberLookup (ec, expr_type.BaseType, op_name, false, loc);
			
			if (mg != null) {
				Arguments = new ArrayList ();
				Arguments.Add (new Argument (expr, Argument.AType.Expression));
				
				method = Invocation.OverloadResolve (ec, (MethodGroupExpr) mg,
								     Arguments, loc);
				if (method != null) {
					MethodInfo mi = (MethodInfo) method;
					type = mi.ReturnType;
					return this;
				} else {
					error23 (expr_type);
					return null;
				}
					
			}

			//
			// Step 2: Default operations on CLI native types.
			//

			// Only perform numeric promotions on:
			// +, -, ++, --

			if (expr_type == null)
				return null;
			
			if (oper == Operator.Negate){
				if (expr_type != TypeManager.bool_type) {
					error23 (expr.Type);
					return null;
				}
				
				type = TypeManager.bool_type;
				return this;
			}

			if (oper == Operator.BitComplement) {
				if (!((expr_type == TypeManager.int32_type) ||
				      (expr_type == TypeManager.uint32_type) ||
				      (expr_type == TypeManager.int64_type) ||
				      (expr_type == TypeManager.uint64_type) ||
				      (expr_type.IsSubclassOf (TypeManager.enum_type)))){
					error23 (expr.Type);
					return null;
				}
				type = expr_type;
				return this;
			}

			if (oper == Operator.Addition) {
				//
				// A plus in front of something is just a no-op, so return the child.
				//
				return expr;
			}

			//
			// Deals with -literals
			// int     operator- (int x)
			// long    operator- (long x)
			// float   operator- (float f)
			// double  operator- (double d)
			// decimal operator- (decimal d)
			//
			if (oper == Operator.Subtraction){
				//
				// Fold a "- Constant" into a negative constant
				//
			
				Expression e = null;

				//
				// Is this a constant? 
				//
				if (expr is IntLiteral)
					e = new IntLiteral (-((IntLiteral) expr).Value);
				else if (expr is LongLiteral)
					e = new LongLiteral (-((LongLiteral) expr).Value);
				else if (expr is FloatLiteral)
					e = new FloatLiteral (-((FloatLiteral) expr).Value);
				else if (expr is DoubleLiteral)
					e = new DoubleLiteral (-((DoubleLiteral) expr).Value);
				else if (expr is DecimalLiteral)
					e = new DecimalLiteral (-((DecimalLiteral) expr).Value);
				
				if (e != null){
					e = e.Resolve (ec);
					return e;
				}

				//
				// Not a constant we can optimize, perform numeric 
				// promotions to int, long, double.
				//
				//
				// The following is inneficient, because we call
				// ConvertImplicit too many times.
				//
				// It is also not clear if we should convert to Float
				// or Double initially.
				//
				if (expr_type == TypeManager.uint32_type){
					//
					// FIXME: handle exception to this rule that
					// permits the int value -2147483648 (-2^31) to
					// bt written as a decimal interger literal
					//
					type = TypeManager.int64_type;
					expr = ConvertImplicit (ec, expr, type, loc);
					return this;
				}

				if (expr_type == TypeManager.uint64_type){
					//
					// FIXME: Handle exception of `long value'
					// -92233720368547758087 (-2^63) to be written as
					// decimal integer literal.
					//
					error23 (expr_type);
					return null;
				}

				e = ConvertImplicit (ec, expr, TypeManager.int32_type, loc);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				} 

				e = ConvertImplicit (ec, expr, TypeManager.int64_type, loc);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				}

				e = ConvertImplicit (ec, expr, TypeManager.double_type, loc);
				if (e != null){
					expr = e;
					type = e.Type;
					return this;
				}

				error23 (expr_type);
				return null;
			}

			//
			// The operand of the prefix/postfix increment decrement operators
			// should be an expression that is classified as a variable,
			// a property access or an indexer access
			//
			if (oper == Operator.PreDecrement || oper == Operator.PreIncrement ||
			    oper == Operator.PostDecrement || oper == Operator.PostIncrement){
				if (expr.ExprClass == ExprClass.Variable){
					if (IsIncrementableNumber (expr_type) ||
					    expr_type == TypeManager.decimal_type){
						type = expr_type;
						return this;
					}
				} else if (expr.ExprClass == ExprClass.IndexerAccess){
					//
					// FIXME: Verify that we have both get and set methods
					//
					throw new Exception ("Implement me");
				} else if (expr.ExprClass == ExprClass.PropertyAccess){
					//
					// FIXME: Verify that we have both get and set methods
					//
					throw new Exception ("Implement me");
				} else {
					report118 (loc, expr, "variable, indexer or property access");
				}
			}

			if (oper == Operator.AddressOf){
				if (expr.ExprClass != ExprClass.Variable){
					Error (211, "Cannot take the address of non-variables");
					return null;
				}
				type = Type.GetType (expr.Type.ToString () + "*");
			}
			
			Error (187, "No such operator '" + OperName () + "' defined for type '" +
			       TypeManager.CSharpName (expr_type) + "'");
			return null;

		}

		public override Expression DoResolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);

			if (expr == null)
				return null;

			eclass = ExprClass.Value;
			return ResolveOperator (ec);
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type expr_type = expr.Type;

			if (method != null) {

				// Note that operators are static anyway
				
				if (Arguments != null) 
					Invocation.EmitArguments (ec, method, Arguments);

				//
				// Post increment/decrement operations need a copy at this
				// point.
				//
				if (oper == Operator.PostDecrement || oper == Operator.PostIncrement)
					ig.Emit (OpCodes.Dup);
				

				ig.Emit (OpCodes.Call, (MethodInfo) method);

				//
				// Pre Increment and Decrement operators
				//
				if (oper == Operator.PreIncrement || oper == Operator.PreDecrement){
					ig.Emit (OpCodes.Dup);
				}
				
				//
				// Increment and Decrement should store the result
				//
				if (oper == Operator.PreDecrement || oper == Operator.PreIncrement ||
				    oper == Operator.PostDecrement || oper == Operator.PostIncrement){
					((LValue) expr).Store (ec);
				}
				return;
			}
			
			switch (oper) {
			case Operator.Addition:
				throw new Exception ("This should be caught by Resolve");
				
			case Operator.Subtraction:
				expr.Emit (ec);
				ig.Emit (OpCodes.Neg);
				break;
				
			case Operator.Negate:
				expr.Emit (ec);
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ceq);
				break;
				
			case Operator.BitComplement:
				expr.Emit (ec);
				ig.Emit (OpCodes.Not);
				break;
				
			case Operator.AddressOf:
				((MemoryLocation)expr).AddressOf (ec);
				break;
				
			case Operator.Indirection:
				throw new Exception ("Not implemented yet");
				
			case Operator.PreIncrement:
			case Operator.PreDecrement:
				if (expr.ExprClass == ExprClass.Variable){
					//
					// Resolve already verified that it is an "incrementable"
					// 
					expr.Emit (ec);
					ig.Emit (OpCodes.Ldc_I4_1);
					
					if (oper == Operator.PreDecrement)
						ig.Emit (OpCodes.Sub);
					else
						ig.Emit (OpCodes.Add);
					ig.Emit (OpCodes.Dup);
					((LValue) expr).Store (ec);
				} else {
					throw new Exception ("Handle Indexers and Properties here");
				}
				break;
				
			case Operator.PostIncrement:
			case Operator.PostDecrement:
				if (expr.ExprClass == ExprClass.Variable){
					//
					// Resolve already verified that it is an "incrementable"
					// 
					expr.Emit (ec);
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Ldc_I4_1);
					
					if (oper == Operator.PostDecrement)
						ig.Emit (OpCodes.Sub);
					else
						ig.Emit (OpCodes.Add);
					((LValue) expr).Store (ec);
				} else {
					throw new Exception ("Handle Indexers and Properties here");
				}
				break;
				
			default:
				throw new Exception ("This should not happen: Operator = "
						     + oper.ToString ());
			}
		}
		

		public override void EmitStatement (EmitContext ec)
		{
			//
			// FIXME: we should rewrite this code to generate
			// better code for ++ and -- as we know we wont need
			// the values on the stack
			//
			Emit (ec);
			ec.ig.Emit (OpCodes.Pop);
		}
	}
	
	public class Probe : Expression {
		public readonly string ProbeType;
		public readonly Operator Oper;
		Expression expr;
		Type probe_type;
		
		public enum Operator {
			Is, As
		}
		
		public Probe (Operator oper, Expression expr, string probe_type)
		{
			Oper = oper;
			ProbeType = probe_type;
			this.expr = expr;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			probe_type = ec.TypeContainer.LookupType (ProbeType, false);

			if (probe_type == null)
				return null;

			expr = expr.Resolve (ec);
			
			type = TypeManager.bool_type;
			eclass = ExprClass.Value;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			expr.Emit (ec);
			
			if (Oper == Operator.Is){
				ig.Emit (OpCodes.Isinst, probe_type);
				ig.Emit (OpCodes.Ldnull);
				ig.Emit (OpCodes.Cgt_Un);
			} else {
				ig.Emit (OpCodes.Isinst, probe_type);
			}
		}
	}

	// <summary>
	//   This represents a typecast in the source language.
	//
	//   FIXME: Cast expressions have an unusual set of parsing
	//   rules, we need to figure those out.
	// </summary>
	public class Cast : Expression {
		string target_type;
		Expression expr;
		Location   loc;
			
		public Cast (string cast_type, Expression expr, Location loc)
		{
			this.target_type = cast_type;
			this.expr = expr;
			this.loc = loc;
		}

		public string TargetType {
			get {
				return target_type;
			}
		}

		public Expression Expr {
			get {
				return expr;
			}
			set {
				expr = value;
			}
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return null;
			
			type = ec.TypeContainer.LookupType (target_type, false);
			eclass = ExprClass.Value;
			
			if (type == null)
				return null;

			expr = ConvertExplicit (ec, expr, type, loc);

			return expr;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// This one will never happen
			//
			throw new Exception ("Should not happen");
		}
	}

	public class Binary : Expression {
		public enum Operator {
			Multiply, Division, Modulus,
			Addition, Subtraction,
			LeftShift, RightShift,
			LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual, 
			Equality, Inequality,
			BitwiseAnd,
			ExclusiveOr,
			BitwiseOr,
			LogicalAnd,
			LogicalOr
		}

		Operator oper;
		Expression left, right;
		MethodBase method;
		ArrayList  Arguments;
		Location   loc;
		

		public Binary (Operator oper, Expression left, Expression right, Location loc)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
			this.loc = loc;
		}

		public Operator Oper {
			get {
				return oper;
			}
			set {
				oper = value;
			}
		}
		
		public Expression Left {
			get {
				return left;
			}
			set {
				left = value;
			}
		}

		public Expression Right {
			get {
				return right;
			}
			set {
				right = value;
			}
		}


		// <summary>
		//   Returns a stringified representation of the Operator
		// </summary>
		string OperName ()
		{
			switch (oper){
			case Operator.Multiply:
				return "*";
			case Operator.Division:
				return "/";
			case Operator.Modulus:
				return "%";
			case Operator.Addition:
				return "+";
			case Operator.Subtraction:
				return "-";
			case Operator.LeftShift:
				return "<<";
			case Operator.RightShift:
				return ">>";
			case Operator.LessThan:
				return "<";
			case Operator.GreaterThan:
				return ">";
			case Operator.LessThanOrEqual:
				return "<=";
			case Operator.GreaterThanOrEqual:
				return ">=";
			case Operator.Equality:
				return "==";
			case Operator.Inequality:
				return "!=";
			case Operator.BitwiseAnd:
				return "&";
			case Operator.BitwiseOr:
				return "|";
			case Operator.ExclusiveOr:
				return "^";
			case Operator.LogicalOr:
				return "||";
			case Operator.LogicalAnd:
				return "&&";
			}

			return oper.ToString ();
		}

		Expression ForceConversion (EmitContext ec, Expression expr, Type target_type)
		{
			if (expr.Type == target_type)
				return expr;

			return ConvertImplicit (ec, expr, target_type, new Location (-1));
		}
		
		//
		// Note that handling the case l == Decimal || r == Decimal
		// is taken care of by the Step 1 Operator Overload resolution.
		//
		void DoNumericPromotions (EmitContext ec, Type l, Type r)
		{
			if (l == TypeManager.double_type || r == TypeManager.double_type){
				//
				// If either operand is of type double, the other operand is
				// conveted to type double.
				//
				if (r != TypeManager.double_type)
					right = ConvertImplicit (ec, right, TypeManager.double_type, loc);
				if (l != TypeManager.double_type)
					left = ConvertImplicit (ec, left, TypeManager.double_type, loc);
				
				type = TypeManager.double_type;
			} else if (l == TypeManager.float_type || r == TypeManager.float_type){
				//
				// if either operand is of type float, th eother operand is
				// converd to type float.
				//
				if (r != TypeManager.double_type)
					right = ConvertImplicit (ec, right, TypeManager.float_type, loc);
				if (l != TypeManager.double_type)
					left = ConvertImplicit (ec, left, TypeManager.float_type, loc);
				type = TypeManager.float_type;
			} else if (l == TypeManager.uint64_type || r == TypeManager.uint64_type){
				Expression e;
				Type other;
				//
				// If either operand is of type ulong, the other operand is
				// converted to type ulong.  or an error ocurrs if the other
				// operand is of type sbyte, short, int or long
				//
				
				if (l == TypeManager.uint64_type){
					if (r != TypeManager.uint64_type && right is IntLiteral){
						e = TryImplicitIntConversion (l, (IntLiteral) right);
						if (e != null)
							right = e;
					}
					other = right.Type;
				} else {
					if (left is IntLiteral){
						e = TryImplicitIntConversion (r, (IntLiteral) left);
						if (e != null)
							left = e;
					}
					other = left.Type;
				}

				if ((other == TypeManager.sbyte_type) ||
				    (other == TypeManager.short_type) ||
				    (other == TypeManager.int32_type) ||
				    (other == TypeManager.int64_type)){
					string oper = OperName ();
					
					Error (34, loc, "Operator `" + OperName ()
					       + "' is ambiguous on operands of type `"
					       + TypeManager.CSharpName (l) + "' "
					       + "and `" + TypeManager.CSharpName (r)
					       + "'");
				}
				type = TypeManager.uint64_type;
			} else if (l == TypeManager.int64_type || r == TypeManager.int64_type){
				//
				// If either operand is of type long, the other operand is converted
				// to type long.
				//
				if (l != TypeManager.int64_type)
					left = ConvertImplicit (ec, left, TypeManager.int64_type, loc);
				if (r != TypeManager.int64_type)
					right = ConvertImplicit (ec, right, TypeManager.int64_type, loc);

				type = TypeManager.int64_type;
			} else if (l == TypeManager.uint32_type || r == TypeManager.uint32_type){
				//
				// If either operand is of type uint, and the other
				// operand is of type sbyte, short or int, othe operands are
				// converted to type long.
				//
				Type other = null;
				
				if (l == TypeManager.uint32_type)
					other = r;
				else if (r == TypeManager.uint32_type)
					other = l;

				if ((other == TypeManager.sbyte_type) ||
				    (other == TypeManager.short_type) ||
				    (other == TypeManager.int32_type)){
					left = ForceConversion (ec, left, TypeManager.int64_type);
					right = ForceConversion (ec, right, TypeManager.int64_type);
					type = TypeManager.int64_type;
				} else {
					//
					// if either operand is of type uint, the other
					// operand is converd to type uint
					//
					left = ForceConversion (ec, left, TypeManager.uint32_type);
					right = ForceConversion (ec, right, TypeManager.uint32_type);
					type = TypeManager.uint32_type;
				} 
			} else if (l == TypeManager.decimal_type || r == TypeManager.decimal_type){
				if (l != TypeManager.decimal_type)
					left = ConvertImplicit (ec, left, TypeManager.decimal_type, loc);
				if (r != TypeManager.decimal_type)
					right = ConvertImplicit (ec, right, TypeManager.decimal_type, loc);

				type = TypeManager.decimal_type;
			} else {
				Expression l_tmp, r_tmp;

				l_tmp = ForceConversion (ec, left, TypeManager.int32_type);
				if (l_tmp == null) {
					error19 ();
					left = l_tmp;
					return;
				}
				
				r_tmp = ForceConversion (ec, right, TypeManager.int32_type);
				if (r_tmp == null) {
					error19 ();
					right = r_tmp;
					return;
				}
				
				type = TypeManager.int32_type;
			}
		}

		void error19 ()
		{
			Error (19, loc,
			       "Operator " + OperName () + " cannot be applied to operands of type `" +
			       TypeManager.CSharpName (left.Type) + "' and `" +
			       TypeManager.CSharpName (right.Type) + "'");
						     
		}
		
		Expression CheckShiftArguments (EmitContext ec)
		{
			Expression e;
			Type l = left.Type;
			Type r = right.Type;

			e = ForceConversion (ec, right, TypeManager.int32_type);
			if (e == null){
				error19 ();
				return null;
			}
			right = e;

			if (((e = ConvertImplicit (ec, left, TypeManager.int32_type, loc)) != null) ||
			    ((e = ConvertImplicit (ec, left, TypeManager.uint32_type, loc)) != null) ||
			    ((e = ConvertImplicit (ec, left, TypeManager.int64_type, loc)) != null) ||
			    ((e = ConvertImplicit (ec, left, TypeManager.uint64_type, loc)) != null)){
				left = e;
				type = e.Type;

				return this;
			}
			error19 ();
			return null;
		}
		
		Expression ResolveOperator (EmitContext ec)
		{
			Type l = left.Type;
			Type r = right.Type;

			//
			// Step 1: Perform Operator Overload location
			//
			Expression left_expr, right_expr;
			
			string op = "op_" + oper;

			left_expr = MemberLookup (ec, l, op, false, loc);
			if (left_expr == null && l.BaseType != null)
				left_expr = MemberLookup (ec, l.BaseType, op, false, loc);
			
			right_expr = MemberLookup (ec, r, op, false, loc);
			if (right_expr == null && r.BaseType != null)
				right_expr = MemberLookup (ec, r.BaseType, op, false, loc);
			
			MethodGroupExpr union = Invocation.MakeUnionSet (left_expr, right_expr);
			
			if (union != null) {
				Arguments = new ArrayList ();
				Arguments.Add (new Argument (left, Argument.AType.Expression));
				Arguments.Add (new Argument (right, Argument.AType.Expression));
				
				method = Invocation.OverloadResolve (ec, union, Arguments, loc);
				if (method != null) {
					MethodInfo mi = (MethodInfo) method;
					type = mi.ReturnType;
					return this;
				} else {
					error19 ();
					return null;
				}
			}	

			//
			// Step 2: Default operations on CLI native types.
			//
			
			// Only perform numeric promotions on:
			// +, -, *, /, %, &, |, ^, ==, !=, <, >, <=, >=
			//
			if (oper == Operator.Addition){
				//
				// If any of the arguments is a string, cast to string
				//
				if (l == TypeManager.string_type){
					if (r == TypeManager.string_type){
						// string + string
						method = TypeManager.string_concat_string_string;
					} else {
						// string + object
						method = TypeManager.string_concat_object_object;
						right = ConvertImplicit (ec, right,
									 TypeManager.object_type, loc);
					}
					type = TypeManager.string_type;

					Arguments = new ArrayList ();
					Arguments.Add (new Argument (left, Argument.AType.Expression));
					Arguments.Add (new Argument (right, Argument.AType.Expression));

					return this;
					
				} else if (r == TypeManager.string_type){
					// object + string
					method = TypeManager.string_concat_object_object;
					Arguments = new ArrayList ();
					Arguments.Add (new Argument (left, Argument.AType.Expression));
					Arguments.Add (new Argument (right, Argument.AType.Expression));

					left = ConvertImplicit (ec, left, TypeManager.object_type, loc);
					type = TypeManager.string_type;

					return this;
				}

				//
				// FIXME: is Delegate operator + (D x, D y) handled?
				//
			}
			
			if (oper == Operator.LeftShift || oper == Operator.RightShift)
				return CheckShiftArguments (ec);

			if (oper == Operator.LogicalOr || oper == Operator.LogicalAnd){
				if (l != TypeManager.bool_type || r != TypeManager.bool_type)
					error19 ();

				type = TypeManager.bool_type;
				return this;
			} 

			//
			// We are dealing with numbers
			//

			DoNumericPromotions (ec, l, r);

			if (left == null || right == null)
				return null;

			
			if (oper == Operator.BitwiseAnd ||
			    oper == Operator.BitwiseOr ||
			    oper == Operator.ExclusiveOr){
				if (!((l == TypeManager.int32_type) ||
				      (l == TypeManager.uint32_type) ||
				      (l == TypeManager.int64_type) ||
				      (l == TypeManager.uint64_type))){
					error19 ();
					return null;
				}
				type = l;
			}

			if (oper == Operator.Equality ||
			    oper == Operator.Inequality ||
			    oper == Operator.LessThanOrEqual ||
			    oper == Operator.LessThan ||
			    oper == Operator.GreaterThanOrEqual ||
			    oper == Operator.GreaterThan){
				type = TypeManager.bool_type;
			}

			return this;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			left = left.Resolve (ec);
			right = right.Resolve (ec);

			if (left == null || right == null)
				return null;

			if (left.Type == null)
				throw new Exception (
					"Resolve returned non null, but did not set the type! (" +
					left + ")");
			if (right.Type == null)
				throw new Exception (
					"Resolve returned non null, but did not set the type! (" +
					right + ")");

			eclass = ExprClass.Value;

			return ResolveOperator (ec);
		}

		public bool IsBranchable ()
		{
			if (oper == Operator.Equality ||
			    oper == Operator.Inequality ||
			    oper == Operator.LessThan ||
			    oper == Operator.GreaterThan ||
			    oper == Operator.LessThanOrEqual ||
			    oper == Operator.GreaterThanOrEqual){
				return true;
			} else
				return false;
		}

		// <summary>
		//   This entry point is used by routines that might want
		//   to emit a brfalse/brtrue after an expression, and instead
		//   they could use a more compact notation.
		//
		//   Typically the code would generate l.emit/r.emit, followed
		//   by the comparission and then a brtrue/brfalse.  The comparissions
		//   are sometimes inneficient (there are not as complete as the branches
		//   look for the hacks in Emit using double ceqs).
		//
		//   So for those cases we provide EmitBranchable that can emit the
		//   branch with the test
		// </summary>
		public void EmitBranchable (EmitContext ec, int target)
		{
			OpCode opcode;
			bool close_target = false;
			
			left.Emit (ec);
			right.Emit (ec);
			
			switch (oper){
			case Operator.Equality:
				if (close_target)
					opcode = OpCodes.Beq_S;
				else
					opcode = OpCodes.Beq;
				break;

			case Operator.Inequality:
				if (close_target)
					opcode = OpCodes.Bne_Un_S;
				else
					opcode = OpCodes.Bne_Un;
				break;

			case Operator.LessThan:
				if (close_target)
					opcode = OpCodes.Blt_S;
				else
					opcode = OpCodes.Blt;
				break;

			case Operator.GreaterThan:
				if (close_target)
					opcode = OpCodes.Bgt_S;
				else
					opcode = OpCodes.Bgt;
				break;

			case Operator.LessThanOrEqual:
				if (close_target)
					opcode = OpCodes.Ble_S;
				else
					opcode = OpCodes.Ble;
				break;

			case Operator.GreaterThanOrEqual:
				if (close_target)
					opcode = OpCodes.Bge_S;
				else
					opcode = OpCodes.Ble;
				break;

			default:
				throw new Exception ("EmitBranchable called on non-EmitBranchable operator: "
						     + oper.ToString ());
			}

			ec.ig.Emit (opcode, target);
		}
		
		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type l = left.Type;
			Type r = right.Type;
			OpCode opcode;

			if (method != null) {

				// Note that operators are static anyway
				
				if (Arguments != null) 
					Invocation.EmitArguments (ec, method, Arguments);
				
				if (method is MethodInfo)
					ig.Emit (OpCodes.Call, (MethodInfo) method);
				else
					ig.Emit (OpCodes.Call, (ConstructorInfo) method);

				return;
			}
			
			left.Emit (ec);
			right.Emit (ec);

			switch (oper){
			case Operator.Multiply:
				if (ec.CheckState){
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Mul_Ovf;
					else if (l==TypeManager.uint32_type || l==TypeManager.uint64_type)
						opcode = OpCodes.Mul_Ovf_Un;
					else
						opcode = OpCodes.Mul;
				} else
					opcode = OpCodes.Mul;

				break;

			case Operator.Division:
				if (l == TypeManager.uint32_type || l == TypeManager.uint64_type)
					opcode = OpCodes.Div_Un;
				else
					opcode = OpCodes.Div;
				break;

			case Operator.Modulus:
				if (l == TypeManager.uint32_type || l == TypeManager.uint64_type)
					opcode = OpCodes.Rem_Un;
				else
					opcode = OpCodes.Rem;
				break;

			case Operator.Addition:
				if (ec.CheckState){
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Add_Ovf;
					else if (l==TypeManager.uint32_type || l==TypeManager.uint64_type)
						opcode = OpCodes.Add_Ovf_Un;
					else
						opcode = OpCodes.Mul;
				} else
					opcode = OpCodes.Add;
				break;

			case Operator.Subtraction:
				if (ec.CheckState){
					if (l == TypeManager.int32_type || l == TypeManager.int64_type)
						opcode = OpCodes.Sub_Ovf;
					else if (l==TypeManager.uint32_type || l==TypeManager.uint64_type)
						opcode = OpCodes.Sub_Ovf_Un;
					else
						opcode = OpCodes.Sub;
				} else
					opcode = OpCodes.Sub;
				break;

			case Operator.RightShift:
				opcode = OpCodes.Shr;
				break;
				
			case Operator.LeftShift:
				opcode = OpCodes.Shl;
				break;

			case Operator.Equality:
				opcode = OpCodes.Ceq;
				break;

			case Operator.Inequality:
				ec.ig.Emit (OpCodes.Ceq);
				ec.ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.LessThan:
				opcode = OpCodes.Clt;
				break;

			case Operator.GreaterThan:
				opcode = OpCodes.Cgt;
				break;

			case Operator.LessThanOrEqual:
				ec.ig.Emit (OpCodes.Cgt);
				ec.ig.Emit (OpCodes.Ldc_I4_0);
				
				opcode = OpCodes.Ceq;
				break;

			case Operator.GreaterThanOrEqual:
				ec.ig.Emit (OpCodes.Clt);
				ec.ig.Emit (OpCodes.Ldc_I4_1);
				
				opcode = OpCodes.Sub;
				break;

			case Operator.LogicalOr:
			case Operator.BitwiseOr:
				opcode = OpCodes.Or;
				break;

			case Operator.LogicalAnd:
			case Operator.BitwiseAnd:
				opcode = OpCodes.And;
				break;

			case Operator.ExclusiveOr:
				opcode = OpCodes.Xor;
				break;

			default:
				throw new Exception ("This should not happen: Operator = "
						     + oper.ToString ());
			}

			ig.Emit (opcode);
		}
	}

	public class Conditional : Expression {
		Expression expr, trueExpr, falseExpr;
		Location loc;
		
		public Conditional (Expression expr, Expression trueExpr, Expression falseExpr, Location l)
		{
			this.expr = expr;
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
			this.loc = l;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public Expression TrueExpr {
			get {
				return trueExpr;
			}
		}

		public Expression FalseExpr {
			get {
				return falseExpr;
			}
		}

		public override Expression DoResolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);

			if (expr.Type != TypeManager.bool_type)
				expr = Expression.ConvertImplicitRequired (
					ec, expr, TypeManager.bool_type, loc);
			
			trueExpr = trueExpr.Resolve (ec);
			falseExpr = falseExpr.Resolve (ec);

			if (expr == null || trueExpr == null || falseExpr == null)
				return null;
			
			if (trueExpr.Type == falseExpr.Type)
				type = trueExpr.Type;
			else {
				Expression conv;

				//
				// First, if an implicit conversion exists from trueExpr
				// to falseExpr, then the result type is of type falseExpr.Type
				//
				conv = ConvertImplicit (ec, trueExpr, falseExpr.Type, loc);
				if (conv != null){
					type = falseExpr.Type;
					trueExpr = conv;
				} else if ((conv = ConvertImplicit(ec, falseExpr,trueExpr.Type,loc))!= null){
					type = trueExpr.Type;
					falseExpr = conv;
				} else {
					Error (173, loc, "The type of the conditional expression can " +
					       "not be computed because there is no implicit conversion" +
					       " from `" + TypeManager.CSharpName (trueExpr.Type) + "'" +
					       " and `" + TypeManager.CSharpName (falseExpr.Type) + "'");
					return null;
				}
			}

			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_target = ig.DefineLabel ();
			Label end_target = ig.DefineLabel ();

			expr.Emit (ec);
			ig.Emit (OpCodes.Brfalse, false_target);
			trueExpr.Emit (ec);
			ig.Emit (OpCodes.Br, end_target);
			ig.MarkLabel (false_target);
			falseExpr.Emit (ec);
			ig.MarkLabel (end_target);
		}
	}

	//
	// SimpleName expressions are initially formed of a single
	// word and it only happens at the beginning of the expression.
	//
	// The expression will try to be bound to a Field, a Method
	// group or a Property.  If those fail we pass the name to our
	// caller and the SimpleName is compounded to perform a type
	// lookup.  The idea behind this process is that we want to avoid
	// creating a namespace map from the assemblies, as that requires
	// the GetExportedTypes function to be called and a hashtable to
	// be constructed which reduces startup time.  If later we find
	// that this is slower, we should create a `NamespaceExpr' expression
	// that fully participates in the resolution process. 
	//
	// For example `System.Console.WriteLine' is decomposed into
	// MemberAccess (MemberAccess (SimpleName ("System"), "Console"), "WriteLine")
	//
	// The first SimpleName wont produce a match on its own, so it will
	// be turned into:
	// MemberAccess (SimpleName ("System.Console"), "WriteLine").
	//
	// System.Console will produce a TypeExpr match.
	//
	// The downside of this is that we might be hitting `LookupType' too many
	// times with this scheme.
	//
	public class SimpleName : Expression {
		public readonly string Name;
		public readonly Location Location;
		
		public SimpleName (string name, Location l)
		{
			Name = name;
			Location = l;
		}

		public static void Error120 (Location l, string name)
		{
			Report.Error (
				120, l,
				"An object reference is required " +
				"for the non-static field `"+name+"'");
		}
		
		//
		// Checks whether we are trying to access an instance
		// property, method or field from a static body.
		//
		Expression MemberStaticCheck (Expression e)
		{
			if (e is FieldExpr){
				FieldInfo fi = ((FieldExpr) e).FieldInfo;
				
				if (!fi.IsStatic){
					Error120 (Location, Name);
					return null;
				}
			} else if (e is MethodGroupExpr){
				MethodGroupExpr mg = (MethodGroupExpr) e;

				if (!mg.RemoveInstanceMethods ()){
					Error120 (Location, mg.Methods [0].Name);
					return null;
				}
				return e;
			} else if (e is PropertyExpr){
				if (!((PropertyExpr) e).IsStatic){
					Error120 (Location, Name);
					return null;
				}
			}

			return e;
		}
		
		//
		// 7.5.2: Simple Names. 
		//
		// Local Variables and Parameters are handled at
		// parse time, so they never occur as SimpleNames.
		//
		public override Expression DoResolve (EmitContext ec)
		{
			Expression e;

			//
			// Stage 1: Performed by the parser (binding to local or parameters).
			//

			//
			// Stage 2: Lookup members
			//
			e = MemberLookup (ec, ec.TypeContainer.TypeBuilder, Name, true, Location);
			if (e == null){
				//
				// Stage 3: Lookup symbol in the various namespaces. 
				// 
				Type t;
				
				if ((t = ec.TypeContainer.LookupType (Name, true)) != null)
					return new TypeExpr (t);

				//
				// Stage 3 part b: Lookup up if we are an alias to a type
				// or a namespace.
				//
				// Since we are cheating: we only do the Alias lookup for
				// namespaces if the name does not include any dots in it
				//
				
				// IMPLEMENT ME.  Read mcs/mcs/TODO for ideas, or rewrite
				// using NamespaceExprs (dunno how that fixes the alias
				// per-file though).
				
				// No match, maybe our parent can compose us
				// into something meaningful.
				//
				return this;
			}

			// Step 2, continues here.
			if (e is TypeExpr)
				return e;

			if (e is FieldExpr){
				FieldExpr fe = (FieldExpr) e;
				
				if (!fe.FieldInfo.IsStatic)
					fe.InstanceExpression = new This (Location.Null);
			} 				

			if (ec.IsStatic)
				return MemberStaticCheck (e);
			else
				return e;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// If this is ever reached, then we failed to
			// find the name as a namespace
			//

			Error (103, Location, "The name `" + Name +
			       "' does not exist in the class `" +
			       ec.TypeContainer.Name + "'");
		}
	}

	// <summary>
	//   A simple interface that should be implemeneted by LValues
	// </summary>
	public interface LValue {

		// <summary>
		//   The Store method should store the contents of the top
		//   of the stack into the storage that is implemented by
		//   the particular implementation of LValue
		// </summary>
		void Store     (EmitContext ec);

		// <summary>
		//   Allows an LValue to perform any necessary semantic
		//   analysis in an lvalue-context.
		// </summary>

		Expression LValueResolve (EmitContext ec);
	}

	// <summary>
	//   This interface is implemented by variables
	// </summary>
	public interface MemoryLocation {
		// <summary>
		//   The AddressOf method should generate code that loads
		//   the address of the LValue and leaves it on the stack
		// </summary>
		void AddressOf (EmitContext ec);
	}
	
	public class LocalVariableReference : Expression, LValue, MemoryLocation {
		public readonly string Name;
		public readonly Block Block;

		VariableInfo variable_info;
		
		public LocalVariableReference (Block block, string name)
		{
			Block = block;
			Name = name;
			eclass = ExprClass.Variable;
		}

		public VariableInfo VariableInfo {
			get {
				if (variable_info == null)
					variable_info = Block.GetVariableInfo (Name);
				return variable_info;
			}
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			VariableInfo vi = VariableInfo;

			type = vi.VariableType;
			return this;
		}

		public Expression LValueResolve (EmitContext ec)
		{
			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			VariableInfo vi = VariableInfo;
			ILGenerator ig = ec.ig;
			int idx = vi.Idx;

			vi.Used = true;
			
			switch (idx){
			case 0:
				ig.Emit (OpCodes.Ldloc_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Ldloc_1);
				break;

			case 2:
				ig.Emit (OpCodes.Ldloc_2);
				break;

			case 3:
				ig.Emit (OpCodes.Ldloc_3);
				break;

			default:
				if (idx <= 255)
					ig.Emit (OpCodes.Ldloc_S, (byte) idx);
				else
					ig.Emit (OpCodes.Ldloc, idx);
				break;
			}
		}

		public static void Store (ILGenerator ig, int idx)
		{
			switch (idx){
			case 0:
				ig.Emit (OpCodes.Stloc_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Stloc_1);
				break;
				
			case 2:
				ig.Emit (OpCodes.Stloc_2);
				break;
				
			case 3:
				ig.Emit (OpCodes.Stloc_3);
				break;
				
			default:
				if (idx <= 255)
					ig.Emit (OpCodes.Stloc_S, (byte) idx);
				else
					ig.Emit (OpCodes.Stloc, idx);
				break;
			}
		}
		
		public void Store (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			VariableInfo vi = VariableInfo;

			vi.Assigned = true;

			// Funny seems the above generates optimal code for us, but
			// seems to take too long to generate what we need.
			// ig.Emit (OpCodes.Stloc, vi.LocalBuilder);

			Store (ig, vi.Idx);
		}

		public void AddressOf (EmitContext ec)
		{
			VariableInfo vi = VariableInfo;
			int idx = vi.Idx;

			vi.Used = true;
			vi.Assigned = true;
			
			if (idx <= 255)
				ec.ig.Emit (OpCodes.Ldloca_S, (byte) idx);
			else
				ec.ig.Emit (OpCodes.Ldloca, idx);
		}
	}

	public class ParameterReference : Expression, LValue, MemoryLocation {
		public readonly Parameters Pars;
		public readonly String Name;
		public readonly int Idx;
		int arg_idx;
		
		public ParameterReference (Parameters pars, int idx, string name)
		{
			Pars = pars;
			Idx  = idx;
			Name = name;
			eclass = ExprClass.Variable;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			Type [] types = Pars.GetParameterInfo (ec.TypeContainer);

			type = types [Idx];

			arg_idx = Idx;
			if (!ec.IsStatic)
				arg_idx++;
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			if (arg_idx <= 255)
				ec.ig.Emit (OpCodes.Ldarg_S, (byte) arg_idx);
			else
				ec.ig.Emit (OpCodes.Ldarg, arg_idx);
		}

		public void Store (EmitContext ec)
		{
			if (arg_idx <= 255)
				ec.ig.Emit (OpCodes.Starg_S, (byte) arg_idx);
			else
				ec.ig.Emit (OpCodes.Starg, arg_idx);
			
		}

		public void AddressOf (EmitContext ec)
		{
			if (arg_idx <= 255)
				ec.ig.Emit (OpCodes.Ldarga_S, (byte) arg_idx);
			else
				ec.ig.Emit (OpCodes.Ldarga, arg_idx);
		}

		public Expression LValueResolve (EmitContext ec)
		{
			return this;
		}
	}
	
	// <summary>
	//   Used for arguments to New(), Invocation()
	// </summary>
	public class Argument {
		public enum AType {
			Expression,
			Ref,
			Out
		};

		public readonly AType Type;
		public Expression expr;

		public Argument (Expression expr, AType type)
		{
			this.expr = expr;
			this.Type = type;
		}

		public Expression Expr {
			get {
				return expr;
			}

			set {
				expr = value;
			}
		}

		public bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);

			return expr != null;
		}

		public void Emit (EmitContext ec)
		{
			expr.Emit (ec);
		}
	}

	// <summary>
	//   Invocation of methods or delegates.
	// </summary>
	public class Invocation : ExpressionStatement {
		public readonly ArrayList Arguments;
		public readonly Location Location;
		
		Expression expr;
		MethodBase method = null;
			
		static Hashtable method_parameter_cache;

		static Invocation ()
		{
			method_parameter_cache = new Hashtable ();
		}
			
		//
		// arguments is an ArrayList, but we do not want to typecast,
		// as it might be null.
		//
		// FIXME: only allow expr to be a method invocation or a
		// delegate invocation (7.5.5)
		//
		public Invocation (Expression expr, ArrayList arguments, Location l)
		{
			this.expr = expr;
			Arguments = arguments;
			Location = l;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		// <summary>
		//   Returns the Parameters (a ParameterData interface) for the
		//   Method `mb'
		// </summary>
		public static ParameterData GetParameterData (MethodBase mb)
		{
			object pd = method_parameter_cache [mb];

			if (pd != null)
				return (ParameterData) pd;

			if (mb is MethodBuilder || mb is ConstructorBuilder){
				MethodCore mc = TypeContainer.LookupMethodByBuilder (mb);

				InternalParameters ip = mc.ParameterInfo;
				method_parameter_cache [mb] = ip;

				return (ParameterData) ip;
			} else {
				ParameterInfo [] pi = mb.GetParameters ();
				ReflectionParameters rp = new ReflectionParameters (pi);
				method_parameter_cache [mb] = rp;

				return (ParameterData) rp;
			}
		}

		// <summary>
		//   Tells whether a user defined conversion from Type `from' to
		//   Type `to' exists.
		//
		//   FIXME: we could implement a cache here. 
		// </summary>
		static bool ConversionExists (EmitContext ec, Type from, Type to, Location loc)
		{
			// Locate user-defined implicit operators

			Expression mg;
			
			mg = MemberLookup (ec, to, "op_Implicit", false, loc);

			if (mg != null) {
				MethodGroupExpr me = (MethodGroupExpr) mg;
				
				for (int i = me.Methods.Length; i > 0;) {
					i--;
					MethodBase mb = me.Methods [i];
					ParameterData pd = GetParameterData (mb);
					
					if (from == pd.ParameterType (0))
						return true;
				}
			}

			mg = MemberLookup (ec, from, "op_Implicit", false, loc);

			if (mg != null) {
				MethodGroupExpr me = (MethodGroupExpr) mg;

				for (int i = me.Methods.Length; i > 0;) {
					i--;
					MethodBase mb = me.Methods [i];
					MethodInfo mi = (MethodInfo) mb;
					
					if (mi.ReturnType == to)
						return true;
				}
			}
			
			return false;
		}
		
		// <summary>
		//  Determines "better conversion" as specified in 7.4.2.3
		//  Returns : 1 if a->p is better
		//            0 if a->q or neither is better 
		// </summary>
		static int BetterConversion (EmitContext ec, Argument a, Type p, Type q, bool use_standard,
					     Location loc)
		{
			Type argument_type = a.Expr.Type;
			Expression argument_expr = a.Expr;

			if (argument_type == null)
				throw new Exception ("Expression of type " + a.Expr + " does not resolve its type");

			if (p == q)
				return 0;
			
			if (argument_type == p)
				return 1;

			if (argument_type == q)
				return 0;

			//
			// Now probe whether an implicit constant expression conversion
			// can be used.
			//
			// An implicit constant expression conversion permits the following
			// conversions:
			//
			//    * A constant-expression of type `int' can be converted to type
			//      sbyte, byute, short, ushort, uint, ulong provided the value of
			//      of the expression is withing the range of the destination type.
			//
			//    * A constant-expression of type long can be converted to type
			//      ulong, provided the value of the constant expression is not negative
			//
			// FIXME: Note that this assumes that constant folding has
			// taken place.  We dont do constant folding yet.
			//

			if (argument_expr is IntLiteral){
				IntLiteral ei = (IntLiteral) argument_expr;
				int value = ei.Value;
				
				if (p == TypeManager.sbyte_type){
					if (value >= SByte.MinValue && value <= SByte.MaxValue)
						return 1;
				} else if (p == TypeManager.byte_type){
					if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
						return 1;
				} else if (p == TypeManager.short_type){
					if (value >= Int16.MinValue && value <= Int16.MaxValue)
						return 1;
				} else if (p == TypeManager.ushort_type){
					if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
						return 1;
				} else if (p == TypeManager.uint32_type){
					//
					// we can optimize this case: a positive int32
					// always fits on a uint32
					//
					if (value >= 0)
						return 1;
				} else if (p == TypeManager.uint64_type){
					//
					// we can optimize this case: a positive int32
					// always fits on a uint64
					//
					if (value >= 0)
						return 1;
				}
			} else if (argument_type == TypeManager.int64_type && argument_expr is LongLiteral){
				LongLiteral ll = (LongLiteral) argument_expr;
				
				if (p == TypeManager.uint64_type){
					if (ll.Value > 0)
						return 1;
				}
			}

			if (q == null) {

				Expression tmp;

				if (use_standard)
					tmp = ConvertImplicitStandard (ec, argument_expr, p, loc);
				else
					tmp = ConvertImplicit (ec, argument_expr, p, loc);

				if (tmp != null)
					return 1;
				else
					return 0;

			}

			if (ConversionExists (ec, p, q, loc) == true &&
			    ConversionExists (ec, q, p, loc) == false)
				return 1;

			if (p == TypeManager.sbyte_type)
				if (q == TypeManager.byte_type || q == TypeManager.ushort_type ||
				    q == TypeManager.uint32_type || q == TypeManager.uint64_type)
					return 1;

			if (p == TypeManager.short_type)
				if (q == TypeManager.ushort_type || q == TypeManager.uint32_type ||
				    q == TypeManager.uint64_type)
					return 1;

			if (p == TypeManager.int32_type)
				if (q == TypeManager.uint32_type || q == TypeManager.uint64_type)
					return 1;

			if (p == TypeManager.int64_type)
				if (q == TypeManager.uint64_type)
					return 1;

			return 0;
		}
		
		// <summary>
		//  Determines "Better function" and returns an integer indicating :
		//  0 if candidate ain't better
		//  1 if candidate is better than the current best match
		// </summary>
		static int BetterFunction (EmitContext ec, ArrayList args,
					   MethodBase candidate, MethodBase best,
					   bool use_standard, Location loc)
		{
			ParameterData candidate_pd = GetParameterData (candidate);
			ParameterData best_pd;
			int argument_count;

			if (args == null)
				argument_count = 0;
			else
				argument_count = args.Count;

			if (candidate_pd.Count == 0 && argument_count == 0)
				return 1;

			if (best == null) {
				if (candidate_pd.Count == argument_count) {
					int x = 0;
					for (int j = argument_count; j > 0;) {
						j--;
						
						Argument a = (Argument) args [j];
						
						x = BetterConversion (
							ec, a, candidate_pd.ParameterType (j), null,
							use_standard, loc);
						
						if (x <= 0)
							break;
					}
					
					if (x > 0)
						return 1;
					else
						return 0;
					
				} else
					return 0;
			}

			best_pd = GetParameterData (best);

			if (candidate_pd.Count == argument_count && best_pd.Count == argument_count) {
				int rating1 = 0, rating2 = 0;
				
				for (int j = argument_count; j > 0;) {
					j--;
					int x, y;
					
					Argument a = (Argument) args [j];

					x = BetterConversion (ec, a, candidate_pd.ParameterType (j),
							      best_pd.ParameterType (j), use_standard, loc);
					y = BetterConversion (ec, a, best_pd.ParameterType (j),
							      candidate_pd.ParameterType (j), use_standard,
							      loc);
					
					rating1 += x;
					rating2 += y;
				}

				if (rating1 > rating2)
					return 1;
				else
					return 0;
			} else
				return 0;
			
		}

		public static string FullMethodDesc (MethodBase mb)
		{
			StringBuilder sb = new StringBuilder (mb.Name);
			ParameterData pd = GetParameterData (mb);
			
			sb.Append (" (");
			for (int i = pd.Count; i > 0;) {
				i--;
				sb.Append (TypeManager.CSharpName (pd.ParameterType (i)));
				if (i != 0)
					sb.Append (", ");
			}
			
			sb.Append (")");
			return sb.ToString ();
		}

		public static MethodGroupExpr MakeUnionSet (Expression mg1, Expression mg2)
		{
			MemberInfo [] miset;
			MethodGroupExpr union;
			
			if (mg1 != null && mg2 != null) {
				
				MethodGroupExpr left_set = null, right_set = null;
				int length1 = 0, length2 = 0;
				
				left_set = (MethodGroupExpr) mg1;
				length1 = left_set.Methods.Length;
				
				right_set = (MethodGroupExpr) mg2;
				length2 = right_set.Methods.Length;

				ArrayList common = new ArrayList ();
				
				for (int i = 0; i < left_set.Methods.Length; i++) {
					for (int j = 0; j < right_set.Methods.Length; j++) {
						if (left_set.Methods [i] == right_set.Methods [j]) 
							common.Add (left_set.Methods [i]);
					}
				}
				
				miset = new MemberInfo [length1 + length2 - common.Count];

				left_set.Methods.CopyTo (miset, 0);

				int k = 0;
				
				for (int j = 0; j < right_set.Methods.Length; j++)
					if (!common.Contains (right_set.Methods [j]))
						miset [length1 + k++] = right_set.Methods [j];
				
				union = new MethodGroupExpr (miset);

				return union;

			} else if (mg1 == null && mg2 != null) {
				
				MethodGroupExpr me = (MethodGroupExpr) mg2; 
				
				miset = new MemberInfo [me.Methods.Length];
				me.Methods.CopyTo (miset, 0);

				union = new MethodGroupExpr (miset);
				
				return union;

			} else if (mg2 == null && mg1 != null) {
				
				MethodGroupExpr me = (MethodGroupExpr) mg1; 
				
				miset = new MemberInfo [me.Methods.Length];
				me.Methods.CopyTo (miset, 0);

				union = new MethodGroupExpr (miset);
				
				return union;
			}
			
			return null;
		}

		// <summary>
		//   Find the Applicable Function Members (7.4.2.1)
		//
		//   me: Method Group expression with the members to select.
		//       it might contain constructors or methods (or anything
		//       that maps to a method).
		//
		//   Arguments: ArrayList containing resolved Argument objects.
		//
		//   loc: The location if we want an error to be reported, or a Null
		//        location for "probing" purposes.
		//
		//   inside_user_defined: controls whether OverloadResolve should use the 
		//   ConvertImplicit or ConvertImplicitStandard during overload resolution.
		//
		//   Returns: The MethodBase (either a ConstructorInfo or a MethodInfo)
		//            that is the best match of me on Arguments.
		//
		// </summary>
		public static MethodBase OverloadResolve (EmitContext ec, MethodGroupExpr me,
							  ArrayList Arguments, Location loc,
							  bool use_standard)
		{
			ArrayList afm = new ArrayList ();
			int best_match_idx = -1;
			MethodBase method = null;
			int argument_count;
			
			for (int i = me.Methods.Length; i > 0; ){
				i--;
				MethodBase candidate  = me.Methods [i];
				int x;

				x = BetterFunction (ec, Arguments, candidate, method, use_standard, loc);
				
				if (x == 0)
					continue;
				else {
					best_match_idx = i;
					method = me.Methods [best_match_idx];
				}
			}

			if (Arguments == null)
				argument_count = 0;
			else
				argument_count = Arguments.Count;
			
			ParameterData pd;
			
			// Now we see if we can at least find a method with the same number of arguments
			// and then try doing implicit conversion on the arguments
			if (best_match_idx == -1) {
				
				for (int i = me.Methods.Length; i > 0;) {
					i--;
					MethodBase mb = me.Methods [i];
					pd = GetParameterData (mb);
					
					if (pd.Count == argument_count) {
						best_match_idx = i;
						method = me.Methods [best_match_idx];
						break;
					} else
						continue;
				}

			}

			if (method == null)
				return null;

			// And now convert implicitly, each argument to the required type
			
			pd = GetParameterData (method);

			for (int j = argument_count; j > 0;) {
				j--;
				Argument a = (Argument) Arguments [j];
				Expression a_expr = a.Expr;
				Type parameter_type = pd.ParameterType (j);
				
				if (a_expr.Type != parameter_type){
					Expression conv;

					if (use_standard)
						conv = ConvertImplicitStandard (ec, a_expr, parameter_type,
										Location.Null);
					else
						conv = ConvertImplicit (ec, a_expr, parameter_type,
									Location.Null);

					if (conv == null){
						if (!Location.IsNull (loc)) {
							Error (1502, loc,
							       "The best overloaded match for method '" + FullMethodDesc (method) +
							       "' has some invalid arguments");
							Error (1503, loc,
							       "Argument " + (j+1) +
							       ": Cannot convert from '" + TypeManager.CSharpName (a_expr.Type)
							       + "' to '" + TypeManager.CSharpName (pd.ParameterType (j)) + "'");
						}
						return null;
					}
					//
					// Update the argument with the implicit conversion
					//
					if (a_expr != conv)
						a.Expr = conv;
				}
			}
			
			return method;
		}

		public static MethodBase OverloadResolve (EmitContext ec, MethodGroupExpr me,
							  ArrayList Arguments, Location loc)
		{
			return OverloadResolve (ec, me, Arguments, loc, false);
		}
			
		public override Expression DoResolve (EmitContext ec)
		{
			//
			// First, resolve the expression that is used to
			// trigger the invocation
			//
			this.expr = expr.Resolve (ec);
			if (this.expr == null)
				return null;

			if (!(this.expr is MethodGroupExpr)){
				report118 (Location, this.expr, "method group");
				return null;
			}

			//
			// Next, evaluate all the expressions in the argument list
			//
			if (Arguments != null){
				for (int i = Arguments.Count; i > 0;){
					--i;
					Argument a = (Argument) Arguments [i];

					if (!a.Resolve (ec))
						return null;
				}
			}

			method = OverloadResolve (ec, (MethodGroupExpr) this.expr, Arguments,
						  Location);

			if (method == null){
				Error (-6, Location,
				       "Could not find any applicable function for this argument list");
				return null;
			}

			if (method is MethodInfo)
				type = ((MethodInfo)method).ReturnType;

			eclass = ExprClass.Value;
			return this;
		}

		public static void EmitArguments (EmitContext ec, MethodBase method, ArrayList Arguments)
		{
			int top;

			if (Arguments != null)
				top = Arguments.Count;
			else
				top = 0;

			for (int i = 0; i < top; i++){
				Argument a = (Argument) Arguments [i];

				a.Emit (ec);
			}
		}

		public static void EmitCall (EmitContext ec,
					     bool is_static, Expression instance_expr,
					     MethodBase method, ArrayList Arguments)
		{
			ILGenerator ig = ec.ig;
			bool struct_call = false;
				
			if (!is_static){
				//
				// If this is ourselves, push "this"
				//
				if (instance_expr == null){
					ig.Emit (OpCodes.Ldarg_0);
				} else {
					//
					// Push the instance expression
					//
					if (instance_expr.Type.IsSubclassOf (TypeManager.value_type)){

						struct_call = true;

						//
						// If the expression is an LValue, then
						// we can optimize and use AddressOf on the
						// return.
						//
						// If not we have to use some temporary storage for
						// it.
						if (instance_expr is MemoryLocation)
							((MemoryLocation) instance_expr).AddressOf (ec);
						else {
							Type t = instance_expr.Type;
							
							instance_expr.Emit (ec);
							LocalBuilder temp = ec.GetTemporaryStorage (t);
							ig.Emit (OpCodes.Stloc, temp);
							ig.Emit (OpCodes.Ldloca, temp);
						}
					} else 
						instance_expr.Emit (ec);
				}
			}

			if (Arguments != null)
				EmitArguments (ec, method, Arguments);

			if (is_static || struct_call){
				if (method is MethodInfo)
					ig.Emit (OpCodes.Call, (MethodInfo) method);
				else
					ig.Emit (OpCodes.Call, (ConstructorInfo) method);
			} else {
				if (method is MethodInfo)
					ig.Emit (OpCodes.Callvirt, (MethodInfo) method);
				else
					ig.Emit (OpCodes.Callvirt, (ConstructorInfo) method);
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			MethodGroupExpr mg = (MethodGroupExpr) this.expr;
			
			EmitCall (ec, method.IsStatic, mg.InstanceExpression, method, Arguments);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);

			// 
			// Pop the return value if there is one
			//
			if (method is MethodInfo){
				if (((MethodInfo)method).ReturnType != TypeManager.void_type)
					ec.ig.Emit (OpCodes.Pop);
			}
		}
	}

	public class New : ExpressionStatement {

		public enum NType {
			Object,
			Array
		};

		public readonly NType     NewType;
		public readonly ArrayList Arguments;
		public readonly string    RequestedType;

		// These are for the case when we have an array
		public readonly string    Rank;
		public readonly ArrayList Initializers;

		Location Location;
		MethodBase method = null;


		//
		// If set, the new expression is for a value_target, and
		// we will not leave anything on the stack.
		//
		Expression value_target;
		
		public New (string requested_type, ArrayList arguments, Location loc)
		{
			RequestedType = requested_type;
			Arguments = arguments;
			NewType = NType.Object;
			Location = loc;
		}

		public New (string requested_type, ArrayList exprs, string rank, ArrayList initializers, Location loc)
		{
			RequestedType = requested_type;
			Rank          = rank;
			Initializers  = initializers;
			NewType       = NType.Array;
			Location      = loc;

			Arguments = new ArrayList ();

			foreach (Expression e in exprs)
				Arguments.Add (new Argument (e, Argument.AType.Expression));
			
		}

		public static string FormLookupType (string base_type, int idx_count, string rank)
		{
			StringBuilder sb = new StringBuilder (base_type);

			sb.Append (rank);

			sb.Append ("[");
			for (int i = 1; i < idx_count; i++)
				sb.Append (",");
			sb.Append ("]");
			
			return sb.ToString ();
		}
		
		public Expression ValueTypeVariable {
			get {
				return value_target;
			}

			set {
				value_target = value;
			}
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			if (NewType == NType.Object) {
				type = ec.TypeContainer.LookupType (RequestedType, false);
				
				if (type == null)
					return null;
				
				Expression ml;

				ml = MemberLookup (ec, type, ".ctor", false,
						   MemberTypes.Constructor, AllBindingsFlags, Location);

				bool is_struct;
				is_struct = type.IsSubclassOf (TypeManager.value_type);

				if (! (ml is MethodGroupExpr)){
					if (!is_struct){
						report118 (Location, ml, "method group");
						return null;
					}
				}

				if (ml != null){
					if (Arguments != null){
						for (int i = Arguments.Count; i > 0;){
							--i;
							Argument a = (Argument) Arguments [i];
							
							if (!a.Resolve (ec))
								return null;
						}
					}
					
					method = Invocation.OverloadResolve (
						ec, (MethodGroupExpr) ml,
						Arguments, Location);
				
					if (method == null && !is_struct) {
						Error (-6, Location,
						       "New invocation: Can not find a constructor for " +
						       "this argument list");
						return null;
					}
				} 
				
				eclass = ExprClass.Value;
				return this;
			}

			if (NewType == NType.Array) {
				throw new Exception ("Finish array creation");
			}

			return null;
		}

		//
		// This DoEmit can be invoked in two contexts:
		//    * As a mechanism that will leave a value on the stack (new object)
		//    * As one that wont (init struct)
		//
		// You can control whether a value is required on the stack by passing
		// need_value_on_stack.  The code *might* leave a value on the stack
		// so it must be popped manually
		//
		// Returns whether a value is left on the stack
		//
		bool DoEmit (EmitContext ec, bool need_value_on_stack)
		{
			if (method == null){
				MemoryLocation ml = (MemoryLocation) value_target;

				ml.AddressOf (ec);
			} else {
				Invocation.EmitArguments (ec, method, Arguments);
				ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) method);
				return true;
			}

			//
			// It must be a value type, sanity check
			//
			if (value_target != null){
				ec.ig.Emit (OpCodes.Initobj, type);

				if (need_value_on_stack){
					value_target.Emit (ec);
					return true;
				}
				return false;
			}

			throw new Exception ("No method and no value type");
		}

		public override void Emit (EmitContext ec)
		{
			DoEmit (ec, true);
		}
		
		public override void EmitStatement (EmitContext ec)
		{
			if (DoEmit (ec, false))
				ec.ig.Emit (OpCodes.Pop);
		}
	}

	//
	// Represents the `this' construct
	//
	public class This : Expression, LValue, MemoryLocation {
		Location loc;
		
		public This (Location loc)
		{
			this.loc = loc;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			eclass = ExprClass.Variable;
			type = ec.TypeContainer.TypeBuilder;

			if (ec.IsStatic){
				Report.Error (26, loc,
					      "Keyword this not valid in static code");
				return null;
			}
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarg_0);
		}

		public void Store (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Starg, 0);
		}

		public void AddressOf (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldarga_S, (byte) 0);
		}

		public Expression LValueResolve (EmitContext ec)
		{
			if (ec.TypeContainer is Class){
				Report.Error (1604, loc, "Cannot assign to `this'");
				return null;
			}

			return this;
		}
	}

	// <summary>
	//   Implements the typeof operator
	// </summary>
	public class TypeOf : Expression {
		public readonly string QueriedType;
		Type typearg;
		
		public TypeOf (string queried_type)
		{
			QueriedType = queried_type;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			typearg = ec.TypeContainer.LookupType (QueriedType, false);

			if (typearg == null)
				return null;

			type = TypeManager.type_type;
			eclass = ExprClass.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldtoken, typearg);
			ec.ig.Emit (OpCodes.Call, TypeManager.system_type_get_type_from_handle);
		}
	}

	public class SizeOf : Expression {
		public readonly string QueriedType;
		
		public SizeOf (string queried_type)
		{
			this.QueriedType = queried_type;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// FIXME: Implement;
			throw new Exception ("Unimplemented");
			// return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
		}
	}

	public class MemberAccess : Expression {
		public readonly string Identifier;
		Expression expr;
		Expression member_lookup;
		Location loc;
		
		public MemberAccess (Expression expr, string id, Location l)
		{
			this.expr = expr;
			Identifier = id;
			loc = l;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		void error176 (Location loc, string name)
		{
			Report.Error (176, loc, "Static member `" +
				      name + "' cannot be accessed " +
				      "with an instance reference, qualify with a " +
				      "type name instead");
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);

			if (expr == null)
				return null;

			if (expr is SimpleName){
				SimpleName child_expr = (SimpleName) expr;
				
				expr = new SimpleName (child_expr.Name + "." + Identifier, loc);

				return expr.Resolve (ec);
			}
					
			member_lookup = MemberLookup (ec, expr.Type, Identifier, false, loc);

			//
			// Method Groups
			//
			if (member_lookup is MethodGroupExpr){
				MethodGroupExpr mg = (MethodGroupExpr) member_lookup;
				
				//
				// Type.MethodGroup
				//
				if (expr is TypeExpr){
					if (!mg.RemoveInstanceMethods ()){
						SimpleName.Error120 (loc, mg.Methods [0].Name); 
						return null;
					}

					return member_lookup;
				}

				//
				// Instance.MethodGroup
				//
				if (!mg.RemoveStaticMethods ()){
					error176 (loc, mg.Methods [0].Name);
					return null;
				}
				
				mg.InstanceExpression = expr;
					
				return member_lookup;
			}

			if (member_lookup is FieldExpr){
				FieldExpr fe = (FieldExpr) member_lookup;

				if (expr is TypeExpr){
					if (!fe.FieldInfo.IsStatic){
						error176 (loc, fe.FieldInfo.Name);
						return null;
					}
					return member_lookup;
				} else {
					if (fe.FieldInfo.IsStatic){
						error176 (loc, fe.FieldInfo.Name);
						return null;
					}
					fe.InstanceExpression = expr;

					return fe;
				}
			}

			if (member_lookup is PropertyExpr){
				PropertyExpr pe = (PropertyExpr) member_lookup;

				if (expr is TypeExpr){
					if (!pe.IsStatic){
						SimpleName.Error120 (loc, pe.PropertyInfo.Name);
						return null;
					}
					return pe;
				} else {
					if (pe.IsStatic){
						error176 (loc, pe.PropertyInfo.Name);
						return null;
					}
					pe.InstanceExpression = expr;

					return pe;
				}
			}
			
			Console.WriteLine ("Support for " + member_lookup + " is not present yet");
			Environment.Exit (0);
			return null;
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Should not happen I think");
		}

	}

	// <summary>
	//   Fully resolved expression that evaluates to a type
	// </summary>
	public class TypeExpr : Expression {
		public TypeExpr (Type t)
		{
			Type = t;
			eclass = ExprClass.Type;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
		}
	}

	// <summary>
	//   MethodGroup Expression.
	//  
	//   This is a fully resolved expression that evaluates to a type
	// </summary>
	public class MethodGroupExpr : Expression {
		public MethodBase [] Methods;
		Expression instance_expression = null;
		
		public MethodGroupExpr (MemberInfo [] mi)
		{
			Methods = new MethodBase [mi.Length];
			mi.CopyTo (Methods, 0);
			eclass = ExprClass.MethodGroup;
		}

		//
		// `A method group may have associated an instance expression' 
		// 
		public Expression InstanceExpression {
			get {
				return instance_expression;
			}

			set {
				instance_expression = value;
			}
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("This should never be reached");
		}

		bool RemoveMethods (bool keep_static)
		{
			ArrayList smethods = new ArrayList ();
			int top = Methods.Length;
			int i;
			
			for (i = 0; i < top; i++){
				MethodBase mb = Methods [i];

				if (mb.IsStatic == keep_static)
					smethods.Add (mb);
			}

			if (smethods.Count == 0)
				return false;

			Methods = new MethodBase [smethods.Count];
			smethods.CopyTo (Methods, 0);

			return true;
		}
		
		// <summary>
		//   Removes any instance methods from the MethodGroup, returns
		//   false if the resulting set is empty.
		// </summary>
		public bool RemoveInstanceMethods ()
		{
			return RemoveMethods (true);
		}

		// <summary>
		//   Removes any static methods from the MethodGroup, returns
		//   false if the resulting set is empty.
		// </summary>
		public bool RemoveStaticMethods ()
		{
			return RemoveMethods (false);
		}
	}

	// <summary>
	//   Fully resolved expression that evaluates to a Field
	// </summary>
	public class FieldExpr : Expression, LValue, MemoryLocation {
		public readonly FieldInfo FieldInfo;
		public Expression InstanceExpression;
			
		public FieldExpr (FieldInfo fi)
		{
			FieldInfo = fi;
			eclass = ExprClass.Variable;
			type = fi.FieldType;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			if (!FieldInfo.IsStatic){
				if (InstanceExpression == null){
					throw new Exception ("non-static FieldExpr without instance var\n" +
							     "You have to assign the Instance variable\n" +
							     "Of the FieldExpr to set this\n");
				}

				InstanceExpression = InstanceExpression.Resolve (ec);
				if (InstanceExpression == null)
					return null;
				
			}
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (FieldInfo.IsStatic)
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			else {
				InstanceExpression.Emit (ec);
				
				ig.Emit (OpCodes.Ldfld, FieldInfo);
			}
		}

		public void Store (EmitContext ec)
		{
			if (FieldInfo.IsStatic)
				ec.ig.Emit (OpCodes.Stsfld, FieldInfo);
			else
				ec.ig.Emit (OpCodes.Stfld, FieldInfo);
		}

		public void AddressOf (EmitContext ec)
		{
			if (FieldInfo.IsStatic)
				ec.ig.Emit (OpCodes.Ldsflda, FieldInfo);
			else {
				InstanceExpression.Emit (ec);
				ec.ig.Emit (OpCodes.Ldflda, FieldInfo);
			}
		}

		public Expression LValueResolve (EmitContext ec)
		{
			if (!FieldInfo.IsInitOnly)
				return this;

			//
			// InitOnly fields can only be assigned in constructors
			//

			if (ec.IsConstructor)
				return this;

			return null;
		}
	}
	
	// <summary>
	//   Expression that evaluates to a Property.  The Assign class
	//   might set the `Value' expression if we are in an assignment. 
	// </summary>
	public class PropertyExpr : ExpressionStatement {
		public readonly PropertyInfo PropertyInfo;
		public readonly bool IsStatic;
		MethodInfo [] Accessors;
		Location loc;
		
		Expression instance_expr;
		Expression value;
		
		public PropertyExpr (PropertyInfo pi, Location l)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
			IsStatic = false;
			loc = l;
			Accessors = TypeManager.GetAccessors (pi);

			if (Accessors != null)
				for (int i = 0; i < Accessors.Length; i++){
					if (Accessors [i] != null)
						if (Accessors [i].IsStatic)
							IsStatic = true;
				}
			else
				Accessors = new MethodInfo [2];
			
			type = pi.PropertyType;
		}

		//
		// Controls the Value of the PropertyExpr.  If the value
		// is null, then the property is being used in a `read' mode.
		// otherwise the property is used in assignment mode.
		//
		// The value is set to a fully resolved type by assign.
		//
		public Expression Value {
			get {
				return value;
			}

			set {
				this.value = value;
			}
		}

		//
		// The instance expression associated with this expression
		//
		public Expression InstanceExpression {
			set {
				instance_expr = value;
			}

			get {
				return instance_expr;
			}
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			if (value == null){
				if (!PropertyInfo.CanRead){
					Report.Error (154, loc, 
						      "The property `" + PropertyInfo.Name +
						      "' can not be used in " +
						      "this context because it lacks a get accessor");
					return null;
				}
			} else {
				if (!PropertyInfo.CanWrite){
					Report.Error (200, loc, 
						      "The property `" + PropertyInfo.Name +
						      "' can not be assigned to, as it has not set accessor");
					return null;
				}
			}
			
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			if (value == null)
				Invocation.EmitCall (ec, IsStatic, instance_expr, Accessors [0], null);
			else {
				Argument arg = new Argument (value, Argument.AType.Expression);
				ArrayList args = new ArrayList ();

				args.Add (arg);
				Invocation.EmitCall (ec, IsStatic, instance_expr, Accessors [1], args);
			}
		}

		override public void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			if (value == null){
				ec.ig.Emit (OpCodes.Pop);
			}
		}
	}

	// <summary>
	//   Fully resolved expression that evaluates to a Expression
	// </summary>
	public class EventExpr : Expression {
		public readonly EventInfo EventInfo;
		Location loc;
		
		public EventExpr (EventInfo ei, Location loc)
		{
			EventInfo = ei;
			this.loc = loc;
			eclass = ExprClass.EventAccess;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			// We are born in resolved state. 
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
			// FIXME: Implement.
		}
	}
	
	public class CheckedExpr : Expression {

		public Expression Expr;

		public CheckedExpr (Expression e)
		{
			Expr = e;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			Expr = Expr.Resolve (ec);

			if (Expr == null)
				return null;

			eclass = Expr.ExprClass;
			type = Expr.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			bool last_check = ec.CheckState;
			
			ec.CheckState = true;
			Expr.Emit (ec);
			ec.CheckState = last_check;
		}
		
	}

	public class UnCheckedExpr : Expression {

		public Expression Expr;

		public UnCheckedExpr (Expression e)
		{
			Expr = e;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			Expr = Expr.Resolve (ec);

			if (Expr == null)
				return null;

			eclass = Expr.ExprClass;
			type = Expr.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			bool last_check = ec.CheckState;
			
			ec.CheckState = false;
			Expr.Emit (ec);
			ec.CheckState = last_check;
		}
		
	}
	
	public class ElementAccess : Expression, LValue {
		
		public ArrayList  Arguments;
		public Expression Expr;

		Location   location;
		
		public ElementAccess (Expression e, ArrayList e_list, Location loc)
		{
			Expr = e;

			Arguments = new ArrayList ();
			foreach (Expression tmp in e_list)
				Arguments.Add (new Argument (tmp, Argument.AType.Expression));
			
			location  = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			Expr = Expr.Resolve (ec);

			//Console.WriteLine (Expr.ToString ());

			if (Expr == null) 
				return null;

			if (Arguments == null)
				return null;

			if (Expr.ExprClass != ExprClass.Variable) {
				report118 (location, Expr, "variable");
				return null;
			}

			if (Arguments != null){
				for (int i = Arguments.Count; i > 0;){
					--i;
					Argument a = (Argument) Arguments [i];
					
					if (!a.Resolve (ec))
						return null;
					
					Type a_type = a.expr.Type;
					if (!(StandardConversionExists (a_type, TypeManager.int32_type) ||
					      StandardConversionExists (a_type, TypeManager.uint32_type) ||
					      StandardConversionExists (a_type, TypeManager.int64_type) ||
					      StandardConversionExists (a_type, TypeManager.uint64_type)))
						return null;
					
				}
			}			

			// FIXME : Implement the actual storage here.
			
			throw new Exception ("Finish element access");
			
		}

		public void Store (EmitContext ec)
		{
			throw new Exception ("Implement me !");
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Implement me !");
		}

		public Expression LValueResolve (EmitContext ec)
		{
			return this;
		}
	}
	
	public class BaseAccess : Expression {

		public enum BaseAccessType {
			Member,
			Indexer
		};
		
		public readonly BaseAccessType BAType;
		public readonly string         Member;
		public readonly ArrayList      Arguments;

		public BaseAccess (BaseAccessType t, string member, ArrayList args)
		{
			BAType = t;
			Member = member;
			Arguments = args;
			
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// FIXME: Implement;
			throw new Exception ("Unimplemented");
			// return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Unimplemented");
		}
	}

	// <summary>
	//   This class exists solely to pass the Type around and to be a dummy
	//   that can be passed to the conversion functions (this is used by
	//   foreach implementation to typecast the object return value from
	//   get_Current into the proper type.  All code has been generated and
	//   we only care about the side effect conversions to be performed
	// </summary>
	
	public class EmptyExpression : Expression {
		public EmptyExpression ()
		{
			type = TypeManager.object_type;
			eclass = ExprClass.Value;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// nothing, as we only exist to not do anything.
		}
	}

	public class UserCast : Expression {
		MethodBase method;
		Expression source;
		
		public UserCast (MethodInfo method, Expression source)
		{
			this.method = method;
			this.source = source;
			type = method.ReturnType;
			eclass = ExprClass.Value;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			//
			// We are born fully resolved
			//
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			source.Emit (ec);
			
			if (method is MethodInfo)
				ig.Emit (OpCodes.Call, (MethodInfo) method);
			else
				ig.Emit (OpCodes.Call, (ConstructorInfo) method);

		}

	}
}
