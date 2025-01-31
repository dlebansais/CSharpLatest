﻿namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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

/// <summary>
/// Represents a code fix provider for CSL1004.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSL1004CodeFixProvider))]
[Shared]
public class CSL1004CodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the list of fixable diagnostics.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [CSL1004ConsiderUsingRecord.DiagnosticId];

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
        (Diagnostic Diagnostic, ClassDeclarationSyntax Declaration) = await CodeFixTools.FindNodeToFix<ClassDeclarationSyntax>(context).ConfigureAwait(false);

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
        // Save the leading and trailing trivias to restore it later.
        SyntaxTriviaList PreservedClassDeclarationLeadingTrivia = classDeclaration.GetLeadingTrivia();
        SyntaxTriviaList PreservedClassDeclarationTrailingTrivia = classDeclaration.GetTrailingTrivia();
        SyntaxTokenList NewModifiers = classDeclaration.Modifiers;

        // Save the trailing trivia in the identifier part to restore it as trailing trivia of the parameter list.
        SyntaxToken NewIdentifier = classDeclaration.Identifier;
        NewIdentifier = NewIdentifier.WithoutTrivia();

        // Replace the class keyword with the record keyword.
        SyntaxTriviaList PreservedClassKeywordTrailingTrivia = classDeclaration.Keyword.TrailingTrivia;
        SyntaxToken NewKeyword = SyntaxFactory.Token(SyntaxKind.RecordKeyword).WithoutTrivia().WithTrailingTrivia(PreservedClassKeywordTrailingTrivia);

        // There was no diagnostic if there is a parameter list already.
        Contract.Assert(classDeclaration.ParameterList is null);

        // Gets the list of parameters for the record.
        Collection<ParameterSyntax> ParameterCandidates = ConstructorAnalysis.GetParameterCandidates(classDeclaration);
        ConstructorDeclarationSyntax ConstructorCandidate = Contract.AssertNotNull(ConstructorAnalysis.GetConstructorCandidate(classDeclaration, ParameterCandidates));

        // Get the list of assignments that are simplified as record arguments.
        (bool HasPropertyAssignmentsOnly, Collection<AssignmentExpressionSyntax> Assignments) = ConstructorAnalysis.GetPropertyAssignments(classDeclaration, ConstructorCandidate);
        Contract.Assert(HasPropertyAssignmentsOnly);
        Contract.Assert(Assignments.Count > 0);

        List<MemberDeclarationSyntax> NewMembers = [];
        SyntaxTriviaList? PassedOverLeadingTrivia = null;

        foreach (MemberDeclarationSyntax Member in classDeclaration.Members)
        {
            MemberDeclarationSyntax? ConvertedMember = Member;

            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
            {
                // There can be only one constructor if the diagnostic is to suggest to use a record.
                Contract.Assert(ConstructorDeclaration == ConstructorCandidate);

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
        SyntaxList<MemberDeclarationSyntax> NewMemberList = SyntaxFactory.List(NewMembers);

        List<ParameterSyntax> PropertyParameters = Assignments.ToList().ConvertAll(assignment => ConstructorCodeFixes.ToParameter(assignment, classDeclaration));
        SeparatedSyntaxList<ParameterSyntax> SeparatedParameterList = SyntaxFactory.SeparatedList(PropertyParameters);
        ParameterListSyntax NewParameterList = SyntaxFactory.ParameterList(SeparatedParameterList);

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
            NewDeclaration = NewDeclaration.WithMembers(NewMemberList);
            NewDeclaration = NewDeclaration.WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        }

        // Restore the leading and trailing trivias.
        NewDeclaration = NewDeclaration.WithLeadingTrivia(PreservedClassDeclarationLeadingTrivia).WithTrailingTrivia(PreservedClassDeclarationTrailingTrivia);

        // Add an annotation to format the new node.
        RecordDeclarationSyntax FormattedDeclaration = NewDeclaration.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old declaration with the new declaration.
        return await document.WithReplacedNode(classDeclaration, NewDeclaration, cancellationToken).ConfigureAwait(false);
    }
}
