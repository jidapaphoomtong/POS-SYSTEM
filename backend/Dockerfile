# Use .NET SDK for build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build-env
WORKDIR /src

# Copy only the required files for restoring dependencies
COPY *.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy the rest of the application files
COPY . .

# Build and publish the application
RUN dotnet publish -c Release -o /app --no-restore

# Use runtime image for the running application
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
WORKDIR /app
COPY --from=build-env /app .

# Copy the published application from build-env
EXPOSE 5293
ENV ASPNETCORE_URLS=http://+:5293

# Start the application
ENTRYPOINT ["dotnet", "backend.dll"]