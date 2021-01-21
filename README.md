# Ready. Set. Tarkov!

Ready. Set. Tarkov! is an open source tool for the game Escape from Tarkov.

## What it does

Ready. Set. Tarkov! reads the application logs provided by the EscapeFromTarkov.exe to let you know when your match is about start by flashing the Tarkov taskbar icon orange.

## Concerns

This program does not read any part of the EscapeFromTarkov.exe memory. It soley reads the text logs in your game install folder. It then uses the `FlashWindow` function from `user32.dll` to flash the Tarkov taskbar icon.

All pinvoke calls are in the file `User32.cs`. You can browse that to see how I interact with the running processes (getting the exe location to know where logs are and finding the correct window handle).

### Possible Future Features

- Sound customization
- Swap foreground window on player spawn
- Toast notifications
