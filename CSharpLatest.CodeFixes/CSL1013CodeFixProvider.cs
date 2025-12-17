namespace CSharpLatest;

#if ENABLE_CSL1013
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
using Microsoft.CodeAnalysis.Formatting;
#else
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#endif

/// <summary>
/// Represents a code fix provider for CSL1013.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1013CodeFixProvider))]
[Shared]
public class CSL1013CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1013ChangeExtensionFunctionToExtensionMember.DiagnosticId];

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
                title: CodeFixResources.CSL1013CodeFixTitle,
                createChangedDocument: c => ChangeToExtensionMember(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1013CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ChangeToExtensionMember(Document document,
        MethodDeclarationSyntax declaration,
        CancellationToken cancellationToken)
    {
        Document Result = document;

#if ENABLE_CSL1013

        SeparatedSyntaxList<ParameterSyntax> Parameters = declaration.ParameterList.Parameters;
        Contract.Assert(Parameters.Count > 0);
        SeparatedSyntaxList<ParameterSyntax> NewMethodParameters = Parameters.RemoveAt(0);
        ParameterListSyntax NewMethodParameterList = declaration.ParameterList.WithParameters(NewMethodParameters);
        SyntaxTokenList Modifiers = declaration.Modifiers;
        SyntaxTokenList NewMethodModifiers = SyntaxFactory.TokenList(Modifiers.Where(m => !m.IsKind(SyntaxKind.StaticKeyword)));
        MethodDeclarationSyntax NewMethodDeclaration = declaration.WithParameterList(NewMethodParameterList)
                                                                  .WithModifiers(NewMethodModifiers);

        SyntaxList<AttributeListSyntax> EmptyAttributeList = SyntaxFactory.List<AttributeListSyntax>();
        SyntaxTokenList EmptyModifiers = SyntaxFactory.TokenList();

        ParameterSyntax FirstParameter = Parameters[0];
        SyntaxTokenList NewExtensionModifiers = SyntaxFactory.TokenList(FirstParameter.Modifiers.Where(m => !m.IsKind(SyntaxKind.ThisKeyword)));
        ParameterSyntax NewExtensionParameter = FirstParameter.WithModifiers(NewExtensionModifiers);
        ParameterListSyntax NewExtensionParameterList = SyntaxFactory.ParameterList([NewExtensionParameter]);

        SyntaxList<TypeParameterConstraintClauseSyntax> EmptyClauseList = SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();
        SyntaxList<MemberDeclarationSyntax> NewMemberList = SyntaxFactory.List<MemberDeclarationSyntax>([NewMethodDeclaration]);

        ExtensionBlockDeclarationSyntax NewExtensionDeclaration = SyntaxFactory.ExtensionBlockDeclaration(EmptyAttributeList,
                                                                                                          EmptyModifiers,
                                                                                                          SyntaxFactory.Token(SyntaxKind.ExtensionKeyword),
                                                                                                          null,
                                                                                                          NewExtensionParameterList,
                                                                                                          EmptyClauseList,
                                                                                                          SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                                                                                                          NewMemberList,
                                                                                                          SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
                                                                                                          SyntaxFactory.Token(SyntaxKind.None));
        NewExtensionDeclaration = NewExtensionDeclaration.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old declaration with the new one.
        Result = await document.WithReplacedNode(declaration, NewExtensionDeclaration, cancellationToken).ConfigureAwait(false);
        Result = await Formatter.FormatAsync(Result, cancellationToken: cancellationToken).ConfigureAwait(false);
#else
        // Don't fix anything.
#endif

        return Result;
    }
}
