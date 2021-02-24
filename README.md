# Welcome to Sally

![alt text](https://sallynet.blob.core.windows.net/content/sally_banner.jpg "Sally's mood banner")

[![GitHub license](https://img.shields.io/github/license/Naereen/StrapDown.js.svg)](https://github.com/Naereen/StrapDown.js/blob/master/LICENSE) [![Build Status](https://travis-ci.com/Chilllli/Sally.NET.svg?token=e9oxuon9Djni1ERDenE9&branch=master)](https://travis-ci.com/Chilllli/Sally.NET) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/3dce132ba96d4ba69cb0de2479196363)](https://www.codacy.com?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Chilllli/Sally.NET&amp;utm_campaign=Badge_Grade) [![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://GitHub.com/Naereen/StrapDown.js/graphs/commit-activity)

[![forthebadge](https://forthebadge.com/images/badges/built-with-love.svg)](https://forthebadge.com) [![forthebadge](https://forthebadge.com/images/badges/made-with-c-sharp.svg)](https://forthebadge.com) [![ForTheBadge powered-by-electricity](http://ForTheBadge.com/images/badges/powered-by-electricity.svg)](http://ForTheBadge.com)

Sally is a friendly multipurpose discordbot. She provides many game integrations and interesting APIs. It is possible to customize Sally with self made plugins. The plugins need to be written in C#.

## Quick Links

**Homepage**:  
<https://its-sally.net>

**Sally Invite Link**:  
<https://invite.its-sally.net>

**Join the Discord server, if you are looking for support or just wanna hang out!**  
<https://discord.gg/hjPRKyY>

## Overview

* [Commands](#commands)
* [Features](#features)
* [Self-hosting](#self-hosting)
  * [Windows](#windows)
  * [Linux](#linux)
  * [Mac](#mac)
  * [Docker](#docker)
* [Support](#support)

## Commands

All Commands can be found on the webpage: <https://its-sally.net/commands>.
It will be updated regularly.

## Features

### Integrations - Games

* Rocket League
* Terraria
* Oldschool Runescape
* Osu! (planned)
* League of Legends (planned)

### Integrations - APIs

* Wikipedia
* OpenWeather
* Konachan
* Cleverbot
* more planned

### Build-In

* Command suggestion
* YouTube music player
* Mood system
* Level and rankup system
* Livechat replies
* Service subscription

## Self-hosting

* Prerequisite:
  * MySQL or (planned sqlite)
  * Dotnet 3.0 or newer

### Windows

coming soon!

### Linux

1. Install MySql Server  
Guide: [How to install a mysql server on Linux 18.04](https://www.digitalocean.com/community/tutorials/how-to-install-mysql-on-ubuntu-18-04)

2. Install .NET Core  
Guide: [How to install .NET Core on Linux 18.04](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#1804-)

3. Clone this repository

        git clone https://github.com/Chilllli/Sally.NET.git

4. Move into repo directory

        cd Sally.NET

5. Compile release build

        dotnet build Sally.NET.sln -c Release

6. Start the bot for the first time  
**Note: The bot will crash because the config file is missing!**

        dotnet Sally/bin/Release/netcoreapp3.0/Sally.dll

7. Create a file named "configuration.json" under `<git repo root>/Sally/bin/Release/netcoreapp3.0/config`  
Paste in following structure:

        {
        "token":"",
        "db_user":"",
        "db_database":"",
        "db_password":"",
        "db_host":"",
        "radioControlChannel":"",
        "meId":"",
        "gainedXp":"",
        "xpTimerInMin":"",
        "WeatherPlace":"",
        "WeatherApiKey":"",
        "CleverApi":""
        }
   **Note: You need to provied your own values for these properties!**

8. Run the bot again

        dotnet Sally/bin/Release/netcoreapp3.0/Sally.dll

    Now the bot should run just fine. If something not working, you may check your credentials.

   **Note: I recommend using tmux, then the bot can run in the background!**  
   Guide: [Getting started with tmux](https://linuxhandbook.com/tmux/)

### Mac

coming soon!

### Docker

coming soon!

## Support

You can directly support Sally and me via Patreon: <https://patreon.com/sallydev>

**Thanks for tuning in!** (づ｡◕‿‿◕｡)づ
