// CalcLib.h

#include <Windows.h>
#include <emmintrin.h>
#include <omp.h>
#include <stdio.h>

#pragma once

using namespace System;

extern "C" __declspec(dllexport) void Base();
extern "C" __declspec(dllexport) void CalcurateMinMax(int *img, int *sum, int *pmax, int *pmin, int size, int *min, int *max, int bit);
extern "C" __declspec(dllexport) void DrawImage(int *hBit, int *img, int width, int height, int min, int max, int span, int bit);
extern "C" __declspec(dllexport) void DrawImageRGB(int *hBit, int *img, int width, int height, int min, int max, int span, int bit);
extern "C" __declspec(dllexport) void GetAvgData(int *sum, int *avg, int count, int size);
extern "C" __declspec(dllexport) void GetDeltaData(int *delta, int *max, int *min, int size);
extern "C" __declspec(dllexport) void CalcurateMinMax2(int *img, int size, int *min, int *max);
extern "C" __declspec(dllexport) void DrawImage2(int *hBit, int *img, int width, int height, int min, int max, int span, int bit);
extern "C" __declspec(dllexport) void DrawImage2RGB(int *hBit, int *img, int width, int height, int min, int max, int span, int bit);