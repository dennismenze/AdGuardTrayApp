# 🚀 AdGuard Tray App - Release Scripts

Automatisierte Scripts für Versionierung, Commits und Releases mit GitHub Copilot Integration.

## 📁 Scripts Übersicht

| Script | Zweck | Verwendung |
|--------|-------|------------|
| `setup.ps1` | Einrichtung aller Tools | Einmalig ausführen |
| `auto-release.ps1` | Vollautomatisches Release | Version bump + Tag + Push |
| `quick-commit.ps1` | Schnelle Commits | Committen mit Copilot |

## 🛠️ Ersteinrichtung

1. **Setup ausführen** (als Administrator empfohlen):
```powershell
.\scripts\setup.ps1
```

2. **PowerShell neu starten** oder Profil neu laden:
```powershell
. $PROFILE
```

3. **GitHub Copilot authentifizieren**:
```powershell
github-copilot-cli auth
```

## 🚀 Verwendung

### Auto-Release (Empfohlen)
Vollautomatisches Release mit Versionierung:

```powershell
# Patch Release (1.0.0 → 1.0.1)
release patch

# Minor Release (1.0.0 → 1.1.0)  
release minor

# Major Release (1.0.0 → 2.0.0)
release major

# Dry Run (nur anzeigen, nicht ausführen)
release patch -DryRun

# Mit eigener Commit-Nachricht
.\scripts\auto-release.ps1 -VersionType patch -CustomMessage "🐛 fix: Kritischer Hotfix für Windows Hello"
```

### Quick Commit
Schnelle Commits mit GitHub Copilot:

```powershell
# Interaktiver Commit mit Copilot
qcommit

# Alle Dateien committen
qcommit -All

# Mit automatischem Push
qcommit -All -Push

# Mit eigener Nachricht
qcommit -Message "🔧 config: Update GitHub Actions"
```

### Manuelle Scripts
```powershell
# Auto-Release mit allen Optionen
.\scripts\auto-release.ps1 -VersionType patch -DryRun -Force -SkipCopilot

# Quick-Commit mit allen Optionen  
.\scripts\quick-commit.ps1 -Message "Custom message" -All -Push -SkipCopilot
```

## 🎯 Workflow Beispiele

### Typischer Development Workflow
```powershell
# 1. Änderungen machen
# ... Code bearbeiten ...

# 2. Quick Commit
qcommit -All

# 3. Weitere Commits...
qcommit -All

# 4. Release erstellen
release patch
```

### Hotfix Workflow
```powershell
# 1. Hotfix implementieren
# ... Bug fixen ...

# 2. Commit mit spezifischer Nachricht
qcommit -Message "🐛 fix: Behebe kritischen Windows Hello Fehler" -All

# 3. Sofortiges Patch-Release
release patch
```

### Feature Workflow
```powershell
# 1. Feature entwickeln
# ... Mehrere Commits ...

# 2. Minor Release für neues Feature
release minor
```

## ⚙️ Konfiguration

### Commit Message Template
Die Commit-Message-Vorlage ist in `.copilot/commit-messages.md` definiert und beinhaltet:
- Emoji-Guide für verschiedene Commit-Types
- AdGuard-spezifische Kategorien
- Deutsche Beschreibungen
- Conventional Commits Format

### VS Code Integration
Nach dem Setup ist GitHub Copilot in VS Code konfiguriert:
- Automatische Commit-Message-Generierung
- Template-basierte Vorschläge
- "Generate Commit Message with Copilot" Button

## 🔧 Was passiert beim Release?

1. **Versionsprüfung**: Aktuelle Version aus Git-Tags ermitteln
2. **Version-Bump**: Neue Version berechnen (patch/minor/major)
3. **Projektdatei**: Version in `AdGuardTrayApp.csproj` aktualisieren
4. **Commit-Message**: GitHub Copilot generiert passende Nachricht
5. **Git-Operationen**: Commit erstellen, Tag setzen, Push
6. **GitHub Actions**: Automatisches Release-Build wird ausgelöst

## 🎨 Commit Message Format

### Standard-Format
```
<emoji> <type>: <beschreibung>

<optional: details>
<optional: breaking changes>
<optional: closes issues>

Co-authored-by: GitHub Copilot <copilot@github.com>
```

### Beispiele
```
🚀 feat: Windows Hello Fingerprint-Priorisierung hinzufügen
🐛 fix: Behebe ERROR_INVALID_PARAMETER bei Authentifizierung  
🔧 config: Erweitere GitHub Actions für Multi-Architecture
📝 docs: Aktualisiere README mit neuen Scripts
♻️ refactor: Vereinfache Windows Hello Implementation
```

## 🛡️ Sicherheitshinweise

- **Backup**: Scripts erstellen automatisch Git-Commits, stellen Sie sicher dass Ihr Code committed ist
- **Testing**: Verwenden Sie `-DryRun` um Änderungen zu testen
- **Remote**: Scripts pushen automatisch zu `origin` - stellen Sie sicher dass das korrekt ist
- **Permissions**: Setup-Script sollte als Administrator ausgeführt werden

## 🔍 Troubleshooting

### GitHub Copilot funktioniert nicht
```powershell
# Authentifizierung prüfen
github-copilot-cli auth status

# Neu authentifizieren
github-copilot-cli auth

# Ohne Copilot arbeiten
qcommit -SkipCopilot
release patch -SkipCopilot
```

### Node.js/Git Probleme
```powershell
# Setup erneut ausführen
.\scripts\setup.ps1 -UpdateTools
```

### PowerShell Aliase nicht verfügbar
```powershell
# Profil neu laden
. $PROFILE

# Oder manuell die Scripts ausführen
.\scripts\auto-release.ps1 -VersionType patch
```

## 📦 Dependencies

Das Setup-Script installiert automatisch:
- **Node.js** (v16+) - für GitHub Copilot CLI
- **GitHub CLI** - für GitHub Integration  
- **GitHub Copilot CLI** - für AI-Commit-Messages
- **PSGitHubCopilotCLI** - PowerShell Modul (optional)

## 🎯 Erweiterte Optionen

### Auto-Release
```powershell
# Parameter
-VersionType    # patch, minor, major
-CustomMessage  # Eigene Commit-Nachricht  
-DryRun        # Nur anzeigen, nicht ausführen
-SkipCopilot   # Ohne GitHub Copilot
-Force         # Ignoriere Warnungen
```

### Quick-Commit
```powershell
# Parameter
-Message       # Eigene Commit-Nachricht
-All          # Alle Dateien stagen
-Push         # Automatisch pushen
-SkipCopilot  # Ohne GitHub Copilot
```

## 🚀 CI/CD Integration

Die Scripts triggern automatisch die GitHub Actions Pipeline:
- **Push von Tags** (`v*`) startet Release-Build
- **Multi-Architecture** Builds (x64, x86, ARM64)
- **MSI-Installer** Generierung
- **GitHub Release** mit Artifacts

## 💡 Tipps

1. **Aliase verwenden**: `release patch` ist kürzer als `.\scripts\auto-release.ps1 -VersionType patch`
2. **DryRun nutzen**: Immer erst `-DryRun` um zu sehen was passiert
3. **Copilot trainieren**: Je mehr Sie es nutzen, desto besser werden die Commit-Messages
4. **Templates anpassen**: Bearbeiten Sie `.copilot/commit-messages.md` für projektspezifische Anpassungen
