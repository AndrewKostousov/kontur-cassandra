@echo off

set JAVA_HOME=C:\Program Files\Java\jdk1.8.0_112

set LOGS_DIR=Cassandra\logs
set DATA_DIR=Cassandra\db\data

if %1==2 Cassandra\apache-cassandra-2.2.8\bin\cassandra.bat
if %1==3 Cassandra\apache-cassandra-3.9\bin\cassandra.bat
