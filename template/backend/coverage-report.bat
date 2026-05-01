@ECHO OFF

REM Install tools if not present
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

REM Clean and build solution
dotnet restore Ambev.DeveloperEvaluation.sln
dotnet build Ambev.DeveloperEvaluation.sln --configuration Release --no-restore

REM Run tests with coverage
set COVERAGE_DIR=%CD%\TestResults
if not exist "%COVERAGE_DIR%" mkdir "%COVERAGE_DIR%"

dotnet test tests\Ambev.DeveloperEvaluation.Unit\Ambev.DeveloperEvaluation.Unit.csproj --no-restore --verbosity normal ^
/p:CollectCoverage=true ^
/p:CoverletOutputFormat=json ^
/p:CoverletOutput="%COVERAGE_DIR%\coverage-unit.json" ^
/p:Exclude="[*]*.Program%2c[*]*.Startup%2c[*]*.Migrations.*%2c[*]*.GlobalUsings%2c[*]Microsoft.CodeAnalysis.*"

dotnet test tests\Ambev.DeveloperEvaluation.Integration\Ambev.DeveloperEvaluation.Integration.csproj --no-restore --verbosity normal ^
/p:CollectCoverage=true ^
/p:CoverletOutputFormat=json ^
/p:MergeWith="%COVERAGE_DIR%\coverage-unit.json" ^
/p:CoverletOutput="%COVERAGE_DIR%\coverage-integration.json" ^
/p:Exclude="[*]*.Program%2c[*]*.Startup%2c[*]*.Migrations.*%2c[*]*.GlobalUsings%2c[*]Microsoft.CodeAnalysis.*"

dotnet test tests\Ambev.DeveloperEvaluation.Functional\Ambev.DeveloperEvaluation.Functional.csproj --no-restore --verbosity normal ^
/p:CollectCoverage=true ^
/p:CoverletOutputFormat="json%2ccobertura" ^
/p:MergeWith="%COVERAGE_DIR%\coverage-integration.json" ^
/p:CoverletOutput="%COVERAGE_DIR%\coverage-final" ^
/p:Threshold=80 ^
/p:ThresholdType=line ^
/p:ThresholdStat=total ^
/p:Exclude="[*]*.Program%2c[*]*.Startup%2c[*]*.Migrations.*%2c[*]*.GlobalUsings%2c[*]Microsoft.CodeAnalysis.*"

REM Generate coverage report
reportgenerator ^
-reports:"./TestResults/coverage-final.cobertura.xml" ^
-targetdir:"./TestResults/CoverageReport" ^
-reporttypes:Html

REM Removing temporary files
rmdir /s /q bin 2>nul
rmdir /s /q obj 2>nul

echo.
echo Coverage report generated at TestResults/CoverageReport/index.html
pause
