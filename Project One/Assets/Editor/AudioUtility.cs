using UnityEngine;

public static class AudioUtility
{
    // 分块加载波形数据（避免一次性读取长音频）
    public static void GetWaveFormChunk(AudioClip clip, float[] output, int startSample, int endSample)
{
    if (clip == null || output == null || startSample < 0 || endSample > clip.samples * clip.channels)
    {
        Debug.LogError("Invalid parameters for waveform chunk loading");
        return;
    }

    float[] chunkSamples = new float[endSample - startSample];
    bool success = clip.GetData(chunkSamples, startSample);
    
    if (!success)
    {
        Debug.LogError("Failed to load audio chunk");
        return;
    }

    // 修复：严格按时间比例下采样
    int totalSamples = endSample - startSample;
    int outputSamples = output.Length;
    float samplesPerPoint = (float)totalSamples / (outputSamples); // 每个输出点对应的原始样本数
    int step =1;//波长缩短一半参数

    for (int i = 0; i < outputSamples; i++)
    {
        float sum = 0;
        int count = 0;
        int chunkStart = Mathf.FloorToInt(i * samplesPerPoint*step);
        int chunkEnd = Mathf.FloorToInt((i + 1) * samplesPerPoint*step);

        for (int j = chunkStart; j < chunkEnd && j < chunkSamples.Length; j++)
        {
            sum += Mathf.Abs(chunkSamples[j]);
            count++;
        }
        output[i] = count > 0 ? sum / count*10f  : 0f; // 保持振幅增强
    }
}
}