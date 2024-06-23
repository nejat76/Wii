//config.c
//Copyright (C) 2009 by WiiCrazy (I.R.on)
//This is Free Software released under the GNU/GPL License.
#include <stdio.h>
#include <libxml\mxml.h>
#include <gccore.h>
#include <errno.h>
#include "config.h"
#include <fat.h>

//#define LDEBUG

#define CRAZY_INTRO_CFG_FILE "fat0:/crazyintro.xml"

char version[10];

u64 upTitleId = 0;
char upTitleName[40];

u64 downTitleId = 0;
char downTitleName[40];

u64 rightTitleId = 0;
char rightTitleName[40];

u64 leftTitleId = 0;
char leftTitleName[40];

u8 upTitleType = '1';
u8 downTitleType = '1';
u8 rightTitleType = '1';
u8 leftTitleType = '1';

char videoFile[1024];

u64 GetUpTitleId() 
{
	return upTitleId;
}

u64 GetDownTitleId() 
{
	return downTitleId;
}

u64 GetRightTitleId() 
{
	return rightTitleId;
}

u64 GetLeftTitleId() 
{
	return leftTitleId;
}

char * GetVideoFile() 
{
	return videoFile;
}


u64 StrTitleIdToLong(char * str_title_id, u8 title_type) 
{
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
	
	//if (*str_title_id==NULL) 
	//{
	//	return 0LL;
	//}
	
	title_id = title_id + 
	(((u64)str_title_id[0])<<24) + 
	(((u64)str_title_id[1])<<16) + 
	(((u64)str_title_id[2])<<8) + 
	((u64)str_title_id[3]);
	
	return title_id;
}


bool ConfigExists() 
{
	FILE *fp = fopen(CRAZY_INTRO_CFG_FILE,"r");
	if( fp ) 
	{
		fclose(fp);
		return true;
	} else 
	{
		return false;
	}
}

void LoadConfigFile()
{
   FILE *fp;
   mxml_node_t *tree;
   mxml_node_t *data;
   mxml_node_t *group;
   mxml_node_t *upB;
   mxml_node_t *leftB;
   mxml_node_t *rightB;
   mxml_node_t *downB;
   mxml_node_t *pictureintroeffect;
   mxml_node_t *pictureoutroeffect;   
   mxml_node_t *videofile;   

   const char * tmp;

	#ifdef LDEBUG
	printf("\nLoading Config File");
	sleep(1);
	#endif

   /*Load our xml file! */
   fp = fopen(CRAZY_INTRO_CFG_FILE, "r");
   if (!fp) return;
   	#ifdef LDEBUG
	printf("\nFile opened");
	sleep(1);
	#endif

   tree = mxmlLoadFile(NULL, fp, MXML_NO_CALLBACK);
   fclose(fp);
   
   	#ifdef LDEBUG
	printf("\nXml Loaded");
	sleep(1);
	#endif


   /*Load and printf our values! */
   /* As a note, its a good idea to normally check if node* is NULL */
   data = mxmlFindElement(tree, tree, "config", NULL, NULL, MXML_DESCEND);
   
   	#ifdef LDEBUG
	printf("\nConfig found");
	sleep(1);
	#endif

  
   tmp = mxmlElementGetAttr(data,"version");
   if (tmp!=NULL) strcpy(version,tmp);
   
   	#ifdef LDEBUG
	printf("\nVersion read %s", version);
	sleep(1);
	#endif

   
   upB = mxmlFindElement(tree, tree, "UpButtonLaunches", NULL, NULL, MXML_DESCEND);  
   tmp = mxmlElementGetAttr(upB, "title_type"); if (tmp!=NULL) upTitleType = tmp[0];
   tmp = mxmlElementGetAttr(upB, "title_id"); upTitleId = StrTitleIdToLong(tmp,upTitleType);        
   tmp = mxmlElementGetAttr(upB, "title_name"); if (tmp!=NULL) strcpy(upTitleName,tmp);    

   leftB = mxmlFindElement(tree, tree, "LeftButtonLaunches", NULL, NULL, MXML_DESCEND);   
   tmp = mxmlElementGetAttr(leftB, "title_type"); if (tmp!=NULL) leftTitleType = tmp[0];
   tmp = mxmlElementGetAttr(leftB, "title_id"); leftTitleId = StrTitleIdToLong(tmp, leftTitleType); 
   tmp = mxmlElementGetAttr(leftB, "title_name"); if (tmp!=NULL) strcpy(leftTitleName,tmp); 

   rightB = mxmlFindElement(tree, tree, "RightButtonLaunches", NULL, NULL, MXML_DESCEND);
   tmp = mxmlElementGetAttr(rightB, "title_type"); if (tmp!=NULL) rightTitleType = tmp[0];      
   tmp = mxmlElementGetAttr(rightB, "title_id");  rightTitleId = StrTitleIdToLong(tmp,rightTitleType);   
   tmp = mxmlElementGetAttr(rightB, "title_name"); if (tmp!=NULL) strcpy(leftTitleName,tmp); 

   downB = mxmlFindElement(tree, tree, "DownButtonLaunches", NULL, NULL, MXML_DESCEND);
   tmp = mxmlElementGetAttr(downB, "title_type"); if (tmp!=NULL) downTitleType = tmp[0];         
   tmp = mxmlElementGetAttr(downB, "title_id"); downTitleId = StrTitleIdToLong(tmp,downTitleType);   
   tmp = mxmlElementGetAttr(downB, "title_name"); if (tmp!=NULL) strcpy(downTitleName,tmp); 


   videofile = mxmlFindElement(tree, tree, "VideoFile", NULL, NULL, MXML_DESCEND);
   tmp = mxmlElementGetAttr(videofile , "filename"); if (tmp!=NULL) strcpy(videoFile,tmp); 

    #ifdef LDEBUG
	printf("\nBegin dealloc");
	sleep(1);
	#endif

/*
   if(downB!=NULL) mxmlDelete(downB);         
   if(rightB!=NULL) mxmlDelete(rightB);      
   if(leftB!=NULL) mxmlDelete(leftB);      
   if(upB!=NULL) mxmlDelete(upB);   
   if(group!=NULL) mxmlDelete(group);
   if(data!=NULL) mxmlDelete(data);
   if(tree!=NULL) mxmlDelete(tree);
   */
   
    #ifdef LDEBUG
	printf("\nDealloc finished");
	sleep(1);
	#endif

}


