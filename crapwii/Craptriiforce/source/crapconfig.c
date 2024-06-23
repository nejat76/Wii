#include <gccore.h>
#include <stdio.h>
#include "crapconfig.h"

u32 ci(char val) 
{
	if (val>='0' && val<='9') { return val - '0'; }
	if (val>='A' && val<='F') { return val - 'A'+10; }
	if (val>='a' && val<='f') { return val - 'a'+10; }	
}

u32 char8touint32(char * val) 
{

	u32 x=(ci(val[0])<<28) + 
	(ci(val[1])<<24) + 
	(ci(val[2])<<20) + 
	(ci(val[3])<<16) + 
	(ci(val[4])<<12) + 
	(ci(val[5])<<8) + 
	(ci(val[6])<<4) + 
	(ci(val[7]));
	
	return x;
}

void dumpConfig(LoaderConfig config) 
{
	printf("Title Id          : %s\n", config.titleId);		
	printf("VerboseLog        : %s\n", config.verboseLog?"1":"0");
	printf("Ocarina Selection : %s\n", config.ocarinaSelection?"1":"0");
	printf("Use SD Loader     : %s\n", config.useSDLoader?"1":"0");
	printf("Fixes             : %d\n", config.fix);	
}


void parseConfiguration(char * config, char * titleId, LoaderConfig* loaderConfig) 
{
	bool verboseLog = false;
	bool ocarinaSelection = false;
	int fix=0;
	bool useSDLoader = false;

	verboseLog = config[VERBOSE_LOG] == '1'? true : false;
	ocarinaSelection = config[OCARINA_SELECTION] == '1' ? true : false;
	char fixChar = config[FIX];
	useSDLoader = (config[USB_OR_SDCARD] == TYPE_SD_LOADER ? true : false);
	
	fix = atoi(&fixChar);
	
	loaderConfig->verboseLog = verboseLog;
	loaderConfig->useSDLoader = useSDLoader;	
	loaderConfig->ocarinaSelection = ocarinaSelection;
	loaderConfig->fix = fix;
	strcpy(loaderConfig->titleId, titleId);
	if (verboseLog) 
	{
		dumpConfig(*loaderConfig);
	}
}


