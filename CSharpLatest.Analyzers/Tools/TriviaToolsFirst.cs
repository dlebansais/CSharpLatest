namespace CSharpLatest;

using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Helper providing methods for analyzers.
/// </summary>
internal static class TriviaToolsFirst
{
    /// <summary>
    /// Gets the first trivia.
    /// </summary>
    /// <param name="xmlTrivia">The trivia list.</param>
    public static SyntaxTrivia FirstTrivia(SyntaxTriviaList xmlTrivia)
    {
        SyntaxTrivia First = xmlTrivia.FirstOrDefault(AnalyzerTools.IsDocTrivia);
        return First;
    }
}
