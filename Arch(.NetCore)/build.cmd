
rem call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
rem dotnet tool install Octopus.DotNet.Cli --global
powershell -command "& { write-output 2019.06.05.2230 | out-file -filepath .\version.tmp -nonewline -encoding ascii }"
rem powershell -command "& { get-date -format yyyy.M.d.HHmm | out-file -filepath .\version.tmp -nonewline -encoding ascii }"
set /p VERSION=< .\version.tmp

rem build and test .net framework assemblies...
nuget restore Bhbk.Lib.Aurora.Data.EF6\Bhbk.Lib.Aurora.Data.EF6.csproj -SolutionDirectory . -Verbosity quiet

rem build and test .net standard/core assemblies...
dotnet restore Bhbk.WebApi.Aurora.sln --verbosity quiet
dotnet build Bhbk.WebApi.Aurora.sln --configuration Release --verbosity quiet /p:platform=x64
rem dotnet test Bhbk.Daemon.Aurora.Tests\Bhbk.Daemon.Aurora.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\
rem dotnet test Bhbk.WebApi.Aurora.Tests\Bhbk.WebApi.Aurora.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\

rem package .net standard/core assemblies...
dotnet pack Bhbk.Lib.Aurora\Bhbk.Lib.Aurora.csproj -p:PackageVersion=%VERSION% --output . --configuration Release /p:platform=x64
dotnet publish Bhbk.Cli.Aurora\Bhbk.Cli.Aurora.csproj --output Bhbk.Cli.Aurora\bin\Release\netcoreapp3.1\publish\ --configuration Release /p:platform=x64
dotnet publish Bhbk.Daemon.Aurora\Bhbk.Daemon.Aurora.csproj --output Bhbk.Daemon.Aurora\bin\Release\netcoreapp3.1\publish --configuration Release /p:platform=x64
dotnet publish Bhbk.WebApi.Aurora\Bhbk.WebApi.Aurora.csproj --output Bhbk.WebApi.Aurora\bin\Release\netcoreapp3.1\publish\ --configuration Release /p:platform=x64
dotnet octo pack --id=Bhbk.Cli.Aurora --version=%VERSION% --basePath=Bhbk.Cli.Aurora\bin\Release\netcoreapp3.1\publish\ --outFolder=. --overwrite
dotnet octo pack --id=Bhbk.Daemon.Aurora --version=%VERSION% --basePath=Bhbk.Daemon.Aurora\bin\Release\netcoreapp3.1\publish\ --outFolder=. --overwrite
dotnet octo pack --id=Bhbk.WebApi.Aurora --version=%VERSION% --basePath=Bhbk.WebApi.Aurora\bin\Release\netcoreapp3.1\publish\ --outFolder=. --overwrite

set VERSION=
rem dotnet tool uninstall Octopus.DotNet.Cli --global
rem powershell -command "& { update-package -reinstall }"
