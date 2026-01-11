namespace CSharpLatest.AsyncEventCodeGeneration;

using System.Linq;
using System.Threading;
using Contracts;
using CSharpLatest.Events;
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
        if (syntaxNode is not VariableDeclaratorSyntax VariableDeclarator)
            return false;

        VariableDeclarationSyntax VariableDeclaration = Contract.AssertNotNull(VariableDeclarator.FirstAncestorOrSelf<VariableDeclarationSyntax>());

        // Only accept events without accessors.
        if (VariableDeclaration.Parent is not EventFieldDeclarationSyntax EventDeclaration)
            return false;

        // Ignore events that are not in a class and a namespace.
        if (!IsInsideNamespaceAndClass(syntaxNode))
            return false;

        // Ignore events without the expected "partial event AsyncEventHandler" signature.
        return IsValidSignature(EventDeclaration, VariableDeclaration);
    }

    private static bool IsInsideNamespaceAndClass(SyntaxNode syntaxNode)
    {
        return (syntaxNode.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not null ||
                syntaxNode.FirstAncestorOrSelf<StructDeclarationSyntax>() is not null ||
                syntaxNode.FirstAncestorOrSelf<RecordDeclarationSyntax>() is not null) &&
                syntaxNode.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is not null;
    }

    private static bool IsValidSignature(EventFieldDeclarationSyntax eventDeclaration, VariableDeclarationSyntax variableDeclaration)
    {
        // Ignore events without the partial modifier.
        if (!eventDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
            return false;

        // Ignore events not using <see cref="AsyncEventHandler"/>, <see cref="AsyncEventHandler{TEventArgs}"/>, or <see cref="AsyncEventHandler{TSender, TEventArgs}"/>.
        return IsReturnTypeAsyncEventHandler(variableDeclaration, out _, out _, out _);
    }

    private static bool IsReturnTypeAsyncEventHandler(VariableDeclarationSyntax variableDeclaration, out DispatcherKind dispatcherKind, out string senderType, out string argumentType)
    {
        if (variableDeclaration.Type is GenericNameSyntax GenericName)
        {
            string TypeName = GenericName.Identifier.Text;
            if (TypeName == nameof(AsyncEventHandler))
            {
                if (GenericName.TypeArgumentList.Arguments.Count == 1)
                {
                    senderType = string.Empty;
                    argumentType = GenericName.TypeArgumentList.Arguments[0].ToString();
                    dispatcherKind = DispatcherKind.WithEventArgs;
                    return true;
                }
                else if (GenericName.TypeArgumentList.Arguments.Count == 2)
                {
                    senderType = GenericName.TypeArgumentList.Arguments[0].ToString();
                    argumentType = GenericName.TypeArgumentList.Arguments[1].ToString();
                    dispatcherKind = DispatcherKind.WithSenderAndEventArgs;
                    return true;
                }
            }
        }
        else if (variableDeclaration.Type is IdentifierNameSyntax IdentifierName)
        {
            string TypeName = IdentifierName.Identifier.Text;
            if (TypeName == nameof(AsyncEventHandler))
            {
                senderType = string.Empty;
                argumentType = string.Empty;
                dispatcherKind = DispatcherKind.Simple;
                return true;
            }
        }

        dispatcherKind = default;
        senderType = string.Empty;
        argumentType = string.Empty;
        return false;
    }
}
