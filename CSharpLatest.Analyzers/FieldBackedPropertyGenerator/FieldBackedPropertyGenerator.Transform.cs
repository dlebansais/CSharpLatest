namespace CSharpLatest.FieldBackedProperty;

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
public partial class FieldBackedPropertyGenerator
{
    private static PropertyModel TransformFieldBackedPropertyAttribute(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        SyntaxNode TargetNode = context.TargetNode;
        PropertyDeclarationSyntax PropertyDeclaration = Contract.AssertOfType<PropertyDeclarationSyntax>(TargetNode);
        string SymbolName = context.TargetSymbol.Name;

        PropertyAttributeModel TextModel = GetPropertyText(PropertyDeclaration);
        string GeneratedPropertyDeclaration = GetGeneratedPropertyDeclaration(context, SymbolName, TextModel);
        PropertyModel Model = GetBareboneModel(context, PropertyDeclaration, SymbolName, TextModel, GeneratedPropertyDeclaration);
        UpdateWithDocumentation(PropertyDeclaration, ref Model);
        UpdateWithGeneratedFieldDeclaration(PropertyDeclaration, SymbolName, TextModel, ref Model);

        return Model;
    }

    private static PropertyAttributeModel GetPropertyText(PropertyDeclarationSyntax propertyDeclaration)
    {
        Collection<AttributeSyntax> MemberAttributes = AttributeHelper.GetMemberSupportedAttributes(context: null, propertyDeclaration, [typeof(FieldBackedPropertyAttribute)]);
        AttributeValidityCheckResult? PropertyAttributeResult = null;

        foreach (AttributeSyntax Attribute in MemberAttributes)
            if (Attribute.ArgumentList is AttributeArgumentListSyntax AttributeArgumentList)
            {
                IReadOnlyList<AttributeArgumentSyntax> AttributeArguments = AttributeArgumentList.Arguments;
                PropertyAttributeResult = IsValidPropertyAttribute(propertyDeclaration, AttributeArguments);
            }

        PropertyAttributeResult = Contract.AssertNotNull(PropertyAttributeResult);
        Contract.Assert(PropertyAttributeResult.Result == AttributeGeneration.Valid);
        Contract.Assert(PropertyAttributeResult.ArgumentValues.Count == 3);

        return new PropertyAttributeModel(GetterText: PropertyAttributeResult.ArgumentValues[0],
                                     SetterText: PropertyAttributeResult.ArgumentValues[1],
                                     InitializerText: PropertyAttributeResult.ArgumentValues[2]);
    }

    private static PropertyModel GetBareboneModel(GeneratorAttributeSyntaxContext context,
                                                  PropertyDeclarationSyntax propertyDeclaration,
                                                  string symbolName,
                                                  PropertyAttributeModel propertyTextModel,
                                                  string generatedPropertyDeclaration)
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

        return new PropertyModel(
            Namespace: Namespace,
            ClassName: ClassName,
            DeclarationTokens: Contract.AssertNotNull(DeclarationTokens),
            FullClassName: Contract.AssertNotNull(FullClassName),
            SymbolName: symbolName,
            PropertyTextModel: propertyTextModel,
            Documentation: string.Empty,
            GeneratedPropertyDeclaration: generatedPropertyDeclaration,
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

    private static void UpdateWithDocumentation(PropertyDeclarationSyntax propertyDeclaration, ref PropertyModel model)
    {
        if (GeneratorHelper.HasUpdatedNodeDocumentation(propertyDeclaration, out string? documentation))
            model = model with { Documentation = documentation };
    }

    private static void UpdateWithGeneratedFieldDeclaration(PropertyDeclarationSyntax propertyDeclaration, string symbolName, PropertyAttributeModel propertyTextModel, ref PropertyModel model)
    {
        if (CheckFieldKeywordSupport(propertyDeclaration))
            return;

        SyntaxTokenList Modifiers = SyntaxFactory.TokenList([SyntaxFactory.Token(SyntaxKind.PrivateKeyword)]);

        SyntaxToken Identifier = SyntaxFactory.Identifier(Settings.FieldPrefix + symbolName);
        VariableDeclaratorSyntax VariableDeclarator = SyntaxFactory.VariableDeclarator(Identifier);

        EqualsValueClauseSyntax? Initializer = GetInitializer(propertyTextModel);
        VariableDeclarator = VariableDeclarator.WithInitializer(Initializer);

        VariableDeclarationSyntax VariableDeclaration = SyntaxFactory.VariableDeclaration(propertyDeclaration.Type.WithLeadingTrivia(SyntaxFactory.Space), SyntaxFactory.SingletonSeparatedList(VariableDeclarator));
        FieldDeclarationSyntax FieldDeclaration = SyntaxFactory.FieldDeclaration([], Modifiers, VariableDeclaration);

        string Tab = new(' ', Math.Max(Settings.TabLength, 1));
        SyntaxTriviaList LeadingTrivia = GetLeadingTriviaWithTwoLineEnd(Tab);

        FieldDeclaration = FieldDeclaration.WithLeadingTrivia(LeadingTrivia);

        model = model with { GeneratedFieldDeclaration = FieldDeclaration.ToFullString() };
    }

    private static EqualsValueClauseSyntax? GetInitializer(PropertyAttributeModel propertyTextModel)
    {
        EqualsValueClauseSyntax? Initializer = null;

        string InitializerText = propertyTextModel.InitializerText;
        if (InitializerText.Length > 0)
        {
            ExpressionSyntax InitializerExpression = SyntaxFactory.ParseExpression(InitializerText);
            Initializer = SyntaxFactory.EqualsValueClause(InitializerExpression.WithLeadingTrivia(SyntaxFactory.Space)).WithLeadingTrivia(SyntaxFactory.Space);
        }

        return Initializer;
    }

    private static bool CheckFieldKeywordSupport(PropertyDeclarationSyntax propertyDeclaration)
        => ((CSharpParseOptions)propertyDeclaration.SyntaxTree.Options).LanguageVersion > LanguageVersion.CSharp13;
}
