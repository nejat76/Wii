#ifndef __CRAPMODE__
#include <ogcsys.h>

#include "libpng/pngu/pngu.h"
#include "background_png.h"

extern u32 *xfb;


void Video_DrawPng(IMGCTX ctx, PNGUPROP imgProp, u16 x, u16 y, GXRModeObj *vmode)
{
	PNGU_DECODE_TO_COORDS_YCbYCr(ctx, x, y, imgProp.imgWidth, imgProp.imgHeight, vmode->fbWidth, vmode->xfbHeight, xfb);
}


s32 DrawPng(void *img, u32 x, u32 y, GXRModeObj *vmode)
{
	IMGCTX   ctx = NULL;
	PNGUPROP imgProp;

	s32 ret;

	/* Select PNG data */
	ctx = PNGU_SelectImageFromBuffer(img);
	if (!ctx) {
		ret = -1;
		goto out;
	}

	/* Get image properties */
	ret = PNGU_GetImageProperties(ctx, &imgProp);
	if (ret != PNGU_OK) {
		ret = -1;
		goto out;
	}

	/* Draw image */
	Video_DrawPng(ctx, imgProp, x, y, vmode);

	/* Success */
	ret = 0;

out:
	/* Free memory */
	if (ctx)
		PNGU_ReleaseImageContext(ctx);

	return ret;
}

void DrawBackground(GXRModeObj *vmode)
{
	/* Draw background */
	DrawPng((u8 *)background_png, 0, 0, vmode);
} 
#endif