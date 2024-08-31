namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1004CodeFixProvider)), Shared]
public class CSL1004CodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return [CSL1004ConsiderUsingPrimaryConstructor.DiagnosticId]; }
    }

    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Find the declaration identified by the diagnostic.
        var (Diagnostic, Declaration) = await CodeFixTools.FindNodeToFix<ClassDeclarationSyntax>(context);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.CSL1004CodeFixTitle,
                createChangedDocument: c => ChangeToRecord(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1004CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ChangeToRecord(Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        // Save the leading and trailing trivias to restore it later.
        SyntaxTriviaList PreservedClassDeclarationLeadingTrivia = classDeclaration.GetLeadingTrivia();
        SyntaxTriviaList PreservedClassDeclarationTrailingTrivia = classDeclaration.GetTrailingTrivia();
        var NewModifiers = classDeclaration.Modifiers;

        // Save the trailing trivia in the identifier part to restore it as trailing trivia of the parameter list.
        SyntaxToken NewIdentifier = classDeclaration.Identifier;
        NewIdentifier = NewIdentifier.WithoutTrivia();

        // Replace the class keyword with the record keyword.
        SyntaxTriviaList PreservedClassKeywordTrailingTrivia = classDeclaration.Keyword.TrailingTrivia;
        SyntaxToken NewKeyword = SyntaxFactory.Token(SyntaxKind.RecordKeyword).WithoutTrivia().WithTrailingTrivia(PreservedClassKeywordTrailingTrivia);

        // There was no diagnostic if there is a parameter list already.
        Debug.Assert(classDeclaration.ParameterList is null);

        // Gets the list of parameters for the record.
        List<ParameterSyntax> ParameterCandidates = ConstructorAnalysis.GetParameterCandidates(classDeclaration);
        ConstructorDeclarationSyntax? ConstructorCandidate = ConstructorAnalysis.GetConstructorCandidate(classDeclaration, ParameterCandidates);
        Debug.Assert(ConstructorCandidate != null);

        // Get the list of assignments that are simplified as record arguments.
        (bool HasPropertyAssignmentsOnly, List<AssignmentExpressionSyntax> Assignments) = ConstructorAnalysis.GetPropertyAssignments(classDeclaration, ConstructorCandidate!);
        Debug.Assert(HasPropertyAssignmentsOnly);
        Debug.Assert(Assignments.Count > 0);

        List<MemberDeclarationSyntax> NewMembers = new();
        SyntaxTriviaList? PassedOverLeadingTrivia = null;

        foreach (var Member in classDeclaration.Members)
        {
            MemberDeclarationSyntax? ConvertedMember = Member;

            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
            {
                // There can be only one constructor if the diagnostic is to suggest to use a record.
                Debug.Assert(ConstructorDeclaration == ConstructorCandidate);

                // Get the trivia to pass over to the next member, and ignore the constructor.
                PassedOverLeadingTrivia = Member.GetLeadingTrivia();
                ConvertedMember = null;
            }
            else if (Member is PropertyDeclarationSyntax PropertyDeclaration)
            {
                // Check if this is one of the properties to update.
                if (ConstructorCodeFixes.FindPropertyInitializer(PropertyDeclaration, Assignments) is not null)
                {
                    // Get the trivia to pass over to the next member, and ignore the property.
                    PassedOverLeadingTrivia = Member.GetLeadingTrivia();
                    ConvertedMember = null;
                }
            }

            if (ConvertedMember is not null)
            {
                if (PassedOverLeadingTrivia is not null)
                {
                    ConvertedMember = ConvertedMember.WithLeadingTrivia(PassedOverLeadingTrivia);
                    PassedOverLeadingTrivia = null;
                }

                NewMembers.Add(ConvertedMember);
            }
        }

        // Update the class with the modified members.
        var NewMemberList = SyntaxFactory.List(NewMembers);

        List<ParameterSyntax> PropertyParameters = Assignments.ConvertAll(assignment => ConstructorCodeFixes.ToParameter(assignment, classDeclaration));
        var SeparatedParameterList = SyntaxFactory.SeparatedList(PropertyParameters);
        var NewParameterList = SyntaxFactory.ParameterList(SeparatedParameterList);

        // Create the record.
        RecordDeclarationSyntax NewDeclaration = SyntaxFactory.RecordDeclaration(SyntaxFactory.List<AttributeListSyntax>(),
                                                                                 NewModifiers,
                                                                                 NewKeyword,
                                                                                 NewIdentifier,
                                                                                 typeParameterList: null,
                                                                                 NewParameterList,
                                                                                 baseList: null,
                                                                                 SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                                                                                 SyntaxFactory.List<MemberDeclarationSyntax>());

        // Set members.
        if (NewMemberList.Count > 0)
        {
            NewDeclaration = NewDeclaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
            NewDeclaration = NewDeclaration.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken));
            NewDeclaration = NewDeclaration.WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
            NewDeclaration = NewDeclaration.WithMembers(NewMemberList);
        }

        // Restore the leading and trailing trivias.
        NewDeclaration = NewDeclaration.WithLeadingTrivia(PreservedClassDeclarationLeadingTrivia).WithTrailingTrivia(PreservedClassDeclarationTrailingTrivia);

        // Replace the old declaration with the new declaration.
        return await document.WithReplacedNode(cancellationToken, classDeclaration, NewDeclaration);
    }
}
