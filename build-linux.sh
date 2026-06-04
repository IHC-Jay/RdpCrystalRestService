#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

APP_NAME="RDPCrystalRestService"
PUBLISH_DIR="$SCRIPT_DIR/publish"
RID="linux-x64"
NUGET_CONFIG="$SCRIPT_DIR/NuGet.Config"
PROJECT_FILE="$SCRIPT_DIR/$APP_NAME.csproj"

if [ ! -f "$NUGET_CONFIG" ]; then
    echo "NuGet config not found: $NUGET_CONFIG"
    exit 1
fi

if [ ! -f "$PROJECT_FILE" ]; then
    echo "Project file not found: $PROJECT_FILE"
    exit 1
fi

# Ensure required content files exist before publish.
if [ ! -f "$SCRIPT_DIR/appsettings.json" ]; then
    echo "Missing required file: $SCRIPT_DIR/appsettings.json"
    exit 1
fi

rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"

echo "=== Building $APP_NAME for $RID ==="
dotnet publish "$PROJECT_FILE" \
    --configfile "$NUGET_CONFIG" \
    --configuration Release \
    --runtime $RID \
    --self-contained false \
    --output "$PUBLISH_DIR" \
    /p:UseAppHost=true

echo ""
echo "=== Publish complete: $PUBLISH_DIR ==="
echo "Run with: ./run-linux.sh"
