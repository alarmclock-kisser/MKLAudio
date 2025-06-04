#pragma OPENCL EXTENSION cl_khr_fp64 : enable // Enable double precision

typedef struct {
    double x; // Use double for complex components
    double y; // Use double for complex components
} Vector2;

__kernel void timestretch_double(
    __global const Vector2* input,       // Complex FFT chunks
    __global Vector2* output,            // Output: stretched FFTs
    const int chunkSize,                 // e.g. 1024
    const int overlapSize,               // e.g. 512
    const int samplerate,                // e.g. 44100
    const double factor                  // Stretches factor, now double
) {
    int bin = get_global_id(0);  // Bin index
    int chunk = get_global_id(1); // Chunk index

    int inputChunkOffset = chunk * chunkSize; // Offset for current output chunk

    if (bin >= chunkSize) return; // Skip invalid bins

    // Calculate source chunk index using double precision
    double sourceChunkDouble = (double)chunk / factor;
    int sourceChunk = (int)sourceChunkDouble; // Integer source chunk

    int sourceChunkOffset = sourceChunk * chunkSize; // Offset for source chunk

    int input_idx = sourceChunkOffset + bin; // Input index for current bin
    int output_idx = inputChunkOffset + bin; // Output index for current bin

    // Copy complex value from source chunk if valid
    // Assumes get_global_size(1) represents total input chunks
    if (sourceChunk < get_global_size(1)) {
        output[output_idx] = input[input_idx];
    } else {
        output[output_idx] = (Vector2){0.0, 0.0}; // Set to zero if out of bounds
    }
}