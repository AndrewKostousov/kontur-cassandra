@echo off

FOR /F "skip=2 tokens=2*" %%A IN ('REG QUERY "HKLM\Software\JavaSoft\Java Runtime Environment" /v CurrentVersion') DO set CurVer=%%B
FOR /F "skip=2 tokens=2*" %%A IN ('REG QUERY "HKLM\Software\JavaSoft\Java Runtime Environment\%CurVer%" /v JavaHome') DO set JAVA_HOME=%%B

set LOGS_DIR=Cassandra\logs
set DATA_DIR=Cassandra\db\data

if %1==2 Cassandra\apache-cassandra-2.2.8\bin\cassandra.bat
if %1==3 Cassandra\apache-cassandra-3.9\bin\cassandra.bat
