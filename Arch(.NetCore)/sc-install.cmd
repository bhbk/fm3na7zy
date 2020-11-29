
pushd Bhbk.Daemon.Aurora.SFTP\bin\x64\Debug\net5.0\

rem sc.exe create BhbkDaemonAuroraSFTP binpath= %cd%\Bhbk.Daemon.Aurora.SFTP.exe displayname= "Bhbk Aurora SFTP" type= own obj= "NT AUTHORITY\NETWORK SERVICE" password= "" start= delayed-auto
sc.exe create BhbkDaemonAuroraSFTP binpath= %cd%\Bhbk.Daemon.Aurora.SFTP.exe displayname= "Bhbk Aurora SFTP" type= own start= delayed-auto

powershell -command "& { Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted }"
powershell -command "& '%~dpn0.ps1'"
powershell -command "& { Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Restricted }"
popd

timeout 3

sc.exe start BhbkDaemonAuroraSFTP
