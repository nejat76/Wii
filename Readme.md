# Software related to the Wii


I'll be sharing the source code of Wii tools developed by me here...

You can find the binary releases for this programs on my homepage here : http://www.tepetaklak.com/wii/

Beware that None of them is actively developed so I can't guarantee that you can even compile them, I'm just pushing them here from a non development environment.

 - SaveTemper is the code name for the FE100 tool. I guess (it's 14 years old) FE is iron's (which is my nick) chemical element symbol and 100 is the sum of 80+20 which are referring to original programs created by Segher. This software is a simplification effort for the end user for those programs, namely tachtig (eighty) and twintig (twenty). 

 - crapwii folder contains source code for both latest Crap and latest WiiGSC release with a tiny bit of change. 
   - Bat files left intact and contains absolute paths in my old dev env. since I'm not actively developing.
   - Changing the installers would need proper devkitppc setup, I don't remember which lib versions they were compatible with. 
   - WiiGSC builds and runs fine with Visual Studio 2019 but I didn't test it. 
   - Installation feature not converted to VS2019 properly so it's missing.
   - The change is to the muzak folder where I removed jt's tracks and put one of my own compositions. Music is used in the info screen. 
   - Sources for several other stuff is also here
      - KeyStego : a simple steganography application. 
	  - CrazyInstaller : fantastic tool that can create installers for channels also capable of creating pc side executable installers which use wiiload feature in the Homebrew channel.

 - CrazyIntroVideo is a preloader plugin like application to play videos when you start the wii. mplayerwii library is used in compiled form.