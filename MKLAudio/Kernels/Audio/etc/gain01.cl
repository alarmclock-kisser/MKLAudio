__kernel void gain01(
    __global const float* input,
    long length,
    __global float* output,
    float factor) // Lautstärke-Faktor (z.B. 0.5 für -6dB, 2.0 für +6dB)
{
    long i = get_global_id(0);
    if (i >= length) {
        return;
    }

    // Einfach das Sample mit dem Faktor multiplizieren
    // Optional: clamp auf [-1.0, 1.0], um Clipping zu vermeiden, falls factor > 1.0
    output[i] = clamp(input[i] * factor, -1.0f, 1.0f);
}