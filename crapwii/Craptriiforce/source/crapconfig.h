#ifndef _CONFIG_H_
#define _CONFIG_H_

#define VERBOSE_LOG 6
#define REGION_OVERRIDE 7
#define OVERRIDEN_REGION 8
#define OCARINA_SELECTION 9
#define FORCE_VIDEO_MODE 10
#define SELECTED_LANGUAGE 11
#define USB_OR_SDCARD 12
#define ALT_DOL_TYPE 13
#define FIX 15
//16-23 
#define DOL_OFFSET_IN_DISC 16

//Alt dol's name... max 8+3 = 11 + null
#define CONF_TITLE_ID 24
#define ALT_DOL_NAME 28

#define ALT_DOL_FROM_NAND 1
#define ALT_DOL_FROM_SD_CARD 2
#define ALT_DOL_FROM_DISC 3

#define TYPE_USB_LOADER '0'
#define TYPE_SD_LOADER '1'

/*
0 CFGUSB
6 1
7 0
8 0
9 0
10 0
11 0
12 0
13 4
14 0 NU
15 0
16 002CE9E0
24 UMPE
28 rs5mp1_p.dol
*/

typedef struct loaderconfig {
	bool verboseLog;
	bool ocarinaSelection;
	int useSDLoader;
	int fix;
	int altDolType;
	char discId[7];
	char titleId[5];
	u32 altDolOffset;
	char altDolName[13];
} LoaderConfig;

void parseConfiguration(char * config, char * discId, LoaderConfig* loaderConfig);

#endif
