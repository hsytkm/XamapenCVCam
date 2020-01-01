#include "pch.h"
//#include <opencv2/core.hpp>
//#include <opencv2/highgui.hpp>
//
//#pragma comment( lib, "libopencv_core.a" )
//#pragma comment( lib, "libopencv_highgui.a" )

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