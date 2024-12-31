@echo off

setlocal

call ..\Certification\set_tokens.bat

set PROJECTNAME=CSharpLatest
set TOKEN=%CSHARP_LATEST_CODECOV_TOKEN%
set TESTPROJECTNAME1=%PROJECTNAME%.Test
set TESTPROJECTNAME2=%PROJECTNAME%.PropertyGenerator.Test
set PLATFORM=x64
set CONFIGURATION=Release
set FRAMEWORK=net8.0
set RESULTFILENAME=Coverage-%PROJECTNAME%.xml
set RESULTFILEPATH=".\Test\%TESTPROJECTNAME1%\bin\x64\%CONFIGURATION%\%FRAMEWORK%\%RESULTFILENAME%"

set OPENCOVER_VERSION=4.7.1221
set OPENCOVER=OpenCover.%OPENCOVER_VERSION%
set OPENCOVER_EXE=".\packages\%OPENCOVER%\tools\OpenCover.Console.exe"

set CODECOV_UPLOADER_VERSION=0.7.2
set CODECOV_UPLOADER=CodecovUploader.%CODECOV_UPLOADER_VERSION%
set CODECOV_UPLOADER_EXE=".\packages\%CODECOV_UPLOADER%\tools\codecov.exe"

set REPORTGENERATOR_VERSION=5.2.0
set REPORTGENERATOR=ReportGenerator.%REPORTGENERATOR_VERSION%
set REPORTGENERATOR_EXE=".\packages\%REPORTGENERATOR%\tools\net8.0\ReportGenerator.exe"

nuget install OpenCover -Version %OPENCOVER_VERSION% -OutputDirectory packages
nuget install CodecovUploader -Version %CODECOV_UPLOADER_VERSION% -OutputDirectory packages
nuget install ReportGenerator -Version %REPORTGENERATOR_VERSION% -OutputDirectory packages

if '%TOKEN%' == '' goto error_console1
if not exist %OPENCOVER_EXE% goto error_console2
if not exist %CODECOV_UPLOADER_EXE% goto error_console3
if not exist %REPORTGENERATOR_EXE% goto error_console4

if exist ".\Test\%TESTPROJECTNAME1%\publish" rd /S /Q ".\Test\%TESTPROJECTNAME1%\publish"
if exist ".\Test\%TESTPROJECTNAME2%\publish" rd /S /Q ".\Test\%TESTPROJECTNAME2%\publish"

dotnet build ./Test/%TESTPROJECTNAME1% "/p:Platform=%PLATFORM%" -c %CONFIGURATION% -f %FRAMEWORK%
dotnet build ./Test/%TESTPROJECTNAME2% "/p:Platform=%PLATFORM%" -c %CONFIGURATION% -f %FRAMEWORK%

if exist .\Test\%TESTPROJECTNAME1%\*.log del .\Test\%TESTPROJECTNAME1%\*.log
if exist .\Test\%TESTPROJECTNAME2%\*.log del .\Test\%TESTPROJECTNAME2%\*.log
if exist %RESULTFILEPATH% del %RESULTFILEPATH%

rem Execute tests within OpenCover.
echo %RESULTFILEPATH%
%OPENCOVER_EXE% -register:user -target:"C:\Program Files\dotnet\dotnet.exe" -targetargs:"test .\Test\%TESTPROJECTNAME1%\bin\x64\%CONFIGURATION%\%FRAMEWORK%\%TESTPROJECTNAME1%.dll -l console;verbosity=detailed" "-filter:+[*]* -[%TESTPROJECTNAME1%*]*" -output:%RESULTFILEPATH% -mergeoutput
%OPENCOVER_EXE% -register:user -target:"C:\Program Files\dotnet\dotnet.exe" -targetargs:"test .\Test\%TESTPROJECTNAME2%\bin\x64\%CONFIGURATION%\%FRAMEWORK%\%TESTPROJECTNAME2%.dll -l console;verbosity=detailed" "-filter:+[*]* -[%TESTPROJECTNAME2%*]*" -output:%RESULTFILEPATH% -mergeoutput

if not exist %RESULTFILEPATH% goto end
%CODECOV_UPLOADER_EXE% -f %RESULTFILEPATH% -t %TOKEN%
%REPORTGENERATOR_EXE% -reports:%RESULTFILEPATH% -targetdir:.\CoverageReports "-assemblyfilters:+%PROJECTNAME%.*" "-filefilters:-*.g.cs;-*.Designer.cs;-*Microsoft.NET.Test.Sdk.Program.cs"
goto end

:error_console1
echo ERROR: CodeCov token not set.
goto end

:error_console2
echo ERROR: OpenCover.Console not found.
goto end

:error_console3
echo ERROR: Codecov not found.
goto end

:error_console4
echo ERROR: ReportGenerator not found.
goto end

:end
if exist *.log del *.log
if exist *Result*.xml del *Result*.xml
