#!/usr/bin/env bash

# Auto-accept incoming contacts script for QuIXI
# Automatically accepts all incoming contact requests

echo "Starting auto-accept script..."

while true; do
    # Get pending contact requests
    pending=$(curl -s "http://localhost:8001/getPendingContacts" | jq -r '.result[]?.address' 2>/dev/null)

    if [ -n "$pending" ]; then
        echo "Accepting pending contacts: $pending"
        for address in $pending; do
            # Accept the contact
            curl -s -X POST "http://localhost:8001/acceptContact" \
                -H "Content-Type: application/json" \
                -d "{\"address\":\"$address\"}" >/dev/null
            echo "Accepted contact: $address"
        done
    fi

    # Wait before checking again
    sleep 30
done</content>
<parameter name="filePath">c:\Users\User\IxiHome\QuIXI\Examples\HomeAssistant\ixiAutoAccept.sh