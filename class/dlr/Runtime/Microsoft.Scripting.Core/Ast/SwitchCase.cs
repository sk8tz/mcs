/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic.Utils;
#else
using Microsoft.Scripting.Utils;
#endif

#if CODEPLEX_40
namespace System.Linq.Expressions {
#else
namespace Microsoft.Linq.Expressions {
#endif
    /// <summary>
    /// Represents one case of a <see cref="SwitchExpression"/>.
    /// </summary>
#if !SILVERLIGHT
    [DebuggerTypeProxy(typeof(Expression.SwitchCaseProxy))]
#endif
    public sealed class SwitchCase {
        private readonly ReadOnlyCollection<Expression> _testValues;
        private readonly Expression _body;

        internal SwitchCase(Expression body, ReadOnlyCollection<Expression> testValues) {
            _body = body;
            _testValues = testValues;
        }

        /// <summary>
        /// Gets the values of this case. This case is selected for execution when the <see cref="SwitchExpression.SwitchValue"/> matches any of these values.
        /// </summary>
        public ReadOnlyCollection<Expression> TestValues {
            get { return _testValues; }
        }

        /// <summary>
        /// Gets the body of this case.
        /// </summary>
        public Expression Body {
            get { return _body; }
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>. 
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="Object"/>. </returns>
        public override string ToString() {
            return ExpressionStringBuilder.SwitchCaseToString(this);
        }

        /// <summary>
        /// Creates a new expression that is like this one, but using the
        /// supplied children. If all of the children are the same, it will
        /// return this expression.
        /// </summary>
        /// <param name="testValues">The <see cref="TestValues" /> property of the result.</param>
        /// <param name="body">The <see cref="Body" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public SwitchCase Update(IEnumerable<Expression> testValues, Expression body) {
            if (testValues == TestValues && body == Body) {
                return this;
            }
            return Expression.SwitchCase(body, testValues);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="Microsoft.Linq.Expressions.SwitchCase">SwitchCase</see> for use in a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="body">The body of the case.</param>
        /// <param name="testValues">The test values of the case.</param>
        /// <returns>The created <see cref="Microsoft.Linq.Expressions.SwitchCase">SwitchCase</see>.</returns>
        public static SwitchCase SwitchCase(Expression body, params Expression[] testValues) {
            return SwitchCase(body, (IEnumerable<Expression>)testValues);
        }

        /// <summary>
        /// Creates a <see cref="Microsoft.Linq.Expressions.SwitchCase">SwitchCase</see> for use in a <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="body">The body of the case.</param>
        /// <param name="testValues">The test values of the case.</param>
        /// <returns>The created <see cref="Microsoft.Linq.Expressions.SwitchCase">SwitchCase</see>.</returns>
        public static SwitchCase SwitchCase(Expression body, IEnumerable<Expression> testValues) {
            RequiresCanRead(body, "body");
            
            var values = testValues.ToReadOnly();
            RequiresCanRead(values, "testValues");
            ContractUtils.RequiresNotEmpty(values, "testValues");

            return new SwitchCase(body, values);
        }
    }
}
