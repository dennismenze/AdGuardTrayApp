<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# AdGuard Tray App - Copilot Instructions

This is a C# Windows Forms application that runs in the system tray and provides AdGuard Home control functionality.

## Project Overview
- **Technology**: C# .NET 8.0 Windows Forms
- **Purpose**: System tray application for AdGuard Home IP unblocking with Windows Hello authentication
- **Key Features**:
  - System tray integration with contextual menu
  - Windows Hello authentication (PIN, Fingerprint, Face recognition)
  - AdGuard Home API integration for client management
  - 60-minute timer with automatic reset
  - Prevention of application closure during active sessions
  - Visual state indicators (locked/unlocked icons)

## Architecture
- `MainForm.cs`: Main application logic, tray icon management, AdGuard API calls
- `ConfigurationForm.cs`: Settings dialog for AdGuard connection parameters
- `Program.cs`: Application entry point with single instance protection
- `AdGuardTrayApp.csproj`: Project configuration with Windows Forms and JSON dependencies

## Coding Guidelines
- Use async/await patterns for HTTP requests
- Implement proper error handling with user-friendly notifications
- Follow Windows Forms best practices for UI components
- Use dependency injection where appropriate
- Maintain clean separation between UI and business logic
- Handle Windows API calls safely with proper P/Invoke declarations

## AdGuard Integration
- Uses Basic Authentication with AdGuard Home API
- Manages client configurations via REST endpoints
- Creates temporary "AutoAllow_" clients for IP unblocking
- Implements automatic cleanup after timer expiration

## Security Considerations
- Windows Hello integration for authentication
- Secure credential storage
- Prevention of unauthorized application termination
- Safe handling of AdGuard credentials

When working with this codebase, prioritize user experience, security, and reliability.
