#ifndef EDGE_DETECTOR_INCLUDED
#define EDGE_DETECTOR_INCLUDED

static float2 sobelSamplePoints[9] = 
{
    float2(-1, 1), float2(0, 1), float2(1, 1),
    float2(-1, 0), float2(0, 0), float2(1, 0),
    float2(-1, -1), float2(0, -1), float2(1, -1)
};

static float sobelXMatrix[9] = 
{
    1, 0, -1,
    2, 0, -2,
    1, 0, -1
};

static float sobelYMatrix[9] = 
{
    1, 2, 1,
    0, 0, 0,
    -1, -2, -1
};

// ------------------------------------- DEPTH -------------------------------------//
void DepthSobel_float(float2 UV, float Thickness, out float pixVal)
{
    float2 sobel = 0;
    [unroll] for (int i = 0; i<9; i++)
    {
        float depth = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV+ sobelSamplePoints[i]*Thickness);
        sobel += depth*float2(sobelXMatrix[i],sobelYMatrix[i]);
    }
    pixVal = length(sobel);
}

/*
// ------------------------------------- COLOR -------------------------------------//
void ColorSobel_float(float2 UV, float Thickness, out float pixVal)
{
    float2 sobelR = 0;
    float2 sobelG = 0;
    float2 sobelB = 0;
    [unroll] for (int i = 0; i < 9; i ++)
    {
        float3 rgb = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV + sobelSamplePoints[i] * Thickness);
        float2 kernel = float2(sobelXMatrix[i], sobelYMatrix[i]);
        sobelR += rgb.r * kernel;
        sobelG += rgb.g * kernel;
        sobelB += rgb.b * kernel;
    }

    pixVal = max(length(sobelR), max(length(sobelG), length(sobelB)));
}
*/

#endif //EDGE_DETECTOR_INCLUDED