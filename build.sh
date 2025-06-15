#!/bin/bash
# Build script for local development and testing

CONFIGURATION="Release"
RUNTIME="linux-x64"
ALL_PLATFORMS=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -r|--runtime)
            RUNTIME="$2"
            shift 2
            ;;
        -a|--all-platforms)
            ALL_PLATFORMS=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [options]"
            echo "Options:"
            echo "  -c, --configuration  Build configuration (Debug/Release)"
            echo "  -r, --runtime        Target runtime"
            echo "  -a, --all-platforms  Build for all platforms"
            echo "  -h, --help          Show this help"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

PROJECT_PATH="AdGuardTrayApp/AdGuardTrayApp.csproj"
OUTPUT_DIR="builds"

# Clean previous builds
if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
fi
mkdir -p "$OUTPUT_DIR"

if [ "$ALL_PLATFORMS" = true ]; then
    RUNTIMES=("win-x64" "win-arm64" "linux-x64" "linux-arm64" "osx-x64" "osx-arm64")
else
    RUNTIMES=("$RUNTIME")
fi

for RT in "${RUNTIMES[@]}"; do
    echo "🔨 Building for $RT..."
    
    BUILD_OUTPUT="$OUTPUT_DIR/$RT"
    
    # Build and publish
    dotnet publish "$PROJECT_PATH" \
        --configuration "$CONFIGURATION" \
        --runtime "$RT" \
        --self-contained true \
        --output "$BUILD_OUTPUT" \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:PublishTrimmed=false
    
    if [ $? -eq 0 ]; then
        echo "✅ Build for $RT completed successfully"
        
        # Create archive
        ARCHIVE_NAME="AdGuardTrayApp-$RT"
        if [[ $RT == win* ]]; then
            ARCHIVE_PATH="$OUTPUT_DIR/$ARCHIVE_NAME.zip"
            (cd "$BUILD_OUTPUT" && zip -r "../$ARCHIVE_NAME.zip" .)
        else
            ARCHIVE_PATH="$OUTPUT_DIR/$ARCHIVE_NAME.tar.gz"
            tar -czf "$ARCHIVE_PATH" -C "$BUILD_OUTPUT" .
        fi
        
        echo "📦 Archive created: $ARCHIVE_PATH"
    else
        echo "❌ Build for $RT failed"
    fi
done

echo ""
echo "📋 Build summary:"
ls -la "$OUTPUT_DIR"
