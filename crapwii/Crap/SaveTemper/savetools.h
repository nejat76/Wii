
#ifndef _SAVE_TOOLS_H
#define _SAVE_TOOLS_H


#ifdef __cplusplus
	extern "C" {
#endif

int twintig(char * app_folder, char * sourceDirectory, char * sourceFolder, char * dataFile);
int tachtig(char * app_folder, char * dataFile);
int extract(char * iso, char * filename );
int extractwad(char * extractionfolder, char * app_folder, char * wadFileName);
int signBanner(char * bannerFile, char * newBannerFile, u8 * title_name);
int packwad(u8 * appPath,u8* wadFile, u8* trailerFile, u8 * ticketFile, u8 * tmdFile, u8 * certFile, u8 sign_type, u8 sign_tik, u8 sign_tmd, u8 *new_id);
char * get_wadlib_titleid(void);
#ifdef __cplusplus
	}
#endif

#endif