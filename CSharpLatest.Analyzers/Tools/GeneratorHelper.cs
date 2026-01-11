namespace CSharpLatest;

using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;

/// <summary>
/// Helper class for the code generator.
/// </summary>
internal static class GeneratorHelper
{
    private const string UsingDirectivePrefix = "using ";

    /// <summary>
    /// Checks whether using directives contain 'using global::System'.
    /// </summary>
    /// <param name="usings">The using directives to check.</param>
    /// <returns><see langword="true"/> if using directives contain 'using global::System'; otherwise, <see langword="false"/>.</returns>
    public static bool HasGlobalSystem(string usings)
    {
        if (usings.Length == 0)
            return false;

        string[] Lines = usings.Split('\n');

        foreach (string Line in Lines)
            if (Line == "using global::System;" || StringStartsWith(Line, "using global::System."))
                return true;

        return false;
    }

    /// <summary>
    /// Sorts using directives.
    /// </summary>
    /// <param name="usings">The using directives to sort.</param>
    /// <returns>The sorted directives.</returns>
    public static string SortUsings(string usings)
    {
        if (usings == string.Empty)
            return string.Empty;

        List<string> Namespaces = [];
        string[] Lines = usings.Split('\n');

        foreach (string Line in Lines)
            if (IsUsingDirective(Line, out string Directive))
                Namespaces.Add(Directive);

        Namespaces.Sort(SortWithSystemFirst);
        Namespaces = [.. Namespaces.Distinct()];

        string Result = string.Empty;

        foreach (string DirectiveNamespace in Namespaces)
            Result += $"\n{UsingDirectivePrefix}{DirectiveNamespace};";

        return Result + "\n";
    }

    private static bool IsUsingDirective(string line, out string directiveNamespace)
    {
        string TrimmedLine = line.Trim(' ').Trim('\n').Trim('\r');

        if (StringStartsWith(TrimmedLine, UsingDirectivePrefix))
        {
            string RawNamespace = TrimmedLine.Substring(UsingDirectivePrefix.Length, TrimmedLine.Length - UsingDirectivePrefix.Length - 1);
            string[] Names = RawNamespace.Split('.');

            List<string> TrimmedNames = [];
            foreach (string Name in Names)
                TrimmedNames.Add(Name.Trim());

            directiveNamespace = string.Join(".", TrimmedNames);
            return true;
        }

        Contract.Unused(out directiveNamespace);
        return false;
    }

    private static int SortWithSystemFirst(string line1, string line2)
    {
        if (IsSystemUsing(line1) && !IsSystemUsing(line2))
            return -1;
        else if (!IsSystemUsing(line1) && IsSystemUsing(line2))
            return 1;
        else
#if NETFRAMEWORK
            return string.Compare(line1, line2);
#else
            return string.Compare(line1, line2, StringComparison.Ordinal);
#endif
    }

    private static bool IsSystemUsing(string usingNamespace) => usingNamespace == "System" || StringStartsWith(usingNamespace, "System.") || usingNamespace == "global::System" || StringStartsWith(usingNamespace, "global::System.");

    /// <summary>
    /// Returns whether the string <paramref name="s"/> starts with the prefix <paramref name="prefix"/>, performing a <see cref="StringComparison.Ordinal"/> comparison.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <param name="prefix">The prefix.</param>
    public static bool StringStartsWith(string s, string prefix) => s.StartsWith(prefix, StringComparison.Ordinal);
}