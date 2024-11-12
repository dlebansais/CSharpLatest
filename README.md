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

| Code                      | Diagnostic                                                |
| ------------------------- | --------------------------------------------------------- |
| [CSL1000](doc/CSL1000.md) | Variables that are not modified should be made constants. |
| [CSL1001](doc/CSL1001.md) | Use `is null` instead of `== null`                        |
| [CSL1002](doc/CSL1002.md) | Use `is not null` instead of `!= null`                    |
| [CSL1003](doc/CSL1003.md) | Consider using primary contructors to simplify your code. |
| [CSL1004](doc/CSL1004.md) | Consider using records to simplify your code.             |
| [CSL1005](doc/CSL1005.md) | Simplify one line getter.                                 |
| [CSL1006](doc/CSL1006.md) | Simplify one line setter.                                 |
