$env:BUILD_SOURCEBRANCH="refs/head/main"
$env:BUILD_REPOSITORY_NAME="dotnet-diagnostictests"
$env:SYSTEM_TEAMPROJECT="internal"
$env:BUILD_REASON="Manual"
$env:_HelixAccessToken="AAAElLXsw_KDuKk4NQeAWhgoVzw"

$env:TargetOS="Windows"
$env:TargetArch="x64"
$env:IsMono="false"
$env:IsBrowser="false"
$env:IsAndroid="false"
$env:IsCET="false"

./.dotnet/dotnet.exe msbuild -tl:off -bl:helix.binlog C:\Users\maxcharlamb\sources\diagnostics\eng\Helix\SendToHelix.proj