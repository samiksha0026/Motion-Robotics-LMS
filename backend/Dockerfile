# =============================================================
# Stage 1: Build
# =============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first — enables Docker layer caching for restore
COPY MotionRobotics.LMS.API/MotionRobotics.LMS.API.csproj MotionRobotics.LMS.API/

# Restore dependencies only (cached unless .csproj changes)
RUN dotnet restore MotionRobotics.LMS.API/MotionRobotics.LMS.API.csproj

# Copy the rest of the source code
COPY MotionRobotics.LMS.API/ MotionRobotics.LMS.API/

# Publish in Release mode — no dev tools, no test projects
RUN dotnet publish MotionRobotics.LMS.API/MotionRobotics.LMS.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    --runtime linux-x64 \
    --self-contained false

# =============================================================
# Stage 2: Runtime (final image — no SDK, minimal size)
# =============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root system user and group for security
RUN addgroup --system appgroup && \
    adduser --system --ingroup appgroup --no-create-home appuser

# Copy only the published output from the build stage
COPY --from=build /app/publish .

# Transfer ownership of app files to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port 8080 (standard non-privileged port for containers)
EXPOSE 8080

# Tell ASP.NET Core to listen on port 8080
# All other config (DB, JWT, secrets) must be injected via environment variables at runtime
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MotionRobotics.LMS.API.dll"]
