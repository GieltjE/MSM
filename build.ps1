$bindir="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin"

& $bindir\MSBuild.exe /nr:True .\MSM.sln /target:Clean /p:Platform=x64 /p:Configuration=Release
& $bindir\MSBuild.exe /nr:True .\MSM.sln /target:Restore /p:Platform=x64 /p:Configuration=Release
& $bindir\MSBuild.exe /nr:True .\MSM.sln /target:Rebuild /p:Platform=x64 /p:Configuration=Release

& $bindir\MSBuild.exe /nr:True .\MSM.sln /target:Clean /p:Platform=x86 /p:Configuration=Release
& $bindir\MSBuild.exe /nr:True .\MSM.sln /target:Restore /p:Platform=x86 /p:Configuration=Release
& $bindir\MSBuild.exe /nr:True .\MSM.sln /target:Rebuild /p:Platform=x86 /p:Configuration=Release

$version = (get-item -Path 'bin\x64\Release\MSM.exe').VersionInfo.ProductVersion

Remove-Item Release -Recurse -Force
New-Item -Path "." -Name "Release" -ItemType "directory"

$sevenzipdir="C:\Program Files\7-Zip"
& $sevenzipdir\7z.exe a -t7z ".\Release\v$version-x64.7z" ".\bin\x64\Release\*" -mx9 -y
& $sevenzipdir\7z.exe a -t7z ".\Release\v$version-x86.7z" ".\bin\x64\Release\*" -mx9 -y

& $sevenzipdir\7z.exe a -tzip ".\Release\v$version-x64.zip" ".\bin\x86\Release\*" -mx9 -y
& $sevenzipdir\7z.exe a -tzip ".\Release\v$version-x86.zip" ".\bin\x86\Release\*" -mx9 -y