# -*- coding: utf-8 -*-
"""
Refocuses and unfocuses the window every 30s to ensure

@author: OTA_Des001
"""

import pyautogui as pag
import time

while True:
    pag.moveTo(500,500)
    pag.click()
    pag.typewrite(['d','e','a','d','b','e','e','f'], interval=0.2)
    time.sleep(9)
    pag.moveTo(100,10)
    pag.click()
    pag.typewrite(['d','e','a','d','b','e','e','f'], interval=0.2)
    time.sleep(1)
