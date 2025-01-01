# CSharpLatest

Roslyn-based analysis of C# code to suggest the use of features in the latest version of the language.

[![Build status](https://ci.appveyor.com/api/projects/status/bqxani5j890cufgm?svg=true)](https://ci.appveyor.com/project/dlebansais/csharplatest)
[![codecov](https://codecov.io/gh/dlebansais/CSharpLatest/graph/badge.svg?token=oFfnLirFJg)](https://codecov.io/gh/dlebansais/CSharpLatest)
[![CodeFactor](https://www.codefactor.io/repository/github/dlebansais/csharplatest/badge)](https://www.codefactor.io/repository/github/dlebansais/csharplatest)
[![NuGet](https://img.shields.io/nuget/v/dlebansais.CSharpLatest.svg)](https://www.nuget.org/packages/dlebansais.CSharpLatest)

## How to install

To install this analyzer, in Visual Studio:

+ Open menu `Tools` -> `NuGet Package Manager` -> `Manage NuGet Packages for Solution`. The `NuGet - Solution` window appears.  
+ In the top right corner, make sure `Package Source` is selected to be either `nuget.org` or `All`.
+ Click the `Browse` tab and in the search prompt type `CSharpLatest`.
+ A list of packages appears, one one them called `dlebansais.CSharpLatest`.
+ Click to select this package and in the right pane check projects you want to be analyzed.
+ Click the `Install` button.

## How to uninstall

To uninstall this analyzer, in Visual Studio:

+ Open menu `Tools` -> `NuGet Package Manager` -> `Manage NuGet Packages for Solution`. The `NuGet - Solution` window appears.  
+ Click to select the `dlebansais.CSharpLatest` package and in the right pane uncheck projects you no longer want to be analyzed.
+ Click the `Uninstall` button.
 
## List of diagnostics

| Code                      | Diagnostic                                                       |
| ------------------------- | ---------------------------------------------------------------- |
| [CSL1000](doc/CSL1000.md) | Variables that are not modified should be made constants.        |
| [CSL1001](doc/CSL1001.md) | Use `is null` instead of `== null`                               |
| [CSL1002](doc/CSL1002.md) | Use `is not null` instead of `!= null`                           |
| [CSL1003](doc/CSL1003.md) | Consider using primary contructors to simplify your code.        |
| [CSL1004](doc/CSL1004.md) | Consider using records to simplify your code.                    |
| [CSL1005](doc/CSL1005.md) | Simplify one line getter.                                        |
| [CSL1006](doc/CSL1006.md) | Simplify one line setter.                                        |
| [CSL1007](doc/CSL1007.md) | Add missing braces.                                              |
| [CSL1008](doc/CSL1008.md) | Remove unnecessary braces.                                       |
| [CSL1009](doc/CSL1009.md) | `FieldBackedPropertyAttribute` is missing argument.              |
| [CSL1010](doc/CSL1010.md) | `init` accessor not supported in `FieldBackedPropertyAttribute`. |

### CSL1000: Variables that are not modified should be made constants

This diagnostic comes directly from the [Roslyn Analyzer Tutorial](https://learn.microsoft.com/en-us/visualstudio/extensibility/getting-started-with-roslyn-analyzers).

### CSL1001: Use `is null` instead of `== null`

Feature available since C# 7.0: [`is` operator](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/is)

### CSL1002: Use `is not null` instead of `!= null`

Feature available since C# 9: [logical patterns](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#logical-patterns)

### CSL1003: Consider using primary contructors to simplify your code

Feature available since C# 12: [primary constructors](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors). **Superseded by [IDE0290](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0290).**

### CSL1004: Consider using records to simplify your code

Feature available since C# 9: [records](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)

### CSL1005: Simplify one line getter

Feature available since C# 6 (and extended to accessors in 7): [expression body](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-operator#expression-body-definition). **Superseded by [IDE0025](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0025) and [IDE0027](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0027)**.

### CSL1006: Simplify one line setter

Feature available since C# 7: [expression body](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-operator#expression-body-definition). **Superseded by [IDE0027](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0027)**.

### CSL1007: Add missing braces and CSL1008: Remove unnecessary braces

Improves on rule [IDE0011](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0011).

### CSL1009 and CSL1010

The `FieldBackedProperty` attribute can be used in code conditionally compiled with and without C#13+ ('Preview') to emulate the functionality of the [`field` keyword](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/field-keyword).

