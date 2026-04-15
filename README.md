# ByondPatcher

BYOND ads patcher. Removes ads and the 30-second connection delay.

Supported versions:
- 512 and earlier
- 513-515
- 516+ (tested on 516.1680)

## Usage

### Download

Grab `ByondPatcher.exe` from the [latest release](https://github.com/SijyKijy/ByondPatcher/releases/latest) and run it.

### Build from source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet publish ./src -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
./publish/ByondPatcher.exe
```


