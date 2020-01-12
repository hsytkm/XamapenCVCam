#include "OpenCvTest.h"

#include <opencv2/core.hpp>

DllExport double GetMatMeanY(int black_length, int white_length) {
	int row = 100;

	// 1. 引数で指定された割合で、単色の黒/白 Mat を作成
	cv::Mat brack = cv::Mat::zeros(row, black_length, CV_8UC1);
	cv::Mat white(row, white_length, CV_8UC1, cv::Scalar(255, 255, 255));

	// 2. サイドバイサイドで 2つのMat を結合
	cv::Mat merge;
	hconcat(brack, white, merge);

	// 3. 結合したMatの平均輝度値を求める（単色なので輝度(Y)と呼べない気もする）
	return cv::mean(merge)[0];
}

DllExport int GetMyInt() {
	return 321;
}

bool SetStringToCharArraySub(char* dest, int destLength, const char* src, int srcLength) {
	if (destLength < 1) return true;

	bool err;
	int copyLength, nullIndex;

	if (destLength + 1 >= srcLength) {
		// 全部収まるパターン
		err = false;
		copyLength = srcLength;
		nullIndex = srcLength;
	}
	else {
		// 全部収まらないパターン
		err = true;
		copyLength = destLength - 1;
		nullIndex = destLength - 1;
	}

	for (int i = 0; i < copyLength; i++) {
		dest[i] = src[i];
	}
	dest[nullIndex] = '\0';
	return err;
}

bool SetStringToCharArray(char* dest, int destLength, const char* src) {
	return SetStringToCharArraySub(dest, destLength, src, (int)strlen(src));
}

// string(Out) 予約文字列
DllExport bool GetString(char* dest, int destLength) {
	return SetStringToCharArray(dest, destLength, "Hello, I'm Library!");
}




















struct ImagePixels
{
	unsigned char* PixelsPtr;
	int PixelsSize;
	int Width;
	int Height;
	int BytesPerPixel;
	int Stride;
};

DllExport double OpenCv_GetAverageG(const ImagePixels& image) {
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

DllExport void OpenCv_ToNegaPosi(ImagePixels& image) {
	unsigned char* head = image.PixelsPtr;
	unsigned char* tail = head + image.PixelsSize;

	// RGBA
	for (unsigned char* p = head; p < tail; p += image.BytesPerPixel) {
		*(p + 0) = 0xff - *(p + 0);
		*(p + 1) = 0xff - *(p + 1);
		*(p + 2) = 0xff - *(p + 2);
	}
}
