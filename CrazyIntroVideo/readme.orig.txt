MPlayerWii source code
----------------------

Alright, this is a total mess for now. If you need to tweak stuff, go look into source/template.c

Otherwise, there's the diff patch for MPlayer-1.0rc2 "mplayer-patch" which is annoying as hell
to build... So for now, I'd recommend you link against the MPlayer binaries built in the mplayer 
folder until I clean all this mess up :)

The "libogc" folder is included since it includes the latest CVS revision and a bunch
of patches for USB support, etc... You can move it to your devkitPro folder.

-Christophe