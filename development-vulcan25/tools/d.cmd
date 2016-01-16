@echo off

if /i "%1" EQU ""       goto end
if /i %1 EQU exec       goto dtexec
if /i %1 EQU execx86	goto dtexecx86
if /i %1 EQU b          goto build
if /i %1 EQU bx86       goto buildx86
if /i %1 EQU msb        goto msb
if /i %1 EQU msb64      goto msb64
if /i %1 EQU updateXSD  goto updateXSD

goto push


:: --------------------------------------------------
:updateXSD
COPY "%VULCANENGINE_PATH%\xsd\vulcan2.xsd" "%VSINSTALLDIR%\Xml\Schemas\"
goto end:


:: --------------------------------------------------
:dtexec
dtexec /file %2
goto end:

:: --------------------------------------------------
:dtexecx86
"%ProgramFiles(x86)%\Microsoft SQL Server\100\DTS\Binn\dtexec.exe" /file %2
goto end:

:: --------------------------------------------------
:build
msbuild.exe /t:BuildFile /p:TargetFile=%2
goto end:

:: --------------------------------------------------
:buildx86
%VULCANENGINE86_PATH%\vulcan %2 -t %VULCAN_ROOT%\bin -r VULCAN=%VULCAN_ROOT%
goto end:

:: --------------------------------------------------
:msb
msbuild.exe %2 %3 %4 %5 %6 %7 %8 %9
goto end:

:: --------------------------------------------------
:msb64
%windir%\Microsoft.Net\Framework64\v3.5\msbuild.exe %2 %3 %4 %5 %6 %7 %8 %9
goto end:

:: --------------------------------------------------
:push
pushd %VULCAN_ROOT%\%1
goto end

:: --------------------------------------------------
:end
