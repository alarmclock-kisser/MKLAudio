typedef struct {
    float x;
    float y;
} Vector2;

__kernel void timestretch_simple(
    __global const Vector2* input,       // komplexe FFT-Chunks, linear aneinandergereiht
    __global Vector2* output,            // Output: gestreckte FFTs
    const int chunkSize,                 // z.B. 1024
    const int overlapSize,               // z.B. 512 (wird hier vielleicht nicht direkt verwendet, aber f�r Konsistenz)
    const int samplerate,                // z.B. 44100 (wird hier nicht direkt verwendet)
    const float factor                   // z.B. 1.5 = langsamer (gestreckt), 0.5 = schneller (gestaucht)
) {
    int bin = get_global_id(0);  // Bin innerhalb eines Chunks (0..chunkSize-1)
    int chunk = get_global_id(1); // Welcher Chunk

    // Berechnung des Offsets f�r den aktuellen Chunk im Input-Buffer
    int inputChunkOffset = chunk * chunkSize;

    // Nur g�ltige Bins und Chunks verarbeiten
    if (bin >= chunkSize) {
        return; // Dieser Work-Item ist f�r einen ung�ltigen Bin zust�ndig
    }

    // Bestimme den 'urspr�nglichen' Chunk-Index im Input,
    // der dem aktuellen Output-Chunk entspricht, basierend auf dem Faktor.
    // Beispiel: Wenn factor = 1.5, dann ist output chunk 1 bei input chunk 1 / 1.5 = 0.66
    // Wir nehmen den n�chsten ganzen Chunk oder interpolieren.
    // F�r einen einfachen Ansatz runden wir einfach.
    float sourceChunkFloat = (float)chunk / factor;
    int sourceChunk = (int)sourceChunkFloat; // Indiziert den 'Quell'-Chunk im Input

    // Stellen sicher, dass wir nicht �ber das Ende der Input-Daten hinauslesen.
    // Annahme: totalChunks muss vom Host �bergeben werden oder global_size(1) nutzen
    // F�r diesen Entwurf nehmen wir an, dass die input-Buffer-Gr��e gro� genug ist.
    // In einer realen Anwendung m�sstest du hier pr�fen, ob sourceChunk g�ltig ist.
    // get_num_groups(1) w�re die Anzahl der Output Chunks. Du m�sstest die Anzahl der Input Chunks kennen.
    // Hier vereinfacht: wir gehen davon aus, dass input gen�gend Chunks enth�lt
    // oder die Host-Seite den totalChunks-Wert �bergeben hat.
    // Wenn sourceChunk >= inputTotalChunks, sollte man Nullen schreiben oder eine andere Strategie w�hlen.

    int sourceChunkOffset = sourceChunk * chunkSize;
    
    // Berechne den Index im Quell-Chunk.
    // Wenn sourceChunkFloat nicht exakt sourceChunk ist, kann man interpolieren.
    // F�r diesen einfachen Ansatz nehmen wir nur den direkten Bin.
    int input_idx = sourceChunkOffset + bin;
    int output_idx = inputChunkOffset + bin;

    // �berpr�fen, ob der Input-Index g�ltig ist
    // (Annahme: input_size ist die Gesamtzahl der Vector2 Elemente im input-Buffer)
    // Wenn input_idx >= input_size, dann schreiben wir Nullen oder den letzten g�ltigen Wert.
    // Hier einfach: Wenn der sourceChunk zu gro� ist, schreiben wir Nullen
    // oder verlassen uns darauf, dass der Host die output-size passend macht.
    // Dies m�sste in einer vollst�ndigen Implementierung robuster sein.
    
    // F�r einen einfachen Kopiervorgang basierend auf dem Faktor:
    // Kopiere den komplexen Wert vom Source-Chunk zum aktuellen Output-Bin.
    // Hier wird einfach nur der Inhalt des n�chstgelegenen urspr�nglichen Chunks kopiert.
    // Das ist das grundlegendste Time-Stretching ohne Phase Vocoder.
    if (sourceChunk < get_global_size(1)) { // Annahme: get_global_size(1) ist die Anzahl der Input Chunks
        output[output_idx] = input[input_idx];
    } else {
        // Au�erhalb der Input-Daten: Nullen setzen
        output[output_idx] = (Vector2){0.0f, 0.0f};
    }
}