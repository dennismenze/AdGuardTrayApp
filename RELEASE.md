# Release Guide 🚀

## Automatische Releases mit MSI-Unterstützung

Das Projekt erstellt automatisch Windows-Releases mit **ZIP-Archiven** und **MSI-Installern** für verschiedene Architekturen.

### Wie man ein Release erstellt:

```bash
# 1. Version Tag erstellen
git tag v1.0.1

# 2. Tag pushen (löst automatischen Build aus)
git push origin v1.0.1
```

### Unterstützte Windows-Architekturen:
- **x64** - Moderne PCs (Intel/AMD 64-bit) 
- **x86** - Ältere 32-bit Systeme  
- **ARM64** - Windows on ARM Geräte

### Was wird automatisch erstellt:
1. ✅ **ZIP-Archive** - Portable Versionen für alle Architekturen
2. ✅ **MSI-Installer** - Professionelle Installer mit Start Menu & Desktop Shortcuts
3. ✅ **GitHub Release** mit allen Dateien
4. ✅ **Release Notes** automatisch generiert

### Lokale Builds:
```powershell
# Nur ZIP-Archive
.\build.ps1 -Runtime win-x64

# Mit MSI-Installern
.\build.ps1 -Runtime win-x64 -CreateMSI

# Alle Architekturen mit MSI
.\build.ps1 -AllPlatforms -CreateMSI
```

## MSI-Installer Features

### Vorteile der MSI-Installer:
- 🏠 **Start Menu Shortcut** - Einfacher Zugriff über das Startmenü
- 🖥️ **Desktop Shortcut** - Optional während Installation
- 🗂️ **Ordentliche Installation** - Installiert in Program Files
- 🔄 **Saubere Deinstallation** - Über Windows "Programme hinzufügen/entfernen"
- 📝 **Registry-Einträge** - Professionelle Windows-Integration

### Installation Optionen:

#### Option 1: MSI Installer (Empfohlen)
```
AdGuardTrayApp-windows-x64.msi    # Für moderne PCs
AdGuardTrayApp-windows-x86.msi    # Für ältere Systeme  
AdGuardTrayApp-windows-arm64.msi  # Für ARM-Geräte
```

#### Option 2: Portable ZIP
```
AdGuardTrayApp-windows-x64.zip    # Portable Version
AdGuardTrayApp-windows-x86.zip    # Keine Installation nötig
AdGuardTrayApp-windows-arm64.zip  # Einfach entpacken & ausführen
```

## Technische Details

### WiX Toolset Integration:
- **WiX 4.0.5** für MSI-Erstellung
- **Professional Installer** mit Windows-Standards
- **Self-contained** - Keine .NET Runtime Installation nötig
- **Code-Signing Ready** - Vorbereitet für zukünftige Signierung

### Lokale MSI-Entwicklung:
```powershell
# WiX Toolset installieren
dotnet tool install --global wix --version 4.0.5
wix extension add WixToolset.UI.wixext/4.0.5
wix extension add WixToolset.Util.wixext/4.0.5

# MSI für x64 erstellen
.\build.ps1 -Runtime win-x64 -CreateMSI
```

## Troubleshooting

### WiX Installation:
```powershell
# Falls WiX-Fehler auftreten
dotnet tool uninstall --global wix
dotnet tool install --global wix --version 4.0.5
```

### MSI-Permissions:
- **Installation**: Möglicherweise Administratorrechte erforderlich
- **Windows Defender**: Kann unbekannte MSI-Dateien blockieren
- **Antivirus**: Eventuell Ausnahme für MSI-Dateien hinzufügen

### Build-Fehler beheben:
- ✅ .NET 8 SDK installiert
- ✅ NuGet Quellen konfiguriert
- ✅ WiX Toolset installiert (für MSI)

Das System ist jetzt vollständig für professionelle Windows-Releases mit MSI-Installern konfiguriert! 🎉