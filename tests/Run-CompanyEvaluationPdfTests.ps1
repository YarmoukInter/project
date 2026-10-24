$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$webRoot = Join-Path $projectRoot 'Yarmouk/Yarmouk'
$testOutput = Join-Path $projectRoot 'tmp/qa-evaluation-pdf'
New-Item -ItemType Directory -Force $testOutput | Out-Null
# Build the application before running this script. Uses only in-memory Firebase fixtures.
& (Join-Path $webRoot 'bin/roslyn/csc.exe') /nologo "/out:$testOutput/CompanyEvaluationPdfTests.exe" "/reference:$webRoot/bin/Yarmouk.dll" "/reference:$webRoot/bin/Newtonsoft.Json.dll" /reference:System.Net.Http.dll (Join-Path $PSScriptRoot 'CompanyEvaluationPdfTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'PDF test compilation failed.' }
foreach ($name in @('Yarmouk', 'Newtonsoft.Json', 'PdfSharp-gdi', 'MigraDoc.DocumentObjectModel-gdi', 'MigraDoc.Rendering-gdi')) {
    Copy-Item (Join-Path $webRoot "bin/$name.dll") $testOutput
}
Set-Content (Join-Path $testOutput 'CompanyEvaluationPdfTests.exe.config') '<configuration><appSettings><add key="FirebaseDatabaseUrl" value="https://fixture.invalid" /></appSettings></configuration>'
Push-Location $testOutput
try {
    & ./CompanyEvaluationPdfTests.exe
    if ($LASTEXITCODE -ne 0) { throw 'PDF tests failed.' }
} finally { Pop-Location }
