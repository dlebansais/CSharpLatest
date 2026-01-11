namespace CSharpLatest.AsyncEventCodeGeneration;

using System.Collections.Generic;
using System.Threading;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class AsyncEventGenerator
{
    private static EventModel TransformAsyncEventAttribute(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        SyntaxNode TargetNode = context.TargetNode;
        VariableDeclarationSyntax VariableDeclaration = Contract.AssertOfType<VariableDeclarationSyntax>(TargetNode.FirstAncestorOrSelf<VariableDeclarationSyntax>());
        EventFieldDeclarationSyntax EventDeclaration = Contract.AssertOfType<EventFieldDeclarationSyntax>(TargetNode.FirstAncestorOrSelf<EventFieldDeclarationSyntax>());
        string SymbolName = context.TargetSymbol.Name;

        bool IsReturnType = IsReturnTypeAsyncEventHandler(VariableDeclaration, out DispatcherKind DispatcherKind, out string SenderType, out string ArgumentType);
        Contract.Assert(IsReturnType, "The return type has been tested before.");

        string GeneratedEventDeclaration = GetGeneratedEventDeclaration(context, SymbolName, DispatcherKind, SenderType, ArgumentType);
        EventModel Model = GetBareboneModel(context, EventDeclaration, SymbolName, GeneratedEventDeclaration);
        UpdateWithDocumentation(EventDeclaration, ref Model);

        Model = Model with { DispatcherKind = DispatcherKind };
        return Model;
    }

    private static EventModel GetBareboneModel(GeneratorAttributeSyntaxContext context,
                                               EventFieldDeclarationSyntax eventDeclaration,
                                               string symbolName,
                                               string generatedEventDeclaration)
    {
        INamedTypeSymbol ContainingClass = Contract.AssertNotNull(context.TargetSymbol.ContainingType);
        INamespaceSymbol ContainingNamespace = Contract.AssertNotNull(ContainingClass.ContainingNamespace);

        string Namespace = ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        string ClassName = ContainingClass.Name;
        string? DeclarationTokens = null;
        string? FullClassName = null;

        if (eventDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>() is ClassDeclarationSyntax ClassDeclaration)
        {
            DeclarationTokens = "class";
            FullClassName = ClassNameWithTypeParameters(ClassName, ClassDeclaration.TypeParameterList, ClassDeclaration.ConstraintClauses);
        }

        if (eventDeclaration.FirstAncestorOrSelf<StructDeclarationSyntax>() is StructDeclarationSyntax StructDeclaration)
        {
            DeclarationTokens = "struct";
            FullClassName = ClassNameWithTypeParameters(ClassName, StructDeclaration.TypeParameterList, StructDeclaration.ConstraintClauses);
        }

        if (eventDeclaration.FirstAncestorOrSelf<RecordDeclarationSyntax>() is RecordDeclarationSyntax RecordDeclaration)
        {
            DeclarationTokens = RecordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) ? "record struct" : "record";
            FullClassName = ClassNameWithTypeParameters(ClassName, RecordDeclaration.TypeParameterList, RecordDeclaration.ConstraintClauses);
        }

        return new EventModel(
            Namespace: Namespace,
            ClassName: ClassName,
            DeclarationTokens: Contract.AssertNotNull(DeclarationTokens),
            FullClassName: Contract.AssertNotNull(FullClassName),
            SymbolName: symbolName,
            Documentation: string.Empty,
            DispatcherKind: default,
            SenderType: string.Empty,
            ArgumentType: string.Empty,
            GeneratedEventDeclaration: generatedEventDeclaration);
    }

    private static string ClassNameWithTypeParameters(string fullClassName, TypeParameterListSyntax? typeParameterList, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
    {
        string Result = fullClassName;

        if (typeParameterList is not null)
        {
            Result += typeParameterList.ToString();

            string ConstraintClausesText = constraintClauses.ToString();
            if (ConstraintClausesText != string.Empty)
                Result += " " + ConstraintClausesText;
        }

        return Result;
    }

    private static void UpdateWithDocumentation(EventFieldDeclarationSyntax eventDeclaration, ref EventModel model)
    {
        if (eventDeclaration.HasLeadingTrivia)
        {
            SyntaxTriviaList LeadingTrivia = eventDeclaration.GetLeadingTrivia();

            List<SyntaxTrivia> SupportedTrivias = [];
            foreach (SyntaxTrivia trivia in LeadingTrivia)
                if (IsSupportedTrivia(trivia))
                    SupportedTrivias.Add(trivia);

            // Trim consecutive end of lines until there is only at most one at the beginning.
            bool HadEndOfLine = false;
            while (HasStartingEndOfLineTrivias(SupportedTrivias))
            {
                int PreviousRemaining = SupportedTrivias.Count;

                HadEndOfLine = true;
                SupportedTrivias.RemoveAt(0);

                // Ensures that this while loop is not infinite.
                int Remaining = SupportedTrivias.Count;
                Contract.Assert(Remaining + 1 == PreviousRemaining);
            }

            if (HadEndOfLine)
            {
                // Trim whitespace trivias at start.
                while (IsFirstTriviaWhitespace(SupportedTrivias))
                {
                    int PreviousRemaining = SupportedTrivias.Count;

                    SupportedTrivias.RemoveAt(0);

                    // Ensures that this while loop is not infinite.
                    int Remaining = SupportedTrivias.Count;
                    Contract.Assert(Remaining + 1 == PreviousRemaining);
                }
            }

            // Remove successive whitespace trivias.
            int i = 0;
            while (i + 1 < SupportedTrivias.Count)
            {
                int PreviousRemaining = SupportedTrivias.Count - i;

                if (SupportedTrivias[i].IsKind(SyntaxKind.WhitespaceTrivia) && SupportedTrivias[i + 1].IsKind(SyntaxKind.WhitespaceTrivia))
                    SupportedTrivias.RemoveAt(i);
                else
                    i++;

                int Remaining = SupportedTrivias.Count - i;

                // Ensures that this while loop is not infinite.
                Contract.Assert(Remaining + 1 == PreviousRemaining);
            }

            LeadingTrivia = SyntaxFactory.TriviaList(SupportedTrivias);

            if (LeadingTrivia.Any(SyntaxKind.SingleLineDocumentationCommentTrivia))
                model = model with { Documentation = LeadingTrivia.ToFullString().Trim('\r').Trim('\n').TrimEnd(' ') };
        }
    }

    private static bool IsSupportedTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.EndOfLineTrivia) ||
               trivia.IsKind(SyntaxKind.WhitespaceTrivia) ||
               trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
               trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    private static bool IsFirstTriviaWhitespace(List<SyntaxTrivia> trivias)
    {
        // If we reach this event there is at least one end of line, therefore at least one trivia.
        Contract.Assert(trivias.Count > 0);

        SyntaxTrivia FirstTrivia = trivias[0];

        return FirstTrivia.IsKind(SyntaxKind.WhitespaceTrivia);
    }

    private static bool HasStartingEndOfLineTrivias(List<SyntaxTrivia> trivias)
    {
        int Count = 0;

        for (int i = 0; i < trivias.Count; i++)
        {
            SyntaxTrivia Trivia = trivias[i];

            if (Trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                Count++;

                if (Count > 1)
                    return true;
            }
            else if (!Trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return false;
            }
        }

        return false;
    }
}
