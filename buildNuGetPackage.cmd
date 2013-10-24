SET version="0.1"

msbuild /p:Configuration=Release Ninjector\Ninjector.csproj
MKDIR nuget\lib\net40
COPY Ninjector\bin\Release\*.dll nuget\lib\net40\

COPY Ninjector.nuspec nuget\

Tools\NuGet\nuget.exe pack nuget\Ninjector.nuspec -Version %version%

RMDIR nuget /S /Q