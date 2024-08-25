namespace CSharpLatest;

using System;
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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1003CodeFixProvider)), Shared]
public class CSL1003CodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return [CSL1003ConsiderUsingPrimaryConstructor.DiagnosticId]; }
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
                title: CodeFixResources.CSL1003CodeFixTitle,
                createChangedDocument: c => ChangeToUsingPrimaryConstructor(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1003CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ChangeToUsingPrimaryConstructor(Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        SyntaxTriviaList LeadingTrivia = classDeclaration.GetLeadingTrivia();

        // Remove the trailing trivia in the identifier part.
        SyntaxToken Identifier = classDeclaration.Identifier;
        SyntaxTriviaList TrailingTrivia = Identifier.TrailingTrivia;
        ClassDeclarationSyntax NewDeclaration = classDeclaration.WithIdentifier(Identifier.WithoutTrivia());

        Debug.Assert(classDeclaration.ParameterList is null);
        List<ParameterSyntax> ParameterCandidates = CSL1003ConsiderUsingPrimaryConstructor.GetParameterCandidates(classDeclaration);
        ConstructorDeclarationSyntax? ConstructorCandidate = CSL1003ConsiderUsingPrimaryConstructor.GetConstructorCandidate(classDeclaration, ParameterCandidates);
        Debug.Assert(ConstructorCandidate != null);

        SyntaxTriviaList ConstructorLeadingTrivia = ConstructorCandidate!.GetLeadingTrivia();
        SyntaxTriviaList ConstructorTrailingTrivia = ConstructorCandidate!.GetTrailingTrivia();

        List<MemberDeclarationSyntax> NewMembers = new();
        (bool HasAssignmentsOnly, List<AssignmentExpressionSyntax> Assignments) = CSL1003ConsiderUsingPrimaryConstructor.GetPropertyAssignments(classDeclaration, ConstructorCandidate!);

        SyntaxTriviaList? LeadingTriviaPassedOver = null;

        foreach (var Member in classDeclaration.Members)
        {
            MemberDeclarationSyntax? ConvertedMember = Member;

            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
            {
                if (ConstructorDeclaration != ConstructorCandidate)
                {
                    SyntaxTriviaList OtherConstructorLeadingTrivia = ConstructorDeclaration!.GetLeadingTrivia();
                    SyntaxTriviaList OtherConstructorTrailingTrivia = ConstructorDeclaration!.GetTrailingTrivia();
                    ConstructorDeclarationSyntax NewConstructorDeclaration = ReplaceConstructor(ConstructorDeclaration, ParameterCandidates, Assignments);
                    ConvertedMember = NewConstructorDeclaration;
                }
                else
                {
                    LeadingTriviaPassedOver = Member.GetLeadingTrivia();
                    ConvertedMember = null;
                }
            }
            else if (Member is PropertyDeclarationSyntax PropertyDeclaration)
            {
                if (FindPropertyInitializer(PropertyDeclaration, Assignments) is EqualsValueClauseSyntax Initializer)
                {
                    SyntaxTriviaList PropertyTrailingTrivia = PropertyDeclaration.GetTrailingTrivia();
                    PropertyDeclarationSyntax NewPropertyDeclaration = PropertyDeclaration.WithInitializer(Initializer);
                    NewPropertyDeclaration = NewPropertyDeclaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    NewPropertyDeclaration = NewPropertyDeclaration.WithTrailingTrivia(PropertyTrailingTrivia);

                    ConvertedMember = NewPropertyDeclaration;
                }
            }

            if (ConvertedMember is not null)
            {
                if (LeadingTriviaPassedOver is not null)
                {
                    ConvertedMember = ConvertedMember.WithLeadingTrivia(LeadingTriviaPassedOver);
                    LeadingTriviaPassedOver = null;
                }

                NewMembers.Add(ConvertedMember);
            }
        }

        var MemberList = SyntaxFactory.List(NewMembers);
        NewDeclaration = NewDeclaration.WithMembers(MemberList);

        var SeparatedParameterList = SyntaxFactory.SeparatedList(ParameterCandidates);
        var ParameterList = SyntaxFactory.ParameterList(SeparatedParameterList);
        ParameterList = ParameterList.WithTrailingTrivia(TrailingTrivia);
        NewDeclaration = NewDeclaration.WithParameterList(ParameterList);
        NewDeclaration = NewDeclaration.WithLeadingTrivia(LeadingTrivia);

        ClassDeclarationSyntax FormattedDeclaration = NewDeclaration.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old expression with the new expression.
        return await document.WithReplacedNode(cancellationToken, classDeclaration, FormattedDeclaration);
    }

    private static ConstructorDeclarationSyntax ReplaceConstructor(ConstructorDeclarationSyntax constructorDeclaration, List<ParameterSyntax> thisParameters, List<AssignmentExpressionSyntax> initialAssignments)
    {
        Debug.Assert(constructorDeclaration.Initializer is null);

        var ParameterList = constructorDeclaration.ParameterList;
        SyntaxToken CloseParenToken = ParameterList.CloseParenToken;
        SyntaxTriviaList CloseParenTrailingTrivia = ParameterList.CloseParenToken.TrailingTrivia;
        var NewParameterList = ParameterList.WithoutTrailingTrivia();

        List<ArgumentSyntax> Arguments = thisParameters.ConvertAll(ToArgument);
        var SeparatedArgumentList = SyntaxFactory.SeparatedList(Arguments);
        var ArgumentList = SyntaxFactory.ArgumentList(SeparatedArgumentList);

        SyntaxToken ColonToken = SyntaxFactory.Token(SyntaxKind.ColonToken).WithLeadingTrivia(SyntaxFactory.TriviaList([SyntaxFactory.Whitespace(" ")]));
        SyntaxToken ThisKeyword = SyntaxFactory.Token(SyntaxKind.ThisKeyword).WithLeadingTrivia(SyntaxFactory.TriviaList([SyntaxFactory.Whitespace(" ")]));

        ConstructorInitializerSyntax Initializer = SyntaxFactory.ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, ColonToken, ThisKeyword, ArgumentList);

        Initializer = Initializer.WithTrailingTrivia(CloseParenTrailingTrivia);

        ConstructorDeclarationSyntax NewConstructorDeclaration = constructorDeclaration.WithInitializer(Initializer);
        NewConstructorDeclaration = NewConstructorDeclaration.WithParameterList(NewParameterList);

        if (constructorDeclaration.Body is BlockSyntax Body)
        {
            List<StatementSyntax> Statements = new(Body.Statements);

            Debug.Assert(initialAssignments.Count <= Statements.Count);

            for (int i = 0; i < initialAssignments.Count; i++)
            {
                StatementSyntax Statement = Statements[i];

                Debug.Assert(Statement is ExpressionStatementSyntax);
                ExpressionStatementSyntax ExpressionStatement = (ExpressionStatementSyntax)Statement;

                Debug.Assert(ExpressionStatement.Expression is AssignmentExpressionSyntax);
                AssignmentExpressionSyntax Assignment = (AssignmentExpressionSyntax)ExpressionStatement.Expression;

                AssignmentExpressionSyntax InitialAssignment = initialAssignments[i];
                Debug.Assert(CSL1003ConsiderUsingPrimaryConstructor.IsSyntaxNodeEquivalent(Assignment, InitialAssignment));
            }

            List<StatementSyntax> RemainingStatements = Statements.GetRange(initialAssignments.Count, Statements.Count - initialAssignments.Count);
            var NewStatementList = SyntaxFactory.List(RemainingStatements);
            var NewBody = Body.WithStatements(NewStatementList);

            NewConstructorDeclaration = NewConstructorDeclaration.WithBody(NewBody);
        }

        if (constructorDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ExpressionBody)
        {
            Debug.Assert(ExpressionBody.Expression is AssignmentExpressionSyntax);
            AssignmentExpressionSyntax Assignment = (AssignmentExpressionSyntax)ExpressionBody.Expression;

            Debug.Assert(initialAssignments.Count == 1);
            AssignmentExpressionSyntax InitialAssignment = initialAssignments[0];

            Debug.Assert(CSL1003ConsiderUsingPrimaryConstructor.IsSyntaxNodeEquivalent(Assignment, InitialAssignment));

            var NewStatementList = SyntaxFactory.List(new List<StatementSyntax>());
            var NewLineTrivia = constructorDeclaration.SemicolonToken.TrailingTrivia;
            var OpenBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithoutTrivia().WithTrailingTrivia(NewLineTrivia);
            var CloseBraceToken = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithoutTrivia().WithTrailingTrivia(NewLineTrivia);
            var NewBody = SyntaxFactory.Block(OpenBraceToken, NewStatementList, CloseBraceToken);

            NewConstructorDeclaration = NewConstructorDeclaration.WithExpressionBody(null).WithBody(NewBody).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
        }

        return NewConstructorDeclaration;
    }

    private static ArgumentSyntax ToArgument(ParameterSyntax parameter)
    {
        IdentifierNameSyntax IdentifierName = SyntaxFactory.IdentifierName(parameter.Identifier);
        ArgumentSyntax Argument = SyntaxFactory.Argument(IdentifierName);
        return Argument;
    }

    private static EqualsValueClauseSyntax? FindPropertyInitializer(PropertyDeclarationSyntax propertyDeclaration, List<AssignmentExpressionSyntax> assignments)
    {
        EqualsValueClauseSyntax? Initializer = null;

        string PropertyName = propertyDeclaration.Identifier.Text;

        if (assignments.Find(assignment => assignment.Left is IdentifierNameSyntax IdentifierName && IdentifierName.Identifier.Text  == PropertyName) is AssignmentExpressionSyntax Assignment)
        {
            ExpressionSyntax Expression = Assignment.Right;
            Initializer = SyntaxFactory.EqualsValueClause(Expression);
        }

        return Initializer;
    }
}
