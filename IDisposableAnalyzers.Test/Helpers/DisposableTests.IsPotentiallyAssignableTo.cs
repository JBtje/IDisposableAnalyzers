﻿namespace IDisposableAnalyzers.Test.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Moq;
    using NUnit.Framework;

    internal partial class DisposableTests
    {
        internal class IsPotentiallyAssignableTo
        {
            [TestCase("1", false)]
            [TestCase("null", false)]
            [TestCase("\"abc\"", false)]
            public void ShortCircuit(string code, bool expected)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class Foo
    {
        internal Foo()
        {
            var value = PLACEHOLDER;
        }
    }
}";
                testCode = testCode.AssertReplace("PLACEHOLDER", code);
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var value = syntaxTree.BestMatch<EqualsValueClauseSyntax>(code).Value;
                Assert.AreEqual(expected, Disposable.IsPotentiallyAssignableTo(value, new Mock<SemanticModel>(MockBehavior.Strict).Object, CancellationToken.None));
            }

            [TestCase("new string(' ', 1)", false)]
            [TestCase("new System.Text.StringBuilder()", false)]
            public void ObjectCreation(string code, bool expected)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class Foo
    {
        internal Foo()
        {
            var value = PLACEHOLDER;
        }
    }
}";
                testCode = testCode.AssertReplace("PLACEHOLDER", code);
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.BestMatch<EqualsValueClauseSyntax>(code).Value;
                Assert.AreEqual(expected, Disposable.IsPotentiallyAssignableTo(value, semanticModel, CancellationToken.None));
            }

            [Test]
            public void NewObjectInvoke()
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
            var kernel = new StandardKernel();
            var disposable = kernel.Get<IDisposable>();
        }
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var semanticModel = CSharpCompilation.Create("test", new[] {syntaxTree}, MetadataReferences.FromAttributes())
                                                     .GetSemanticModel(syntaxTree);
                var value = syntaxTree.BestMatch<EqualsValueClauseSyntax>("kernel.Get<IDisposable>()").Value;
                Assert.AreEqual(true, Disposable.IsPotentiallyAssignableTo(value, semanticModel, CancellationToken.None));
            }
        }
    }
}