FROM mcr.microsoft.com/azure-functions/dotnet:4 AS base

WORKDIR /home/site/wwwroot
COPY ["./publish/", "./"]

# ENTRYPOINT ["dotnet", "myfunctest.dll"]