# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first (better layer caching)
COPY MotionRobotics.LMS.API/MotionRobotics.LMS.API.csproj MotionRobotics.LMS.API/
RUN dotnet restore MotionRobotics.LMS.API/MotionRobotics.LMS.API.csproj

# Copy everything else
COPY MotionRobotics.LMS.API/ MotionRobotics.LMS.API/

# Publish
RUN dotnet publish MotionRobotics.LMS.API/MotionRobotics.LMS.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Copy outer wwwroot (syllabus PDFs, etc.) into publish output
COPY wwwroot/ /app/publish/wwwroot/

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MotionRobotics.LMS.API.dll"]