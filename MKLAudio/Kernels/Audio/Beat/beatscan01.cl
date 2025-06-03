#define BANDS 32  // Anzahl Frequenzbänder
#define HISTORY 24 // Anzahl gespeicherte Frames (~0.5s bei 512 Hop)

typedef struct {
    float x; // Real
    float y; // Imag
} Complex;

__kernel void beatscan01(
    __global const Complex* fft_chunk, // Input: FFT-Chunk (1024 Bins)
    __global float* beat_output,       // Output: Beat-Wahrscheinlichkeit [0-1]
    const int chunk_size,              // FFT-Größe (1024)
    const float sample_rate,           // 44100 etc.
    __private float energy_history[HISTORY][BANDS] // Automatisch vom Compiler verwaltet
) {
    int gid = get_global_id(0);
    int bin = gid % BANDS;
    int frame = gid / BANDS;
    
    // 1. Frequenzbänder definieren (0-200Hz, 200-400Hz, ...)
    float band_low = bin * (200.0f / BANDS);
    float band_high = (bin + 1) * (200.0f / BANDS);
    
    // 2. Energie im Frequenzband berechnen
    float energy = 0.0f;
    for (int i = (int)(band_low * chunk_size / sample_rate); 
         i < (int)(band_high * chunk_size / sample_rate); 
         i++) {
        energy += fft_chunk[i].x * fft_chunk[i].x + fft_chunk[i].y * fft_chunk[i].y;
    }
    energy = sqrt(energy);
    
    // 3. Dynamische Schwelle berechnen (Mittelwert der letzten HISTORY Frames)
    float threshold = 0.0f;
    for (int i = 0; i < HISTORY; i++) {
        threshold += energy_history[i][bin];
    }
    threshold /= HISTORY;
    
    // 4. Beat-Wahrscheinlichkeit berechnen
    float beat_prob = max(0.0f, (energy - threshold * 1.3f) / (threshold + 0.001f));
    beat_output[gid] = beat_prob;
    
    // 5. Energie für zukünftige Frames speichern (Ringbuffer)
    energy_history[frame % HISTORY][bin] = energy;
}