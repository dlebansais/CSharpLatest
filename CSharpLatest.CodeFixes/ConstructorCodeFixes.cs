namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code fix constructor.
/// </summary>
public static partial class ConstructorCodeFixes
{
    /// <summary>
    /// Creates a simplified constructor by removing assignments that are in the primary constructor.
    /// </summary>
    /// <param name="constructorDeclaration">The constructor to simplify.</param>
    /// <param name="primaryConstructorParameters">Parameters in the primary constructor.</param>
    /// <param name="initialAssignments">The list of removed assignments.</param>
    [RequireNotNull(nameof(constructorDeclaration))]
    [RequireNotNull(nameof(initialAssignments))]
    private static ConstructorDeclarationSyntax SimplifiedConstructorVerified(ConstructorDeclarationSyntax constructorDeclaration, Collection<ParameterSyntax> primaryConstructorParameters, Collection<AssignmentExpressionSyntax> initialAssignments)
    {
        Contract.Assert(constructorDeclaration.Initializer is null);

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
            Contract.Assert(initialAssignments.Count <= Statements.Count);

            for (int i = 0; i < initialAssignments.Count; i++)
            {
                StatementSyntax Statement = Statements[i];
                ExpressionStatementSyntax ExpressionStatement = Contract.AssertOfType<ExpressionStatementSyntax>(Statement);
                AssignmentExpressionSyntax Assignment = Contract.AssertOfType<AssignmentExpressionSyntax>(ExpressionStatement.Expression);

                AssignmentExpressionSyntax InitialAssignment = initialAssignments[i];
                Contract.Assert(ConstructorAnalysis.IsSyntaxNodeEquivalent(Assignment, InitialAssignment));
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
            AssignmentExpressionSyntax Assignment = Contract.AssertOfType<AssignmentExpressionSyntax>(ExpressionBody.Expression);

            Contract.Assert(initialAssignments.Count == 1);
            AssignmentExpressionSyntax InitialAssignment = initialAssignments[0];
            Contract.Assert(ConstructorAnalysis.IsSyntaxNodeEquivalent(Assignment, InitialAssignment));

            // Forget the previously saved trivia and pick the one following the semicolon.
            PreservedTrailingTrivia = constructorDeclaration.SemicolonToken.TrailingTrivia;

            // Use no trivia to ensure { } formatting, because the default endofline can get wrong.
            var OpenBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithoutTrivia();
            var CloseBraceToken = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithoutTrivia().WithTrailingTrivia(PreservedTrailingTrivia);
            PreservedTrailingTrivia = null;

            // Create an empty block body to replace the expression body and its semicolon.
            var NewBody = SyntaxFactory.Block(OpenBraceToken, SyntaxFactory.List<StatementSyntax>(), CloseBraceToken);
            NewConstructorDeclaration = NewConstructorDeclaration.WithExpressionBody(null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None)).WithBody(NewBody);
        }

        // Create the call to 'this' with assignments to parameters of the primary constructor as arguments.
        SyntaxToken ColonToken = SyntaxFactory.Token(SyntaxKind.ColonToken).WithLeadingTrivia(Whitespace());
        SyntaxToken ThisKeyword = SyntaxFactory.Token(SyntaxKind.ThisKeyword).WithLeadingTrivia(Whitespace());

        List<ArgumentSyntax> Arguments = primaryConstructorParameters.ToList().ConvertAll(ToArgument);
        var SeparatedArgumentList = SyntaxFactory.SeparatedList(Arguments);
        var ThisOpenParenToken = SyntaxFactory.Token(SyntaxKind.OpenParenToken);
        var ThisCloseParenToken = SyntaxFactory.Token(SyntaxKind.CloseParenToken);
        var ArgumentList = SyntaxFactory.ArgumentList(ThisOpenParenToken, SeparatedArgumentList, ThisCloseParenToken);
        ConstructorInitializerSyntax Initializer = SyntaxFactory.ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, ColonToken, ThisKeyword, ArgumentList);

        // Restore a previously saved trailing trivia, if any.
        if (PreservedTrailingTrivia is not null)
            Initializer = Initializer.WithTrailingTrivia(PreservedTrailingTrivia);

        NewConstructorDeclaration = NewConstructorDeclaration.WithInitializer(Initializer);

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
    [RequireNotNull(nameof(propertyDeclaration))]
    private static EqualsValueClauseSyntax? FindPropertyInitializerVerified(PropertyDeclarationSyntax propertyDeclaration, Collection<AssignmentExpressionSyntax> assignments)
    {
        EqualsValueClauseSyntax? Initializer = null;
        string PropertyName = propertyDeclaration.Identifier.Text;

        if (assignments.ToList().Find(assignment => IsPropertyDestinationOfAssignment(PropertyName, assignment)) is AssignmentExpressionSyntax Assignment)
        {
            ExpressionSyntax Expression = Assignment.Right;
            Initializer = SyntaxFactory.EqualsValueClause(Expression);
        }

        return Initializer;
    }

    private static bool IsPropertyDestinationOfAssignment(string propertyName, AssignmentExpressionSyntax assignment)
    {
        // Consistency check: only assinments of this type can considered in the diagnostic.
        IdentifierNameSyntax IdentifierName = Contract.AssertOfType<IdentifierNameSyntax>(assignment.Left);

        return IdentifierName.Identifier.Text == propertyName;
    }

    /// <summary>
    /// Creates a simple whitespace trivia.
    /// </summary>
    private static SyntaxTriviaList Whitespace() => SyntaxFactory.TriviaList([SyntaxFactory.Whitespace(" ")]);

    /// <summary>
    /// Converts the destination of an assignment expression to a parameter.
    /// </summary>
    /// <param name="assignmentExpression">The assignment.</param>
    /// <param name="classDeclaration">The class containing the property.</param>
    [RequireNotNull(nameof(assignmentExpression))]
    [RequireNotNull(nameof(classDeclaration))]
    private static ParameterSyntax ToParameterVerified(AssignmentExpressionSyntax assignmentExpression, ClassDeclarationSyntax classDeclaration)
    {
        IdentifierNameSyntax Left = Contract.AssertOfType<IdentifierNameSyntax>(assignmentExpression.Left);
        SyntaxToken PropertyIdentifier = Left.Identifier.WithoutTrivia();

        PropertyDeclarationSyntax? MatchingPropertyDeclaration = null;
        foreach (MemberDeclarationSyntax MemberDeclaration in classDeclaration.Members)
            if (MemberDeclaration is PropertyDeclarationSyntax PropertyDeclaration && PropertyDeclaration.Identifier.Text == PropertyIdentifier.Text)
            {
                MatchingPropertyDeclaration = PropertyDeclaration;
                break;
            }

        MatchingPropertyDeclaration = Contract.AssertNotNull(MatchingPropertyDeclaration);
        TypeSyntax PropertyType = MatchingPropertyDeclaration.Type.WithoutTrivia();

        return SyntaxFactory.Parameter(SyntaxFactory.List<AttributeListSyntax>(), SyntaxFactory.TokenList(), PropertyType, PropertyIdentifier.WithLeadingTrivia(Whitespace()), null);
    }
}
