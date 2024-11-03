import tkinter as tk
import math
import time
import paho.mqtt.client as mqtt
import threading
import argparse

# MQTT 設定
#MQTT_BROKER = "test.mosquitto.org"
MQTT_BROKER = "127.0.0.1"
MQTT_PORT = 1883
MQTT_TOPIC = "temperature"

# 初始化 MQTT 客戶端
mqtt_client = mqtt.Client()

def on_connect(client, userdata, flags, rc):
    print(f"Connected with result code {rc}\nID:{ID}")

mqtt_client.on_connect = on_connect
mqtt_client.connect(MQTT_BROKER, MQTT_PORT, 60)

# 全域變數
ID = ""

# 更新顯示器
def update_display(value):
    display_str = f"{value:.2f}"
    canvas.itemconfig(display_text, text=display_str)

# 發送 MQTT 訊息
def send_mqtt_message(value):
    message = f"ID:{ID},Temperature:{value:.2f}"
    mqtt_client.publish(MQTT_TOPIC, message)

# 更新數據和顯示器
def update_data():
    while True:
        t = time.time() % 50  # 50 秒週期
        value = 15 * math.sin(2 * math.pi * t / 50) + 15  # 範圍從 -10 到 40
        update_display(value)
        send_mqtt_message(value)
        time.sleep(1)

# 建立主窗口
root = tk.Tk()
root.title("數字顯示器模擬")

# 建立畫布
canvas = tk.Canvas(root, width=300, height=100)
canvas.pack()

# 顯示大大的數字
display_text = canvas.create_text(150, 50, text="0.00", font=("Arial", 48), fill="black")

# 啟動 MQTT 客戶端
mqtt_client.loop_start()

# 啟動數據更新線程
threading.Thread(target=update_data, daemon=True).start()

# 處理命令列引數
parser = argparse.ArgumentParser(description="MQTT Simulator with large digit display")
parser.add_argument("--id", required=True, help="Device ID")
args = parser.parse_args()

# 設定 ID
ID = args.id

# 運行主循環
root.mainloop()

