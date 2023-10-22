@echo off

setlocal

set PROJECTNAME=CSharpLatest
set TESTPROJECTNAME=%PROJECTNAME%.Test
set RESULTFILENAME=Coverage-%PROJECTNAME%.xml
set OPENCOVER_VERSION=4.7.1189
set OPENCOVER=OpenCover.%OPENCOVER_VERSION%
set CODECOV_VERSION=1.12.2
set CODECOV=Codecov.%CODECOV_VERSION%
set NUINT_CONSOLE_VERSION=3.11.1
set NUINT_CONSOLE=NUnit.ConsoleRunner.%NUINT_CONSOLE_VERSION%
set FRAMEWORK=netcoreapp3.1

nuget install OpenCover -Version %OPENCOVER_VERSION% -OutputDirectory packages
nuget install CodeCov -Version %CODECOV_VERSION% -OutputDirectory packages
nuget install NUnit.ConsoleRunner -Version %NUINT_CONSOLE_VERSION% -OutputDirectory packages

if not exist ".\packages\%OPENCOVER%\tools\OpenCover.Console.exe" goto error_console1
if not exist ".\packages\%CODECOV%\tools\codecov.exe" goto error_console2
if not exist ".\packages\%NUINT_CONSOLE%\tools\nunit3-console.exe" goto error_console3

call ..\Certification\set_tokens.bat

if exist ".\Test\%TESTPROJECTNAME%\publish" rd /S /Q ".\Test\%TESTPROJECTNAME%\publish"

rem dotnet build ./%PROJECTNAME% -c Debug -f %FRAMEWORK% /p:Platform=x64
dotnet build ./Test/%TESTPROJECTNAME% -c Debug -f %FRAMEWORK%
rem dotnet publish ./Test/%TESTPROJECTNAME% -c Release -f %FRAMEWORK% /p:Platform=x64 -o ./Test/%TESTPROJECTNAME%/publish/x64/Release

rem if not exist ".\Test\%TESTPROJECTNAME%\publish\x64\Debug\%TESTPROJECTNAME%.dll" goto error_not_built
rem if not exist ".\Test\%TESTPROJECTNAME%\publish\x64\Release\%TESTPROJECTNAME%.dll" goto error_not_built
rem copy ".\%PROJECTNAME%\bin\x64\Debug\%FRAMEWORK%\%PROJECTNAME%.dll" ".\Test\%TESTPROJECTNAME%\publish\x64\Debug\"
rem copy ".\%PROJECTNAME%\bin\x64\Debug\%FRAMEWORK%\%PROJECTNAME%.pdb" ".\Test\%TESTPROJECTNAME%\publish\x64\Debug\"
rem copy ".\%PROJECTNAME%\bin\x64\Release\%FRAMEWORK%\%PROJECTNAME%.dll" ".\Test\%TESTPROJECTNAME%\publish\x64\Release\"
rem copy ".\%PROJECTNAME%\bin\x64\Release\%FRAMEWORK%\%PROJECTNAME%.pdb" ".\Test\%TESTPROJECTNAME%\publish\x64\Release\"

if exist .\Test\%TESTPROJECTNAME%\*.log del .\Test\%TESTPROJECTNAME%\*.log
if exist .\Test\%TESTPROJECTNAME%\obj\x64\Debug\%RESULTFILENAME% del .\Test\%TESTPROJECTNAME%\obj\x64\Debug\%RESULTFILENAME%
rem if exist .\Test\%TESTPROJECTNAME%\obj\x64\Release\%RESULTFILENAME% del .\Test\%TESTPROJECTNAME%\obj\x64\Release\%RESULTFILENAME%
".\packages\%OPENCOVER%\tools\OpenCover.Console.exe" -register:user -target:"C:\Program Files\dotnet\dotnet.exe" -targetargs:"test .\Test\%TESTPROJECTNAME%\bin\Debug\%FRAMEWORK%\%TESTPROJECTNAME%.dll -l console;verbosity=detailed" "-filter:+[*]* -[%TESTPROJECTNAME%*]*" -output:".\Test\%TESTPROJECTNAME%\obj\Debug\%RESULTFILENAME%"
rem ".\packages\%OPENCOVER%\tools\OpenCover.Console.exe" -register:user -target:".\packages\%NUINT_CONSOLE%\tools\nunit3-console.exe" -targetargs:".\Test\%TESTPROJECTNAME%\bin\x64\Release\%FRAMEWORK%\%TESTPROJECTNAME%.dll --trace=Debug --labels=Before" -filter:"+[%PROJECTNAME%*]* -[%TESTPROJECTNAME%*]*" -output:".\Test\%TESTPROJECTNAME%\obj\x64\Release\%RESULTFILENAME%"
if exist .\Test\%TESTPROJECTNAME%\obj\Debug\%RESULTFILENAME% .\packages\%CODECOV%\tools\codecov -f ".\Test\%TESTPROJECTNAME%\obj\Debug\%RESULTFILENAME%" -t %CSHARP_LATEST_CODECOV_TOKEN%
rem if exist .\Test\%TESTPROJECTNAME%\obj\x64\Release\%RESULTFILENAME% .\packages\%CODECOV%\tools\codecov -f ".\Test\%TESTPROJECTNAME%\obj\x64\Release\%RESULTFILENAME%" -t %MPFRDOTNET_CODECOV_TOKEN%
goto end

:error_console1
echo ERROR: OpenCover.Console not found.
goto end

:error_console2
echo ERROR: Codecov not found.
goto end

:error_console3
echo ERROR: nunit3-console not found.
goto end

:error_not_built
echo ERROR: %TESTPROJECTNAME%.dll not built (both Debug and Release are required).
goto end

:end
