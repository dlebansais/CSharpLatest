﻿namespace CSharpLatest;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
                createChangedDocument: c => ChangeToPrimaryConstructor(context.Document, Declaration, c),
                equivalenceKey: nameof(CodeFixResources.CSL1003CodeFixTitle)),
            Diagnostic);
    }

    private static async Task<Document> ChangeToPrimaryConstructor(Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        Document Result = document;

        // Save the leading trivia to restore it later.
        SyntaxTriviaList PreservedClassDeclarationLeadingTrivia = classDeclaration.GetLeadingTrivia();

        // Save the trailing trivia in the identifier part to restore it as trailing trivia of the parameter list.
        SyntaxToken NewIdentifier = classDeclaration.Identifier;
        SyntaxTriviaList PreservedIdentifierTrailingTrivia = NewIdentifier.TrailingTrivia;
        ClassDeclarationSyntax NewDeclaration = classDeclaration.WithIdentifier(NewIdentifier.WithoutTrivia());

        // There was no diagnostic if there is a parameter list already.
        Debug.Assert(classDeclaration.ParameterList is null);

        // Gets the list of parameters for the primary constructor, and the constructor we got them from (we know it exists or there would be no diagnostic).
        Collection<ParameterSyntax> ParameterCandidates = ConstructorAnalysis.GetParameterCandidates(classDeclaration);
        ConstructorDeclarationSyntax? ConstructorCandidate = ConstructorAnalysis.GetConstructorCandidate(classDeclaration, ParameterCandidates);
        Debug.Assert(ConstructorCandidate != null);

        // Get the list of assignments that are simplified as primary constructor arguments.
        (bool HasPropertyAssignmentsOnly, Collection<AssignmentExpressionSyntax> Assignments) = ConstructorAnalysis.GetPropertyAssignments(classDeclaration, ConstructorCandidate!);
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
                    ConvertedMember = ConstructorCodeFixes.SimplifiedConstructor(ConstructorDeclaration, ParameterCandidates, Assignments);
                }
            }
            else if (Member is PropertyDeclarationSyntax PropertyDeclaration)
            {
                // Gets the initializer if this is one of the properties to update.
                if (ConstructorCodeFixes.FindPropertyInitializer(PropertyDeclaration, Assignments) is EqualsValueClauseSyntax Initializer)
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
        var NewParameterList = SyntaxFactory.ParameterList(SeparatedParameterList).WithTrailingTrivia(PreservedIdentifierTrailingTrivia);
        NewDeclaration = NewDeclaration.WithParameterList(NewParameterList);

        // Restore the leading trivia.
        NewDeclaration = NewDeclaration.WithLeadingTrivia(PreservedClassDeclarationLeadingTrivia);

        // Add an annotation to format the new node.
        var FormattedDeclaration = NewDeclaration.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old declaration with the new declaration.
        return await document.WithReplacedNode(cancellationToken, classDeclaration, FormattedDeclaration);
    }
}
