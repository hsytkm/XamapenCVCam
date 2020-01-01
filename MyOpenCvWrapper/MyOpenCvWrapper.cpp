#include "MyOpenCvWrapper.h"

MyOpenCvWrapper::MyOpenCvWrapper()
{
}

MyOpenCvWrapper::~MyOpenCvWrapper()
{
}



#define DllExport extern "C"

#if 1
DllExport double GetImageAverage() {
    return 12.34;
}
#else
DllExport double GetImageAverage() {
    cv::Mat img1 = cv::Mat::zeros(500, 500, CV_8UC1);

    return cv::mean(img1)[0];
}
#endif

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