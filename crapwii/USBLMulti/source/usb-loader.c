#include <stdio.h>
#include <stdlib.h>
#include <ogcsys.h>

#include "config.h"
#include "disc.h"
#include "gui.h"
#include "menu.h"
#include "restart.h"
#include "subsystem.h"
#include "sys.h"
#include "video.h"
#include "multilang.h"

char defaultDiscId[7];
char defaultConfig[64];

int main(int argc, char **argv)
{
	/* SET DEFAULT CONFIG FOR CRAP */
	
	strcpy(defaultDiscId, "CRAPPY");
	strcpy(defaultConfig,   "CFGUSB00000000000000000000000000000000000");
	
	/*
	strcpy(defaultDiscId, "SMNP01");
	strcpy(defaultConfig,   "CFGUSB10000000002000000000000000000000000");
	*/
	//strcpy(defaultDiscId, "R3MP01");	//Metroid Prime
	//strcpy(defaultDiscId, "RVUP8P");	//Virtua tennis
	//strcpy(defaultConfig, "CFGUSB0000000000");
	//strcpy(defaultConfig, "CFGUSB1000000100002CE9E0U3MPrs5mp1_p.dol");
	//strcpy(defaultConfig, "CFGUSB1000000100002CE9E0U3MPrs5mp1_p.dol");
	////strcpy(defaultConfig,     "CFGUSB00000000100002CE9E0U3MPrs5mp1_p.dol");	
	//strcpy(defaultConfig, "CFGUSB1000000400002CE9E0UMPErs5mp1_p.dol");
	//strcpy(defaultConfig, "CFGUSB1000000400003FE2F8UMPErs5mp1_p.dol");	
	//strcpy(defaultConfig, "CFGUSB1000000200003FE2F8UMPErs5mp2_p.dol");
	s32 ret;
	
	bool enableDisplay = (defaultConfig[VERBOSE_LOG]=='1');
	
	/* Load Custom IOS */
	ret = IOS_ReloadIOS(34);

	/* Initialize system */
	Sys_Init();

	/* Initialize subsystems */
	Subsystem_Init();

	/* Set video mode */
	Video_SetMode(enableDisplay);
	

	#ifndef __CRAPMODE__
	/* Initialize console */
	Gui_InitConsole();
	
	/* Show background */
	Gui_DrawBackground();
	#endif

	/* Check if Custom IOS is loaded */
	if (ret < 0) {
		printf(MSG_GENERIC_ERROR);
		printf(MSG_CIOS_RELOAD_ERROR, ret);

		goto out;
	}

	/* Initialize DIP module */
	ret = Disc_Init();
	if (ret < 0) {
		printf(MSG_GENERIC_ERROR);
		printf(MSG_DIP_MODULE_INIT_ERROR, ret);

		goto out;
	}
	
	#ifdef __CRAPMODE__
	Menu_Boot(defaultDiscId, defaultConfig);	
	#else	
	/* Menu loop */
	Menu_Loop();
	#endif

out:
	/* Restart */
	Restart_Wait();

	return 0;
}
