namespace CSharpLatest.Test;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing;

internal static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, string)"/>
    public static async Task VerifyRefactoringAsync(string source, string fixedSource)
        => await VerifyRefactoringAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource).ConfigureAwait(false);

    /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult, string)"/>
    public static async Task VerifyRefactoringAsync(string source, DiagnosticResult expected, string fixedSource)
        => await VerifyRefactoringAsync(source, [expected], fixedSource).ConfigureAwait(false);

    /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult[], string)"/>
    public static async Task VerifyRefactoringAsync(string source, DiagnosticResult[] expected, string fixedSource)
    {
        Test test = new();

        if (test.IsDiagnosticEnabledd && test.HasHelpLink)
        {
            test = new()
            {
                TestCode = source,
                FixedCode = fixedSource,
            };
        }

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
