namespace CSharpLatest.AsyncEventHandler;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class AsyncEventHandlerGenerator
{
    /*
    private static void Foo()
    {
        _ = 1;
        Task.Run(async () =>
        {
            try
            {
                Task task = FooAsync();
                await task.ConfigureAwait(false);
                task.Exception
            }
            catch (System.Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Fatal: exception in {{symbolName}}Async.\r\n{exception.Message}\r\n{exception.StackTrace}");
                throw;
            }
        }).Wait();
    }

    private static async Task FooAsync()
    {
        _ = 1;
        await Task.Delay(1000).ConfigureAwait(false);
    }
    */

    private static string GetGeneratedMethodDeclaration(GeneratorAttributeSyntaxContext context, string symbolName, MethodAttributeModel methodAttributeModel)
    {
        // Foo();
        SyntaxNode TargetNode = context.TargetNode;
        MethodDeclarationSyntax MethodDeclaration = Contract.AssertOfType<MethodDeclarationSyntax>(TargetNode);

        string Tab = "    ";
        SyntaxTriviaList LeadingTrivia = GetLeadingTriviaWithLineEnd(Tab);
        SyntaxTriviaList LeadingTriviaWithoutLineEnd = GetLeadingTriviaWithoutLineEnd(Tab);
        SyntaxTriviaList TrailingTrivia = MethodDeclaration.Modifiers.Last().TrailingTrivia;

        SyntaxList<AttributeListSyntax> CodeAttributes = GenerateCodeAttributes();
        MethodDeclaration = MethodDeclaration.WithAttributeLists(CodeAttributes);

        MethodDeclaration = MethodDeclaration.WithReturnType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)));

        SyntaxToken SymbolIdentifier = SyntaxFactory.Identifier(symbolName).WithLeadingTrivia(SyntaxFactory.Whitespace(" "));
        MethodDeclaration = MethodDeclaration.WithIdentifier(SymbolIdentifier);

        SyntaxTokenList Modifiers = GenerateMethodModifiers(MethodDeclaration, LeadingTrivia, TrailingTrivia);
        MethodDeclaration = MethodDeclaration.WithModifiers(Modifiers);
        MethodDeclaration = MethodDeclaration.WithLeadingTrivia(LeadingTriviaWithoutLineEnd);
        MethodDeclaration = MethodDeclaration.WithBody(null);
        MethodDeclaration = MethodDeclaration.WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));

        string FullString = MethodDeclaration.ToFullString();
        string Scheduler = methodAttributeModel.UseDispatcher
            ? methodAttributeModel.WaitUntilCompletion
              ? "_ = Dispatcher.Invoke"
              : "_ = Dispatcher.BeginInvoke"
            : methodAttributeModel.WaitUntilCompletion
              ? "Task.Run"
              : "_ = Task.Run";
        string Waiter = !methodAttributeModel.UseDispatcher && methodAttributeModel.WaitUntilCompletion
            ? ".Wait()"
            : string.Empty;

        string ReplacementText =
        $$"""
                {
                    {{Scheduler}}(async () =>
                    {
                        try
                        {
                            await {{symbolName}}Async().ConfigureAwait(false);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine($"Fatal: exception in {{symbolName}}Async.\r\n{exception.Message}\r\n{exception.StackTrace}");
                            throw;
                        }
                    }){{Waiter}};
                }
            """;

        FullString = FullString.Replace("=>true", ReplacementText);

        return FullString;
    }

    private static SyntaxTriviaList GetLeadingTriviaWithLineEnd(string tab)
    {
        List<SyntaxTrivia> Trivias =
        [
            SyntaxFactory.EndOfLine("\n"),
            SyntaxFactory.Whitespace(tab),
        ];

        return SyntaxFactory.TriviaList(Trivias);
    }

    private static SyntaxTriviaList GetLeadingTriviaWithoutLineEnd(string tab)
    {
        List<SyntaxTrivia> Trivias =
        [
            SyntaxFactory.Whitespace(tab),
        ];

        return SyntaxFactory.TriviaList(Trivias);
    }

    private static SyntaxList<AttributeListSyntax> GenerateCodeAttributes()
    {
        NameSyntax AttributeName = SyntaxFactory.IdentifierName(nameof(GeneratedCodeAttribute));

        string ToolName = GetToolName();
        SyntaxToken ToolNameToken = SyntaxFactory.Literal(ToolName);
        LiteralExpressionSyntax ToolNameExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, ToolNameToken);
        AttributeArgumentSyntax ToolNameAttributeArgument = SyntaxFactory.AttributeArgument(ToolNameExpression);

        string ToolVersion = GetToolVersion();
        SyntaxToken ToolVersionToken = SyntaxFactory.Literal(ToolVersion);
        LiteralExpressionSyntax ToolVersionExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, ToolVersionToken);
        AttributeArgumentSyntax ToolVersionAttributeArgument = SyntaxFactory.AttributeArgument(ToolVersionExpression);

        AttributeArgumentListSyntax ArgumentList = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([ToolNameAttributeArgument, ToolVersionAttributeArgument]));
        AttributeSyntax Attribute = SyntaxFactory.Attribute(AttributeName, ArgumentList);
        AttributeListSyntax AttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList([Attribute]));
        SyntaxList<AttributeListSyntax> Attributes = SyntaxFactory.List([AttributeList]);

        return Attributes;
    }

    private static string GetToolName()
    {
        System.Reflection.AssemblyName ExecutingAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        return ExecutingAssemblyName.Name.ToString();
    }

    private static string GetToolVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

    private static SyntaxTokenList GenerateMethodModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia)
    {
        List<SyntaxToken> ModifierTokens = GenerateMethodDefaultModifiers(memberDeclaration, leadingTrivia, trailingTrivia);

        return SyntaxFactory.TokenList(ModifierTokens);
    }

    private static List<SyntaxToken> GenerateMethodDefaultModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia)
    {
        List<SyntaxToken> ModifierTokens = [];
        SyntaxTriviaList CurrentTrivia = leadingTrivia;

        // Replicate access modifiers in the generated code.
        foreach (SyntaxToken Modifier in memberDeclaration.Modifiers)
        {
            string ModifierText = Modifier.Text;
            bool IsAccessModifier = ModifierText is "public" or "protected" or "internal" or "private" or "file";
            bool IsOtherModifier = ModifierText is "virtual" or "override" or "sealed";

            if (IsAccessModifier || IsOtherModifier)
                AddModifier(ModifierTokens, Modifier, ref CurrentTrivia);
        }

        UpdateTrivia(ref CurrentTrivia);

        // Replicate other modifiers that have to be after 'partial' in the generated code.
        foreach (SyntaxToken Modifier in memberDeclaration.Modifiers)
        {
            string ModifierText = Modifier.Text;

            if (ModifierText is "static" or "unsafe")
                AddModifier(ModifierTokens, Modifier, ref CurrentTrivia);
        }

        int LastItemIndex = ModifierTokens.Count - 1;
        ModifierTokens[LastItemIndex] = ModifierTokens[LastItemIndex].WithTrailingTrivia(trailingTrivia);

        return ModifierTokens;
    }

    private static void AddModifier(List<SyntaxToken> modifierTokens, SyntaxToken modifier, ref SyntaxTriviaList currentTrivia)
    {
        SyntaxToken StaticModifierToken = SyntaxFactory.Identifier(modifier.Text);
        StaticModifierToken = StaticModifierToken.WithLeadingTrivia(currentTrivia);
        modifierTokens.Add(StaticModifierToken);

        UpdateTrivia(ref currentTrivia);
    }

    private static void UpdateTrivia(ref SyntaxTriviaList triviaList) => triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Space);
}
