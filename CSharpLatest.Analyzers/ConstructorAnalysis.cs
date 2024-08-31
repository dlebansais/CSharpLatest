﻿namespace CSharpLatest;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides helpers for analyzing constructors.
/// </summary>
public static class ConstructorAnalysis
{
    /// <summary>
    /// Values of a best suggestion.
    /// </summary>
    public enum BestSuggestion
    {
        /// <summary>
        /// No suggestion.
        /// </summary>
        None,

        /// <summary>
        /// Primary constructor suggestion.
        /// </summary>
        PrimaryConstructor,

        /// <summary>
        /// Record suggestion.
        /// </summary>
        Record,
    }

    /// <summary>
    /// Gets the best suggestion for a class declaration.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    public static BestSuggestion GetBestSuggestion(ClassDeclarationSyntax classDeclaration)
    {
        // Make sure the class has a name to display.
        if (classDeclaration.Identifier.Text == string.Empty)
            return BestSuggestion.None;

        // Make sure the declaration doesn't already use a primary contructor.
        if (classDeclaration.ParameterList is not null)
            return BestSuggestion.None;

        // No diagnostic if any of the constructors calls this() or base().
        if (classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().Any(constructor => constructor.Initializer is not null))
            return BestSuggestion.None;

        // If the list of candidates is empty, no primary constructor can be used.
        List<ParameterSyntax> ParameterCandidates = GetParameterCandidates(classDeclaration);
        if (ParameterCandidates.Count == 0)
            return BestSuggestion.None;

        // If there isn't a constructor that handles these parameters only, let's not try to second-guess the code.
        if (GetConstructorCandidate(classDeclaration, ParameterCandidates) is not ConstructorDeclarationSyntax ConstructorCandidate)
            return BestSuggestion.None;

        // If the constructor is doing anything else than assigning properties, let's not try to second-guess the code.
        (bool HasPropertyAssignmentsOnly, List<AssignmentExpressionSyntax> Assignments) = GetPropertyAssignments(classDeclaration, ConstructorCandidate);
        if (!HasPropertyAssignmentsOnly || Assignments.Count == 0)
            return BestSuggestion.None;

        int ConstructorCount = 0;

        // If other constructors don't do the same, let's not try to second-guess the code.
        foreach (var Member in classDeclaration.Members)
            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
            {
                ConstructorCount++;

                if (ConstructorDeclaration != ConstructorCandidate && !IsConstructorStartingWithAssignments(ConstructorDeclaration, Assignments))
                    return BestSuggestion.None;
            }

        // If there is only one constructor, considering our constraints a record is a better option.
        if (ConstructorCount > 1)
            return BestSuggestion.PrimaryConstructor;

        return BestSuggestion.Record;
    }

    /// <summary>
    /// Gets the list of parameters that are candidates to be in a primary constructor.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    public static List<ParameterSyntax> GetParameterCandidates(ClassDeclarationSyntax classDeclaration)
    {
        List<ParameterSyntax> Result = new();
        bool IsFirstConstructor = true;

        foreach (var Member in classDeclaration.Members)
            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
            {
                List<ParameterSyntax> Parameters = [.. ConstructorDeclaration.ParameterList.Parameters];

                if (IsFirstConstructor)
                {
                    IsFirstConstructor = false;
                    Result = [.. Parameters];
                }
                else
                {
                    int LastCandidateIndex;

                    for (LastCandidateIndex = 0; LastCandidateIndex < Result.Count && LastCandidateIndex < Parameters.Count; LastCandidateIndex++)
                        if (!IsSameParameter(Result[LastCandidateIndex], Parameters[LastCandidateIndex]))
                            break;

                    Result = Result.GetRange(0, LastCandidateIndex);
                }
            }

        return Result;
    }

    private static bool IsSameParameter(ParameterSyntax p1, ParameterSyntax p2)
    {
        return IsSyntaxNodeEquivalent(p1, p2);
    }

    /// <summary>
    /// Gets the constructor that takes the provided list of parameters.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="parameterCandidates">The list of parameters.</param>
    /// <returns></returns>
    public static ConstructorDeclarationSyntax? GetConstructorCandidate(ClassDeclarationSyntax classDeclaration, List<ParameterSyntax> parameterCandidates)
    {
        foreach (var Member in classDeclaration.Members)
            if (Member is ConstructorDeclarationSyntax ConstructorDeclaration)
                if (ConstructorDeclaration.ParameterList.Parameters.Count == parameterCandidates.Count)
                    return ConstructorDeclaration;

        return null;
    }

    /// <summary>
    /// Gets the list of property assignments in the constructor.
    /// </summary>
    /// <param name="classDeclaration">The class declaration.</param>
    /// <param name="constructorDeclaration">The constructor declaration.</param>
    /// <returns><see langword="true"/> and the assignments if the constructor has no other statements; otherwise, <see langword="false"/>.</returns>
    public static (bool, List<AssignmentExpressionSyntax>) GetPropertyAssignments(ClassDeclarationSyntax classDeclaration, ConstructorDeclarationSyntax constructorDeclaration)
    {
        (bool HasOtherStatements, List<AssignmentExpressionSyntax> Assignments) = GetConstructorStartingAssignments(constructorDeclaration);

        if (HasOtherStatements)
            return (false, Assignments);

        foreach (AssignmentExpressionSyntax Assignment in Assignments)
        {
            Debug.Assert(Assignment.Left is IdentifierNameSyntax);
            IdentifierNameSyntax IdentifierName = (IdentifierNameSyntax)Assignment.Left;

            if (!classDeclaration.Members.OfType<PropertyDeclarationSyntax>().Any(propertyDeclaration => propertyDeclaration.Identifier.Text == IdentifierName.Identifier.Text && propertyDeclaration.Initializer is null))
                return (false, Assignments);
        }

        return (true, Assignments);
    }

    private static bool IsConstructorStartingWithAssignments(ConstructorDeclarationSyntax constructorDeclaration, List<AssignmentExpressionSyntax> expectedAssignments)
    {
        (bool HasOtherStatements, List<AssignmentExpressionSyntax> Assignments) = GetConstructorStartingAssignments(constructorDeclaration);

        if (Assignments.Count < expectedAssignments.Count)
            return false;

        for (int i = 0; i < expectedAssignments.Count; i++)
            if (!IsSyntaxNodeEquivalent(Assignments[i], expectedAssignments[i]))
                return false;

        return true;
    }

    /// <summary>
    /// Checks whether two <see cref="SyntaxNode"/> are equivalent.
    /// This method ignores leading and trailing trivias.
    /// </summary>
    /// <param name="node1">The first assignment.</param>
    /// <param name="node2">The second assignment.</param>
    public static bool IsSyntaxNodeEquivalent<T>(T node1, T node2)
        where T : SyntaxNode
    {
        T CleanNode1 = node1.WithoutLeadingTrivia().WithoutTrailingTrivia();
        T CleanNode2 = node2.WithoutLeadingTrivia().WithoutTrailingTrivia();

        return CleanNode1.IsEquivalentTo(CleanNode2);
    }

    private static (bool, List<AssignmentExpressionSyntax>) GetConstructorStartingAssignments(ConstructorDeclarationSyntax constructorDeclaration)
    {
        List<AssignmentExpressionSyntax> Assignments = new();
        bool HasOtherStatements = false;

        if (constructorDeclaration.Body is BlockSyntax Body)
        {
            foreach (var Statement in Body.Statements)
            {
                if (Statement is not ExpressionStatementSyntax ExpressionStatement || ExpressionStatement.Expression is not AssignmentExpressionSyntax Assignment)
                {
                    HasOtherStatements = true;
                    break;
                }

                if (Assignment.Left is not IdentifierNameSyntax || Assignment.Right is not IdentifierNameSyntax)
                {
                    HasOtherStatements = true;
                    break;
                }

                Assignments.Add(Assignment);
            }
        }

        if (constructorDeclaration.ExpressionBody is ArrowExpressionClauseSyntax ExpressionBody)
        {
            if (ExpressionBody.Expression is AssignmentExpressionSyntax Assignment)
                Assignments = new() { Assignment };
            else
                HasOtherStatements = true;
        }

        return (HasOtherStatements, Assignments);
    }
}