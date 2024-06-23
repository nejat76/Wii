#ifndef _MENU_H_
#define _MENU_H_

#ifdef __CRAPMODE__

void Menu_Boot(char *, char *);
#else
/* Prototypes */
void Menu_Device(void);
void Menu_Format(void);
void Menu_Install(void);
void Menu_Remove(void);
void Menu_Loop(void);
void Menu_Boot(char *, char *);
#endif


#endif

