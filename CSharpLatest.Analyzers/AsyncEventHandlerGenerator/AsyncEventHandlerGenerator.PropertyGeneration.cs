namespace CSharpLatest.AsyncEventHandler;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class AsyncEventHandlerGenerator
{
    private static string GetGeneratedMethodDeclaration(GeneratorAttributeSyntaxContext context, string symbolName, MethodAttributeModel methodAttributeModel)
    {
        _ = methodAttributeModel; // TODO: use

        SyntaxNode TargetNode = context.TargetNode;
        MethodDeclarationSyntax MethodDeclaration = Contract.AssertOfType<MethodDeclarationSyntax>(TargetNode);

        string Tab = "    ";
        SyntaxTriviaList LeadingTrivia = GetLeadingTriviaWithLineEnd(Tab);
        SyntaxTriviaList LeadingTriviaWithoutLineEnd = GetLeadingTriviaWithoutLineEnd(Tab);
        SyntaxTriviaList TrailingTrivia = MethodDeclaration.Modifiers.Last().TrailingTrivia;

        SyntaxList<AttributeListSyntax> CodeAttributes = GenerateCodeAttributes();
        MethodDeclaration = MethodDeclaration.WithAttributeLists(CodeAttributes);

        SyntaxToken SymbolIdentifier = SyntaxFactory.Identifier(symbolName);
        MethodDeclaration = MethodDeclaration.WithIdentifier(SymbolIdentifier);

        SyntaxTokenList Modifiers = GenerateMethodModifiers(MethodDeclaration, LeadingTrivia, TrailingTrivia);
        MethodDeclaration = MethodDeclaration.WithModifiers(Modifiers);
        MethodDeclaration = MethodDeclaration.WithLeadingTrivia(LeadingTriviaWithoutLineEnd);

        return MethodDeclaration.ToFullString();
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
