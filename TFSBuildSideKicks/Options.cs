// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Options.cs" company="Hamid SHahid">Copyright Hamid Shahid</copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace TFSBuildSideKicks
{
    using CommandLine;

    /// <summary>
    /// THe class represents all the command line options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets the Team Project Collection
        /// </summary>
        [Option('c', "teamprojectcollection", Required = true, HelpText = "The url for teamproject colleciton")]
        public string TeamProjectCollection { get; set; }

        /// <summary>
        /// Gets or sets the Team Project
        /// </summary>
        [Option('t', "teamproject", Required = true, HelpText = "The name of the team project")]
        public string TeamProject { get; set; }

        /// <summary>
        /// Gets or sets the Action
        /// </summary>
        [Option('a', "action", Required = true, HelpText = "Possible Value addbranch")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the Branch
        /// </summary>
        [Option('r', "branch", HelpText = "The name of branch")]
        public string Branch { get; set; }

        /// <summary>
        /// Gets or sets the Build Definition Name
        /// </summary>
        [Option('b', "build", HelpText = "The name of the build")]
        public string BuildDefinition { get; set; }

        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        [Option('p', "path", HelpText = "The folder path where the build is located")]
        public string Path { get; set; }
    }
}
