using OpenTK.Graphics.ES11;

namespace MKLAudio
{
	public partial class WindowMain : Form
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		public AudioHandling AudioH;
		public ImageHandling ImageH;
		public VideoHandling VideoH;

		public OpenClService Service;





		private Dictionary<NumericUpDown, int> previousNumericValues = [];
		private bool isProcessing;



		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public WindowMain()
		{
			this.InitializeComponent();

			this.Repopath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(0, 0);

			this.AudioH = new AudioHandling(this.Repopath, this.listBox_log, this.listBox_tracks, this.pictureBox_wave, this.button_play, this.textBox_time, this.label_meta, this.hScrollBar_offset, this.vScrollBar_volume, this.numericUpDown_zoom);
			this.ImageH = new ImageHandling(this.Repopath, this.listBox_images, this.pictureBox_image, this.numericUpDown_zoomImage, this.label_imageMeta);

			this.Service = new OpenClService(this.Repopath, this.listBox_log, this.comboBox_devices);

			this.VideoH = new VideoHandling(this.Repopath, this.AudioH, this.ImageH, this.Service);

			this.Service.SelectDeviceLike("Intel");

			this.Service.KernelCompiler?.FillGenericKernelNamesCombobox(this.comboBox_kernelNames);
			this.Service.FillSpecificKernels(this.comboBox_kernelsStretch, "timestretch");
			this.Service.FillSpecificKernels(this.comboBox_kernelBeatScan, "beatscan");

			// Register events
			this.RegisterNumericToSecondPow(this.numericUpDown_chunkSize);
			this.listBox_log.DoubleClick += (s, e) => this.CopyLogLineToClipboard(this.listBox_log.SelectedIndex);

			// Load resources
			this.AudioH.LoadResourcesAudios();
			this.ImageH.LoadResourcesImages();
		}






		// ----- ----- ----- METHODS ----- ----- ----- \\
		public void CopyLogLineToClipboard(int index = -1)
		{
			if (index < 0)
			{
				// If no index is provided, use the selected index
				index = this.listBox_log.SelectedIndex;
			}

			// Check if index is valid
			if (index < 0 || index >= this.listBox_log.Items.Count)
			{
				MessageBox.Show("Invalid log line index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get the log line and copy it to clipboard
			string logLine = this.listBox_log.Items[index].ToString() ?? string.Empty;
			Clipboard.SetText(logLine);
			MessageBox.Show($"{logLine}", "Log line copied to clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void RegisterNumericToSecondPow(NumericUpDown numeric)
		{
			// Initialwert speichern
			this.previousNumericValues.Add(numeric, (int) numeric.Value);

			numeric.ValueChanged += (s, e) =>
			{
				// Verhindere rekursive Aufrufe
				if (this.isProcessing)
				{
					return;
				}

				this.isProcessing = true;

				try
				{
					int newValue = (int) numeric.Value;
					int oldValue = this.previousNumericValues[numeric];
					int max = (int) numeric.Maximum;
					int min = (int) numeric.Minimum;

					// Nur verarbeiten, wenn sich der Wert tatsächlich geändert hat
					if (newValue != oldValue)
					{
						int calculatedValue;

						if (newValue > oldValue)
						{
							// Verdoppeln, aber nicht über Maximum
							calculatedValue = Math.Min(oldValue * 2, max);
						}
						else if (newValue < oldValue)
						{
							// Halbieren, aber nicht unter Minimum
							calculatedValue = Math.Max(oldValue / 2, min);
						}
						else
						{
							calculatedValue = oldValue;
						}

						// Nur aktualisieren wenn notwendig
						if (calculatedValue != newValue)
						{
							numeric.Value = calculatedValue;
						}

						this.previousNumericValues[numeric] = calculatedValue;
					}
				}
				finally
				{
					this.isProcessing = false;
				}
			};
		}




		// ----- ----- ----- EVENT HANDLERS ----- ----- ----- \\
		private void button_info_Click(object sender, EventArgs e)
		{
			// If CTRL down
			if (ModifierKeys.HasFlag(Keys.Control))
			{
				this.Service.GetInfoPlatformInfo(null, false, true);
			}
			else
			{
				this.Service.GetInfoDeviceInfo(null, false, true);
			}
		}

		private void button_import_Click(object sender, EventArgs e)
		{
			this.AudioH.ImportAudioFile();

			this.AudioH.RefreshView();
		}

		private void button_export_Click(object sender, EventArgs e)
		{
			this.AudioH.CurrentTrack?.Export();
		}

		private void comboBox_kernelNames_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.Service.KernelCompiler?.FillGenericKernelVersionsCombobox(this.comboBox_kernelVersions, this.comboBox_kernelNames.SelectedItem?.ToString() ?? "", true);
		}

		private void button_kernelLoad_Click(object sender, EventArgs e)
		{
			this.Service.KernelCompiler?.LoadKernel(this.comboBox_kernelNames.SelectedItem?.ToString() + this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "", "", this.panel_kernelArguments, this.checkBox_invariables.Checked);
		}

		private void checkBox_invariables_CheckedChanged(object sender, EventArgs e)
		{
			string kernelName = this.comboBox_kernelNames.SelectedItem?.ToString() + this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "";

			this.Service.RebuildKernelArgs(this.panel_kernelArguments, kernelName, this.checkBox_invariables.Checked);
		}

		private void button_move_Click(object sender, EventArgs e)
		{
			// Abort if no current track is selected
			if (this.AudioH.CurrentTrack == null)
			{
				MessageBox.Show("No track selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Service.MoveAudio(this.AudioH.CurrentTrack, (int) this.numericUpDown_chunkSize.Value, (float) this.numericUpDown_overlap.Value, 1.0f, this.checkBox_log.Checked);

			this.AudioH.RefreshView();
			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_fft_Click(object sender, EventArgs e)
		{
			// Abort if no current track is selected
			if (this.AudioH.CurrentTrack == null)
			{
				MessageBox.Show("No track selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Service.PerformFFT(this.AudioH.CurrentTrack, (int) this.numericUpDown_chunkSize.Value, (float) this.numericUpDown_overlap.Value, this.checkBox_log.Checked);

			this.AudioH.RefreshView();
			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_kernelExecute_Click(object sender, EventArgs e)
		{
			string kernelBaseName = this.comboBox_kernelNames.SelectedItem?.ToString() ?? "";
			string kernelVersion = this.comboBox_kernelVersions.SelectedItem?.ToString() ?? "";
			if (string.IsNullOrEmpty(kernelBaseName) || string.IsNullOrEmpty(kernelVersion))
			{
				MessageBox.Show("Please select a kernel name and version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Load kernel
			this.Service.KernelCompiler?.LoadKernel(kernelBaseName + kernelVersion, "", null, this.checkBox_invariables.Checked);
			if (this.Service.KernelCompiler?.Kernel == null || this.Service.KernelCompiler.KernelFile == null)
			{
				MessageBox.Show("Failed to load kernel.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Decide if AUDIO / IMAGE kernel
			if (this.Service.KernelCompiler.KernelFile.Contains("\\Image\\"))
			{
				// Check current image
				if (this.ImageH.CurrentObject == null)
				{
					MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Exec
				this.Service.ExecuteImageKernel(
					this.ImageH.CurrentObject,
					kernelBaseName, kernelVersion,
					null,
					this.checkBox_log.Checked
				);

				this.ImageH.FillImagesListBox();
			}
			else if (this.Service.KernelCompiler.KernelFile.Contains("\\Audio\\"))
			{
				// Check current track
				if (this.AudioH.CurrentTrack == null)
				{
					MessageBox.Show("No track selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// Exec
				this.Service.ExecuteAudioKernel(
					this.AudioH.CurrentTrack,
					kernelBaseName, kernelVersion,
					(int) this.numericUpDown_chunkSize.Value,
					(float) this.numericUpDown_overlap.Value,
					null,
					this.checkBox_log.Checked
				);

				this.AudioH.RefreshView();
			}
			else
			{
				MessageBox.Show("Unknown kernel type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_waveCreate_Click(object sender, EventArgs e)
		{
			string wave = this.comboBox_waves.SelectedItem?.ToString() ?? "sin";
			int samplerate = (int) this.numericUpDown_waveSamplerate.Value;
			int duration = (int) this.numericUpDown_waveTime.Value;

			this.AudioH.CreateWaveform(wave, duration, samplerate, 2, 16, true);
		}

		private void button_normalize_Click(object sender, EventArgs e)
		{
			this.AudioH.CurrentTrack?.Normalize();

			this.AudioH.RefreshView();
		}

		private void button_reset_Click(object sender, EventArgs e)
		{
			this.AudioH.CurrentTrack?.Reload();

			this.AudioH.RefreshView();
		}

		private void numericUpDown_bpmStart_ValueChanged(object sender, EventArgs e)
		{
			// Adjust factor
			this.numericUpDown_stretchFactor.Value = this.numericUpDown_bpmStart.Value / this.numericUpDown_bpmTarget.Value;
		}

		private void numericUpDown_bpmTarget_ValueChanged(object sender, EventArgs e)
		{
			// Adjust factor
			this.numericUpDown_stretchFactor.Value = Math.Min(this.numericUpDown_stretchFactor.Maximum, this.numericUpDown_bpmStart.Value / this.numericUpDown_bpmTarget.Value);
		}

		private void numericUpDown_stretchFactor_ValueChanged(object sender, EventArgs e)
		{
			// Adjust target BPM
			if (this.numericUpDown_bpmStart.Value > 0)
			{
				this.numericUpDown_bpmTarget.Value = Math.Min(this.numericUpDown_bpmTarget.Maximum, Math.Max(this.numericUpDown_bpmTarget.Minimum, this.numericUpDown_bpmStart.Value / this.numericUpDown_stretchFactor.Value));
			}
			else
			{
				this.numericUpDown_bpmTarget.Value = 0;
			}
		}

		private void button_stretch_Click(object sender, EventArgs e)
		{
			string kernelName = this.comboBox_kernelsStretch.SelectedItem?.ToString() ?? "";
			if (string.IsNullOrEmpty(kernelName))
			{
				MessageBox.Show("Please select a kernel for timestretching.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Abort if no current track is selected
			if (this.AudioH.CurrentTrack == null)
			{
				MessageBox.Show("No track selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Get optional args
			Dictionary<string, object> optionalArgs = new()
			{
				{ "factor", (float) this.numericUpDown_stretchFactor.Value }
			};

			// Exec
			this.Service.ExecuteAudioKernel(
				this.AudioH.CurrentTrack,
				kernelName, "",
				(int) this.numericUpDown_chunkSize.Value,
				(float) this.numericUpDown_overlap.Value,
				optionalArgs,
				this.checkBox_log.Checked
			);

			this.AudioH.RefreshView();
			this.Service.FillPointers(this.listBox_pointers);
		}

		private void listBox_tracks_SelectedIndexChanged(object sender, EventArgs e)
		{
			float bpm = this.AudioH.CurrentTrack?.Bpm ?? 0.0f;

			this.numericUpDown_bpmStart.Value = bpm > 10 ? (decimal) bpm : 10;
		}

		private void button_scan_Click(object sender, EventArgs e)
		{
			// Abort if no current track is selected
			if (this.AudioH.CurrentTrack == null)
			{
				MessageBox.Show("No track selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string beatScanKernel = this.comboBox_kernelBeatScan.SelectedItem?.ToString() ?? "";
			int chunkSize = (int) this.numericUpDown_chunkSize.Value;
			float overlap = (float) this.numericUpDown_overlap.Value;
			Dictionary<string, object> optionalArguments = new()
			{
				{ "minFreq", (float) this.numericUpDown_minFreq.Value },
				{ "maxFreq", (float) this.numericUpDown_maxFreq.Value }
			};

			float bpm = this.Service.ExecuteBeatScan(this.AudioH.CurrentTrack, beatScanKernel, "", chunkSize, overlap, optionalArguments, this.checkBox_log.Checked);

			this.textBox_beatScan.Text = bpm > 0 ? bpm.ToString("F5") + " BPM" : "Failed.";

			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_moveImage_Click(object sender, EventArgs e)
		{
			// Check current image
			if (this.ImageH.CurrentObject == null)
			{
				MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			this.Service.MoveImage(this.ImageH.CurrentObject, this.checkBox_log.Checked);

			this.ImageH.FillImagesListBox();
			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_importImage_Click(object sender, EventArgs e)
		{
			this.ImageH.ImportImage();

			this.ImageH.FillImagesListBox();
			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_exportImage_Click(object sender, EventArgs e)
		{
			this.ImageH.CurrentObject?.Export();

			this.ImageH.FillImagesListBox();
		}

		private void button_resetImage_Click(object sender, EventArgs e)
		{
			this.ImageH.CurrentObject?.ResetImage();
			this.ImageH.FitZoom();

			this.ImageH.FillImagesListBox();
			this.Service.FillPointers(this.listBox_pointers);
		}

		private void button_renderVideo_Click(object sender, EventArgs e)
		{
			// Check current track
			if (this.AudioH.CurrentTrack == null)
			{
				MessageBox.Show("No audio track selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string exportPath = Path.Combine(this.Repopath, "Resources", "ExportedVideos");
			int frameRate = (int) this.numericUpDown_framerate.Value;
			float threshold = 0.7f;
			double minZoom = 1000;
			double maxZoom = 10000;
			double zoomMulti = 1.05d;

			double[] zooms = this.Service.ExecuteBeatZoom(this.AudioH.CurrentTrack, "beatZoom", "01", 0, this.AudioH.CurrentTrack.Samplerate, frameRate, threshold, minZoom, maxZoom, zoomMulti, this.checkBox_log.Checked);



			this.VideoH.RenderVideo(exportPath, "export", ".mp4", frameRate);
		}
	}
}
