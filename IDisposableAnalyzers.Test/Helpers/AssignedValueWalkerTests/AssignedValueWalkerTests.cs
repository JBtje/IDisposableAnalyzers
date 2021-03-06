namespace IDisposableAnalyzers.Test.Helpers.AssignedValueWalkerTests
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal partial class AssignedValueWalkerTests
    {
        [TestCase("var temp1 = this.value;", "")]
        [TestCase("var temp2 = this.value;", "1")]
        [TestCase("var temp3 = this.value;", "1")]
        public void LambdaInCtor(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System;

    internal class Foo
    {
        private int value;

        public Foo()
        {
            var temp1 = this.value;
            this.Meh += (o, e) => this.value = 1;
            var temp2 = this.value;
        }

        public event EventHandler Meh;

        internal void Bar()
        {
            var temp3 = this.value;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}