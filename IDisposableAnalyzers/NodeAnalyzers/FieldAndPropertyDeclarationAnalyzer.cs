namespace IDisposableAnalyzers
{
    using System;
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class FieldAndPropertyDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            IDISP002DisposeMember.Descriptor,
            IDISP006ImplementIDisposable.Descriptor,
            IDISP008DontMixInjectedAndCreatedForMember.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleField, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(HandleProperty, SyntaxKind.PropertyDeclaration);
        }

        private static void HandleField(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is IFieldSymbol field &&
                !field.IsStatic &&
                !field.IsConst &&
                FieldOrProperty.TryCreate(field, out var fieldOrProperty) &&
                Disposable.IsPotentiallyAssignableFrom(field.Type, context.Compilation))
            {
                HandleFieldOrProperty(context, fieldOrProperty);
            }
        }

        private static void HandleProperty(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var property = (IPropertySymbol)context.ContainingSymbol;
            if (property.IsStatic ||
                property.IsIndexer)
            {
                return;
            }

            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            if (propertyDeclaration.ExpressionBody != null)
            {
                return;
            }

            if (propertyDeclaration.TryGetSetter(out var setter) &&
                setter.Body != null)
            {
                // Handle the backing field
                return;
            }

            if (FieldOrProperty.TryCreate(property, out var fieldOrProperty) &&
                Disposable.IsPotentiallyAssignableFrom(property.Type, context.Compilation))
            {
                HandleFieldOrProperty(context, fieldOrProperty);
            }
        }

        private static void HandleFieldOrProperty(SyntaxNodeAnalysisContext context, FieldOrProperty fieldOrProperty)
        {
            using (var assignedValues = AssignedValueWalker.Borrow(fieldOrProperty.Symbol, context.SemanticModel, context.CancellationToken))
            {
                using (var recursive = RecursiveValues.Borrow(assignedValues, context.SemanticModel, context.CancellationToken))
                {
                    if (Disposable.IsAnyCreation(recursive, context.SemanticModel, context.CancellationToken).IsEither(Result.Yes, Result.AssumeYes))
                    {
                        if (context.Node.TryFirstAncestorOrSelf<TypeDeclarationSyntax>(out var typeDeclaration) &&
                            DisposableMember.IsDisposed(fieldOrProperty, typeDeclaration, context.SemanticModel, context.CancellationToken).IsEither(Result.No, Result.AssumeNo) &&
                            !TestFixture.IsAssignedAndDisposedInSetupAndTearDown(fieldOrProperty, typeDeclaration, context.SemanticModel, context.CancellationToken))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(IDISP002DisposeMember.Descriptor, context.Node.GetLocation()));

                            if (!DisposeMethod.TryFindFirst(fieldOrProperty.ContainingType, context.Compilation, Search.TopLevel, out _) &&
                                !TestFixture.IsAssignedInSetUp(fieldOrProperty, typeDeclaration, context.SemanticModel, context.CancellationToken, out _))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(IDISP006ImplementIDisposable.Descriptor, context.Node.GetLocation()));
                            }
                        }

                        if (Disposable.IsAnyCachedOrInjected(recursive, context.SemanticModel, context.CancellationToken).IsEither(Result.Yes, Result.AssumeYes) ||
                            IsMutableFromOutside(fieldOrProperty))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(IDISP008DontMixInjectedAndCreatedForMember.Descriptor, context.Node.GetLocation()));
                        }
                    }
                }
            }
        }

        private static bool IsMutableFromOutside(FieldOrProperty fieldOrProperty)
        {
            if (fieldOrProperty.Symbol is IFieldSymbol field)
            {
                if (field.IsReadOnly)
                {
                    return false;
                }

                switch (field.DeclaredAccessibility)
                {
                    case Accessibility.Private:
                        return false;
                    case Accessibility.Protected:
                        return !field.ContainingType.IsSealed;
                    case Accessibility.Internal:
                    case Accessibility.ProtectedOrInternal:
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.Public:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (fieldOrProperty.Symbol is IPropertySymbol property)
            {
                if (property.SetMethod == null ||
                    property.DeclaredAccessibility == Accessibility.Private)
                {
                    return false;
                }

                switch (property.SetMethod.DeclaredAccessibility)
                {
                    case Accessibility.Private:
                        return false;
                    case Accessibility.Protected:
                        return !property.ContainingType.IsSealed;
                    case Accessibility.Internal:
                    case Accessibility.ProtectedOrInternal:
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.Public:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            throw new InvalidOperationException("Should not get here.");
        }
    }
}
