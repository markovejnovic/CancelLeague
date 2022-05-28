# CancelLeague

## What is `CancelLeague`?

I have a league of legends problem. Some of my friends told me to join playing league with them. Then I started to play too much and at this point the damn thing is sucking the life out of me.

So the obvious solution is to disable myself from playing league when I'm not playing with my friends. That's where `CancelLeague` comes in:

1. CancelLeague is a discord bot that allows everyone except blacklisted people to buy me time.
2. CancelLeague is a system windows process (meaning if it dies, the system BSODs).
3. CancelLeague should be run on start to make sure that the moment you boot up, you are banned from playing League.

## Code quality

Nonexistent and I really don't mind. This is a hacky solution anyways.

## Antiviruses

Eh, I'm expecting antiviruses to flag this program as a virus because I'm calling `RtlSetProcessIsCritical`, but the program is fine. Check the source.