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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

#if CLR2
namespace Microsoft.Scripting.Ast {
#else
namespace System.Linq.Expressions {
#endif
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal sealed class ExpressionStringBuilder : ExpressionVisitor {
        private StringBuilder _out;

        // Associate every unique label or anonymous parameter in the tree with an integer.
        // The label is displayed as Label_#.
        private Dictionary<object, int> _ids;

        private ExpressionStringBuilder() {
            _out = new StringBuilder();
        }

        public override string ToString() {
            return _out.ToString();
        }

        private void AddLabel(LabelTarget label) {
            if (_ids == null) {
                _ids = new Dictionary<object, int>();
                _ids.Add(label, 0);
            } else {
                if (!_ids.ContainsKey(label)) {
                    _ids.Add(label, _ids.Count);
                }
            }
        }

        private int GetLabelId(LabelTarget label) {
            if (_ids == null) {
                _ids = new Dictionary<object, int>();
                AddLabel(label);
                return 0;
            } else {
                int id;
                if (!_ids.TryGetValue(label, out id)) {
                    //label is met the first time
                    id = _ids.Count;
                    AddLabel(label);
                }
                return id;
            }
        }

        private void AddParam(ParameterExpression p) {
            if (_ids == null) {
                _ids = new Dictionary<object, int>();
                _ids.Add(_ids, 0);
            } else {
                if (!_ids.ContainsKey(p)) {
                    _ids.Add(p, _ids.Count);
                }
            }
        }

        private int GetParamId(ParameterExpression p) {
            if (_ids == null) {
                _ids = new Dictionary<object, int>();
                AddParam(p);
                return 0;
            } else {
                int id;
                if (!_ids.TryGetValue(p, out id)) {
                    // p is met the first time
                    id = _ids.Count;
                    AddParam(p);
                }
                return id;
            }
        }

        #region The printing code

        private void Out(string s) {
            _out.Append(s);
        }

        private void Out(char c) {
            _out.Append(c);
        }

        #endregion

        #region Output an expresstion tree to a string

        /// <summary>
        /// Output a given expression tree to a string.
        /// </summary>
        internal static string ExpressionToString(Expression node) {
            Debug.Assert(node != null);
            ExpressionStringBuilder esb = new ExpressionStringBuilder();
            esb.Visit(node);
            return esb.ToString();
        }

        internal static string CatchBlockToString(CatchBlock node) {
            Debug.Assert(node != null);
            ExpressionStringBuilder esb = new ExpressionStringBuilder();
            esb.VisitCatchBlock(node);
            return esb.ToString();
        }

        internal static string SwitchCaseToString(SwitchCase node) {
            Debug.Assert(node != null);
            ExpressionStringBuilder esb = new ExpressionStringBuilder();
            esb.VisitSwitchCase(node);
            return esb.ToString();
        }

        /// <summary>
        /// Output a given member binding to a string.
        /// </summary>
        internal static string MemberBindingToString(MemberBinding node) {
            Debug.Assert(node != null);
            ExpressionStringBuilder esb = new ExpressionStringBuilder();
            esb.VisitMemberBinding(node);
            return esb.ToString();
        }

        /// <summary>
        /// Output a given ElementInit to a string.
        /// </summary>
        internal static string ElementInitBindingToString(ElementInit node) {
            Debug.Assert(node != null);
            ExpressionStringBuilder esb = new ExpressionStringBuilder();
            esb.VisitElementInit(node);
            return esb.ToString();
        }

        // More proper would be to make this a virtual method on Action
        private static string FormatBinder(CallSiteBinder binder) {
            ConvertBinder convert;
            GetMemberBinder getMember;
            SetMemberBinder setMember;
            DeleteMemberBinder deleteMember;
            GetIndexBinder getIndex;
            SetIndexBinder setIndex;
            DeleteIndexBinder deleteIndex;
            InvokeMemberBinder call;
            InvokeBinder invoke;
            CreateInstanceBinder create;
            UnaryOperationBinder unary;
            BinaryOperationBinder binary;

            if ((convert = binder as ConvertBinder) != null) {
                return "Convert " + convert.Type;
            } else if ((getMember = binder as GetMemberBinder) != null) {
                return "GetMember " + getMember.Name;
            } else if ((setMember = binder as SetMemberBinder) != null) {
                return "SetMember " + setMember.Name;
            } else if ((deleteMember = binder as DeleteMemberBinder) != null) {
                return "DeleteMember " + deleteMember.Name;
            } else if ((getIndex = binder as GetIndexBinder) != null) {
                return "GetIndex";
            } else if ((setIndex = binder as SetIndexBinder) != null) {
                return "SetIndex";
            } else if ((deleteIndex = binder as DeleteIndexBinder) != null) {
                return "DeleteIndex";
            } else if ((call = binder as InvokeMemberBinder) != null) {
                return "Call " + call.Name;
            } else if ((invoke = binder as InvokeBinder) != null) {
                return "Invoke";
            } else if ((create = binder as CreateInstanceBinder) != null) {
                return "Create";
            } else if ((unary = binder as UnaryOperationBinder) != null) {
                return unary.Operation.ToString();
            } else if ((binary = binder as BinaryOperationBinder) != null) {
                return binary.Operation.ToString();
            } else {
                return "CallSiteBinder";
            }
        }

        private void VisitExpressions<T>(char open, IList<T> expressions, char close) where T : Expression {
            Out(open);
            if (expressions != null) {
                bool isFirst = true;
                foreach (T e in expressions) {
                    if (isFirst) {
                        isFirst = false;
                    } else {
                        Out(", ");
                    }
                    Visit(e);
                }
            }
            Out(close);
        }

        protected internal override Expression VisitDynamic(DynamicExpression node) {
            Out(FormatBinder(node.Binder));
            VisitExpressions('(', node.Arguments, ')');
            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected internal override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.ArrayIndex) {
                Visit(node.Left);
                Out("[");
                Visit(node.Right);
                Out("]");
            } else {
                string op;
                switch (node.NodeType) {
                    case ExpressionType.Assign: op = "="; break;
                    case ExpressionType.Equal: op = "=="; break;
                    case ExpressionType.NotEqual: op = "!="; break;
                    case ExpressionType.AndAlso: op = "AndAlso"; break;
                    case ExpressionType.OrElse: op = "OrElse"; break;
                    case ExpressionType.GreaterThan: op = ">"; break;
                    case ExpressionType.LessThan: op = "<"; break;
                    case ExpressionType.GreaterThanOrEqual: op = ">="; break;
                    case ExpressionType.LessThanOrEqual: op = "<="; break;
                    case ExpressionType.Add: op = "+"; break;
                    case ExpressionType.AddAssign: op = "+="; break;
                    case ExpressionType.AddAssignChecked: op = "+="; break;
                    case ExpressionType.AddChecked: op = "+"; break;
                    case ExpressionType.Subtract: op = "-"; break;
                    case ExpressionType.SubtractAssign: op = "-="; break;
                    case ExpressionType.SubtractAssignChecked: op = "-="; break;
                    case ExpressionType.SubtractChecked: op = "-"; break;
                    case ExpressionType.Divide: op = "/"; break;
                    case ExpressionType.DivideAssign: op = "/="; break;
                    case ExpressionType.Modulo: op = "%"; break;
                    case ExpressionType.ModuloAssign: op = "%="; break;
                    case ExpressionType.Multiply: op = "*"; break;
                    case ExpressionType.MultiplyAssign: op = "*="; break;
                    case ExpressionType.MultiplyAssignChecked: op = "*="; break;
                    case ExpressionType.MultiplyChecked: op = "*"; break;
                    case ExpressionType.LeftShift: op = "<<"; break;
                    case ExpressionType.LeftShiftAssign: op = "<<="; break;
                    case ExpressionType.RightShift: op = ">>"; break;
                    case ExpressionType.RightShiftAssign: op = ">>="; break;
                    case ExpressionType.And:
                        if (node.Type == typeof(bool) || node.Type == typeof(bool?)) {
                            op = "And";
                        } else {
                            op = "&";
                        }
                        break;
                    case ExpressionType.AndAssign:
                        if (node.Type == typeof(bool) || node.Type == typeof(bool?)) {
                            op = "&&=";
                        } else {
                            op = "&=";
                        }
                        break;
                    case ExpressionType.Or:
                        if (node.Type == typeof(bool) || node.Type == typeof(bool?)) {
                            op = "Or";
                        } else {
                            op = "|";
                        }
                        break;
                    case ExpressionType.OrAssign:
                        if (node.Type == typeof(bool) || node.Type == typeof(bool?)) {
                            op = "||=";
                        } else { op = "|="; }
                        break;
                    case ExpressionType.ExclusiveOr: op = "^"; break;
                    case ExpressionType.ExclusiveOrAssign: op = "^="; break;
                    case ExpressionType.Power: op = "^"; break;
                    case ExpressionType.PowerAssign: op = "**="; break;
                    case ExpressionType.Coalesce: op = "??"; break;

                    default:
                        throw new InvalidOperationException();
                }
                Out("(");
                Visit(node.Left);
                Out(' ');
                Out(op);
                Out(' ');
                Visit(node.Right);
                Out(")");
            }
            return node;
        }

        protected internal override Expression VisitParameter(ParameterExpression node) {
            if (node.IsByRef) {
                Out("ref ");
            }
            if (String.IsNullOrEmpty(node.Name)) {
                int id = GetParamId(node);
                Out("Param_" + id);
            } else {
                Out(node.Name);
            }
            return node;
        }

        protected internal override Expression VisitLambda<T>(Expression<T> node) {
            if (node.Parameters.Count == 1) {
                // p => body
                Visit(node.Parameters[0]);
            } else {
                // (p1, p2, ..., pn) => body
                VisitExpressions('(', node.Parameters, ')');
            }
            Out(" => ");
            Visit(node.Body);
            return node;
        }

        protected internal override Expression VisitListInit(ListInitExpression node) {
            Visit(node.NewExpression);
            Out(" {");
            for (int i = 0, n = node.Initializers.Count; i < n; i++) {
                if (i > 0) {
                    Out(", ");
                }
                Out(node.Initializers[i].ToString());
            }
            Out("}");
            return node;
        }

        protected internal override Expression VisitConditional(ConditionalExpression node) {
            Out("IIF(");
            Visit(node.Test);
            Out(", ");
            Visit(node.IfTrue);
            Out(", ");
            Visit(node.IfFalse);
            Out(")");
            return node;
        }

        protected internal override Expression VisitConstant(ConstantExpression node) {
            if (node.Value != null) {
                string sValue = node.Value.ToString();
                if (node.Value is string) {
                    Out("\"");
                    Out(sValue);
                    Out("\"");
                } else if (sValue == node.Value.GetType().ToString()) {
                    Out("value(");
                    Out(sValue);
                    Out(")");
                } else {
                    Out(sValue);
                }
            } else {
                Out("null");
            }
            return node;
        }

        protected internal override Expression VisitDebugInfo(DebugInfoExpression node) {
            string s = String.Format(
                CultureInfo.CurrentCulture,
                "<DebugInfo({0}: {1}, {2}, {3}, {4})>",
                node.Document.FileName,
                node.StartLine,
                node.StartColumn,
                node.EndLine,
                node.EndColumn
            );
            Out(s);
            return node;
        }

        protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
            VisitExpressions('(', node.Variables, ')');
            return node;
        }

        // Prints ".instanceField" or "declaringType.staticField"
        private void OutMember(Expression instance, MemberInfo member) {
            if (instance != null) {
                Visit(instance);
                Out("." + member.Name);
            } else {
                // For static members, include the type name
                Out(member.DeclaringType.Name + "." + member.Name);
            }
        }

        protected internal override Expression VisitMember(MemberExpression node) {
            OutMember(node.Expression, node.Member);
            return node;
        }

        protected internal override Expression VisitMemberInit(MemberInitExpression node) {
            if (node.NewExpression.Arguments.Count == 0 &&
                node.NewExpression.Type.Name.Contains("<")) {
                // anonymous type constructor
                Out("new");
            } else {
                Visit(node.NewExpression);
            }
            Out(" {");
            for (int i = 0, n = node.Bindings.Count; i < n; i++) {
                MemberBinding b = node.Bindings[i];
                if (i > 0) {
                    Out(", ");
                }
                VisitMemberBinding(b);
            }
            Out("}");
            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment) {
            Out(assignment.Member.Name);
            Out(" = ");
            Visit(assignment.Expression);
            return assignment;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding) {
            Out(binding.Member.Name);
            Out(" = {");
            for (int i = 0, n = binding.Initializers.Count; i < n; i++) {
                if (i > 0) {
                    Out(", ");
                }
                VisitElementInit(binding.Initializers[i]);
            }
            Out("}");
            return binding;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding) {
            Out(binding.Member.Name);
            Out(" = {");
            for (int i = 0, n = binding.Bindings.Count; i < n; i++) {
                if (i > 0) {
                    Out(", ");
                }
                VisitMemberBinding(binding.Bindings[i]);
            }
            Out("}");
            return binding;
        }

        protected override ElementInit VisitElementInit(ElementInit initializer) {
            Out(initializer.AddMethod.ToString());
            VisitExpressions('(', initializer.Arguments, ')');
            return initializer;
        }

        protected internal override Expression VisitInvocation(InvocationExpression node) {
            Out("Invoke(");
            Visit(node.Expression);
            for (int i = 0, n = node.Arguments.Count; i < n; i++) {
                Out(", ");
                Visit(node.Arguments[i]);
            }
            Out(")");
            return node;
        }

        protected internal override Expression VisitMethodCall(MethodCallExpression node) {
            int start = 0;
            Expression ob = node.Object;

            if (Attribute.GetCustomAttribute(node.Method, typeof(ExtensionAttribute)) != null) {
                start = 1;
                ob = node.Arguments[0];
            }

            if (ob != null) {
                Visit(ob);
                Out(".");
            }
            Out(node.Method.Name);
            Out("(");
            for (int i = start, n = node.Arguments.Count; i < n; i++) {
                if (i > start)
                    Out(", ");
                Visit(node.Arguments[i]);
            }
            Out(")");
            return node;
        }

        protected internal override Expression VisitNewArray(NewArrayExpression node) {
            switch (node.NodeType) {
                case ExpressionType.NewArrayBounds:
                    // new MyType[](expr1, expr2)
                    Out("new " + node.Type.ToString());
                    VisitExpressions('(', node.Expressions, ')');
                    break;
                case ExpressionType.NewArrayInit:
                    // new [] {expr1, expr2}
                    Out("new [] ");
                    VisitExpressions('{', node.Expressions, '}');
                    break;
            }
            return node;
        }

        protected internal override Expression VisitNew(NewExpression node) {
            Out("new " + node.Type.Name);
            Out("(");
            for (int i = 0; i < node.Arguments.Count; i++) {
                if (i > 0) {
                    Out(", ");
                }
                if (node.Members != null) {
                    Out(node.Members[i].Name);
                    Out(" = ");
                }
                Visit(node.Arguments[i]);
            }
            Out(")");
            return node;
        }

        protected internal override Expression VisitTypeBinary(TypeBinaryExpression node) {
            Out("(");
            Visit(node.Expression);
            switch (node.NodeType) {
                case ExpressionType.TypeIs:
                    Out(" Is ");
                    break;
                case ExpressionType.TypeEqual:
                    Out(" TypeEqual ");
                    break;
            }
            Out(node.TypeOperand.Name);
            Out(")");
            return node;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected internal override Expression VisitUnary(UnaryExpression node) {
            switch (node.NodeType) {
                case ExpressionType.TypeAs:
                    Out("(");
                    break;
                case ExpressionType.Not:
                    Out("Not(");
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    Out("-");
                    break;
                case ExpressionType.UnaryPlus:
                    Out("+");
                    break;
                case ExpressionType.Quote:
                    break;
                case ExpressionType.Throw:
                    Out("throw(");
                    break;
                case ExpressionType.Increment:
                    Out("Increment(");
                    break;
                case ExpressionType.Decrement:
                    Out("Decrement(");
                    break;
                case ExpressionType.PreIncrementAssign:
                    Out("++");
                    break;
                case ExpressionType.PreDecrementAssign:
                    Out("--");
                    break;
                case ExpressionType.OnesComplement:
                    Out("~(");
                    break;
                default:
                    Out(node.NodeType.ToString());
                    Out("(");
                    break;
            }

            Visit(node.Operand);

            switch (node.NodeType) {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.UnaryPlus:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.Quote:
                    break;
                case ExpressionType.TypeAs:
                    Out(" As ");
                    Out(node.Type.Name);
                    Out(")");
                    break;
                case ExpressionType.PostIncrementAssign:
                    Out("++");
                    break;
                case ExpressionType.PostDecrementAssign:
                    Out("--");
                    break;
                default:
                    Out(")");
                    break;
            }
            return node;
        }

        protected internal override Expression VisitBlock(BlockExpression node) {
            Out("{");
            foreach (var v in node.Variables) {
                Out("var ");
                Visit(v);
                Out(";");
            }
            Out(" ... }");
            return node;
        }

        protected internal override Expression VisitDefault(DefaultExpression node) {
            Out("default(");
            Out(node.Type.Name);
            Out(")");
            return node;
        }

        protected internal override Expression VisitLabel(LabelExpression node) {
            Out("{ ... } ");
            DumpLabel(node.Target);
            Out(":");
            return node;
        }

        protected internal override Expression VisitGoto(GotoExpression node) {
            Out(node.Kind.ToString().ToLower(CultureInfo.CurrentCulture));
            DumpLabel(node.Target);
            if (node.Value != null) {
                Out(" (");
                Visit(node.Value);
                Out(") ");
            }
            return node;
        }

        protected internal override Expression VisitLoop(LoopExpression node) {
            Out("loop { ... }");
            return node;
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node) {
            Out("case ");
            VisitExpressions('(', node.TestValues, ')');
            Out(": ...");
            return node;
        }

        protected internal override Expression VisitSwitch(SwitchExpression node) {
            Out("switch ");
            Out("(");
            Visit(node.SwitchValue);
            Out(") { ... }");
            return node;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node) {
            Out("catch (" + node.Test.Name);
            if (node.Variable != null) {
                Out(node.Variable.Name ?? "");
            }
            Out(") { ... }");
            return node;
        }

        protected internal override Expression VisitTry(TryExpression node) {
            Out("try { ... }");
            return node;
        }

        protected internal override Expression VisitIndex(IndexExpression node) {
            if (node.Object != null) {
                Visit(node.Object);
            } else {
                Debug.Assert(node.Indexer != null);
                Out(node.Indexer.DeclaringType.Name);
            }
            if (node.Indexer != null) {
                Out(".");
                Out(node.Indexer.Name);
            }

            VisitExpressions('[', node.Arguments, ']');
            return node;
        }

        protected internal override Expression VisitExtension(Expression node) {
            // Prefer an overriden ToString, if available.
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding;
            var toString = node.GetType().GetMethod("ToString", flags, null, Type.EmptyTypes, null);
            if (toString.DeclaringType != typeof(Expression)) {
                Out(node.ToString());
                return node;
            }

            Out("[");
            // For 3.5 subclasses, print the NodeType.
            // For Extension nodes, print the class name.
            if (node.NodeType == ExpressionType.Extension) {
                Out(node.GetType().FullName);
            } else {
                Out(node.NodeType.ToString());
            }
            Out("]");
            return node;
        }

        private void DumpLabel(LabelTarget target) {
            if (!String.IsNullOrEmpty(target.Name)) {
                Out(target.Name);
            } else {
                int labelId = GetLabelId(target);
                Out("UnamedLabel_" + labelId);
            }
        }
        #endregion
    }
}
