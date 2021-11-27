# Ready. Set. Tarkov!

<img src="images/RST.png" height=100 align=right>

Ready. Set. Tarkov! is an open source tool for the game Escape from Tarkov.

## What it does

Ready. Set. Tarkov! reads the application logs provided by the EscapeFromTarkov.exe to let you know when your match is about start by flashing the Tarkov taskbar icon orange.

## Usage

Download the latest [release](https://www.github.com/InKahootz/ReadySetTarkov/releases/latest/download/ReadySetTarkov.zip). Extract the zip. Run the executable.  
You can confirm it's running by looking for the red icon in the tray. Right click to see options.

Nothing is required as a prerequisite to run as it's a self-contained .NET 6 application. This is also why the executable is a bit on the larger side for a relatively simple app.

You can use the .NET 6 Desktop dependent app which is a lot smaller but you will first need to download the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-6.0.0-windows-x64-installer).

## Concerns

This program does not read any part of the EscapeFromTarkov.exe memory. It soley reads the text logs in your game install folder. It then uses the `FlashWindow` function from `user32.dll` to flash the Tarkov taskbar icon.

All pinvoke calls are in the file `NativeMethods.cs`. You can browse that to see how I interact with the running processes (getting the exe location to know where logs are and finding the correct window handle).

I and my close Tarkov friends have been using the program since the first release (Jan 2021) without any bans.

### Developing (Windows Only)

#### Visual Studio

Just build and run.

#### CLI

```cmd
dotnet build
dotnet run -p ReadySetTarkov
```

### Possible Future Features

- Sound customization
- Swap foreground window on player spawn
- Toast notifications

### Credits

Sounds from [Notification Sounds](https://notificationsounds.com/)