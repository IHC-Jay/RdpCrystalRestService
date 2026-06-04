# RDPCrystalRestService — Linux Deployment Guide

## Prerequisites
- .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
- IRIS DLLs are included in the `IRIS/` folder (already referenced by the project)

---

## 1. Build & Publish

```bash
chmod +x build-linux.sh run-linux.sh test-linux.sh
./build-linux.sh
```

Output goes to `./publish/`.

---

## 2. Configure

Credentials and paths are read from `appsettings.json` (base config) and overridden by environment variables at runtime. **Never commit real credentials** — supply them via env vars:

```bash
export Credentials__Iris__Username="youruser"
export Credentials__Iris__Password="yourpassword"
export Credentials__Rdp__LicenseKey="your-rdp-license"
export AppRuntime__DefaultRulesPath="/opt/rdp/rules"
export AppRuntime__DefaultDataPath="/var/lib/rdp/data"
```

Or set them in `appsettings.json` before publishing (dev only).

---

## 3. Run

```bash
./run-linux.sh
```

Service starts on `http://0.0.0.0:5137` by default.  
Override port: `export ASPNETCORE_URLS="http://0.0.0.0:8080"`

By default, `run-linux.sh` now uses writable paths under your home directory:

- `AppRuntime__DefaultDataPath=$HOME/rdp/data`
- `AppRuntime__DefaultRulesPath=$HOME/rdp/rules`

This avoids permission errors like `Access to the path '/var/lib/rdp' is denied` when running as a non-root user.

If you must use `/var/lib/rdp` and `/opt/rdp/rules`, grant permission first:

```bash
sudo mkdir -p /var/lib/rdp/data /opt/rdp/rules
sudo chown -R $USER:$USER /var/lib/rdp /opt/rdp/rules
```

---

## 4. Smoke Test

In a second terminal (while service is running):

```bash
./test-linux.sh
# or with custom host:
./test-linux.sh http://myserver:5137
```

---

## 5. Manual curl Examples

```bash
HOST=http://localhost:5137

# Returns 400 — unsupported operation
curl -s "$HOST/X12?operation=INVALID" | python3 -m json.tool

# Returns 400 — missing transaction
curl -s "$HOST/X12?operation=RDP" | python3 -m json.tool

# Returns 404 — file not found (if rulesPath exists but file does not)
curl -s "$HOST/X12?operation=RDP&transaction=837P&fileName=missing.txt&rulesPath=/opt/rdp/rules" | python3 -m json.tool

# RDP validate with inline x12String
X12="ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *200101*1200*U*00501*000000001*0*T*:~..."
curl -sG "$HOST/X12" \
  --data-urlencode "operation=RDP" \
  --data-urlencode "transaction=837P" \
  --data-urlencode "x12String=$X12" | python3 -m json.tool

# POST echo
curl -s -X POST "$HOST/X12" \
  -H "Content-Type: application/json" \
  -d '{"hello":"world"}' | python3 -m json.tool
```

---

## 6. Response codes

| Code | Meaning |
|------|---------|
| 200  | Success |
| 400  | Bad request / invalid input |
| 404  | Record or file not found |
| 422  | X12 validation failed (FAIL status) |
| 500  | Unexpected server error |
