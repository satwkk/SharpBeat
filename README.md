# SHARPBEAT
A fast C# discord moderation and music streaming bot supporting multiple music platform APIs.

# Commands Available
-play <keyworld or url>
-skip
-leave
-listqueue
-owner
-addrole <username> <role>
-removerole <username> <role>

# Usage
1. Paste your bot token inside Program.cs

```c#
var token = "token goes here";
await client.LoginAsync( TokenType.Bot, token );
await client.StartAsync();
await Task.Delay( -1 );
```

2. Build and run !
