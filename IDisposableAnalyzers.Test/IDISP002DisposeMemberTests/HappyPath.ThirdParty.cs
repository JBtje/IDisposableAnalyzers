namespace IDisposableAnalyzers.Test.IDISP002DisposeMemberTests
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
        private readonly IDisposable disposable;

        public Foo()
        {
            var kernel = new StandardKernel();
            this.disposable = kernel.Get<IDisposable>();
        }
    }
}";

                AnalyzerAssert.Valid<IDISP002DisposeMember>(testCode);
            }
        }
    }
}