// MPlayer Wii glue code
// Some portions shamelessly stolen from the Hively Player source code

//Notes by WiiCrazy (I.R.on)
//Most of the core stuff is untouched, just some Video_Flush statements which seemed to generate lots of garbage
//display both at the start and end of the video display
//And also the key detection and responding routines... Hence I just needed to return the stop video key 

#include <stdio.h>
#include <stdlib.h>
#include <gccore.h>
#include <wiiuse/wpad.h>
#include <fat.h>
#include <string.h>
#include <unistd.h>
#include <sys/dir.h>
#include <malloc.h>
#include <ogcsys.h>
#include <errno.h>
#include <network.h>
#include <sys/iosupport.h>
#include "config.h"
#include "menu_tik_bin.h"
#include "menu_tmd_bin.h"
#include "menu_certs_bin.h"
#include "time.h"

#include "gx_supp.h"

#define DEFAULT_FIFO_SIZE 256 * 1024

#define MEMALIGN(x) __attribute__((__aligned__(x)))

static void *xfb[2] = {NULL,};
static int whichfb = 0;
static float m_scr_width, m_scr_height;
static float m_outaspect = 1.0f;
float m_screenleft_shift = 0, m_screenright_shift = 0;
float m_screentop_shift = 0, m_screenbottom_shift = 0;
static int m_palwsfix = 0;

static GXRModeObj *rmode = NULL;

//static GXTexObj texobj MEMALIGN(32);
static unsigned char gp_fifo[DEFAULT_FIFO_SIZE] MEMALIGN(32);
static unsigned char *texturemem MEMALIGN(32) = NULL; 
//static Mtx44 projectionMatrix MEMALIGN(32);
//static Mtx modelViewMatrix MEMALIGN(32);

//mplayer var/functions
extern int main2(int argc,char* argv[]);
extern void mplayer_init_font();
extern int mp_msg_level_all;
extern int correct_pts;
char *cookies_file = NULL;
int rtsp_port = 0;
char *rtsp_destination = NULL; 
int g_force_cache = 0, g_cache_size = 0;


//Mplayer audio stuff callback
#define BUF_LEN 8192
#define NB_BUF 32
//#define BUF_LEN 32768
//#define NB_BUF 8
#define BUF_TOTALSIZE (NB_BUF*BUF_LEN)
#define AUDIO_PREBUF (BUF_TOTALSIZE/4)

static int curplay = 0, curwrite = 0;
static int inbuf = 0, audplay = 0;
static u8 audioBuf[NB_BUF][BUF_LEN] ATTRIBUTE_ALIGN(32);
static mutex_t m_audioMutex;
static int m_au_rate, m_au_channels, m_au_bps;

void wii_audio_reset()
{
	LWP_MutexLock(m_audioMutex);
	AUDIO_StopDMA();
	memset( audioBuf, 0, BUF_TOTALSIZE );
	DCFlushRange( audioBuf, BUF_TOTALSIZE );
	curplay = curwrite = 0;
	inbuf = 0; audplay = 0;
	LWP_MutexUnlock(m_audioMutex);
}

int wii_audio_init(int rate, int channels, int bps)
{
	m_au_rate = rate;
	m_au_channels = channels;
	m_au_bps = bps;
	wii_audio_reset();
	return 0;
}

static void dmaCallback()
{	
	LWP_MutexLock(m_audioMutex);
	
	if(audplay)
	{
		AUDIO_StopDMA();
		
		inbuf -= BUF_LEN;
		if(inbuf<0) 
		{
			inbuf = 0;
			audplay = 0;
			LWP_MutexUnlock(m_audioMutex);
			return;
		}
	}
	
	AUDIO_InitDMA( (u32)audioBuf[curplay], BUF_LEN );
	AUDIO_StartDMA();
	curplay++;
	if(curplay>=NB_BUF) curplay = 0;
	
	audplay = 1;
	
	LWP_MutexUnlock(m_audioMutex);
}

int audioGetAvail()
{
	return BUF_TOTALSIZE-inbuf;
}

int wii_audio_get_space(void)
{
	int nb = 0;
	LWP_MutexLock(m_audioMutex);
	nb = audioGetAvail();
	LWP_MutexUnlock(m_audioMutex);
	return nb;
}
int wii_audio_play(char *data,int len,int flags)
{	
	int eat = 0;
	
	LWP_MutexLock(m_audioMutex);
	
	while(len>0)
	{
		int l = len;
		int remain = audioGetAvail();
		if(l>remain) l = remain;
		if(l>BUF_LEN) l = BUF_LEN;
		if(l<BUF_LEN)
			break;
		memcpy(audioBuf[curwrite], data, l);
		DCFlushRange(audioBuf[curwrite], l);
		curwrite++;
		if(curwrite>=NB_BUF) curwrite = 0;
		inbuf+=l;
		
		len -=l;
		data += l;
		eat += l;	
	}
	
	LWP_MutexUnlock(m_audioMutex);	
	
	//launch playback here if passed a certain threshold
	if(!audplay && inbuf>=AUDIO_PREBUF)
	{
		dmaCallback();
	}
		
	return eat;
}
float wii_audio_get_delay(int in_mplayerbuf)
{
	int nb = 0;
	LWP_MutexLock(m_audioMutex);
	if(!audplay)
		nb = inbuf;
	else
		nb = inbuf - (BUF_LEN - AUDIO_GetDMABytesLeft());
	LWP_MutexUnlock(m_audioMutex);
	
	return (float)nb/(float)m_au_bps;
}


// POWER BUTTON

u8 HWButton = 0;

/**
 * Callback for the reset button on the Wii.
 */
void WiiResetPressed()
{
	HWButton = SYS_RETURNTOMENU;
}
 
/**
 * Callback for the power button on the Wii.
 */
void WiiPowerPressed()
{
	HWButton = SYS_POWEROFF_STANDBY;
}
 
/**
 * Callback for the power button on the Wiimote.
 * @param[in] chan The Wiimote that pressed the button
 */
void WiimotePowerPressed(s32 chan)
{
	HWButton = SYS_POWEROFF_STANDBY;
}


void my_exit()
{
	WPAD_Shutdown();
	usleep(500); 
	
	SYS_ResetSystem(HWButton, 0, 0);
	
	exit(0);
}

// END POWER BUTTON


//Mplayer video stuff callback
static int m_width, m_height;
extern GXRModeObj *vmode;
void wii_video_config(int width, int height)
{
	m_width = width;
	m_height = height;

  GX_InitVideo();

  {
  float sar, par; 

  int image_width = vmode->viWidth, image_height = vmode->xfbHeight;
  int d_width = width, d_height = height;

  if (CONF_GetAspectRatio())
		sar = 16.0f / 9.0f;
	else
		sar = 4.0f / 3.0f;

	par = (float) d_width / (float) d_height;
	par *= (float) vmode->fbWidth / (float) vmode->xfbHeight;
	par /= sar;

	if (par > sar) {
		width = vmode->viWidth;
		height = (float) width / par;
	} else {
		height = vmode->viHeight;
		width = (float) height * par + vmode->viWidth - vmode->fbWidth;
	}
  GX_SetCamPosZ(345);
  GX_StartYUV(image_width, image_height, width / 2, height / 2); 
  }
}

void wii_draw_frame(unsigned char *src[])
{
	mp_msg_level_all = 0; //disable mplayer's text output
	
  //YV12 drawing implementation
  {
    u16 pitch[3];
    int image_width = m_width, image_height = m_height;

    pitch[0] = image_width;
    pitch[1] = image_width / 2;
    pitch[2] = image_width / 2; 
    GX_RenderYUV(image_width, image_height, src, pitch); 
  }
 }

static int m_krept = 0;
static time_t m_trept;



void CheckRetval(int retval)
{
	switch(retval)
	{
		case ES_EINVAL:
			printf("FAILED! (Invalid Argument)\n\tQuitting...\n");
			RetvalFail();
			break;

		case ES_ENOMEM:
			printf("FAILED! (Out of memory)\n\tQuitting...\n");
			RetvalFail();
			break;

		case ES_ENOTINIT:
			printf("FAILED! (Not Initialized)\n\tQuitting...\n");
			RetvalFail();
			break;

		case ES_EALIGN:
			printf("FAILED! (Not Aligned)\n\tQuitting...\n");
			RetvalFail();
			break;
	}
}

void RetvalFail()
{
	VIDEO_Flush();
	VIDEO_WaitVSync();
	printf("\n\nEnding ES!... ");

	int retval2=__ES_Close();

	if(retval2!=0)
	{
		printf("FAILED!\n\tQuitting...\n");
	}
	exit(-1);
}

void SystemMenuAuth()
{
	int rest = ES_Identify((signed_blob*)menu_certs_bin, 
					menu_certs_bin_size, 
					(signed_blob*)menu_tmd_bin, 
					menu_tmd_bin_size, 
					(signed_blob*)menu_tik_bin, 
					menu_tik_bin_size, 
					0);
	CheckRetval(rest);
}

void clean_up() 
{
	LWP_MutexDestroy(m_audioMutex);
}

void launchTitle(u64 titleID, char* name, int need_sys)
{
	clean_up();
	int retval;
	if(need_sys)
		SystemMenuAuth();

	VIDEO_Flush();
	VIDEO_WaitVSync();

	u32 cnt ATTRIBUTE_ALIGN(32);
	retval = ES_GetNumTicketViews(titleID, &cnt);
	CheckRetval(retval);
	tikview *views = (tikview *)memalign( 32, sizeof(tikview)*cnt );

	retval = ES_GetTicketViews(titleID, views, cnt);
	CheckRetval(retval);

	retval = ES_LaunchTitle(titleID, &views[0]);
	CheckRetval(retval);
}

static u64 title_to_launch = 0x0001000148415858LL;

void exitToSystemMenu() 
{
	WPAD_Shutdown();
	usleep(500); 
	SYS_ResetSystem(SYS_RETURNTOMENU, 0, 0);	
}

//Should replace this, somewhat bugged... actual keypresses handled by
//precompiled mplayerwii code so I just pause the video to prevent the
//irritating sound then at the next press 
int wii_check_events() 
{
	bool continueRead=false;
	s32 pressed;
	s32 held;

	WPAD_ScanPads();
	pressed = WPAD_ButtonsDown(0);
	held = WPAD_ButtonsHeld(0);
	
	if(HWButton ) { my_exit(); }

	if((pressed & WPAD_BUTTON_A) || (held & WPAD_BUTTON_A))
	{
		return 1;
	}

	if((pressed & WPAD_BUTTON_B) || (held & WPAD_BUTTON_B))
	{
		return 1;
	}

	if((pressed & WPAD_BUTTON_LEFT) || (held & WPAD_BUTTON_LEFT))
	{	
		return 1;
	}

	if((pressed & WPAD_BUTTON_RIGHT) || (held & WPAD_BUTTON_RIGHT))
	{
		return 1;
	}

	if((pressed & WPAD_BUTTON_UP) || (held & WPAD_BUTTON_UP))
	{
		return 1;
	}

	if((pressed & WPAD_BUTTON_DOWN) || (held & WPAD_BUTTON_DOWN))
	{
		return 1;
	}


	return 0;
}


void wii_sleep(float t)
{
	struct timespec ts = {0,};
	t/=2;
	if(t>=1.0f) 
		ts.tv_nsec = TB_NSPERSEC-1;
	else
		ts.tv_nsec = t * TB_NSPERSEC;
	nanosleep(&ts);
}

void respondToKey() 
{
	bool continueRead=false;
	s32 pressed;
	s32 held;

	WPAD_ScanPads();
	pressed = WPAD_ButtonsDown(0);
	held = WPAD_ButtonsHeld(0);
	
	if((pressed & WPAD_BUTTON_A) || (held & WPAD_BUTTON_A))
	{
		exitToSystemMenu();
	}

	if((pressed & WPAD_BUTTON_B) || (held & WPAD_BUTTON_B))
	{
		continueRead = true;
	}

	if((pressed & WPAD_BUTTON_LEFT) || (held & WPAD_BUTTON_LEFT))
	{	
		if (GetLeftTitleId()!=0) {
			title_to_launch = GetLeftTitleId();
			continueRead = true;
		}
	}

	if((pressed & WPAD_BUTTON_RIGHT) || (held & WPAD_BUTTON_RIGHT))
	{
		if (GetRightTitleId()!=0) {
			title_to_launch = GetRightTitleId();
			continueRead = true;
		}
	}

	if((pressed & WPAD_BUTTON_UP) || (held & WPAD_BUTTON_UP))
	{
		if (GetUpTitleId()!=0) {
			title_to_launch = GetUpTitleId();
			continueRead = true;
		}
	}

	if((pressed & WPAD_BUTTON_DOWN) || (held & WPAD_BUTTON_DOWN))
	{
		if (GetDownTitleId()!=0) {
			title_to_launch = GetDownTitleId();
			continueRead = true;
		}
	}

	if (continueRead) 
	{
	      VIDEO_WaitVSync(); 
		int rest = ES_Identify((signed_blob*)menu_certs_bin, menu_certs_bin_size, (signed_blob*)menu_tmd_bin, menu_tmd_bin_size, (signed_blob*)menu_tik_bin, menu_tik_bin_size, 0);
		
		__ES_Close();
		__ES_Init();

		if (title_to_launch!=0LL) {
			launchTitle(title_to_launch, "", 1);
		} else {
			exitToSystemMenu();
		}
	}
}

void play_file(char *filename)
{		
	char *args[]={"", filename};
	
	//printf("\x1b[2J\x1b[2;0H");
	mp_msg_level_all = -1; //disable mplayer text output	
	
	main2(2, args);
	free(texturemem);
	texturemem = NULL;
	
	whichfb = 0;
	VIDEO_SetNextFramebuffer(xfb[whichfb]);
	AUDIO_StopDMA();	
	
	curplay = curwrite = 0;
	inbuf = 0; audplay = 0;
	
	//VIDEO_Flush();
	VIDEO_SetBlack(TRUE);

	respondToKey();	
}


///////////////////////////////////////

#define MAXPATHLEN 1024
#define SCR_FILELEN 80
static int topfile;

#define MAX_NBDISP 20



void setupScreen()
{
	// Obtain the preferred video mode from the system
	// This will correspond to the settings in the Wii menu
	rmode = VIDEO_GetPreferredMode(NULL);
	if(m_palwsfix)
	{
		rmode->viWidth = 678;
		rmode->viXOrigin = (VI_MAX_WIDTH_PAL - 678)/2;
	}

	// Allocate memory for the display in the uncached region
	xfb[0] = MEM_K0_TO_K1(SYS_AllocateFramebuffer(rmode));
	xfb[1] = MEM_K0_TO_K1(SYS_AllocateFramebuffer(rmode));
	whichfb = 0;
	
	// Initialise the console, required for printf
	//We do not want any printfs so we are disabling it
	//precompiled main2 just prints the filename which is annoying...
	//console_init(xfb[whichfb],20,20,rmode->fbWidth,rmode->xfbHeight,rmode->fbWidth*VI_DISPLAY_PIX_SZ);
	
	// Set up the video registers with the chosen mode
	VIDEO_Configure(rmode);
	
	// Tell the video hardware where our display memory is
	VIDEO_SetNextFramebuffer(xfb[whichfb]);
	
	// Make the display visible
	VIDEO_SetBlack(FALSE);

	// Flush the video register changes to the hardware
	//VIDEO_Flush();

	// Wait for Video setup to complete
	VIDEO_WaitVSync();
	if(rmode->viTVMode&VI_NON_INTERLACE) VIDEO_WaitVSync();

	// init GX stuff
	memset(&gp_fifo, 0, DEFAULT_FIFO_SIZE);
	GXColor background = {0, 0, 0, 0xff};
    GX_Init(&gp_fifo, DEFAULT_FIFO_SIZE);	
    GX_SetCopyClear(background, 0x00ffffff); 
	GX_SetViewport(0,0,rmode->fbWidth,rmode->efbHeight,0,1);
    GX_SetDispCopyYScale((f32)rmode->xfbHeight/(f32)rmode->efbHeight);
    GX_SetDispCopySrc(0,0,rmode->fbWidth,rmode->efbHeight);
    GX_SetDispCopyDst(rmode->fbWidth,rmode->xfbHeight); 
	GX_SetPixelFmt(GX_PF_RGB8_Z24, GX_ZC_LINEAR);
    GX_SetCullMode(GX_CULL_NONE);
    GX_SetZMode(GX_FALSE,GX_ALWAYS,GX_TRUE);
    GX_SetColorUpdate(GX_TRUE);
    GX_CopyDisp(xfb[whichfb],GX_TRUE);
	GX_SetDispCopyGamma(GX_GM_1_0);
    GX_ClearVtxDesc();
    GX_SetVtxAttrFmt(GX_VTXFMT0,GX_VA_POS,GX_POS_XYZ,GX_F32,0);
    GX_SetVtxAttrFmt(GX_VTXFMT0,GX_VA_TEX0,GX_TEX_ST,GX_F32,0);
    GX_SetVtxDesc(GX_VA_POS,GX_DIRECT);
    GX_SetVtxDesc(GX_VA_TEX0,GX_DIRECT);
    GX_Flush();
	
	m_scr_width = rmode->fbWidth; m_scr_height = rmode->xfbHeight;
	
	//init display aspect ratio
	{
		int c = CONF_GetAspectRatio();
		if(c == CONF_ASPECT_16_9)
			m_outaspect = (16.0f * (float)m_scr_height) / ((float) m_scr_width * 9.0f);
		else
			m_outaspect = 1.0f;
	}	
}

char * getRandomFile() 
{
	srand((unsigned int)time((time_t *)NULL));
	char str[1024];
	strcpy(str, GetVideoFile());

      char *p;
	int countFiles = 0;
	int selectedRandom;
      p = strtok(str, ";");

	if (p==NULL) 
	{
		return GetVideoFile();
	}

	//Get count of filenames  
      while (p != NULL)
      {
	    countFiles++;
	    p = strtok(NULL, ";");
      }
	
	//Reinit string and randomly select one
	//Randomly select one

	selectedRandom = (int)(rand() / (((double)RAND_MAX + 1)/ countFiles));

	strcpy(str, GetVideoFile());
      p = strtok(str, ";");
	if (selectedRandom==0) 
	{
		return p;
	}

	int i;
      for (i=0;i<selectedRandom;i++)
      {
	    p = strtok(NULL, ";");
      }
	
	return p;
}


//---------------------------------------------------------------------------------
int main(int argc, char **argv) {
//---------------------------------------------------------------------------------

	VIDEO_Init();
	WPAD_Init();

	AUDIO_Init( NULL );
	LWP_MutexInit(&m_audioMutex, false);
	AUDIO_SetDSPSampleRate( AI_SAMPLERATE_48KHZ );
	AUDIO_RegisterDMACallback( dmaCallback );

	setupScreen();
	fatInit(8,false);

	//Initialize Power button callbacks
	SYS_SetResetCallback(WiiResetPressed);
	SYS_SetPowerCallback(WiiPowerPressed);
	WPAD_SetPowerButtonCallback(WiimotePowerPressed);
	
	//read configuration file from the SD card
	fatMountNormalInterface(PI_INTERNAL_SD,8);
	fatSetDefaultInterface(PI_INTERNAL_SD);	

	if (!ConfigExists()) 
	{
		exitToSystemMenu();
	}
	
	LoadConfigFile();
	char videoFileName[150];
	strcpy(videoFileName, 	"fat0:/");
	strcat(videoFileName, 	getRandomFile());
	play_file(videoFileName);
	fatUnmount(PI_INTERNAL_SD);		
	LWP_MutexDestroy(m_audioMutex);
	exitToSystemMenu();
	return 0; //not needed
}
