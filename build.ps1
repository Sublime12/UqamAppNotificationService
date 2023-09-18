rmdir build -Recurse -Force

echo "Building for Windows"
echo "First, install the dotnet sdk"

dotnet publish --sc --os win -c Production -o ./build/windows/

echo "Then create a file named appsettings.json in the build/windows folder and change the PermanentCode and the Password to your own values"

