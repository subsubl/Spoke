#!/usr/bin/env bash

# Home Assistant Integration Startup Script
# Launches QuIXI and Home Assistant sync components

echo "Starting Home Assistant integration for QuIXI..."

# Set script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Check dependencies
command -v dotnet >/dev/null 2>&1 || { echo "Error: .NET is not installed"; exit 1; }
command -v python3 >/dev/null 2>&1 || { echo "Error: Python3 is not installed"; exit 1; }
command -v mosquitto >/dev/null 2>&1 || { echo "Error: Mosquitto is not installed"; exit 1; }
command -v curl >/dev/null 2>&1 || { echo "Error: curl is not installed"; exit 1; }
command -v jq >/dev/null 2>&1 || { echo "Error: jq is not installed"; exit 1; }

# Check config file
if [ ! -f "hass_config.json" ]; then
    echo "Error: hass_config.json not found. Please create it with your Home Assistant configuration."
    exit 1
fi

# Validate config
python3 -c "
import json
try:
    with open('hass_config.json', 'r') as f:
        config = json.load(f)
    required = ['url', 'token']
    for key in required:
        if key not in config:
            print(f'Error: {key} not found in config')
            exit(1)
    print('Config validation passed')
except Exception as e:
    print(f'Config validation failed: {e}')
    exit(1)
" || exit 1

# Start Mosquitto if not running
if ! pgrep -x "mosquitto" > /dev/null; then
    echo "Starting Mosquitto MQTT broker..."
    sudo systemctl start mosquitto 2>/dev/null || mosquitto -d
fi

# Start QuIXI if not running
if ! pgrep -f "QuIXI.dll" > /dev/null; then
    echo "Starting QuIXI..."
    cd ../..
    nohup dotnet run --project QuIXI/QuIXI.csproj > quixi.log 2>&1 &
    QUIXI_PID=$!
    echo $QUIXI_PID > quixi.pid
    cd "$SCRIPT_DIR"

    # Wait for QuIXI to start
    echo "Waiting for QuIXI to initialize..."
    sleep 10
fi

# Start Home Assistant sync script
echo "Starting Home Assistant sync..."
python3 hass_sync.py restart &
HASS_PID=$!
echo $HASS_PID > hass_sync.pid

# Start message handler
echo "Starting message handler..."
./ixiMessageHandler.sh &
HANDLER_PID=$!
echo $HANDLER_PID > handler.pid

# Start auto-accept script
echo "Starting auto-accept script..."
./ixiAutoAccept.sh &
ACCEPT_PID=$!
echo $ACCEPT_PID > accept.pid

echo "Home Assistant integration started successfully!"
echo "PIDs saved to *.pid files"
echo "Check logs: quixi.log, hass_sync.log"
echo "Stop with: kill $QUIXI_PID $HASS_PID $HANDLER_PID $ACCEPT_PID"

# Wait for interrupt
trap 'echo "Stopping..."; kill $HASS_PID $HANDLER_PID $ACCEPT_PID 2>/dev/null; exit 0' INT TERM

wait