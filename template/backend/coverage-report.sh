#!/bin/bash

echo "Install tools if not present"
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

echo "Clean and build solution"
dotnet restore
dotnet build  Ambev.DeveloperEvaluation.sln --configuration Release --no-restore

echo "Run tests with coverage"
mkdir -p ./TestResults

dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj --no-restore --verbosity normal \
/p:CollectCoverage=true \
/p:CoverletOutputFormat=json \
/p:CoverletOutput=./TestResults/coverage-unit.json \
/p:Exclude="[*]*.Program,[*]*.Startup,[*]*.Migrations.*,[*]*.GlobalUsings,[*]Microsoft.CodeAnalysis.*"

dotnet test tests/Ambev.DeveloperEvaluation.Integration/Ambev.DeveloperEvaluation.Integration.csproj --no-restore --verbosity normal \
/p:CollectCoverage=true \
/p:CoverletOutputFormat=json \
/p:MergeWith=./TestResults/coverage-unit.json \
/p:CoverletOutput=./TestResults/coverage-integration.json \
/p:Exclude="[*]*.Program,[*]*.Startup,[*]*.Migrations.*,[*]*.GlobalUsings,[*]Microsoft.CodeAnalysis.*"

dotnet test tests/Ambev.DeveloperEvaluation.Functional/Ambev.DeveloperEvaluation.Functional.csproj --no-restore --verbosity normal \
/p:CollectCoverage=true \
/p:CoverletOutputFormat=\"json,cobertura\" \
/p:MergeWith=./TestResults/coverage-integration.json \
/p:CoverletOutput=./TestResults/coverage-final \
/p:Threshold=80 \
/p:ThresholdType=line \
/p:ThresholdStat=total \
/p:Exclude="[*]*.Program,[*]*.Startup,[*]*.Migrations.*,[*]*.GlobalUsings,[*]Microsoft.CodeAnalysis.*"

echo "Generate coverage report"
reportgenerator \
-reports:"./TestResults/coverage-final.cobertura.xml" \
-targetdir:"./TestResults/CoverageReport" \
-reporttypes:Html

echo "Removing temporary files"
rm -rf bin obj

echo ""
echo "Coverage report generated at TestResults/CoverageReport/index.html"
pause
