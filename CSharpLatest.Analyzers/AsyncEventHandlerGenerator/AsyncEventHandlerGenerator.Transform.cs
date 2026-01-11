namespace CSharpLatest.AsyncEventHandlerCodeGeneration;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynHelpers;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class AsyncEventHandlerGenerator
{
    private static MethodModel TransformAsyncEventHandlerAttribute(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        SyntaxNode TargetNode = context.TargetNode;
        MethodDeclarationSyntax MethodDeclaration = Contract.AssertOfType<MethodDeclarationSyntax>(TargetNode);
        string SymbolName = context.TargetSymbol.Name;

        Contract.Assert(SymbolName.EndsWith("Async", StringComparison.Ordinal));
        SymbolName = SymbolName[..^"Async".Length];

        MethodAttributeModel MethodAttributeModel = GetMethodAttribute(MethodDeclaration);
        string GeneratedMethodDeclaration = GetGeneratedMethodDeclaration(context, SymbolName, MethodAttributeModel);
        MethodModel Model = GetBareboneModel(context, MethodDeclaration, SymbolName, MethodAttributeModel, GeneratedMethodDeclaration);
        UpdateWithDocumentation(MethodDeclaration, ref Model);
        UpdateUsings(context, ref Model);

        return Model;
    }

    private static MethodAttributeModel GetMethodAttribute(MethodDeclarationSyntax methodDeclaration)
    {
        Collection<AttributeSyntax> MemberAttributes = AttributeHelper.GetMemberSupportedAttributes(context: null, methodDeclaration, [typeof(AsyncEventHandlerAttribute)]);
        AttributeValidityCheckResult? MethodAttributeResult = null;

        foreach (AttributeSyntax Attribute in MemberAttributes)
            if (Attribute.ArgumentList is AttributeArgumentListSyntax AttributeArgumentList)
            {
                IReadOnlyList<AttributeArgumentSyntax> AttributeArguments = AttributeArgumentList.Arguments;
                MethodAttributeResult = IsValidMethodAttribute(methodDeclaration, AttributeArguments);
            }
            else
            {
                MethodAttributeResult = new(AttributeGeneration.Valid, (false, false), -1);
            }

        MethodAttributeResult = Contract.AssertNotNull(MethodAttributeResult);
        Contract.Assert(MethodAttributeResult.Result == AttributeGeneration.Valid);

        return new MethodAttributeModel(WaitUntilCompletion: MethodAttributeResult.ArgumentValues.Item1,
                                        UseDispatcher: MethodAttributeResult.ArgumentValues.Item2);
    }

    private static MethodModel GetBareboneModel(GeneratorAttributeSyntaxContext context,
                                                  MethodDeclarationSyntax methodDeclaration,
                                                  string symbolName,
                                                  MethodAttributeModel methodAttributeModel,
                                                  string generatedMethodDeclaration)
    {
        INamedTypeSymbol ContainingClass = Contract.AssertNotNull(context.TargetSymbol.ContainingType);
        INamespaceSymbol ContainingNamespace = Contract.AssertNotNull(ContainingClass.ContainingNamespace);

        string Namespace = ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        string ClassName = ContainingClass.Name;
        string? DeclarationTokens = null;
        string? FullClassName = null;

        if (methodDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>() is ClassDeclarationSyntax ClassDeclaration)
        {
            DeclarationTokens = "class";
            FullClassName = ClassNameWithTypeParameters(ClassName, ClassDeclaration.TypeParameterList, ClassDeclaration.ConstraintClauses);
        }

        if (methodDeclaration.FirstAncestorOrSelf<StructDeclarationSyntax>() is StructDeclarationSyntax StructDeclaration)
        {
            DeclarationTokens = "struct";
            FullClassName = ClassNameWithTypeParameters(ClassName, StructDeclaration.TypeParameterList, StructDeclaration.ConstraintClauses);
        }

        if (methodDeclaration.FirstAncestorOrSelf<RecordDeclarationSyntax>() is RecordDeclarationSyntax RecordDeclaration)
        {
            DeclarationTokens = RecordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) ? "record struct" : "record";
            FullClassName = ClassNameWithTypeParameters(ClassName, RecordDeclaration.TypeParameterList, RecordDeclaration.ConstraintClauses);
        }

        return new MethodModel(
            Namespace: Namespace,
            UsingsBeforeNamespace: string.Empty,
            UsingsAfterNamespace: string.Empty,
            ClassName: ClassName,
            DeclarationTokens: Contract.AssertNotNull(DeclarationTokens),
            FullClassName: Contract.AssertNotNull(FullClassName),
            SymbolName: symbolName,
            MethodAttributeModel: methodAttributeModel,
            Documentation: string.Empty,
            GeneratedMethodDeclaration: generatedMethodDeclaration);
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

    private static void UpdateWithDocumentation(MethodDeclarationSyntax methodDeclaration, ref MethodModel model)
    {
        if (methodDeclaration.HasLeadingTrivia)
        {
            SyntaxTriviaList LeadingTrivia = methodDeclaration.GetLeadingTrivia();

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
        // If we reach this method there is at least one end of line, therefore at least one trivia.
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

    private static void UpdateUsings(GeneratorAttributeSyntaxContext context, ref MethodModel model)
    {
        // We know it's not null from KeepNodeForPipeline().
        BaseNamespaceDeclarationSyntax BaseNamespaceDeclaration = Contract.AssertNotNull(context.TargetNode.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>());

        if (BaseNamespaceDeclaration.Usings.Count > 0)
        {
            string UsingString = BaseNamespaceDeclaration.Usings.ToFullString();
            Contract.Assert(UsingString.Length > 0);

            model = model with { UsingsAfterNamespace = UsingString };
        }

        if (BaseNamespaceDeclaration.Parent is CompilationUnitSyntax CompilationUnit)
            model = model with { UsingsBeforeNamespace = CompilationUnit.Usings.ToFullString() };

        AddMissingUsingsAndSort(ref model);
    }

    private static void AddMissingUsingsAndSort(ref MethodModel model)
    {
        model = model with { UsingsBeforeNamespace = GeneratorHelper.SortUsings(model.UsingsBeforeNamespace) };
        model = model with { UsingsAfterNamespace = GeneratorHelper.SortUsings(model.UsingsAfterNamespace) };
        bool UseGlobal = GeneratorHelper.HasGlobalSystem(model.UsingsAfterNamespace);

        AddMissingUsing(ref model, "CSharpLatest", isGlobal: false);
        AddMissingUsing(ref model, "System.CodeDom.Compiler", isGlobal: UseGlobal);
        AddMissingUsing(ref model, "System.Diagnostics", isGlobal: UseGlobal);

        model = model with { UsingsAfterNamespace = GeneratorHelper.SortUsings(model.UsingsAfterNamespace) };
    }

    private static void AddMissingUsing(ref MethodModel model, string usingDirective, bool isGlobal)
    {
        string GlobalDirective = $"using global::{usingDirective};\n";
        string NonGlobalDirective = $"using {usingDirective};\n";
        bool IsDirectiveBeforeNamespace;
        bool IsDirectiveAfterNamespace;

#if NETSTANDARD2_1_OR_GREATER
        IsDirectiveBeforeNamespace = model.UsingsBeforeNamespace.Contains(GlobalDirective, StringComparison.Ordinal) || model.UsingsBeforeNamespace.Contains(NonGlobalDirective, StringComparison.Ordinal);
        IsDirectiveAfterNamespace = model.UsingsAfterNamespace.Contains(GlobalDirective, StringComparison.Ordinal) || model.UsingsAfterNamespace.Contains(NonGlobalDirective, StringComparison.Ordinal);
#else
        IsDirectiveBeforeNamespace = model.UsingsBeforeNamespace.Contains(GlobalDirective) || model.UsingsBeforeNamespace.Contains(NonGlobalDirective);
        IsDirectiveAfterNamespace = model.UsingsAfterNamespace.Contains(GlobalDirective) || model.UsingsAfterNamespace.Contains(NonGlobalDirective);
#endif

        if (!IsDirectiveBeforeNamespace && !IsDirectiveAfterNamespace)
        {
            string LineToAdd = isGlobal ? GlobalDirective : NonGlobalDirective;
            Contract.Assert(!model.UsingsAfterNamespace.Contains(LineToAdd));

            model = model with { UsingsAfterNamespace = model.UsingsAfterNamespace + LineToAdd };
        }
    }
}
