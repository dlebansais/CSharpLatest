#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CSharpLatest.Test;

using System.Collections.Generic;

public static class TestTools
{
    public static Dictionary<string, string> ToOptions(string args)
    {
        Dictionary<string, string> Result = [];
        List<string> AvailableOptions = ["csharp_prefer_braces", "foo"];

        string[] SplittedArgs = args.Split(';');
        for (int i = 0; i < SplittedArgs.Length && i < AvailableOptions.Count; i++)
        {
            if (SplittedArgs[i].Length > 0)
                Result.Add(AvailableOptions[i], SplittedArgs[i]);
        }

        return Result;
    }
}
