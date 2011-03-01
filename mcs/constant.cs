//
// constant.cs: Constants.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2001-2003 Ximian, Inc.
// Copyright 2003-2008 Novell, Inc.
//

using System;
using System.Globalization;

#if STATIC
using IKVM.Reflection.Emit;
#else
using System.Reflection.Emit;
#endif

namespace Mono.CSharp {

	/// <summary>
	///   Base class for constants and literals.
	/// </summary>
	public abstract class Constant : Expression
	{
		static readonly NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;

		protected Constant (Location loc)
		{
			this.loc = loc;
		}

		override public string ToString ()
		{
			return this.GetType ().Name + " (" + GetValueAsLiteral () + ")";
		}

		/// <summary>
		///  This is used to obtain the actual value of the literal
		///  cast into an object.
		/// </summary>
		public abstract object GetValue ();

		public abstract long GetValueAsLong ();

		public abstract string GetValueAsLiteral ();

#if !STATIC
		//
		// Returns an object value which is typed to contant type
		//
		public virtual object GetTypedValue ()
		{
			return GetValue ();
		}
#endif

		public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, TypeSpec target, bool expl)
		{
			if (!expl && IsLiteral && 
				BuildinTypeSpec.IsPrimitiveNumericOrDecimalType (target) &&
				BuildinTypeSpec.IsPrimitiveNumericOrDecimalType (type)) {
				ec.Report.Error (31, loc, "Constant value `{0}' cannot be converted to a `{1}'",
					GetValueAsLiteral (), TypeManager.CSharpName (target));
			} else {
				base.Error_ValueCannotBeConverted (ec, loc, target, expl);
			}
		}

		public Constant ImplicitConversionRequired (ResolveContext ec, TypeSpec type, Location loc)
		{
			Constant c = ConvertImplicitly (ec, type);
			if (c == null)
				Error_ValueCannotBeConverted (ec, loc, type, false);

			return c;
		}

		public virtual Constant ConvertImplicitly (ResolveContext rc, TypeSpec type)
		{
			if (this.type == type)
				return this;

			if (Convert.ImplicitNumericConversion (this, type) == null) 
				return null;

			bool fail;			
			object constant_value = ChangeType (GetValue (), type, out fail);
			if (fail){
				//
				// We should always catch the error before this is ever
				// reached, by calling Convert.ImplicitStandardConversionExists
				//
				throw new InternalErrorException ("Missing constant conversion between `{0}' and `{1}'",
				  TypeManager.CSharpName (Type), TypeManager.CSharpName (type));
			}

			return CreateConstant (rc, type, constant_value, loc);
		}

		//
		//  Returns a constant instance based on Type
		//
		public static Constant CreateConstant (ResolveContext rc, TypeSpec t, object v, Location loc)
		{
			return CreateConstantFromValue (t, v, loc).Resolve (rc);
		}

		public static Constant CreateConstantFromValue (TypeSpec t, object v, Location loc)
		{
			if (t.BuildinType > 0) {
				switch (t.BuildinType) {
				case BuildinTypeSpec.Type.Int:
					return new IntConstant ((int) v, loc);
				case BuildinTypeSpec.Type.String:
					return new StringConstant ((string) v, loc);
				case BuildinTypeSpec.Type.UInt:
					return new UIntConstant ((uint) v, loc);
				case BuildinTypeSpec.Type.Long:
					return new LongConstant ((long) v, loc);
				case BuildinTypeSpec.Type.ULong:
					return new ULongConstant ((ulong) v, loc);
				case BuildinTypeSpec.Type.Float:
					return new FloatConstant ((float) v, loc);
				case BuildinTypeSpec.Type.Double:
					return new DoubleConstant ((double) v, loc);
				case BuildinTypeSpec.Type.Short:
					return new ShortConstant ((short) v, loc);
				case BuildinTypeSpec.Type.UShort:
					return new UShortConstant ((ushort) v, loc);
				case BuildinTypeSpec.Type.SByte:
					return new SByteConstant ((sbyte) v, loc);
				case BuildinTypeSpec.Type.Byte:
					return new ByteConstant ((byte) v, loc);
				case BuildinTypeSpec.Type.Char:
					return new CharConstant ((char) v, loc);
				case BuildinTypeSpec.Type.Bool:
					return new BoolConstant ((bool) v, loc);
				case BuildinTypeSpec.Type.Decimal:
					return new DecimalConstant ((decimal) v, loc);
				}
			}

			if (t.IsEnum) {
				var real_type = EnumSpec.GetUnderlyingType (t);
				return new EnumConstant (CreateConstantFromValue (real_type, v, loc).Resolve (null), t);
			}

			if (v == null) {
				if (TypeManager.IsNullableType (t))
					return Nullable.LiftedNull.Create (t, loc);

				if (TypeManager.IsReferenceType (t))
					return new NullConstant (t, loc);
			}

			throw new InternalErrorException ("Constant value `{0}' has unexpected underlying type `{1}'",
				v, TypeManager.CSharpName (t));
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			Arguments args = new Arguments (2);
			args.Add (new Argument (this));
			args.Add (new Argument (new TypeOf (new TypeExpression (type, loc), loc)));

			return CreateExpressionFactoryCall (ec, "Constant", args);
		}

		/// <summary>
		/// Maybe ConvertTo name is better. It tries to convert `this' constant to target_type.
		/// It throws OverflowException 
		/// </summary>
		// DON'T CALL THIS METHOD DIRECTLY AS IT DOES NOT HANDLE ENUMS
		public abstract Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type);

		// This is a custom version of Convert.ChangeType() which works
		// with the TypeBuilder defined types when compiling corlib.
		static object ChangeType (object value, TypeSpec targetType, out bool error)
		{
			IConvertible convert_value = value as IConvertible;

			if (convert_value == null) {
				error = true;
				return null;
			}

			//
			// We cannot rely on build-in type conversions as they are
			// more limited than what C# supports.
			// See char -> float/decimal/double conversion
			//
			error = false;
			try {
				switch (targetType.BuildinType) {
				case BuildinTypeSpec.Type.Bool:
					return convert_value.ToBoolean (nfi);
				case BuildinTypeSpec.Type.Byte:
					return convert_value.ToByte (nfi);
				case BuildinTypeSpec.Type.Char:
					return convert_value.ToChar (nfi);
				case BuildinTypeSpec.Type.Short:
					return convert_value.ToInt16 (nfi);
				case BuildinTypeSpec.Type.Int:
					return convert_value.ToInt32 (nfi);
				case BuildinTypeSpec.Type.Long:
					return convert_value.ToInt64 (nfi);
				case BuildinTypeSpec.Type.SByte:
					return convert_value.ToSByte (nfi);
				case BuildinTypeSpec.Type.Decimal:
					if (convert_value.GetType () == typeof (char))
						return (decimal) convert_value.ToInt32 (nfi);
					return convert_value.ToDecimal (nfi);
				case BuildinTypeSpec.Type.Double:
					if (convert_value.GetType () == typeof (char))
						return (double) convert_value.ToInt32 (nfi);
					return convert_value.ToDouble (nfi);
				case BuildinTypeSpec.Type.Float:
					if (convert_value.GetType () == typeof (char))
						return (float) convert_value.ToInt32 (nfi);
					return convert_value.ToSingle (nfi);
				case BuildinTypeSpec.Type.String:
					return convert_value.ToString (nfi);
				case BuildinTypeSpec.Type.UShort:
					return convert_value.ToUInt16 (nfi);
				case BuildinTypeSpec.Type.UInt:
					return convert_value.ToUInt32 (nfi);
				case BuildinTypeSpec.Type.ULong:
					return convert_value.ToUInt64 (nfi);
				case BuildinTypeSpec.Type.Object:
					return value;
				}
			} catch {
			}

			error = true;
			return null;
		}

		/// <summary>
		///   Attempts to do a compile-time folding of a constant cast.
		/// </summary>
		public Constant TryReduce (ResolveContext ec, TypeSpec target_type, Location loc)
		{
			try {
				return TryReduce (ec, target_type);
			}
			catch (OverflowException) {
				if (ec.ConstantCheckState && Type.BuildinType != BuildinTypeSpec.Type.Decimal) {
					ec.Report.Error (221, loc,
						"Constant value `{0}' cannot be converted to a `{1}' (use `unchecked' syntax to override)",
						GetValueAsLiteral (), target_type.GetSignatureForError ());
				} else {
					Error_ValueCannotBeConverted (ec, loc, target_type, false);
				}

				return New.Constantify (target_type, loc).Resolve (ec);
			}
		}

		Constant TryReduce (ResolveContext ec, TypeSpec target_type)
		{
			if (Type == target_type)
				return this;

			Constant c;
			if (TypeManager.IsEnumType (target_type)) {
				c = TryReduce (ec, EnumSpec.GetUnderlyingType (target_type));
				if (c == null)
					return null;

				return new EnumConstant (c, target_type).Resolve (ec);
			}

			c = ConvertExplicitly (ec.ConstantCheckState, target_type);
			if (c != null)
				c = c.Resolve (ec);

			return c;
		}

		/// <summary>
		/// Need to pass type as the constant can require a boxing
		/// and in such case no optimization is possible
		/// </summary>
		public bool IsDefaultInitializer (TypeSpec type)
		{
			if (type == Type)
				return IsDefaultValue;

			return this is NullLiteral;
		}

		public abstract bool IsDefaultValue {
			get;
		}

		public abstract bool IsNegative {
			get;
		}

		//
		// When constant is declared as literal
		//
		public virtual bool IsLiteral {
			get { return false; }
		}
		
		public virtual bool IsOneInteger {
			get { return false; }
		}		

		//
		// Returns true iff 1) the stack type of this is one of Object, 
		// int32, int64 and 2) this == 0 or this == null.
		//
		public virtual bool IsZeroInteger {
			get { return false; }
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			// do nothing
		}

		public sealed override Expression Clone (CloneContext clonectx)
		{
			// No cloning is not needed for constants
			return this;
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			throw new NotSupportedException ("should not be reached");
		}

		public override System.Linq.Expressions.Expression MakeExpression (BuilderContext ctx)
		{
#if STATIC
			return base.MakeExpression (ctx);
#else
			return System.Linq.Expressions.Expression.Constant (GetTypedValue (), type.GetMetaInfo ());
#endif
		}

		public new Constant Resolve (ResolveContext rc)
		{
			if (eclass != ExprClass.Unresolved)
				return this;

			// Resolved constant has to be still a constant
			Constant c = (Constant) DoResolve (rc);
			if (c == null)
				return null;

			if ((c.eclass & ExprClass.Value) == 0) {
				c.Error_UnexpectedKind (rc, ResolveFlags.VariableOrValue, loc);
				return null;
			}

			if (c.type == null)
				throw new InternalErrorException ("Expression `{0}' did not set its type after Resolve", c.GetType ());

			return c;
		}
	}

	public abstract class IntegralConstant : Constant
	{
		protected IntegralConstant (Location loc) :
			base (loc)
		{
		}

		public override void Error_ValueCannotBeConverted (ResolveContext ec, Location loc, TypeSpec target, bool expl)
		{
			try {
				ConvertExplicitly (true, target);
				base.Error_ValueCannotBeConverted (ec, loc, target, expl);
			}
			catch
			{
				ec.Report.Error (31, loc, "Constant value `{0}' cannot be converted to a `{1}'",
					GetValue ().ToString (), TypeManager.CSharpName (target));
			}
		}

		public override string GetValueAsLiteral ()
		{
			return GetValue ().ToString ();
		}
		
		public abstract Constant Increment ();
	}
	
	public class BoolConstant : Constant {
		public readonly bool Value;
		
		public BoolConstant (bool val, Location loc):
			base (loc)
		{
			Value = val;
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.bool_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override object GetValue ()
		{
			return (object) Value;
		}

		public override string GetValueAsLiteral ()
		{
			return Value ? "true" : "false";
		}

		public override long GetValueAsLong ()
		{
			return Value ? 1 : 0;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}
		
		public override void Emit (EmitContext ec)
		{
			if (Value)
				ec.Emit (OpCodes.Ldc_I4_1);
			else
				ec.Emit (OpCodes.Ldc_I4_0);
		}

		public override bool IsDefaultValue {
			get {
				return !Value;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}
	
		public override bool IsZeroInteger {
			get { return Value == false; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			return null;
		}

	}

	public class ByteConstant : IntegralConstant
	{
		public readonly byte Value;

		public ByteConstant (byte v, Location loc):
			base (loc)
		{
			Value = v;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitInt (Value);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			type = TypeManager.byte_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new ByteConstant (checked ((byte)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context){
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class CharConstant : Constant {
		public readonly char Value;

		public CharConstant (char v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.char_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode ((ushort) Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitInt (Value);
		}

		static string descape (char c)
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

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}

		public override string GetValueAsLiteral ()
		{
			return "\"" + descape (Value) + "\"";
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}

		public override bool IsZeroInteger {
			get { return Value == '\0'; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);

			case BuildinTypeSpec.Type.Short:
				if (in_checked_context) {
					if (Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.Int:
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class SByteConstant : IntegralConstant {
		public readonly sbyte Value;

		public SByteConstant (sbyte v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.sbyte_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitInt (Value);
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
		    return new SByteConstant (checked((sbyte)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}
		
		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		
		
		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class ShortConstant : IntegralConstant {
		public readonly short Value;

		public ShortConstant (short v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.short_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitInt (Value);
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new ShortConstant (checked((short)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}
		
		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}
		
		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value < SByte.MinValue || Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();

				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context) {
					if (Value < Char.MinValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class UShortConstant : IntegralConstant
	{
		public readonly ushort Value;

		public UShortConstant (ushort v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.ushort_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitInt (Value);
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}
	
		public override Constant Increment ()
		{
			return new UShortConstant (checked((ushort)(Value + 1)), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}
		
		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		
	
		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				if (in_checked_context) {
					if (Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.Int:
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context) {
					if (Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}
	}

	public class IntConstant : IntegralConstant {
		public readonly int Value;

		public IntConstant (int v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.int32_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitInt (Value);
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new IntConstant (checked(Value + 1), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}
		
		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}
		
		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value < SByte.MinValue || Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				if (in_checked_context) {
					if (Value < Int16.MinValue || Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context) {
					if (Value < UInt16.MinValue || Value > UInt16.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				if (in_checked_context) {
					if (Value < UInt32.MinValue)
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context) {
					if (Value < Char.MinValue || Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

		public override Constant ConvertImplicitly (ResolveContext rc, TypeSpec type)
		{
			if (this.type == type)
				return this;

			Constant c = TryImplicitIntConversion (type);
			if (c != null)
				return c.Resolve (rc);

			return base.ConvertImplicitly (rc, type);
		}

		/// <summary>
		///   Attempts to perform an implicit constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		Constant TryImplicitIntConversion (TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.SByte:
				if (Value >= SByte.MinValue && Value <= SByte.MaxValue)
					return new SByteConstant ((sbyte) Value, loc);
				break;
			case BuildinTypeSpec.Type.Byte:
				if (Value >= Byte.MinValue && Value <= Byte.MaxValue)
					return new ByteConstant ((byte) Value, loc);
				break;
			case BuildinTypeSpec.Type.Short:
				if (Value >= Int16.MinValue && Value <= Int16.MaxValue)
					return new ShortConstant ((short) Value, loc);
				break;
			case BuildinTypeSpec.Type.UShort:
				if (Value >= UInt16.MinValue && Value <= UInt16.MaxValue)
					return new UShortConstant ((ushort) Value, loc);
				break;
			case BuildinTypeSpec.Type.UInt:
				if (Value >= 0)
					return new UIntConstant ((uint) Value, loc);
				break;
			case BuildinTypeSpec.Type.ULong:
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (Value >= 0)
					return new ULongConstant ((ulong) Value, loc);
				break;
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, loc);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, loc);
			}

			return null;
		}
	}

	public class UIntConstant : IntegralConstant {
		public readonly uint Value;

		public UIntConstant (uint v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.uint32_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitInt (unchecked ((int) Value));
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new UIntConstant (checked(Value + 1), loc);
		}
	
		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}
		
		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value < 0 || Value > byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				if (in_checked_context) {
					if (Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context) {
					if (Value < UInt16.MinValue || Value > UInt16.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				if (in_checked_context) {
					if (Value > Int32.MaxValue)
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context) {
					if (Value < Char.MinValue || Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class LongConstant : IntegralConstant {
		public readonly long Value;

		public LongConstant (long v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.int64_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitLong (Value);
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return Value;
		}

		public override Constant Increment ()
		{
			return new LongConstant (checked(Value + 1), loc);
		}
		
		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}
		
		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value < Byte.MinValue || Value > Byte.MaxValue)
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value < SByte.MinValue || Value > SByte.MaxValue)
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				if (in_checked_context) {
					if (Value < Int16.MinValue || Value > Int16.MaxValue)
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context) {
					if (Value < UInt16.MinValue || Value > UInt16.MaxValue)
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				if (in_checked_context) {
					if (Value < Int32.MinValue || Value > Int32.MaxValue)
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				if (in_checked_context) {
					if (Value < UInt32.MinValue || Value > UInt32.MaxValue)
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				if (in_checked_context && Value < 0)
					throw new OverflowException ();
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context) {
					if (Value < Char.MinValue || Value > Char.MaxValue)
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

		public override Constant ConvertImplicitly (ResolveContext rc, TypeSpec type)
		{
			if (Value >= 0 && type == TypeManager.uint64_type) {
				return new ULongConstant ((ulong) Value, loc).Resolve (rc);
			}

			return base.ConvertImplicitly (rc, type);
		}
	}

	public class ULongConstant : IntegralConstant {
		public readonly ulong Value;

		public ULongConstant (ulong v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.uint64_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.EmitLong (unchecked ((long) Value));
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override long GetValueAsLong ()
		{
			return (long) Value;
		}

		public override Constant Increment ()
		{
			return new ULongConstant (checked(Value + 1), loc);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return false;
			}
		}
		
		public override bool IsOneInteger {
			get {
				return Value == 1;
			}
		}		

		public override bool IsZeroInteger {
			get { return Value == 0; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context && Value > Byte.MaxValue)
					throw new OverflowException ();
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context && Value > ((ulong) SByte.MaxValue))
					throw new OverflowException ();
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				if (in_checked_context && Value > ((ulong) Int16.MaxValue))
					throw new OverflowException ();
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context && Value > UInt16.MaxValue)
					throw new OverflowException ();
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				if (in_checked_context && Value > UInt32.MaxValue)
					throw new OverflowException ();
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				if (in_checked_context && Value > UInt32.MaxValue)
					throw new OverflowException ();
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				if (in_checked_context && Value > Int64.MaxValue)
					throw new OverflowException ();
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context && Value > Char.MaxValue)
					throw new OverflowException ();
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class FloatConstant : Constant {
		public float Value;

		public FloatConstant (float v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.float_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.Emit (OpCodes.Ldc_R4, Value);
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override string GetValueAsLiteral ()
		{
			return Value.ToString ();
		}

		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value < byte.MinValue || Value > byte.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value < sbyte.MinValue || Value > sbyte.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				if (in_checked_context) {
					if (Value < short.MinValue || Value > short.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context) {
					if (Value < ushort.MinValue || Value > ushort.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				if (in_checked_context) {
					if (Value < int.MinValue || Value > int.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				if (in_checked_context) {
					if (Value < uint.MinValue || Value > uint.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				if (in_checked_context) {
					if (Value < long.MinValue || Value > long.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				if (in_checked_context) {
					if (Value < ulong.MinValue || Value > ulong.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context) {
					if (Value < (float) char.MinValue || Value > (float) char.MaxValue || float.IsNaN (Value))
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class DoubleConstant : Constant {
		public double Value;

		public DoubleConstant (double v, Location loc):
			base (loc)
		{
			Value = v;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.double_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			enc.Encode (Value);
		}

		public override void Emit (EmitContext ec)
		{
			ec.Emit (OpCodes.Ldc_R8, Value);
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override string GetValueAsLiteral ()
		{
			return Value.ToString ();
		}

		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.Byte:
				if (in_checked_context) {
					if (Value < Byte.MinValue || Value > Byte.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new ByteConstant ((byte) Value, Location);
			case BuildinTypeSpec.Type.SByte:
				if (in_checked_context) {
					if (Value < SByte.MinValue || Value > SByte.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new SByteConstant ((sbyte) Value, Location);
			case BuildinTypeSpec.Type.Short:
				if (in_checked_context) {
					if (Value < short.MinValue || Value > short.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new ShortConstant ((short) Value, Location);
			case BuildinTypeSpec.Type.UShort:
				if (in_checked_context) {
					if (Value < ushort.MinValue || Value > ushort.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new UShortConstant ((ushort) Value, Location);
			case BuildinTypeSpec.Type.Int:
				if (in_checked_context) {
					if (Value < int.MinValue || Value > int.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new IntConstant ((int) Value, Location);
			case BuildinTypeSpec.Type.UInt:
				if (in_checked_context) {
					if (Value < uint.MinValue || Value > uint.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new UIntConstant ((uint) Value, Location);
			case BuildinTypeSpec.Type.Long:
				if (in_checked_context) {
					if (Value < long.MinValue || Value > long.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new LongConstant ((long) Value, Location);
			case BuildinTypeSpec.Type.ULong:
				if (in_checked_context) {
					if (Value < ulong.MinValue || Value > ulong.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new ULongConstant ((ulong) Value, Location);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, Location);
			case BuildinTypeSpec.Type.Char:
				if (in_checked_context) {
					if (Value < (double) char.MinValue || Value > (double) char.MaxValue || double.IsNaN (Value))
						throw new OverflowException ();
				}
				return new CharConstant ((char) Value, Location);
			case BuildinTypeSpec.Type.Decimal:
				return new DecimalConstant ((decimal) Value, Location);
			}

			return null;
		}

	}

	public class DecimalConstant : Constant {
		public readonly decimal Value;

		public DecimalConstant (decimal d, Location loc):
			base (loc)
		{
			Value = d;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.decimal_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			int [] words = decimal.GetBits (Value);
			int power = (words [3] >> 16) & 0xff;

			if (power == 0) {
				if (Value <= int.MaxValue && Value >= int.MinValue) {
					if (TypeManager.void_decimal_ctor_int_arg == null) {
						TypeManager.void_decimal_ctor_int_arg = TypeManager.GetPredefinedConstructor (
							TypeManager.decimal_type, loc, TypeManager.int32_type);

						if (TypeManager.void_decimal_ctor_int_arg == null)
							return;
					}

					ec.EmitInt ((int) Value);
					ec.Emit (OpCodes.Newobj, TypeManager.void_decimal_ctor_int_arg);
					return;
				}

				if (Value <= long.MaxValue && Value >= long.MinValue) {
					if (TypeManager.void_decimal_ctor_long_arg == null) {
						TypeManager.void_decimal_ctor_long_arg = TypeManager.GetPredefinedConstructor (
							TypeManager.decimal_type, loc, TypeManager.int64_type);

						if (TypeManager.void_decimal_ctor_long_arg == null)
							return;
					}

					ec.EmitLong ((long) Value);
					ec.Emit (OpCodes.Newobj, TypeManager.void_decimal_ctor_long_arg);
					return;
				}
			}

			ec.EmitInt (words [0]);
			ec.EmitInt (words [1]);
			ec.EmitInt (words [2]);

			// sign
			ec.EmitInt (words [3] >> 31);

			// power
			ec.EmitInt (power);

			if (TypeManager.void_decimal_ctor_five_args == null) {
				TypeManager.void_decimal_ctor_five_args = TypeManager.GetPredefinedConstructor (
					TypeManager.decimal_type, loc, TypeManager.int32_type, TypeManager.int32_type,
					TypeManager.int32_type, TypeManager.bool_type, TypeManager.byte_type);

				if (TypeManager.void_decimal_ctor_five_args == null)
					return;
			}

			ec.Emit (OpCodes.Newobj, TypeManager.void_decimal_ctor_five_args);
		}

		public override bool IsDefaultValue {
			get {
				return Value == 0;
			}
		}

		public override bool IsNegative {
			get {
				return Value < 0;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			switch (target_type.BuildinType) {
			case BuildinTypeSpec.Type.SByte:
				return new SByteConstant ((sbyte) Value, loc);
			case BuildinTypeSpec.Type.Byte:
				return new ByteConstant ((byte) Value, loc);
			case BuildinTypeSpec.Type.Short:
				return new ShortConstant ((short) Value, loc);
			case BuildinTypeSpec.Type.UShort:
				return new UShortConstant ((ushort) Value, loc);
			case BuildinTypeSpec.Type.Int:
				return new IntConstant ((int) Value, loc);
			case BuildinTypeSpec.Type.UInt:
				return new UIntConstant ((uint) Value, loc);
			case BuildinTypeSpec.Type.Long:
				return new LongConstant ((long) Value, loc);
			case BuildinTypeSpec.Type.ULong:
				return new ULongConstant ((ulong) Value, loc);
			case BuildinTypeSpec.Type.Char:
				return new CharConstant ((char) Value, loc);
			case BuildinTypeSpec.Type.Float:
				return new FloatConstant ((float) Value, loc);
			case BuildinTypeSpec.Type.Double:
				return new DoubleConstant ((double) Value, loc);
			}

			return null;
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override string GetValueAsLiteral ()
		{
			return Value.ToString () + "M";
		}

		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}
	}

	public class StringConstant : Constant {
		public readonly string Value;

		public StringConstant (string s, Location loc):
			base (loc)
		{
			Value = s;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = TypeManager.string_type;
			eclass = ExprClass.Value;
			return this;
		}

		public override object GetValue ()
		{
			return Value;
		}

		public override string GetValueAsLiteral ()
		{
			// FIXME: Escape the string.
			return "\"" + Value + "\"";
		}

		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}
		
		public override void Emit (EmitContext ec)
		{
			if (Value == null) {
				ec.Emit (OpCodes.Ldnull);
				return;
			}

			//
			// Use string.Empty for both literals and constants even if
			// it's not allowed at language level
			//
			if (Value.Length == 0 && ec.Module.Compiler.Settings.Optimize && ec.CurrentType != TypeManager.string_type) {
				if (TypeManager.string_empty == null)
					TypeManager.string_empty = TypeManager.GetPredefinedField (TypeManager.string_type, "Empty", loc, TypeManager.string_type);

				if (TypeManager.string_empty != null) {
					ec.Emit (OpCodes.Ldsfld, TypeManager.string_empty);
					return;
				}
			}

			ec.Emit (OpCodes.Ldstr, Value);
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			// cast to object
			if (type != targetType)
				enc.Encode (type);

			enc.Encode (Value);
		}

		public override bool IsDefaultValue {
			get {
				return Value == null;
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
	}

	//
	// Null constant can have its own type, think of `default (Foo)'
	//
	public class NullConstant : Constant
	{
		public NullConstant (TypeSpec type, Location loc)
			: base (loc)
		{
			eclass = ExprClass.Value;
			this.type = type;
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			if (type == InternalType.Null || type == TypeManager.object_type) {
				// Optimized version, also avoids referencing literal internal type
				Arguments args = new Arguments (1);
				args.Add (new Argument (this));
				return CreateExpressionFactoryCall (ec, "Constant", args);
			}

			return base.CreateExpressionTree (ec);
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			return this;
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			// Type it as string cast
			if (targetType == TypeManager.object_type || targetType == InternalType.Null)
				enc.Encode (TypeManager.string_type);

			var ac = targetType as ArrayContainer;
			if (ac != null) {
				if (ac.Rank != 1 || ac.Element.IsArray)
					base.EncodeAttributeValue (rc, enc, targetType);
				else
					enc.Encode (uint.MaxValue);
			} else {
				enc.Encode (byte.MaxValue);
			}
		}

		public override void Emit (EmitContext ec)
		{
			ec.Emit (OpCodes.Ldnull);

			// Only to make verifier happy
			if (TypeManager.IsGenericParameter (type))
				ec.Emit (OpCodes.Unbox_Any, type);
		}

		public override string ExprClassName {
			get {
				return GetSignatureForError ();
			}
		}

		public override Constant ConvertExplicitly (bool inCheckedContext, TypeSpec targetType)
		{
			if (targetType.IsPointer) {
				if (IsLiteral || this is NullPointer)
					return new EmptyConstantCast (new NullPointer (loc), targetType);

				return null;
			}

			// Exlude internal compiler types
			if (targetType.Kind == MemberKind.InternalCompilerType && targetType != InternalType.Dynamic && targetType != InternalType.Null)
				return null;

			if (!IsLiteral && !Convert.ImplicitStandardConversionExists (this, targetType))
				return null;

			if (TypeManager.IsReferenceType (targetType))
				return new NullConstant (targetType, loc);

			if (TypeManager.IsNullableType (targetType))
				return Nullable.LiftedNull.Create (targetType, loc);

			return null;
		}

		public override Constant ConvertImplicitly (ResolveContext rc, TypeSpec targetType)
		{
			return ConvertExplicitly (false, targetType);
		}

		public override string GetSignatureForError ()
		{
			return "null";
		}

		public override object GetValue ()
		{
			return null;
		}

		public override string GetValueAsLiteral ()
		{
			return GetSignatureForError ();
		}

		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}

		public override bool IsDefaultValue {
			get { return true; }
		}

		public override bool IsNegative {
			get { return false; }
		}

		public override bool IsNull {
			get { return true; }
		}

		public override bool IsZeroInteger {
			get { return true; }
		}
	}

	/// <summary>
	///   The value is constant, but when emitted has a side effect.  This is
	///   used by BitwiseAnd to ensure that the second expression is invoked
	///   regardless of the value of the left side.  
	/// </summary>
	public class SideEffectConstant : Constant {
		public Constant value;
		Expression side_effect;
		
		public SideEffectConstant (Constant value, Expression side_effect, Location loc) : base (loc)
		{
			this.value = value;
			while (side_effect is SideEffectConstant)
				side_effect = ((SideEffectConstant) side_effect).side_effect;
			this.side_effect = side_effect;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			value = value.Resolve (rc);

			type = value.Type;
			eclass = ExprClass.Value;
			return this;
		}

		public override object GetValue ()
		{
			return value.GetValue ();
		}

		public override string GetValueAsLiteral ()
		{
			return value.GetValueAsLiteral ();
		}

		public override long GetValueAsLong ()
		{
			return value.GetValueAsLong ();
		}

		public override void Emit (EmitContext ec)
		{
			side_effect.EmitSideEffect (ec);
			value.Emit (ec);
		}

		public override void EmitSideEffect (EmitContext ec)
		{
			side_effect.EmitSideEffect (ec);
			value.EmitSideEffect (ec);
		}

		public override bool IsDefaultValue {
			get { return value.IsDefaultValue; }
		}

		public override bool IsNegative {
			get { return value.IsNegative; }
		}

		public override bool IsZeroInteger {
			get { return value.IsZeroInteger; }
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			Constant new_value = value.ConvertExplicitly (in_checked_context, target_type);
			if (new_value == null)
				return null;

			var c = new SideEffectConstant (new_value, side_effect, new_value.Location);
			c.type = target_type;
			c.eclass = eclass;
			return c;
		}
	}
}
