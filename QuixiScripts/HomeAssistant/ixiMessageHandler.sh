#!/usr/bin/env bash

. helpers.sh

# Listen for MQTT messages
mosquitto_sub -t "Chat/#" | while read -r message; do
    # Parse JSON message
    data=$(echo "$message" | jq -rc '[.sender.base58Address,.data.data] | @tsv')
    sender=$(echo "$data" | awk '{print $1}')
    cmd=$(echo "$data" | awk '{print tolower($2)}')
    args="$(echo "$data" | cut -f2-)" || args=""

    case "$cmd" in
        status)
            # Forward to Home Assistant sync script
            python3 hass_sync.py status "$sender"
            ;;
        devices)
            python3 hass_sync.py devices "$sender"
            ;;
        control)
            # Parse control command: "control light.living_room on"
            entity_id=$(echo "$args" | awk '{print $2}')
            action=$(echo "$args" | awk '{print $3}')
            python3 hass_sync.py control "$sender" "$entity_id" "$action"
            ;;
        sync)
            python3 hass_sync.py sync "$sender"
            ;;
        help)
            send_message "$sender" "Home Assistant Commands:
status - Check HA connection status
devices - List all HA devices
control <entity_id> <on|off|toggle|open|close|stop> - Control device
sync - Force sync all device states
help - Show this help

Examples:
control light.living_room on
control switch.kitchen off
control cover.garage_door open"
            ;;
        temp)
            temp "$sender"
            ;;
        wifi)
            wifi "$sender" "${args[@]}"
            ;;
        contacts)
            contacts "$sender" "${args[@]}"
            ;;
        *)
            send_message "$sender" "Unknown command, use help for Home Assistant commands."
            ;;
    esac
done</content>
<parameter name="filePath">c:\Users\User\IxiHome\QuIXI\Examples\HomeAssistant\ixiMessageHandler.sh