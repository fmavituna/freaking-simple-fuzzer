:: Get the version
powershell  (Get-Command "./fsf.exe").FileVersionInfo.ProductVersion > tmp_version.txt
SET /P VR= < tmp_version.txt
del tmp_version.txt

:: Del old package
del FSF*.zip
echo %date% - %time% > ReleaseTime.txt

:: Pack it with release-time + new name with the version
7z a -tzip -r "FSF-%VR%.zip" FSF.exe *.dll FSF.exe.config ..\Docs\licence.txt 
