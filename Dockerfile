FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SportsBuddyMatcher/SportsBuddyMatcher.Repository/SportsBuddyMatcher.Repository.csproj SportsBuddyMatcher/SportsBuddyMatcher.Repository/
COPY SportsBuddyMatcher/SportsBuddyMatcher.BusinessLogic/SportsBuddyMatcher.BusinessLogic.csproj SportsBuddyMatcher/SportsBuddyMatcher.BusinessLogic/
COPY SportsBuddyMatcher/SportsBuddyMatcher/SportsBuddyMatcher.csproj SportsBuddyMatcher/SportsBuddyMatcher/
RUN dotnet restore SportsBuddyMatcher/SportsBuddyMatcher/SportsBuddyMatcher.csproj

COPY SportsBuddyMatcher/ SportsBuddyMatcher/
RUN dotnet publish SportsBuddyMatcher/SportsBuddyMatcher/SportsBuddyMatcher.csproj \
    --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

COPY tools/NetherlandsSeed/NetherlandsSeed.csproj tools/NetherlandsSeed/
COPY tools/NetherlandsSeed/Program.cs tools/NetherlandsSeed/
RUN dotnet publish tools/NetherlandsSeed/NetherlandsSeed.csproj \
    --configuration Release --output /seed/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /seed/publish /seed
RUN mkdir -p /app/wwwroot/uploads && chown -R app:app /app/wwwroot/uploads
USER app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "SportsBuddyMatcher.dll"]
