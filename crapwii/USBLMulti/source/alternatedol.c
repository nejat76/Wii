#include <ogcsys.h>
#include <stdio.h>
#include <gccore.h>
#include <string.h>
#include <malloc.h>
#include "config.h"
#include "apploader.h"
#include "wdvd.h"


void cleanUp() 
{	
	int ret = Fat_UnmountSDHC();	
	if (ret < 0) {
		printf("[+] SD Error\n");
	}
}

/** Alternate dolloader made by WiiPower modified by dimok **/
bool Load_Dol(void **buffer, int* dollen, char * filepath, char * dolFile, bool verboseLog) {
    int ret;
    FILE* file;
    void* dol_buffer;

    char fullpath[200];
    snprintf(fullpath, 200, "%s%s", filepath, dolFile);

	ret = Fat_MountSDHC();
	if (ret < 0) {
		printf("[+] SD Error\n");
		sleep (2);
		return 0;
	}
	
	if (verboseLog) {
		printf("Loading dol from sd card : %s\n", fullpath);
	}
    file = fopen(fullpath, "rb");

    if (file == NULL) {
		printf("File not found!\n");	
        fclose(file);
		cleanUp();
        return false;
    }

    int filesize;
    fseek(file, 0, SEEK_END);
    filesize = ftell(file);
    fseek(file, 0, SEEK_SET);

    dol_buffer = malloc(filesize);
    if (dol_buffer == NULL) {
		printf("Can't allocate space for dol!\n");		
        fclose(file);
		cleanUp();
		return false;
    }
    ret = fread( dol_buffer, 1, filesize, file);
    if (ret != filesize) {
		printf("Filesize and data read do not match!\n");			
        free(dol_buffer);
        fclose(file);
		cleanUp();
        return false;
    }
    fclose(file);

	cleanUp();
    *buffer = dol_buffer;
    *dollen = filesize;
	if (verboseLog) 
	{
		printf("Dol loaded\b");	
	}
    return true;
}

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

static dolheader *dolfile;


u32 load_dol_image(void *dolstart) {

    u32 i;

    if (dolstart) {
        dolfile = (dolheader *) dolstart;
        for (i = 0; i < 7; i++) {
            if ((!dolfile->text_size[i]) || (dolfile->text_start[i] < 0x100)) continue;
            VIDEO_WaitVSync();
            ICInvalidateRange ((void *) dolfile->text_start[i],dolfile->text_size[i]);
            memmove ((void *) dolfile->text_start[i],dolstart+dolfile->text_pos[i],dolfile->text_size[i]);
        }

        for (i = 0; i < 11; i++) {
            if ((!dolfile->data_size[i]) || (dolfile->data_start[i] < 0x100)) continue;
            VIDEO_WaitVSync();
            memmove ((void *) dolfile->data_start[i],dolstart+dolfile->data_pos[i],dolfile->data_size[i]);
            DCFlushRangeNoSync ((void *) dolfile->data_start[i],dolfile->data_size[i]);
        }
        /*
        memset ((void *) dolfile->bss_start, 0, dolfile->bss_size);
        DCFlushRange((void *) dolfile->bss_start, dolfile->bss_size);
        */
        return dolfile->entry_point;
    }
    return 0;
}

static int i;
static int phase;

u32 load_dol_start(void *dolstart) {
    if (dolstart) {
        dolfile = (dolheader *)dolstart;
        return dolfile->entry_point;
    } else {
        return 0;
    }

    memset((void *)dolfile->bss_start, 0, dolfile->bss_size);
    DCFlushRange((void *)dolfile->bss_start, dolfile->bss_size);

    phase = 0;
    i = 0;
}

bool load_dol_image_modified(void **offset, u32 *pos, u32 *len) {
    if (phase == 0) {
        if (i == 7) {
            phase = 1;
            i = 0;
        } else {
            if ((!dolfile->text_size[i]) || (dolfile->text_start[i] < 0x100)) {
                *offset = 0;
                *pos = 0;
                *len = 0;
            } else {
                *offset = (void *)dolfile->text_start[i];
                *pos = dolfile->text_pos[i];
                *len = dolfile->text_size[i];
            }
            i++;
            return true;
        }
    }

    if (phase == 1) {
        if (i == 11) {
            phase = 2;
            return false;
        }

        if ((!dolfile->data_size[i]) || (dolfile->data_start[i] < 0x100)) {
            *offset = 0;
            *pos = 0;
            *len = 0;
        } else {
            *offset = (void *)dolfile->data_start[i];
            *pos = dolfile->data_pos[i];
            *len = dolfile->data_size[i];
        }
        i++;
        return true;
    }
    return false;
}

u32 Load_Dol_from_disc(u32 doloffset, bool verboseLog) {
    int ret;
    void *dol_header;
    u32 entrypoint;

	if (verboseLog) 
	{
		printf("Reading dol from disc at offset %x\n", doloffset);
	}
	
    dol_header = memalign(32, sizeof(dolheader));
    if (dol_header == NULL) {
        return -1;
    }

    ret = WDVD_Read(dol_header, sizeof(dolheader), (doloffset<<2));

    entrypoint = load_dol_start(dol_header);

    if (entrypoint == 0) {
        free(dol_header);
        return -1;
    }
	if (verboseLog) 
	{
		printf("Loading dol from disc\n");
	}
    void *offset;
    u32 pos;
    u32 len;
    while (load_dol_image_modified(&offset, &pos, &len)) {
        if (len != 0) {
            ret = WDVD_Read(offset, len, (doloffset<<2) + pos);

            //DCFlushRange(offset, len);

            //gamepatches(offset, len, videoSelected, patchcountrystring, vipatch);

            Remove_001_Protection(offset, len);
			DCFlushRange(offset, len);
        }
    }

    free(dol_header);

    return entrypoint;
}
