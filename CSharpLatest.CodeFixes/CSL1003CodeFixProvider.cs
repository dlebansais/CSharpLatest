﻿namespace CSharpLatest;

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

        // Save the leading trivia to restore it later.
        SyntaxTriviaList PreservedClassDeclarationLeadingTrivia = classDeclaration.GetLeadingTrivia();

        // Save the trailing trivia in the identifier part to restore it as trailing trivia of the parameter list.
        SyntaxToken Identifier = classDeclaration.Identifier;
        SyntaxTriviaList PreservedIdentifierTrailingTrivia = Identifier.TrailingTrivia;
        ClassDeclarationSyntax NewDeclaration = classDeclaration.WithIdentifier(Identifier.WithoutTrivia());

        // There was no diagnostic if there is a parameter list already.
        Debug.Assert(classDeclaration.ParameterList is null);

        // Gets the list of parameters for the primary constructor, and the constructor we got them from (we know it exists or there would be no diagnostic).
        List<ParameterSyntax> ParameterCandidates = CSL1003ConsiderUsingPrimaryConstructor.GetParameterCandidates(classDeclaration);
        ConstructorDeclarationSyntax? ConstructorCandidate = CSL1003ConsiderUsingPrimaryConstructor.GetConstructorCandidate(classDeclaration, ParameterCandidates);
        Debug.Assert(ConstructorCandidate != null);

        // Get the list of assignments that are simplified as primary constructor arguments.
        (bool HasPropertyAssignmentsOnly, List<AssignmentExpressionSyntax> Assignments) = CSL1003ConsiderUsingPrimaryConstructor.GetPropertyAssignments(classDeclaration, ConstructorCandidate!);
        Debug.Assert(HasPropertyAssignmentsOnly);
        Debug.Assert(Assignments.Count > 0);

        List<MemberDeclarationSyntax> NewMembers = new();

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
        ParameterList = ParameterList.WithTrailingTrivia(PreservedIdentifierTrailingTrivia);
        NewDeclaration = NewDeclaration.WithParameterList(ParameterList);
        NewDeclaration = NewDeclaration.WithLeadingTrivia(PreservedClassDeclarationLeadingTrivia);

        // Replace the old expression with the new expression.
        return await document.WithReplacedNode(cancellationToken, classDeclaration, NewDeclaration);
    }

    private static ConstructorDeclarationSyntax ReplaceConstructor(ConstructorDeclarationSyntax constructorDeclaration, List<ParameterSyntax> thisParameters, List<AssignmentExpressionSyntax> initialAssignments)
    {
        Debug.Assert(constructorDeclaration.Initializer is null);

        ConstructorDeclarationSyntax NewConstructorDeclaration = constructorDeclaration;
        var LeadingTrivia = constructorDeclaration.ParameterList.CloseParenToken.LeadingTrivia;
        SyntaxTriviaList? TrailingTrivia = constructorDeclaration.ParameterList.CloseParenToken.TrailingTrivia;
        var CloseParenToken = constructorDeclaration.ParameterList.CloseParenToken.WithoutTrivia().WithLeadingTrivia(LeadingTrivia);
        var NewParameterList = constructorDeclaration.ParameterList.WithCloseParenToken(CloseParenToken);
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

            TrailingTrivia = constructorDeclaration.SemicolonToken.TrailingTrivia;
            var OpenBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithoutTrivia();
            var CloseBraceToken = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithoutTrivia().WithTrailingTrivia(TrailingTrivia);
            TrailingTrivia = null;

            var NewBody = SyntaxFactory.Block(OpenBraceToken, SyntaxFactory.List<StatementSyntax>(), CloseBraceToken);
            NewConstructorDeclaration = NewConstructorDeclaration.WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None)).WithBody(NewBody);
        }

        SyntaxToken ColonToken = SyntaxFactory.Token(SyntaxKind.ColonToken).WithLeadingTrivia(Whitespace());
        SyntaxToken ThisKeyword = SyntaxFactory.Token(SyntaxKind.ThisKeyword).WithLeadingTrivia(Whitespace());

        List<ArgumentSyntax> Arguments = thisParameters.ConvertAll(ToArgument);
        var SeparatedArgumentList = SyntaxFactory.SeparatedList(Arguments);
        var OpenParenToken2 = SyntaxFactory.Token(SyntaxKind.OpenParenToken);
        var CloseParenToken2 = SyntaxFactory.Token(SyntaxKind.CloseParenToken);
        var ArgumentList = SyntaxFactory.ArgumentList(OpenParenToken2, SeparatedArgumentList, CloseParenToken2);
        ConstructorInitializerSyntax Initializer = SyntaxFactory.ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, ColonToken, ThisKeyword, ArgumentList);

        if (TrailingTrivia is not null)
            Initializer = Initializer.WithTrailingTrivia(TrailingTrivia);

        NewConstructorDeclaration = NewConstructorDeclaration.WithInitializer(Initializer);

        var SemicolonToken = SyntaxFactory.Token(SyntaxKind.None).WithoutTrivia();
        NewConstructorDeclaration = NewConstructorDeclaration.WithSemicolonToken(SemicolonToken);

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

    private static SyntaxTriviaList Whitespace()
    {
        return SyntaxFactory.TriviaList([SyntaxFactory.Whitespace(" ")]);
    }
}
