// 기본 DLL 파일입니다.

#include "stdafx.h"

#include "CalcLib.h"

int ConvertTemp(int pixel)
{
	int DL[] = {8667, 8878, 9095, 9318, 9548,
		9784, 10027, 10276, 10533, 10797,
		11068, 11346, 11632, 11926, 12227,
		12537, 12854, 13180, 13514, 13857,
		14208, 14569, 14938, 15317, 15705,
		16103};
	int val[] = { 205, 211, 217, 223, 230, 
		236, 243, 249, 257, 264, 
		271, 278, 286, 294, 301, 
		310, 317, 326, 334, 343, 
		351, 361, 369, 379, 388, 
		398 };

	for(int i = 0; i < 26; i++)
	{
		if(pixel < DL[i])
		{
			int sub = 0;

			if(i == 0)
				sub = pixel - 8462;
			else
				sub = pixel - DL[i - 1];

			return (int)((i + 18 + sub / (double)val[i]) * 100);
		}
	}

	return 0;
}

void setIntZeros(int *array, int size)
{
	for(int i=0; i<size; i++)
		array[i] = 0;
}
void setShortZeros(short *array, int size)
{
	for(int i=0; i<size; i++)
		array[i] = 0;
}

void CalcurateMinMax(int *img, int *sum, int *pmax, int *pmin, int size, int *min, int *max, int bit)
{
	//병렬처리를 통해 속도 개선(OPEN MP)
	int minVal = 65535;
	int maxVal = 0;

	unsigned char* buf = (unsigned char*)img;

	unsigned short* tMax = (unsigned short*)pmax;
	unsigned short* tMin = (unsigned short*)pmin;

	int bitSize = bit / 8;		//16bit인지 8bit인지 확인

	#pragma omp parallel for
	for(int i = 0; i < size / bitSize; i++)
	{
		unsigned int pixel = 0;
		
		if(bitSize == 1)
			pixel = buf[i];
		else if(bitSize == 2)
		{
			pixel = (buf[i * 2]) + (buf[i * 2 + 1] << 8);
		}

		pixel = ConvertTemp(pixel);

		sum[i] += pixel;

		if(tMax[i] == 0) tMax[i] = pixel;
		if(tMin[i] == 0) tMin[i] = pixel;

		if(tMax[i] < pixel) tMax[i] = pixel;
		if(tMin[i] > pixel) tMin[i] = pixel;

		if (pixel < minVal)
		{
			#pragma omp critical (MIN)
			{
				if (pixel < minVal)
					minVal = pixel;
			}
		}
		else if (pixel > maxVal)
		{
			#pragma omp critical (MAX)
			{
				if (pixel > maxVal)
					maxVal = pixel;
			}
		}
	}

	*min = minVal;
	*max = maxVal;
}
void CalcurateMinMax_Double(int *img, int size, double *min, double *max)
{
	double minVal = 99999999;
	double maxVal = -99999999;

	double* buf = (double*)img;

//#pragma omp parallel for
	for(int i = 0; i < size; i++)
	{
		double pixel = buf[i];

		if (pixel < minVal)
		{
//#pragma omp critical (MIN)
			{
				if (pixel < minVal)
					minVal = pixel;
			}
		}
		else if (pixel > maxVal)
		{
//#pragma omp critical (MAX)
			{
				if (pixel > maxVal)
					maxVal = pixel;
			}
		}
	}

	*min = minVal;
	*max = maxVal;

}
void CalcurateMinMax2(int *img, int size, int *min, int *max)
{
	//병렬처리를 통해 속도 개선(OPEN MP)
	int minVal = 65535;
	int maxVal = 0;

	unsigned short* buf = (unsigned short*)img;

#pragma omp parallel for
	for(int i = 0; i < size; i++)
	{
		unsigned int pixel = buf[i];

		if (pixel < minVal)
		{
#pragma omp critical (MIN)
			{
				if (pixel < minVal)
					minVal = pixel;
			}
		}
		else if (pixel > maxVal)
		{
#pragma omp critical (MAX)
			{
				if (pixel > maxVal)
					maxVal = pixel;
			}
		}
	}

	*min = minVal;
	*max = maxVal;
}

void DrawImage(int *hBit, int *img, int width, int height, int min, int max, int span, int bit)
{
	//SIMD를 통한 속도 개선 필요
	//__m128i* dest = (__m128i*)(hBit);
	//__m128i xmmA;

	//int src[4];

	unsigned char* buf = (unsigned char*)img;

	int i = 0;
	int bitSize = bit / 8;		//16bit인지 8bit인지 확인
	for(int w = 0; w < width; w++)
	{
		for(int h = 0; h < height; h++)
		{
			unsigned int pixel = 0;

			if(bitSize == 1)
				pixel = buf[i];
			else if(bitSize == 2)
			{
				pixel = (buf[i * 2]) + (buf[i * 2 + 1] << 8);
			}

			pixel = ConvertTemp(pixel);

			byte pix = (byte)(((pixel - min) * 255) / span);
			hBit[i] = 0xff000000 + RGB(pix, pix, pix);
			i++;
		}
	}
}

void DrawImageRGB(int *hBit, int *img, int width, int height, int min, int max, int span, int bit)
{
	unsigned char* buf = (unsigned char*)img;

	int i = 0;
	int bitSize = bit / 8;		//16bit인지 8bit인지 확인

	for(int w = 0; w < width; w++)
	{
		for(int h = 0; h < height; h++)
		{
			unsigned int pixel = 0;

			if(bitSize == 1)
				pixel = buf[i];
			else if(bitSize == 2)
			{
				pixel = (buf[i * 2]) + (buf[i * 2 + 1] << 8);
			}

			pixel = ConvertTemp(pixel);
			

			byte pix = (byte)(((pixel - min) * 255) / span);

			int constStage=4;

			unsigned int widthValue = (int)(255.0/constStage);

			int state = (int)pix / widthValue;
			byte pixValue = (((byte)pix - state * (double)widthValue) / widthValue) * 255;
			byte r;
			byte g;
			byte b;

			switch (state)
			{
			case 0:
				b=0;
				g = pixValue;
				r = 255;
				break;
			case 1:
				b=0;
				g = 255;
				r = 255-pixValue;
				break;
			case 2:
				b=pixValue;
				g = 255;
				r = 0;
				break;
			case 3:
				b=255;
				g = 255-pixValue;
				r = 0;
				break;
			default:
				break;
			}

			hBit[i] = 0xff000000 + RGB(b, g, r);
			i++;
		}
	}
}


void DrawImage2RGB(int *hBit, int *img, int width, int height, int min, int max, int span, int bit)
{
	unsigned char* buf = (unsigned char*)img;

	int i = 0;
	int bitSize = bit / 8;		//16bit인지 8bit인지 확인
	for(int w = 0; w < width; w++)
	{
		for(int h = 0; h < height; h++)
		{
			unsigned int pixel = 0;

			if(bitSize == 1)
				pixel = buf[i];
			else if(bitSize == 2)
			{
				pixel = (buf[i * 2]) + (buf[i * 2 + 1] << 8);
			}


			byte pix = (byte)(((pixel - min) * 255) / span);

			int constStage=4;

			unsigned int widthValue = (int)(255.0/constStage);

			int state = (int)pix / widthValue;
			byte pixValue = (((byte)pix - state * (double)widthValue) / widthValue) * 255;
			byte r;
			byte g;
			byte b;

			switch (state)
			{
			case 0:
				b=0;
				g = pixValue;
				r = 255;
				break;
			case 1:
				b=0;
				g = 255;
				r = 255-pixValue;
				break;
			case 2:
				b=pixValue;
				g = 255;
				r = 0;
				break;
			case 3:
				b=255;
				g = 255-pixValue;
				r = 0;
				break;
			default:
				break;
			}

			hBit[i] = 0xff000000 + RGB(b, g, r);
			i++;
		}
	}
}
void DrawImage2RGB_Double(int *hBit, int *img, int width, int height, double min, double max, double span, int bit)
{
	double* buf = (double*)img;

	int i = 0;
	
	for(int w = 0; w < width; w++)
	{
		for(int h = 0; h < height; h++)
		{
			double pixel = buf[i];

			
			byte pix = (byte)(((pixel - min) * 255) / span);

			int constStage=4;

			unsigned int widthValue = (int)(255.0/constStage);

			int state = (int)pix / widthValue;
			byte pixValue = (((byte)pix - state * (double)widthValue) / widthValue) * 255;
			byte r;
			byte g;
			byte b;

			switch (state)
			{
			case 0:
				b=0;
				g = pixValue;
				r = 255;
				break;
			case 1:
				b=0;
				g = 255;
				r = 255-pixValue;
				break;
			case 2:
				b=pixValue;
				g = 255;
				r = 0;
				break;
			case 3:
				b=255;
				g = 255-pixValue;
				r = 0;
				break;
			default:
				break;
			}

			hBit[i] = 0xff000000 + RGB(b, g, r);
			i++;
		}
	}
}

void DrawImage2_Double(int *hBit, int *img, int width, int height, double min, double max, double span, int bit)
{
	double* buf = (double*)img;

	int i=0;

	for(int w=0; w<width; w++)
	{
		for(int h=0; h<height; h++)
		{
			double pixel = buf[i];

			byte pix = (byte)(((pixel - min) * 255) / span);
			hBit[i] = 0xff000000 + RGB(pix,pix,pix);
			i++;
		}
	}
}


void DrawImage2(int *hBit, int *img, int width, int height, int min, int max, int span, int bit)
{
	//SIMD를 통한 속도 개선 필요
	//__m128i* dest = (__m128i*)(hBit);
	//__m128i xmmA;

	//int src[4];

	unsigned char* buf = (unsigned char*)img;

	int i = 0;
	int bitSize = bit / 8;		//16bit인지 8bit인지 확인
	for(int w = 0; w < width; w++)
	{
		for(int h = 0; h < height; h++)
		{
			unsigned int pixel = 0;

			if(bitSize == 1)
				pixel = buf[i];
			else if(bitSize == 2)
			{
				pixel = (buf[i * 2]) + (buf[i * 2 + 1] << 8);
			}

			byte pix = (byte)(((pixel - min) * 255) / span);
			hBit[i] = 0xff000000 + RGB(pix, pix, pix);
			i++;
		}
	}
}

void GetAvgData(int *sum, int *avg, int count, int size)
{
	unsigned short* tAvg = (unsigned short*)avg;

#pragma omp parallel for
	for(int i = 0; i < size; i++)
	{
		tAvg[i] = (int)(sum[i] / count);
	}
}

void GetDeltaData(int *delta, int *max, int *min, int size)
{
	unsigned short* tDelta = (unsigned short*)delta;
	unsigned short* tMax = (unsigned short*)max;
	unsigned short* tMin = (unsigned short*)min;

#pragma omp parallel for
	for(int i = 0; i < size; i++)
	{
		tDelta[i] = tMax[i] - tMin[i];
	}
}

void Base()
{

}