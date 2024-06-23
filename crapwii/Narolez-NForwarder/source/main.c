/*
Dol SD/USB Forwarder [NForwarder]

Copyright (c) 2009 Narolez [narolez[at]googlemail[dot]com]
 
Credits to svpe, Joseph Jordan, SpaceJump, WiiPower, Oggzee and Zektor

This software is provided 'as-is', without any express or implied warranty.
In no event will the authors be held liable for any damages arising from
the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1.The origin of this software must not be misrepresented; you must not
claim that you wrote the original software. If you use this software in a
product, an acknowledgment in the product documentation would be
appreciated but is not required.

2.Altered source versions must be plainly marked as such, and must not be
misrepresented as being the original software.

3.This notice may not be removed or altered from any source distribution.
*/

/* 
	RELEASE NOTES
	
	v1.7 add defines for CFG USB Loader and Wiiflow	
	v1.6 new button mapping for passing IOS arguments:
	        - press 1 and 2 OR A and B: force ios=249
		    - press 1 OR A;             force ios=222-mload
		    - press 2 OR B:             force ios=223-mload
			- press nothing:            no force, loader default
	v1.5 black screen bugfix, preloader compatiblity		
	v1.4 adjusted countdown for button query - increase performance
	v1.3 bugfix for code dump when run internal dol
	v1,2 change to linkerscript rvl.ld (bugfix for large included dols)
	v1.1 Pass config parameter for IOS (ios=222-mload, ios=223-mload or ios=249)
	     as argument argv[1]
	V1.0 Initial Release
*/

#include <stdio.h>
#include <gccore.h>
#include <fat.h>
#include <string.h>
#include <malloc.h>
#include <unistd.h>
#include <sdcard/wiisd_io.h>
#include <wiiuse/wpad.h>

#include "dol.h"

//#define DEBUG

static void *xfb = NULL;
static GXRModeObj *rmode = NULL;

void init()
{
	// Initialise the video system
	VIDEO_Init();
	
	// initialise the attached controllers
	WPAD_Init();

	// Obtain the preferred video mode from the system
	// This will correspond to the settings in the Wii menu
	rmode = VIDEO_GetPreferredMode(NULL);

	// Allocate memory for the display in the uncached region
	xfb = MEM_K0_TO_K1(SYS_AllocateFramebuffer(rmode));
	
	// Set up the video registers with the chosen mode
	VIDEO_Configure(rmode);
	
	// Tell the video hardware where our display memory is
	VIDEO_SetNextFramebuffer(xfb);
	
	// Make the display invisible
	#ifdef DEBUG
	VIDEO_SetBlack(FALSE);	
	#else
	VIDEO_SetBlack(TRUE);	
	#endif

	// Flush the video register changes to the hardware
	VIDEO_Flush();

	// Wait for Video setup to complete
	VIDEO_WaitVSync();

	if(rmode->viTVMode&VI_NON_INTERLACE) VIDEO_WaitVSync();
	
	#ifdef DEBUG
		// Set console parameters
    int x = 20, y = 20, w, h;
    w = rmode->fbWidth - (x * 2);
    h = rmode->xfbHeight - (y + 20);

    // Initialize the console - CON_InitEx works after VIDEO_ calls
    CON_InitEx(rmode, x, y, w, h);
	#endif

}


void *allocate_memory(u32 size)
{
	return memalign(32, (size+31)&(~31) );
}


#define SD 0
#define USB 1
#define STORAGENOTREADYRETRIES 5
DISC_INTERFACE storage;

u8 mountStorage(int device)
{
	if (device == SD) storage = __io_wiisd;
	else storage = __io_usbstorage;

	int retrycount = 0;

retry:
	storage.startup();

	if (!fatMountSimple("fat", &storage)) 
	{
		fatUnmount("fat");
		storage.shutdown();

		// Retry if USB device is not ready
		if(device == USB && retrycount <= STORAGENOTREADYRETRIES) 
		{
			sleep(1);
			retrycount++;
			goto retry;
		}
		
		return FALSE;
	}
	return TRUE;			
}

#define FILENAMECHARACTERS 200

// define USB Loader
#define WIIFLOW
//#define CFGUL
//#define USBLOADERGX

// Configurable USB Loader
#ifdef CFGUL 
	#define MAXFILES 4
	// add/change filenames here, don't forget to edit MAXFILES!
	char filenames[MAXFILES][FILENAMECHARACTERS] = 
	{ 
		{"fat:/apps/usbloader/boot.dol"},	
		//{"fat:/apps/tester/boot.dol"},			
		{"fat:/apps/usbloader_cfg/boot.dol"},
		{"fat:/apps/usb_loader/boot.dol"},
		{"fat:/apps/usb-loader/boot.dol"}	
	};
#endif

// USB Loader GX
#ifdef USBLOADERGX
	#define MAXFILES 4
	// add/change filenames here, don't forget to edit MAXFILES!
	char filenames[MAXFILES][FILENAMECHARACTERS] = 
	{ 
		{"fat:/apps/usbloader_gx/boot.dol"},	
		{"fat:/apps/usbloadergx/boot.dol"},	
		{"fat:/apps/usb_loader/boot.dol"},
		{"fat:/apps/usb-loader/boot.dol"}	
	};
#endif

// Wiiflow
#ifdef WIIFLOW
	#define MAXFILES 2
	// add/change filenames here, don't forget to edit MAXFILES!
	char filenames[MAXFILES][FILENAMECHARACTERS] = 
	{ 
		{"fat:/apps/wiiflow/boot.dol"},
		{"fat:/wiiflow/boot.dol"}
	};
#endif

char filename[FILENAMECHARACTERS];
FILE* dolFile = NULL;


void GetFirstAvailableDol( )
{
	int i;
	for(i = 0; i < MAXFILES; i++)
	{
		sprintf(filename, filenames[i]); 
		dolFile = fopen(filename, "rb");
		if(dolFile != NULL) break;
	}
	
	if(dolFile == NULL)
	{
		fatUnmount("fat");
		storage.shutdown();
	}
	
	return;
}


int main(int argc, char **argv) 
{
	// init stuff
	init();

	// try loading from SD
	if(mountStorage(SD)) GetFirstAvailableDol();

	// try loading from USB if no dol file on SD found
	if(dolFile == NULL && mountStorage(USB)) GetFirstAvailableDol();

	// no dol file on SD or USB found, go to wii system menu!
	if(dolFile == NULL) SYS_ResetSystem(SYS_RETURNTOMENU, 0, 0);

	// set default "not set"
#define SELECTED_PARTITION 12	
#define EXTRA_PARAMETERS 13
#define VERBOSE_LOG 6
	char defaultConfig[142];
	//128 characters reserved for extra parameters to pass to the loader
	#ifdef CFGUL	
	#ifdef DEBUG
	strcpy(defaultConfig, "CFGCNF0000000debug=1 partition=NTFS1");		
	#else
	strcpy(defaultConfig, "CFGCNF0000000PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMET");
	#endif
	#endif

	#ifdef USBLOADERGX
	#ifdef DEBUG
	strcpy(defaultConfig, "CFGUGX0000000");	
	#else
	strcpy(defaultConfig, "CFGUGX0000000PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMET");
	#endif
	#endif		

	#ifdef WIIFLOW
	#ifdef DEBUG
	strcpy(defaultConfig, "CFGWFL0000000");	
	#else
	strcpy(defaultConfig, "CFGWFL0000000PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMETERS PLACEHOLDER FOR EXTRA PARAMET");
	#endif
	#endif		

	
	char cfgparam[12];
	
	#ifdef CFGUL
	#ifdef DEBUG
	strcpy(cfgparam, "SMNP01");		
	#else
	strcpy(cfgparam, "CRAPPY");
	//cfgparam[8] = defaultConfig[SELECTED_PARTITION]; //Take it as an extra parameter
	#endif	
	#endif

	#ifdef USBLOADERGX
	#ifdef DEBUG
	strcpy(cfgparam, "SMNP01");	
	#else
	strcpy(cfgparam, "CRAPPY");	
	#endif
	#endif

	#ifdef WIIFLOW
	#ifdef DEBUG
	strcpy(cfgparam, "SMNP01");	
	#else
	strcpy(cfgparam, "CRAPPY");	
	#endif
	#endif
	
	//Let's break the arguments passed by Crap into an array and later set them into the argv array...
	
	#define MAXPARAMETERS 10
	#define MAXPARAMETERSIZE 20
	
	char extraParameters[MAXPARAMETERS][MAXPARAMETERSIZE];
	char *pExtraParameters = defaultConfig + EXTRA_PARAMETERS;
	char *p;
	int curExtraParameterCount = 0;
	int extraParametersLength = 0;
	
	#ifdef CFGUL
	sprintf(extraParameters[curExtraParameterCount], "intro=1"); //Make intro parameter the first extra parameter
	#ifdef DEBUG
	printf("\nAdding %s with length %d", extraParameters[curExtraParameterCount], strlen(extraParameters[curExtraParameterCount]));
	#endif
	
	extraParametersLength = extraParametersLength + strlen(extraParameters[curExtraParameterCount]);
	curExtraParameterCount++;	
	
	#endif
	
	//MAXPARAMETERS-1 more parameters can be specified in the configuration placeholder by the channel creator.
	p = strtok(pExtraParameters, " ");

	if (p!=NULL) 
	{
		while ((p != NULL) && (curExtraParameterCount<MAXPARAMETERS))
		{			
			strcpy(extraParameters[curExtraParameterCount], p); //Copy a parameter to the array
			p = strtok(NULL, " "); //next argument
			
			extraParametersLength = extraParametersLength + strlen(extraParameters[curExtraParameterCount]);			
			#ifdef DEBUG			
			printf("\nAdding %s with length %d", extraParameters[curExtraParameterCount], strlen(extraParameters[curExtraParameterCount]));
			sleep(1);
			#endif
			curExtraParameterCount++; //increase count of arguments by one
		}
	}	

	#ifdef DEBUG	
	printf("\nTotal param length = %d", extraParametersLength);
	sleep(5);
	#endif

	//TODO: Make Crap patch this so that we don't need to create two different loaders
	//Requires gui changes at the moment so... 
	//[20091204] gave up it will mess the Crap gui... it's better to define at least this parameter as default in the loader.. 
	//user can define the extra parameters in a textbox in Crap.
	//cfgparam[16] = defaultConfig[VERBOSE_LOG] == '0' ? '1' : '0';
	

	// build arguments for filename and cfgparam
	// argv[0] = filename | argv[1] = cfgparam (could be: ios=222-mload, ios=223-mload or ios=249)
	struct __argv arg;
	
	bzero(&arg, sizeof(arg));
	arg.argvMagic = ARGV_MAGIC;
	
	#ifdef DEBUG
	printf("\nFilename length = %d", strlen(filename));
	printf("\nCfgParam length = %d", strlen(cfgparam));
	printf("\nExtra p  length = %d", extraParametersLength);	
	printf("\nCount ext param = %d", curExtraParameterCount);	
	printf("\nOverhead        = %d", 3);
	#endif
	
	
	arg.length = strlen(filename) + 1 + strlen(cfgparam) + 1 + extraParametersLength + curExtraParameterCount  + 1;
	#ifdef DEBUG
	printf("\nArgument length = %d", arg.length);
	#endif
	
	arg.commandLine = malloc(arg.length);
	
	#ifdef USBLOADERGX
	strcpy(arg.commandLine, filename);	
	//arg.commandLine[0] = 'U';arg.commandLine[1] = 'S';arg.commandLine[2] = 'B';
	#else
	strcpy(arg.commandLine, filename);
	#endif
	
	strcpy(&arg.commandLine[strlen(filename) + 1], cfgparam);
	
	int position = strlen(filename) + strlen(cfgparam) + 2;
	int i;
	for (i=0;i<curExtraParameterCount;i++) 
	{
		strcpy(&arg.commandLine[position], extraParameters[i]);	
		#ifdef DEBUG
		printf("\nCopying %d into %d", i, position);
		#endif
		position = position + strlen(extraParameters[i]);
		arg.commandLine[position] = '\x00';
		position++;
	}
		
	#ifdef DEBUG
	printf("\nNulling position = %d", arg.length - 1);	
	printf("\nNulling position = %d", arg.length - 2);		
	#endif
	arg.commandLine[arg.length - 1] = '\x00';
	arg.commandLine[arg.length - 2] = '\x00';
	
	arg.argc = 2+curExtraParameterCount;
	arg.argv = &arg.commandLine;
	arg.endARGV = arg.argv + 2 + curExtraParameterCount;
	// dol file on SD or USB found, read it!
	int pos = ftell(dolFile);
	fseek(dolFile, 0, SEEK_END);
	int size = ftell(dolFile);
	fseek(dolFile, pos, SEEK_SET);
	void* myBuffer = allocate_memory(size);
	fread( myBuffer, 1, size, dolFile);

	// close the file and the storage after buffering
	fclose(dolFile);
	fatUnmount("fat");
	storage.shutdown();
	
	// Shutdown the Wiimote
	WPAD_Shutdown();

	// run the buffered dol with arguments
	run_dol(myBuffer, &arg);		
	
	return 0;
}
