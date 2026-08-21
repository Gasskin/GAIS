set LUBAN_DLL=.\_luban_examples-main\Tools\Luban\Luban.dll

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-simple-json ^
    -d json  ^
    --conf .\luban.conf ^
    -x outputCodeDir=.\output_debug_json\Code ^
    -x outputDataDir=.\output_debug_json

pause