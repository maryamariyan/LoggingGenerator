## cheatsheet: 

> install crank command line tool
> git clean -fdx
> delete code generated files and dll from PerfRunner folder

> dotnet build -c Release /p:UseMechanismGenerator=true

> copy any generated file to PerfRunner folder
> copy Extras dll to PerfRunner folder

> dotnet build -c Release /p:RunningCrank=true

> crank --config PerfRunner\PerfRunner.yml --scenario runner --application.options.outputfiles PerfRunner\Microsoft.Extensions.Logging.Extras.dll --application.buildarguments /p:RunningCrank=true  --application.buildarguments /p:PublishReadyToRun=true --application.buildarguments /p:UseJsonGenerator=true --profile remote-win --iterations 2 --output static-dozen-props.json

Low-Pri TODOs:

> TODO: cleanup any unused System.Text.Json logic
> TODO: make more user friendly

High-Pri TODOs:

> TODO: Compare struct vs. class memory allocation usage (sealed class vs. readonly struct to be exact)