#define SUPPORTS_VISITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using SysExpr = System.Linq.Expressions.Expression;

using FastExpressionCompiler.LightExpression.ImTools;

namespace FastExpressionCompiler.LightExpression
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public abstract partial class Expression
    {
        #region Implicit convert
        public static implicit operator Expression(Int32 value)
		    => Constant(value);

		public static implicit operator Expression(Single value)
		    => Constant(value);

		public static implicit operator Expression(Double value)
		    => Constant(value);

		public static implicit operator Expression(Boolean value)
		    => Constant(value);

		public static implicit operator Expression(String value)
		    => Constant(value);

		public static implicit operator Expression(Char value)
		    => Constant(value);
        #endregion

        #region Operator overloading
        public static Expression operator + (Expression left, Expression right)
		    => Add(left, right);

		public static Expression operator + (Expression expr)
		    => UnaryPlus(expr);

		public static Expression operator - (Expression left, Expression right)
		    => Subtract(left, right);

		public static Expression operator - (Expression expr)
		    => Negate(expr);

		public static Expression operator * (Expression left, Expression right)
		    => Multiply(left, right);

		public static Expression operator / (Expression left, Expression right)
		    => Divide(left, right);

		public static Expression operator % (Expression left, Expression right)
		    => Modulo(left, right);

		/// <summary>
		/// Caution: use LOGICAL operator to make CONDITIONAL LOGICAL expression
		/// </summary>
		public static Expression operator & (Expression left, Expression right)
		    => AndAlso(left, right);

		/// <summary>
		/// Caution: use LOGICAL operator to make CONDITIONAL LOGICAL expression
		/// </summary>
		public static Expression operator | (Expression left, Expression right)
		    => AndAlso(left, right);

		public static Expression operator ^ (Expression left, Expression right)
		    => ExclusiveOr(left, right);

		public static Expression operator ! (Expression expr)
		    => Not(expr);

		public static Expression operator < (Expression left, Expression right)
		    => LessThan(left, right);

		public static Expression operator > (Expression left, Expression right)
		    => GreaterThan(left, right);

		public static Expression operator <= (Expression left, Expression right)
		    => LessThanOrEqual(left, right);

		public static Expression operator >= (Expression left, Expression right)
		    => GreaterThanOrEqual(left, right);

		public static Expression operator << (Expression left, Expression right)
		    => LeftShift(left, right);

		public static Expression operator >> (Expression left, Expression right)
		    => RightShift(left, right);
        #endregion

        #region Field and property
        public static MemberExpression Field(Type type, String fieldName)
            => Field(type.FindField(fieldName)
                ?? throw new ArgumentException($"Declared field with the name '{fieldName}' is not found '{type}'", nameof(fieldName)));

        public static MemberExpression Property(Type type, String propertyName)
            => Property(type.FindProperty(propertyName)
                ?? throw new ArgumentException($"Declared property with the name '{propertyName}' is not found '{type}'", nameof(propertyName)));

        public static MemberExpression PropertyOrField(Type type, string memberName)
            => type.FindProperty(memberName) != null ? Property(type, memberName) : Field(type, memberName);

		public MemberExpression this[String memberName] => PropertyOrField(this, memberName);
        #endregion

        #region Extra calls
        public static MethodCallExpression CallExtended(MethodInfo method, bool isTailcall, bool noVirtual, IReadOnlyList<Expression> arguments)
            => new ExtendedMethodCallExpression(null, method, isTailcall, noVirtual, arguments);

        public static MethodCallExpression CallExtended(Expression instance, MethodInfo method, bool isTailcall, bool noVirtual, IReadOnlyList<Expression> arguments)
            => new ExtendedMethodCallExpression(instance, method, isTailcall, noVirtual, arguments);

        public static MethodCallExpression CallExtended(Type type, string methodName, Type[] typeArguments, bool isTailcall, bool noVirtual, IReadOnlyList<Expression> arguments)
            => CallExtended(null, type.FindMethodOrThrow(methodName, typeArguments, arguments, TypeTools.StaticMethods), isTailcall, noVirtual, arguments);

        public static MethodCallExpression CallExtended(Expression instance, string methodName, Type[] typeArguments, bool isTailcall, bool noVirtual, IReadOnlyList<Expression> arguments)
            => CallExtended(instance, instance.Type.FindMethodOrThrow(methodName, typeArguments, arguments, TypeTools.InstanceMethods), isTailcall, noVirtual, arguments);
        #endregion

        public static NewExpression New(Type type, IReadOnlyList<Expression> arguments)
            => New(type.FindConstructorOrThrow(arguments), arguments);
    }
    
    public sealed class ExtendedMethodCallExpression : ManyArgumentsMethodCallExpression
    {
        private readonly bool _IsTailcall;
        private readonly bool _NoVirtual;
        public override bool IsTailcall => _IsTailcall;
        public override bool NoVirtual => _NoVirtual;
        public override Expression Object { get; }

        internal ExtendedMethodCallExpression(Expression instance, MethodInfo method, bool isTailcall, bool noVirtual, IReadOnlyList<Expression> arguments)
            : base(method, arguments)
        {
            Object = instance;
            _IsTailcall = isTailcall;
            _NoVirtual = noVirtual;
        }
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(Trimming.Message)]
    public abstract class UserDataExpression : Expression
    {
        public sealed override ExpressionType NodeType => ExpressionType.Default;
        public override Type Type => typeof(void);
#if SUPPORTS_VISITOR
        protected internal override Expression Accept(ExpressionVisitor visitor) => this;
#endif
        internal override SysExpr CreateSysExpression(ref SmallList<LightAndSysExpr> _) => SysExpr.Empty();
    }

    /// <summary>A dummy expression that can containes user data</summary>
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(Trimming.Message)]
	public sealed class UserDataExpression<T> : UserDataExpression
	{
        public T UserData { get; }
        public UserDataExpression(T userData) => UserData = userData;
	}
}
