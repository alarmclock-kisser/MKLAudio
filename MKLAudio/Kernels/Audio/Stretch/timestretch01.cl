// FFT Time-Stretching mit Phase-Vocoder
// - Feste Chunk-Größe (z.B. 1024 Bins)
// - Interne Phasenverwaltung (keine externen Arrays)
// - Output-Chunk hat gleiche Größe wie Input (FFT-Bins bleiben gleich)

typedef struct {
    float x; // Realteil
    float y; // Imaginärteil
} Vector2;

__constant float PI = 3.141592653589793f;
__constant float TWO_PI = 6.283185307179586f;

__kernel void timestretch01(
    __global const Vector2* input,      // Input-Chunk (FFT-Bins)
    __global Vector2* output,           // Output-Chunk (zeitgestreckt)
    const int chunkSize,                // FFT-Größe (z.B. 1024)
    const int overlapSize,              // Overlap (z.B. 512)
    const float stretchFactor,          // Zeitstreckfaktor (2.0 = doppelt so lang)
    const float sampleRate              // Für Frequenzberechnung
) {
    int gid = get_global_id(0);
    if (gid >= chunkSize) return;

    // Lokaler Speicher für Phasen (pro Workgroup)
    __local float prevPhases[1024];     // Maximal unterstützte Chunk-Größe
    float currentPhase;

    // Magnitude & Phase des aktuellen Bins
    float mag = hypot(input[gid].x, input[gid].y);
    float phase = atan2(input[gid].y, input[gid].x);

    // Initialisierung der Phasen (nur durch 1. Work-Item)
    if (gid == 0) {
        for (int i = 0; i < chunkSize; i++) {
            prevPhases[i] = 0.0f;
        }
    }
    barrier(CLK_LOCAL_MEM_FENCE);

    // Phase-Vocoder-Verarbeitung (nicht für DC-Bin gid=0)
    if (gid > 0) {
        // Frequenz dieses Bins
        float binFreq = gid * (sampleRate / chunkSize);

        // Phasendifferenz zum letzten Chunk
        float phaseDiff = phase - prevPhases[gid];
        phaseDiff = phaseDiff - TWO_PI * floor(phaseDiff / TWO_PI + 0.5f); // Wrap auf [-π, π]

        // Tatsächliche Frequenz (unter Berücksichtigung der Phasendifferenz)
        float trueFreq = binFreq + (phaseDiff * sampleRate) / (TWO_PI * overlapSize);

        // Neue Phase mit Zeitstreckung berechnen
        currentPhase = prevPhases[gid] + (TWO_PI * trueFreq * overlapSize * stretchFactor) / sampleRate;
        prevPhases[gid] = currentPhase; // Für nächsten Chunk speichern

        // Phase normalisieren und Output setzen
        currentPhase = atan2(sin(currentPhase), cos(currentPhase)); // Wrap auf [-π, π]
        output[gid].x = mag * cos(currentPhase);
        output[gid].y = mag * sin(currentPhase);
    } else {
        // DC-Bin (Index 0) unverändert durchreichen
        output[gid] = input[gid];
    }
    barrier(CLK_LOCAL_MEM_FENCE);
}