typedef struct {
    float x;
    float y;
} Vector2;

__kernel void beatscan_singleChunk02(
    __global Vector2* input,   // komplexe FFT-Daten pro Chunk
    __global float* output,    // 1 Ergebnis (BPM) pro Chunk
    const int chunkSize,       // z. B. 65536
    const int samplerate       // z. B. 44100
) {
    int chunkId = get_global_id(0);
    int offset = chunkId * chunkSize;

    // Frequenzbereich für BPM: 0.5–4 Hz entspricht ~30–240 BPM
    int minBin = (int)(0.5f * chunkSize / samplerate);
    int maxBin = (int)(4.0f * chunkSize / samplerate);

    if (minBin < 1) minBin = 1;
    if (maxBin > chunkSize / 2) maxBin = chunkSize / 2;

    float maxMag = 0.0f;
    int maxIdx = minBin;

    for (int i = minBin; i < maxBin; i++) {
        Vector2 val = input[offset + i];
        float mag = val.x * val.x + val.y * val.y; // Betrag²

        if (mag > maxMag) {
            maxMag = mag;
            maxIdx = i;
        }
    }

    float freq = (float)maxIdx * samplerate / chunkSize;
    float bpm = freq * 60.0f;

    output[chunkId] = bpm;
}
