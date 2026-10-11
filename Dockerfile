# Stage 1: Base runtime image (Linux)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Project 385.csproj", "./"]
RUN dotnet restore "Project 385.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Project 385.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish output
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Project 385.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Project 385.dll"]