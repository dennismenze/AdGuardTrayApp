# 🛡️ AdGuard Tray App - GitHub Copilot Commit Instructions

## Format Guidelines
Verwende folgendes Format für Commit-Nachrichten:

```
<emoji> <type>: <kurze beschreibung>

<optional: detaillierte beschreibung>
<optional: breaking changes>
<optional: schließt issues>
```

## Emoji Guide für Commit-Types

### 🎯 Hauptkategorien
- 🚀 **feat**: Neue Features
- 🐛 **fix**: Bugfixes
- 🔧 **config**: Konfigurationsänderungen
- 📝 **docs**: Dokumentation
- 🎨 **style**: Code-Formatierung
- ♻️ **refactor**: Code-Refactoring
- ⚡ **perf**: Performance-Verbesserungen
- ✅ **test**: Tests hinzufügen/ändern
- 🔨 **build**: Build-System/Abhängigkeiten
- 👷 **ci**: CI/CD-Änderungen
- 🔀 **merge**: Merge-Commits
- ⏪ **revert**: Commits rückgängig machen

### 🛡️ AdGuard-spezifische Emojis
- 🔐 **auth**: Windows Hello/Authentifizierung
- 🌐 **api**: AdGuard API-Integration
- 🖥️ **ui**: System Tray/GUI-Änderungen
- ⏰ **timer**: Timer-Funktionalität
- 🔄 **state**: Zustandsverwaltung
- 📦 **deploy**: Deployment/Release
- 🛠️ **msi**: MSI-Installer-Änderungen

## Regeln
1. **Sprache**: Deutsch für Beschreibungen
2. **Länge**: Erste Zeile max. 72 Zeichen
3. **Imperativ**: "Add" nicht "Added"
4. **Kontext**: Erkläre WARUM, nicht nur WAS
5. **Breaking Changes**: Mit "BREAKING CHANGE:" kennzeichnen
6. **Issues**: Mit "Fixes #123" oder "Closes #123" verknüpfen

## Beispiele

### ✅ Gute Commit-Messages
```
🚀 feat: Windows Hello Fingerprint-Priorisierung hinzufügen

- Fingerabdruck-Scanner wird nun bevorzugt vor PIN/Gesicht
- Verbesserte Flags für optimale biometrische Authentifizierung
- Fallback-Mechanismus für bessere Kompatibilität

Fixes #45
```

```
🐛 fix: Fehlercode 87 bei Windows Hello Authentifizierung beheben

- ERROR_INVALID_PARAMETER durch ungültige CREDUI_INFO Struktur
- Hinzufügung von Parametervalidierung und Fallback-Methode
- Verbesserte Fehlerdiagnose mit detailliertem Logging

Closes #87
```

```
🔧 config: Release-Pipeline für Multi-Architecture erweitern

- Support für x64, x86 und ARM64 hinzugefügt
- MSI-Installer-Generierung für alle Architekturen
- Verbesserte Artifact-Verwaltung in GitHub Actions
```

### ❌ Schlechte Commit-Messages
```
fix bug                    // Zu unspezifisch
Fixed the authentication   // Falsche Zeitform
WIP                       // Nicht informativ
```

## Spezielle Hinweise für AdGuard Tray App
- **Security**: Sicherheitsrelevante Änderungen immer mit 🔐 markieren
- **API Changes**: AdGuard API-Änderungen mit 🌐 und Version erwähnen
- **UI Changes**: Tray-Icon/Menu-Änderungen mit 🖥️ kennzeichnen
- **Breaking Changes**: Bei Config-Format-Änderungen BREAKING CHANGE verwenden

## Co-Author Format
Wenn GitHub Copilot bei der Erstellung geholfen hat:
```
Co-authored-by: GitHub Copilot <copilot@github.com>
```
