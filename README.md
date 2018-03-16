# .NET Core console app template

App contains setup for:
1. [Dependency](https://msdn.microsoft.com/en-us/magazine/mt707534.aspx) injection container
1. [Configuration](https://msdn.microsoft.com/en-us/magazine/mt632279.aspx) (config.json + environment variables + command line arguments bounded to POCO classes delivered through dependecy injection)
1. [Logging](https://msdn.microsoft.com/en-us/magazine/mt694089.aspx) to console and file
1. [App metrics](https://www.app-metrics.io/getting-started/reservoir-sampling/) bootstrap
1. [RabbitMQ consumer](http://easynetq.com/) to illustrate long running process hosted within the sample app.
1. Using [Cake](https://cakebuild.net/) to unify build process on local dev machine and CI server.

## Working with Visual Studio
All common actions should work (build, rebuild, debug, run) without any additional steps needed.

## Working with Visual Studio Code
build.ps1 allows for following build targets:

`.\build.ps1 -Target Clean`<br/>
Deletes bin, obj and other build artefacts.

`.\build.ps1 -Target Restore`<br/>
Restore nuget packages.

`.\build.ps1 -Target Build`<br/>
Compiles the projects using MsBuild.

`.\build.ps1 -Target ReBuild`<br/>
Runs Clean, Restore and Build targets.

`.\build.ps1 -Target Test`<br/>
Runs *Clean, Restore, Build* targets and than executes all xunit test assemblies.

`.\build.ps1 -Target Pack` (default target if none provided)<br/>
Runs *Clean, Restore, Build, Test* targets and than packs relevant libraries as nuget packages to be published in .\artifacts directory.

Above commands are available in VS Code as build tasks (Ctrl+Shift+B) and debug main app using F5.

You can also add `-Configuration Debug|Release` to build.ps1 in order to force particular configuration. Default is Release.
Tasks configured to be run in VS Code (Ctrl+Shift+B) has Debug as default except of the target *Pack*.

Another useful parameter is `-Version 1.2.0`. This is expected to be passed in CI setting. Otherwise the semantic version is attempted to be guessed from git (mostly via git tags) - see [GitVersion tool](http://gitversion.readthedocs.io/en/stable/).

## On RabbitMQ running locally
Simply run `docker-compose up` from root directory. You will find rabbitmq at port **5672** and rabbitmq managment web at **15672**. User: dev, password: dev, vhost: dev.<br />
IP address depends on your host OS:

**Linux** usually exposes these ports on localhost.

**Windows** usually have docker machine running behind the NAT, so you need to figure IP by yourself running `ipconfig` from commandline.

**MacOSX** no idea.
