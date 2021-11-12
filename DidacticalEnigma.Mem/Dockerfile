FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DidacticalEnigma.Mem/DidacticalEnigma.Mem.csproj", "DidacticalEnigma.Mem/"]
RUN dotnet restore "DidacticalEnigma.Mem/DidacticalEnigma.Mem.csproj"
COPY . .
WORKDIR "/src/DidacticalEnigma.Mem"
RUN dotnet build "DidacticalEnigma.Mem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DidacticalEnigma.Mem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DidacticalEnigma.Mem.dll"]
