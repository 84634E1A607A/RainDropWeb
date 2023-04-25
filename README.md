# RainDrop Web

## Description

This is a cross-platform and flexible version of the offline [Yushiyan](http://yushiyan.net/yushiyan) virtual instrument
interface.

## Features

- Cross-platform (currently supports Windows x64, Linux x64, and macOS x64 / arm.)
- Web-based (However, .Net core is required to run the server.)
- Open-source (The communication protocol is guessed by monitoring the serial traffic.)
- Flexible (Resize the window to change the look!)

## How to develop

1. Install [.Net Core 7](https://dotnet.microsoft.com/download/dotnet-core/7.0). I'm not sure whether installing the
   ASP.Net Core Runtime is enough. It should be though, but it cannot compile this program from the source.
2. Find a browser. Either Edge, Chrome, Safari, Firefox should be fine. Don't challenge me with IE6.
3. Clone this repository. (You may need to install Git first. I'm not going to tell you how to install Git.)
4. Run `dotnet build` in the root directory of this repository. Then `dotnet run`.
5. Open your browser and go to `http://localhost:5186/` (This should be the default port).

## Adding a new architecture

1. Go to [FTDI Driver Download Page](https://ftdichip.com/drivers/d2xx-drivers/) and find the driver for your OS.
2. Copy the library file to the project directory and replace the existing one (`ftdi2xx.dll`, `libftdi2xx.so`,
   or `libftdi2xx.dylib` (currently missing, for MacOS)).
3. Run and see if it goes well.

## How to contribute

PRs and Issues are welcome.