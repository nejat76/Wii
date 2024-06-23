#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>
#include <ogcsys.h>
#include "config.h"
#include "disc.h"
#include "fat.h"
#include "gui.h"
#include "menu.h"
#include "partition.h"
#include "restart.h"
#include "sys.h"
#include "utils.h"
#include "video.h"
#include "wbfs.h"
#include "wpad.h"
#include "multilang.h"

/* Constants */
#define ENTRIES_PER_PAGE	12
#define MAX_CHARACTERS		30

/* Gamelist buffer */
static struct discHdr *gameList = NULL;

/* Gamelist variables */
static s32 gameCnt = 0, gameSelected = 0, gameStart = 0;

/* WBFS device */
static s32 wbfsDev = WBFS_MIN_DEVICE;


s32 __Menu_EntryCmp(const void *a, const void *b)
{
	struct discHdr *hdr1 = (struct discHdr *)a;
	struct discHdr *hdr2 = (struct discHdr *)b;

	/* Compare strings */
	return strcmp(hdr1->title, hdr2->title);
}

#ifndef __CRAPMODE__

s32 __Menu_GetEntries(void)
{
	struct discHdr *buffer = NULL;

	u32 cnt, len;
	s32 ret;

	/* Get list length */
	ret = WBFS_GetCount(&cnt);
	if (ret < 0)
		return ret;

	/* Buffer length */
	len = sizeof(struct discHdr) * cnt;

	/* Allocate memory */
	buffer = (struct discHdr *)memalign(32, len);
	if (!buffer)
		return -1;

	/* Clear buffer */
	memset(buffer, 0, len);

	/* Get header list */
	ret = WBFS_GetHeaders(buffer, cnt, sizeof(struct discHdr));
	if (ret < 0)
		goto err;

	/* Sort entries */
	qsort(buffer, cnt, sizeof(struct discHdr), __Menu_EntryCmp);

	/* Free memory */
	if (gameList)
		free(gameList);

	/* Set values */
	gameList = buffer;
	gameCnt  = cnt;

	/* Reset variables */
	gameSelected = gameStart = 0;

	return 0;

err:
	/* Free memory */
	if (buffer)
		free(buffer);

	return ret;
}

char *__Menu_PrintTitle(char *name)
{
	static char buffer[MAX_CHARACTERS + 4];

	/* Clear buffer */
	memset(buffer, 0, sizeof(buffer));

	/* Check string length */
	if (strlen(name) > (MAX_CHARACTERS + 3)) {
		strncpy(buffer, name,  MAX_CHARACTERS);
		strncat(buffer, "...", 3);

		return buffer;
	}

	return name;
}

void __Menu_PrintInfo(struct discHdr *header)
{
	f32 size = 0.0;

	/* Get game size */
	WBFS_GameSize(header->id, &size);

	/* Print game info */
	printf("    %s\n",                    header->title);
	printf("    (%c%c%c%c) (%.2fGB)\n\n", header->id[0], header->id[1], header->id[2], header->id[3], size);
}

void __Menu_MoveList(s8 delta)
{
	s32 index;

	/* No game list */
	if (!gameCnt)
		return;

	/* Select next entry */
	gameSelected += delta;

	/* Out of the list? */
	if (gameSelected <= -1)
		gameSelected = (gameCnt - 1);
	if (gameSelected >= gameCnt)
		gameSelected = 0;

	/* List scrolling */
	index = (gameSelected - gameStart);

	if (index >= ENTRIES_PER_PAGE)
		gameStart += index - (ENTRIES_PER_PAGE - 1);
	if (index <= -1)
		gameStart += index;
}

void __Menu_ShowList(void)
{
	f32 free, used;

	/* Get free space */
	WBFS_DiskSpace(&used, &free);

	printf(MSG_SELECT_GAME);

	/* No game list*/
	if (gameCnt) {
		u32 cnt;

		/* Print game list */
		for (cnt = gameStart; cnt < gameCnt; cnt++) {
			struct discHdr *header = &gameList[cnt];

			/* Entries per page limit reached */
			if ((cnt - gameStart) >= ENTRIES_PER_PAGE)
				break;

			/* Print entry */
			printf("\t%2s %s\n", (gameSelected == cnt) ? ">>" : "  ", __Menu_PrintTitle(header->title));
		}
	} else
		printf(MSG_NO_GAMES_FOUND);

	printf("\n\n");

	/* Print free/used space */
	printf(MSG_FREE_SPACE, free);
	printf(MSG_USED_SPACE, used);
}

void __Menu_ShowCover(void)
{
	struct discHdr *header = &gameList[gameSelected];

	/* No game list*/
	if (!gameCnt)
		return;

	/* Draw cover */
	Gui_DrawCover(header->id);
}

void Menu_Format(void)
{
	partitionEntry partitions[MAX_PARTITIONS];

	u32 cnt, sector_size;
	s32 ret, selected = 0;

	/* Clear console */
	Con_Clear();

	/* Get partition entries */
	ret = Partition_GetEntries(wbfsDev, partitions, &sector_size);
	if (ret < 0) {
		printf(MSG_NO_PARTITIONS, ret);

		/* Restart */
		Restart_Wait();
	}

loop:
	/* Clear console */
	Con_Clear();

	printf(MSG_SELECT_PARTITION);
	printf(MSG_FORMAT);

	/* Print partition list */
	for (cnt = 0; cnt < MAX_PARTITIONS; cnt++) {
		partitionEntry *entry = &partitions[cnt];

		/* Calculate size in gigabytes */
		f32 size = entry->size * (sector_size / GB_SIZE);

		/* Selected entry */
		(selected == cnt) ? printf(">> ") : printf("   ");
		fflush(stdout);

		/* Valid partition */
		if (size)
			printf(MSG_PART_DISPLAY1,       cnt + 1, size);
		else 
			printf(MSG_PART_DISPLAY2, cnt + 1);
	}

	partitionEntry *entry = &partitions[selected];
	u32           buttons = Wpad_WaitButtons();

	/* UP/DOWN buttons */
	if (buttons & WPAD_BUTTON_UP) {
		if ((--selected) <= -1)
			selected = MAX_PARTITIONS - 1;
	}

	if (buttons & WPAD_BUTTON_DOWN) {
		if ((++selected) >= MAX_PARTITIONS)
			selected = 0;
	}

	/* B button */
	if (buttons & WPAD_BUTTON_B)
		return;

	/* Valid partition */
	if (entry->size) {
		/* A button */
		if (buttons & WPAD_BUTTON_A)
			goto format;
	}

	goto loop;

format:
	/* Clear console */
	Con_Clear();

	printf(MSG_FORMAT_PROMPT1);
	printf(MSG_FORMAT_PROMPT2);

	printf(MSG_FORMAT_PROMPT3,                  selected + 1);
	printf(MSG_FORMAT_PROMPT4, entry->size * (sector_size / GB_SIZE), entry->type);

	printf(MSG_FORMAT_PROMPT5);
	printf(MSG_FORMAT_PROMPT6);

	/* Wait for user answer */
	for (;;) {
		u32 buttons = Wpad_WaitButtons();

		/* A button */
		if (buttons & WPAD_BUTTON_A)
			break;

		/* B button */
		if (buttons & WPAD_BUTTON_B)
			goto loop;
	}

	printf(MSG_FORMAT_PROMPT7);
	fflush(stdout);

	/* Format partition */
	ret = WBFS_Format(entry->sector, entry->size);
	if (ret < 0) {
		printf(MSG_ERROR_FORMAT , ret);
		goto out;
	} else
		printf(MSG_FORMAT_OK);

out:
	printf("\n");
	printf(MSG_FORMAT_CONTINUE);

	/* Wait for any button */
	Wpad_WaitButtons();
}

void __Menu_Controls(void)
{
	u32 buttons = Wpad_WaitButtons();

	/* UP/DOWN buttons */
	if (buttons & WPAD_BUTTON_UP)
		__Menu_MoveList(-1);

	if (buttons & WPAD_BUTTON_DOWN)
		__Menu_MoveList(1);

	/* LEFT/RIGHT buttons */
	if (buttons & WPAD_BUTTON_LEFT)
		__Menu_MoveList(-ENTRIES_PER_PAGE);

	if (buttons & WPAD_BUTTON_RIGHT)
		__Menu_MoveList(ENTRIES_PER_PAGE);


	/* HOME button */
	if (buttons & WPAD_BUTTON_HOME)
		Restart();

	/* PLUS (+) button */
	if (buttons & WPAD_BUTTON_PLUS)
		Menu_Install();

	/* MINUS (-) button */
	if (buttons & WPAD_BUTTON_MINUS)
		Menu_Remove();

	/* ONE (1) button */
	if (buttons & WPAD_BUTTON_1)
		Menu_Device();

	/* A button */
	if (buttons & WPAD_BUTTON_A)
		Menu_Boot("","");
}


void Menu_Device(void)
{
	u32 timeout = 30;
	s32 ret;

	/* Ask user for device */
	for (;;) {
		char *devname = MSG_DEVNAME_UNKNOWN;

		/* Set device name */
		switch (wbfsDev) {
		case WBFS_DEVICE_USB:
			devname = MSG_USB_MASS_STORAGE;
			break;

		case WBFS_DEVICE_SDHC:
			devname = MSG_SD_SDHC;
			break;
		}

		/* Clear console */
		Con_Clear();

		printf(MSG_SELECT_DEVICE);
		printf("    < %s >\n\n", devname);

		printf(MSG_DEVICE_SELECT);
		printf(MSG_PRESSA_CONTINUE);

		u32 buttons = Wpad_WaitButtons();

		/* LEFT/RIGHT buttons */
		if (buttons & WPAD_BUTTON_LEFT) {
			if ((--wbfsDev) < WBFS_MIN_DEVICE)
				wbfsDev = WBFS_MAX_DEVICE;
		}
		if (buttons & WPAD_BUTTON_RIGHT) {
			if ((++wbfsDev) > WBFS_MAX_DEVICE)
				wbfsDev = WBFS_MIN_DEVICE;
		}

		/* A button */
		if (buttons & WPAD_BUTTON_A)
			break;
	}

	printf(MSG_MOUNTING_DEVICE_WAIT);
	printf(MSG_TIMEOUT, timeout);
	fflush(stdout);

	/* Initialize WBFS */
	ret = WBFS_Init(wbfsDev, timeout);
	if (ret < 0) {
		printf(MSG_ERROR_WBFSINIT, ret);

		/* Restart wait */
		Restart_Wait();
	}
	
		/* Try to open device */
	while (WBFS_Open() < 0) {
		/* Clear console */
		Con_Clear();

		printf(MSG_FORMAT_CONFIRM_1);

		printf(MSG_FORMAT_CONFIRM_2);
		printf(MSG_FORMAT_CONFIRM_3);

		printf(MSG_FORMAT_CONFIRM_4);
		printf(MSG_FORMAT_CONFIRM_5);
		
		/* Wait for user answer */
		for (;;) {
			u32 buttons = Wpad_WaitButtons();

			/* A button */
			if (buttons & WPAD_BUTTON_A)
				break;

			/* B button */
			if (buttons & WPAD_BUTTON_B)
				Restart();
		}

		/* Format device */
		Menu_Format();
	}

	/* Get game list */
	__Menu_GetEntries();
}


void Menu_Install(void)
{
	static struct discHdr header ATTRIBUTE_ALIGN(32);

	s32 ret;

	/* Clear console */
	Con_Clear();

	printf(MSG_INSTALL_PROMPT1);
	printf(MSG_INSTALL_PROMPT2);

	printf(MSG_INSTALL_PROMPT3);
	printf(MSG_INSTALL_PROMPT4);

	/* Wait for user answer */
	for (;;) {
		u32 buttons = Wpad_WaitButtons();

		/* A button */
		if (buttons & WPAD_BUTTON_A)
			break;

		/* B button */
		if (buttons & WPAD_BUTTON_B)
			return;
	}

	/* Disable WBFS mode */
	Disc_SetWBFS(0, NULL);

	printf(MSG_INSERT_DISC);
	fflush(stdout);

	/* Wait for disc */
	ret = Disc_Wait();
	if (ret < 0) {
		printf(MSG_DISC_ERROR, ret);
		goto out;
	} else
		printf(MSG_DISC_OK );

	printf(MSG_OPENING_DISC);
	fflush(stdout);

	/* Open disc */
	ret = Disc_Open();
	if (ret < 0) {
		printf(MSG_DISC_ERROR, ret);
		goto out;
	} else
		printf(MSG_DISC_OK_2);

	/* Check disc */
	ret = Disc_IsWii();
	if (ret < 0) {
		printf(MSG_NOT_VALID_DISC);
		goto out;
	}

	/* Read header */
	Disc_ReadHeader(&header);

	/* Check if game is already installed */
	ret = WBFS_CheckGame(header.id);
	if (ret) {
		printf(MSG_ERROR_GAME_ALREADY_INSTALLED);
		goto out;
	}

	printf(MSG_INSTALLING_GAME);

	printf("    %s\n",           header.title);
	printf("    (%c%c%c%c)\n\n", header.id[0], header.id[1], header.id[2], header.id[3]);

	/* Install game */
	ret = WBFS_AddGame();
	if (ret < 0) {
		printf(MSG_ERROR_INSTALLING_GAME, ret);
		goto out;
	}

	/* Reload entries */
	__Menu_GetEntries();

out:
	printf("\n");
	printf(MSG_INSTALL_CONTINUE);

	/* Wait for any button */
	Wpad_WaitButtons();
}

void Menu_Remove(void)
{
	struct discHdr *header = NULL;

	s32 ret;

	/* No game list */
	if (!gameCnt)
		return;

	/* Selected game */
	header = &gameList[gameSelected];

	/* Clear console */
	Con_Clear();

	printf(MSG_REMOVE_CONFIRMATION_1);
	printf(MSG_REMOVE_CONFIRMATION_2);

	/* Show game info */
	__Menu_PrintInfo(header);

	printf(MSG_REMOVE_CONTINUE_1);
	printf(MSG_REMOVE_CONTINUE_2);

	/* Wait for user answer */
	for (;;) {
		u32 buttons = Wpad_WaitButtons();

		/* A button */
		if (buttons & WPAD_BUTTON_A)
			break;

		/* B button */
		if (buttons & WPAD_BUTTON_B)
			return;
	}

	printf(MSG_REMOVING_WAIT);
	fflush(stdout);

	/* Remove game */
	ret = WBFS_RemoveGame(header->id);
	if (ret < 0) {
		printf(MSG_ERROR_REMOVING, ret);
		goto out;
	} else
		printf(MSG_REMOVING_OK);

	/* Reload entries */
	__Menu_GetEntries();

out:
	printf("\n");
	printf(MSG_REMOVE_CONTINUE);

	/* Wait for any button */
	Wpad_WaitButtons();
}

#endif


void Menu_Boot(char * discId, char * config)
{
	struct discHdr *header = NULL;
	LoaderConfig loaderConfig;
	s32 ret;

	#ifndef __CRAPMODE__

	/* No game list */
	if (!gameCnt)
		return;

	/* Selected game */
	header = &gameList[gameSelected];

	/* Clear console */
	Con_Clear();

	printf(MSG_BOOT_CONFIRM_1);
	printf("    game?\n\n");

	/* Show game info */
	__Menu_PrintInfo(header);

	printf(MSG_BOOT_CONTINUE_1);
	printf(MSG_BOOT_CONTINUE_2);

	/* Wait for user answer */
	for (;;) {
		u32 buttons = Wpad_WaitButtons();

		/* A button */
		if (buttons & WPAD_BUTTON_A)
			break;

		/* B button */
		if (buttons & WPAD_BUTTON_B)
			return;
	}

	printf("\n");
	printf(MSG_GAME_BOOTING);
	
	/* Set WBFS mode */
	Disc_SetWBFS(wbfsDev, header->id);

	#else
	
	parseConfiguration(config, discId, &loaderConfig);
	
	//sleep(10);
	
	if (loaderConfig.verboseLog) { printf("USB Loader 1.5 by Waninkoko, Crappified version by WiiCrazy/I.R.on\n"); }
	if (loaderConfig.useSDLoader) {
		if (loaderConfig.verboseLog) { printf("Mounting SD Card\n"); }	
		wbfsDev = WBFS_DEVICE_SDHC;
		ret = WBFS_Init(2, 15);		
	} else 
	{
		wbfsDev = WBFS_DEVICE_USB;
		if (loaderConfig.verboseLog) { printf("Mounting USB Drive\n"); }		
		ret = WBFS_Init(1, 15);
	}
	
	//sleep(3);
	
	WBFS_Open();
	
	if (loaderConfig.verboseLog) { printf("Setting WBFS Mode\n"); }			
	/* Set WBFS mode */
	Disc_SetWBFS(wbfsDev, discId);
	
	#endif

	if (loaderConfig.verboseLog) { printf("Opening Disc\n"); }			
	/* Open disc */
	ret = Disc_Open();
	if (ret < 0) {
		printf(MSG_DISC_OPEN_ERROR, ret);
		goto out;
	}
	
	if (loaderConfig.verboseLog) { printf("Boting Game %s\n", discId); }
	
	/* Boot Wii disc */
	Disc_WiiBoot(loaderConfig);

	printf(MSG_GAME_BOOT_ERROR, ret);

out:
	printf("\n");
	printf(MSG_BOOT_CONTINUE);

	/* Wait for button */
	Wpad_WaitButtons();
}

#ifndef __CRAPMODE__
void Menu_Loop(void)
{
	/* Device menu */
	Menu_Device();

	/* Get game list */
	__Menu_GetEntries();

	/* Menu loop */
	for (;;) {
		/* Clear console */
		Con_Clear();

		/* Show gamelist */
		__Menu_ShowList();

		/* Show cover */
		__Menu_ShowCover();

		/* Controls */
		__Menu_Controls();
	}
}
#endif
