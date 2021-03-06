namespace IDisposableAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static partial class Disposable
    {
        internal static bool IsDisposedBefore(ISymbol symbol, ExpressionSyntax assignment, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (TryGetScope(assignment, out var block))
            {
                using (var walker = InvocationWalker.Borrow(block))
                {
                    foreach (var invocation in walker.Invocations)
                    {
                        if (invocation.IsExecutedBefore(assignment) != true)
                        {
                            continue;
                        }

                        if (DisposeCall.IsDisposing(invocation, symbol, semanticModel, cancellationToken))
                        {
                            return true;
                        }
                    }
                }
            }

            if (assignment is AssignmentExpressionSyntax assignmentExpression &&
                semanticModel.GetSymbolSafe(assignmentExpression.Left, cancellationToken) is IPropertySymbol property &&
                property.TryGetSetter(cancellationToken, out var setter))
            {
                using (var pooled = InvocationWalker.Borrow(setter))
                {
                    foreach (var invocation in pooled.Invocations)
                    {
                        if (DisposeCall.IsDisposing(invocation, symbol, semanticModel, cancellationToken) ||
                            DisposeCall.IsDisposing(invocation, property, semanticModel, cancellationToken))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

            bool TryGetScope(SyntaxNode node, out BlockSyntax result)
            {
                result = null;
                if (node.FirstAncestor<AnonymousFunctionExpressionSyntax>() is AnonymousFunctionExpressionSyntax lambda)
                {
                    result = lambda.Body as BlockSyntax;
                }
                else if (node.FirstAncestor<AccessorDeclarationSyntax>() is AccessorDeclarationSyntax accessor)
                {
                    result = accessor.Body;
                }
                else if (node.FirstAncestor<BaseMethodDeclarationSyntax>() is BaseMethodDeclarationSyntax method)
                {
                    result = method.Body;
                }

                return result != null;
            }
        }
    }
}
