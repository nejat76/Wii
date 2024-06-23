#ifndef _APPLOADER_H_
#define _APPLOADER_H_

/* Entry point */
typedef void (*entry_point)(void);

/* Prototypes */
bool Remove_001_Protection(void *, int );

s32 Apploader_Run(LoaderConfig loaderConfig, entry_point *); 

#endif
