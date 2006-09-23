//
// literal.cs: Literal representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001 Ximian, Inc.
//
//
// Notice that during parsing we create objects of type Literal, but the
// types are not loaded (thats why the Resolve method has to assign the
// type at that point).
//
// Literals differ from the constants in that we know we encountered them
// as a literal in the source code (and some extra rules apply there) and
// they have to be resolved (since during parsing we have not loaded the
// types yet) while constants are created only after types have been loaded
// and are fully resolved when born.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

//
// I put System.Null just so we do not have to special case it on 
// TypeManager.CSharpName
//
namespace System {
	//
	// Represents the Null Type, just used as a placeholder for the type in NullLiteral
	//
	public class Null {
	}
}
	
namespace Mono.CSharp {

	//
	// The NullType just exists to compare type equality, and for
	// expressions that might have the `null type'
	//
	public class NullType {
	}


	public abstract class NullConstant : Constant
	{
		public NullConstant (Location loc):
			base (loc)
		{
			eclass = ExprClass.Value;
		}
		
		override public string AsString ()
		{
			return "null";
		}

		public override object GetValue ()
		{
			return null;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit(OpCodes.Ldnull);
		}

		public override string GetSignatureForError ()
		{
			return "null";
		}

		public override Constant Increment ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue 
		{
			get { return true; }
		}

		public override bool IsNegative 
		{
			get { return false; }
		}

		public override bool IsZeroInteger 
		{
			get { return true; }
		}

		public override Constant Reduce(bool inCheckedContext, Type target_type)
		{
			if (!TypeManager.IsValueType (target_type))
				return new EmptyConstantCast (this, target_type);

			return null;
		}

		public override Constant ToType(Type targetType)
		{
			if (!TypeManager.IsValueType (targetType))
				return new EmptyConstantCast (this, targetType);

			return null;
		}
	}

	//
	// Represents default(X) when result can be reduced to null
	//
	public class NullDefault : EmptyConstantCast
	{
		public NullDefault(Constant value, Type type)
			: base (value, type)
		{
		}

		public override void Error_ValueCannotBeConverted (Location loc, Type target, bool expl)
		{
			base.Error_ValueCannotBeConverted(loc, target, expl);
		}
	}

	//
	// The null Literal constant
	//
	public class NullLiteral : NullConstant {
		public NullLiteral (Location loc):
			base (loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.null_type;
			return this;
		}

		public override void Error_ValueCannotBeConverted (Location loc, Type t, bool expl)
		{
			if (TypeManager.IsGenericParameter (t)) {
				Report.Error(403, loc,
					"Cannot convert null to the type parameter `{0}' becaues it could be a value " +
					"type. Consider using `default ({0})' instead", t.Name);
			} else {
				Report.Error(37, loc, "Cannot convert null to `{0}' because it is a value type",
					TypeManager.CSharpName(t));
			}
		}

		public override Constant ToType (Type targetType)
		{
			if (targetType.IsPointer)
				return new EmptyConstantCast (NullPointer.Null, targetType);

			if (TypeManager.IsGenericParameter(targetType)) {
				GenericConstraints gc = null;

#if GMCS_SOURCE
				gc = TypeManager.GetTypeParameterConstraints(targetType);
#endif
				if (gc != null && gc.IsReferenceType)
					return new EmptyConstantCast (this, targetType);

				Error_ValueCannotBeConverted (loc, targetType, false);
				return null;
			}

			return base.ToType(targetType);
		}

	}

	//
	// A null literal in a pointer context
	//
	public class NullPointer : NullLiteral {
		public static readonly NullLiteral Null;

		static NullPointer ()
		{
			Null = new NullPointer ();
		}

		private NullPointer ():
			base (Location.Null)
		{
			type = TypeManager.object_type;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
				
			// TODO: why not use Ldnull instead ?
			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Conv_U);
		}
	}

	public class BoolLiteral : BoolConstant {
		public BoolLiteral (bool val, Location loc) : base (val, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.bool_type;
			return this;
		}
	}

	public class CharLiteral : CharConstant {
		public CharLiteral (char c, Location loc) : base (c, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.char_type;
			return this;
		}
	}

	public class IntLiteral : IntConstant {
		public IntLiteral (int l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.int32_type;
			return this;
		}
	}

	public class UIntLiteral : UIntConstant {
		public UIntLiteral (uint l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.uint32_type;
			return this;
		}
	}
	
	public class LongLiteral : LongConstant {
		public LongLiteral (long l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.int64_type;

			return this;
		}
	}

	public class ULongLiteral : ULongConstant {
		public ULongLiteral (ulong l, Location loc) : base (l, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.uint64_type;
			return this;
		}
	}
	
	public class FloatLiteral : FloatConstant {
		
		public FloatLiteral (float f, Location loc) : base (f, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.float_type;
			return this;
		}
	}

	public class DoubleLiteral : DoubleConstant {
		public DoubleLiteral (double d, Location loc) : base (d, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.double_type;

			return this;
		}

		public override void Error_ValueCannotBeConverted (Location loc, Type target, bool expl)
		{
			if (target == TypeManager.float_type) {
				Error_664 (loc, "float", "f");
				return;
			}

			if (target == TypeManager.decimal_type) {
				Error_664 (loc, "decimal", "m");
				return;
			}

			base.Error_ValueCannotBeConverted (loc, target, expl);
		}

		static void Error_664 (Location loc, string type, string suffix)
		{
			Report.Error (664, loc,
				"Literal of type double cannot be implicitly converted to type `{0}'. Add suffix `{1}' to create a literal of this type",
				type, suffix);
		}
	}

	public class DecimalLiteral : DecimalConstant {
		public DecimalLiteral (decimal d, Location loc) : base (d, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.decimal_type;
			return this;
		}
	}

	public class StringLiteral : StringConstant {
		public StringLiteral (string s, Location loc) : base (s, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.string_type;

			return this;
		}
	}
}
