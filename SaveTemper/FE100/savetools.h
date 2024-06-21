
#ifndef _SAVE_TOOLS_H
#define _SAVE_TOOLS_H


#ifdef __cplusplus
	extern "C" {
#endif

int twintig(int useBannerBin, char * app_folder, char * sourceDirectory, char * sourceFolder, char * dataFile);
int tachtig(int useBannerBin, char * app_folder, char * dataFile);
int get_savelib_errno(void);
char * get_savelib_errstr(void);
char * get_savelib_output(void);

#ifdef __cplusplus
	}
#endif

#endif