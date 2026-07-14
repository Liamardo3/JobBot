#!/bin/bash
export PATH="/opt/homebrew/bin:/usr/local/bin:/usr/bin:/bin"
export HOME="/Users/liamardo"
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
cd /Users/liamardo/Desktop/JobBot/JobBot
echo "run.sh started at $(date)" >> /Users/liamardo/Desktop/JobBot/JobBot/logs/jobbot.log
./publish/JobBot >> /Users/liamardo/Desktop/JobBot/JobBot/logs/jobbot.log 2>&1
echo "run.sh finished at $(date)" >> /Users/liamardo/Desktop/JobBot/JobBot/logs/jobbot.log
