# AdGuard Tray App

A Windows 11 System Tray application for automatic control of AdGuard Home with Windows Hello authentication.

## Features

- **System Tray Integration**: Runs in the system tray with modern, professional icons
- **Windows Hello Authentication**: Supports PIN, fingerprint, and facial recognition
- **Automatic IP Detection**: Automatically detects your local network IP address
- **Selective Service Unblocking**: Choose specific blocked services for temporary unblocking
- **Custom Filter Rules**: Manage app-specific filter rules with `#ADGUARD_TRAY_APP` tag
- **Intelligent Backup & Restore**: Automatic backup and restoration of all original settings
- **Visual Status**: Shows current state via professional AdGuard-style shield icons (locked/unlocked)
- **Service Selection Dialog**: Clear display of all blocked services
- **Automatic Reset**: Reactivates blocking after time expires or app is closed
- **User-Friendly Interface**: Tooltips and intuitive design for easy configuration

## New in this Version

### ✨ Automatic IP Detection
- **Smart Network Detection**: Automatically finds your local network IP address (192.168.x.x, 10.x.x.x, 172.16-31.x.x)
- **One-Click Update**: 🔍 button next to the IP field for instant IP detection
- **Fallback Support**: Gracefully handles network detection errors with sensible defaults

### 🎨 Professional Icons
- **Modern Shield Design**: AdGuard-inspired shield icons with gradient effects
- **Status Indicators**: 
  - 🔴 Red shield with lock icon = Blocked state
  - 🟢 Green shield with checkmark = Unblocked state
- **High-Quality Rendering**: Anti-aliased, 32x32 pixel icons for crisp display

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
- **Target IP**: IP address to be unblocked (automatically detected or manually set)
- **Duration**: Unblocking duration in minutes (default: 60)
- **Autostart**: Automatically start with Windows

### IP Address Configuration
The app now automatically detects your local network IP address when the configuration dialog opens. You can:
- Use the automatically detected IP (recommended)
- Click the 🔍 button to re-detect your IP address
- Manually enter a different IP address if needed

## Advanced Features

### Service Selection
- Interactive dialog showing all available blocked services
- Real-time status updates
- Service-specific unblocking with visual indicators

### Security Features
- Windows Hello integration for secure access
- Automatic settings backup and restoration
- Prevention of unauthorized configuration changes

## Usage

### Basic Operation
1. **Unlock**: Click on tray icon → Windows Hello authentication → Service selection dialog
2. **Service Selection**: Choose which blocked services to temporarily unblock
3. **Automatic Lock**: Services are automatically re-blocked after the timer expires

### Context Menu
Right-click the tray icon for:
- Configuration settings
- Manual lock/unlock toggle
- Application exit

## Architecture

The application consists of several key components:

- `MainForm.cs`: Main application logic and tray icon management
- `ConfigurationForm.cs`: Settings dialog with automatic IP detection
- `ServiceSelectionForm.cs`: Service selection interface
- `AdGuardApiService.cs`: AdGuard Home API integration
- `Program.cs`: Application entry point with single instance protection

## Development

### Building from Source
```powershell
git clone <repository-url>
cd AdGuardTrayApp
dotnet restore
dotnet build --configuration Release
```

### Requirements for Development
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Windows SDK (for Windows Hello APIs)

## Troubleshooting

### Common Issues
1. **IP Detection Failed**: Check network connectivity and try manual IP entry
2. **AdGuard Connection Error**: Verify AdGuard Home server URL and credentials
3. **Windows Hello Not Available**: Ensure Windows Hello is set up on your device

### Debug Mode
The application includes comprehensive logging for troubleshooting. Check the debug output for detailed information about API calls and network operations.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

- AdGuard Home team for the excellent filtering solution
- Microsoft for Windows Hello integration APIs
- .NET community for framework support
# Test
