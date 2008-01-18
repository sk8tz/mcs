//
// LambdaExpression.cs
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	internal class EmitContext {
		internal LambdaExpression Owner;
		internal Type [] ParamTypes;
		internal DynamicMethod Method;
		internal ILGenerator ig;
		
		static object mlock = new object ();
		static int method_count;
		static string GenName ()
		{
			lock (mlock){
				return "<LINQ-" + method_count++ + ">";
			}
		}
		
		public EmitContext (LambdaExpression owner)
		{
			Owner = owner;

			ParamTypes = new Type [Owner.parameters.Count];
			for (int i = 0; i < Owner.parameters.Count; i++)
				ParamTypes [i] = Owner.parameters [i].Type;

			//
			// We probably want to use the 3.5 new API calls to associate
			// the method with the "sandboxed" Assembly, instead am currently
			// dumping these types in this class
			//
			Type owner_of_code = typeof (EmitContext);
			
			Method = new DynamicMethod (GenName (), Owner.Type, ParamTypes, owner_of_code);
			ig = Method.GetILGenerator ();
		}

		internal Delegate CreateDelegate ()
		{
			return Method.CreateDelegate (Owner.delegate_type);			
		}
	}
	
	public class LambdaExpression : Expression {

		//
		// LambdaExpression parameters
		//
		Expression body;
		internal ReadOnlyCollection<ParameterExpression> parameters;
		internal Type delegate_type;

		// This is set during compilation
		Delegate lambda_delegate;

		static bool CanAssign (Type target, Type source)
		{
			// This catches object and value type mixage, type compatibility is handled later
			if (target.IsValueType ^ source.IsValueType)
				return false;
			
			return target.IsAssignableFrom (source);
		}

		internal LambdaExpression (Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
			: this (delegateType, body, new ReadOnlyCollection<ParameterExpression> (new List<ParameterExpression> (parameters)))
			{
			}
				
		internal LambdaExpression (Type delegateType, Expression body, ReadOnlyCollection<ParameterExpression> parameters)
			: base (ExpressionType.Lambda, body.Type)
		{
			if (!delegateType.IsSubclassOf (typeof (System.Delegate))){
				throw new ArgumentException ("delegateType");
			}
			
			MethodInfo [] invokes = delegateType.GetMethods (BindingFlags.Instance | BindingFlags.Public);
			MethodInfo invoke = null;
			foreach (MethodInfo m in invokes)
				if (m.Name == "Invoke"){
					invoke = m;
					break;
				}
			if (invoke == null)
				throw new ArgumentException ("delegate must contain an Invoke method", "delegateType");

			ParameterInfo [] invoke_parameters = invoke.GetParameters ();
			if (invoke_parameters.Length != parameters.Count)
				throw new ArgumentException ("Different number of arguments in delegate {0}", "delegateType");

			for (int i = 0; i < invoke_parameters.Length; i++){
				if (!CanAssign (parameters [i].Type, invoke_parameters [i].ParameterType))
					throw new ArgumentException (String.Format ("Can not assign a {0} to a {1}", invoke_parameters [i].ParameterType, parameters [i].Type));
			}

			if (!CanAssign (invoke.ReturnType, body.Type))
				throw new ArgumentException (String.Format ("body type {0} can not be assigned to {1}", body.Type, invoke.ReturnType));
			
			this.body = body;
			this.parameters = parameters;
			delegate_type = delegateType;
		}
		
		public Expression Body {
			get { return body; }
		}

		public ReadOnlyCollection<ParameterExpression> Parameters {
			get { return parameters; }
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
		
		public Delegate Compile ()
		{
			if (lambda_delegate == null){
				EmitContext ec = new EmitContext (this);
				
				body.Emit (ec);

				ec.ig.Emit (OpCodes.Ret);
				lambda_delegate = ec.CreateDelegate ();
			}
			return lambda_delegate;
		}
	}
}
