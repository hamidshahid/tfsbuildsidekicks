// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hamid Shahid">Copyright Hamid Shahid</copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace TFSBuildSideKicks
{
    using System;
    using System.Collections.Generic;
    using CommandLine;

    /// <summary>
    /// The class containing the main method.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The program exit code
        /// </summary>
        private static int exitCode = 0;

        /// <summary>
        /// The main method
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Returns 0 if successful, -1 otherwise</returns>
        public static int Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => ProcessBuildUpdates(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));

            return exitCode;
        }

        /// <summary>
        /// Writes to console
        /// </summary>
        /// <param name="message">The message to write</param>
        private static void Log(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// The method processes build updates using the given updates
        /// </summary>
        /// <param name="options">The options</param>
        /// <returns>Successful = 0, Failure = Non-Zero</returns>
        private static int ProcessBuildUpdates(Options options)
        {
            switch (options.Action.ToLowerInvariant())
            {
                case "addbranch":
                    try
                    {
                        var updater = new TeamBuildUpdater(options.TeamProjectCollection, options.TeamProject);
                        var branchAdeed = updater.AddBranchToBuild(options.BuildDefinition, options.Path, options.Branch);
                        if (branchAdeed)
                        {
                            Log($"Successfully added the branch {options.Branch} in the build triggers of build definition {options.BuildDefinition}.");
                        }
                        else
                        {
                            Log($"The branch {options.Branch} already exists in the build trigger of build definition {options.BuildDefinition}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Unable to add branch to build definition. Error {ex.Message}");
                        return -1;
                    }

                    break;
                case "removebranch":
                    try
                    {
                        var updater = new TeamBuildUpdater(options.TeamProjectCollection, options.TeamProject);
                        var branchRemoved = updater.RemoveBranchFromBuild(options.BuildDefinition, options.Path, options.Branch);
                        if (branchRemoved)
                        {
                            Log($"Successfully removed the branch {options.Branch} from the triggers of build definition {options.BuildDefinition}.");
                        }
                        else
                        {
                            Log($"The branch {options.Branch} does not exist in the build trigger of build definition {options.BuildDefinition}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Unable to add branch to build definition. Error {ex.Message}");
                        return -1;
                    }

                    break;
            }

            return 0;
        }

        /// <summary>
        /// Handles error in command line parameters
        /// </summary>
        /// <param name="errs">The enumeration of errors</param>
        private static void HandleParseError(IEnumerable<Error> errs)
        {
            exitCode = 0;
        }
    }
}
