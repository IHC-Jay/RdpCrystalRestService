#!/bin/bash
set -e

PUBLISH_DIR="./publish"
APP_NAME="RDPCrystalRestService"
PORT=5137

# Default rules/data paths — override via env or appsettings
export ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
export ASPNETCORE_URLS="http://0.0.0.0:$PORT"

# Use writable defaults on Linux hosts without root access.
export AppRuntime__DefaultDataPath=${AppRuntime__DefaultDataPath:-"$HOME/rdp/data"}
export AppRuntime__DefaultRulesPath=${AppRuntime__DefaultRulesPath:-"$HOME/rdp/rules"}

# Ensure runtime directories exist to avoid UnauthorizedAccessException.
mkdir -p "$AppRuntime__DefaultDataPath"
mkdir -p "$AppRuntime__DefaultRulesPath"

# Optional: set credentials via environment (preferred over appsettings for secrets)
# export Credentials__Iris__Username="youruser"
# export Credentials__Iris__Password="yourpassword"
# export Credentials__Rdp__LicenseKey="your-rdp-license-key"

# Optional: override IRIS endpoint/namespace for Linux runs.
# Note: run-linux.sh defaults ASPNETCORE_ENVIRONMENT to Production, so
# appsettings.Development.json is not used unless you set ASPNETCORE_ENVIRONMENT=Development.
# export Iris__Host="lp-itfdevvp2"
# export Iris__Port="6973"
# export Iris__DefaultNamespace="CORERULES"

# Optional: override paths (if using system-managed directories)
# export AppRuntime__DefaultRulesPath="/opt/rdp/rules"
# export AppRuntime__DefaultDataPath="/var/lib/rdp/data"

echo "=== Starting $APP_NAME on port $PORT (env: $ASPNETCORE_ENVIRONMENT) ==="
echo "Data path : $AppRuntime__DefaultDataPath"
echo "Rules path: $AppRuntime__DefaultRulesPath"
echo "IRIS host : ${Iris__Host:-from appsettings.json}"
echo "IRIS port : ${Iris__Port:-from appsettings.json}"
echo "IRIS ns   : ${Iris__DefaultNamespace:-from appsettings.json}"
cd "$PUBLISH_DIR"
./$APP_NAME
