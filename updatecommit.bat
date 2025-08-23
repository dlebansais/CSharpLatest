if exist "%~1packages\dlebansais.GitCommitId*" goto run1
if exist "%~1..\Version Tools\GitCommitId.exe" goto run2
goto error

:run1
echo Updating Commit Id.
for /D %%F in (%~1packages\dlebansais.GitCommitId*) do "%%F\lib\net481\GitCommitId.exe" %2 -u
goto end

:run2
"%~1..\Version Tools\GitCommitId.exe" %2 -u
goto end

:error
echo Failed to update Commit Id.
goto end

:end
