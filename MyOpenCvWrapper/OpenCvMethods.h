#pragma once
#include "ImagePixels.h"
#include <opencv2/core.hpp>

cv::Mat ToMat(const ImagePixels& pixels) {
	int srcRows = pixels.Height;
	int srcCols = pixels.Width;
	int srcChannels = pixels.BytesPerPixel;
	int srcStride = pixels.Stride;
	const unsigned char* srcPixels = pixels.PixelsPtr;

	cv::Mat dst(srcRows, srcCols, CV_8UC4);

	int i = 0;
	for (int y = 0; y < srcRows; ++y) {
		for (int x = 0; x < srcCols; ++x) {
			for (int c = 0; c < srcChannels; ++c) {
				int n = (y * srcStride) + (x * srcChannels) + c;
				dst.data[i++] = srcPixels[n];
			}
		}
	}
	return dst;
}

void UpdateMatPixels(const cv::Mat& src, unsigned char* dstHead, int dstSize) {
	unsigned char* dst = dstHead;
	int srcBytesPerPixel = static_cast<int>(src.elemSize());

	// �������ݐ�̃T�C�Y�`�F�b�N(�O�̂���)
	int srcSize = src.rows * src.cols * srcBytesPerPixel;
	if (srcSize > dstSize) return;

	for (int y = 0; y < src.rows; ++y) {
		for (int x = 0; x < src.cols; ++x) {
			for (int c = 0; c < srcBytesPerPixel; ++c) {
				int n = (y * src.step) + (x * srcBytesPerPixel) + c;
				*(dst++) = src.data[n];
			}
		}
	}
}

DllExport void OpenCv_ToNegaPosi(ImagePixels& pixels) {
	// Mat�`���ɕϊ�
	cv::Mat mat = ToMat(pixels);

	// �l�K�|�W���]
	mat = ~mat;

	// ���̓������ɉ�f�l�������߂�
	UpdateMatPixels(mat, pixels.PixelsPtr, pixels.PixelsSize);
}
