// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Compiler.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;

namespace Microsoft.DotNet.Tools.Compiler.Vbc
{
    public class CompileVbcCommand
    {
        private const int ExitFailed = 1;

        public static int Main(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            CommandLineApplication app = new CommandLineApplication();
            app.Name = "dotnet compile-vbc";
            app.FullName = ".NET VB Compiler";
            app.Description = "VB Compiler for the .NET Platform";
            app.HandleResponseFiles = true;
            app.HelpOption("-h|--help");

            CommonCompilerOptionsCommandLine commonCompilerCommandLine = CommonCompilerOptionsCommandLine.AddOptions(app);
            AssemblyInfoOptionsCommandLine assemblyInfoCommandLine = AssemblyInfoOptionsCommandLine.AddOptions(app);

            CommandOption tempOutput = app.Option("--temp-output <arg>", "Compilation temporary directory", CommandOptionType.SingleValue);
            CommandOption outputName = app.Option("--out <arg>", "Name of the output assembly", CommandOptionType.SingleValue);
            CommandOption references = app.Option("--reference <arg>...", "Path to a compiler metadata reference", CommandOptionType.MultipleValue);
            CommandOption analyzers = app.Option("--analyzer <arg>...", "Path to an analyzer assembly", CommandOptionType.MultipleValue);
            CommandOption resources = app.Option("--resource <arg>...", "Resources to embed", CommandOptionType.MultipleValue);
            CommandArgument sources = app.Argument("<source-files>...", "Compilation sources", multipleValues: true);

            app.OnExecute(() =>
            {
                if (!tempOutput.HasValue())
                {
                    Reporter.Error.WriteLine("Option '--temp-output' is required");
                    return ExitFailed;
                }

                CommonCompilerOptions commonOptions = commonCompilerCommandLine.GetOptionValues();

                AssemblyInfoOptions assemblyInfoOptions = assemblyInfoCommandLine.GetOptionValues();

                var translated = TranslateCommonOptions(commonOptions, outputName.Value());

                var allArgs = new List<string>(translated);
                allArgs.AddRange(GetDefaultOptions());

                // Generate assembly info
                /*DISABLED BECAUSE AssemblyInfoGenerator doesnt undertstand vb
                 * just add something like https://github.com/dotnet/cli/blob/04f40f906dce2678d80fb9787e68de76ee6bf57e/src/Microsoft.DotNet.Compiler.Common/AssemblyInfoFileGenerator.cs#L19

                var assemblyInfo = Path.Combine(tempOutput.Value(), $"dotnet-compile.assemblyinfo.vb");

                File.WriteAllText(assemblyInfo, AssemblyInfoFileGenerator.GenerateCSharp(assemblyInfoOptions, sources.Values));

                allArgs.Add($"\"{assemblyInfo}\"");

                */

                if (outputName.HasValue())
                {
                    allArgs.Add($"-out:\"{outputName.Value()}\"");
                }

                allArgs.AddRange(analyzers.Values.Select(a => $"-a:\"{a}\""));
                allArgs.AddRange(references.Values.Select(r => $"-r:\"{r}\""));

                // Resource has two parts separated by a comma
                // Only the first should be quoted. This is handled
                // in dotnet-compile where the information is present.
                allArgs.AddRange(resources.Values.Select(resource => $"-resource:{resource}"));

                allArgs.AddRange(sources.Values.Select(s => $"\"{s}\""));

                var rsp = Path.Combine(tempOutput.Value(), "dotnet-compile-vbc.rsp");

                File.WriteAllLines(rsp, allArgs, Encoding.UTF8);

                // Execute VBC!
                var result = RunVbc(new string[] { $"-noconfig", "@" + $"{rsp}" })
                    .WorkingDirectory(Directory.GetCurrentDirectory())
                    .ForwardStdErr()
                    .ForwardStdOut()
                    .Execute();

                // RENAME things
                // This is currently necessary as the BUILD task of the dotnet executable expects (at this stage) a .DLL (not an EXE).
                var outExe = outputName.Value();
                outExe = outExe + ".exe";
                if (!string.IsNullOrEmpty(outExe) && File.Exists(outExe))
                {
                    string outDll = outExe.Replace(".dll.exe", ".dll");
                    if (File.Exists(outDll))
                    {
                        File.Delete(outDll);
                    }
                    System.IO.File.Move(outExe, outDll);

                    string outPdbOrig = outExe.Replace(".dll.exe", ".dll.pdb");
                    string outPdb = outPdbOrig.Replace(".dll.pdb", ".pdb");
                    if (File.Exists(outPdb))
                    {
                        File.Delete(outPdb);
                    }
                    System.IO.File.Move(outPdbOrig, outPdb);
                }

                return result.ExitCode;
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
#if DEBUG
                Reporter.Error.WriteLine(ex.ToString());
#else
                Reporter.Error.WriteLine(ex.Message);
#endif
                return ExitFailed;
            }
        }

        // TODO: Review if this is the place for default options
        private static IEnumerable<string> GetDefaultOptions()
        {
            var args = new List<string>()
            {
                "-nostdlib",
                "-nologo",
            };

            return args;
        }

        private static IEnumerable<string> TranslateCommonOptions(CommonCompilerOptions options, string outputName)
        {
            List<string> commonArgs = new List<string>();

            if (options.Defines != null)
            {
                commonArgs.AddRange(options.Defines.Select(def => $"-d:{def}")); // short for -define:
            }

            if (options.SuppressWarnings != null)
            {
                commonArgs.AddRange(options.SuppressWarnings.Select(w => $"-nowarn:{w}"));
            }

            // Additional arguments are added verbatim
            if (options.AdditionalArguments != null)
            {
                commonArgs.AddRange(options.AdditionalArguments);
            }

            if (options.LanguageVersion != null)
            {
                commonArgs.Add($"-langversion:{GetLanguageVersion(options.LanguageVersion)}");
            }

            if (options.Platform != null)
            {
                commonArgs.Add($"-platform:{options.Platform}");
            }

            /* // The following is not available in VBC.
            if (options.AllowUnsafe == true)
            {
                commonArgs.Add("-unsafe");
            }
            */

            if (options.WarningsAsErrors == true)
            {
                commonArgs.Add("-warnaserror");
            }

            if (options.Optimize == true)
            {
                commonArgs.Add("-optimize");
            }

            if (options.KeyFile != null)
            {
                commonArgs.Add($"-keyfile:\"{options.KeyFile}\"");

                // If we're not on Windows, full signing isn't supported, so we'll
                // public sign, unless the public sign switch has explicitly been
                // set to false
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                    options.PublicSign == null)
                {
                    commonArgs.Add("-publicsign");
                }
            }

            if (options.DelaySign == true)
            {
                commonArgs.Add("-delaysign");
            }

            commonArgs.Add("-vbruntime*"); // This appears to be necessary; without or other usage fails.

            if (options.PublicSign == true)
            {
                commonArgs.Add("-publicsign");
            }

            if (options.GenerateXmlDocumentation == true)
            {
                commonArgs.Add($"-doc:\"{Path.ChangeExtension(outputName, "xml")}\"");
            }

            if (options.EmitEntryPoint != true)
            {
                commonArgs.Add("-t:library"); // short for -target:
            }

            if (string.IsNullOrEmpty(options.DebugType))
            {
                commonArgs.Add(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "-debug:full"
                    : "-debug:portable");
            }
            else
            {
                commonArgs.Add(options.DebugType == "portable"
                    ? "-debug:portable"
                    : "-debug:full");
            }

            return commonArgs;
        }

        private static string GetLanguageVersion(string languageVersion)
        {

//TODO: Not sure how this should be translated to VB... guessing just "vb" instead of the original "csharp".

            // project.json supports the enum that the roslyn APIs expose
            if (languageVersion?.StartsWith("vb", StringComparison.OrdinalIgnoreCase) == true)
            {
                // We'll be left with the number csharp6 = 6
                return languageVersion.Substring("vb".Length);
            }
            return languageVersion;
        }

        private static Command RunVbc(string[] vbcArgs)
        {

//TODO: Need to utilize the .NET Core vbc.exe...
// For now, we are using the .NET Framework vbc.exe instead of the .NET Core vbc.exe as we are not yet sure where that is located/available.

            var vbcEnvExe = Environment.GetEnvironmentVariable("DOTNET_VBC_PATH");
            var exec = Environment.GetEnvironmentVariable("DOTNET_VBC_EXEC")?.ToUpper() ?? "COREHOST";
            
            var muxer = new Muxer();

            if (vbcEnvExe == null)
            {
                Reporter.Error.WriteLine("Env var DOTNET_VBC_PATH is required");
                Reporter.Error.WriteLine("DOTNET_VBC_PATH=path/to/vbc.dll");
                Reporter.Error.WriteLine("DOTNET_VBC_EXEC if value is COREHOST it's run like 'dotnet '%DOTNET_VBC_PATH%', otherwise just '%DOTNET_VBC_PATH%'");
                throw new Exception("cannot locate vbc");
            }
            else
            {
                switch (exec)
                {
                    case "RUN":
                        return Command.Create(vbcEnvExe, vbcArgs.ToArray());

                    case "COREHOST":
                    default:
                        var host = muxer.MuxerPath;
                        return Command.Create(host, new[] { vbcEnvExe }.Concat(vbcArgs).ToArray());
                }
            }
        }
    }
}

