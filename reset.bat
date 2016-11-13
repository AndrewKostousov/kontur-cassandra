@echo off

echo Cleaning storage...
rmdir /s /q Cassandra\db
if not exist Cassandra\db echo Done!

echo Removing logs...
rmdir /s /q Cassandra\logs
if not exist Cassandra\logs echo Done!