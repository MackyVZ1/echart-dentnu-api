# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
# Since echart_dentnu_api.csproj is directly in the root of the build context (.),
# we copy it to the current WORKDIR (/src).
COPY ["echart_dentnu_api.csproj", "./"]
RUN dotnet restore "echart_dentnu_api.csproj"

# Copy the rest of the application code
# This copies everything from the build context (.) into /src
COPY . .

# Build the application in Release configuration
# We are already in /src, so just build the .csproj directly.
RUN dotnet build "echart_dentnu_api.csproj" -c Release -o /app/build

# Stage 2: Publish the application (create a lean runtime image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose the port your application listens on
EXPOSE 8080 

# Copy the published application from the build stage
COPY --from=build /app/build .

# Command to run the application
ENTRYPOINT ["dotnet", "echart_dentnu_api.dll"]