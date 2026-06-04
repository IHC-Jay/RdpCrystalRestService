#!/bin/bash
# Test script for RDPCrystalRestService
# Run after the service is started with run-linux.sh

HOST="${1:-http://localhost:5137}"
BASE="$HOST/X12"
PASS=0
FAIL=0

check() {
    local label="$1"
    local expected_status="$2"
    local actual_status="$3"
    local body="$4"

    if [ "$actual_status" = "$expected_status" ]; then
        echo "  [PASS] $label (HTTP $actual_status)"
        PASS=$((PASS + 1))
    else
        echo "  [FAIL] $label — expected HTTP $expected_status, got HTTP $actual_status"
        echo "         Response: $body"
        FAIL=$((FAIL + 1))
    fi
}

echo ""
echo "=== RDPCrystalRestService Smoke Tests ==="
echo "    Target: $BASE"
echo ""

# ── Test 1: Missing operation parameter → 400 ─────────────────────────────────
echo "Group 1: Input validation"
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" "$BASE")
check "Missing operation → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 2: Unsupported operation → 400 ───────────────────────────────────────
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" "$BASE?operation=INVALID")
check "Unsupported operation → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 3: RDP missing transaction → 400 ─────────────────────────────────────
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" "$BASE?operation=RDP")
check "RDP missing transaction → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 4: RDP missing fileName and x12String → 400 ─────────────────────────
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" "$BASE?operation=RDP&transaction=837P")
check "RDP missing fileName/x12String → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 5: RDP x12String too short → 400 ────────────────────────────────────
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" \
    "$BASE?operation=RDP&transaction=837P&x12String=TOOSHORT")
check "RDP x12String too short → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 6: IRIS missing x12Id → 400 ─────────────────────────────────────────
echo ""
echo "Group 2: IRIS operation"
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" \
    "$BASE?operation=IRIS&transaction=837P")
check "IRIS missing x12Id → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 7: IRIS non-numeric x12Id → 400 / 500 (ArgumentException) ───────────
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" \
    "$BASE?operation=IRIS&transaction=837P&x12Id=NOT_A_NUMBER")
check "IRIS non-numeric x12Id → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 8: Validate missing transaction → 400 ───────────────────────────────
echo ""
echo "Group 3: Validate operation"
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" \
    "$BASE?operation=Validate")
check "Validate missing transaction → 400" "400" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Test 9: POST echo ─────────────────────────────────────────────────────────
echo ""
echo "Group 4: POST"
resp=$(curl -s -o /tmp/rdp_body.txt -w "%{http_code}" \
    -X POST "$BASE" \
    -H "Content-Type: application/json" \
    -d '{"test":"hello"}')
check "POST echo → 200" "200" "$resp" "$(cat /tmp/rdp_body.txt)"

# ── Summary ───────────────────────────────────────────────────────────────────
echo ""
echo "=== Results: $PASS passed, $FAIL failed ==="
echo ""

if [ "$FAIL" -gt 0 ]; then
    exit 1
fi
