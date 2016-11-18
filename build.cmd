@ECHO OFF

REM pushd

REM pack dotnet-compile-vbc tool
cd src\dotnet-compile-vbc

@ECHO ON
dotnet restore -v Warning
dotnet pack

@ECHO OFF

REM -- The following has been removed since we now successfully can build with the real (Windows) compiler.
REM REM create an example vbc compiler :D
REM cd ..\vbc\
REM dotnet restore
REM dotnet build
REM REM set the path of vbc, so dotnet-compile-vbc can know where is.
REM set DOTNET_VBC_PATH=%CD%\bin\Debug\netcoreapp1.0\vbc.dll
REM REM like that is `dotnet %DOTNET_VBC_PATH%`, without is run as `%DOTNET_VBC_PATH%`
REM REM useful for run vbc.exe :D
REM set DOTNET_VBC_EXEC=COREHOST

REM build a vb console app, with example vbc
REM the restore -f it's to use the just created package
REM cd ..\console
REM dotnet restore -f ..\dotnet-compile-vbc\bin\Debug
REM dotnet -v build

REM build the vb console app with normal vbc.exe
REM will fail, but is ok <--- This is no longer the case... it now works.
REM It is important to note that the DOTNET_VBC_PATH *MUST* be the one in the MSBuild folder, not the Framework
REM as the Framework one is way outdated.
REM set DOTNET_VBC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\vbc.exe
set DOTNET_VBC_PATH=c:\Program Files (x86)\MSBuild\14.0\Bin\vbc.exe
set DOTNET_VBC_EXEC=RUN
REM dotnet -v build

cd ..\console

@ECHO ON

dotnet restore -v Warning
REM dotnet build
dotnet run

@ECHO OFF

cd ..\..\

REM popd

REM done
