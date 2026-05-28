Set-ExecutionPolicy -Scope CurrentUser Unrestricted

foreach ($tfm in @("net8.0", "net10.0")) {
    dotnet publish -c Release -r linux-x64 --self-contained false -f $tfm
    $zipName = if ($tfm -eq "net10.0") { "ExternalLibrary.zip" } else { "ExternalLibrary_$tfm.zip" }
    Compress-Archive -Path ".\bin\Release\$tfm\linux-x64\publish\*" -DestinationPath $zipName -Force
}