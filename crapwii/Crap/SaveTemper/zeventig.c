// Copyright 2007,2008  Segher Boessenkool  <segher@kernel.crashing.org>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt

#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>

#include "tools.h"

#define ERROR(s) do { fprintf(stdout, s "\n"); exit(1); } while (0)

static FILE *fp;

static u8 *get_wad(u32 len)
{
	u32 rounded_len;
	u8 *p;

	rounded_len = round_up(len, 0x40);
	p = malloc(rounded_len);
	if (p == 0) {
		fatal(-1, "Error malloc for file content");
		return NULL;
	}
	if (len)
		if (fread(p, rounded_len, 1, fp) != 1) {
			fatal(-1,"Content read, len = %x", len);
			return NULL;
		}
	return p;
}

static int do_app_file(char * appPath, u8 *app, u32 app_len, u8 *tik, u8 *tmd)
{
	u8 title_key[16];
	u8 iv[16];
	u32 i;
	u8 *p;
	u32 len;
	u32 rounded_len;
	u32 num_contents;
	u32 cid;
	u16 index;
	u16 type;
	char name[17];
	FILE *fp;

	if (decrypt_title_key(appPath, tik, title_key)!=0) 
	{
		return -1;
	}

	sprintf(name, "%016llx", be64(tmd + 0x018c));
	if (mkdir(name, 0777)) {
		set_wadlib_titleid(name);
		//fatal(-1,"mkdir %s", name);
		//return -1;
	}

	if (chdir(name)) {
		fatal("chdir %s", name);
		return -1;
	}

	num_contents = be16(tmd + 0x01de);
	p = app;

	for (i = 0; i < num_contents; i++) {
		cid = be32(tmd + 0x01e4 + 0x24*i);
		index = be16(tmd + 0x01e8 + 0x24*i);
		type = be16(tmd + 0x01ea + 0x24*i);
		len = be64(tmd + 0x01ec + 0x24*i);
		rounded_len = round_up(len, 0x40);
		fprintf(stdout, "--- cid=%08x index=%04x type=%04x len=%08x\n",
		        cid, index, type, len);

		memset(iv, 0, sizeof iv);
		memcpy(iv, tmd + 0x01e8 + 0x24*i, 2);
		aes_cbc_dec(title_key, iv, p, rounded_len, p);

		sprintf(name, "%08x.app", index);
		fp = fopen(name, "wb");
		if (fp == 0) {
			fatal("open %s", name);
			return -1;
		}
		if (fwrite(p, len, 1, fp) != 1) {
			fatal("write %s", name);
			return -1;
		}
		fclose(fp);

		p += rounded_len;
	}

	if (chdir("..")) {
		fatal(-1, "chdir ..");
		return -1;
	}

	return 0;
}


static int do_install_wad(char * appPath, u8 *header)
{
	FILE * cf;
	u32 header_len;
	u32 cert_len;
	u32 tik_len;
	u32 tmd_len;
	u32 app_len;
	u32 trailer_len;
	u8 *cert;
	u8 *tik;
	u8 *tmd;
	u8 *app;
	u8 *trailer;
	u32 ret;
	char name[30];
	char buf[32];

	header_len = be32(header);
	if (header_len != 0x20) {
		fatal(-1, "bad install header length (%x)", header_len);
		return -1;
	}

	cert_len = be32(header + 8);
	// 0 = be32(header + 0x0c);
	tik_len = be32(header + 0x10);
	tmd_len = be32(header + 0x14);
	app_len = be32(header + 0x18);
	trailer_len = be32(header + 0x1c);

	cert = get_wad(cert_len); if (!cert) {return -1; };
	tik = get_wad(tik_len);if (!tik) {return -1; };
	tmd = get_wad(tmd_len);if (!tmd) {return -1; };	
	//fread(buf, 32, 1, fp);	
	app = get_wad(app_len);if (!app) {return -1; };
	hexdump(app, 128);
	trailer = get_wad(trailer_len);if (!trailer) {return -1; };
	hexdump(trailer, 128);
	
	// File Dump
	// Create/Select Folder
	sprintf(name, "%016llx", be64(tmd + 0x018c));
	if (mkdir(name, 0777)) {
		//fatal(-1, "mkdir %s", name);
		//return -1;
	}
	if (chdir(name)) {
		fatal("chdir %s", name);
		return -1;
	}
	// File Dump
	sprintf(name, "%016llx.cert", be64(tmd + 0x018c));
	cf = fopen(name, "wb");
	fwrite(cert, cert_len, 1, cf);
	fclose(cf);
	
	if (trailer_len>0) {
	sprintf(name, "%016llx.trailer", be64(tmd + 0x018c));
	cf = fopen(name, "wb");
	fwrite(trailer, trailer_len, 1, cf);
	fclose(cf);
	}
	sprintf(name, "%016llx.tmd", be64(tmd + 0x018c));
	cf = fopen(name, "wb");
	fwrite(tmd, tmd_len, 1, cf);
	fclose(cf);

	sprintf(name, "%016llx.tik", be64(tmd + 0x018c));
	cf = fopen(name, "wb");
	fwrite(tik, tik_len, 1, cf);
	fclose(cf);
	
	fprintf(stdout, "ticket:\n");
	hexdump(tik, tik_len);
	fprintf(stdout, "tmd:\n");
	dump_tmd(tmd);
	fprintf(stdout, "cert chain:\n");
	hexdump(cert, cert_len);

	/*printf("Normal sign check...\n");
	ret = check_cert_chain(tik, tik_len, cert, cert_len);
	if (ret)
		fprintf(stdout, "ticket cert failure (%d)\n", ret);

	ret = check_cert_chain(tmd, tmd_len, cert, cert_len);
	if (ret)
		fprintf(stdout, "tmd cert failure (%d)\n", ret);
	printf("Trucha sign check...\n");
	ret = check_cert_chain_trucha(tik, tik_len, cert, cert_len);
	if (ret)
		fprintf(stdout, "ticket cert failure (%d)\n", ret);

	ret = check_cert_chain_trucha(tmd, tmd_len, cert, cert_len);
	if (ret)
		fprintf(stdout, "tmd cert failure (%d)\n", ret);*/
	
	
	if (chdir("..")) {
		fatal("chdir ..", name);
		return -1;
	}

	if (do_app_file(appPath, app, app_len, tik, tmd)!=0) 
	{
		return -1;
	}

	return 0;
}


/*
static void do_install_wad(u8 *header)
{
	u32 header_len;
	u32 cert_len;
	u32 tik_len;
	u32 tmd_len;
	u32 app_len;
	u32 trailer_len;
	u8 *cert;
	u8 *tik;
	u8 *tmd;
	u8 *app;
	u8 *trailer;
	u32 ret;

	header_len = be32(header);
	if (header_len != 0x20)
		die("bad install header length (%x)", header_len);

	cert_len = be32(header + 8);
	// 0 = be32(header + 0x0c);
	tik_len = be32(header + 0x10);
	tmd_len = be32(header + 0x14);
	app_len = be32(header + 0x18);
	trailer_len = be32(header + 0x1c);

	cert = get_wad(cert_len);
	tik = get_wad(tik_len);
	tmd = get_wad(tmd_len);
	app = get_wad(app_len);
	trailer = get_wad(trailer_len);

	fprintf(stdout, "ticket:\n");
	hexdump(tik, tik_len);
	fprintf(stdout, "tmd:\n");
	dump_tmd(tmd);
	fprintf(stdout, "cert chain:\n");
	hexdump(cert, cert_len);

	ret = check_cert_chain(tik, tik_len, cert, cert_len);
	if (ret)
		fprintf(stdout, "ticket cert failure (%d)\n", ret);

	ret = check_cert_chain(tmd, tmd_len, cert, cert_len);
	if (ret)
		fprintf(stdout, "tmd cert failure (%d)\n", ret);

	do_app_file(app, app_len, tik, tmd);
}
*/

static int do_wad(char * appPath)
{
	u8 header[0x80];
	u32 header_len;
	u32 header_type;

	if (fread(header, 0x40, 1, fp) != 1) {
		if (!feof(fp))
		{
			fatal(-1, "Error when reading wad header!");
			return -1;
		}
		else {
			return 0;
		}
	}
	header_len = be32(header);
	if (header_len >= 0x80) {
		fatal(-1, "Wad header too big!");
		return -1;
	}
	if (header_len >= 0x40) {
		if (fread(header + 0x40, 0x40, 1, fp) != 1) {
			fatal(-1, "reading wad header (2)");
			return -1;
		}
	}

	fprintf(stdout, "wad header:\n");
	hexdump(header, header_len);

	header_type = be32(header + 4);
	switch (header_type) {
	case 0x49730000:
		if (do_install_wad(appPath, header) != 0) 
		{
			return -1;
		}
		break;
	case 0x69620000:
		if (do_install_wad(appPath, header) !=0) 
		{
			return -1;
		}
		break;
	default:
		fatal(-1, "unknown header type %08x", header_type);
		return -1;
	}

	return 0;
}

int extractwad(char * extractionfolder, char * app_folder, char * wadFileName)
{
	chdir(extractionfolder);
	fp = fopen(wadFileName, "rb");

	if (!fp) {
		fatal(-1, "Can't open %s!", wadFileName);
		return -1;
	}

	while (!feof(fp)) {
		if (do_wad(app_folder)!=0) 
		{
			return -1;
		}
	}

	fclose(fp);

	return 0;
}
