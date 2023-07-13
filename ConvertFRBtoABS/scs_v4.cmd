@echo off
%windir%\Microsoft.NET\Framework\v4.0.30319\csc /nologo /out:bin\ConvertFRBtoABS.exe *.cs Properties\*.cs Lib\*.cs
copy app.config bin\ConvertFRBtoABS.config
echo REPLACE 2.0 with 4.0>>bin\ConvertFRBtoABS.config
