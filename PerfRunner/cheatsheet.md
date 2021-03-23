## cheatsheet: 

> install crank command line tool
> git clean -fdx
> delete code generated files and dll from PerfRunner folder

> dotnet build -c Release /p:UseMechanismGenerator=true

> copy any generated file to PerfRunner folder
> copy Extras dll to PerfRunner folder

> dotnet build -c Release /p:RunningCrank=true

> crank --config PerfRunner\PerfRunner.yml --scenario runner --application.options.outputfiles PerfRunner\Microsoft.Extensions.Logging.Extras.dll --application.buildarguments /p:RunningCrank=true  --application.buildarguments /p:PublishReadyToRun=true --application.buildarguments /p:UseJsonGenerator=true --profile remote-win --iterations 2 --output struct-dozen-props.json

Low-Pri TODOs:

> TODO: make more user friendly
