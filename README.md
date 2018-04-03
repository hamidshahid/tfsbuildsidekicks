# TFS Build Sidekicks

TFS Builds aka Build VNext provides can be created and edited using TFS's web user interface. This is fine for cross platform editing, however, there were certain things that we were able to do for builds using tools like "Community TFS Build Manager", which are not possible anymore. 

I found it quite peeeving that I could not edit multiple builds at the same time (e.g. Adding/Remove a branch to multiple builds, changing 
Default agent queue, adding/removing a demand, etc.), so decided to create this project.

The project is built in Microsoft .Net core and would run in any platform supported by .net core. 


Usage Information:

`dotnet TFSBuildSideKicks.dll --teamprojectcollection <team_project_collection_url> --teamproject <team_project_name> --action [ addbranch | removebranch] [--branch <branchName> --build <build_defintion_name>]`


### AddBranch
The "addbranch" action adds a branch to the  continuous integration trigger for the given build. For Example:

`dotnet TFSBuildSideKicks.dll --teamprojectcollection http://mytfs.domain:8080/tfs/tpc --teamproject myteamproject --action addbranch --branch releases/master --build release-ci`

### RemoveBranch
The "removebranch" action removes  a branch form the  continuous integration trigger for the given build. For Example:

`dotnet TFSBuildSideKicks.dll --teamprojectcollection http://mytfs.domain:8080/tfs/tpc --teamproject myteamproject --action removebranch  --branch releases/master --build release-ci`
