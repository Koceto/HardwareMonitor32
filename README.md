# Hardware Monitor 32
The goal of this App is to wrap *[HWInfo](https://www.hwinfo.com/)* and provide the selected statistics via API.  

## Requirements
HWInfo 32/64 version 6.42.
*Might work with newer version, but isn't tested.*

1. Enable "*Shared Memory Support*"
2. Enable "*Auto Start*"
3. Enable "*Minimize Sensors on Startup*"
4. Enable "*Minimize Sensors instead of closing*"

## Installation
Installation inscriptions for the API.

1. Download the latest zip from the release section
2. Unzip into a **permanent folder you don't plan on changing soon.**
3. Open Command Prompt with administrator permissions
4. Execute the following command: *sc create "HardwareMonitor32" binPath="**CHOSEN UNZIP FOLDER**\HardwareMonitor32\Release\HardwareMonitor32.exe"*  
You should receive this message: *[SC] CreateService SUCCESS*  
To delete the service replace *create* with *delete*.
5. Done.

**Important** - API should be reachable on http://localhost:5167/HardwareMonitor/Status