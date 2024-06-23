#include <gccore.h>
#include <stdio.h>
#include "config.h"

u32 ci(char val) 
{
	if (val>='0' && val<='9') { return val - '0'; }
	if (val>='A' && val<='F') { return val - 'A'+10; }
	if (val>='a' && val<='f') { return val - 'a'+10; }	
}

u32 char8touint32(char * val) 
{

	//printf("%x %x %x %x %x %x %x %x\n", ci(val[0]), ci(val[1]), ci(val[2]), ci(val[3]), ci(val[4]), ci(val[5]), ci(val[6]), ci(val[7]) );

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
	printf("Disc Id           : %s\n", config.discId);
	printf("VerboseLog        : %s\n", config.verboseLog?"1":"0");
	printf("Ocarina Selection : %s\n", config.ocarinaSelection?"1":"0");
	printf("Use SD Loader     : %s\n", config.useSDLoader?"1":"0");
	printf("Fixes             : %d\n", config.fix);	
	printf("Alt Dol Type      : %d\n", config.altDolType);
	printf("Alt Dol Name      : %s\n", config.altDolName);
	printf("Alt Dol Offset    : %x\n", config.altDolOffset);
}

void parseConfiguration(char * config, char * discId, LoaderConfig* loaderConfig) 
{
	bool verboseLog = false;
	bool ocarinaSelection = false;
	int altDolType = 0;
	int fix=0;
	bool useSDLoader = false;

	verboseLog = config[VERBOSE_LOG] == '1'? true : false;
	ocarinaSelection = config[OCARINA_SELECTION] == '1' ? true : false;
	char fixChar = config[FIX];
	char altDolConfig = config[ALT_DOL_TYPE];
	useSDLoader = (config[USB_OR_SDCARD] == TYPE_SD_LOADER ? true : false);
	char titleId[5];
	char altDolName[13];
	u32 altDolOffset = 0;

	
	fix = atoi(&fixChar);
	altDolType = atoi(&altDolConfig);
	altDolOffset = char8touint32(config + DOL_OFFSET_IN_DISC);
	strcpy(altDolName, config+ALT_DOL_NAME);		
	memcpy(titleId, config+TITLE_ID, 4); titleId[4]=0;			
	
	/*
	if (altDolType == ALT_DOL_FROM_SD_CARD) 
	{
		strcpy(altDolName, config+ALT_DOL_NAME);		
		printf("Will boot from sd card : %s\n", altDolName);				
	} else if (altDolType == ALT_DOL_FROM_DISC) 
	{
		altDolOffset = char8touint32(config + DOL_OFFSET_IN_DISC);
		printf("Will boot from disc\n");		
	} else if (altDolType == ALT_DOL_FROM_NAND) {
		memcpy(titleId, config+TITLE_ID, 4); titleId[4]=0;		
		printf("Will boot from nand\n");
	}
	*/
	
	loaderConfig->verboseLog = verboseLog;
	loaderConfig->useSDLoader = useSDLoader;	
	loaderConfig->ocarinaSelection = ocarinaSelection;
	loaderConfig->fix = fix;
	loaderConfig->altDolType = altDolType;
	loaderConfig->altDolOffset = altDolOffset;
	strcpy(loaderConfig->altDolName, altDolName);
	strcpy(loaderConfig->discId, discId);
	strcpy(loaderConfig->titleId, titleId);
	if (verboseLog) 
	{
		dumpConfig(*loaderConfig);
	}
	//sleep(3);
}


