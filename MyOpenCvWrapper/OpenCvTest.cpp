#include "OpenCvTest.h"

#include <opencv2/core.hpp>

DllExport double GetMatMeanY(int black_length, int white_length) {
	int row = 100;

	// 1. �����Ŏw�肳�ꂽ�����ŁA�P�F�̍�/�� Mat ���쐬
	cv::Mat brack = cv::Mat::zeros(row, black_length, CV_8UC1);
	cv::Mat white(row, white_length, CV_8UC1, cv::Scalar(255, 255, 255));

	// 2. �T�C�h�o�C�T�C�h�� 2��Mat ������
	cv::Mat merge;
	hconcat(brack, white, merge);

	// 3. ��������Mat�̕��ϋP�x�l�����߂�i�P�F�Ȃ̂ŋP�x(Y)�ƌĂׂȂ��C������j
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
		// �S�����܂�p�^�[��
		err = false;
		copyLength = srcLength;
		nullIndex = srcLength;
	}
	else {
		// �S�����܂�Ȃ��p�^�[��
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

// string(Out) �\�񕶎���
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

DllExport double GetAverageG(ImagePixels *image) {
	unsigned char* pixels = image->PixelsPtr;
	int size = image->PixelsSize;

	int width = image->Width;
	int height = image->Height;
	int stride = image->Stride;
	int bytesPerPixel = image->BytesPerPixel;

	unsigned long long sum = 0;
	for (int y = 0; y < height; y++) {
		for (int x = 0; x < width; x++) {
			int n = (height * stride) + (width * bytesPerPixel);
			sum += pixels[n + 2];
		}
	}
	double ave = (double)(sum) / (width * height);

	return ave;
}

