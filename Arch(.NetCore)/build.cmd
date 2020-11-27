
rem call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
rem dotnet tool install Octopus.DotNet.Cli --global
powershell -command "& { write-output 2020.11.25.1900 | out-file -filepath .\version.tmp -nonewline -encoding ascii }"
rem powershell -command "& { get-date -format yyyy.M.d.HHmm | out-file -filepath .\version.tmp -nonewline -encoding ascii }"
set /p VERSION=< .\version.tmp

rem build and test .net framework assemblies...
nuget restore Bhbk.Lib.Aurora.Data_EF6\Bhbk.Lib.Aurora.Data_EF6.csproj -SolutionDirectory . -Verbosity quiet

rem build and test .net standard/core assemblies...
dotnet restore Bhbk.Aurora.sln --verbosity quiet
dotnet build Bhbk.Aurora.sln --configuration Release --verbosity quiet /p:platform=x64
rem dotnet test Bhbk.Daemon.Aurora.FTP.Tests\Bhbk.Daemon.Aurora.FTP.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\
rem dotnet test Bhbk.Daemon.Aurora.SFTP.Tests\Bhbk.Daemon.Aurora.SFTP.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\
rem dotnet test Bhbk.WebApi.Aurora.Tests\Bhbk.WebApi.Aurora.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\

rem package .net standard/core assemblies...
dotnet pack Bhbk.Lib.Aurora\Bhbk.Lib.Aurora.csproj -p:PackageVersion=%VERSION% --output . --configuration Release /p:platform=x64
dotnet publish Bhbk.Cli.Aurora\Bhbk.Cli.Aurora.csproj --output Bhbk.Cli.Aurora\bin\x64\Release\net5.0\publish\ --configuration Release /p:platform=x64
dotnet publish Bhbk.Daemon.Aurora.FTP\Bhbk.Daemon.Aurora.FTP.csproj --output Bhbk.Daemon.Aurora.FTP\bin\x64\Release\net5.0\publish\ --configuration Release /p:platform=x64
dotnet publish Bhbk.Daemon.Aurora.SFTP\Bhbk.Daemon.Aurora.SFTP.csproj --output Bhbk.Daemon.Aurora.SFTP\bin\x64\Release\net5.0\publish\ --configuration Release /p:platform=x64
dotnet publish Bhbk.WebApi.Aurora\Bhbk.WebApi.Aurora.csproj --output Bhbk.WebApi.Aurora\bin\x64\Release\net5.0\publish\ --configuration Release /p:platform=x64
dotnet octo pack --id=Bhbk.Cli.Aurora --version=%VERSION% --basePath=Bhbk.Cli.Aurora\bin\x64\Release\net5.0\publish\ --outFolder=. --overwrite
dotnet octo pack --id=Bhbk.Daemon.Aurora.FTP --version=%VERSION% --basePath=Bhbk.Daemon.Aurora.FTP\bin\x64\Release\net5.0\publish\ --outFolder=. --overwrite
dotnet octo pack --id=Bhbk.Daemon.Aurora.SFTP --version=%VERSION% --basePath=Bhbk.Daemon.Aurora.SFTP\bin\x64\Release\net5.0\publish\ --outFolder=. --overwrite
dotnet octo pack --id=Bhbk.WebApi.Aurora --version=%VERSION% --basePath=Bhbk.WebApi.Aurora\bin\x64\Release\net5.0\publish\ --outFolder=. --overwrite

set VERSION=

rem dotnet tool uninstall Octopus.DotNet.Cli --global
rem powershell -command & { update-package -reinstall }
