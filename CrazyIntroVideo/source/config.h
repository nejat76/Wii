//config.h
//Copyright (C) 2009 by WiiCrazy (I.R.on)
//This is Free Software released under the GNU/GPL License.

#ifndef _CRAZYINTRO_CFG_
#define _CRAZYINTRO_CFG_

u64 GetUpTitleId(void);

u64 GetDownTitleId(void);

u64 GetRightTitleId(void);

u64 GetLeftTitleId(void);

bool ConfigExists(void);

void CreateConfigFile(void);

void LoadConfigFile(void);

char * GetVideoFile();

#endif
