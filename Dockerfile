FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FinOpsFlow.Web/FinOpsFlow.Web.csproj", "FinOpsFlow.Web/"]
COPY ["FinOpsFlow.Core/FinOpsFlow.Core.csproj", "FinOpsFlow.Core/"]
COPY ["FinOpsFlow.Infrastructure/FinOpsFlow.Infrastructure.csproj", "FinOpsFlow.Infrastructure/"]
RUN dotnet restore "FinOpsFlow.Web/FinOpsFlow.Web.csproj"
COPY . .
RUN dotnet publish "FinOpsFlow.Web/FinOpsFlow.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=base /app .
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinOpsFlow.Web.dll"]