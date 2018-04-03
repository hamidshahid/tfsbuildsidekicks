// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TeamBuildUpdater.cs" company="Hamid SHahid">Copyright Hamid Shahid</copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace TFSBuildSideKicks
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The class contains functionality to update Team Builds.
    /// </summary>
    public class TeamBuildUpdater
    {
        /// <summary>
        /// The url for team project collection.
        /// </summary>
        private string teamProjectCollectionUrl;

        /// <summary>
        /// The team project name.
        /// </summary>
        private string teamProjectName;

        /// <summary>
        /// Initializes a new instance of the TeamBuildUpdater class.
        /// </summary>
        /// <param name="teamProjectCollectionUrl">The Team Project Collection Uri.</param>
        /// <param name="teamProjectName">The Team Project Name.</param>
        public TeamBuildUpdater(string teamProjectCollectionUrl, string teamProjectName)
        {
            this.teamProjectCollectionUrl = teamProjectCollectionUrl;
            this.teamProjectName = teamProjectName;
        }

        /// <summary>
        /// Adds a branch to the given build.
        /// </summary>
        /// <param name="buildDefinitionName">The build definition name.</param>
        /// <param name="path">The path where the build definition exists.</param>
        /// <param name="branchName">The name of the branch to add.</param>
        /// <returns>True if new branch added. False otherwise</returns>
        public bool AddBranchToBuild(string buildDefinitionName, string path, string branchName)
        {
            var id = this.GetBuildDefinitionId(buildDefinitionName, path).Result;
            var build = this.GetBuildDefinitionDetails(id).Result;
            return this.AddBranch(id, build, branchName);
        }

        /// <summary>
        /// Removes the given branch from the build
        /// </summary>
        /// <param name="buildDefinitionName">The build definition name.</param>
        /// <param name="path">The path where the build definition exists.</param>
        /// <param name="branchName">The name of the branch to add.</param>
        /// <returns>True if new branch added. False otherwise</returns>
        public bool RemoveBranchFromBuild(string buildDefinitionName, string path, string branchName)
        {
            var id = this.GetBuildDefinitionId(buildDefinitionName, path).Result;
            var build = this.GetBuildDefinitionDetails(id).Result;
            return this.RemoveBranch(id, build, branchName);
        }

        /// <summary>
        /// Returns Id of the given build definition.
        /// </summary>
        /// <param name="buildDefinitionName">The name of the build definition.</param>
        /// <param name="path">The path where the build exists.</param>
        /// <returns>The Id of the build definition</returns>
        public async Task<int> GetBuildDefinitionId(string buildDefinitionName, string path)
        {
            using (var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }))
            {
                string requestUrl = $"{new Uri(this.teamProjectCollectionUrl)}/{this.teamProjectName}/_apis/build/definitions?api-version=2.0&name={buildDefinitionName}&path={path}";
                var response = client.GetAsync(requestUrl).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching build definition Id. HTTP Code-{response.StatusCode}. Reason {response.ReasonPhrase}");
                }

                var result = await response.Content.ReadAsStringAsync();
                var definitions = JObject.Parse(result)["value"];
                if (definitions != null & definitions.HasValues)
                {
                    return int.Parse(definitions[0]["id"].ToString());
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the object containing the build details.
        /// </summary>
        /// <param name="buildDefinitionId">The Build Definition Id.</param>
        /// <returns>The build details.</returns>
        public async Task<JObject> GetBuildDefinitionDetails(int buildDefinitionId)
        {
            using (var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }))
            {
                string requestUrl = $"{new Uri(this.teamProjectCollectionUrl)}/{this.teamProjectName}/_apis/build/definitions/{buildDefinitionId}?api-version=2.0";
                var response = await client.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error fetching build definition. HTTP Code-{response.StatusCode}. Reason {response.ReasonPhrase}");
                }

                var result = response.Content.ReadAsStringAsync().Result;
                return JObject.Parse(result);
            }
        }

        /// <summary>
        /// Remove the given branch from the given build.
        /// </summary>
        /// <param name="buildDefinitionId">The id of the build definition.</param>
        /// <param name="build">The build object.</param>
        /// <param name="branchName">The name of the branch name.</param>
        /// <returns>True if branch was removed successfully. False otherwise</returns>
        private bool RemoveBranch(int buildDefinitionId, JObject build, string branchName)
        {
            var branchRemoved = false;
            if (build["triggers"] != null && build["triggers"][0] != null)
            {
                var branches = build["triggers"][0]["branchFilters"] as JArray;
                if (branches.HasValues && branches.Any(s => string.Compare($"+refs/heads/{branchName}", s.ToString(), false) == 0))
                {
                    var item = branches.FirstOrDefault(s => string.Compare($"+refs/heads/{branchName}", s.ToString(), false) == 0);
                    branches.Remove(item);
                    branchRemoved = true;
                }

                if (branches.Count == 0)
                {
                    build["triggers"] = null;
                }
            }

            if (branchRemoved)
            {
                build["comment"] = $"Branch {branchName} removed from triggers.";
                var json = JsonConvert.SerializeObject(build, Newtonsoft.Json.Formatting.Indented);
                using (var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }))
                {
                    string requestUrl = $"{new Uri(this.teamProjectCollectionUrl)}/{this.teamProjectName}/_apis/build/definitions/{buildDefinitionId}?api-version=2.2";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PutAsync(requestUrl, content).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error in updating build definition. HTTP Code-{response.StatusCode}. Reason {response.ReasonPhrase}");
                    }
                }
            }

            return branchRemoved;
        }

        /// <summary>
        /// Add Branch to the given build.
        /// </summary>
        /// <param name="buildDefinitionId">The id of the build definition.</param>
        /// <param name="build">The build object.</param>
        /// <param name="branchName">The name of the branch name.</param>
        /// <returns>True if new branch added. False otherwise</returns>
        private bool AddBranch(int buildDefinitionId, JObject build, string branchName)
        {
            if (build["triggers"] == null)
            {
                var newTrigger = new JObject
                {
                    { "branchFilters", new JArray() },
                    { "pathFilters", new JArray() },
                    { "batchChanges", false },
                    { "maxConcurrentBuildsPerBranch", 1 },
                    { "pollingInterval", 0 },
                    { "triggerType", "continuousIntegration" }
                };

                JArray triggers = new JArray();
                triggers.Add(newTrigger);
                build["triggers"] = triggers;
            }

            var branches = build["triggers"][0]["branchFilters"] as JArray;
            var branchAdded = false;
            if (branches != null)
            {
                if (!branches.HasValues || !branches.Any(s => string.Compare($"+refs/heads/{branchName}", s.ToString(), false) == 0))
                {
                    branches.Add($"+refs/heads/{branchName}");
                    branchAdded = true;
                }
            }

            if (branchAdded)
            {
                build["comment"] = $"Branch {branchName} added to triggers.";
                var json = JsonConvert.SerializeObject(build, Newtonsoft.Json.Formatting.Indented);
                using (var client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }))
                {
                    string requestUrl = $"{new Uri(this.teamProjectCollectionUrl)}/{this.teamProjectName}/_apis/build/definitions/{buildDefinitionId}?api-version=2.2";
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PutAsync(requestUrl, content).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error in updating build definition. HTTP Code-{response.StatusCode}. Reason {response.ReasonPhrase}");
                    }
                }
            }

            return branchAdded;
        }
    }
}