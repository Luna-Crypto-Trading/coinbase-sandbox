# Use the official .NET SDK image with multi-arch support
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY src/CoinbaseSandbox.Api/*.csproj ./src/CoinbaseSandbox.Api/
COPY src/CoinbaseSandbox.Application/*.csproj ./src/CoinbaseSandbox.Application/
COPY src/CoinbaseSandbox.Domain/*.csproj ./src/CoinbaseSandbox.Domain/
COPY src/CoinbaseSandbox.Infrastructure/*.csproj ./src/CoinbaseSandbox.Infrastructure/
COPY tests/IntegrationTests/*.csproj ./tests/IntegrationTests/

# Restore dependencies
RUN dotnet restore -a $TARGETARCH

# Copy the rest of the source code
COPY . .

# Build and publish the API project for the target architecture
RUN dotnet publish src/CoinbaseSandbox.Api/CoinbaseSandbox.Api.csproj -c Release -a $TARGETARCH -o /app/publish

# Build the runtime image
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /app/publish .

# Copy the WebSocket tester HTML file
COPY websocker-tester.html ./wwwroot/websocket-tester.html
COPY src/CoinbaseSandbox.Api/wwwroot/dashboard.html ./wwwroot/dashboard.html
# Expose ports for HTTP and WebSockets
EXPOSE 5226

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5226
ENV ASPNETCORE_ENVIRONMENT=Production

# Set the entry point
ENTRYPOINT ["dotnet", "CoinbaseSandbox.Api.dll"]