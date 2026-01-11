namespace CSharpLatest.FieldBackedProperty;

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class FieldBackedPropertyGenerator
{
    private static void OutputFieldBackedMethod(SourceProductionContext context, (GeneratorSettings Settings, ImmutableArray<PropertyModel> Models) modelAndSettings)
    {
        foreach (PropertyModel Model in modelAndSettings.Models)
        {
            string SourceText = $$"""
                #nullable enable

                namespace {{Model.Namespace}};

                using System.CodeDom.Compiler;
                
                partial {{Model.DeclarationTokens}} {{Model.FullClassName}}
                {
                {{Model.Documentation}}{{Model.GeneratedPropertyDeclaration}}{{Model.GeneratedFieldDeclaration}}
                }
                """;
            SourceText = Replace(SourceText, "\r\n", "\n");

            context.AddSource($"{Model.ClassName}_{Model.SymbolName}.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(SourceText, Encoding.UTF8));
        }
    }

    /// <summary>
    /// Returns a string with <paramref name="oldString"/> replaced with <paramref name="newString"/>.
    /// </summary>
    /// <param name="s">The string with substrings to replace.</param>
    /// <param name="oldString">The string to replace.</param>
    /// <param name="newString">The new string.</param>
    private static string Replace(string s, string oldString, string newString) =>
#if NETSTANDARD2_1_OR_GREATER
        s.Replace(oldString, newString, StringComparison.Ordinal);
#else
        s.Replace(oldString, newString);
#endif
}
