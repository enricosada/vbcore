# dotnet-compile-vbc

In order to get a workable solution, the approach has been to utilize
the existing (known) vbc.exe that comes with Visual Studio 2015 (Roslyn).

This means that this only works on Windows (at this point)... however, the results
are able to be cross-platform deployed.

The goal is to wire directly with the vbc.exe that is (or at least should be)
included with .NET Core; I know that it exists, but only saw it in passing. ;-)

How does this work?

The dotnet command can be extended by adding additional commands as libraries in 
a particular folder.  So when dotnet attempts to do the build process, this library will
be called because of how the following line in the project.json file is handled:

    "compilerName": "vbc",

The vbc portion is appended to dotnet-compile- by the dotnet executable to produce dotnet-compile-vbc and
dotnet will then look in this folder for a library of that name.

This library then sets up the process for the code to be compiled using the vbc.exe compiler.