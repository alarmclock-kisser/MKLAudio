#pragma OPENCL EXTENSION cl_khr_fp64 : enable

typedef struct {
    float x;
    float y;
} Vector2;

#ifndef M_PI
#define M_PI 3.14159265358979323846f
#endif

float calculate_hann_window(int idx, int windowSize) {
    if (windowSize <= 1) return 1.0f;
    return 0.5f * (1.0f - cos(2.0f * M_PI * idx / (float)(windowSize - 1)));
}

__kernel void timestretch05(
    __global const Vector2* input,
    __global Vector2* output,
    const int chunkSize,
    const int overlapSize,
    const int samplerate,
    const float factor
) {
    int bin = get_global_id(0);
    int chunk = get_global_id(1);

    int totalOutputChunks = get_global_size(1); // Anzahl der Output-Chunks
    int totalInputChunks = get_global_size(1); // Annahme: Input hat gleiche Anzahl Chunks wie Output

    if (chunk >= totalOutputChunks || bin >= chunkSize) return;

    int hopIn = chunkSize - overlapSize;
    float hopOutFloat = hopIn * factor;

    // Berechnung des Quell-Chunks und des Interpolationsfaktors
    float currentInputChunkFloat = (float)chunk * hopOutFloat / (float)hopIn;
    int inputChunk1_idx = (int)floor(currentInputChunkFloat);
    int inputChunk2_idx = inputChunk1_idx + 1;

    if (inputChunk1_idx < 0 || inputChunk1_idx >= totalInputChunks) {
        output[chunk * chunkSize + bin] = (Vector2){0.0f, 0.0f};
        return;
    }
    if (inputChunk2_idx >= totalInputChunks) {
        inputChunk2_idx = inputChunk1_idx;
    }

    // Zugriff auf Input-Spektren
    Vector2 curSpec = input[inputChunk1_idx * chunkSize + bin];
    Vector2 nextSpec = input[inputChunk2_idx * chunkSize + bin];

    // Amplituden-Interpolation
    float mag1 = hypot(curSpec.x, curSpec.y);
    float mag2 = hypot(nextSpec.x, nextSpec.y);
    float alpha = currentInputChunkFloat - floor(currentInputChunkFloat);
    float interpolatedMag = mag1 * (1.0f - alpha) + mag2 * alpha;

    // Phasenextraktion
    float phaseIn1 = atan2(curSpec.y, curSpec.x);
    float phaseIn2 = atan2(nextSpec.y, nextSpec.x);

    // Erwarteter Phasenvorschub (basierend auf Frequenz des Bins)
    float expectedDeltaPhase = 2.0f * M_PI * bin * hopIn / (float)chunkSize;

    // Phasendifferenz unwrap (Korrektur der Phasenabweichung)
    float deltaInputPhase = phaseIn2 - phaseIn1;
    float phaseResidual = fmod(deltaInputPhase - expectedDeltaPhase + M_PI, 2.0f * M_PI) - M_PI;

    // Berechnung der neuen Output-Phase
    // Dies ist eine Näherung der Phase Vocoder Akkumulation für Parallelität
    float outputPhase = phaseIn1 + (currentInputChunkFloat - inputChunk1_idx) * expectedDeltaPhase + phaseResidual * factor;

    // Hann-Fenster anwenden
    float hannWindowValue = calculate_hann_window(bin, chunkSize);
    interpolatedMag *= hannWindowValue;

    // Output des neuen komplexen Werts
    output[chunk * chunkSize + bin].x = interpolatedMag * cos(outputPhase);
    output[chunk * chunkSize + bin].y = interpolatedMag * sin(outputPhase);
}