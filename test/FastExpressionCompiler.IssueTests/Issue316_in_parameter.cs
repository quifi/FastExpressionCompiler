using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;
using System;
using NUnit.Framework;

#if LIGHT_EXPRESSION
using static FastExpressionCompiler.LightExpression.Expression;
namespace FastExpressionCompiler.LightExpression.IssueTests
#else
using static System.Linq.Expressions.Expression;
namespace FastExpressionCompiler.IssueTests
#endif
{
    [TestFixture]
    public class Issue316_in_parameter : ITest
    {
        public int Run()
        {
            Test_constructor_in_struct_parameter_constant();
            Test_method_in_struct_parameter_constant();
            return 1;
        }

        public class ParseException : Exception
        {
            public ParseException(string message, in TextPosition position) : base(message)
            {
                Position = position;
            }

            public TextPosition Position { get; set; }
        }

        public struct TextPosition
        {
            public int Position;
        }

        [Test]
        public void Test_constructor_in_struct_parameter_constant()
        {
            var position = new TextPosition { Position = 42 };
            var program = Throw(New(typeof(ParseException).GetConstructors()[0], Constant("314"), Constant(position)));

            var expr = Expression.Lambda<Action>(program);
            expr.PrintCSharp(s => s.Replace(GetType().Name + ".", ""));

            var fSys = expr.CompileSys();
            fSys.PrintIL("sys");
            Assert.Throws<ParseException>(() => fSys());

            var fFast = expr.CompileFast();
            fFast.PrintIL("fast");
            Assert.Throws<ParseException>(() => fFast());
        }

        public static void ThrowParseException(in TextPosition position) => throw new ParseException("from method", in position);

        [Test]
        public void Test_method_in_struct_parameter_constant()
        {
            var position = new TextPosition { Position = 42 };
            var program = Call(GetType().GetMethod(nameof(ThrowParseException)), Constant(position));

            var expr = Expression.Lambda<Action>(program);
            expr.PrintCSharp(s => s.Replace(GetType().Name + ".", ""));

            var fSys = expr.CompileSys();
            fSys.PrintIL("sys");
            Assert.Throws<ParseException>(() => fSys());

            var fFast = expr.CompileFast();
            fFast.PrintIL("fast");
            Assert.Throws<ParseException>(() => fFast());
        }
    }
}