//
// Expression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace System.Linq.Expressions {

	public abstract class Expression {

		ExpressionType node_type;
		Type type;

		static BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
		static BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

		public ExpressionType NodeType {
			get { return node_type; }
		}

		public Type Type {
			get { return type; }
		}

		// TODO: remove when all Expression subtypes
		// have their constructor implemented
		protected Expression ()
		{
		}

		protected Expression (ExpressionType node_type, Type type)
		{
			this.node_type = node_type;
			this.type = type;
		}

		public override string ToString ()
		{
			return ExpressionPrinter.ToString (this);
		}

		static void CheckMethod (MethodInfo m)
		{
		}

#region Binary Expressions
		static bool IsInt (Type t)
		{
			return t == typeof (byte) || t == typeof (sbyte) ||
				t == typeof (short) || t == typeof (ushort) ||
				t == typeof (int) || t == typeof (uint) ||
				t == typeof (long) || t == typeof (ulong);
		}

		static bool IsNumber (Type t)
		{
			if (IsInt (t))
				return true;

			return t == typeof (float) || t == typeof (double) || t == typeof (decimal);
		}

		static MethodInfo GetUnaryOperator (string oper_name, Type on_type, Expression expression)
		{
			var methods = on_type.GetMethods (PublicStatic);

			foreach (var method in methods) {
				if (method.Name != oper_name)
					continue;

				var parameters = method.GetParameters ();
				if (parameters.Length != 1)
					continue;

				if (!parameters [0].ParameterType.IsAssignableFrom (expression.Type))
					continue;

				return method;
			}

			return null;
		}

		static MethodInfo UnaryCoreCheck (string oper_name, Expression expression, MethodInfo method)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");

			if (method != null) {
				if (method.ReturnType == typeof (void))
					throw new ArgumentException ("Specified method must return a value", "method");

				if (!method.IsStatic)
					throw new ArgumentException ("Method must be static", "method");

				var parameters = method.GetParameters ();

				if (parameters.Length != 1)
					throw new ArgumentException ("Must have only one parameters", "method");

				if (!parameters [0].ParameterType.IsAssignableFrom (expression.Type))
					throw new InvalidOperationException ("left-side argument type does not match left expression type");

				return method;
			} else {
				if (IsNumber (expression.Type))
					return null;

				if (oper_name != null) {
					method = GetUnaryOperator (oper_name, expression.Type, expression);
					if (method != null)
						return method;
				}

				throw new InvalidOperationException (
					String.Format ("Operation {0} not defined for {1}", oper_name != null ? oper_name.Substring (3) : "is", expression.Type));
			}
		}

		static MethodInfo GetBinaryOperator (string oper_name, Type on_type, Expression left, Expression right)
		{
			MethodInfo [] methods = on_type.GetMethods (PublicStatic);

			foreach (MethodInfo m in methods){
				if (m.Name != oper_name)
					continue;

				ParameterInfo [] pi = m.GetParameters ();
				if (pi.Length != 2)
					continue;

				if (!pi [0].ParameterType.IsAssignableFrom (left.Type))
					continue;

				if (!pi [1].ParameterType.IsAssignableFrom (right.Type))
					continue;

				// Method has papers in order.
				return m;
			}

			return null;
		}

		//
		// Performs basic checks on the incoming expressions for binary expressions
		// and any provided MethodInfo.
		//
		static MethodInfo BinaryCoreCheck (string oper_name, Expression left, Expression right, MethodInfo method)
		{
			if (left == null)
				throw new ArgumentNullException ("left");
			if (right == null)
				throw new ArgumentNullException ("right");

			if (method != null){
				if (method.ReturnType == typeof (void))
					throw new ArgumentException ("Specified method must return a value", "method");

				if (!method.IsStatic)
					throw new ArgumentException ("Method must be static", "method");
				ParameterInfo [] pi = method.GetParameters ();

				if (pi.Length != 2)
					throw new ArgumentException ("Must have only two parameters", "method");

				Type ltype = left.Type.IsValueType && IsNullable (left.Type) ? GetNullableOf(left.Type) : left.Type;
				Type rtype = left.Type.IsValueType && IsNullable (right.Type) ? GetNullableOf(right.Type) :right.Type;
					
				if (ltype != pi [0].ParameterType)
					throw new InvalidOperationException ("left-side argument type does not match left expression type");

				if (rtype != pi [1].ParameterType)
					throw new InvalidOperationException ("right-side argument type does not match right expression type");

				return method;
			} else {
				Type ltype = left.Type;
				Type rtype = right.Type;
				Type ultype = left.Type;
				Type urtype = right.Type;

				if (IsNullable (ltype))
					ultype = GetNullableOf (ltype);

				if (IsNullable (rtype))
					urtype = GetNullableOf (rtype);

				if (oper_name == "op_BitwiseOr" || oper_name == "op_BitwiseAnd"){
					if (ultype == typeof (bool)){
						if (ultype == urtype && ltype == rtype)
							return null;
					}
				}
				
				if (oper_name == "op_LogicalAnd" || oper_name == "op_LogicalOr"){
					if (ultype == typeof (bool)){
						if (ultype == urtype && ltype == rtype)
							return null;

						method = GetBinaryOperator (oper_name, rtype, left, right);
						if (method != null)
							return method;
					}
				} else {
					// Use IsNumber to avoid expensive reflection.
					if (IsNumber (ultype)){
						if (ultype == urtype && ltype == rtype)
							return method;
						
						if (oper_name != null){
							method = GetBinaryOperator (oper_name, rtype, left, right);
							if (method != null)
								return method;
						}
					}
				}
				
				if (oper_name != null){
					method = GetBinaryOperator (oper_name, ltype, left, right);
					if (method != null)
						return method;
				}

				//
				// == and != allow reference types without operators defined.
				//
				if (!ltype.IsValueType && !rtype.IsValueType &&
				    (oper_name == "op_Equality" || oper_name == "op_Inequality"))
					return null;
				
				throw new InvalidOperationException (
					String.Format ("Operation {0} not defined for {1} and {2}", oper_name != null ? oper_name.Substring (3) : "is", ltype, rtype));
			}
		}

		//
		// This is like BinaryCoreCheck, but if no method is used adds the restriction that
		// only ints and bools are allowed
		//
		static MethodInfo BinaryBitwiseCoreCheck (string oper_name, Expression left, Expression right, MethodInfo method)
		{
			if (left == null)
				throw new ArgumentNullException ("left");
			if (right == null)
				throw new ArgumentNullException ("right");

			if (method == null){
				// avoid reflection shortcut and catches Ints/bools before we check Numbers in general
				if (left.Type == right.Type && (left.Type == typeof (bool) || IsInt (left.Type)))
					return method;
			}

			method = BinaryCoreCheck (oper_name, left, right, method);
			if (method == null){
				//
				// The check in BinaryCoreCheck allows a bit more than we do
				// (floats and doubles).  Catch this here
				//
				if (left.Type == typeof(double) || left.Type == typeof(float))
					throw new InvalidOperationException ("Types not supported");
			}
			return method;
		}

		static BinaryExpression MakeSimpleBinary (ExpressionType et, Expression left, Expression right, MethodInfo method)
		{
			Type result = method == null ? left.Type : method.ReturnType;
			bool is_lifted;
			
			if (method == null){
				if (IsNullable (left.Type)){
					if (!IsNullable (right.Type))
						throw new Exception ("Assertion, internal error: left is nullable, requires right to be as well");
					is_lifted = true;
				} else
					is_lifted = false;
			} else {
				//
				// FIXME: implement
				//
				is_lifted = false;
			}
			
			return new BinaryExpression (et, result, left, right, false, is_lifted, method, null);
		}

		static UnaryExpression MakeSimpleUnary (ExpressionType et, Expression expression, MethodInfo method)
		{
			Type result = method == null ? expression.Type : method.ReturnType;

			return new UnaryExpression (et, expression, result, method);
		}

		static BinaryExpression MakeBoolBinary (ExpressionType et, Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			Type result;
			Type ltype = left.Type;
			Type rtype = right.Type;
			bool lnullable = IsNullable (ltype);
			bool rnullable = IsNullable (rtype);
			bool is_lifted;

			//
			// Implement the rules as described in "Expression.Equal" method.  
			//
			if (method == null){
				if (lnullable == false && rnullable == false){
					is_lifted = false;
					result = typeof (bool);
				} else if (lnullable && rnullable){
					is_lifted = true;
					result = liftToNull ? typeof(bool?) : typeof (bool);
				} else
					throw new Exception ("Internal error: this should have been caught in BinaryCoreCheck");
			} else {
				ParameterInfo [] pi = method.GetParameters ();
				Type mltype = pi [0].ParameterType;
				Type mrtype = pi [1].ParameterType;
				
				if (ltype == mltype && rtype == mrtype){
					is_lifted = false;
					result = method.ReturnType;
				}
				else if (ltype.IsValueType && rtype.IsValueType &&
					   ((lnullable && GetNullableOf (ltype) == mltype) ||
					    (rnullable && GetNullableOf (rtype) == mrtype))){
					is_lifted = true;
					if (method.ReturnType == typeof(bool)){
						result = liftToNull ? typeof(bool?) : typeof(bool);
					} else {
						//
						// This behavior is not documented: what
						// happens if the result is not typeof(bool), but
						// the parameters are nullable: the result
						// becomes nullable<returntype>
						//
						// See:
						// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=323139
						result = typeof (Nullable<>).MakeGenericType (method.ReturnType);
							//Type.GetType ("System.Nullable`1[" + method.ReturnType.ToString () + "]");
					}
				} else {
					is_lifted = false;
					result = method.ReturnType;
				}
			}

			return new BinaryExpression (et, result, left, right, liftToNull, is_lifted, method, null);
		}

		//
		// Arithmetic
		//
		public static BinaryExpression Add (Expression left, Expression right)
		{
			return Add (left, right, null);
		}

		public static BinaryExpression Add (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Addition", left, right, method);

			return MakeSimpleBinary (ExpressionType.Add, left, right, method);
		}

		public static BinaryExpression AddChecked (Expression left, Expression right)
		{
			return AddChecked (left, right, null);
		}

		public static BinaryExpression AddChecked (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Addition", left, right, method);

			//
			// The check in BinaryCoreCheck allows a bit more than we do
			// (byte, sbyte).  Catch that here
			//
			
			if (method == null){
				Type ltype = left.Type;

				if (ltype == typeof (byte) || ltype == typeof (sbyte))
					throw new InvalidOperationException (String.Format ("SubtractChecked not defined for {0} and {1}", left.Type, right.Type));
			}

			return MakeSimpleBinary (ExpressionType.AddChecked, left, right, method);
		}

		public static BinaryExpression Subtract (Expression left, Expression right)
		{
			return Subtract (left, right, null);
		}

		public static BinaryExpression Subtract (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Subtraction", left, right, method);
			return MakeSimpleBinary (ExpressionType.Subtract, left, right, method);
		}

		public static BinaryExpression SubtractChecked (Expression left, Expression right)
		{
			return SubtractChecked (left, right, null);
		}

		public static BinaryExpression SubtractChecked (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Subtraction", left, right, method);

			//
			// The check in BinaryCoreCheck allows a bit more than we do
			// (byte, sbyte).  Catch that here
			//

			if (method == null){
				Type ltype = left.Type;

				if (ltype == typeof (byte) || ltype == typeof (sbyte))
					throw new InvalidOperationException (String.Format ("SubtractChecked not defined for {0} and {1}", left.Type, right.Type));
			}
			return MakeSimpleBinary (ExpressionType.SubtractChecked, left, right, method);
		}

		public static BinaryExpression Modulo (Expression left, Expression right)
		{
			return Modulo (left, right, null);
		}

		public static BinaryExpression Modulo (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Modulus", left, right, method);

			return MakeSimpleBinary (ExpressionType.Modulo, left, right, method);
		}

		public static BinaryExpression Multiply (Expression left, Expression right)
		{
			return Multiply (left, right, null);
		}

		public static BinaryExpression Multiply (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Multiply", left, right, method);

			return MakeSimpleBinary (ExpressionType.Multiply, left, right, method);
		}

		public static BinaryExpression MultiplyChecked (Expression left, Expression right)
		{
			return MultiplyChecked (left, right, null);
		}

		public static BinaryExpression MultiplyChecked (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Multiply", left, right, method);

			return MakeSimpleBinary (ExpressionType.MultiplyChecked, left, right, method);
		}

		public static BinaryExpression Divide (Expression left, Expression right)
		{
			return Divide (left, right, null);
		}

		public static BinaryExpression Divide (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Division", left, right, method);

			return MakeSimpleBinary (ExpressionType.Divide, left, right, method);
		}

		public static BinaryExpression Power (Expression left, Expression right)
		{
			return Power (left, right, null);
		}

		public static BinaryExpression Power (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck (null, left, right, method);

			if (left.Type != typeof (double))
				throw new InvalidOperationException ("Power only supports double arguments");

			return MakeSimpleBinary (ExpressionType.Power, left, right, method);
		}

		//
		// Bitwise
		//
		public static BinaryExpression And (Expression left, Expression right)
		{
			return And (left, right, null);
		}

		public static BinaryExpression And (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryBitwiseCoreCheck ("op_BitwiseAnd", left, right, method);

			return MakeSimpleBinary (ExpressionType.And, left, right, method);
		}

		public static BinaryExpression Or (Expression left, Expression right)
		{
			return Or (left, right, null);
		}

		public static BinaryExpression Or (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryBitwiseCoreCheck ("op_BitwiseOr", left, right, method);

			return MakeSimpleBinary (ExpressionType.Or, left, right, method);
		}

		public static BinaryExpression ExclusiveOr (Expression left, Expression right)
		{
			return ExclusiveOr (left, right, null);
		}

		public static BinaryExpression ExclusiveOr (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryBitwiseCoreCheck ("op_ExclusiveOr", left, right, method);

			return MakeSimpleBinary (ExpressionType.ExclusiveOr, left, right, method);
		}

		public static BinaryExpression LeftShift (Expression left, Expression right)
		{
			return LeftShift (left, right, null);
		}

		public static BinaryExpression LeftShift (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryBitwiseCoreCheck ("op_LeftShift", left, right, method);

			return MakeSimpleBinary (ExpressionType.LeftShift, left, right, method);
		}

		public static BinaryExpression RightShift (Expression left, Expression right)
		{
			return RightShift (left, right, null);
		}

		public static BinaryExpression RightShift (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_RightShift", left, right, method);

			return MakeSimpleBinary (ExpressionType.RightShift, left, right, method);
		}

		//
		// Short-circuit
		//
		public static BinaryExpression AndAlso (Expression left, Expression right)
		{
			return AndAlso (left, right, null);
		}

		public static BinaryExpression AndAlso (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_BitwiseAnd", left, right, method);

			return MakeBoolBinary (ExpressionType.AndAlso, left, right, false, method);
		}

		public static BinaryExpression OrElse (Expression left, Expression right)
		{
			return OrElse (left, right, null);
		}

		public static BinaryExpression OrElse (Expression left, Expression right, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_BitwiseOr", left, right, method);
			if (method == null){
				if (left.Type != typeof (bool))
					throw new InvalidOperationException ("Only booleans are allowed for OrElse");
			}

			return MakeBoolBinary (ExpressionType.OrElse, left, right, false, method);
		}

		//
		// Comparison
		//
		public static BinaryExpression Equal (Expression left, Expression right)
		{
			return Equal (left, right, false, null);
		}

		public static BinaryExpression Equal (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Equality", left, right, method);

			return MakeBoolBinary (ExpressionType.Equal, left, right, liftToNull, method);
		}

		public static BinaryExpression NotEqual (Expression left, Expression right)
		{
			return NotEqual (left, right, false, null);
		}


		public static BinaryExpression NotEqual (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_Inequality", left, right, method);

			return MakeBoolBinary (ExpressionType.NotEqual, left, right, liftToNull, method);
		}

		public static BinaryExpression GreaterThan (Expression left, Expression right)
		{
			return GreaterThan (left, right, false, null);
		}

		public static BinaryExpression GreaterThan (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_GreaterThan", left, right, method);

			return MakeBoolBinary (ExpressionType.GreaterThan, left, right, liftToNull, method);
		}

		public static BinaryExpression GreaterThanOrEqual (Expression left, Expression right)
		{
			return GreaterThanOrEqual (left, right, false, null);
		}


		public static BinaryExpression GreaterThanOrEqual (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_GreaterThanOrEqual", left, right, method);

			return MakeBoolBinary (ExpressionType.GreaterThanOrEqual, left, right, liftToNull, method);
		}

		public static BinaryExpression LessThan (Expression left, Expression right)
		{
			return LessThan (left, right, false, null);
		}

		public static BinaryExpression LessThan (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_LessThan", left, right, method);

			return MakeBoolBinary (ExpressionType.LessThan, left, right, liftToNull, method);
		}

		public static BinaryExpression LessThanOrEqual (Expression left, Expression right)
		{
			return LessThanOrEqual (left, right, false, null);
		}

		public static BinaryExpression LessThanOrEqual (Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			method = BinaryCoreCheck ("op_LessThanOrEqual", left, right, method);

			return MakeBoolBinary (ExpressionType.LessThanOrEqual, left, right, liftToNull, method);
		}

		//
		// Miscelaneous
		//

		static void ArrayCheck (Expression array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (!array.Type.IsArray)
				throw new ArgumentException ("The array argument must be of type array");
		}

		public static BinaryExpression ArrayIndex (Expression array, Expression index)
		{
			ArrayCheck (array);
			if (index == null)
				throw new ArgumentNullException ("index");
			if (array.Type.GetArrayRank () != 1)
				throw new ArgumentException ("The array argument must be a single dimensional array");
			if (index.Type != typeof (int))
				throw new ArgumentException ("The index must be of type int");

			return new BinaryExpression (ExpressionType.ArrayIndex, array.Type.GetElementType (), array, index);
		}

		public static BinaryExpression Coalesce (Expression left, Expression right)
		{
			return Coalesce (left, right, null);
		}

		public static BinaryExpression Coalesce (Expression left, Expression right, LambdaExpression conversion)
		{
			if (left == null)
				throw new ArgumentNullException ("left");
			if (right == null)
				throw new ArgumentNullException ("right");

			//
			// First arg must ne nullable (either Nullable<T> or a reference type
			//
			if (left.Type.IsValueType && !IsNullable (left.Type))
				throw new InvalidOperationException ("Left expression can never be null");
			
			Type result = null;

			if (IsNullable (left.Type)){
				Type lbase = GetNullableOf (left.Type);
				
				if (!IsNullable (right.Type) && lbase.IsAssignableFrom (right.Type))
					result = lbase;
			}

			if (result == null && left.Type.IsAssignableFrom (right.Type))
				result = left.Type;

			if (result == null){
				if (IsNullable (left.Type) && right.Type.IsAssignableFrom (GetNullableOf (left.Type))){
					result = right.Type;
				}
			}

			if (result == null)
				throw new ArgumentException ("Incompatible argument types");

			//
			// FIXME: What do we do with "conversion"?
			//
			return new BinaryExpression (ExpressionType.Coalesce, result, left, right, false, false, null, conversion);
		}

		//
		// MakeBinary constructors
		//
		public static BinaryExpression MakeBinary (ExpressionType binaryType, Expression left, Expression right)
		{
			return MakeBinary (binaryType, left, right, false, null);
		}

		public static BinaryExpression MakeBinary (ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method)
		{
			return MakeBinary (binaryType, left, right, liftToNull, method, null);
		}

		public static BinaryExpression MakeBinary (ExpressionType binaryType, Expression left, Expression right, bool liftToNull, MethodInfo method, LambdaExpression conversion)
		{
			switch (binaryType) {
			case ExpressionType.Add:
				return Add (left, right, method);
			case ExpressionType.AddChecked:
				return AddChecked (left, right, method);
			case ExpressionType.AndAlso:
				return AndAlso (left, right);
			case ExpressionType.Coalesce:
				return Coalesce (left, right, conversion);
			case ExpressionType.Divide:
				return Divide (left, right, method);
			case ExpressionType.Equal:
				return Equal (left, right, liftToNull, method);
			case ExpressionType.ExclusiveOr:
				return ExclusiveOr (left, right, method);
			case ExpressionType.GreaterThan:
				return GreaterThan (left, right, liftToNull, method);
			case ExpressionType.GreaterThanOrEqual:
				return GreaterThanOrEqual (left, right, liftToNull, method);
			case ExpressionType.LeftShift:
				return LeftShift (left, right, method);
			case ExpressionType.LessThan:
				return LessThan (left, right, liftToNull, method);
			case ExpressionType.LessThanOrEqual:
				return LessThanOrEqual (left, right, liftToNull, method);
			case ExpressionType.Modulo:
				return Modulo (left, right, method);
			case ExpressionType.Multiply:
				return Multiply (left, right, method);
			case ExpressionType.MultiplyChecked:
				return MultiplyChecked (left, right, method);
			case ExpressionType.NotEqual:
				return NotEqual (left, right, liftToNull, method);
			case ExpressionType.OrElse:
				return OrElse (left, right);
			case ExpressionType.Power:
				return Power (left, right, method);
			case ExpressionType.RightShift:
				return RightShift (left, right, method);
			case ExpressionType.Subtract:
				return Subtract (left, right, method);
			case ExpressionType.SubtractChecked:
				return SubtractChecked (left, right, method);
			case ExpressionType.And:
				return And (left, right, method);
			case ExpressionType.Or:
				return Or (left, right, method);
			}

			throw new ArgumentException ("MakeBinary expect a binary node type");
		}

#endregion

		public static MethodCallExpression ArrayIndex (Expression array, params Expression [] indexes)
		{
			return ArrayIndex (array, indexes as IEnumerable<Expression>);
		}

		public static MethodCallExpression ArrayIndex (Expression array, IEnumerable<Expression> indexes)
		{
			ArrayCheck (array);

			if (indexes == null)
				throw new ArgumentNullException ("indexes");

			var args = indexes.ToReadOnlyCollection ();
			if (array.Type.GetArrayRank () != args.Count)
				throw new ArgumentException ("The number of arguments doesn't match the rank of the array");

			foreach (var arg in args)
				if (arg.Type != typeof (int))
					throw new ArgumentException ("The index must be of type int");

			return Call (array, array.Type.GetMethod ("Get", PublicInstance), args);
		}

		public static UnaryExpression ArrayLength (Expression array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (!array.Type.IsArray)
				throw new ArgumentException ("The type of the expression must me Array");
			if (array.Type.GetArrayRank () != 1)
				throw new ArgumentException ("The array must be a single dimensional array");

			return new UnaryExpression (ExpressionType.ArrayLength, array, typeof (int));
		}

		[MonoTODO]
		public static MemberAssignment Bind (MemberInfo member, Expression expression)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberAssignment Bind (MethodInfo propertyAccessor, Expression expression)
		{
			throw new NotImplementedException ();
		}

		public static MethodCallExpression Call (Expression instance, MethodInfo method)
		{
			return Call (instance, method, null as IEnumerable<Expression>);
		}

		public static MethodCallExpression Call (MethodInfo method, params Expression [] arguments)
		{
			return Call (null, method, arguments as IEnumerable<Expression>);
		}

		public static MethodCallExpression Call (Expression instance, MethodInfo method, params Expression [] arguments)
		{
			return Call (instance, method, arguments as IEnumerable<Expression>);
		}

		public static MethodCallExpression Call (Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
		{
			if (method == null)
				throw new ArgumentNullException ("method");
			if (instance == null && !method.IsStatic)
				throw new ArgumentNullException ("instance");
			if (instance != null && !method.DeclaringType.IsAssignableFrom (instance.Type))
				throw new ArgumentException ("Type is not assignable to the declaring type of the method");

			var args = arguments.ToReadOnlyCollection ();
			var parameters = method.GetParameters ();

			if (args.Count != parameters.Length)
				throw new ArgumentException ("The number of arguments doesn't match the number of parameters");

			// TODO: check for assignability of the arguments on the parameters

			return new MethodCallExpression (instance, method, args);
		}

		[MonoTODO]
		public static MethodCallExpression Call (Expression instance, string methodName, Type [] typeArguments, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MethodCallExpression Call (Type type, string methodName, Type [] typeArguments, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		public static ConditionalExpression Condition (Expression test, Expression ifTrue, Expression ifFalse)
		{
			if (test == null)
				throw new ArgumentNullException ("test");
			if (ifTrue == null)
				throw new ArgumentNullException ("ifTrue");
			if (ifFalse == null)
				throw new ArgumentNullException ("ifFalse");
			if (test.Type != typeof (bool))
				throw new ArgumentException ("Test expression should be of type bool");
			if (ifTrue.Type != ifFalse.Type)
				throw new ArgumentException ("The ifTrue and ifFalse type do not match");

			return new ConditionalExpression (test, ifTrue, ifFalse);
		}

		public static ConstantExpression Constant (object value)
		{
			if (value == null)
				return new ConstantExpression (null, typeof (object));

			return Constant (value, value.GetType ());
		}

		public static ConstantExpression Constant (object value, Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			//
			// value must be compatible with type, no conversions
			// are allowed
			//
			if (value == null){
				if (type.IsValueType && !IsNullable (type))
					throw new ArgumentException ();
			} else {
				if (!(type.IsValueType && IsNullable (type)) && value.GetType () != type)
					throw new ArgumentException ();

			}

			return new ConstantExpression (value, type);
		}

		[MonoTODO]
		public static UnaryExpression Convert (Expression expression, Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression Convert (Expression expression, Type type, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression ConvertChecked (Expression expression, Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static UnaryExpression ConvertChecked (Expression expression, Type type, MethodInfo method)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ElementInit ElementInit (MethodInfo addMethod, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ElementInit ElementInit (MethodInfo addMethod, IEnumerable<Expression> arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Field (Expression expression, FieldInfo field)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Field (Expression expression, string fieldName)
		{
			throw new NotImplementedException ();
		}

		public static Type GetActionType (params Type [] typeArgs)
		{
			if (typeArgs == null)
				throw new ArgumentNullException ("typeArgs");

			if (typeArgs.Length > 4)
				throw new ArgumentException ("No Action type of this arity");

			if (typeArgs.Length == 0)
				return typeof (Action);

			Type action = null;
			switch (typeArgs.Length) {
			case 1:
				action = typeof (Action<>);
				break;
			case 2:
				action = typeof (Action<,>);
				break;
			case 3:
				action = typeof (Action<,,>);
				break;
			case 4:
				action = typeof (Action<,,,>);
				break;
			}

			return action.MakeGenericType (typeArgs);
		}

		public static Type GetFuncType (params Type [] typeArgs)
		{
			if (typeArgs == null)
				throw new ArgumentNullException ("typeArgs");

			if (typeArgs.Length < 1 || typeArgs.Length > 5)
				throw new ArgumentException ("No Func type of this arity");

			Type func = null;
			switch (typeArgs.Length) {
			case 1:
				func = typeof (Func<>);
				break;
			case 2:
				func = typeof (Func<,>);
				break;
			case 3:
				func = typeof (Func<,,>);
				break;
			case 4:
				func = typeof (Func<,,,>);
				break;
			case 5:
				func = typeof (Func<,,,,>);
				break;
			}

			return func.MakeGenericType (typeArgs);
		}

		[MonoTODO]
		public static InvocationExpression Invoke (Expression expression, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static InvocationExpression Invoke (Expression expression, IEnumerable<Expression> arguments)
		{
			throw new NotImplementedException ();
		}

		public static Expression<TDelegate> Lambda<TDelegate> (Expression body, params ParameterExpression [] parameters)
		{
			if (body == null)
				throw new ArgumentNullException ("body");

			return new Expression<TDelegate> (body, parameters);
		}

		[MonoTODO]
		public static Expression<TDelegate> Lambda<TDelegate> (Expression body, IEnumerable<ParameterExpression> parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static LambdaExpression Lambda (Expression body, params ParameterExpression [] parameters)
		{
			throw new NotImplementedException ();
		}

		public static LambdaExpression Lambda (Type delegateType, Expression body, params ParameterExpression [] parameters)
		{
			return Lambda (delegateType, body, parameters as IEnumerable<ParameterExpression>);
		}

		[MonoTODO]
		public static LambdaExpression Lambda (Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
		{
			if (delegateType == null)
				throw new ArgumentNullException ("delegateType");
			if (body == null)
				throw new ArgumentNullException ("body");

			return new LambdaExpression (delegateType, body, parameters.ToReadOnlyCollection ());
		}

		public static MemberListBinding ListBind (MemberInfo member, params ElementInit [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberListBinding ListBind (MemberInfo member, IEnumerable<ElementInit> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberListBinding ListBind (MethodInfo propertyAccessor, params ElementInit [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberListBinding ListBind (MethodInfo propertyAccessor, IEnumerable<ElementInit> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, params ElementInit [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, IEnumerable<ElementInit> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, params Expression [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, IEnumerable<Expression> initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, MethodInfo addMethod, params Expression [] initializers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ListInitExpression ListInit (NewExpression newExpression, MethodInfo addMethod, IEnumerable<Expression> initializers)
		{
			throw new NotImplementedException ();
		}

		public static MemberExpression MakeMemberAccess (Expression expression, MemberInfo member)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");
			if (member == null)
				throw new ArgumentNullException ("member");

			var field = member as FieldInfo;
			if (field != null)
				return Field (expression, field);

			var property = member as PropertyInfo;
			if (property != null)
				return Property (expression, property);

			throw new ArgumentException ("Member should either be a field or a property");
		}

		public static UnaryExpression MakeUnary (ExpressionType unaryType, Expression operand, Type type)
		{
			return MakeUnary (unaryType, operand, type, null);
		}

		public static UnaryExpression MakeUnary (ExpressionType unaryType, Expression operand, Type type, MethodInfo method)
		{
			switch (unaryType) {
			case ExpressionType.ArrayLength:
				return ArrayLength (operand);
			case ExpressionType.Convert:
				return Convert (operand, type, method);
			case ExpressionType.ConvertChecked:
				return ConvertChecked (operand, type, method);
			case ExpressionType.Negate:
				return Negate (operand, method);
			case ExpressionType.NegateChecked:
				return NegateChecked (operand, method);
			case ExpressionType.Not:
				return Not (operand, method);
			case ExpressionType.Quote:
				return Quote (operand);
			case ExpressionType.TypeAs:
				return TypeAs (operand, type);
			case ExpressionType.UnaryPlus:
				return UnaryPlus (operand, method);
			}

			throw new ArgumentException ("MakeUnary expect an unary operator");
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MemberInfo member, params MemberBinding [] binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MemberInfo member, IEnumerable<MemberBinding> binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MethodInfo propertyAccessor, params MemberBinding [] binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberMemberBinding MemberBind (MethodInfo propertyAccessor, IEnumerable<MemberBinding> binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberInitExpression MemberInit (NewExpression newExpression, params MemberBinding [] binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberInitExpression MemberInit (NewExpression newExpression, IEnumerable<MemberBinding> binding)
		{
			throw new NotImplementedException ();
		}

		public static UnaryExpression Negate (Expression expression)
		{
			return Negate (expression, null);
		}

		public static UnaryExpression Negate (Expression expression, MethodInfo method)
		{
			method = UnaryCoreCheck ("op_UnaryNegation", expression, method);

			return MakeSimpleUnary (ExpressionType.Negate, expression, method);
		}

		public static UnaryExpression NegateChecked (Expression expression)
		{
			return NegateChecked (expression, null);
		}

		public static UnaryExpression NegateChecked (Expression expression, MethodInfo method)
		{
			method = UnaryCoreCheck ("op_UnaryNegation", expression, method);

			return MakeSimpleUnary (ExpressionType.Negate, expression, method);
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, params Expression [] arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, IEnumerable<Expression> arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, IEnumerable<Expression> arguments, params MemberInfo [] members)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static NewExpression New (ConstructorInfo constructor, IEnumerable<Expression> arguments, IEnumerable<MemberInfo> members)
		{
			throw new NotImplementedException ();
		}

		public static NewArrayExpression NewArrayBounds (Type type, params Expression [] bounds)
		{
			return NewArrayBounds (type, bounds as IEnumerable<Expression>);
		}

		public static NewArrayExpression NewArrayBounds (Type type, IEnumerable<Expression> bounds)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (bounds == null)
				throw new ArgumentNullException ("bounds");

			var array_bounds = bounds.ToReadOnlyCollection ();
			foreach (var expression in array_bounds)
				if (!IsInt (expression.Type))
					throw new ArgumentException ("The bounds collection can only contain expression of integers types");

			return new NewArrayExpression (ExpressionType.NewArrayBounds, type.MakeArrayType (array_bounds.Count), array_bounds);
		}

		public static NewArrayExpression NewArrayInit (Type type, params Expression [] initializers)
		{
			return NewArrayInit (type, initializers as IEnumerable<Expression>);
		}

		public static NewArrayExpression NewArrayInit (Type type, IEnumerable<Expression> initializers)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (initializers == null)
				throw new ArgumentNullException ("initializers");

			var array_initializers = initializers.ToReadOnlyCollection ();

			foreach (var expression in initializers) {
				if (expression == null)
					throw new ArgumentNullException ("initializers");

				if (!type.IsAssignableFrom (expression.Type))
					throw new InvalidOperationException ();

				// TODO: Quote elements if type == typeof (Expression)
			}

			return new NewArrayExpression (ExpressionType.NewArrayInit, type.MakeArrayType (), array_initializers);
		}

		public static UnaryExpression Not (Expression expression)
		{
			return Not (expression, null);
		}

		public static UnaryExpression Not (Expression expression, MethodInfo method)
		{
			method = UnaryCoreCheck ("op_LogicalNot", expression, method);

			return MakeSimpleUnary (ExpressionType.Not, expression, method);
		}

		public static ParameterExpression Parameter (Type type, string name)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			return new ParameterExpression (type, name);
		}

		[MonoTODO]
		public static MemberExpression Property (Expression expression, MethodInfo propertyAccessor)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Property (Expression expression, PropertyInfo property)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression Property (Expression expression, string propertyName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberExpression PropertyOrField (Expression expression, string propertyOrFieldName)
		{
			throw new NotImplementedException ();
		}

		public static UnaryExpression Quote (Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");

			return new UnaryExpression (ExpressionType.Quote, expression, expression.GetType ());
		}

		public static UnaryExpression TypeAs (Expression expression, Type type)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");
			if (type == null)
				throw new ArgumentNullException ("type");
			if (type.IsValueType && !IsNullable (type))
				throw new ArgumentException ("TypeAs expect a reference or a nullable type");

			return new UnaryExpression (ExpressionType.TypeAs, expression, type);
		}

		public static TypeBinaryExpression TypeIs (Expression expression, Type type)
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");
			if (type == null)
				throw new ArgumentNullException ("type");

			return new TypeBinaryExpression (ExpressionType.TypeIs, expression, type, typeof (bool));
		}

		public static UnaryExpression UnaryPlus (Expression expression)
		{
			return UnaryPlus (expression, null);
		}

		public static UnaryExpression UnaryPlus (Expression expression, MethodInfo method)
		{
			method = UnaryCoreCheck ("op_UnaryPlus", expression, method);

			return MakeSimpleUnary (ExpressionType.UnaryPlus, expression, method);
		}

		internal static bool IsNullable (Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>);
		}

		internal static bool IsUnsigned (Type t)
		{
			if (t.IsPointer)
				return IsUnsigned (t.GetElementType ());

			return t == typeof (ushort) || t == typeof (uint) || t == typeof (ulong) || t == typeof (byte);
		}

		//
		// returns the T in a a Nullable<T> type.
		//
		internal static Type GetNullableOf (Type type)
		{
			return type.GetGenericArguments () [0];
		}
		
		//
		// This method must be overwritten by derived classes to
		// compile the expression
		//
		internal abstract void Emit (EmitContext ec);
	}
}
