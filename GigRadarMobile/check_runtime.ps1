Get-AppxPackage | Where-Object {$_.Name -like '*WindowsAppRuntime*'} | Select-Object Name, Version | Format-Table -AutoSize
