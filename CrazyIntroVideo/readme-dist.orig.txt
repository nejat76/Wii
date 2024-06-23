                                                  /

                                                /
        /|_____________|\_____/| ___|\_______  /\____|\______|\__ 
        \_  _   _  \_____  \_  |_\___   \_   \/  \_  __  \_____  \_
        /   /   /   /  |/   /  |    |    /   /    /  |/   /  |/   /__
       /   /   /   /   ____/   |    _    \___    /   ____/   _  _/   \
       \  /   /   /    | _/    |    |      /    /    |       |       /
      \ \/\  /   /\____| \__________|     /    /\____________| _____/ /
      ==== \/\  / ================= |_____\   / ============ |/ =======
              \/                           \ /                       
        ____ _              _  _ __ ___     /             /\
       |                               |             /\  /  \ _    _ 
       |       MPlayerWii v o.o7       |  /     /\  /  \/   /_\|  _\|
       |                               |       /  \/   /   /   | /  |
       |   coded by rOn - 10/09/2008   |     _/   /   /   /    |/   |
       |                               |     \  _/  _/   / sns |    |  
       |___ __ _  _              _ ____|    \ \  ________\_____|____| |
                                            == \| =====================

MPlayerWii
----------
http://ronwarez.com/mplayerwii

This is a native port of MPlayer to the Nintendo Wii. It is currently in VERY EARLY stage.

You'll need to have the Nintendo Homebrew Channel installed to use this application. 

installation:
-------------
-unzip the package into the "apps" folder on your SD card.
-place the "mplayerwii.conf" file on the root folder of your SD card.
-place video and audio files on the root folder of your SD card.
-launch MPlayerWii through the Homebrew Channel.

usage:
------
-select file with up/down buttons on the wiimote.
-during playback, you can use:
 -wiimote:
   -left and right buttons to seek
   -up and down buttons to change the volume
 -nunchuk:
   -joystick to position and scale the video display
   -Z button to stretch the video
   -C to reset the video position to default settings
 -classic controller:
   -left joystick to position the video display
   -right joystick to scale the video display
   -B to reset the video position to default settings
-home button to return to the menu or exit the application.

changelog:
----------
-v0.08:
 -added SD Gecko support (thanks to CashMan's Productions)
 -added more default HTTP streams in mplayerwii.conf

-v0.07 (10/09/08):
 -fixed screen color depth/banding issues (uses code from dhewg's mplayer port)
 -added progress bar osd display during seek/volume on videos

-v0.06 (10/03/08):
 -huge speedup on USB devices (thanks to rodries and Hermes)
 -added classic controller support to position and scale the video display

-v0.05 (08/15/08):
 -added file folders support
 -added fix for PAL widescreen users (set the correct setting in mplayerwii.conf)
 -much faster USB reading speed (huge thanks to rodries)

-v0.04 (07/31/08):
 -mount/unmount the SD card and/or USB drive so you can swap them out without reloading MPlayerWii (thanks to rodries)
 -added USB read-ahead cache (thanks to rodries)
 -added preliminary samba network share support (thanks to scip)
 -fixed startup crash when a usblan or usb keyboard is attached
 -added network streaming cache
 -added video scaling with the nunchuk (use joystick to move, Z button to stretch and C to reset)
 -added configuration file (put in the SD root folder)
 -new ascii logo by sensah

-v0.03 (07/14/08):
 -happy french bastille day!
 -added USB storage support (USB reading is dead slow unfortunately, only good for MP3 files)
 -added volume controls (Up/Down buttons on the wiimote)
 -added wiimote buttons repeat during playback
 -added preliminary test network streaming (shoutcast and mms)
 -longer filename display in the file selection screen
 -fix for widescreen aspect ratio (uses the Wii display settings)
 -more audio playback fixes

-v0.02 (07/09/08):
 -added svenp's libfat patches (better read speed on SD cards)
 -added pause button (A button on the wiimote)
 -disabled MPlayer's error/status text during video playback
 -file list now displays 20 files instead of 10
 -improved audio playback code

-v0.01 (07/05/08):
 -Initial release. Full of bugs, audio sync code is awful and will probably crash often. 
  Also, there's room for lots of speed improvements. 

credits:
--------
MPlayer - The MPlayer team
Wii port - Christophe Thibault (chris at aegis-corp.org) 
Ascii logo - sensah
