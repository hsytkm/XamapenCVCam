#pragma once
#include "ImagePixels.h"

#include <string>
#include <vector>

#include <opencv2/core.hpp>
#include <opencv2/objdetect.hpp>
#include <opencv2/imgproc.hpp>

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

	// 書き込み先のサイズチェック(念のため)
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
	// Mat形式に変換
	cv::Mat mat = ToMat(pixels);

	// ネガポジ反転
	mat = ~mat;

	// 入力メモリに画素値を書き戻す
	UpdateMatPixels(mat, pixels.PixelsPtr, pixels.PixelsSize);
}

// 原画サイズのまま顔検出
int OpenCv_DrawFaceFramesFullSize(ImagePixels& pixels, std::string path) {
	cv::Mat mat = ToMat(pixels);

	cv::CascadeClassifier cascade;
	cascade.load(path);		// "haarcascade_frontalface_alt.xml"

	std::vector<cv::Rect> faces;
	cascade.detectMultiScale(mat, faces, 1.1, 3, 0, cv::Size(20, 20));

	int faceNum = static_cast<int>(faces.size());
	for (int i = 0; i < faceNum; i++)
	{
		cv::rectangle(mat,
			cv::Point(faces[i].x, faces[i].y),
			cv::Point(faces[i].x + faces[i].width, faces[i].y + faces[i].height),
			cv::Scalar(0, 255, 0), 20);
	}

	// 入力メモリに画素値を書き戻す
	UpdateMatPixels(mat, pixels.PixelsPtr, pixels.PixelsSize);

	return faceNum;
}

// 原画を縮小してから顔検出
int OpenCv_DrawFaceFramesResize(ImagePixels& pixels, std::string path) {
	cv::Mat src = ToMat(pixels);
	cv::Mat tmp;

	cv::CascadeClassifier cascade;
	cascade.load(path);		// "haarcascade_frontalface_alt.xml"

	const int normalizeLengthMax = 320;
	const int lengthMax = (std::max)({ src.rows, src.cols });
	const int tickness = lengthMax / 100;
	double zoomRatio;

	// 元画像をリサイズしてから顔検出する
	if (normalizeLengthMax < lengthMax) {
		zoomRatio = static_cast<double>(normalizeLengthMax) / lengthMax;
		cv::resize(src, tmp, cv::Size(), zoomRatio, zoomRatio);
	}
	else {
		zoomRatio = 1.0;
		tmp = src.clone();
	}

	std::vector<cv::Rect> facesSource;
	cascade.detectMultiScale(tmp, facesSource, 1.1, 3, 0, cv::Size(20, 20));

	int faceNum = static_cast<int>(facesSource.size());
	for (int i = 0; i < faceNum; i++) {
		int x = static_cast<int>(std::round(facesSource[i].x / zoomRatio));
		int y = static_cast<int>(std::round(facesSource[i].y / zoomRatio));
		int width = static_cast<int>(std::round(facesSource[i].width / zoomRatio));
		int height = static_cast<int>(std::round(facesSource[i].height / zoomRatio));

		cv::rectangle(src,
			cv::Point(x, y),
			cv::Point(x + width, y + height),
			cv::Scalar(0, 255, 0), 20);
	}

	// 入力メモリに画素値を書き戻す
	UpdateMatPixels(src, pixels.PixelsPtr, pixels.PixelsSize);

	return faceNum;
}

DllExport int OpenCv_DrawFaceFrames(ImagePixels& pixels, const char* c_path) {
	std::string path(c_path);

	//return OpenCv_DrawFaceFramesFullSize(pixels, path);
	return OpenCv_DrawFaceFramesResize(pixels, path);
}
