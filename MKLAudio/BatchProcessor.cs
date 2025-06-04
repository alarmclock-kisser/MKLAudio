using System;
using System.Collections.Generic;
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



			this.ProcessLinear();


			this.ExportAll();
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

		public void ProcessLinear()
		{
			// Log action
			this.Log("Processing files in linear order...", this.InputFiles.Count + " files");

			// Reset progressbar
			this.ProgressBar.Value = 0;
			this.ProgressBar.Maximum = this.InputFiles.Count;

			// Step 1) Load every file as obj with audioH
			foreach (string file in this.InputFiles)
			{
				this.AudioH.AddTrackAsync(file);
			}
			this.Log("Loaded " + this.AudioH.Tracks.Count + " tracks.");

			// Step 2) Process every track with OpenCL
			this.Log("Processing tracks with OpenCL...", this.AudioH.Tracks.Count + " tracks");
			foreach(AudioObject track in this.AudioH.Tracks)
			{
				// Get bpm and stretch factor
				float initialBpm = track.Bpm;
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
					true
				);

				this.Log("Processed track '" + track.Name, "BPM now: " + track.Bpm, 1);
			}

			this.Log("Finished processing tracks.");




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
