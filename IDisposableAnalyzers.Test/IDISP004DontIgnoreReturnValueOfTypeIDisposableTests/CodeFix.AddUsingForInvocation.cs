namespace IDisposableAnalyzers.Test.IDISP004DontIgnoreReturnValueOfTypeIDisposableTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class AddUsingForInvocation
        {
            private static readonly IDISP004DontIgnoreReturnValueOfTypeIDisposable Analyzer = new IDISP004DontIgnoreReturnValueOfTypeIDisposable();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("IDISP004");
            private static readonly AddUsingCodeFixProvider Fix = new AddUsingCodeFixProvider();

            [Test]
            public void AddUsingForIgnoredFileOpenRead()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        public void Meh()
        {
            ↓File.OpenRead(string.Empty);
            var i = 1;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        public void Meh()
        {
            using (File.OpenRead(string.Empty))
            {
                var i = 1;
            }
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void AddUsingForIgnoredReturnEmpty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        public void Meh()
        {
            ↓File.OpenRead(string.Empty);
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        public void Meh()
        {
            using (File.OpenRead(string.Empty))
            {
            }
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void AddUsingForIgnoredReturnManyStatements()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        public void Meh()
        {
            ↓File.OpenRead(string.Empty);
            var a = 1;
            var b = 2;
            if (a == b)
            {
                var c = 3;
            }

            var d = 4;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        public void Meh()
        {
            using (File.OpenRead(string.Empty))
            {
                var a = 1;
                var b = 2;
                if (a == b)
                {
                    var c = 3;
                }

                var d = 4;
            }
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void NoFixForArgument()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public class Foo
    {
        internal static string Bar()
        {
            return Meh(↓File.OpenRead(string.Empty));
        }

        private static string Meh(Stream stream) => stream.ToString();
    }
}";

                AnalyzerAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
            }
        }
    }
}
