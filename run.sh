#!/bin/bash
export PATH="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
cd "$HOME/Desktop/JobBot/JobBot"
./publish/JobBot >> "$HOME/Desktop/JobBot/JobBot/logs/jobbot.log" 2>&1
