#pragma once

struct ImagePixels
{
	unsigned char* PixelsPtr;
	int PixelsSize;
	int Width;
	int Height;
	int BytesPerPixel;
	int Stride;
};
