namespace CSharpLatest;

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents a code generator.
/// </summary>
public partial class PropertyGenerator
{
    private static string GetGeneratedPropertyDeclaration(PropertyModel model, GeneratorAttributeSyntaxContext context)
    {
        SyntaxNode TargetNode = context.TargetNode;
        PropertyDeclarationSyntax PropertyDeclaration = Contract.AssertOfType<PropertyDeclarationSyntax>(TargetNode);
        bool IsFieldKeywordSupported = CheckFieldKeywordSupport(PropertyDeclaration);

        string Tab = new(' ', Math.Max(Settings.TabLength, 1));
        SyntaxTriviaList LeadingTrivia = GetLeadingTriviaWithLineEnd(Tab);
        SyntaxTriviaList LeadingTriviaWithoutLineEnd = GetLeadingTriviaWithoutLineEnd(Tab);
        SyntaxTriviaList TrailingTrivia = PropertyDeclaration.Modifiers.Last().TrailingTrivia;

        SyntaxList<AttributeListSyntax> CodeAttributes = GenerateCodeAttributes();
        PropertyDeclaration = PropertyDeclaration.WithAttributeLists(CodeAttributes);

        SyntaxToken SymbolIdentifier = SyntaxFactory.Identifier(model.SymbolName);
        PropertyDeclaration = PropertyDeclaration.WithIdentifier(SymbolIdentifier);

        SyntaxTokenList Modifiers = GeneratePropertyModifiers(PropertyDeclaration, LeadingTrivia, TrailingTrivia);
        PropertyDeclaration = PropertyDeclaration.WithModifiers(Modifiers);

        AccessorListSyntax PropertyAccessorList = GenerateAccessorList(model, IsFieldKeywordSupported, LeadingTrivia, LeadingTriviaWithoutLineEnd, Tab);
        PropertyDeclaration = PropertyDeclaration.WithAccessorList(PropertyAccessorList);

        if (IsFieldKeywordSupported && HasInitializer(model, out EqualsValueClauseSyntax Initializer))
            PropertyDeclaration = PropertyDeclaration.WithInitializer(Initializer);

        PropertyDeclaration = PropertyDeclaration.WithLeadingTrivia(LeadingTriviaWithoutLineEnd);

        return PropertyDeclaration.ToFullString();
    }

    private static SyntaxTriviaList GetLeadingTriviaWithTwoLineEnd(string tab)
    {
        List<SyntaxTrivia> Trivias =
        [
            SyntaxFactory.EndOfLine("\n"),
            SyntaxFactory.EndOfLine("\n"),
            SyntaxFactory.Whitespace(tab),
        ];

        return SyntaxFactory.TriviaList(Trivias);
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

    private static SyntaxTokenList GeneratePropertyModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia)
    {
        List<SyntaxToken> ModifierTokens = GeneratePropertyDefaultModifiers(memberDeclaration, leadingTrivia, trailingTrivia);

        return SyntaxFactory.TokenList(ModifierTokens);
    }

    private static List<SyntaxToken> GeneratePropertyDefaultModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTriviaList leadingTrivia, SyntaxTriviaList trailingTrivia)
    {
        List<SyntaxToken> ModifierTokens = [];
        SyntaxTriviaList CurrentTrivia = leadingTrivia;

        // Replicate access modifiers in the generated code.
        foreach (SyntaxToken Modifier in memberDeclaration.Modifiers)
        {
            string ModifierText = Modifier.Text;
            bool IsAccessModifier = ModifierText is "public" or "protected" or "internal" or "private" or "file";
            bool IsOtherModifier = ModifierText is "virtual" or "override" or "sealed" or "required";

            if (IsAccessModifier || IsOtherModifier)
                AddModifier(ModifierTokens, Modifier, ref CurrentTrivia);
        }

        SyntaxToken PartialModifierToken = SyntaxFactory.Identifier("partial");
        PartialModifierToken = PartialModifierToken.WithLeadingTrivia(CurrentTrivia);
        ModifierTokens.Add(PartialModifierToken);

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

    private static AccessorListSyntax GenerateAccessorList(PropertyModel model, bool isFieldKeywordSupported, SyntaxTriviaList tabTrivia, SyntaxTriviaList tabTriviaWithoutLineEnd, string tab)
    {
        Debug.Assert(tabTriviaWithoutLineEnd.Count > 0);

        SyntaxToken OpenBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken);
        OpenBraceToken = OpenBraceToken.WithLeadingTrivia(tabTrivia);

        List<SyntaxTrivia> TrivialList = [.. tabTrivia, SyntaxFactory.Whitespace(tab)];
        SyntaxTriviaList TabAccessorsTrivia = SyntaxFactory.TriviaList(TrivialList);

        SyntaxToken CloseBraceToken = SyntaxFactory.Token(SyntaxKind.CloseBraceToken);
        CloseBraceToken = CloseBraceToken.WithLeadingTrivia(tabTrivia);

        string? FieldReplacement = isFieldKeywordSupported
            ? null
            : Settings.FieldPrefix + model.SymbolName;

        AccessorDeclarationSyntax Getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);

        if (IsTextBlockBody(model.GetterText, FieldReplacement, out BlockSyntax GetterBlockBody))
        {
            Getter = Getter.WithBody(GetterBlockBody);
        }
        else
        {
            ArrowExpressionClauseSyntax? GetterExpressionBody = ToTextExpressionBody(model.GetterText, FieldReplacement);

            Getter = Getter.WithExpressionBody(GetterExpressionBody);
            Getter = Getter.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        AccessorDeclarationSyntax Setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);

        if (IsTextBlockBody(model.SetterText, FieldReplacement, out BlockSyntax SetterBlockBody))
        {
            Setter = Setter.WithBody(SetterBlockBody);
        }
        else
        {
            ArrowExpressionClauseSyntax? SetterExpressionBody = ToTextExpressionBody(model.SetterText, FieldReplacement);

            Setter = Setter.WithExpressionBody(SetterExpressionBody);
            Setter = Setter.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        Getter = Getter.WithLeadingTrivia(TabAccessorsTrivia);
        Setter = Setter.WithLeadingTrivia(TabAccessorsTrivia);

        return SyntaxFactory.AccessorList(OpenBraceToken, [Getter, Setter], CloseBraceToken);
    }

    private static bool IsTextBlockBody(string text, string? fieldReplacement, out BlockSyntax blockBody)
    {
        if (text != string.Empty)
        {
            if (fieldReplacement is not null)
                text = text.Replace("field", fieldReplacement);

            CompilationUnitSyntax ParsedRoot = (CompilationUnitSyntax)SyntaxFactory.ParseSyntaxTree(text).GetRoot();

            Contract.Assert(ParsedRoot.Members.Count > 0);
            GlobalStatementSyntax GlobalStatement = (GlobalStatementSyntax)ParsedRoot.Members[0];

            if (GlobalStatement.Statement is BlockSyntax Block)
            {
                blockBody = Block.WithLeadingTrivia(SyntaxFactory.Space);
                return true;
            }
        }

        Contract.Unused(out blockBody);
        return false;
    }

    private static ArrowExpressionClauseSyntax? ToTextExpressionBody(string text, string? fieldReplacement)
    {
        ArrowExpressionClauseSyntax? expressionBody = null;

        if (text != string.Empty)
        {
            if (fieldReplacement is not null)
                text = text.Replace("field", fieldReplacement);

            ExpressionSyntax Invocation = SyntaxFactory.ParseExpression(text);
            string FullText = Invocation.ToString();
            if (FullText == text)
            {
                expressionBody = SyntaxFactory.ArrowExpressionClause(Invocation.WithLeadingTrivia(SyntaxFactory.Space)).WithLeadingTrivia(SyntaxFactory.Space);
            }
        }

        return expressionBody;
    }
}
