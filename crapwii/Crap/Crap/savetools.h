//Crap, Copyright 2009 WiiCrazy/I.R.on of Irduco (nejat@tepetaklak.com)
//Distributed under the terms of the GNU GPL v2.0
//See http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt for more information

#ifndef _SAVE_TOOLS_H
#define _SAVE_TOOLS_H


#ifdef __cplusplus
	extern "C" {
#endif

int twintig(char * app_folder, char * sourceDirectory, char * sourceFolder, char * dataFile);
int tachtig(char * app_folder, char * dataFile);
int extract(char * iso, char * filename );
int extractwad(char * extractionfolder, char * app_folder, char * wadFileName);
int signBanner(char * bannerFile, char * newBannerFile, unsigned char * title_name);
int packwad(unsigned char * appPath, unsigned char * wadFile, unsigned char* trailerFile,unsigned char  * ticketFile, unsigned char  * tmdFile, unsigned char  * certFile, unsigned char  sign_type, unsigned char  sign_tik, unsigned char  sign_tmd, unsigned char  *new_id);

int get_savelib_errno(void);
char * get_savelib_errstr(void);
char * get_savelib_output(void);
char * get_wadlib_titleid(void);

#ifdef __cplusplus
	}
#endif

#endif