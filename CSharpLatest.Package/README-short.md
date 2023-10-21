# CSharpLatest

Roslyn-based analysis of C# code to suggest the use of features in the latest version of the language.

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

| Code    | Diagnostic                                                |
| ------- | --------------------------------------------------------- |
| CSL0000 | Variables that are not modified should be made constants. |
| CSL0001 | Use `is null` instead of `== null`                        |
