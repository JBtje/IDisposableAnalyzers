﻿namespace IDisposableAnalyzers.Test.IDISP002DisposeMemberTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class Rx
        {
            private static readonly FieldAndPropertyDeclarationAnalyzer Analyzer = new FieldAndPropertyDeclarationAnalyzer();
            private static readonly DisposeMemberCodeFixProvider CodeFix = new DisposeMemberCodeFixProvider();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("IDISP002");

            [Test]
            public void SerialDisposable()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;
    using System.Reactive.Disposables;

    public sealed class Foo : IDisposable
    {
        ↓private readonly SerialDisposable disposable = new SerialDisposable();

        public void Update()
        {
            this.disposable.Disposable = File.OpenRead(string.Empty);
        }

        public void Dispose()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;
    using System.Reactive.Disposables;

    public sealed class Foo : IDisposable
    {
        private readonly SerialDisposable disposable = new SerialDisposable();

        public void Update()
        {
            this.disposable.Disposable = File.OpenRead(string.Empty);
        }

        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void ObservableSubscribe()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        ↓private readonly IDisposable disposable;

        public Foo(IObservable<object> observable)
        {
            this.disposable = observable.Subscribe(_ => { });
        }

        public void Dispose()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class Foo : IDisposable
    {
        private readonly IDisposable disposable;

        public Foo(IObservable<object> observable)
        {
            this.disposable = observable.Subscribe(_ => { });
        }

        public void Dispose()
        {
            this.disposable?.Dispose();
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, CodeFix, ExpectedDiagnostic, testCode, fixedCode);
            }
        }
    }
}
