$zipName = "QunatumStorageRedux.zip"
Remove-Item ../$zipName

$version = "0.1.0"
Write-Host 'Version:'$version

New-Item -ItemType directory -Path ../QuantumStorageReduxRelease/ -Force
Copy-Item "About","Assemblies","Defs","Languages","Textures" -Destination ../QuantumStorageReduxRelease/ -Recurse

$aboutFile = "../QuantumStorageReduxRelease/About/About.xml"
(Get-Content $aboutFile) -replace "{{version}}", $version | Set-Content $aboutFile

& 7z a ../$zipName ../QuantumStorageReduxRelease/
