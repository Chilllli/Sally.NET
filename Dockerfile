FROM dotnet/sdk
FROM mysql

WORKDIR Sally.NET/

RUN dotnet build Sally.NET.sln -c Release