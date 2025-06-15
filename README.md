# AdGuard Tray App

A Windows 11 System Tray application for automatic control of AdGuard Home with Windows Hello authentication.

## Features

- **System Tray Integration**: Runs in the system tray
- **Windows Hello Authentication**: Supports PIN, fingerprint, and facial recognition
- **Selective Service Unblocking**: Choose specific blocked services for temporary unblocking
- **Custom Filter Rules**: Manage app-specific filter rules with `#ADGUARD_TRAY_APP` tag
- **Intelligent Backup & Restore**: Automatic backup and restoration of all original settings
- **Visual Status**: Shows current state via icons (locked/unlocked)
- **Service Selection Dialog**: Clear display of all blocked services
- **Automatic Reset**: Reactivates blocking after time expires or app is closed

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- AdGuard Home Server with API access
- Windows Hello capable device (optional, for enhanced authentication)

## Installation

1. Install .NET 8.0 Runtime (if not already present)
2. **Compile project**:

   ```powershell
   dotnet build --configuration Release
   ```

   Or use the pre-configured build task:

   ```powershell
   # In VS Code: Ctrl+Shift+P → "Tasks: Run Task" → "Build AdGuard Tray App"
   ```

3. Start executable from `bin\Release\net8.0-windows\`

## Configuration

On first start or via the context menu, the following settings can be configured:

- **AdGuard Host**: URL of the AdGuard Home server (e.g., `http://192.168.178.30:3000/`)
- **Username**: AdGuard Home username
- **Password**: AdGuard Home password
- **Target IP**: IP address to be unblocked (default: current device IP)
- **Duration**: Unblocking duration in minutes (default: 60)

## Advanced Features

### Service Selection Dialog

After authentication, a detailed dialog opens with two tabs:

#### Blocked Services

- List of all services blocked by AdGuard
- Selective choice for temporary unblocking

#### Custom Filter Rules

- Management of app-specific rules with the tag `#ADGUARD_TRAY_APP`
- Status display: ✅ Active and ⏸️ disabled rules
- Temporary deactivation of individual rules

### Backup & Restore System

- Automatic backup of all original settings before changes
- Complete restoration after timer expiration

## Usage

1. **Unlock**: Click on tray icon → Windows Hello authentication → Service selection dialog
2. **Service Selection**:
   - Choose specific blocked services for unblocking
   - Manage custom filter rules
3. **Automatic Reset**: After time expires, all changes are automatically reverted

## Technical Details

- **Framework**: .NET 8.0 Windows Forms
- **Architecture**:
  - `MainForm.cs`: Main application logic and tray icon management
  - `AdGuardApiService.cs`: AdGuard Home API integration
  - `ServiceSelectionForm.cs`: Dialog for service selection and filter rules
  - `ConfigurationForm.cs`: Configuration dialog
  - `Program.cs`: Application startup with single-instance protection
- **API**: AdGuard Home REST API with Basic Authentication
- **Authentication**: Windows Credential UI (CredUI) for Windows Hello
- **Client Management**: Creates and manages temporary "AutoAllow_" clients in AdGuard

## Development

```powershell
# Clone project and compile
git clone <repository-url>
cd AdGuardTrayApp
dotnet build

# Start debug mode
dotnet run
```

## License

This project is released under the MIT License.
