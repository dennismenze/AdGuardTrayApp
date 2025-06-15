# 🤖 GitHub Copilot Helper für Git Operations
# Gemeinsame Funktionen für Commit-Message Generation

function Get-GitCopilotCommitMessage {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Prompt,
        
        [Parameter(Mandatory=$false)]
        [string]$Context = ""
    )
    
    # Prüfe GitHub CLI
    if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
        throw "GitHub CLI nicht gefunden! Installiere mit: winget install GitHub.cli"
    }
    
    Write-Host "🤖 Verwende GitHub Copilot..." -ForegroundColor Cyan
    
    try {
        # Bereinige den Prompt von problematischen Zeichen
        $cleanPrompt = $Prompt -replace '"', "'" -replace '`', '' -replace '\r?\n', ' '
        $cleanPrompt = $cleanPrompt -replace '\s+', ' ' # Mehrfache Leerzeichen entfernen
        $cleanPrompt = $cleanPrompt.Trim()
        
        # Kürze den Prompt falls er zu lang ist
        if ($cleanPrompt.Length -gt 200) {
            $cleanPrompt = $cleanPrompt.Substring(0, 200) + "..."
        }        
        Write-Info "Verwendeter Prompt: $($cleanPrompt.Substring(0, [Math]::Min(50, $cleanPrompt.Length)))..."
        
        # Verwende Start-Process mit Array-Argumenten für bessere Escaping-Behandlung
        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = "gh"
        # Verwende ArgumentList anstatt Arguments String
        $psi.ArgumentList.Add("copilot")
        $psi.ArgumentList.Add("suggest")
        $psi.ArgumentList.Add("-t")
        $psi.ArgumentList.Add("git")
        $psi.ArgumentList.Add($cleanPrompt)
        $psi.UseShellExecute = $false
        $psi.RedirectStandardInput = $true
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.CreateNoWindow = $true
          $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $psi
        $null = $process.Start()  # Verhindere "True" Ausgabe
        
        # Sende "1" + Enter für "Copy command to clipboard"
        $process.StandardInput.WriteLine("1")
        $process.StandardInput.Close()
        
        # Lese Ausgabe
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $null = $process.WaitForExit()  # Verhindere ExitCode Ausgabe
        
        $copilotResult = @()
        if ($stdout) { $copilotResult += $stdout -split "`n" }
        if ($stderr) { $copilotResult += $stderr -split "`n" }
        
        if ($copilotResult) {
            # Konvertiere Array zu String für besseres Parsing
            $copilotText = $copilotResult -join " "
            
            # Verschiedene Patterns für Commit-Messages
            $patterns = @(
                'git commit -m ["'']([^"'']+)["'']',  # Einfache Message
                'git commit -m ["''](.+?)["''] -m ["''](.+?)["'']',  # Multi-line Message
                '# Suggestion:\s*git commit -m ["'']([^"'']+)["'']'  # Mit Suggestion Header
            )
              foreach ($pattern in $patterns) {
                if ($copilotText -match $pattern) {
                    $suggestedMessage = $matches[1]
                    
                    # Bereinige PowerShell-Artefakte und Encoding-Probleme
                    $suggestedMessage = $suggestedMessage -replace '^True\s*', ''  # Entferne "True" am Anfang
                    $suggestedMessage = $suggestedMessage -replace '^False\s*', '' # Entferne "False" am Anfang
                    $suggestedMessage = $suggestedMessage -replace '^\d+\s*', ''   # Entferne Zahlen am Anfang
                    $suggestedMessage = $suggestedMessage -replace '­ƒÜÇ', '🚀'
                    $suggestedMessage = $suggestedMessage -replace '├ä', 'Ä'
                    $suggestedMessage = $suggestedMessage -replace '├╝', 'ü'
                    $suggestedMessage = $suggestedMessage -replace '├ñ', 'ö'
                    $suggestedMessage = $suggestedMessage -replace 'Ã¤', 'ä'
                    $suggestedMessage = $suggestedMessage -replace 'Ã¼', 'ü'
                    $suggestedMessage = $suggestedMessage -replace 'Ã¶', 'ö'
                    $suggestedMessage = $suggestedMessage.Trim()  # Entferne Whitespace
                    
                    # Falls Multi-line Message, füge zweite Zeile hinzu
                    if ($matches.Count -gt 2 -and $matches[2]) {
                        $secondLine = $matches[2] -replace '├ä', 'Ä' -replace '├╝', 'ü' -replace '├ñ', 'ö'
                        $secondLine = $secondLine.Trim()
                        return $suggestedMessage + "`n`n" + $secondLine
                    } else {
                        return $suggestedMessage
                    }
                }            }
            
            # Fallback: Suche nach anderen Commit-Message Patterns
            foreach ($line in $copilotResult) {
                if ($line -match '^[🚀🐛🔧📝🎨♻️].*:.*' -and $line.Length -gt 10 -and $line.Length -lt 150) {
                    $cleanLine = $line -replace '­ƒÜÇ', '🚀' -replace '├ä', 'Ä' -replace '├╝', 'ü'
                    $cleanLine = $cleanLine -replace '^True\s*', '' -replace '^False\s*', '' -replace '^\d+\s*', ''
                    $cleanLine = $cleanLine.Trim()
                    return $cleanLine
                }
            }
        }
        
        throw "Keine brauchbare Commit-Message von Copilot erhalten. Ausgabe: $($copilotResult -join '; ')"
        
    } catch {
        throw "GitHub Copilot Fehler: $($_.Exception.Message)"
    }
}

function Get-CommitPromptForChanges {
    param(
        [Parameter(Mandatory=$false)]
        [string]$CommitType = "general"
    )
    
    # Hole staged changes
    $stagedFiles = git diff --cached --name-status
    $changedFilesText = ($stagedFiles | ForEach-Object { $_ }) -join ', '
    
    # Hole detaillierte Diff-Informationen
    $diffSummary = git diff --cached --stat --summary 2>$null
    $diffLines = git diff --cached --unified=1 2>$null | Select-Object -First 20
    
    # Erstelle Prompt basierend auf Commit-Type
    switch ($CommitType) {
        "release" {
            return @"
Create a German release commit message with emoji for these changes:

Files changed: $changedFilesText

Requirements:
- Use German language
- Start with 📦 release: emoji
- Mention version bump
- Use conventional commit format
- Keep under 72 characters for the first line
"@
        }
        default {
            return @"
Create a specific German git commit message with emoji for these changes:

Files changed: $changedFilesText

Diff summary:
$($diffSummary -join "`n")

Code changes context:
$($diffLines -join "`n")

Requirements:
- Use German language
- Start with appropriate emoji (🚀 feat, 🐛 fix, 🔧 config, 📝 docs, 🎨 style, ♻️ refactor)
- Be specific about what was changed, not just which files
- Use conventional commit format: "emoji type: specific description"
- Keep under 72 characters for the first line
"@
        }
    }
}
