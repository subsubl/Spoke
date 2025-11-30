#!/usr/bin/env bash

send_message() {
    local addr="$1"
    local msg="$2"
    curl --silent --get \
        --data-urlencode "channel=0" \
        --data-urlencode "message=$msg" \
        --data-urlencode "address=$addr" \
        "http://localhost:8001/sendChatMessage" >/dev/null
}

# escape function for JSON string values
json_escape() {
    local s="$1"
    s=${s//\\/\\\\}   # backslashes
    s=${s//\"/\\\"}   # quotes
    s=${s//$'\n'/\\n} # newlines
    s=${s//$'\r'/\\r} # carriage returns
    s=${s//$'\t'/\\t} # tabs
    printf '%s' "$s"
}

send_request() {
    local cmd="$1"
    shift

    local json_params=""
    local first=1

    for kv in "$@"; do
        key="${kv%%=*}"
        val="${kv#*=}"
        val=$(json_escape "$val")
        if [[ $first -eq 1 ]]; then
            json_params="\"$key\":\"$val\""
            first=0
        else
            json_params="$json_params,\"$key\":\"$val\""
        fi
    done

    local json="{\"method\":\"$cmd\",\"params\":{${json_params}}}"

    # Capture full response
    local response
    response=$(printf '%s\n' "$json" |
        curl -sS -X POST "http://localhost:8001/" \
            -H "Content-Type: application/json" \
            --data-binary @-)
}

# System info functions
temp() {
    local addr="$1"
    local temp_info
    temp_info=$(vcgencmd measure_temp 2>/dev/null || echo "Temperature: N/A")
    send_message "$addr" "$temp_info"
}

wifi() {
    local addr="$1"
    shift
    local args=("$@")
    local wifi_info

    if [[ ${#args[@]} -eq 0 ]]; then
        # Show current WiFi status
        wifi_info=$(iwconfig wlan0 2>/dev/null | grep -E "ESSID|Signal level" || echo "WiFi: N/A")
        send_message "$addr" "WiFi Status:\\n$wifi_info"
    else
        local action="${args[0]}"
        case "$action" in
            add)
                # Add WiFi network (simplified)
                send_message "$addr" "WiFi network addition not implemented in this example"
                ;;
            remove)
                # Remove WiFi network (simplified)
                send_message "$addr" "WiFi network removal not implemented in this example"
                ;;
            list)
                # List available networks
                wifi_info=$(iwlist wlan0 scan 2>/dev/null | grep ESSID || echo "No networks found")
                send_message "$addr" "Available WiFi Networks:\\n$wifi_info"
                ;;
            *)
                send_message "$addr" "WiFi commands: wifi, wifi list, wifi add <ssid>, wifi remove <ssid>"
                ;;
        esac
    fi
}

contacts() {
    local addr="$1"
    shift
    local args=("$@")

    if [[ ${#args[@]} -eq 0 ]]; then
        # Show help
        send_message "$addr" "Contacts commands: contacts list, contacts accept <address>, contacts add <address>, contacts remove <address>"
    else
        local action="${args[0]}"
        case "$action" in
            list)
                # Get contacts list
                local contacts_list
                contacts_list=$(send_request "getContacts" | jq -r '.result[]?.address' 2>/dev/null || echo "No contacts")
                send_message "$addr" "Contacts:\\n$contacts_list"
                ;;
            accept)
                local target_addr="${args[1]}"
                if [[ -n "$target_addr" ]]; then
                    send_request "acceptContact" "address=$target_addr"
                    send_message "$addr" "Contact request accepted: $target_addr"
                else
                    send_message "$addr" "Usage: contacts accept <address>"
                fi
                ;;
            add)
                local target_addr="${args[1]}"
                if [[ -n "$target_addr" ]]; then
                    send_request "addContact" "address=$target_addr"
                    send_message "$addr" "Contact added: $target_addr"
                else
                    send_message "$addr" "Usage: contacts add <address>"
                fi
                ;;
            remove)
                local target_addr="${args[1]}"
                if [[ -n "$target_addr" ]]; then
                    send_request "removeContact" "address=$target_addr"
                    send_message "$addr" "Contact removed: $target_addr"
                else
                    send_message "$addr" "Usage: contacts remove <address>"
                fi
                ;;
            *)
                send_message "$addr" "Unknown contacts command. Use: contacts list, accept, add, remove"
                ;;
        esac
    fi
}</content>
<parameter name="filePath">c:\Users\User\IxiHome\QuIXI\Examples\HomeAssistant\helpers.sh