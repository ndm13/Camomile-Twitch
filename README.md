Okay, so Twitch feels like giving away free games to Twitch Prime
members, which is cool.  But when you click on the helpful links it
creates, it forces the Twitch Desktop app to open and launch the
game for you, which is a little annoying.  Plus it's not great if
you're the type who's paranoid about tracking data.

This little tool tries to replace these stupid Twitch Fuel links
with actual links to the actual EXEs.  There's some guesswork, some
voodoo, and some user participation (not much, I promise), but it
does the thing.

## What specifically does it do?
Right now, it looks for URL links on the desktop.  If it finds
Twitch links, it looks for the folder that the game is stored in,
using the icon path first and then all the places Twitch Desktop
tries to shove your games by default.

Starting in version 0.7, we utilize `fuel.json` v2, which is the
current version of the format, to find the EXE of the game to link.
If anyone has any examples of other `fuel.json` versions, let me
know and I'll try to add a parser.  This can relilably find the EXE
for the game, at least the one that Twitch launcher uses.

If the game doesn't have a `fuel.json`, we have to hunt for the EXE.
Because Twitch's games don't reliably have the same EXE path, and
we can't reliably guess from the icon path, we have to ask what
looks "right".  Don't worry if you get it wrong; the original link
isn't clobbered, so you can just run it again and choose a different
option!

## Things down the road
- Replace those Start menu links too.
- Does this crap happen on non-Windows?  If so I can port this to Java maybe.
- Better heuristics.
- Some kind of autopilot that best-guesses for you (optional, of course).
- Maybe doing the whole Windows Forms thing.

## Tested games
- &gt;observer_
- Clustertruck
- Death Squared
- Tokyo 42 Complete Edition
- Twinkle Star Sprites
