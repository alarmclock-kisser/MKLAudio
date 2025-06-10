__kernel void ringModulator01(
    __global const float* input,
    long length,
    __global float* output,
    float sampleRate,
    float modFreq,   // Modulationsfrequenz in Hz
    float dryWetMix) // Mischungsverhältnis (0.0 für nur Dry, 1.0 für nur Wet)
{
    long i = get_global_id(0);
    if (i >= length) {
        return;
    }

    float currentSample = input[i];

    // Erzeuge eine Sinuswelle als Modulationssignal
    float modulator = sin(2.0f * M_PI_F * modFreq * ((float)i / sampleRate));

    // Multipliziere das Original-Sample mit dem Modulator
    float modulatedSample = currentSample * modulator;

    // Optional: Clamp auf [-1.0, 1.0]
    modulatedSample = clamp(modulatedSample, -1.0f, 1.0f);

    // Mischen von Original (dry) und Effekt (wet)
    output[i] = mix(currentSample, modulatedSample, dryWetMix);
}