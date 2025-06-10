__kernel void delay01(
    __global const float* input,
    long length,
    __global float* output,
    float sampleRate, // Sample-Rate in Hz (z.B. 44100.0f)
    float delayTimeSeconds, // Verzögerungszeit in Sekunden
    float decayFactor,      // Abklingfaktor pro Echo (0.0 bis 1.0)
    float dryWetMix)        // Mischungsverhältnis (0.0 für nur Dry, 1.0 für nur Wet)
{
    long i = get_global_id(0);
    if (i >= length) {
        return;
    }

    // Berechne den Delay-Offset in Samples
    int delaySamples = (int)(delayTimeSeconds * sampleRate);

    float currentSample = input[i];
    float delayedSample = 0.0f;

    // Hole das Sample von der verzögerten Position
    int delayedIndex = i - delaySamples;

    // Stelle sicher, dass der verzögerte Index gültig ist
    if (delayedIndex >= 0 && delayedIndex < length) {
        // Für einfaches Feedback-Delay müsste hier der Output-Buffer an delayedIndex gelesen werden.
        // Das ist aber bei einem rein parallelen Kernel schwierig, da Output noch nicht geschrieben ist.
        // Für einen "echten" Delay-Effekt mit Feedbackschleife brauchst du einen Zustand,
        // der über mehrere Kernel-Aufrufe persistiert oder einen zusätzlichen globalen Buffer.
        // Dies ist ein *einfaches, nicht-feedback Delay* oder ein "Tap Delay".

        // Wenn es ein Feedback-Delay sein soll, brauchst du einen zusätzlichen Buffer
        // für die Delay-Line, der den vorherigen Output enthält, z.B. __global const float* delayLine
        // und dann: delayedSample = delayLine[delayedIndex] * decayFactor;

        // Für dieses Beispiel: Nur einfaches verzögertes Signal vom Input
        delayedSample = input[delayedIndex] * decayFactor;
    }
    
    // Mischen von Original (dry) und Effekt (wet)
    output[i] = mix(currentSample, currentSample + delayedSample, dryWetMix);
    // Beachte: currentSample + delayedSample kann über 1.0 gehen. Hier könnte ein Limiter sinnvoll sein.
    output[i] = clamp(output[i], -1.0f, 1.0f);
}