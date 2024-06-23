// By BFGR
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
// BFGR WadTools v0.39a

#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>

#include "tools.h"

int signBanner(char * bannerFile, char * newBannerFile, u8 * title_name)
{
	u64 imet_len = 0x600;
	u8 *imet;
	u8 hash[16];
	FILE *fp;
	FILE *fpout;
	int c;
	u32 i,j;
	u8 *text;
	u8 * channelName;

	if (strlen(title_name)>0x2A) 
	{
		fatal(-1, "Channel name too big %s", title_name);
		return -1;
	}
	channelName = malloc(0x54);
	memset(channelName, 0x00, 0x54); //clean channelName
	for (i=0;i<strlen(title_name);i++) 
	{
		channelName[i*2+1] = title_name[i];
	}
	
	fp = fopen(bannerFile, "rb");
	if (!fp) 
	{
		fatal(-1, "Can't open opening banner %s", bannerFile);
		return -1;
	}

	imet = (u8 *)malloc(imet_len);
	if (fread(imet, imet_len, 1, fp) != 1) 
	{
		fatal(-1, "Can't read from opening banner %s", bannerFile);
		fclose(fp);
		return -1;
	}

	//fclose(fp); Don't close... We'll copy the rest of the stuff to the destination banner file...

	memset(imet + 0x05F0, 0x00, 0x0010); // Remove MD5 sum

	//Prepare to modify channel names
	text = (u8 *)malloc(6*0x0054);
	memset(text, 0x00, 6*0x0054); // Remove current channel names

	//Build modified channel names
	for (i=0;i<6;i++) 
	{
		memcpy(text + 0x0054*i, channelName, 0x54);
	}

	memcpy(imet + 0x00B0, text, 6*0x0054); //Copy modified channel names into header

	md5(imet + 0x0040, 0x600, hash); //Take md5 sum

	memcpy(imet + 0x05F0, hash, 0x0010); //Copy hash over to imet header


	fpout = fopen(newBannerFile, "wb");
	if (!fpout) { 
		fatal(-1, "Cannot open modified banner for writing %s.", bannerFile); 
		return -1; 
	}

	if (fwrite(imet, imet_len, 1, fpout) != 1) 
	{
		fclose(fpout);
		fatal(-1, "Cannot write to banner file %s", bannerFile);
		return -1;
	}

	while((c=getc(fp))!=EOF) 
	{
       putc(c,fpout);
	}
    fclose(fp);
    fclose(fpout);	

	return 0;
}
