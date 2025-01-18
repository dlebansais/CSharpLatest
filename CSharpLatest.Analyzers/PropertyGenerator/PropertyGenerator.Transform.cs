namespace CSharpLatest;

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
public partial class PropertyGenerator
{
    private static PropertyModel TransformContractAttributes(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        SyntaxNode TargetNode = context.TargetNode;
        PropertyDeclarationSyntax PropertyDeclaration = Contract.AssertOfType<PropertyDeclarationSyntax>(TargetNode);

        PropertyModel Model = GetBareboneModel(context, PropertyDeclaration);
        (string GetterText, string SetterText, string InitializerText) = GetModelText(PropertyDeclaration);
        Model = Model with { GetterText = GetterText };
        Model = Model with { SetterText = SetterText };
        Model = Model with { InitializerText = InitializerText };
        Model = Model with { Documentation = GetMemberDocumentation(PropertyDeclaration) };
        Model = Model with { GeneratedPropertyDeclaration = GetGeneratedPropertyDeclaration(Model, context) };
        Model = Model with { GeneratedFieldDeclaration = GetGeneratedFieldDeclaration(Model, PropertyDeclaration) };

        return Model;
    }

    private static PropertyModel GetBareboneModel(GeneratorAttributeSyntaxContext context, PropertyDeclarationSyntax propertyDeclaration)
    {
        INamedTypeSymbol ContainingClass = Contract.AssertNotNull(context.TargetSymbol.ContainingType);
        INamespaceSymbol ContainingNamespace = Contract.AssertNotNull(ContainingClass.ContainingNamespace);

        string Namespace = ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
        string ClassName = ContainingClass.Name;
        string? DeclarationTokens = null;
        string? FullClassName = null;

        if (propertyDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>() is ClassDeclarationSyntax ClassDeclaration)
        {
            DeclarationTokens = "class";
            FullClassName = ClassNameWithTypeParameters(ClassName, ClassDeclaration.TypeParameterList, ClassDeclaration.ConstraintClauses);
        }

        if (propertyDeclaration.FirstAncestorOrSelf<StructDeclarationSyntax>() is StructDeclarationSyntax StructDeclaration)
        {
            DeclarationTokens = "struct";
            FullClassName = ClassNameWithTypeParameters(ClassName, StructDeclaration.TypeParameterList, StructDeclaration.ConstraintClauses);
        }

        if (propertyDeclaration.FirstAncestorOrSelf<RecordDeclarationSyntax>() is RecordDeclarationSyntax RecordDeclaration)
        {
            DeclarationTokens = RecordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) ? "record struct" : "record";
            FullClassName = ClassNameWithTypeParameters(ClassName, RecordDeclaration.TypeParameterList, RecordDeclaration.ConstraintClauses);
        }

        string SymbolName = context.TargetSymbol.Name;

        return new PropertyModel(
            Namespace: Namespace,
            ClassName: ClassName,
            DeclarationTokens: Contract.AssertNotNull(DeclarationTokens),
            FullClassName: Contract.AssertNotNull(FullClassName),
            SymbolName: SymbolName,
            GetterText: string.Empty,
            SetterText: string.Empty,
            InitializerText: string.Empty,
            Documentation: string.Empty,
            GeneratedPropertyDeclaration: string.Empty,
            GeneratedFieldDeclaration: string.Empty);
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

    private static (string GetterText, string SetterText, string InitializerText) GetModelText(PropertyDeclarationSyntax propertyDeclaration)
    {
        (string GetterText, string SetterText, string InitializerText) Result = (string.Empty, string.Empty, string.Empty);
        Collection<AttributeSyntax> MemberAttributes = AttributeHelper.GetMemberSupportedAttributes(context: null, propertyDeclaration, [typeof(FieldBackedPropertyAttribute)]);

        foreach (AttributeSyntax Attribute in MemberAttributes)
            if (Attribute.ArgumentList is AttributeArgumentListSyntax AttributeArgumentList)
            {
                IReadOnlyList<AttributeArgumentSyntax> AttributeArguments = AttributeArgumentList.Arguments;
                AttributeValidityCheckResult PropertyAttributeResult = IsValidPropertyAttribute(propertyDeclaration, AttributeArguments);

                Contract.Assert(PropertyAttributeResult.Result == AttributeGeneration.Valid);
                Contract.Assert(PropertyAttributeResult.ArgumentValues.Count == 3);

                Result.GetterText = PropertyAttributeResult.ArgumentValues[0];
                Result.SetterText = PropertyAttributeResult.ArgumentValues[1];
                Result.InitializerText = PropertyAttributeResult.ArgumentValues[2];
            }

        return Result;
    }

    private static string GetMemberDocumentation(PropertyDeclarationSyntax propertyDeclaration)
    {
        string Documentation = string.Empty;

        if (propertyDeclaration.HasLeadingTrivia)
        {
            SyntaxTriviaList LeadingTrivia = propertyDeclaration.GetLeadingTrivia();

            List<SyntaxTrivia> SupportedTrivias = [];
            foreach (SyntaxTrivia trivia in LeadingTrivia)
                if (IsSupportedTrivia(trivia))
                    SupportedTrivias.Add(trivia);

            // Trim consecutive end of lines until there is only at most one at the beginning.
            bool HadEndOfLine = false;
            while (CountStartingEndOfLineTrivias(SupportedTrivias) > 1)
            {
                HadEndOfLine = true;
                SupportedTrivias.RemoveAt(0);
            }

            if (HadEndOfLine)
            {
                // Trim whitespace trivias at start.
                while (IsFirstTriviaWhitespace(SupportedTrivias))
                    SupportedTrivias.RemoveAt(0);
            }

            // Remove successive whitespace trivias.
            int i = 0;
            while (i + 1 < SupportedTrivias.Count)
                if (SupportedTrivias[i].IsKind(SyntaxKind.WhitespaceTrivia) && SupportedTrivias[i + 1].IsKind(SyntaxKind.WhitespaceTrivia))
                    SupportedTrivias.RemoveAt(i);
                else
                    i++;

            LeadingTrivia = SyntaxFactory.TriviaList(SupportedTrivias);

            foreach (SyntaxTrivia Trivia in LeadingTrivia)
                if (Trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                {
                    Documentation = LeadingTrivia.ToFullString().Trim('\r').Trim('\n').TrimEnd(' ');
                    break;
                }
        }

        return Documentation;
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

    private static int CountStartingEndOfLineTrivias(List<SyntaxTrivia> trivias)
    {
        int Count = 0;

        for (int i = 0; i < trivias.Count; i++)
        {
            SyntaxTrivia Trivia = trivias[i];

            if (Trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                Count++;
            else if (!Trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                break;
        }

        return Count;
    }

    private static string GetGeneratedFieldDeclaration(PropertyModel model, PropertyDeclarationSyntax propertyDeclaration)
    {
        if (CheckFieldKeywordSupport(propertyDeclaration))
            return string.Empty;

        SyntaxTokenList Modifiers = SyntaxFactory.TokenList([SyntaxFactory.Token(SyntaxKind.PrivateKeyword)]);

        SyntaxToken Identifier = SyntaxFactory.Identifier(Settings.FieldPrefix + model.SymbolName);
        VariableDeclaratorSyntax VariableDeclarator = SyntaxFactory.VariableDeclarator(Identifier);

        if (HasInitializer(model, out EqualsValueClauseSyntax Initializer))
            VariableDeclarator = VariableDeclarator.WithInitializer(Initializer);

        VariableDeclarationSyntax VariableDeclaration = SyntaxFactory.VariableDeclaration(propertyDeclaration.Type.WithLeadingTrivia(SyntaxFactory.Space), SyntaxFactory.SingletonSeparatedList(VariableDeclarator));
        FieldDeclarationSyntax FieldDeclaration = SyntaxFactory.FieldDeclaration([], Modifiers, VariableDeclaration);

        string Tab = new(' ', Math.Max(Settings.TabLength, 1));
        SyntaxTriviaList LeadingTrivia = GetLeadingTriviaWithTwoLineEnd(Tab);

        FieldDeclaration = FieldDeclaration.WithLeadingTrivia(LeadingTrivia);

        return FieldDeclaration.ToFullString();
    }

    private static bool HasInitializer(PropertyModel model, out EqualsValueClauseSyntax initializer)
    {
        string InitializerText = model.InitializerText;
        if (InitializerText != string.Empty)
        {
            ExpressionSyntax InitializerExpression = SyntaxFactory.ParseExpression(InitializerText);
            initializer = SyntaxFactory.EqualsValueClause(InitializerExpression.WithLeadingTrivia(SyntaxFactory.Space)).WithLeadingTrivia(SyntaxFactory.Space);
            return true;
        }

        Contract.Unused(out initializer);
        return false;
    }

    private static bool CheckFieldKeywordSupport(PropertyDeclarationSyntax propertyDeclaration)
        => ((CSharpParseOptions)propertyDeclaration.SyntaxTree.Options).LanguageVersion > LanguageVersion.CSharp13;
}
