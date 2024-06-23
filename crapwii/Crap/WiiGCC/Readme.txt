-=-=-=-=-=-=-=-=-=-=-=-=-= WiiGSC 1.06b -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
by WiiCrazy (I.R.on)
-=-=-=-=-=-=-=-=-=-=-=-=-= WiiGSC 1.06b -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

Notes regarding WiiGSC
---------------------------------------------------------------------
REQUIREMENTS
-------------------
This is a reworked and renamed reincarnation of Crap which is converted
from Half Managed C++ to Managed C#. Therefore you only need .Net 
Framework 2.0 and if you have it you are already reading this text in 
the info screen of the program. Libeay and stuff is no longer needed!


USAGE
-------------------
Simply choose one of 4 methods to create channels easily,
Banner, iso or wbfs... or using a channel. 
Channel source is for nand emulation with triiforce. 

DISC BANNERS
------------------
If you have the banner already, click the ellipsis button next to 
label banner and select a banner file. Banner file should end with
the disc id. Say for Wii sports it can be, Wii Sports-RSPE01.bnr, 
RSPE01.bnr, BlahBlah-RSPE01.bnr or similar. The key is in the 
ending.

ISO
-----
Simply open the iso, any alt-dol in the iso will be copied to the 
alt-dol folder and the banner of the game will be copied to the 
Banners folder so you can later re-create channels for the very
game.

WBFS
-------
Like iso, important stuff (alt-dols, banner) will be copied to their
respective folders.

Then you can select a loader and it's options... finally creating 
the actual channel.

CHANNEL(WAD) BANNERS FOR TRIIFORCE
-----------------------------------------------------
If you have the banner already, click the ellipsis button next to 
label banner and select a banner file. Banner file should end with
the title id. Banners from channels should have an extension cbnr
unlike banners for disc games which have extension bnr.

CHANNEL
------------
Click the ellipsis button and just point to a wad. Program will extract
the banner of the game and get the title id information for you and 
archive the banner into the banners folder. Then program will 
proceed as if you selected that banner. Extracted wad's contents 
are stored in tempwad folder and will not be cleaned by the program.
You can do it if you want.

BATCH MODE
----------------
If you want to create lots of channels for lots of banners then 
simply grab them all from an explorer page and drop it to the 
program window. Batch mode will be activated and with the 
selection of loader and options you can create channels for all
the banners with a single click.

Here is the folder organization of the program

Loaders (loaders reside here, if you want to add one, just copy here
and modify LoaderConfig.xml in the root folder accordingly..

Muzak (The music you are listening right now stored here)

Shared (common-key should be here)

Wad (Create wads will be stored here)

Temp (Wad unpacking & packing done here)

Alt-dol (Alt dols will be copied to here)

Lang (Each language support file will be here)

Other files used by program

1. WiiGSC.exe.config (ip adress of the wii and similar stuff is stored here)

2. LoaderConfig.xml (all information for loaders stored here, you can 
add/remove/change loaders)

3. base.wxd (a wad containing only nandloader used as an host for injection, 
don't rename and install this)

4. altdolbase.wxd (like base.wxd but for the nand option in the alt-dol
types)



Credits 
-------------------------------------------------------
WiiCrazy/I.R.on

ps: No separate credits for Loaders bundled in the program

Thanks
-------------------------------------------------------
Segher Boessenkool (this time used 70)
Omega Frost (WBFSSync's managed c# library)
Dack for WiiScrubber
Comex, Marcan (for their nand loaders)
Kwiirk for YAL (Yet another loader)
Waninkoko (for wad manager)
Team Twiizers (for homebrew channel)
Joda
Sorg (for the nice reorganization of the usbloader by waninkoko)
Skh
Nicksasa, WiiPower,TheLemonMan (for triiforce)
oggzee (for nice argument stuff in Conf. Usb Loader)
Leathl (for the Wii.cs... of course to all those who created Wii.py too)
usb loader gx team (for usb loader gx)
Narolez (for the nifty forwarder)
Wiichoxp (got the idea for the triiforce loader)
david432111 (Danish translation)
Det1re (German translation)
erikk1 (Dutch translation)
transam81 (Spanish translation)
orwel (French-1 translation)
sirakain (French-2 translation)
wiixale (Italian translation)
hosigumayuugi (Japanese translation)
liuhaohua (S.Chinese translation)

Translation Contributions
-------------------------------------------------------
You can send your translations
to nejat@tepetaklak.com