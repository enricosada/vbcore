# How to

- `set DOTNET_VBC_PATH=c:\Program Files (x86)\MSBuild\14.0\Bin\vbc.exe`
- `set DOTNET_VBC_EXEC=RUN`

And now let's build .net core app, with the .NET Core Sdk

- Skip the `dotnet new`; to being a project, you need the project.json and Moldule1.vb in the current folder. 

- `dotnet restore`
- `dotnet build`
- `dotnet run`
- `dotnet pack`
- `dotnet publish`

