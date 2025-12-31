namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Helper providing methods for analyzers.
/// </summary>
internal static class TriviaToolsLast
{
    /// <summary>
    /// Gets the last trivia.
    /// </summary>
    /// <param name="xmlTrivia">The trivia list.</param>
    public static SyntaxTrivia LastTrivia(SyntaxTriviaList xmlTrivia)
    {
        SyntaxTrivia Last = xmlTrivia.LastOrDefault(AnalyzerTools.IsDocTrivia);
        return Last;
    }
}
