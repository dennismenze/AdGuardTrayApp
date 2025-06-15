# Build script for local development and testing
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$AllPlatforms
)

$projectPath = "AdGuardTrayApp/AdGuardTrayApp.csproj"
$outputDir = "builds"

# Clean previous builds
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

if ($AllPlatforms) {
    $runtimes = @(
        "win-x64",
        "win-arm64", 
        "linux-x64",
        "linux-arm64",
        "osx-x64",
        "osx-arm64"
    )
} else {
    $runtimes = @($Runtime)
}

foreach ($rt in $runtimes) {
    Write-Host "Building for $rt..." -ForegroundColor Green
    
    $buildOutput = "$outputDir/$rt"
    
    # Build and publish
    dotnet publish $projectPath `
        --configuration $Configuration `
        --runtime $rt `
        --self-contained true `
        --output $buildOutput `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishTrimmed=false
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build for $rt completed successfully" -ForegroundColor Green
        
        # Create archive
        $archiveName = "AdGuardTrayApp-$rt"
        if ($rt.StartsWith("win")) {
            $archivePath = "$outputDir/$archiveName.zip"
            Compress-Archive -Path "$buildOutput/*" -DestinationPath $archivePath -Force
        } else {
            $archivePath = "$outputDir/$archiveName.tar.gz"
            tar -czf $archivePath -C $buildOutput .
        }
        
        Write-Host "📦 Archive created: $archivePath" -ForegroundColor Blue
    } else {
        Write-Host "❌ Build for $rt failed" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Build summary:" -ForegroundColor Yellow
Get-ChildItem $outputDir -Name | ForEach-Object { 
    Write-Host "  📁 $_" -ForegroundColor Cyan 
}
