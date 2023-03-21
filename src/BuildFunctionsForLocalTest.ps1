# Creates the publish targets that TimeAspNetCoreApp's various debug configs expect
dotnet publish -c Release
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=true --no-self-contained 
