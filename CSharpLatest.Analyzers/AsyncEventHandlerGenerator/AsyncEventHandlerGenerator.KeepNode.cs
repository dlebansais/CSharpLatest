namespace CSharpLatest.AsyncEventHandlerCodeGeneration;

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
public partial class AsyncEventHandlerGenerator
{
    private static bool KeepNodeForPipeline(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        // Only accept methods.
        if (syntaxNode is not MethodDeclarationSyntax MethodDeclaration)
            return false;

        // Ignore methods that are not in a class and a namespace.
        if (!IsInsideNamespaceAndClass(syntaxNode))
            return false;

        if (!IsValidSignature(MethodDeclaration))
            return false;

        // Ignore methods with parameters that have modifiers.
        if (IsMethodParametersWithModifier(MethodDeclaration))
            return false;

        // Because we set context to null, this check let pass attributes with the same name but from another assembly or namespace.
        // That's ok, we'll catch them later.
        return GetFirstSupportedAttribute(context: null, MethodDeclaration) is not null;
    }

    private static bool IsInsideNamespaceAndClass(SyntaxNode syntaxNode)
    {
        return (syntaxNode.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not null ||
                syntaxNode.FirstAncestorOrSelf<StructDeclarationSyntax>() is not null ||
                syntaxNode.FirstAncestorOrSelf<RecordDeclarationSyntax>() is not null) &&
                syntaxNode.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is not null;
    }

    private static bool IsValidSignature(MethodDeclarationSyntax methodDeclaration)
    {
        // Ignore methods without the async modifier.
        if (!methodDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AsyncKeyword)))
            return false;

        // Ignore methods that do not return Task.
        if (methodDeclaration.ReturnType is not IdentifierNameSyntax ReturnTypeName ||
            ReturnTypeName.Identifier.Text != "Task")
            return false;

        // Ignore methods without the Async prefix.
        return methodDeclaration.Identifier.Text.EndsWith("Async", StringComparison.Ordinal);
    }

    private static bool IsMethodParametersWithModifier(MethodDeclarationSyntax methodDeclaration)
        => methodDeclaration.ParameterList.Parameters.Any(parameter => parameter.Modifiers.Any());

    /// <summary>
    /// Checks whether a method contains at least one attribute we support and returns its name.
    /// All attributes we support must be valid.
    /// </summary>
    /// <param name="context">The analysis context. Can be <see langword="null"/> if no context is available.</param>
    /// <param name="methodDeclaration">The method declaration.</param>
    /// <returns><see langword="null"/> if any of the attributes we support is invalid, or none was found; Otherwise, the name of the first attribute.</returns>
    public static string? GetFirstSupportedAttribute(SyntaxNodeAnalysisContext? context, MethodDeclarationSyntax methodDeclaration)
    {
        Contract.RequireNotNull(methodDeclaration, out MethodDeclarationSyntax MethodDeclaration);

        // Get a list of all supported attributes for this method.
        Collection<AttributeSyntax> MethodAttributes = AttributeHelper.GetMemberSupportedAttributes(context, MethodDeclaration, [typeof(AsyncEventHandlerAttribute)]);
        List<string> AttributeNames = [];

        foreach (AttributeSyntax Attribute in MethodAttributes)
            CheckValidAttribute(Attribute, MethodDeclaration, AttributeNames);

        return AttributeNames.Count > 0 ? AttributeNames[0] : null;
    }

    private static void CheckValidAttribute(AttributeSyntax attribute, MethodDeclarationSyntax methodDeclaration, List<string> attributeNames)
    {
        string AttributeName = AttributeHelper.ToAttributeName(attribute);

        // If the attribute has arguments, validate them.
        if (attribute.ArgumentList is AttributeArgumentListSyntax AttributeArgumentList)
        {
            SeparatedSyntaxList<AttributeArgumentSyntax> AttributeArguments = AttributeArgumentList.Arguments;

            Dictionary<string, Func<MethodDeclarationSyntax, IReadOnlyList<AttributeArgumentSyntax>, AttributeValidityCheckResult>> ValidityVerifierTable = new()
            {
                { nameof(AsyncEventHandlerAttribute), IsValidMethodAttribute },
            };

            Contract.Assert(ValidityVerifierTable.ContainsKey(AttributeName));
            Func<MethodDeclarationSyntax, IReadOnlyList<AttributeArgumentSyntax>, AttributeValidityCheckResult> ValidityVerifier = ValidityVerifierTable[AttributeName];
            AttributeValidityCheckResult CheckResult = ValidityVerifier(methodDeclaration, AttributeArguments);
            AttributeGeneration AttributeGeneration = CheckResult.Result;

            if (AttributeGeneration == AttributeGeneration.Valid)
            {
                Contract.Assert(CheckResult.PositionOfFirstInvalidArgument == -1);
                attributeNames.Add(AttributeName);
            }
            else
            {
                Contract.Assert(AttributeGeneration == AttributeGeneration.Invalid);
                Contract.Assert(CheckResult.PositionOfFirstInvalidArgument >= -1);
                Contract.Assert(CheckResult.PositionOfFirstInvalidArgument < AttributeArguments.Count);
            }
        }
        else
        {
            attributeNames.Add(AttributeName);
        }
    }

    /// <summary>
    /// Checks whether a list of arguments to <see cref="AsyncEventHandlerAttribute"/> are valid.
    /// </summary>
    /// <param name="methodDeclaration">The method with the attribute.</param>
    /// <param name="attributeArguments">The list of arguments.</param>
    public static AttributeValidityCheckResult IsValidMethodAttribute(MethodDeclarationSyntax methodDeclaration, IReadOnlyList<AttributeArgumentSyntax> attributeArguments)
    {
        Contract.RequireNotNull(attributeArguments, out IReadOnlyList<AttributeArgumentSyntax> AttributeArguments);

        if (AttributeArguments.Count == 0)
            return AttributeValidityCheckResult.Invalid(-1);

        bool WaitUntilCompletion = false;
        bool UseDispatcher = false;

        for (int i = 0; i < AttributeArguments.Count; i++)
            if (!IsValidAsyncEventHandlerArgument(AttributeArguments[i], ref WaitUntilCompletion, ref UseDispatcher))
                return AttributeValidityCheckResult.Invalid(i);

        AttributeValidityCheckResult Result = new(AttributeGeneration.Valid, (WaitUntilCompletion, UseDispatcher), -1);
        return Result;
    }

    private static bool IsValidAsyncEventHandlerArgument(AttributeArgumentSyntax attributeArgument, ref bool waitUntilCompletion, ref bool useDispatcher)
    {
        if (attributeArgument.NameEquals is not NameEqualsSyntax NameEquals)
            return false;

        string ArgumentName = NameEquals.Name.Identifier.Text;

        if (!IsBoolAttributeArgument(attributeArgument, out bool ArgumentValue))
            return false;

        if (ArgumentName == nameof(AsyncEventHandlerAttribute.WaitUntilCompletion))
            waitUntilCompletion = ArgumentValue;
        else if (ArgumentName == nameof(AsyncEventHandlerAttribute.UseDispatcher))
            useDispatcher = ArgumentValue;
        else
            return false;

        return true;
    }

    /// <summary>
    /// Checks whether the value of an attribute argument is a bool.
    /// </summary>
    /// <param name="attributeArgument">The attribute argument.</param>
    /// <param name="argumentValue">The bool value upon return.</param>
    public static bool IsBoolAttributeArgument(AttributeArgumentSyntax attributeArgument, out bool argumentValue)
    {
        Contract.RequireNotNull(attributeArgument, out AttributeArgumentSyntax AttributeArgument);

        if (AttributeArgument.Expression is LiteralExpressionSyntax LiteralExpression &&
            (LiteralExpression.Kind() == SyntaxKind.TrueLiteralExpression || LiteralExpression.Kind() == SyntaxKind.FalseLiteralExpression))
        {
            string ArgumentText = LiteralExpression.Token.Text;
            argumentValue = ArgumentText == "true";
            return true;
        }

        Contract.Unused(out argumentValue);
        return false;
    }
}
