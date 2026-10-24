$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$web = Join-Path $root 'Yarmouk/Yarmouk'
$testOutput = Join-Path $root 'output/completion-tests'
New-Item -ItemType Directory -Force $testOutput | Out-Null
& (Join-Path $web 'bin/roslyn/csc.exe') /nologo /utf8output "/out:$testOutput/CompletionWorkflowTests.exe" "/reference:$web/bin/Yarmouk.dll" "/reference:$web/bin/Newtonsoft.Json.dll" /reference:System.Net.Http.dll (Join-Path $PSScriptRoot 'CompletionWorkflowTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed' }
Copy-Item (Join-Path $web 'bin/Yarmouk.dll'),(Join-Path $web 'bin/Newtonsoft.Json.dll') $testOutput
Set-Content (Join-Path $testOutput 'CompletionWorkflowTests.exe.config') '<configuration><appSettings><add key="FirebaseDatabaseUrl" value="https://fixture.invalid" /></appSettings></configuration>'
& (Join-Path $testOutput 'CompletionWorkflowTests.exe') (Join-Path $testOutput 'synthetic-completion-report.pdf')
if ($LASTEXITCODE -ne 0) { throw 'Regression tests failed' }
