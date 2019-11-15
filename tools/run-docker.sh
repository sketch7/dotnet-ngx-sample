#!/bin/bash
SOLUTION=$(node -p "require('./package.json').vsBuildSolution")
IMAGE=dotnet-ngx

npm run build:rel
dotnet publish $SOLUTION -c Release -o ./obj/Docker/publish
docker build -t $IMAGE ./
docker run -p 2200:80 -e 'ASPNETCORE_ENVIRONMENT=Production' -e 'APP_VERSION=1.0.0' $IMAGE