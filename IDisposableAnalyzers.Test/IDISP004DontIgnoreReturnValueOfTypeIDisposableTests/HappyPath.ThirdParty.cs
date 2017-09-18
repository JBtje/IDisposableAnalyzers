namespace IDisposableAnalyzers.Test.IDISP004DontIgnoreReturnValueOfTypeIDisposableTests
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

        public void Bar()
        {
            Kernel.Get<IDisposable>();
        }
    }
}";

                AnalyzerAssert.Valid<IDISP004DontIgnoreReturnValueOfTypeIDisposable>(testCode);
            }
        }
    }
}