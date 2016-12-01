@ECHO OFF

SET DOTNET_VBC_PATH=c:\Program Files (x86)\MSBuild\14.0\Bin\vbc.exe
SET DOTNET_VBC_EXEC=RUN

@ECHO ON

CD src\dotnet-compile-vbc
dotnet restore -v Warning
dotnet pack

CD ..\console
dotnet restore -v Warning
dotnet run

@ECHO OFF

CD ..\..\