set LUBAN_DLL=.\_luban_examples-main\Tools\Luban\Luban.dll
set CONF_ROOT=.\

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-bin ^
    -d bin  ^
    --conf .\luban.conf ^
    -x outputCodeDir=..\Client\Assets\Scripts\Runtime\Luban\Generate ^
    -x outputDataDir=..\Client\Assets\Resources\Luban

pause