namespace CSharpLatest;

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1000CodeFixProvider)), Shared]
public class CSL1000CodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return [CSL1000VariableshouldBeMadeConstant.DiagnosticId]; }
    }

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Find the declaration identified by the diagnostic.
        var (Diagnostic, Declaration) = await CodeFixTools.FindNodeToFix<LocalDeclarationStatementSyntax>(context);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1000CodeFixTitle,
                createChangedDocument: c => MakeConstAsync(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1000CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> MakeConstAsync(Document document, LocalDeclarationStatementSyntax localDeclaration, CancellationToken cancellationToken)
    {
        // Remove the leading trivia from the local declaration.
        SyntaxToken FirstToken = localDeclaration.GetFirstToken();
        SyntaxTriviaList LeadingTrivia = FirstToken.LeadingTrivia;
        LocalDeclarationStatementSyntax TrimmedLocal = localDeclaration.ReplaceToken(FirstToken, FirstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

        // Create a const token with the leading trivia.
        SyntaxToken ConstToken = SyntaxFactory.Token(LeadingTrivia, SyntaxKind.ConstKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

        // Insert the const token into the modifiers list, creating a new modifiers list.
        SyntaxTokenList NewModifiers = TrimmedLocal.Modifiers.Insert(0, ConstToken);

        // If the type of the declaration is 'var', create a new type name
        // for the inferred type.
        VariableDeclarationSyntax VariableDeclaration = localDeclaration.Declaration;
        TypeSyntax VariableTypeName = VariableDeclaration.Type;
        if (VariableTypeName.IsVar)
        {
            SemanticModel SemanticModel = await CodeFixTools.GetSemanticModel(document, cancellationToken);

            // Special case: Ensure that 'var' isn't actually an alias to another type
            // (e.g. using var = System.String).
            IAliasSymbol? AliasInfo = SemanticModel.GetAliasInfo(VariableTypeName, cancellationToken);
            if (AliasInfo is null)
            {
                // Retrieve the type inferred for var.
                ITypeSymbol VariableType = CodeFixTools.GetVarConvertedType(SemanticModel, VariableTypeName, cancellationToken);

                // Special case: Ensure that 'var' isn't actually a type named 'var'.
                if (VariableType.Name != "var")
                {
                    // Create a new TypeSyntax for the inferred type. Be careful
                    // to keep any leading and trailing trivia from the var keyword.
                    TypeSyntax TypeName = SyntaxFactory.ParseTypeName(VariableType.ToDisplayString())
                                                       .WithLeadingTrivia(VariableTypeName.GetLeadingTrivia())
                                                       .WithTrailingTrivia(VariableTypeName.GetTrailingTrivia());

                    // Add an annotation to simplify the type name.
                    TypeSyntax SimplifiedTypeName = TypeName.WithAdditionalAnnotations(Simplifier.Annotation);

                    // Replace the type in the variable declaration.
                    VariableDeclaration = VariableDeclaration.WithType(SimplifiedTypeName);
                }
            }
        }
        
        // Produce the new local declaration.
        LocalDeclarationStatementSyntax NewLocal = TrimmedLocal.WithModifiers(NewModifiers)
                                                               .WithDeclaration(VariableDeclaration);

        // Add an annotation to format the new node.
        var FormattedLocal = NewLocal.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old local declaration with the new local declaration.
        return await document.WithReplacedNode(cancellationToken, localDeclaration, FormattedLocal);
    }
}
