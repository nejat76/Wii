-=-=-=-=-=-=-=-=-=-=-= CrazyIntro POC with Video v0.32 -=-=-=-=-=-=-=-=-=-=-=-=

22/02/2009 WiiCrazy (I.R.on)
---------------------------------------------------------------
Changes : 
* Garbled display at the start and in the end issue is solved
* No multiple wiimote clicking, pressing a single button will work
* You can have more than one video configured, just delimit the filenames with
semicolon. 
Like this, <VideoFile filename="boot.wmv;bootdn1.avi;bootdn2.avi;lost.avi" />
Program will randomly select the video file and play...


-=-=-=-=-=-=-=-=-=-=-= CrazyIntro POC with Video v0.31 -=-=-=-=-=-=-=-=-=-=-=-=

21/02/2009 WiiCrazy (I.R.on)
---------------------------------------------------------------
This is basically proof of concept CrazyIntro with video support. In this 
version mp3/jpeg stuff is deliberately taken off. Currently it's just a rip off 
from mplayerwii... 

CrazyIntro.xml config file can be used for direct channel launching as usual.
A new config element named VideoFile added, you can define the boot video as the 
filename element of VideoFile tag. An example config file and boot video 
provided in the archive.

Sources are left out as usual because they are messy... I'll release 
them as soon as I get them at the quality I expect from any published source.

Current bugs & glitches:

1. Mplayerwii build is rather bulky (which preloader doesn't like)
so I compressed the dol which makes the initial display garbled.

2. You need to press twice on wiimote to launch channels/hbc/system 
menu. Though if it's a quick video you don't need to press anything,
the thing will automatically load system menu when video finished.

Thanks : 
	MickeyBlue (for the example video included in the archive)
	My wife

-=-=-=-=-=-=-=-=-=-=-= CrazyIntro POC with Video v0.31 -=-=-=-=-=-=-=-=-=-=-=-=

LEGAL DISCLAIMER
AUTHOR OF THIS PROGRAM CAN NOT BE HELD RESPONSIBLE FOR ANY 
DAMAGES THIS PROGRAM MAY CAUSE.
