namespace CSharpLatest.AsyncEventHandlerCodeGeneration;

using System.Text;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class AsyncEventHandlerGenerator
{
    private static void OutputEventHandlerMethod(SourceProductionContext context, MethodModel model)
    {
        string SourceText = $$"""
                #nullable enable

                namespace {{model.Namespace}};

                using System.Diagnostics;
                using System.CodeDom.Compiler;
                
                partial {{model.DeclarationTokens}} {{model.FullClassName}}
                {
                {{model.Documentation}}{{model.GeneratedMethodDeclaration}}
                }
                """;
        SourceText = Replace(SourceText, "\r\n", "\n");

        context.AddSource($"{model.ClassName}_{model.SymbolName}Async.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(SourceText, Encoding.UTF8));
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
