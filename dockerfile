# Use the official Microsoft .NET SDK image for development
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore any dependencies (via NuGet)
COPY *.csproj ./
RUN dotnet restore

# Copy the project files and build the app
COPY . ./
RUN dotnet publish -c Debug -o out --no-restore

# Start the second stage of the build using the .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5192

COPY --from=build-env /app/out .
aa