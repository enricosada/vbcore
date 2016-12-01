# netcorecli-vb

To get started, make sure that you have the current release of .NET Core installed.

To make things simple, open MSBuild Command Prompt for Visual Studio 2015 (CMD) prompt.

The DOTNET_VBC_PATH location can be found by typing:

- 'C:\>where vbc.exe

The one we care about is the one that contains the folder MSBuild.  The "Framework" one is "outdated" and WILL NOT WORK.
According to someone on the MS team, "it exists strictly for backward compatibility."

Verify that the following portion in the build.cmd script matches your result...

- `SET DOTNET_VBC_PATH=c:\Program Files (x86)\MSBuild\14.0\Bin\vbc.exe`
- `SET DOTNET_VBC_EXEC=RUN`

Now begin the build process by using the "build.cmd" script.

If everything goes as expected, the final output of this script should show "Hello World!"

I've included a "clean.cmd" script to make it simple to "reset".

Now that the path has been specified and this "build.cmd" script has completed; you can now begin building
.NET Core apps in VB.

In order to not have to use the "build.cmd" script each time; in other words, to be able to being a .NET Core
application as you desire, you will need to add the above two variables to "Advanced System Settings" "Environment Variables".

In Windows 10...

- Right click "This PC".
- Properties
- Advanced system Settings
- Environment Variables
- Add these two variables and values to the System variables list using the New button. 

Currently the "dotnet new" functionality is not working; however, it's pretty simple to accomplish the same
outcome... just copy the two files in the "console" folder to a new folder.

And now let's build .net core app, with the .NET Core Sdk

- `dotnet restore`
- `dotnet build`
- `dotnet run`
- `dotnet pack`
- `dotnet publish`