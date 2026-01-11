namespace CSharpLatest.AsyncEventCodeGeneration;

using System.Text;
using Microsoft.CodeAnalysis;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class AsyncEventGenerator
{
    private static void OutputEventHandlerEvent(SourceProductionContext context, EventModel model)
    {
        string SourceText = $$"""
                #nullable enable
                {{model.UsingsBeforeNamespace}}
                namespace {{model.Namespace}};
                {{model.UsingsAfterNamespace}}
                partial {{model.DeclarationTokens}} {{model.FullClassName}}
                {
                {{model.Documentation}}{{model.GeneratedEventDeclaration}}
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
