# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# =========================
# Stage 1: Build (x64 host)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Install git + certificates so restore works
RUN apt-get update \
    && apt-get install -y --no-install-recommends git ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Clone the Init7.Epg repository directly from GitHub
RUN git clone https://github.com/lazda/Init7.Epg.git .

# Restore solution
RUN dotnet restore "Init7.Epg.sln"

# Publish the ARM64 build
RUN dotnet publish "Init7.Epg/Init7.Epg.csproj" \
    -c OpenWRT \
    -f net9.0 \
    -r linux-arm64 \
    --self-contained true \
    -o /app/publish \
    /p:PublishTrimmed=true

# ========================
# Runtime stage
# ========================
FROM mcr.microsoft.com/dotnet/runtime:9.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Init7.Epg.dll"]



