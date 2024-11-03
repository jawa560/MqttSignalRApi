@echo off

REM 啟動 10 個 MQTT 客戶端，每個客戶端有不同的 ID
for /L %%i in (1,1,10) do (
    echo 啟動 Device00%%i
    start python mqtt_simulator.py --id Device00%%i
)