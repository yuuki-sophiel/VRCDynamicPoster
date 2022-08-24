FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /App
ENTRYPOINT [ "dotnet", "run" ]