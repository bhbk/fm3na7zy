
powershell -command "& { Remove-Item *.nupkg }"
powershell -command "& { Remove-Item *.tmp }"
powershell -command "& { if (Test-Path .\TestResults) { Remove-Item .\TestResults -Recurse -Force } }"
rem powershell -command "& { if (Test-Path .\Packages) { Remove-Item .\Packages -Recurse -Force } }"

cd Bhbk.Cli.Aurora
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Daemon.Aurora.FTP
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"

cd ..\Bhbk.Daemon.Aurora.FTP.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Daemon.Aurora.SFTP
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"

cd ..\Bhbk.Daemon.Aurora.SFTP.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Lib.Aurora
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Lib.Aurora.Data
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Lib.Aurora.Domain
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Mssql.Aurora
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
rem powershell -command "& { Remove-Item *.dbmdl }"

cd ..\Bhbk.WebApi.Aurora
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"

cd ..\Bhbk.WebApi.Aurora.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd..
