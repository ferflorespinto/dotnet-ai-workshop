# Copy all files onto the image and build/publish the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
ARG configuration=Release
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime and entrypoint
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerceMcpServer.dll"]