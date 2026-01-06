namespace CSharpLatest;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code fix provider for CSL1015.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1015CodeFixProvider))]
[Shared]
public class CSL1015CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1015DoNotDeclareAsyncVoidMethods.DiagnosticId];

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
        (Diagnostic Diagnostic, MethodDeclarationSyntax Declaration) = await CodeFixTools.FindNodeToFix<MethodDeclarationSyntax>(context).ConfigureAwait(false);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1015CodeFixTitle,
                createChangedDocument: c => ChangeVoidToTask(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1015CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ChangeVoidToTask(Document document,
        MethodDeclarationSyntax declaration,
        CancellationToken cancellationToken)
    {
        // 1. Replace return type: void -> Task.
        IdentifierNameSyntax TaskType = SyntaxFactory.IdentifierName("Task").WithTriviaFrom(declaration.ReturnType);
        MethodDeclarationSyntax NewMethodDeclaration = declaration.WithReturnType(TaskType);

        document = await document.WithReplacedNode(declaration, NewMethodDeclaration, cancellationToken).ConfigureAwait(false);

        // 2. Add using System.Threading.Tasks if needed.
        CompilationUnitSyntax CompilationUnit = Contract.AssertNotNull(await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) as CompilationUnitSyntax);
        if (!HasTaskUsing(CompilationUnit))
        {
            UsingDirectiveSyntax usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks"));

            CompilationUnitSyntax NewCompilationUnit = CompilationUnit.AddUsings(usingDirective);
            document = await document.WithReplacedNode(CompilationUnit, NewCompilationUnit, cancellationToken).ConfigureAwait(false);
        }

        return document;
    }

    private static bool HasTaskUsing(CompilationUnitSyntax root)
        => root.Usings.Any(u => u.Name?.ToString() == "System.Threading.Tasks");
}
