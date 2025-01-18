namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynHelpers;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class PropertyGenerator
{
    private static bool KeepNodeForPipeline(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        // Only accept properties.
        if (syntaxNode is not PropertyDeclarationSyntax PropertyDeclaration)
            return false;

        // The prefix can't be empty: if invalid in user settings, it's the default prefix.
        string FieldPrefix = Settings.FieldPrefix;
        Contract.Assert(FieldPrefix != string.Empty);

        // Ignore properties that are not in a class and a namespace.
        if ((syntaxNode.FirstAncestorOrSelf<ClassDeclarationSyntax>() is null &&
             syntaxNode.FirstAncestorOrSelf<StructDeclarationSyntax>() is null &&
             syntaxNode.FirstAncestorOrSelf<RecordDeclarationSyntax>() is null) ||
            syntaxNode.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is null)
        {
            return false;
        }

        // Ignore properties without modifier (it has to be partial to work).
        if (PropertyDeclaration.Modifiers.Count == 0)
            return false;

        // Ignore properties with init accessor.
        if (PropertyDeclaration.AccessorList is AccessorListSyntax AccessorList &&
            AccessorList.Accessors.Any(accessor => accessor.Keyword.IsKind(SyntaxKind.InitKeyword)))
        {
            return false;
        }

        // Because we set context to null, this check let pass attributes with the same name but from another assembly or namespace.
        // That's ok, we'll catch them later.
        return GetFirstSupportedAttribute(context: null, PropertyDeclaration) is not null;
    }

    /// <summary>
    /// Checks whether a method or property contains at least one attribute we support and returns its name.
    /// All attributes we support must be valid.
    /// </summary>
    /// <param name="context">The analysis context. Can be <see langword="null"/> if no context is available.</param>
    /// <param name="propertyDeclaration">The member declaration.</param>
    /// <returns><see langword="null"/> if any of the attributes we support is invalid, or none was found; Otherwise, the name of the first attribute.</returns>
    public static string? GetFirstSupportedAttribute(SyntaxNodeAnalysisContext? context, MemberDeclarationSyntax propertyDeclaration)
    {
        Contract.RequireNotNull(propertyDeclaration, out MemberDeclarationSyntax PropertyDeclaration);

        // Get a list of all supported attributes for this property.
        Collection<AttributeSyntax> PropertyAttributes = AttributeHelper.GetMemberSupportedAttributes(context, PropertyDeclaration, [typeof(FieldBackedPropertyAttribute)]);
        List<string> AttributeNames = [];

        foreach (AttributeSyntax Attribute in PropertyAttributes)
            if (!IsValidAttribute(Attribute, PropertyDeclaration, AttributeNames))
                return null;

        return AttributeNames.Count > 0 ? AttributeNames[0] : null;
    }

    private static bool IsValidAttribute(AttributeSyntax attribute, MemberDeclarationSyntax propertyDeclaration, List<string> attributeNames)
    {
        if (attribute.ArgumentList is AttributeArgumentListSyntax AttributeArgumentList)
        {
            string AttributeName = AttributeHelper.ToAttributeName(attribute);
            IReadOnlyList<AttributeArgumentSyntax> AttributeArguments = AttributeArgumentList.Arguments;

            Dictionary<string, Func<MemberDeclarationSyntax, IReadOnlyList<AttributeArgumentSyntax>, AttributeValidityCheckResult>> ValidityVerifierTable = new()
            {
                { nameof(FieldBackedPropertyAttribute), IsValidPropertyAttribute },
            };

            Contract.Assert(ValidityVerifierTable.ContainsKey(AttributeName));
            Func<MemberDeclarationSyntax, IReadOnlyList<AttributeArgumentSyntax>, AttributeValidityCheckResult> ValidityVerifier = ValidityVerifierTable[AttributeName];
            AttributeValidityCheckResult CheckResult = ValidityVerifier(propertyDeclaration, AttributeArguments);
            AttributeGeneration AttributeGeneration = CheckResult.Result;

            if (AttributeGeneration == AttributeGeneration.Invalid)
            {
                return false;
            }
            else
            {
                Contract.Assert(AttributeGeneration == AttributeGeneration.Valid);
                attributeNames.Add(AttributeName);
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether a list of arguments to <see cref="FieldBackedPropertyAttribute"/> are valid.
    /// </summary>
    /// <param name="propertyDeclaration">The property with the attribute.</param>
    /// <param name="attributeArguments">The list of arguments.</param>
    public static AttributeValidityCheckResult IsValidPropertyAttribute(MemberDeclarationSyntax propertyDeclaration, IReadOnlyList<AttributeArgumentSyntax> attributeArguments)
    {
        Contract.RequireNotNull(attributeArguments, out IReadOnlyList<AttributeArgumentSyntax> AttributeArguments);

        if (AttributeArguments.Count == 0)
            return AttributeValidityCheckResult.Invalid(-1);

        string GetterText = string.Empty;
        string SetterText = string.Empty;
        string InitializerText = string.Empty;

        for (int i = 0; i < AttributeArguments.Count; i++)
            if (!IsValidGetterTextOrSetterTextArgument(AttributeArguments[i], ref GetterText, ref SetterText, ref InitializerText))
                return AttributeValidityCheckResult.Invalid(i);

        // Disallow initializer only.
        if (GetterText == string.Empty && SetterText == string.Empty)
            return AttributeValidityCheckResult.Invalid(-1);

        AttributeValidityCheckResult Result = new(AttributeGeneration.Valid, [GetterText, SetterText, InitializerText], -1);
        return Result;
    }

    private static bool IsValidGetterTextOrSetterTextArgument(AttributeArgumentSyntax attributeArgument, ref string getterText, ref string setterText, ref string initializerText)
    {
        if (attributeArgument.NameEquals is not NameEqualsSyntax NameEquals)
            return false;

        string ArgumentName = NameEquals.Name.Identifier.Text;

        if (!IsStringAttributeArgument(attributeArgument, out string ArgumentValue))
            return false;

        // Valid string attribute arguments are never empty.
        Contract.Assert(ArgumentValue != string.Empty);

        if (ArgumentName == nameof(FieldBackedPropertyAttribute.GetterText))
            getterText = ArgumentValue;
        else if (ArgumentName == nameof(FieldBackedPropertyAttribute.SetterText))
            setterText = ArgumentValue;
        else if (ArgumentName == nameof(FieldBackedPropertyAttribute.InitializerText))
            initializerText = ArgumentValue;
        else
            return false;

        return true;
    }

    /// <summary>
    /// Checks whether the value of an attribute argument is a string.
    /// </summary>
    /// <param name="attributeArgument">The attribute argument.</param>
    /// <param name="argumentValue">The string value upon return.</param>
    public static bool IsStringAttributeArgument(AttributeArgumentSyntax attributeArgument, out string argumentValue)
    {
        Contract.RequireNotNull(attributeArgument, out AttributeArgumentSyntax AttributeArgument);

        if (AttributeArgument.Expression is LiteralExpressionSyntax LiteralExpression &&
            LiteralExpression.Kind() == SyntaxKind.StringLiteralExpression)
        {
            string ArgumentText = LiteralExpression.Token.Text;
            ArgumentText = ArgumentText.Trim('"');
            if (ArgumentText != string.Empty)
            {
                argumentValue = ArgumentText;
                return true;
            }
        }

        argumentValue = string.Empty;
        return false;
    }
}
