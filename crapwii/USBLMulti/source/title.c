#include <stdio.h>
#include <stdlib.h>
#include <malloc.h>
#include <string.h>
#include <gccore.h>
#include "config.h"
#include "menu_tik_bin.h"
#include "menu_tmd_bin.h"
#include "menu_certs_bin.h"
#include "title.h"

typedef struct _dolheader {
	u32 text_pos[7];
	u32 data_pos[11];
	u32 text_start[7];
	u32 data_start[11];
	u32 text_size[7];
	u32 data_size[11];
	u32 bss_start;
	u32 bss_size;
	u32 entry_point;
} dolheader;




u32 loadDol(u16 index, bool verboseLog) {
	u32 i;
	static dolheader _dolfile __attribute__((aligned(32)));
	dolheader *dolfile = &_dolfile;
	s32 s = ES_OpenContent(index);
	if(s<0) {
		printf("Error opening content\n");
		return 0;
	}
	printf("! %d\n", ES_ReadContent(s, dolfile, sizeof(dolheader)));
	memset ((void *) dolfile->bss_start, 0, dolfile->bss_size);
	for (i = 0; i < 7; i++) {
		if(dolfile->data_start[i] < sizeof(dolheader)) continue;
		if (verboseLog) 
		{
			printf ("loading text section %u @ 0x%08x (0x%08x bytes)\n",i, dolfile->text_start[i],dolfile->text_size[i]);		
			VIDEO_WaitVSync();
		}

		ES_SeekContent(s, dolfile->text_pos[i], 0);
		ES_ReadContent(s, dolfile->text_start[i], dolfile->text_size[i]);	
		DCFlushRange(dolfile->text_start[i], dolfile->text_size[i]);
	}

	for(i = 0; i < 11; i++) {
		if(dolfile->data_start[i] < sizeof(dolheader)) continue;
		if (verboseLog) 
		{
			printf ("loading data section %u @ 0x%08x (0x%08x bytes)\n",i, dolfile->data_start[i],dolfile->data_size[i]);
			VIDEO_WaitVSync();
		}

		ES_SeekContent(s, dolfile->data_pos[i], 0);
		ES_ReadContent(s, dolfile->data_start[i], dolfile->data_size[i]);
		DCFlushRange(dolfile->data_start[i], dolfile->data_size[i]);
	}
		
	VIDEO_WaitVSync();
	ES_CloseContent(s);	
	
	return dolfile->entry_point;
}

u64 StrTitleIdToLong(char * str_title_id, u8 title_type) 
{
	//Hmm somewhat failed with 64 bit bitwise operations, hence below senseless switch code..
	//Though possibly it's irrelevant
	u64 title_id;
	if (title_type=='1') {
		title_id = 0x0001000100000000LL;
	} else if (title_type =='2') {
		title_id = 0x0001000200000000LL;	
	} else if (title_type =='4') {
		title_id = 0x0001000400000000LL;	
	} else if (title_type =='8') {
		title_id = 0x0001000800000000LL;	
	} else return 0LL;
	
	
	title_id = title_id + 
	(((u64)str_title_id[0])<<24) + 
	(((u64)str_title_id[1])<<16) + 
	(((u64)str_title_id[2])<<8) + 
	((u64)str_title_id[3]);
	
	return title_id;
}


u32 loadAltDol(LoaderConfig loaderConfig) 
{
	u64 titleID = StrTitleIdToLong(loaderConfig.titleId, '1');

	int rest = ES_Identify((signed_blob*)menu_certs_bin, menu_certs_bin_size, (signed_blob*)menu_tmd_bin, menu_tmd_bin_size, (signed_blob*)menu_tik_bin, menu_tik_bin_size, 0);
	if (loaderConfig.verboseLog) 
	{
		printf("ES_Identify returned %d\n", rest);
	}
	__ES_Close();
	__ES_Init();

	//u64 titleID;
	//ES_GetTitleID(&titleID);
	if (loaderConfig.verboseLog) 
	{
		printf("titleID: %016llx\n", titleID);
		//sleep(5);
	}
	
	u32 tmdSize;
	ES_GetStoredTMDSize(titleID, &tmdSize);
	
	// I need to do this because I have an old version of the TMD
	// I want this to be robust against TMD updates, so rather than just updating...
	u8 *tmd = memalign(32, tmdSize);
	
	ES_GetStoredTMD(titleID, (signed_blob *) tmd, tmdSize);
	//u16 boot = *((u16 *)(tmd + 0x1e0));
	u16 altDol = 3;
	u32 dataSize = 9999;
	// This is totally not the C way to do things
	int pos;
	for(pos = 0x1e4; pos < tmdSize; pos+=36) {
		if (loaderConfig.verboseLog) 
		{	
			printf("ALT DOL: %x\n",*((u16 *) (tmd + pos + 0x04))); 
		}
		
		if(*((u16 *) (tmd + pos + 0x04)) == altDol) {
			if (loaderConfig.verboseLog) 
			{
				printf("Pos: %04x DS: %08llx\n", pos, *((u64 *) (tmd + pos + 0x08)));
			}
			dataSize = (u32) *((u64 *) (tmd + pos + 0x08));
			break;
		}	
	}
	

	rest = ES_Identify((signed_blob*)menu_certs_bin, menu_certs_bin_size, (signed_blob*)tmd, tmdSize, (signed_blob*)menu_tik_bin, menu_tik_bin_size, 0);
	if (loaderConfig.verboseLog) 
	{
		printf("ES_Identify (2nd time) returned %d\n", rest);
		printf("Boot: %d\n", altDol);
		printf("Data size: %d\n", dataSize);
	}
	return loadDol(altDol, loaderConfig.verboseLog);
}
