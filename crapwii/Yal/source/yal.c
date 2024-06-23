/*   
    Copyright (C) 2009 Kwiirk

    Yet Another Loader.  The barely minimum usb loader
    
    Based on SoftChip, which should be based on GeckoOS...

    no video, no input, try to load the wbfs passed as argument or return to menu.
    
    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#include <gccore.h>
#include <string.h>
#include <malloc.h>
#include "defs.h"
#include "dip.h"
#include <stdlib.h>
#include <unistd.h>
#include <ogc/lwp_watchdog.h>

#define CIOS 222

#include <stdarg.h>
#include <stdio.h>
void LoadLogo(void);
void DisplayLogo(void);

void debug_printf(const char *fmt, ...) {
/*
  if(usb_isgeckoalive(1)){
    char buf[1024];
    int len;
    va_list ap;
    usb_flush(1);
    va_start(ap, fmt);
    len = vsnprintf(buf, sizeof(buf), fmt, ap);
    va_end(ap);
    if (len <= 0 || len > sizeof(buf)) printf("Error: len = %d\n", len);
    else usb_sendbuffer(1, buf, len);
  }
*/
}

GXRModeObj*		vmode;					// System Video Mode
unsigned int	_Video_Mode;				// System Video Mode (NTSC, PAL or MPAL)	
u32 *xfb;  
char config[17];      
bool debugEnabled = false;
int load_disc(char *discid, int fix);

void Determine_VideoMode(char Region, bool override)
{
	// Get vmode and Video_Mode for system settings first
	u32 tvmode = CONF_GetVideo();
	// Attention: This returns &TVNtsc480Prog for all progressive video modes
        vmode = VIDEO_GetPreferredMode(0);
	switch (tvmode) 
	{
        case CONF_VIDEO_PAL:
                if (CONF_GetEuRGB60() > 0) 
                        _Video_Mode = PAL60;
                else 
                        _Video_Mode = PAL;
                break;
        case CONF_VIDEO_MPAL:
                _Video_Mode = MPAL;
                break;

        case CONF_VIDEO_NTSC:
        default:
                _Video_Mode = NTSC;
	}

	// Overwrite vmode and Video_Mode when disc region video mode is selected and Wii region doesn't match disc region
	if(override) 
	{
        switch (Region) 
        {
        case PAL_Default:
        case PAL_France:
        case PAL_Germany:
        case Euro_X:
        case Euro_Y:
                if (CONF_GetVideo() != CONF_VIDEO_PAL)
                {
                        _Video_Mode = PAL60;

                        if (CONF_GetProgressiveScan() > 0 && VIDEO_HaveComponentCable())
                                vmode = &TVNtsc480Prog; // This seems to be correct!
                        else
                                vmode = &TVEurgb60Hz480IntDf;
                }
                break;

        case NTSC_USA:
        case NTSC_Japan:
                if (CONF_GetVideo() != CONF_VIDEO_NTSC)
                {
                        _Video_Mode = NTSC;
                        if (CONF_GetProgressiveScan() > 0 && VIDEO_HaveComponentCable())
                                vmode = &TVNtsc480Prog;
                        else
                                vmode = &TVNtsc480IntDf;
                }
        default:
                break;
        }
	}
}
void Set_VideoMode(bool enableDisplay)
{
    // TODO: Some exception handling is needed here
 
    // The video mode (PAL/NTSC/MPAL) is determined by the value of 0x800000cc
    // The combination Video_Mode = NTSC and vmode = [PAL]576i, results in an error
    
    *Video_Mode = _Video_Mode;

    VIDEO_Configure(vmode);
    VIDEO_SetNextFramebuffer(xfb);
	if (enableDisplay) 
	{
		VIDEO_SetBlack(false);
	} else 
	{
		VIDEO_SetBlack(true);		
	}
    VIDEO_Flush();
    VIDEO_WaitVSync();
    if(vmode->viTVMode&VI_NON_INTERLACE) VIDEO_WaitVSync();
}
#define CERTS_SIZE	0xA00
static const char certs_fs[] ALIGNED(32) = "/sys/cert.sys";
s32 GetCerts(signed_blob** Certs, u32* Length)
{
	static unsigned char		Cert[CERTS_SIZE] ALIGNED(32);
	memset(Cert, 0, CERTS_SIZE);
	s32				fd, ret;

	fd = IOS_Open(certs_fs, ISFS_OPEN_READ);
	if (fd < 0) return fd;

	ret = IOS_Read(fd, Cert, CERTS_SIZE);
	if (ret < 0)
	{
		if (fd >0) IOS_Close(fd);
		return ret;
	}

	*Certs = (signed_blob*)(Cert);
	*Length = CERTS_SIZE;

	if (fd > 0) IOS_Close(fd);

	return 0;
}

void Reboot()
{
  exit(0);
}
void init_video(bool enableDisplay)
{
    // Initialize subsystems
    VIDEO_Init();

    // Initialize Video
    vmode = VIDEO_GetPreferredMode(0);
    xfb = MEM_K0_TO_K1(SYS_AllocateFramebuffer(vmode));

    VIDEO_Configure(vmode);
    VIDEO_SetNextFramebuffer(xfb);
	if (enableDisplay) 
	{
		VIDEO_SetBlack(false);
	} else {
		VIDEO_SetBlack(true);		
	}
    VIDEO_Flush();
    VIDEO_WaitVSync();

    if (vmode->viTVMode & VI_NON_INTERLACE) VIDEO_WaitVSync();

	// Set console parameters
    int x = 20, y = 20, w, h;
    w = vmode->fbWidth - (x * 2);
    h = vmode->xfbHeight - (y + 20);

    // Initialize the console - CON_InitEx works after VIDEO_ calls
    CON_InitEx(vmode, x, y, w, h);

    // Clear the garbage around the edges of the console
    VIDEO_ClearFrameBuffer(vmode, xfb, COLOR_BLACK);

}

void SetTime(void)
{
	/* Extern */
	extern void settime(u64);

	/* Set proper time */
	settime(secs_to_ticks(time(NULL) - 946684800));
}

//---------------------------------------------------------------------------------
int main(int argc, char **argv) {
//---------------------------------------------------------------------------------
        char discid[7];
	  int fix;
	  strcpy(discid, "LOADER");
	  //strcpy(discid, "SBLE5G");
	  strcpy(config, "CFGYAL00P0000000");
	  //strcpy(config, "CFGYAL10P0000000");
	  
	    if (config[6]=='1') 
	    {
		 debugEnabled = true;
	    }
	  #define FIXCONFIG 15
	  char fixChar = config[FIXCONFIG];
  	  fix = atoi(&fixChar);
	  
        int cios = CIOS;
        SYS_SetResetCallback(Reboot);
        debug_printf("start %s",argv[0]);
        init_video(debugEnabled);
        if (debugEnabled) printf("YAL v0.1 by Kwiirk (modded by WiiCrazy :p )\n");
        //if(usb_isgeckoalive(1))
        //        cios++;
        if (debugEnabled) printf("Loading IOS %d\n",cios);
        IOS_ReloadIOS(cios);
        sleep(1);
        int ret = load_disc(discid, fix);
        if (debugEnabled) printf("returned %d\n",ret);
        sleep(4);
        return ret;
}



/** Anti 002 fix for IOS 249 rev < 12 thanks to WiiPower **/
void Anti_002_fix(void *Address, int Size)
{
        u8 SearchPattern[12] =  { 0x2C, 0x00, 0x00, 0x00, 0x48, 0x00, 0x02, 0x14, 0x3C, 0x60, 0x80, 0x00 };
        u8 PatchData[12] =              { 0x2C, 0x00, 0x00, 0x00, 0x40, 0x82, 0x02, 0x14, 0x3C, 0x60, 0x80, 0x00 };

        void *Addr = Address;
        void *Addr_end = Address+Size;

        while(Addr <= Addr_end-sizeof(SearchPattern))
        {
                if(memcmp(Addr, SearchPattern, sizeof(SearchPattern))==0)
                {
                        memcpy(Addr,PatchData,sizeof(PatchData));
                }
                Addr += 4;
        }
}

bool Remove_002_Protection(void *Address, int Size)
{
	unsigned int SearchPattern[3]	= { 0x2C000000, 0x40820214, 0x3C608000 };
	unsigned int PatchData[3]		= { 0x2C000000, 0x48000214, 0x3C608000 };
	unsigned int *Addr				= (unsigned int*)Address;

	while (Size >= 12)
	{
		if (Addr[0] == SearchPattern[0] && Addr[1] == SearchPattern[1] && Addr[2] == SearchPattern[2])
		{
			*Addr = PatchData[0];
			Addr += 1;
			*Addr = PatchData[1];
			Addr += 1;
			*Addr = PatchData[2];
			return true;
		}

		Addr += 1;
		Size -= 4;
	}

	return false;
} 

int load_disc(char *discid, int fix)
{
        static struct DiscHeader Header ALIGNED(0x20);
        static struct Partition_Descriptor Descriptor ALIGNED(0x20);
        static struct Partition_Info Partition_Info ALIGNED(0x20);
        int i;

        memset(&Header, 0, sizeof(Header));
        memset(&Descriptor, 0, sizeof(Descriptor));
        memset(&Partition_Info, 0, sizeof(Partition_Info));

        if (debugEnabled) printf("Loading disc %s\n",discid);

        DI_Initialize();
        DI_Enable_WBFS(discid);
        DI_Reset();
        memset(Disc_ID, 0, 0x20);
        DI_Read_DiscID(Disc_ID);

        if (*Disc_ID==0x10001 || *Disc_ID==0x10001)
                return 2;
        
	  if (config[7]=='1') {
		Determine_VideoMode(config[8], true);
	  } else {
	      Determine_VideoMode(*Disc_Region, true );
	  }

        DI_Read_Unencrypted(&Header, sizeof(Header), 0);

        if (debugEnabled) printf("%s\n",Header.Title);

        u32 Offset = 0x00040000; // Offset into disc to partition descriptor
        DI_Read_Unencrypted(&Descriptor, sizeof(Descriptor), Offset);

        Offset = Descriptor.Primary_Offset << 2;

        u32 PartSize = sizeof(Partition_Info);
        u32 BufferLen = Descriptor.Primary_Count * PartSize;
        
        // Length must be multiple of 0x20
        BufferLen += 0x20 - (BufferLen % 0x20);
        u8 *PartBuffer = (u8*)memalign(0x20, BufferLen);

        memset(PartBuffer, 0, BufferLen);
        DI_Read_Unencrypted(PartBuffer, BufferLen, Offset);

        struct Partition_Info *Partitions = (struct Partition_Info*)PartBuffer;
        for ( i = 0; i < Descriptor.Primary_Count; i++)
        {
                if (Partitions[i].Type == 0)
                {
                        memcpy(&Partition_Info, PartBuffer + (i * PartSize), PartSize);
                        break;
                }
        }
        Offset = Partition_Info.Offset << 2;
        free(PartBuffer);
        if (!Offset)
                return 3;
        DI_Set_OffsetBase(Offset);
        Offset = 0;
          
        signed_blob* Certs		= 0;
        signed_blob* Ticket		= 0;
        signed_blob* Tmd		= 0;
        
        unsigned int C_Length	= 0;
        unsigned int T_Length	= 0;
        unsigned int MD_Length	= 0;
        
        static u8	Ticket_Buffer[0x800] ALIGNED(32);
        static u8	Tmd_Buffer[0x49e4] ALIGNED(32);
        
        GetCerts(&Certs, &C_Length);
        DI_Read_Unencrypted(Ticket_Buffer, 0x800, Partition_Info.Offset << 2);
        Ticket		= (signed_blob*)(Ticket_Buffer);
        T_Length	= SIGNED_TIK_SIZE(Ticket);

        // Open Partition and get the TMD buffer
        if (DI_Open_Partition(Partition_Info.Offset, 0,0,0, Tmd_Buffer) < 0)
                return 4;
        Tmd = (signed_blob*)(Tmd_Buffer);
        MD_Length = SIGNED_TMD_SIZE(Tmd);
        static struct AppLoaderHeader Loader ALIGNED(32);
        DI_Read(&Loader, sizeof(Loader), 0x00002440);// Offset into the partition to apploader header
        DCFlushRange((void*)(((u32)&Loader) + 0x20),Loader.Size + Loader.Trailer_Size);


        // Read apploader from 0x2460
        DI_Read(Apploader, Loader.Size + Loader.Trailer_Size, 0x00002440 + 0x20);
        DCFlushRange((void*)(((int)&Loader) + 0x20),Loader.Size + Loader.Trailer_Size);


        AppLoaderStart	Start	= Loader.Entry_Point;
        AppLoaderEnter	Enter	= 0;
        AppLoaderLoad		Load	= 0;
        AppLoaderExit		Exit	= 0;
        Start(&Enter, &Load, &Exit);

        void*	Address = 0;
        int		Section_Size;
        int		Partition_Offset;
        if (debugEnabled) printf("Loading game");
        while (Load(&Address, &Section_Size, &Partition_Offset))
        {
                if (!Address) return 5;
                DI_Read(Address, Section_Size, Partition_Offset << 2);
                DCFlushRange(Address, Section_Size);
                if (debugEnabled) printf(".");
        }
        // Patch in info missing from apploader reads
        *Sys_Magic	= 0x0d15ea5e;
        *Version	= 1;
        *Arena_L	= 0x00000000;
        *Bus_Speed	= 0x0E7BE2C0;
        *CPU_Speed	= 0x2B73A840;

        // Enable online mode in games
        memcpy(Online_Check, Disc_ID, 4);
        
        // Retrieve application entry point
        void* Entry = Exit();

	    SetTime();

        // Set Video Mode based on Configuration
        Set_VideoMode(debugEnabled);


	if(fix){
		*(u32 *)0x80003140 = *(u32 *)0x80003188;
		DCFlushRange(0x80003140, 4); 
		if (fix & 1) 
		{
			*(u32 *)0x80003140 = *(u32 *)0x80003188;
		} 
		
		if (fix & 2) 
		{
			Remove_002_Protection((void*)0x80000000, 0x17fffff);
		} 
		
		if (fix & 4) 
		{
			Anti_002_fix((void*)0x80000000, 0x17fffff);
		}
	}

        // Flush application memory range
        DCFlushRange((void*)0x80000000, 0x17fffff);	// TODO: Remove these hardcoded values

        // Cleanup loader information
        DI_Close();

        // Identify as the game
        if (IS_VALID_SIGNATURE(Certs) 	&& IS_VALID_SIGNATURE(Tmd) 	&& IS_VALID_SIGNATURE(Ticket) 
            &&  C_Length > 0 				&& MD_Length > 0 			&& T_Length > 0)
        {
                int ret = ES_Identify(Certs, C_Length, Tmd, MD_Length, Ticket, T_Length, NULL);
                if (ret < 0)
                        return ret;
        }

        debug_printf("start %p\n",Entry);

        SYS_ResetSystem(SYS_SHUTDOWN, 0, 0);

        __asm__ __volatile__
                (
                        "mtlr %0;"			// Move the entry point into link register
                        "blr"				// Branch to address in link register
                        :					// No output registers
                        :	"r" (Entry)		// Input register
                                //:					// difference between C and cpp mode??
                        );
	return 0;
}

