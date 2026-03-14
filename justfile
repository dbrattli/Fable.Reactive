# Fable.Reactive development tasks

src_path := "src"
test_path := "test"

# List available recipes
default:
    @just --list

# --- Build ---

# Build the project
build:
    dotnet build {{src_path}}

# Build in Release mode
build-release:
    dotnet build --configuration Release

# Format source files
format:
    dotnet fantomas {{src_path}} {{test_path}}

# Restore dependencies and tools
restore:
    dotnet tool restore
    dotnet restore

# --- Packaging ---

# Create NuGet package
pack:
    dotnet build {{src_path}}
    dotnet pack {{src_path}} -c Release

# Create NuGet package with specific version (used in CI)
pack-version version:
    dotnet pack {{src_path}} -c Release -p:PackageVersion={{version}} -p:InformationalVersion={{version}}

# Run EasyBuild.ShipIt for release management
shipit *args:
    dotnet shipit --pre-release rc {{args}}

# --- Tests ---

# Run all tests
test:
    dotnet test

# Run tests with coverage
test-coverage:
    dotnet test --collect:"XPlat Code Coverage"
