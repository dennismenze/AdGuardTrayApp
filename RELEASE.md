# AdGuard Tray App - Release Guide

Dieses Projekt ist so konfiguriert, dass automatisch Releases für verschiedene Betriebssysteme erstellt werden.

## Automatische Releases (GitHub Actions)

### Wie es funktioniert
1. **Tag erstellen**: Erstellen Sie einen Git-Tag mit der Version (z.B. `v1.0.0`)
2. **Push**: Pushen Sie den Tag zu GitHub
3. **Build**: GitHub Actions erstellt automatisch Builds für alle Plattformen
4. **Release**: Ein GitHub Release wird automatisch mit allen Binaries erstellt

### Unterstützte Plattformen
- **Windows x64** (`win-x64`) - Primäre Zielplattform
- **Windows ARM64** (`win-arm64`) - Für ARM-basierte Windows-Geräte
- **Linux x64** (`linux-x64`) - Für moderne Linux-Distributionen
- **Linux ARM64** (`linux-arm64`) - Für ARM-basierte Linux-Systeme
- **macOS x64** (`osx-x64`) - Für Intel-basierte Macs
- **macOS ARM64** (`osx-arm64`) - Für Apple Silicon Macs

### Release erstellen

```bash
# 1. Version in der Projektdatei aktualisieren (optional)
# Bearbeiten Sie AdGuardTrayApp/AdGuardTrayApp.csproj

# 2. Änderungen committen
git add .
git commit -m "Prepare release v1.0.0"

# 3. Tag erstellen
git tag v1.0.0

# 4. Tag pushen (löst automatischen Build aus)
git push origin v1.0.0
```

### Manueller Trigger
Sie können den Build auch manuell in GitHub unter "Actions" → "Build and Release" → "Run workflow" auslösen.

## Lokale Builds

### Windows (PowerShell)
```powershell
# Einzelne Plattform
.\build.ps1 -Runtime win-x64

# Alle Plattformen
.\build.ps1 -AllPlatforms

# Mit Debug-Konfiguration
.\build.ps1 -Configuration Debug -Runtime win-x64
```

### Linux/macOS (Bash)
```bash
# Einzelne Plattform
./build.sh --runtime linux-x64

# Alle Plattformen
./build.sh --all-platforms

# Mit Debug-Konfiguration
./build.sh --configuration Debug --runtime linux-x64
```

## Build-Artefakte

### Eigenschaften
- **Self-contained**: Beinhaltet .NET Runtime
- **Single File**: Eine einzige ausführbare Datei
- **Native Libraries**: Eingebettete native Bibliotheken
- **Nicht getrimmte Builds**: Vollständige Kompatibilität

### Ausgabeformate
- **Windows**: `.zip` Archive
- **Linux/macOS**: `.tar.gz` Archive

## Plattform-spezifische Hinweise

### Windows
- Vollständige Windows Hello Unterstützung
- Systray-Integration funktioniert native
- Keine zusätzlichen Abhängigkeiten erforderlich

### Linux
- Möglicherweise zusätzliche Pakete für GUI erforderlich:
  ```bash
  # Ubuntu/Debian
  sudo apt-get install libgtk-3-0 libgdk-pixbuf2.0-0
  
  # CentOS/RHEL/Fedora
  sudo yum install gtk3 gdk-pixbuf2
  ```

### macOS
- Code-Signing möglicherweise erforderlich für Distribution
- Erste Ausführung erfordert möglicherweise Erlaubnis in Systemeinstellungen

## Versionierung

Das Projekt folgt [Semantic Versioning](https://semver.org/):
- **MAJOR**: Inkompatible API-Änderungen
- **MINOR**: Neue Funktionalität (rückwärtskompatibel)
- **PATCH**: Bugfixes (rückwärtskompatibel)

Beispiele:
- `v1.0.0` - Erste stabile Version
- `v1.1.0` - Neue Features hinzugefügt
- `v1.1.1` - Bugfixes
- `v2.0.0` - Breaking Changes

## Troubleshooting

### Build-Fehler
1. Stellen Sie sicher, dass .NET 8 SDK installiert ist
2. Überprüfen Sie, ob alle NuGet-Pakete wiederhergestellt wurden
3. Kontrollieren Sie Pfade in der `.csproj` Datei

### Plattform-spezifische Probleme
- **Linux**: GUI-Bibliotheken installieren
- **macOS**: Gatekeeper-Warnungen - App in Systemeinstellungen erlauben
- **Windows**: Antivirus-Software kann Self-Contained EXE blockieren

## CI/CD Pipeline

Die GitHub Actions Pipeline (`release.yml`) führt folgende Schritte aus:

1. **Checkout**: Code herunterladen
2. **Setup**: .NET SDK installieren
3. **Restore**: NuGet-Pakete wiederherstellen
4. **Build**: Projekt kompilieren
5. **Publish**: Self-contained Binaries erstellen
6. **Archive**: Platform-spezifische Archive erstellen
7. **Upload**: Artefakte hochladen
8. **Release**: GitHub Release mit allen Binaries erstellen

## Nächste Schritte

1. **Icon hinzufügen**: `icon.ico` Datei zum Projekt hinzufügen
2. **Code-Signing**: Für Windows/macOS Distributionen
3. **Installer**: MSI/PKG/DEB Pakete für bessere Installation
4. **Auto-Updates**: Automatische Update-Funktionalität implementieren
