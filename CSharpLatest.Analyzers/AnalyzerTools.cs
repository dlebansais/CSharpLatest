namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Helper providing methods for analyzers.
/// </summary>
internal static class AnalyzerTools
{
    /// <summary>
    /// The minimum version of the language we care about.
    /// </summary>
    public const LanguageVersion MinimumVersionAnalyzed = LanguageVersion.CSharp4;

    // Define this symbol in unit tests to simulate an assertion failure.
    // This will test branches that can only execute in future versions of C#.
    private const string CoverageDirectivePrefix = "#define COVERAGE_A25BDFABDDF8402785EB75AD812DA952";

    /// <summary>
    /// Gets the help link for a diagnostic id.
    /// </summary>
    /// <param name="diagnosticId">The diagnostic id.</param>
    public static string GetHelpLink(string diagnosticId) => $"https://github.com/dlebansais/CSharpLatest/blob/master/doc/{diagnosticId}.md";

    /// <summary>
    /// Asserts that the analyzed node is of the expected type and satisfies requirements, then executes <paramref name="continueAction"/>.
    /// </summary>
    /// <typeparam name="T">The type of the analyzed node.</typeparam>
    /// <param name="context">The analyzer context.</param>
    /// <param name="minimumLanguageVersion">The minimum language version supporting the feature.</param>
    /// <param name="continueAction">The next analysis step.</param>
    /// <param name="analysisAssertions">A list of assertions.</param>
    public static void AssertSyntaxRequirements<T>(SyntaxNodeAnalysisContext context, LanguageVersion minimumLanguageVersion, Action<SyntaxNodeAnalysisContext, T, Collection<IAnalysisAssertion>> continueAction, params Collection<IAnalysisAssertion> analysisAssertions)
        where T : CSharpSyntaxNode
    {
        T ValidNode = (T)context.Node;

        if (IsFeatureSupportedInThisVersion(context, minimumLanguageVersion))
        {
            bool IsCoverageContext = IsCalledForCoverage(context);
            bool AreAllAssertionsTrue = analysisAssertions.TrueForAll(context);

            if (!IsCoverageContext && AreAllAssertionsTrue)
                continueAction(context, ValidNode, analysisAssertions);
        }
    }

    private static bool IsFeatureSupportedInThisVersion(SyntaxNodeAnalysisContext context, LanguageVersion minimumLanguageVersion)
    {
        CSharpParseOptions ParseOptions = (CSharpParseOptions)context.SemanticModel.SyntaxTree.Options;
        return ParseOptions.LanguageVersion >= minimumLanguageVersion;
    }

    private static bool IsCalledForCoverage(SyntaxNodeAnalysisContext context)
    {
        string? FirstDirectiveText = context.SemanticModel.SyntaxTree.GetRoot().GetFirstDirective()?.GetText().ToString();
        return FirstDirectiveText is not null && FirstDirectiveText.StartsWith(CoverageDirectivePrefix, StringComparison.Ordinal);
    }

    private static bool TrueForAll(this IEnumerable<IAnalysisAssertion> analysisAssertions, SyntaxNodeAnalysisContext context) => analysisAssertions.All(analysisAssertion => IsTrue(analysisAssertion, context));

    private static bool IsTrue(this IAnalysisAssertion analysisAssertion, SyntaxNodeAnalysisContext context) => analysisAssertion.IsTrue(context);

    /// <summary>
    /// Gets the base information of a symbol.
    /// </summary>
    /// <param name="typeSymbol">The symbol.</param>
    public static BaseInfo GetBaseInfo(INamedTypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return new BaseInfo(IsObject: false, Depth: 0);

        if (typeSymbol.SpecialType == SpecialType.System_Object)
            return new BaseInfo(IsObject: true, Depth: 0);

        BaseInfo BaseTypeInfo = GetBaseInfo(typeSymbol.BaseType);
        return BaseTypeInfo with { Depth = BaseTypeInfo.Depth + 1 };
    }

    private static string GetUserPreferenceFromContextOptions(SyntaxNodeAnalysisContext context, string setting, string defaultValue)
    {
        AnalyzerConfigOptions Options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);

        // Trick to cover the case where TryGetValue returns true, since it proved impossible to unit test.
        _ = Options.TryGetValue(setting, out string? UserPreference);
        return $"{UserPreference};{defaultValue}".Split(';').Where(s => s.Length > 0).First();
    }

    private static string GetUserPreferenceFromCompilationOptions(SyntaxNodeAnalysisContext context, string setting, string defaultValue)
    {
        foreach (string Key in context.Compilation.Options.SpecificDiagnosticOptions.Keys)
        {
            string[] KeyAndValue = Key.Split('=');
            if (KeyAndValue.Length == 2 && KeyAndValue[0].Trim() == setting)
                return KeyAndValue[1].Trim();
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets user preferences from .editorconfig or a custom test setting.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="setting">The setting to check.</param>
    /// <param name="defaultValue">The default value to use if no setting value is found.</param>
    /// <returns>The setting value, or <paramref name="defaultValue"/>.</returns>
    public static string GetUserPreference(SyntaxNodeAnalysisContext context, string setting, string defaultValue)
    {
        string value = defaultValue;

        value = GetUserPreferenceFromContextOptions(context, setting, value);
        value = GetUserPreferenceFromCompilationOptions(context, setting, value);

        return value;
    }

    /// <summary>
    /// Checks whether an attribute is of the expected type.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="attributeType">The type.</param>
    /// <param name="attribute">The attribute.</param>
    public static bool IsExpectedAttribute(SyntaxNodeAnalysisContext context, Type attributeType, AttributeSyntax? attribute)
    {
        // There must be a parent attribute to any argument except in the most pathological cases.
        Contract.RequireNotNull(attribute, out AttributeSyntax Attribute);

        TypeInfo TypeInfo = context.SemanticModel.GetTypeInfo(Attribute);
        ITypeSymbol? TypeSymbol = TypeInfo.Type;

        return IsExpectedAttribute(context, attributeType, TypeSymbol);
    }

    private static bool IsExpectedAttribute(SyntaxNodeAnalysisContext context, Type attributeType, ITypeSymbol? typeSymbol)
    {
        ITypeSymbol? ExpectedTypeSymbol = context.Compilation.GetTypeByMetadataName(attributeType.FullName);

        return SymbolEqualityComparer.Default.Equals(typeSymbol, ExpectedTypeSymbol);
    }
}
