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
        SyntaxTriviaList? TrailingTrivia = GetModifiersTrailingTrivia(PropertyDeclaration);
        bool SimplifyReturnTypeLeadingTrivia = PropertyDeclaration.Modifiers.Count == 0 && PropertyDeclaration.Type.HasLeadingTrivia;

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

        if (SimplifyReturnTypeLeadingTrivia) // This case applies to properties with zero modifier that become public.
            PropertyDeclaration = PropertyDeclaration.WithType(PropertyDeclaration.Type.WithLeadingTrivia(SyntaxFactory.Space));

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

    private static SyntaxTriviaList? GetModifiersTrailingTrivia(MemberDeclarationSyntax memberDeclaration) => memberDeclaration.Modifiers.Count > 0 ? memberDeclaration.Modifiers.Last().TrailingTrivia : null;

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

    private static SyntaxTokenList GeneratePropertyModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTriviaList leadingTrivia, SyntaxTriviaList? trailingTrivia)
    {
        List<SyntaxToken> ModifierTokens = GeneratePropertyDefaultModifiers(memberDeclaration, leadingTrivia, trailingTrivia);

        return SyntaxFactory.TokenList(ModifierTokens);
    }

    private static List<SyntaxToken> GeneratePropertyDefaultModifiers(MemberDeclarationSyntax memberDeclaration, SyntaxTriviaList leadingTrivia, SyntaxTriviaList? trailingTrivia)
    {
        List<SyntaxToken> ModifierTokens = [];
        SyntaxTriviaList CurrentTrivia = leadingTrivia;

        // Replicate access modifiers in the generated code.
        foreach (SyntaxToken Modifier in memberDeclaration.Modifiers)
        {
            string ModifierText = Modifier.Text;

            if (ModifierText is "public" or "protected" or "internal" or "private" or "file")
            {
                SyntaxToken StaticModifierToken = SyntaxFactory.Identifier(Modifier.Text);
                StaticModifierToken = StaticModifierToken.WithLeadingTrivia(CurrentTrivia);
                ModifierTokens.Add(StaticModifierToken);

                UpdateTrivia(ref CurrentTrivia);
            }
        }

        SyntaxToken PartialModifierToken = SyntaxFactory.Identifier("partial");
        PartialModifierToken = PartialModifierToken.WithLeadingTrivia(CurrentTrivia);
        ModifierTokens.Add(PartialModifierToken);

        UpdateTrivia(ref CurrentTrivia);

        // Replicate other modifiers in the generated code.
        foreach (SyntaxToken Modifier in memberDeclaration.Modifiers)
        {
            string ModifierText = Modifier.Text;

            if (ModifierText is "static" or "virtual" or "override" or "sealed" or "unsafe" or "required")
            {
                SyntaxToken StaticModifierToken = SyntaxFactory.Identifier(Modifier.Text);
                StaticModifierToken = StaticModifierToken.WithLeadingTrivia(CurrentTrivia);
                ModifierTokens.Add(StaticModifierToken);

                UpdateTrivia(ref CurrentTrivia);
            }
        }

        int LastItemIndex = ModifierTokens.Count - 1;
        ModifierTokens[LastItemIndex] = ModifierTokens[LastItemIndex].WithTrailingTrivia(trailingTrivia);

        return ModifierTokens;
    }

    private static void UpdateTrivia(ref SyntaxTriviaList triviaList) => triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Space);

    private static AccessorListSyntax GenerateAccessorList(PropertyModel model, bool isFieldKeywordSupported, SyntaxTriviaList tabTrivia, SyntaxTriviaList tabTriviaWithoutLineEnd, string tab)
    {
        Debug.Assert(tabTriviaWithoutLineEnd.Count > 0);

        SyntaxToken OpenBraceToken = SyntaxFactory.Token(SyntaxKind.OpenBraceToken);
        OpenBraceToken = OpenBraceToken.WithLeadingTrivia(tabTrivia);

        List<SyntaxTrivia> TrivialList = [.. tabTrivia, SyntaxFactory.Whitespace(tab)];
        SyntaxTriviaList TabAccessorsTrivia = SyntaxFactory.TriviaList(TrivialList);

        List<SyntaxTrivia> TrivialListExtraLineEnd = new(TrivialList);
        TrivialListExtraLineEnd.Insert(0, SyntaxFactory.EndOfLine("\n"));
        TrivialListExtraLineEnd.Add(SyntaxFactory.Whitespace(tab));

        SyntaxToken CloseBraceToken = SyntaxFactory.Token(SyntaxKind.CloseBraceToken);
        CloseBraceToken = CloseBraceToken.WithLeadingTrivia(tabTrivia);

        string? FieldReplacement = isFieldKeywordSupported
            ? null
            : Settings.FieldPrefix + model.SymbolName;

        AccessorDeclarationSyntax Getter = IsTextExpressionBody(model.GetterText, FieldReplacement, out ArrowExpressionClauseSyntax GetterExpressionBody)
            ? SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithExpressionBody(GetterExpressionBody).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            : IsTextBlockBody(model.GetterText, FieldReplacement, out BlockSyntax GetterBlockBody)
                ? SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, GetterBlockBody)
                : SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);

        AccessorDeclarationSyntax Setter = IsTextExpressionBody(model.SetterText, FieldReplacement, out ArrowExpressionClauseSyntax SetterExpressionBody)
            ? SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithExpressionBody(SetterExpressionBody).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            : IsTextBlockBody(model.SetterText, FieldReplacement, out BlockSyntax SetterBlockBody)
                ? SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, SetterBlockBody)
                : SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);

        Getter = Getter.WithLeadingTrivia(TabAccessorsTrivia);
        Setter = Setter.WithLeadingTrivia(TabAccessorsTrivia);

        return SyntaxFactory.AccessorList(OpenBraceToken, [Getter, Setter], CloseBraceToken);
    }

    private static bool IsTextExpressionBody(string text, string? fieldReplacement, out ArrowExpressionClauseSyntax expressionBody)
    {
        if (text != string.Empty)
        {
            if (fieldReplacement is not null)
                text = text.Replace("field", fieldReplacement);

            ExpressionSyntax Invocation = SyntaxFactory.ParseExpression(text);
            expressionBody = SyntaxFactory.ArrowExpressionClause(Invocation.WithLeadingTrivia(SyntaxFactory.Space)).WithLeadingTrivia(SyntaxFactory.Space);
            return true;
        }

        Contract.Unused(out expressionBody);
        return false;
    }

    private static bool IsTextBlockBody(string text, string? fieldReplacement, out BlockSyntax blockBody)
    {
        if (text != string.Empty)
        {
            if (fieldReplacement is not null)
                text = text.Replace("field", fieldReplacement);

            if (SyntaxFactory.ParseSyntaxTree(text).GetRoot() is BlockSyntax Block)
            {
                blockBody = Block;
                return true;
            }
        }

        Contract.Unused(out blockBody);
        return false;
    }
}
