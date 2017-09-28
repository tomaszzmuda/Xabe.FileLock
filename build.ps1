#!/bin/bash
dotnet restore
cd Xabe.FileLock
dotnet build /p:GenerateDocumentationFile=true
dotnet pack --no-build /p:GenerateDocumentationFile=true
