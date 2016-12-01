# How to

Assuming you've followed all of the instructions for getting the compiler side working...

Currently the "dotnet new" functionality is not working; however, it's pretty simple to accomplish the same
outcome... just copy the two files in the "console" folder to a new folder.

And now let's build .net core app, with the .NET Core

- Skip the `dotnet new`; to being a project, you need the project.json and Moldule1.vb in the current folder. 

- `dotnet restore`
- `dotnet build`
- `dotnet run`
- `dotnet pack`
- `dotnet publish`