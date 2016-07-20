pushd

REM pack dotnet-compile-vbc tool
cd src\dotnet-compile-vbc
dotnet restore
dotnet pack

REM create an example vbc compiler :D
cd ..\vbc\
dotnet restore
dotnet build
REM set the path of vbc, so dotnet-compile-vbc can know where is.
set DOTNET_VBC_PATH=%CD%\bin\Debug\netcoreapp1.0\vbc.dll
REM like that is `dotnet %DOTNET_VBC_PATH%`, without is run as `%DOTNET_VBC_PATH%`
REM useful for run vbc.exe :D
set DOTNET_VBC_EXEC=COREHOST

REM build a vb console app, with example vbc
REM the restore -f it's to use the just created package
cd ..\console
dotnet restore -f ..\dotnet-compile-vbc\bin\Debug
dotnet -v build

REM build the vb console app with normal vbc.exe
REM will fail, but is ok
set DOTNET_VBC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\vbc.exe
set DOTNET_VBC_EXEC=RUN
dotnet -v build


REM done
popd
