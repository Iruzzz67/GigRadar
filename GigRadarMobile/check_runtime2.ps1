Get-AppxPackage | Where-Object {$_.Name -like '*WinAppRuntime*' -or $_.Name -like '*DDLM*'} | Select-Object Name, Version, InstallLocation | Format-Table -AutoSize
