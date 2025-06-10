__kernel void tremolo01(
    __global const float* input,
    long length,
    __global float* output,
    float sampleRate, // Sample-Rate in Hz (z.B. 44100.0f)
    float rate,    // Modulationsfrequenz in Hz
    float depth)   // Stärke der Modulation (0.0 bis 1.0)
{
    long i = get_global_id(0);
    if (i >= length) {
        return;
    }

    // Audio-Samplerate (muss in deinem Host-Code als Konstante übergeben oder definiert werden)
    // In einem echten Kernel müsstest du Samplerate als Argument übergeben, da es keine globale Variable ist.
    float sample = input[i];

    // Berechne den Modulationswert (Sinuswelle)
    // 2 * PI * rate: Umrechnung der Frequenz in Radiant pro Sample.
    // i / sampleRate: Aktuelle Zeit in Sekunden für dieses Sample.
    float modulation = sin(2.0f * M_PI_F * rate * ( (float)i / sampleRate ) );

    // Skaliere die Modulation auf den Bereich [0.0, 1.0] und wende die Tiefe an.
    // (modulation + 1.0f) / 2.0f skaliert Sinus von [-1, 1] auf [0, 1].
    // Dann mit depth und 1.0f - depth mischen.
    float effectFactor = 1.0f - depth + depth * ((modulation + 1.0f) / 2.0f);

    // Wende den Modulationsfaktor auf das Sample an
    output[i] = sample * effectFactor;
}