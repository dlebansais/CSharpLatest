namespace CSharpLatest;

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code fix provider for CSL1011.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1011CodeFixProvider))]
[Shared]
public class CSL1011CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1011ImplementParamsCollection.DiagnosticId];

    /// <summary>
    /// Gets the fix provider.
    /// </summary>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Registers a code action to invoke a fix.
    /// </summary>
    /// <param name="context">The code fix context.</param>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Find the declaration identified by the diagnostic.
        (Diagnostic Diagnostic, ParameterSyntax Declaration) = await CodeFixTools.FindNodeToFix<ParameterSyntax>(context).ConfigureAwait(false);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1011CodeFixTitle,
                createChangedDocument: c => ReplaceWithReadOnlySpan(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1011CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ReplaceWithReadOnlySpan(Document document,
        ParameterSyntax parameter,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        MethodDeclarationSyntax MethodDeclaration = Contract.AssertNotNull(parameter.FirstAncestorOrSelf<MethodDeclarationSyntax>());
        ParameterListSyntax ParameterList = MethodDeclaration.ParameterList;
        SeparatedSyntaxList<ParameterSyntax> MethodParameters = ParameterList.Parameters;

        int ParameterIndex = MethodParameters.IndexOf(parameter);
        Contract.Assert(ParameterIndex >= 0);
        Contract.Assert(ParameterIndex < MethodParameters.Count);

        ArrayTypeSyntax ParameterType = Contract.AssertNotNull(parameter.Type as ArrayTypeSyntax);
        TypeSyntax ElementType = ParameterType.ElementType;

        SyntaxToken NewIdentifier = SyntaxFactory.Identifier("ReadOnlySpan");
        TypeArgumentListSyntax NewTypeArgumentList = SyntaxFactory.TypeArgumentList([ElementType]);
        GenericNameSyntax NewGenericName = SyntaxFactory.GenericName(NewIdentifier, NewTypeArgumentList).WithTriviaFrom(ParameterType);
        ParameterSyntax NewParameter = parameter.WithType(NewGenericName);

        // Replace the old parameter with the new one.
        Result = await document.WithReplacedNode(parameter, NewParameter, cancellationToken).ConfigureAwait(false);

        return Result;
    }
}
