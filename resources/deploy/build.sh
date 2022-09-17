# set default images name if no argument is passed in
IMAGE=${1:-screen4:latest}

echo "Clean the build output folder..."
rm -rf ./publish

echo "Build service in release..."
dotnet build -c release ../../ScreenProcess/ScreenProcess.csproj

echo "Publish project..."
dotnet publish -c release ../../ScreenProcess/ScreenProcess.csproj -o publish

echo "Build local container"
docker build -t $IMAGE .