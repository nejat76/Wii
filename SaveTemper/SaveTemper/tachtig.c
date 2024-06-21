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
#include "savetools.h"

#define ERROR(s) do { fprintf(stderr, s "\n"); exit(1); } while (0)

static u8 sd_key[16];
static u8 sd_iv[16];
static u8 md5_blanker[16];

static FILE *fp;
static u32 n_files;
static u32 files_size;
static u32 total_size;
static u8 header[0xf0c0];

static int output_image(u8 *data, u32 w, u32 h, const char *name)
{
	FILE *fp;
	u32 x, y;

	fp = fopen(name, "wb");
	if (!fp) {
		fatal(-1,"open %s", name);
		return -1;
	}

	fprintf(fp, "P6 %d %d 255\n", w, h);

	for (y = 0; y < h; y++)
		for (x = 0; x < w; x++) {
			u8 pix[3];
			u16 raw;
			u32 x0, x1, y0, y1, off;

			x0 = x & 3;
			x1 = x >> 2;
			y0 = y & 3;
			y1 = y >> 2;
			off = x0 + 4 * y0 + 16 * x1 + 4 * w * y1;

			raw = be16(data + 2*off);

			// RGB5A3
			if (raw & 0x8000) {
				pix[0] = (raw >> 7) & 0xf8;
				pix[1] = (raw >> 2) & 0xf8;
				pix[2] = (raw << 3) & 0xf8;
			} else {
				pix[0] = (raw >> 4) & 0xf0;
				pix[1] =  raw       & 0xf0;
				pix[2] = (raw << 4) & 0xf0;
			}

			if (fwrite(pix, 3, 1, fp) != 1) {
				fatal(-1,"write %s", name);
				return -1;
			}
		}

	fclose(fp);
	return 0;
}

static int do_file_header(int useBannerBin)
{
	u8 md5_file[16];
	u8 md5_calc[16];
	u32 header_size;
	char name[256];
	char dir[17];
	FILE *out;
	u32 i;

	if (fread(header, sizeof header, 1, fp) != 1) {
		fatal(-1000, "read file header");
		return -1;
	}

	aes_cbc_dec(sd_key, sd_iv, header, sizeof header, header);

	memcpy(md5_file, header + 0x0e, 16);
	memcpy(header + 0x0e, md5_blanker, 16);
	md5(header, sizeof header, md5_calc);

	if (memcmp(md5_file, md5_calc, 0x10)) {
		fatal(-1010, "MD5 mismatch");
		return -1;
	}

	header_size = be32(header + 8);
	if (header_size < 0x72a0 || header_size > 0xf0a0
		|| (header_size - 0x60a0) % 0x1200 != 0) {
		fatal(-1020, "bad file header size");
		return -1;
	}

	snprintf(dir, sizeof dir, "%016llx", be64(header));
	if (mkdir(dir, 0777)) {
		fatal(-1,"mkdir %s", dir);
		return -1;
	}
	if (chdir(dir)) {
		fatal(-1,"chdir %s", dir);
		return -1;
	}

	if (!useBannerBin) 
	{
	out = fopen("###title###", "wb");
	if (!out) {
		fatal(-1100,"open ###title###");
		fclose(out);
		return -1;
	}
	if (fwrite(header + 0x40, 0x80, 1, out) != 1) {
		fatal(-1110, "write ###title###");
		fclose(out);
		return -1;
	}
	fclose(out);

	output_image(header + 0xc0, 192, 64, "###banner###.ppm");
	if (header_size == 0x72a0)
		output_image(header + 0x60c0, 48, 48, "###icon###.ppm");
	else
		for (i = 0; 0x1200*i + 0x60c0 < header_size; i++) {
			snprintf(name, sizeof name, "###icon%d###.ppm", i);
			output_image(header + 0x60c0 + 0x1200*i, 48, 48, name);
		}
	} else 
	{
		int bannerSize = header_size;
		
		FILE * fpb = fopen("banner.bin", "wb");
		if (!fpb) {
			fatal(-1,"can't open banner.bin to write");
			return -1;
		} else 
		{
			if (fwrite(header+0x20, 1, bannerSize, fpb)!=bannerSize) 
			{
				fatal(-1, "writing banner.bin has failed!");
				fclose(fpb);
				return -1;
			}
			fclose(fpb);
		}
	}
	return 0;
}

static int do_backup_header(void)
{
	u8 header[0x80];

	if (fread(header, sizeof header, 1, fp) != 1) {
		fatal(-1200, "read backup header");
		return -1;
	}

	if (be32(header + 4) != 0x426b0001) {
		fatal(-1300,"no Bk header");
		return -1;
	}
	if (be32(header) != 0x70) {
		fatal(-1400, "wrong Bk header size");
		return -1;
	}

	//this can be ignored or appended to the output
	fprintf(stderr, "NG id: %08x\n", be32(header + 8));

	n_files = be32(header + 0x0c);
	files_size = be32(header + 0x10);
	total_size = be32(header + 0x1c);
	//this can be ignored or appended to the output
	fprintf(stderr, "%d files\n", n_files);
	return 0;
}


static mode_t perm_to_mode(u8 perm)
{
	mode_t mode;
	u32 i;

	mode = 0;
	for (i = 0; i < 3; i++) {
		mode <<= 3;
		if (perm & 0x20)
			mode |= 3;
		if (perm & 0x10)
			mode |= 5;
		perm <<= 2;
	}

	return mode;
}

//OK
static int do_file(void)
{
	u8 header[0x80];
	u32 size;
	u32 rounded_size;
	u8 perm, attr, type;
	char *name;
	u8 *data;
	FILE *out;
	mode_t mode;

	if (fread(header, sizeof header, 1, fp) != 1) {
		fatal(-1,"read file header");
		return -1;
	}

	if (be32(header) != 0x03adf17e) {
		fatal(-1,"bad file header");
		return -1;
	}

	size = be32(header + 4);
	perm = header[8];
	attr = header[9];
	type = header[10];
	name = header + 11;

	//can be ignored or appended to the output
	fprintf(stderr, "file: size=%08x perm=%02x attr=%02x type=%02x name=%s\n", size, perm, attr, type, name);

	mode = perm_to_mode(perm);

	switch (type) {
	case 1:
		rounded_size = (size + 63) & ~63;
		data = malloc(rounded_size);
		if (!data) {
			fatal(-1,"malloc");
			return -1;
		}
		if (fread(data, rounded_size, 1, fp) != 1) {
			fatal(-1,"read file data for %s", name);
			return -1;
		}

		aes_cbc_dec(sd_key, header + 0x50, data, rounded_size, data);

		out = fopen(name, "wb");
		if (!out) {
			fatal(-1,"open %s", name);
			return -1;
		}
		if (fwrite(data, size, 1, out) != 1) {
			fatal(-1,"write %s", name);
			return -1;
		}
		fclose(out);

		mode &= ~0111;

		free(data);
		break;

	case 2:
		if (mkdir(name, 0777)) {
			fatal(-1,"mkdir %s", name);
			return -1;
		}
		break;

	default:
		fatal(-1,"unhandled file type");
		return -1;
	}

	if (chmod(name, mode)) {
		fatal(-1,"chmod %s", name);
		return -1;
	}
	return 0;
}

//OK
static int do_sig(void)
{
	u8 sig[0x40];
	u8 ng_cert[0x180];
	u8 ap_cert[0x180];
	u8 hash[0x14];
	u8 *data;
	u32 data_size;
	int ok;

	if (fread(sig, sizeof sig, 1, fp) != 1) {
		fatal(-1,"read signature");
		return -1;
	}
	if (fread(ng_cert, sizeof ng_cert, 1, fp) != 1) {
		fatal(-1,"read NG cert");
		return -1;
	}
	if (fread(ap_cert, sizeof ap_cert, 1, fp) != 1) {
		fatal(-1,"read AP cert");
		return -1;
	}

	data_size = total_size - 0x340;

	data = malloc(data_size);
	if (!data) {
		fatal(-1,"malloc");
		return -1;
	}
	fseek(fp, 0xf0c0, SEEK_SET);
	if (fread(data, data_size, 1, fp) != 1) {
		fatal(-1,"read data for sig check");
		return -1;
	}
	sha(data, data_size, hash);
	sha(hash, 20, hash);
	free(data);

	ok = check_ec(ng_cert, ap_cert, sig, hash);
	//can be appended to the output or ignored
	printf("ok: %d\n", ok);
	return 0;
}

extern int tachtig(int useBannerBin,char * app_folder, char * dataFile)
{
	u32 i;
	u32 mode;

	//if (argc != 2) {
	//	fprintf(stderr, "Usage: %s <data.bin>\n", argv[0]);
	//	return 1;
	//}
	
	if(get_key(app_folder, "shared","sd-key", sd_key, 16)) { return -1;}
	if(get_key(app_folder, "shared","sd-iv", sd_iv, 16)) { return -1;}
	if(get_key(app_folder, "shared","md5-blanker", md5_blanker, 16)) { return -1;}

	fp = fopen(dataFile, "rb");
	if (!fp)
		fatal("open %s", dataFile);

	if (do_file_header(useBannerBin)) { fclose(fp); return -1; }
	if(do_backup_header()) {fclose(fp); return -1;}

	for (i = 0; i < n_files; i++)
		if (do_file()) { fclose(fp);return -1; }

	mode = perm_to_mode(header[0x0c]);
	if (chmod(".", mode)) {
		fatal(-1, "chmod .");
		fclose(fp);
		return -1;
	}
	if (chdir("..")) {
		fatal(-1,"chdir ..");
		fclose(fp);
		return -1;
	}

	if (do_sig()) {
		fclose(fp);
		return -1;
	}

	fclose(fp);

	return 0;
}
