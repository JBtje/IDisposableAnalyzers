namespace IDisposableAnalyzers.Test.IDISP005ReturnTypeShouldIndicateIDisposableTests
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
        private static StandardKernel Kernel = new StandardKernel();

        public object Bar()
        {
            return Kernel.Get<IDisposable>();
        }
    }
}";

                AnalyzerAssert.Valid<IDISP005ReturntypeShouldIndicateIDisposable>(testCode);
            }
        }
    }
}