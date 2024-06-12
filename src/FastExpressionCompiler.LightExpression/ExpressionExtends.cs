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
		{
			return Constant(value);
		}

		public static implicit operator Expression(Single value)
		{
			return Constant(value);
		}

		public static implicit operator Expression(Double value)
		{
			return Constant(value);
		}

		public static implicit operator Expression(Boolean value)
		{
			return Constant(value);
		}

		public static implicit operator Expression(String value)
		{
			return Constant(value);
		}

		public static implicit operator Expression(Char value)
		{
			return Constant(value);
		}
        #endregion

        #region Operator overloading
        public static Expression operator + (Expression left, Expression right)
		{
			return Add(left, right);
		}

		public static Expression operator + (Expression expr)
		{
			return UnaryPlus(expr);
		}

		public static Expression operator - (Expression left, Expression right)
		{
			return Subtract(left, right);
		}

		public static Expression operator - (Expression expr)
		{
			return Negate(expr);
		}

		public static Expression operator * (Expression left, Expression right)
		{
			return Multiply(left, right);
		}

		public static Expression operator / (Expression left, Expression right)
		{
			return Divide(left, right);
		}

		public static Expression operator % (Expression left, Expression right)
		{
			return Modulo(left, right);
		}

		/// <summary>
		/// Caution: use LOGICAL operator to make CONDITIONAL LOGICAL expression
		/// </summary>
		public static Expression operator & (Expression left, Expression right)
		{
			return AndAlso(left, right);
		}

		/// <summary>
		/// Caution: use LOGICAL operator to make CONDITIONAL LOGICAL expression
		/// </summary>
		public static Expression operator | (Expression left, Expression right)
		{
			return AndAlso(left, right);
		}

		public static Expression operator ^ (Expression left, Expression right)
		{
			return ExclusiveOr(left, right);
		}

		public static Expression operator ! (Expression expr)
		{
			return Not(expr);
		}

		public static Expression operator < (Expression left, Expression right)
		{
			return LessThan(left, right);
		}

		public static Expression operator > (Expression left, Expression right)
		{
			return GreaterThan(left, right);
		}

		public static Expression operator <= (Expression left, Expression right)
		{
			return LessThanOrEqual(left, right);
		}

		public static Expression operator >= (Expression left, Expression right)
		{
			return GreaterThanOrEqual(left, right);
		}

		public static Expression operator << (Expression left, Expression right)
		{
			return LeftShift(left, right);
		}

		public static Expression operator >> (Expression left, Expression right)
		{
			return RightShift(left, right);
		}
        #endregion

        #region Field and property
        public static MemberExpression Field(Type type, String fieldName)
            => Field(type.FindField(fieldName)
                ?? throw new ArgumentException($"Declared field with the name '{fieldName}' is not found '{type}'", nameof(fieldName)));

        public static MemberExpression Property(Type type, String propertyName)
            => Property(type.FindProperty(propertyName)
                ?? throw new ArgumentException($"Declared property with the name '{propertyName}' is not found '{type}'", nameof(propertyName)));

        public static MemberExpression PropertyOrField(Type type, string memberName) =>
            type.FindProperty(memberName) != null
                ? Property(type, memberName) : Field(type, memberName);

		public MemberExpression this[String memberName] => PropertyOrField(this, memberName);
        #endregion

        #region Extra calls
        public static MethodCallExpression Tailcall(MethodInfo method, IReadOnlyList<Expression> arguments) =>
            new TailMethodCallExpression(null, method, arguments);

        public static MethodCallExpression Tailcall(Expression instance, MethodInfo method, IReadOnlyList<Expression> arguments) =>
            new TailMethodCallExpression(instance, method, arguments);

        public static MethodCallExpression Tailcall(Type type, string methodName, Type[] typeArguments, IReadOnlyList<Expression> arguments) =>
            Tailcall(null, type.FindMethodOrThrow(methodName, typeArguments, arguments, TypeTools.StaticMethods), arguments);

        public static MethodCallExpression Tailcall(Expression instance, string methodName, Type[] typeArguments, IReadOnlyList<Expression> arguments) =>
            Tailcall(instance, instance.Type.FindMethodOrThrow(methodName, typeArguments, arguments, TypeTools.InstanceMethods), arguments);
        #endregion

        public static NewExpression New(Type type, IReadOnlyList<Expression> arguments) =>
            New(type.FindConstructorOrThrow(arguments), arguments);
    }

    public sealed class TailMethodCallExpression : ManyArgumentsMethodCallExpression
    {
        public override bool IsTailcall => true;
        public override Expression Object { get; }

        internal TailMethodCallExpression(Expression instance, MethodInfo method, IReadOnlyList<Expression> arguments)
            : base(method, arguments) => Object = instance;
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
