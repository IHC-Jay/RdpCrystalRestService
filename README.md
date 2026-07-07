# RdpCrystalRestService

REST service for RDP Crystal X12 validation with optional IRIS reads and validation-error logging.

## Local configuration

IRIS validation-error inserts require all of the following settings at runtime:

- `Iris:Host`
- `Iris:Port`
- `Iris:DefaultNamespace`
- `Credentials:Iris:Username`
- `Credentials:Iris:Password`

The sample appsettings files intentionally leave IRIS credentials blank. Supply them with environment variables, user secrets, or local appsettings overrides before testing `logToDb=1`.

Windows PowerShell example:

```powershell
$env:Credentials__Iris__Username = "youruser"
$env:Credentials__Iris__Password = "yourpassword"
dotnet run
```

When validation runs with `logToDb=1`, the API response details now include whether IRIS logging succeeded or failed.
