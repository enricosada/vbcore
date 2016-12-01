@ECHO OFF

CD src\console
RD /s /q obj
RD /s /q bin
DEL project.lock.json

CD ..\dotnet-compile-vbc
RD /s /q obj
RD /s /q bin
DEL project.lock.json

CD ..\..
