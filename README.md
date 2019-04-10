# Welcome to Sally .NET - everyone can get emotional

Hey! Sally, your friendly Discord-Bot. She is another multipurpose bot with many request options.


# Overview

The following list shows the available commands at the moment. If you want to use commands you need a "$" as prefix.

## Game Commands

### Terraria-related:
Commands in this section need the prefix: **terraria**

- **mods**: shows all active mods on the terraria server

	*Example*: $terraria mods 

Commands in this section need the prefix: **rl**

- **setRank**: set a role with a given rocket league matchmaking rank

	*Available Parameters*:
	
	- a rank as string
	- a devision as number

	*Example*: 
	
	- $rl setRank Gold 3

## Random Commands

- **ping**: make a ping with process duration in ms

	*Example*: $ping
	
## User Commands

- **mute**: mute bot. there are no greeting/bye messages anymore
	
	*Example*: $mute

- **unmute**: unmute bot. you will receive greeting/bye messages again

	*Example*: $unmute

- **myxp**: shows your current level with exp needed to the next level

	*Example*: $myxp

## Trivia Commands

- **ask**: return a search result from wikipedia with the given paramater

	*Example*: $ask Rainbow

## Weather Commands

- **sub2weather**: subs to the weather service. you will get a daily notification of the current weather.

	*Available Parameters*:

	- location as string
	- time
		Allowed Format: h m

	*Example* : $sub2weather Paris 9h30m
	This will set the timer to 9:30 and location to paris.
	
- **unsub2weather**: remove the current weather subscription

	*Example*: $unsub2weather

- **currentWeather**: show the current weather of given location

	*Available Parameters*:
	
	- location as string

	*Example*: $currentWeather Paris
