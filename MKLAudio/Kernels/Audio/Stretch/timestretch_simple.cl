typedef struct {
    float x;
    float y;
} Vector2;

__kernel void timestretch_simple(
    __global const Vector2* input,       // komplexe FFT-Chunks, linear aneinandergereiht
    __global Vector2* output,            // Output: gestreckte FFTs
    const int chunkSize,                 // z.B. 1024
    const int overlapSize,               // z.B. 512 (wird hier vielleicht nicht direkt verwendet, aber für Konsistenz)
    const int samplerate,                // z.B. 44100 (wird hier nicht direkt verwendet)
    const float factor                   // z.B. 1.5 = langsamer (gestreckt), 0.5 = schneller (gestaucht)
) {
    int bin = get_global_id(0);  // Bin innerhalb eines Chunks (0..chunkSize-1)
    int chunk = get_global_id(1); // Welcher Chunk

    // Berechnung des Offsets für den aktuellen Chunk im Input-Buffer
    int inputChunkOffset = chunk * chunkSize;

    // Nur gültige Bins und Chunks verarbeiten
    if (bin >= chunkSize) {
        return; // Dieser Work-Item ist für einen ungültigen Bin zuständig
    }

    // Bestimme den 'ursprünglichen' Chunk-Index im Input,
    // der dem aktuellen Output-Chunk entspricht, basierend auf dem Faktor.
    // Beispiel: Wenn factor = 1.5, dann ist output chunk 1 bei input chunk 1 / 1.5 = 0.66
    // Wir nehmen den nächsten ganzen Chunk oder interpolieren.
    // Für einen einfachen Ansatz runden wir einfach.
    float sourceChunkFloat = (float)chunk / factor;
    int sourceChunk = (int)sourceChunkFloat; // Indiziert den 'Quell'-Chunk im Input

    // Stellen sicher, dass wir nicht über das Ende der Input-Daten hinauslesen.
    // Annahme: totalChunks muss vom Host übergeben werden oder global_size(1) nutzen
    // Für diesen Entwurf nehmen wir an, dass die input-Buffer-Größe groß genug ist.
    // In einer realen Anwendung müsstest du hier prüfen, ob sourceChunk gültig ist.
    // get_num_groups(1) wäre die Anzahl der Output Chunks. Du müsstest die Anzahl der Input Chunks kennen.
    // Hier vereinfacht: wir gehen davon aus, dass input genügend Chunks enthält
    // oder die Host-Seite den totalChunks-Wert übergeben hat.
    // Wenn sourceChunk >= inputTotalChunks, sollte man Nullen schreiben oder eine andere Strategie wählen.

    int sourceChunkOffset = sourceChunk * chunkSize;
    
    // Berechne den Index im Quell-Chunk.
    // Wenn sourceChunkFloat nicht exakt sourceChunk ist, kann man interpolieren.
    // Für diesen einfachen Ansatz nehmen wir nur den direkten Bin.
    int input_idx = sourceChunkOffset + bin;
    int output_idx = inputChunkOffset + bin;

    // Überprüfen, ob der Input-Index gültig ist
    // (Annahme: input_size ist die Gesamtzahl der Vector2 Elemente im input-Buffer)
    // Wenn input_idx >= input_size, dann schreiben wir Nullen oder den letzten gültigen Wert.
    // Hier einfach: Wenn der sourceChunk zu groß ist, schreiben wir Nullen
    // oder verlassen uns darauf, dass der Host die output-size passend macht.
    // Dies müsste in einer vollständigen Implementierung robuster sein.
    
    // Für einen einfachen Kopiervorgang basierend auf dem Faktor:
    // Kopiere den komplexen Wert vom Source-Chunk zum aktuellen Output-Bin.
    // Hier wird einfach nur der Inhalt des nächstgelegenen ursprünglichen Chunks kopiert.
    // Das ist das grundlegendste Time-Stretching ohne Phase Vocoder.
    if (sourceChunk < get_global_size(1)) { // Annahme: get_global_size(1) ist die Anzahl der Input Chunks
        output[output_idx] = input[input_idx];
    } else {
        // Außerhalb der Input-Daten: Nullen setzen
        output[output_idx] = (Vector2){0.0f, 0.0f};
    }
}