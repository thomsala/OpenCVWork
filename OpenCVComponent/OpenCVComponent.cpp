// OpenCVComponent.cpp
#include "pch.h"
#include "OpenCVComponent.h"

#include <opencv2\imgproc\types_c.h>
#include <opencv2\core\core.hpp>
#include <opencv2\imgproc\imgproc.hpp>
#include <vector>
#include <algorithm>

using namespace OpenCVComponent;
using namespace Platform;
using namespace concurrency;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;

void CopyIVectorToMatrix(IVector<int>^ input, cv::Mat& mat, int size);
void CopyMatrixToVector(const cv::Mat& mat, std::vector<int>& vector, int size);

int iLowH = 0;
int iHighH = 179;

int iLowS = 0;
int iHighS = 255;

int iLowV = 0;
int iHighV = 255;
CvScalar lowScalar = CvScalar(iLowH, iLowS, iLowV);

OpenCVLib::OpenCVLib()
{
}

IAsyncOperation<IVectorView<int>^>^ OpenCVLib::ProcessAsync(IVector<int>^ input, int width, int height)
{
    int size = input->Size;
    cv::Mat mat(width, height, CV_8UC4);
    CopyIVectorToMatrix(input, mat, size);

    return create_async([=]() -> IVectorView<int>^
    {
        // convert to grayscale
        cv::Mat intermediateMat;
        cv::cvtColor(mat, intermediateMat, CV_RGB2GRAY);

        // convert to BGRA
        cv::cvtColor(intermediateMat, mat, CV_GRAY2BGRA);

        std::vector<int> output;
        CopyMatrixToVector(mat, output, size);

        // Return the outputs as a VectorView<float>
        return ref new Platform::Collections::VectorView<int>(output);
    });
}


IAsyncOperation<IVectorView<int>^>^ OpenCVLib::FindRedAsync(IVector<int>^ input, int width, int height)
{
	int size = input->Size;
	cv::Mat mat(width, height, CV_8UC4);
	CopyIVectorToMatrix(input, mat, size);

	return create_async([=]() -> IVectorView<int>^
	{


		
		OutputDebugStringA("My output string.");
		cv::Mat intermediateMat;
		cv::cvtColor(mat, intermediateMat, CV_BGR2HSV);
		

		//Threshold the image
		cv::Mat imgThresholded;
		cv::Mat matLow = (cv::Mat_<double>(1, 3) << 160, 50,50);
		cv::Mat matHigh = (cv::Mat_<double>(1, 3) << 180, 255, 255);

		cv::inRange(intermediateMat, matLow, matHigh, imgThresholded);
	
		//morphological opening (remove small objects from the foreground)
		erode(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));
		dilate(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));

		//morphological closing (fill small holes in the foreground)
		dilate(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));
		erode(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));


		// convert to BGRA
		cv::cvtColor(imgThresholded, mat, CV_GRAY2BGRA);
		std::vector<int> output;
		CopyMatrixToVector(mat, output, size);

		// Return the outputs as a VectorView<float>
		return ref new Platform::Collections::VectorView<int>(output);
	});
}

IAsyncOperation<IVector<int>^>^ OpenCVLib::FindPositionAsync(IVector<int>^ input, int width, int height)
{
	int size = input->Size;
	cv::Mat mat(width, height, CV_8UC4);
	CopyIVectorToMatrix(input, mat, size);

	return create_async([=]() -> IVector<int>^
	{


		
		OutputDebugStringA("My output string.");
		cv::Mat intermediateMat;
		cv::cvtColor(mat, intermediateMat, CV_BGR2HSV);


		//Threshold the image
		cv::Mat imgThresholded;
		cv::Mat matLow = (cv::Mat_<double>(1, 3) << 160, 50, 50);
		cv::Mat matHigh = (cv::Mat_<double>(1, 3) << 180, 255, 255);

		cv::inRange(intermediateMat, matLow, matHigh, imgThresholded);

		//morphological opening (remove small objects from the foreground)
		erode(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));
		dilate(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));

		//morphological closing (fill small holes in the foreground)
		dilate(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));
		erode(imgThresholded, imgThresholded, cv::getStructuringElement(2, cv::Size(5, 5)));

		
		int x, y;
		int sommeX = 0, sommeY = 0;
		int nbPixels = 0;
		// We go through the mask to look for the tracked object and get its gravity center
		for (x = 0; x < imgThresholded.cols; x++) {
			for (y = 0; y < imgThresholded.rows; y++) {

				// If its a tracked pixel, count it to the center of gravity's calcul
				if ( imgThresholded.at<uchar>(x,y) == 255) {
					sommeX += x;
					sommeY += y;
					nbPixels++;
				}
			}
		}
		

		int vv[2] = { -1, -1 };
		// If there is no pixel, we return a center outside the image, else we return the center of gravity
		if (nbPixels > 0)
		{
			vv[0] = (int)(sommeX / (nbPixels));
			vv[1] = (int)(sommeY / (nbPixels));
		}
		
		std::vector<int> output(&vv[0], &vv[0] + 2);

		

		return ref new Platform::Collections::Vector<int>(output);
	});
		
}


void CopyIVectorToMatrix(IVector<int>^ input, cv::Mat& mat, int size)
{
    unsigned char* data = mat.data;
    for (int i = 0; i < size; i++)
    {
        int value = input->GetAt(i);
        memcpy(data, (void*) &value, 4);
        data += 4;
    }
}

void CopyMatrixToVector(const cv::Mat& mat, std::vector<int>& vector, int size)
{
    int* data = (int*) mat.data;
    for (int i = 0; i < size; i++)
    {
        vector.push_back(data[i]);
    }

}


