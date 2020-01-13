#pragma once
#include "ImagePixels.h"
#include <opencv2/core.hpp>

DllExport int Lib_GetInt123() {
	return 123;
}

DllExport double Lib_GetAverageG(const ImagePixels& image) {
	const unsigned char* pixels = image.PixelsPtr;
	//int size = image.PixelsSize;
	int width = image.Width;
	int height = image.Height;
	int stride = image.Stride;
	int bytesPerPixel = image.BytesPerPixel;

	unsigned long long sum = 0;
	for (int y = 0; y < height; y++) {
		for (int x = 0; x < width; x++) {
			int n = (y * stride) + (x * bytesPerPixel);
			sum += (unsigned long long)(pixels[n + 1]);	//RGBA
		}
	}
	return (double)sum / (width * height);
}

DllExport void Lib_ToNegaPosi(ImagePixels& image) {
	unsigned char* head = image.PixelsPtr;
	unsigned char* tail = head + image.PixelsSize;

	// RGBA
	for (unsigned char* p = head; p < tail; p += image.BytesPerPixel) {
		*(p + 0) = 0xff - *(p + 0);
		*(p + 1) = 0xff - *(p + 1);
		*(p + 2) = 0xff - *(p + 2);
	}
}
