
$path = $pwd
$acl = Get-Acl $path

$lsacl = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\LOCAL SERVICE","DeleteSubdirectoriesAndFiles, Write, ReadAndExecute, Synchronize","Allow")
$nsacl = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\NETWORK SERVICE","DeleteSubdirectoriesAndFiles, Write, ReadAndExecute, Synchronize","Allow")

$acl.SetAccessRule($lsacl)
$acl.SetAccessRule($nsacl)
$acl | Set-Acl $path

Get-Acl $path | fl
