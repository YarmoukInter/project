$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$webRoot = Join-Path $projectRoot 'Yarmouk/Yarmouk'
$testOutput = Join-Path $projectRoot 'tmp/qa-regression'
New-Item -ItemType Directory -Force $testOutput | Out-Null
# Build the application before running this script.
& (Join-Path $webRoot 'bin/roslyn/csc.exe') /nologo "/out:$testOutput/WorkflowRegressionTests.exe" "/reference:$webRoot/bin/Yarmouk.dll" "/reference:$webRoot/bin/Newtonsoft.Json.dll" /reference:System.Net.Http.dll (Join-Path $PSScriptRoot 'WorkflowRegressionTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Regression test compilation failed.' }
Copy-Item (Join-Path $webRoot 'bin/Yarmouk.dll'),(Join-Path $webRoot 'bin/Newtonsoft.Json.dll') $testOutput
Set-Content (Join-Path $testOutput 'WorkflowRegressionTests.exe.config') '<configuration><appSettings><add key="FirebaseDatabaseUrl" value="https://fixture.invalid" /></appSettings></configuration>'
& (Join-Path $testOutput 'WorkflowRegressionTests.exe')
if ($LASTEXITCODE -ne 0) { throw 'Regression tests failed.' }
