@echo off

SET BENCHMARKS_WORKING_DIR="TimeSeries\Benchmarks\bin\Debug"

%BENCHMARKS_WORKING_DIR%\Benchmarks.exe
python gen-images.py .
