namespace IDisposableAnalyzers.Test.IDISP001DisposeCreatedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class HappyPath
    {
        internal class ThirdParty
        {
            [Test]
            public void NinjectStandardKernelGetIDisposable()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using Ninject;

    public class Foo
    {
        public Foo()
        {
            using (var kernel = new StandardKernel())
            {
                var disposable = kernel.Get<IDisposable>();
            }
        }
    }
}";

                AnalyzerAssert.Valid<IDISP001DisposeCreated>(testCode);
            }
        }
    }
}