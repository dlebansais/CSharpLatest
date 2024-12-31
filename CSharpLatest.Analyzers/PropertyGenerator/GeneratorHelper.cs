namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Helper class for the code generator.
/// </summary>
internal static class GeneratorHelper
{
    /// <summary>
    /// Gets all supported attributes of a method or property.
    /// </summary>
    /// <param name="context">The analysis context. Can be <see langword="null"/> if no context is available.</param>
    /// <param name="memberDeclaration">The method or property.</param>
    /// <param name="supportedAttributeTypes">The list of supported attributes.</param>
    public static List<AttributeSyntax> GetMemberSupportedAttributes(SyntaxNodeAnalysisContext? context, MemberDeclarationSyntax memberDeclaration, Collection<Type> supportedAttributeTypes)
    {
        List<AttributeSyntax> Result = [];

        for (int IndexList = 0; IndexList < memberDeclaration.AttributeLists.Count; IndexList++)
        {
            AttributeListSyntax AttributeList = memberDeclaration.AttributeLists[IndexList];

            for (int Index = 0; Index < AttributeList.Attributes.Count; Index++)
            {
                AttributeSyntax Attribute = AttributeList.Attributes[Index];
                bool IsSameNamespaceAssembly = true;

                if (context is SyntaxNodeAnalysisContext AvailableContext)
                {
                    SymbolInfo SymbolInfo = AvailableContext.SemanticModel.GetSymbolInfo(Attribute);
                    if (SymbolInfo.Symbol is ISymbol AttributeSymbol)
                    {
                        ITypeSymbol AccessTypeSymbol = Contract.AssertNotNull(AvailableContext.Compilation.GetTypeByMetadataName(typeof(AccessAttribute).FullName));
                        INamespaceSymbol ContainingNamespace = Contract.AssertNotNull(AccessTypeSymbol.ContainingNamespace);
                        IAssemblySymbol ContainingAssembly = Contract.AssertNotNull(AccessTypeSymbol.ContainingAssembly);

                        IsSameNamespaceAssembly &= SymbolEqualityComparer.Default.Equals(ContainingNamespace, AttributeSymbol.ContainingNamespace);
                        IsSameNamespaceAssembly &= SymbolEqualityComparer.Default.Equals(ContainingAssembly, AttributeSymbol.ContainingAssembly);
                    }
                    else
                    {
                        IsSameNamespaceAssembly = false;
                    }
                }

                if (IsSameNamespaceAssembly)
                {
                    string AttributeName = ToAttributeName(Attribute);

                    if (supportedAttributeTypes.ToList().ConvertAll(item => item.Name).Contains(AttributeName))
                        Result.Add(Attribute);
                }
            }
        }

        return Result;
    }

    /// <summary>
    /// Returns the full name of an attribute.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    public static string ToAttributeName(AttributeSyntax attribute) => $"{attribute.Name.GetText()}{nameof(Attribute)}";
}