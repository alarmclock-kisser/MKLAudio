__kernel void beatZoom01( 
    __global const float* samples,     // Mono-Audio-Samples
    const int chunkSize,               // Anzahl der Samples (z. B. 8192)
    __global double* zooms,            // Ergebnis-Zooms (Fraktalframes)
    const int framerate,               // Frames pro Sekunde
    const int samplerate,              // z. B. 44100
    const float threshold,             // Lautstärke-Schwelle (0–1)
    const double minZoom,              // Mindestzoom
    const double maxZoom,              // Maximalzoom
    const double zoomMultiplier        // z. B. 1.05
) {
    int frameIndex = get_global_id(0);

    // Berechne Gesamtanzahl an Frames (muss C#-seitig berücksichtigt werden!)
    int totalFrames = (chunkSize * framerate) / samplerate;
    if (frameIndex >= totalFrames)
        return;

    // Framegröße in Samples
    int samplesPerFrame = samplerate / framerate;
    int start = frameIndex * samplesPerFrame;
    int end = start + samplesPerFrame;
    if (end > chunkSize)
        end = chunkSize;

    // Durchschnittliche Amplitude im Frame
    float sum = 0.0f;
    int count = end - start;
    for (int i = start; i < end; ++i)
        sum += fabs(samples[i]);

    float avg = (count > 0) ? (sum / count) : 0.0f;

    // Zoom-Logik
    double zoom = minZoom;
    if (avg > threshold) {
        // zickzack Zoom: alternierend verstärken / abschwächen
        int dir = (frameIndex % 2 == 0) ? 1 : -1;
        zoom = minZoom * pow(zoomMultiplier, (double)(dir * frameIndex));
    }

    // Clamp Zoombereich
    if (zoom > maxZoom) zoom = maxZoom;
    if (zoom < minZoom) zoom = minZoom;

    zooms[frameIndex] = zoom;
}
