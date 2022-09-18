# set default images name if no argument is passed in
IMAGE=${1:-screen4:latest}

echo "Clean the build output folder..."
rm -rf ./publish

echo "Build service in release..."
dotnet build -c release ../../ScreenProcess/ScreenProcess.csproj

echo "Publish project..."
dotnet publish -c release ../../ScreenProcess/ScreenProcess.csproj -o publish
rm ./publish/*.Development.json

echo "Build local container"
docker build -t $IMAGE .



# docker run --rm -e Settings__TickerEmailAccount=$Settings__TickerEmailAccount -e Settings__TickerEmailPWD=$Settings__TickerEmailPWD screen4 ticker
# winpty docker run -it --rm -v /$(pwd)/data:/data -e Settings__TickerEmailAccount=$Settings__TickerEmailAccount -e Settings__TickerEmailPWD=$Settings__TickerEmailPWD screen4