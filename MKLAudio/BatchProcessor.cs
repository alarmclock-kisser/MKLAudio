using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MKLAudio
{
	public class BatchProcessor
	{
		private string Repopath { get; set; }
		private ListBox LogList;
		private ProgressBar ProgressBar;



		public string InputPath { get; set; }
		public string OutputPath { get; set; }
		public int ChunkSize { get; set; } = 512;
		public float Overlap { get; set; } = 0.5f;
		public string StretchKernel { get; set; } = "timestretch";
		public float TargetBpm { get; set; } = 100f;

		public List<long> Times { get; set; } = [];


		private AudioHandling AudioH;
		private OpenClService Service;




		public List<string> InputFiles = [];


		public BatchProcessor(string repopath, string inputPath, string outputPath, OpenClService service, string kernelName = "", float targetBpm = 100f, ListBox? listBox_log = null, ProgressBar? progressBar = null)
		{
			this.Repopath = repopath;
			this.InputPath = inputPath;
			this.OutputPath = outputPath;
			this.Service = service;
			this.StretchKernel = kernelName;
			this.TargetBpm = targetBpm;
			this.LogList = listBox_log ?? new ListBox();
			this.ProgressBar = progressBar ?? new ProgressBar();

			this.AudioH = new AudioHandling(this.Repopath);

			this.VerifyPaths();

			this.GetInputFiles();

			// DEBUG MSGBOX
			MessageBox.Show("BatchProcessor initialized with:\n" +
				"Repopath: " + this.Repopath + "\n" +
				"InputPath: " + this.InputPath + "\n" +
				"Files found" + this.InputFiles.Count + "\n" +
				"OutputPath: " + this.OutputPath + "\n" +
				"StretchKernel: " + this.StretchKernel + "\n" +
				"TargetBpm: " + this.TargetBpm, "BatchProcessor Info", MessageBoxButtons.OK, MessageBoxIcon.Information);



			this.Times = this.ProcessIteratively();


			// this.ExportAll();
		}





		public void Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[Batch]: " + new string(' ', indent * 2) + message;

			if (!string.IsNullOrEmpty(inner))
			{
				msg += " (" + inner + ")";
			}

			// Add to logList
			this.LogList.Items.Add(msg);

			// Scroll down
			this.LogList.SelectedIndex = this.LogList.Items.Count - 1;
		}


		public void Dispose()
		{
			// Reset progressbar
			this.ProgressBar.Value = 0;

			this.AudioH?.Dispose();
		}

		public void VerifyPaths()
		{
			if (string.IsNullOrEmpty(this.Repopath) || !Directory.Exists(this.Repopath))
			{
				this.Repopath = Path.Combine(Directory.GetCurrentDirectory());
			}
			if (string.IsNullOrEmpty(this.InputPath) || !Directory.Exists(this.InputPath))
			{
				this.InputPath = Path.Combine(this.Repopath, "Resources");
			}
			if (string.IsNullOrEmpty(this.OutputPath) || !Directory.Exists(this.OutputPath))
			{
				this.OutputPath = Path.Combine(this.Repopath, "Resources\\Output");
			}
		}

		public void GetInputFiles() =>
			// Get every file in the input directory with .wav, .mp3, or .flac extension
			this.InputFiles = Directory.GetFiles(this.InputPath, "*.*", SearchOption.TopDirectoryOnly)
				.Where(file => file.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
							   file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
							   file.EndsWith(".flac", StringComparison.OrdinalIgnoreCase))
				.ToList();

		public List<long> ProcessLinear()
		{
			// Log action & results times list
			List<long> times = [];
			this.Log("Processing files in linear order...", this.InputFiles.Count + " files");

			// Reset progressbar
			this.ProgressBar.Value = 0;
			this.ProgressBar.Maximum = this.InputFiles.Count * 10;

			// Step 1) Load every file as obj with audioH
			foreach (string file in this.InputFiles)
			{
				this.AudioH.AddTrackAsync(file);
				this.ProgressBar.Value += 1;
			}
			this.Log("Loaded " + this.AudioH.Tracks.Count + " tracks.");

			// Step 2) Process every track with OpenCL
			this.Log("Processing tracks with OpenCL...", this.AudioH.Tracks.Count + " tracks");
			foreach(AudioObject track in this.AudioH.Tracks)
			{
				// STOPWATCH
				Stopwatch sw = Stopwatch.StartNew();

				// Get bpm and stretch factor
				float initialBpm = track.Bpm;
				if (initialBpm * 2 <= this.TargetBpm)
				{
					initialBpm *= 2f; // Double the BPM if it's too low
					this.Log("Track '" + track.Name + "' had BPM: " + track.Bpm, "Doubling BPM to " + initialBpm, 2);
				}
				else if (initialBpm / 2 >= this.TargetBpm)
				{
					initialBpm /= 2f; // Halve the BPM if it's too high
					this.Log("Track '" + track.Name + "' had BPM: " + track.Bpm, "Halving BPM to " + initialBpm, 2);
				}

				if (initialBpm < 10f || initialBpm > 300f)
				{
					this.Log("Track '" + track.Name + "' has an invalid BPM: " + initialBpm, "Skipping", 1);
					continue;
				}
				double stretchFactor = initialBpm / this.TargetBpm;

				// Get optional args
				Dictionary<string, object> optionalArgs;
				if (this.StretchKernel.ToLower().Contains("double"))
				{
					// Double kernel
					optionalArgs = new()
				{
					{ "factor", (double) stretchFactor}
				};
				}
				else
				{
					optionalArgs = new()
				{
					{ "factor", (float) stretchFactor }
				};
				}

				// Log action
				this.Log("Processing track '" + track.Name + "' with stretch factor: " + stretchFactor, "", 1);

				// Exec
				this.Service.ExecuteAudioKernel(
					track,
					this.StretchKernel, "",
					this.ChunkSize,
					this.Overlap,
					optionalArgs,
					false
				);

				// Stop stopwatch and log time
				sw.Stop();
				this.Log("Processed track '" + track.Name + ", BPM now: " + track.Bpm, sw.ElapsedMilliseconds + " ms", 1);
				times.Add(sw.ElapsedMilliseconds);
				this.ProgressBar.Value += 9;
			}

			this.Log("Finished processing tracks.");
			this.ProgressBar.Value = 0;
			this.ProgressBar.Maximum = 0;


			return times;
		}

		public List<long> ProcessIteratively()
		{
			// Log action & results times list
			List<long> times = [];
			this.Log("Processing files in linear order...", this.InputFiles.Count + " files");

			// Reset progressbar
			this.ProgressBar.Value = 0;
			this.ProgressBar.Maximum = this.InputFiles.Count * 10;

			// Step 1) Iterate over loading -> processing -> exporting
			foreach (string file in this.InputFiles)
			{
				Stopwatch sw = Stopwatch.StartNew();

				var obj = this.AudioH.AddTrackAsync(file);
				this.ProgressBar.Value += 1;
	
				this.Log("Loaded " + this.AudioH.Tracks.Count + " tracks.");

				// Step 2) Process every track with OpenCL
				this.Log("Processing tracks with OpenCL...", this.AudioH.Tracks.Count + " tracks");
			
				// Get bpm and stretch factor
				float initialBpm = obj.Bpm;
				if (initialBpm * 2 <= this.TargetBpm)
				{
					initialBpm *= 2f; // Double the BPM if it's too low
					this.Log("Track '" + obj.Name + "' had BPM: " + obj.Bpm, "Doubling BPM to " + initialBpm, 2);
				}
				else if (initialBpm / 2 >= this.TargetBpm)
				{
					initialBpm /= 2f; // Halve the BPM if it's too high
					this.Log("Track '" + obj.Name + "' had BPM: " + obj.Bpm, "Halving BPM to " + initialBpm, 2);
				}

				if (initialBpm < 10f || initialBpm > 300f)
				{
					this.Log("Track '" + obj.Name + "' has an invalid BPM: " + initialBpm, "Skipping", 1);
					continue;
				}
				double stretchFactor = initialBpm / this.TargetBpm;

				// Get optional args
				Dictionary<string, object> optionalArgs;
				if (this.StretchKernel.ToLower().Contains("double"))
				{
					// Double kernel
					optionalArgs = new()
				{
					{ "factor", (double) stretchFactor}
				};
				}
				else
				{
					optionalArgs = new()
				{
					{ "factor", (float) stretchFactor }
				};
				}

				// Log action
				this.Log("Processing track '" + obj.Name + "' with initial BPM: " + obj.Bpm, "stretch factor: " + stretchFactor, 1);

				// Exec
				this.Service.ExecuteAudioKernel(
					obj,
					this.StretchKernel, "",
					this.ChunkSize,
					this.Overlap,
					optionalArgs,
					false
				);

				// Stop stopwatch and log time
				sw.Stop();
				this.Log("Processed track '" + obj.Name + ", BPM now: " + obj.Bpm, sw.ElapsedMilliseconds + " ms", 1);
				times.Add(sw.ElapsedMilliseconds);
				this.ProgressBar.Value += 9;

				obj.Export(this.OutputPath);

				obj.Dispose();
			}

			this.Log("Finished processing tracks.");
			this.ProgressBar.Value = 0;
			this.ProgressBar.Maximum = 0;


			return times;
		}

		public void ExportAll()
		{
			// Log action
			this.Log("Exporting all tracks to output path: " + this.OutputPath, this.AudioH.Tracks.Count + " tracks");
			
			foreach (AudioObject track in this.AudioH.Tracks)
			{
				track.Export(this.OutputPath);
				this.Log("Exported track '" + track.Name + "' to " + this.OutputPath, "", 1);
			}

			this.Log("Finished exporting all tracks. Now disposing AudioHandling with all tracks...");
			this.AudioH.Dispose();
			GC.Collect();
			this.Log("Disposed AudioHandling and collected garbage.");
		}

	}
}
