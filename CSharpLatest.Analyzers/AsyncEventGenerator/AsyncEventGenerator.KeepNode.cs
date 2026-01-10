namespace CSharpLatest.AsyncEventCodeGeneration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class AsyncEventGenerator
{
    private static bool KeepNodeForPipeline(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        // Only accept events.
        if (syntaxNode is not EventDeclarationSyntax EventDeclaration)
            return false;

        // Ignore events that are not in a class and a namespace.
        if (!IsInsideNamespaceAndClass(syntaxNode))
            return false;

        // Ignore events without the expected "partial event AsyncEventHandler" signature.
        return IsValidSignature(EventDeclaration);
    }

    private static bool IsInsideNamespaceAndClass(SyntaxNode syntaxNode)
    {
        return (syntaxNode.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not null ||
                syntaxNode.FirstAncestorOrSelf<StructDeclarationSyntax>() is not null ||
                syntaxNode.FirstAncestorOrSelf<RecordDeclarationSyntax>() is not null) &&
                syntaxNode.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is not null;
    }

    private static bool IsValidSignature(EventDeclarationSyntax eventDeclaration)
    {
        // Ignore events without the partial modifier.
        if (!eventDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
            return false;

        // Ignore events with an implementation.
        if (eventDeclaration.AccessorList is not null)
            return false;

        // Ignore events not using <see cref="AsyncEventHandler"/>, <see cref="AsyncEventHandler{TEventArgs}"/>, or <see cref="AsyncEventHandler{TSender, TEventArgs}"/>.
        return IsReturnTypeAsyncEventHandler(eventDeclaration);
    }

    private static bool IsReturnTypeAsyncEventHandler(EventDeclarationSyntax eventDeclaration)
    {
        if (eventDeclaration.Type is GenericNameSyntax GenericName)
        {
            string TypeName = GenericName.Identifier.Text;
            return TypeName is $"{nameof(AsyncEvent.AsyncEventHandler)}1" or $"{nameof(AsyncEvent.AsyncEventHandler)}2";
        }
        else if (eventDeclaration.Type is IdentifierNameSyntax IdentifierName)
        {
            string TypeName = IdentifierName.Identifier.Text;
            return TypeName == nameof(AsyncEvent.AsyncEventHandler);
        }

        return false;
    }
}
