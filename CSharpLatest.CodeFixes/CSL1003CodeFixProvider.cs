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
        SyntaxTriviaList? PassedOverLeadingTrivia = null;

        foreach (var Member in classDeclaration.Members)
        {
            MemberDeclarationSyntax? ConvertedMember = Member;

            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
            {
                if (ConstructorDeclaration == ConstructorCandidate)
                {
                    // Get the trivia to pass over to the next member, and ignore this constructor (it's a primary constructor now, and the syntax is different).
                    PassedOverLeadingTrivia = Member.GetLeadingTrivia();
                    ConvertedMember = null;
                }
                else
                {
                    // Get the simplified constructor.
                    ConvertedMember = SimplifiedConstructor(ConstructorDeclaration, ParameterCandidates, Assignments);
                }
            }
            else if (Member is PropertyDeclarationSyntax PropertyDeclaration)
            {
                // Gets the initializer if this is one of the properties to update.
                if (FindPropertyInitializer(PropertyDeclaration, Assignments) is EqualsValueClauseSyntax Initializer)
                {
                    // Save the trailing trivia to restore is after the initializer we add.
                    SyntaxTriviaList PreservedPropertyTrailingTrivia = PropertyDeclaration.GetTrailingTrivia();
                    PropertyDeclarationSyntax NewPropertyDeclaration = PropertyDeclaration.WithInitializer(Initializer);
                    NewPropertyDeclaration = NewPropertyDeclaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    NewPropertyDeclaration = NewPropertyDeclaration.WithTrailingTrivia(PreservedPropertyTrailingTrivia);

                    ConvertedMember = NewPropertyDeclaration;
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
        NewDeclaration = NewDeclaration.WithMembers(NewMemberList);

        // Add the primary constructor to the class.
        var SeparatedParameterList = SyntaxFactory.SeparatedList(ParameterCandidates);
        var ParameterList = SyntaxFactory.ParameterList(SeparatedParameterList).WithTrailingTrivia(PreservedIdentifierTrailingTrivia);
        NewDeclaration = NewDeclaration.WithParameterList(ParameterList);

        // Restore the leading trivia.
        NewDeclaration = NewDeclaration.WithLeadingTrivia(PreservedClassDeclarationLeadingTrivia);

        // Replace the old expression with the new expression.
        return await document.WithReplacedNode(cancellationToken, classDeclaration, NewDeclaration);
    }

    /// <summary>
    /// Creates a simplified constructor by removing assignments that are in the primary constructor.
    /// </summary>
    /// <param name="constructorDeclaration">The constructor to simplify.</param>
    /// <param name="primaryConstructorParameters">Parameters in the primary constructor.</param>
    /// <param name="initialAssignments">The list of removed assignments.</param>
    /// <returns></returns>
    private static ConstructorDeclarationSyntax SimplifiedConstructor(ConstructorDeclarationSyntax constructorDeclaration, List<ParameterSyntax> primaryConstructorParameters, List<AssignmentExpressionSyntax> initialAssignments)
    {
        Debug.Assert(constructorDeclaration.Initializer is null);

        // Save the leading and trailling trivias of the closing parenthesis, we will restore them at different places.
        // The trailing trivia is set to null later if it must not be restored.
        var PreservedLeadingTrivia = constructorDeclaration.ParameterList.CloseParenToken.LeadingTrivia;
        SyntaxTriviaList? PreservedTrailingTrivia = constructorDeclaration.ParameterList.CloseParenToken.TrailingTrivia;
        var CloseParenToken = constructorDeclaration.ParameterList.CloseParenToken.WithoutTrivia().WithLeadingTrivia(PreservedLeadingTrivia);
        var NewParameterList = constructorDeclaration.ParameterList.WithCloseParenToken(CloseParenToken);

        // Use the parameter list with removed trivias.
        ConstructorDeclarationSyntax NewConstructorDeclaration = constructorDeclaration.WithParameterList(NewParameterList);

        // In the case of a block body, we remove the first statements.
        if (constructorDeclaration.Body is BlockSyntax Body)
        {
            List<StatementSyntax> Statements = new(Body.Statements);

            // Perfom some consistency checks.
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

            // Get statements beyond those that are removed and create the new block body..
            List<StatementSyntax> RemainingStatements = Statements.GetRange(initialAssignments.Count, Statements.Count - initialAssignments.Count);
            var NewStatementList = SyntaxFactory.List(RemainingStatements);
            var NewBody = Body.WithStatements(NewStatementList);

            NewConstructorDeclaration = NewConstructorDeclaration.WithBody(NewBody);
        }

        // In the case of an expression body, we replace the only statement.
        if (constructorDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ExpressionBody)
        {
            // Perfom some consistency checks.
            Debug.Assert(ExpressionBody.Expression is AssignmentExpressionSyntax);
            AssignmentExpressionSyntax Assignment = (AssignmentExpressionSyntax)ExpressionBody.Expression;

            Debug.Assert(initialAssignments.Count == 1);
            AssignmentExpressionSyntax InitialAssignment = initialAssignments[0];
            Debug.Assert(CSL1003ConsiderUsingPrimaryConstructor.IsSyntaxNodeEquivalent(Assignment, InitialAssignment));

            // Forget the previously saved trivia and pick the one following the semicolon.
            PreservedTrailingTrivia = constructorDeclaration.SemicolonToken.TrailingTrivia;
            var OpenBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithoutTrivia(); // TODO: no WithoutTrivia()?
            var CloseBraceToken = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithoutTrivia().WithTrailingTrivia(PreservedTrailingTrivia);
            PreservedTrailingTrivia = null;

            // Create an empty block body to replace the expression body and its semicolon.
            var NewBody = SyntaxFactory.Block(OpenBraceToken, SyntaxFactory.List<StatementSyntax>(), CloseBraceToken);
            NewConstructorDeclaration = NewConstructorDeclaration.WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None)).WithBody(NewBody);
        }

        // Create the call to 'this' with assignments to parameters of the primary constructor as arguments.
        SyntaxToken ColonToken = SyntaxFactory.Token(SyntaxKind.ColonToken).WithLeadingTrivia(Whitespace());
        SyntaxToken ThisKeyword = SyntaxFactory.Token(SyntaxKind.ThisKeyword).WithLeadingTrivia(Whitespace());

        List<ArgumentSyntax> Arguments = primaryConstructorParameters.ConvertAll(ToArgument);
        var SeparatedArgumentList = SyntaxFactory.SeparatedList(Arguments);
        var ThisOpenParenToken = SyntaxFactory.Token(SyntaxKind.OpenParenToken);
        var ThisCloseParenToken = SyntaxFactory.Token(SyntaxKind.CloseParenToken);
        var ArgumentList = SyntaxFactory.ArgumentList(ThisOpenParenToken, SeparatedArgumentList, ThisCloseParenToken);
        ConstructorInitializerSyntax Initializer = SyntaxFactory.ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, ColonToken, ThisKeyword, ArgumentList);

        // Restore a previously saved trailing trivia, if any.
        if (PreservedTrailingTrivia is not null)
            Initializer = Initializer.WithTrailingTrivia(PreservedTrailingTrivia);

        NewConstructorDeclaration = NewConstructorDeclaration.WithInitializer(Initializer);

        // TODO: remove?
        var SemicolonToken = SyntaxFactory.Token(SyntaxKind.None).WithoutTrivia();
        NewConstructorDeclaration = NewConstructorDeclaration.WithSemicolonToken(SemicolonToken);

        return NewConstructorDeclaration;
    }

    /// <summary>
    /// Converts a parameter to an argument.
    /// </summary>
    /// <param name="parameter">The parameter to convert.</param>
    private static ArgumentSyntax ToArgument(ParameterSyntax parameter)
    {
        var IdentifierName = SyntaxFactory.IdentifierName(parameter.Identifier);
        return SyntaxFactory.Argument(IdentifierName);
    }

    /// <summary>
    /// Checks whether a property is a destination in a list of assignments.
    /// </summary>
    /// <param name="propertyDeclaration">The property to check.</param>
    /// <param name="assignments">The list of assignments.</param>
    /// <returns>An initializer for the property if found; otherwise, <see langword="null"/>.</returns>
    private static EqualsValueClauseSyntax? FindPropertyInitializer(PropertyDeclarationSyntax propertyDeclaration, List<AssignmentExpressionSyntax> assignments)
    {
        EqualsValueClauseSyntax? Initializer = null;
        string PropertyName = propertyDeclaration.Identifier.Text;

        if (assignments.Find(assignment => IsPropertyDestinationOfAssignment(PropertyName, assignment)) is AssignmentExpressionSyntax Assignment)
        {
            ExpressionSyntax Expression = Assignment.Right;
            Initializer = SyntaxFactory.EqualsValueClause(Expression);
        }

        return Initializer;
    }

    private static bool IsPropertyDestinationOfAssignment(string propertyName, AssignmentExpressionSyntax assignment)
    {
        // Consistency check: only assinments of this type can considered in the diagnostic.
        Debug.Assert(assignment.Left is IdentifierNameSyntax);
        IdentifierNameSyntax IdentifierName = (IdentifierNameSyntax)assignment.Left;

        return IdentifierName.Identifier.Text == propertyName;
    }

    /// <summary>
    /// Creates a simple whitespace trivia.
    /// </summary>
    private static SyntaxTriviaList Whitespace()
    {
        return SyntaxFactory.TriviaList([SyntaxFactory.Whitespace(" ")]);
    }
}
