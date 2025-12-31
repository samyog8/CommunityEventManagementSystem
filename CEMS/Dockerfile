FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CommunityEventSystem.csproj", "./"]
RUN dotnet restore "CommunityEventSystem.csproj"
COPY . .
RUN dotnet build "CommunityEventSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommunityEventSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommunityEventSystem.dll"]
