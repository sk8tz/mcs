//
// literal.cs: Literal representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
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

	//
	// The null Literal constant
	//
	public class NullLiteral : Constant {
		public static readonly NullLiteral Null;

		static NullLiteral ()
		{
			Null = new NullLiteral ();
		}
			
		public NullLiteral ()
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

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.null_type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldnull);
		}

		public override Constant Increment ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue {
			get {
				return true;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override bool IsZeroInteger {
			get { return true; }
		}

		public override string GetSignatureForError()
		{
			return "null";
		}

		public override void Error_ValueCannotBeConverted (Location loc, Type t)
		{
			Report.Error (37, loc, "Cannot convert null to `{0}' because it is a value type",
				TypeManager.CSharpName (t));
		}

		public override Constant ToType (Type type, Location loc)
		{
			if (!type.IsValueType && !TypeManager.IsEnumType (type))
				return NullLiteral.Null;

			return base.ToType (type, loc);
		}

	}

	//
	// A null literal in a pointer context
	//
	public class NullPointer : NullLiteral {
		public new static readonly NullLiteral Null;

		static NullPointer ()
		{
			Null = new NullPointer ();
		}

		private NullPointer ()
		{
			type = TypeManager.object_type;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
				
			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Conv_U);
		}
	}

	public class BoolLiteral : BoolConstant {
		public BoolLiteral (bool val) : base (val)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.bool_type;
			return this;
		}
	}

	public class CharLiteral : CharConstant {
		public CharLiteral (char c) : base (c)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.char_type;
			return this;
		}
	}

	public class IntLiteral : IntConstant {
		public static IntLiteral One, Zero;
		
		static IntLiteral ()
		{
			Zero = new IntLiteral (0);
			One = new IntLiteral (1);
		}
		
		public IntLiteral (int l) : base (l)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.int32_type;
			return this;
		}
	}

	public class UIntLiteral : UIntConstant {
		public UIntLiteral (uint l) : base (l)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.uint32_type;
			return this;
		}
	}
	
	public class LongLiteral : LongConstant {
		public LongLiteral (long l) : base (l)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.int64_type;

			return this;
		}
	}

	public class ULongLiteral : ULongConstant {
		public ULongLiteral (ulong l) : base (l)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.uint64_type;
			return this;
		}
	}
	
	public class FloatLiteral : FloatConstant {
		
		public FloatLiteral (float f) : base (f)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.float_type;
			return this;
		}
	}

	public class DoubleLiteral : DoubleConstant {
		public DoubleLiteral (double d) : base (d)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.double_type;

			return this;
		}
	}

	public class DecimalLiteral : DecimalConstant {
		public DecimalLiteral (decimal d) : base (d)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.decimal_type;
			return this;
		}
	}

	public class StringLiteral : StringConstant {
		public StringLiteral (string s) : base (s)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.string_type;

			return this;
		}
	}
}
