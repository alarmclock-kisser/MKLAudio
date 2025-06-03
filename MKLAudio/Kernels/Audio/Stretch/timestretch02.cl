// FFT + Phase Vocoder + IFFT in one kernel (non-recursive version)
#define PI 3.141592653589793f
#define TWO_PI 6.283185307179586f

typedef struct {
    float x; // Real
    float y; // Imag
} Complex;

// Non-recursive Radix-2 FFT (iterative)
void fft_iterative(__local Complex* buffer, int n) {
    // Bit-reversal permutation
    for (int i = 0, j = 0; i < n; i++) {
        if (i < j) {
            Complex temp = buffer[i];
            buffer[i] = buffer[j];
            buffer[j] = temp;
        }
        for (int k = n >> 1; (j ^= k) < k; k >>= 1);
    }

    // Iterative FFT
    for (int s = 2; s <= n; s <<= 1) {
        int m = s >> 1;
        float theta = -TWO_PI / s;
        
        for (int k = 0; k < n; k += s) {
            for (int j = 0; j < m; j++) {
                float twiddle_real = cos(j * theta);
                float twiddle_imag = sin(j * theta);
                
                Complex t = {
                    twiddle_real * buffer[k + j + m].x - twiddle_imag * buffer[k + j + m].y,
                    twiddle_real * buffer[k + j + m].y + twiddle_imag * buffer[k + j + m].x
                };
                
                buffer[k + j + m].x = buffer[k + j].x - t.x;
                buffer[k + j + m].y = buffer[k + j].y - t.y;
                buffer[k + j].x += t.x;
                buffer[k + j].y += t.y;
            }
        }
    }
}

// Non-recursive IFFT (iterative)
void ifft_iterative(__local Complex* buffer, int n) {
    // Conjugate first
    for (int i = 0; i < n; i++) {
        buffer[i].y = -buffer[i].y;
    }
    
    // Use FFT
    fft_iterative(buffer, n);
    
    // Conjugate and normalize
    for (int i = 0; i < n; i++) {
        buffer[i].x /= n;
        buffer[i].y = -buffer[i].y / n;
    }
}

// Hann window function
float hann_window(int pos, int size) {
    return 0.5f * (1.0f - cos(TWO_PI * pos / (size - 1)));
}

__kernel void timestretch02(
    __global const float* input,
    __global float* output,
    const int chunkSize,
    const int overlapSize,
    const float stretchFactor,
    const float sampleRate,
    const int channels,
    const int bitDepth)
{
    int gid = get_global_id(0);
    int outputChunkSize = (int)(chunkSize * stretchFactor);
    
    // Local memory for FFT processing
    __local Complex fftBuffer[1024];
    __local float lastPhase[512];
    
    // Load input with windowing (mono processing for demo)
    float sample = input[gid * channels] * hann_window(gid, chunkSize);
    fftBuffer[gid].x = sample;
    fftBuffer[gid].y = 0.0f;
    
    barrier(CLK_LOCAL_MEM_FENCE);
    
    // Perform FFT (only by first work item)
    if (gid == 0) {
        fft_iterative(fftBuffer, chunkSize);
    }
    barrier(CLK_LOCAL_MEM_FENCE);
    
    // Phase vocoder processing
    if (gid > 0 && gid < chunkSize/2) {
        float mag = hypot(fftBuffer[gid].x, fftBuffer[gid].y);
        float phase = atan2(fftBuffer[gid].y, fftBuffer[gid].x);
        
        // Phase difference calculation
        float phaseDiff = phase - lastPhase[gid];
        phaseDiff -= TWO_PI * floor(phaseDiff / TWO_PI + 0.5f);
        
        // Frequency analysis
        float trueFreq = gid * (sampleRate/chunkSize) + (phaseDiff * sampleRate)/(TWO_PI * overlapSize);
        
        // Phase stretching
        float stretchedPhase = lastPhase[gid] + (TWO_PI * trueFreq * overlapSize * stretchFactor)/sampleRate;
        lastPhase[gid] = phase; // Store for next chunk
        
        // Update frequency bin
        fftBuffer[gid].x = mag * cos(stretchedPhase);
        fftBuffer[gid].y = mag * sin(stretchedPhase);
        
        // Update mirror bin
        fftBuffer[chunkSize-gid].x = fftBuffer[gid].x;
        fftBuffer[chunkSize-gid].y = -fftBuffer[gid].y;
    }
    barrier(CLK_LOCAL_MEM_FENCE);
    
    // Perform IFFT (only by first work item)
    if (gid == 0) {
        ifft_iterative(fftBuffer, chunkSize);
    }
    barrier(CLK_LOCAL_MEM_FENCE);
    
    // Write output with windowing
    if (gid < outputChunkSize) {
        float outVal = fftBuffer[gid].x * hann_window(gid, outputChunkSize);
        for (int c = 0; c < channels; c++) {
            output[gid * channels + c] = outVal;
        }
    }
}