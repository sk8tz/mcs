//
// literal.cs: Literal representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	/// <summary>
	///   Base class for literals
	/// </summary>
	public abstract class Literal : Constant {
		static public string descape (char c)
		{
			switch (c){
			case '\a':
				return "\\a"; 
			case '\b':
				return "\\b"; 
			case '\n':
				return "\\n"; 
			case '\t':
				return "\\t"; 
			case '\v':
				return "\\v"; 
			case '\r':
				return "\\r"; 
			case '\\':
				return "\\\\";
			case '\f':
				return "\\f"; 
			case '\0':
				return "\\0"; 
			case '"':
				return "\\\""; 
			case '\'':
				return "\\\'"; 
			}
			return c.ToString ();
		}

		protected Literal ()
		{
		}
	}

	public class NullLiteral : Literal {
		public static readonly NullLiteral Null;
		
		static NullLiteral ()
		{
			Null = new NullLiteral ();
		}
			
		public NullLiteral ()
		{
			if (Null != null)
				throw new Exception ("More than one null has been created!");
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
			type = TypeManager.object_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldnull);
		}
	}

	public class BoolLiteral : Literal {
		public readonly bool Value;
		
		public BoolLiteral (bool val)
		{
			Value = val;
		}

		override public string AsString ()
		{
			return Value ? "true" : "false";
		}

		public override object GetValue ()
		{
			return (object) Value;
		}
				
		
		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.bool_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			if (Value)
				ec.ig.Emit (OpCodes.Ldc_I4_1);
			else
				ec.ig.Emit (OpCodes.Ldc_I4_0);
		}
	}

	public class CharLiteral : Literal {
		char c;
		
		public CharLiteral (char c)
		{
			this.c = c;
		}

		override public string AsString ()
		{
			return "\"" + descape (c) + "\"";
		}

		public override object GetValue ()
		{
			return (object) c;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.char_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			IntLiteral.EmitInt (ec.ig, c);
		}
	}

	public class IntLiteral : Literal {
		public readonly int Value;

		public IntLiteral (int l)
		{
			Value = l;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			if (Value <= System.Int32.MaxValue &&
			    Value >= System.Int32.MinValue)
				return (object) Value;
			else
				return null;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.int32_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			EmitInt (ig, Value);
		}

		static public void EmitInt (ILGenerator ig, int i)
		{
			switch (i){
			case -1:
				ig.Emit (OpCodes.Ldc_I4_M1);
				break;
				
			case 0:
				ig.Emit (OpCodes.Ldc_I4_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Ldc_I4_1);
				break;
				
			case 2:
				ig.Emit (OpCodes.Ldc_I4_2);
				break;
				
			case 3:
				ig.Emit (OpCodes.Ldc_I4_3);
				break;
				
			case 4:
				ig.Emit (OpCodes.Ldc_I4_4);
				break;
				
			case 5:
				ig.Emit (OpCodes.Ldc_I4_5);
				break;
				
			case 6:
				ig.Emit (OpCodes.Ldc_I4_6);
				break;
				
			case 7:
				ig.Emit (OpCodes.Ldc_I4_7);
				break;
				
			case 8:
				ig.Emit (OpCodes.Ldc_I4_8);
				break;

			default:
				if (i > 0 && i < 127){
					ig.Emit (OpCodes.Ldc_I4_S, (sbyte) i);
				} else
					ig.Emit (OpCodes.Ldc_I4, i);
				break;
			}
		}
	}

	public class UIntLiteral : Literal {
		public readonly uint Value;

		public UIntLiteral (uint l)
		{
			Value = l;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			if (Value <= System.UInt32.MaxValue &&
			    Value >= System.UInt32.MinValue)
				return (object) Value;
			else
				return null;

		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.uint32_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			IntLiteral.EmitInt (ig, unchecked ((int) Value));
		}

	}
	
	public class LongLiteral : Literal {
		public readonly long Value;

		public LongLiteral (long l)
		{
			Value = l;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			if (Value <= System.Int64.MaxValue &&
			    Value >= System.Int64.MinValue)
				return (object) Value;
			else
				return null;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.int64_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			EmitLong (ig, Value);
		}

		static public void EmitLong (ILGenerator ig, long l)
		{
			ig.Emit (OpCodes.Ldc_I8, l);
		}
	}

	public class ULongLiteral : Literal {
		public readonly ulong Value;

		public ULongLiteral (ulong l)
		{
			Value = l;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			if (Value <= System.UInt64.MaxValue &&
			    Value >= System.UInt64.MinValue)
				return (object) Value;
			else
				return null;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.uint64_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			LongLiteral.EmitLong (ig, unchecked ((long) Value));
		}
	}
	
	public class FloatLiteral : Literal {
		public readonly float Value;

		public FloatLiteral (float f)
		{
			Value = f;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return (object) Value;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.float_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldc_R4, Value);
		}
	}

	public class DoubleLiteral : Literal {
		public readonly double Value;

		public DoubleLiteral (double d)
		{
			Value = d;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return (object) Value;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.double_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldc_R8, Value);
		}
	}

	public class DecimalLiteral : Literal {
		public readonly decimal Value;

		public DecimalLiteral (decimal d)
		{
			Value = d;
		}

		override public string AsString ()
		{
			return Value.ToString ();
		}

		public override object GetValue ()
		{
			return (object) Value;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.decimal_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
		}
	}

	public class StringLiteral : Literal {
		public readonly string Value;

		public StringLiteral (string s)
		{
			Value = s;
		}

		// FIXME: Escape the string.
		override public string AsString ()
		{
			return "\"" + Value + "\"";
		}

		public override object GetValue ()
		{
			return (object) Value;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			type = TypeManager.string_type;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldstr, Value);
		}
	}
}
