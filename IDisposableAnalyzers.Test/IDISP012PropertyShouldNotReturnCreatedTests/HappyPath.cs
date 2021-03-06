namespace IDisposableAnalyzers.Test.IDISP012PropertyShouldNotReturnCreatedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ReturnValueAnalyzer();

        private const string DisposableCode = @"
namespace RoslynSandbox
{
    using System;

    public class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}";

        [Test]
        public void PropertyReturning1()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public sealed class Foo
    {
        public int Value
        {
            get
            {
                return 1;
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void PropertyReturningBoxed1()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public sealed class Foo
    {
        public object Value
        {
            get
            {
                return 1;
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void PropertyReturning1ExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public sealed class Foo
    {
        public int Value
        {
            get
            {
                return 1;
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void PropertyReturningNewTimeSpan()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo
    {
        public TimeSpan Value
        {
            get
            {
                return new TimeSpan(1);
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void PropertyReturningNewTimeSpanExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo
    {
        public TimeSpan Value => new TimeSpan(1);
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void PropertyReturningBackingFieldExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private readonly IDisposable disposable = new Disposable();

        public IDisposable Value
        {
            get { return this.disposable; }
        }

        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, DisposableCode, testCode);
        }

        [Test]
        public void PropertyReturningBackingField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private readonly IDisposable disposable = new Disposable();

        public IDisposable Value => this.disposable;

        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, DisposableCode, testCode);
        }

        [Test]
        public void PropertyReturningBackingFieldFunc()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo
    {
        private readonly Func<IDisposable> func;

        public Foo(Func<IDisposable> func)
        {
            this.func = func;
        }

        public IDisposable Disposable => this.func();
    }
}";

            AnalyzerAssert.Valid(Analyzer, DisposableCode, testCode);
        }
    }
}
