# Stage 1: Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5293

# Stage 2: Build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the csproj and restore dependencies first
COPY *.csproj ./
# RUN dotnet restore

# Copy the rest of the files
COPY . . 

# Publish the application
RUN dotnet publish -c Release -o /app

# Stage 3: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "backend.dll"]
