typedef struct {
    float x;
    float y;
} Vector2;

__kernel void beatscan_singleChunk01(
    __global const Vector2* input,  // genau 1 FFT-Chunk mit chunkSize Einträgen
    __global float* output,         // [1] – Energiewert
    const int chunkSize,
    const int samplerate,
    const float minFreq,            // z. B. 20.0f
    const float maxFreq             // z. B. 150.0f
) {
    float binFreq = (float)samplerate / (float)chunkSize;
    float energy = 0.0f;

    for (int bin = 0; bin < chunkSize / 2; bin++) {
        float freq = bin * binFreq;
        if (freq >= minFreq && freq <= maxFreq) {
            float re = input[bin].x;
            float im = input[bin].y;
            energy += re * re + im * im;
        }
    }

    output[0] = energy;
}
