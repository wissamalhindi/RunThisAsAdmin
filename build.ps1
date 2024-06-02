nuget.exe restore .\RunThisAsAdmin.sln
MSBuild.exe .\RunThisAsAdmin.sln -t:Rebuild -p:Configuration=Release
$ArtifactsFolderPath = ".\artifacts"
If (Test-Path $ArtifactsFolderPath) { Remove-Item -Path $ArtifactsFolderPath -Force -Recurse }
New-Item -Path $ArtifactsFolderPath -ItemType "directory"
Copy-Item ".\RunThisAsAdmin\bin\Release\net48\*.exe" -Destination $ArtifactsFolderPath
