__kernel void delay01(
    __global const float* input,
    long length,
    __global float* output,
    float sampleRate, // Sample-Rate in Hz (z.B. 44100.0f)
    float delayTimeSeconds, // Verz�gerungszeit in Sekunden
    float decayFactor,      // Abklingfaktor pro Echo (0.0 bis 1.0)
    float dryWetMix)        // Mischungsverh�ltnis (0.0 f�r nur Dry, 1.0 f�r nur Wet)
{
    long i = get_global_id(0);
    if (i >= length) {
        return;
    }

    // Berechne den Delay-Offset in Samples
    int delaySamples = (int)(delayTimeSeconds * sampleRate);

    float currentSample = input[i];
    float delayedSample = 0.0f;

    // Hole das Sample von der verz�gerten Position
    int delayedIndex = i - delaySamples;

    // Stelle sicher, dass der verz�gerte Index g�ltig ist
    if (delayedIndex >= 0 && delayedIndex < length) {
        // F�r einfaches Feedback-Delay m�sste hier der Output-Buffer an delayedIndex gelesen werden.
        // Das ist aber bei einem rein parallelen Kernel schwierig, da Output noch nicht geschrieben ist.
        // F�r einen "echten" Delay-Effekt mit Feedbackschleife brauchst du einen Zustand,
        // der �ber mehrere Kernel-Aufrufe persistiert oder einen zus�tzlichen globalen Buffer.
        // Dies ist ein *einfaches, nicht-feedback Delay* oder ein "Tap Delay".

        // Wenn es ein Feedback-Delay sein soll, brauchst du einen zus�tzlichen Buffer
        // f�r die Delay-Line, der den vorherigen Output enth�lt, z.B. __global const float* delayLine
        // und dann: delayedSample = delayLine[delayedIndex] * decayFactor;

        // F�r dieses Beispiel: Nur einfaches verz�gertes Signal vom Input
        delayedSample = input[delayedIndex] * decayFactor;
    }
    
    // Mischen von Original (dry) und Effekt (wet)
    output[i] = mix(currentSample, currentSample + delayedSample, dryWetMix);
    // Beachte: currentSample + delayedSample kann �ber 1.0 gehen. Hier k�nnte ein Limiter sinnvoll sein.
    output[i] = clamp(output[i], -1.0f, 1.0f);
}