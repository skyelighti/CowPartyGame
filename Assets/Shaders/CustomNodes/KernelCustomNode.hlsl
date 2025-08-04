#ifndef KERNEL_CREATION_INCLUDED
#define KERNEL_CREATION_INCLUDED

void GenerateKernel_float(float4 UV, float2 kernelSize, float sizeMarkiplier, float2 textureSize, UnityTexture2D weightTex, UnityTexture2D subjectTex, UnitySamplerState samplerState, out float pixVal)
{
    int refWeight = 0;
    float totalWeight = 0;
    float totalNumWeight = 0;
    for (int xPixOffset = -floor(kernelSize.x/2.0); xPixOffset <= floor(kernelSize.x/2.0); xPixOffset++)
    {
        for (int yPixOffset = -floor(kernelSize.y/2.0); yPixOffset <= floor(kernelSize.y/2.0); yPixOffset++)
        {
            //pixOffSet
            float4 currPix = UV + float4(xPixOffset/textureSize.x,yPixOffset/textureSize.y,0,0)*sizeMarkiplier;
            float weight = weightTex.Sample(samplerState, float2(refWeight, 0)).r;
            float4 grayscaleValue = dot(subjectTex.Sample(samplerState, currPix.xy), float3(0.299, 0.587, 0.114));
            totalWeight += weight*grayscaleValue; //currPix is UV -> get color val for that UV coordinate
            totalNumWeight += weight;
            refWeight++;
        }
    }

    if (totalNumWeight > 0)
    {
        pixVal = totalWeight / totalNumWeight;
    }
    else
    {
        pixVal = 0;
    }
}

#endif //KERNEL_CREATION_INCLUDED