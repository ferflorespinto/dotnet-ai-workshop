# Use the official .NET 9 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# This straight up doesn't work because you run into NU1103: Unable to find a stable package.
# This happens because the Microsoft.Extensions.AI.OpenAI packages only have preview versions.
# RUN dotnet restore --configfile NuGet.config

# Copy the whole project (assumes it's already restored)
COPY . ./
RUN dotnet publish -c Debug -o out

# Use the runtime-only image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Expose default port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "QuizApp.dll"]
