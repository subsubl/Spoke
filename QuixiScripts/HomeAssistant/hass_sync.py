#!/usr/bin/env python3
"""
Home Assistant Sync Script for QuIXI
Syncs device states between Home Assistant and Spoke via Ixian network
"""

import asyncio
import json
import logging
import sys
import signal
import os
from datetime import datetime
import websockets
import aiohttp
import requests
import base64
from BridgeConnection import BridgeConnection

# Configuration
HASS_CONFIG_FILE = 'hass_config.json'
QUIXI_API_URL = 'http://localhost:8001'
MQTT_BROKER = 'localhost'
MQTT_PORT = 1883

# Global variables
hass_websocket = None
quixi_session = None
device_states = {}
running = True
bridge_connection = None

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('hass_sync.log'),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger(__name__)

def load_config():
    """Load Home Assistant configuration"""
    try:
        with open(HASS_CONFIG_FILE, 'r') as f:
            return json.load(f)
    except FileNotFoundError:
        logger.error(f"Config file {HASS_CONFIG_FILE} not found")
        sys.exit(1)
    except json.JSONDecodeError as e:
        logger.error(f"Invalid JSON in config file: {e}")
        sys.exit(1)

# Initialize BridgeConnection
async def initialize_bridge_connection():
    global bridge_connection
    config = load_config()
    bridge_connection = BridgeConnection(
        bridgeAddress=config['quixi_bridge_address'],
        ourAddress=config['our_wallet_address'],
        ourPublicKey=base64.b64decode(config['our_public_key'])
    )

async def send_quixi_message(address, message):
    """Send message via QuIXI API with signed authentication"""
    try:
        if not bridge_connection:
            await initialize_bridge_connection()

        signature = bridge_connection.SignData(message, bridge_connection.ourPrivateKey)
        async with quixi_session.post(
            f"{QUIXI_API_URL}/sendChatMessage",
            data={
                'channel': '0',
                'message': message,
                'address': address,
                'signature': signature
            }
        ) as response:
            return response.status == 200
    except Exception as e:
        logger.error(f"Failed to send QuIXI message: {e}")
        return False

async def get_hass_states():
    """Get all states from Home Assistant with retries"""
    retries = 3
    for attempt in range(retries):
        try:
            config = load_config()
            headers = {
                'Authorization': f'Bearer {config["token"]}',
                'Content-Type': 'application/json'
            }

            async with quixi_session.get(
                config['url'].replace('ws://', 'http://').replace('wss://', 'https://') + '/api/states',
                headers=headers
            ) as response:
                if response.status == 200:
                    return await response.json()
                else:
                    logger.error(f"Failed to get HA states: {response.status}")
        except Exception as e:
            logger.error(f"Error getting HA states (attempt {attempt + 1}): {e}")
            if attempt < retries - 1:
                await asyncio.sleep(2 ** attempt)  # Exponential backoff
    return []

async def control_hass_entity(entity_id, service, **kwargs):
    """Control Home Assistant entity"""
    try:
        config = load_config()
        headers = {
            'Authorization': f'Bearer {config["token"]}',
            'Content-Type': 'application/json'
        }

        url = f"{config['url'].replace('ws://', 'http://').replace('wss://', 'https://')}/api/services/{service.split('.')[0]}/{service.split('.')[1]}"

        data = {'entity_id': entity_id, **kwargs}

        async with quixi_session.post(url, headers=headers, json=data) as response:
            return response.status == 200
    except Exception as e:
        logger.error(f"Error controlling HA entity {entity_id}: {e}")
        return False

async def handle_quixi_command(sender, command, args):
    """Handle commands from QuIXI/Spoke"""
    logger.info(f"Processing command from {sender}: {command} {args}")

    if command == 'status':
        config = load_config()
        await send_quixi_message(sender, f"Home Assistant sync active. Connected to {config['url']}")

    elif command == 'devices':
        states = await get_hass_states()
        device_list = [f"{s['entity_id']}: {s['state']}" for s in states[:10]]  # First 10 devices
        message = "Devices:\\n" + "\\n".join(device_list)
        if len(states) > 10:
            message += f"\\n... and {len(states) - 10} more"
        await send_quixi_message(sender, message)

    elif command == 'control':
        if len(args) < 2:
            await send_quixi_message(sender, "Usage: control <entity_id> <command> [params]")
            return

        entity_id = args[0]
        action = args[1].lower()

        # Map actions to Home Assistant services
        service_map = {
            'on': 'turn_on',
            'off': 'turn_off',
            'toggle': 'toggle',
            'open': 'open_cover',
            'close': 'close_cover',
            'stop': 'stop_cover'
        }

        if action in service_map:
            domain = entity_id.split('.')[0]
            service = f"{domain}.{service_map[action]}"
            success = await control_hass_entity(entity_id, service)
            status = "successful" if success else "failed"
            await send_quixi_message(sender, f"Control command {status}: {entity_id} {action}")
        else:
            await send_quixi_message(sender, f"Unknown action: {action}")

    elif command == 'sync':
        states = await get_hass_states()
        device_states.clear()
        for state in states:
            device_states[state['entity_id']] = state['state']
        await send_quixi_message(sender, f"Synced {len(states)} devices")

    elif command == 'help':
        help_text = """Available commands:
status - Check connection status
devices - List devices
control <entity_id> <on|off|toggle> - Control device
sync - Force sync all states
help - Show this help"""
        await send_quixi_message(sender, help_text)

    else:
        await send_quixi_message(sender, f"Unknown command: {command}. Use 'help' for available commands.")

async def hass_websocket_handler():
    """Handle Home Assistant WebSocket connection"""
    global hass_websocket

    config = load_config()
    uri = config['url']

    try:
        async with websockets.connect(uri) as websocket:
            hass_websocket = websocket
            logger.info("Connected to Home Assistant WebSocket")

            # Authenticate
            auth_msg = {
                "type": "auth",
                "access_token": config["token"]
            }
            await websocket.send(json.dumps(auth_msg))

            # Wait for auth response
            response = json.loads(await websocket.recv())
            if response.get("type") != "auth_ok":
                logger.error("Home Assistant authentication failed")
                return

            logger.info("Authenticated with Home Assistant")

            # Subscribe to state changes
            subscribe_msg = {
                "id": 1,
                "type": "subscribe_events",
                "event_type": "state_changed"
            }
            await websocket.send(json.dumps(subscribe_msg))

            # Initial state sync
            states = await get_hass_states()
            for state in states:
                device_states[state['entity_id']] = state['state']
            logger.info(f"Initial sync: {len(states)} devices")

            # Listen for events
            async for message in websocket:
                try:
                    event = json.loads(message)
                    if event.get("type") == "event" and event.get("event", {}).get("event_type") == "state_changed":
                        entity_id = event["event"]["data"]["entity_id"]
                        new_state = event["event"]["data"]["new_state"]["state"]
                        old_state = device_states.get(entity_id)

                        if new_state != old_state:
                            device_states[entity_id] = new_state
                            logger.info(f"State changed: {entity_id} = {new_state}")

                            # Notify Spoke via QuIXI (broadcast to all contacts)
                            # In a real implementation, you'd want to notify specific users
                            # For now, we'll log the change

                except json.JSONDecodeError:
                    continue

    except Exception as e:
        logger.error(f"Home Assistant WebSocket error: {e}")
        hass_websocket = None

async def mqtt_listener():
    """Listen for MQTT messages from QuIXI"""
    import aiomqtt
    try:
        async with aiomqtt.Client(hostname=MQTT_BROKER, port=MQTT_PORT) as client:
            async with client.messages() as messages:
                await client.subscribe("quixi/commands")
                async for message in messages:
                    payload = message.payload.decode()
                    logger.info(f"Received MQTT message: {payload}")
                    # Process MQTT message (e.g., forward to Home Assistant)
    except Exception as e:
        logger.error(f"MQTT listener error: {e}")
        await asyncio.sleep(10)

async def main():
    """Main application loop"""
    global quixi_session, running

    logger.info("Starting Home Assistant sync for QuIXI")

    # Setup HTTP session for QuIXI API
    quixi_session = aiohttp.ClientSession()

    # Setup signal handlers
    def signal_handler(signum, frame):
        global running
        logger.info("Shutdown signal received")
        running = False

    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

    try:
        # Start Home Assistant WebSocket connection
        hass_task = asyncio.create_task(hass_websocket_handler())

        # Start MQTT listener (placeholder)
        mqtt_task = asyncio.create_task(mqtt_listener())

        # Wait for tasks
        await asyncio.gather(hass_task, mqtt_task)

    except Exception as e:
        logger.error(f"Main loop error: {e}")
    finally:
        if quixi_session:
            await quixi_session.close()
        logger.info("Home Assistant sync stopped")

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "restart":
        # Kill existing process
        try:
            with open('hass_sync.pid', 'r') as f:
                pid = int(f.read().strip())
            os.kill(pid, signal.SIGTERM)
            logger.info(f"Terminated existing process {pid}")
        except:
            pass

    # Save PID
    with open('hass_sync.pid', 'w') as f:
        f.write(str(os.getpid()))

    # Run main loop
    asyncio.run(main())