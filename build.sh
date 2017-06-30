#!/bin/bash
dotnet restore
dotnet test Xabe.FileLock.Test/
if [[ -z "${TRAVIS_TAG}" ]]; then 
	exit 0
else
	dotnet pack Xabe.FileLock -c Release -o nuget /p:PackageVersion=$TRAVIS_TAG
	dotnet nuget push Xabe.FileLock/nuget/*.nupkg -k $NUGET_API https://www.nuegt.org/api/v2/package
fi
