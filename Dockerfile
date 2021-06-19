FROM dotnet/runtime
FROM mysql

WORKDIR Sally.NET/

RUN dotnet build Sally.NET.sln -c Release