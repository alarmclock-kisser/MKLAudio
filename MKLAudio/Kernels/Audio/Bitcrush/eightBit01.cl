__kernel void eightBit01(
    __global const float* input,  // Jetzt korrekt: __global vor const float*
    long length,           // Gesamtl�nge des Audio-Arrays
    __global float* output,         // Jetzt korrekt: __global vor float*
    float intensity,       // Intensit�t des Effekts (0.0 bis 1.0)
    int bitdepth)          // Die Ziel-Bittiefe (z.B. 8, 4, 2 f�r extremeren Effekt)
{
    // Die globale ID des aktuellen Arbeitselements (Index des Samples, das verarbeitet wird)
    long i = get_global_id(0); // Standard f�r OpenCL/CUDA Kernel

    // �berpr�fe, ob die ID innerhalb der Grenzen liegt
    if (i >= length) {
        return;
    }

    // Stelle sicher, dass bitdepth sinnvoll ist (mindestens 1 Bit)
    if (bitdepth < 1) {
        bitdepth = 1; // Minimal 1 Bit
    }
    
    // Berechne die Anzahl der m�glichen diskreten Werte f�r die Ziel-Bittiefe
    float numSteps = pow(2.0f, (float)bitdepth);

    // Den Wert eines Quantisierungsschritts berechnen
    float stepSize = 2.0f / numSteps;

    // Aktuelles Sample lesen
    float sample = input[i];

    // Quantisierung durchf�hren:
    float quantizedSample = round(sample / stepSize) * stepSize;

    // Optional: Clamp auf den Bereich [-1.0, 1.0] um Clipping zu vermeiden
    quantizedSample = clamp(quantizedSample, -1.0f, 1.0f);

    // Mische den quantisierten (8-Bit-Effekt) Sound mit dem Original-Sound
    output[i] = mix(sample, quantizedSample, intensity);
}