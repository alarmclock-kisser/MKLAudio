namespace MKLAudio
{
    partial class WindowMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.comboBox_devices = new ComboBox();
			this.listBox_log = new ListBox();
			this.listBox_tracks = new ListBox();
			this.button_play = new Button();
			this.textBox_time = new TextBox();
			this.pictureBox_wave = new PictureBox();
			this.vScrollBar_volume = new VScrollBar();
			this.hScrollBar_offset = new HScrollBar();
			this.numericUpDown_zoom = new NumericUpDown();
			this.button_import = new Button();
			this.button_export = new Button();
			this.label_meta = new Label();
			this.button_move = new Button();
			this.comboBox_kernelNames = new ComboBox();
			this.comboBox_kernelVersions = new ComboBox();
			this.button_kernelLoad = new Button();
			this.panel_kernelArguments = new Panel();
			this.checkBox_invariables = new CheckBox();
			this.numericUpDown_overlap = new NumericUpDown();
			this.numericUpDown_chunkSize = new NumericUpDown();
			this.listBox_pointers = new ListBox();
			this.button_fft = new Button();
			this.checkBox_log = new CheckBox();
			this.button_kernelExecute = new Button();
			this.groupBox_waves = new GroupBox();
			this.label_info_waveTime = new Label();
			this.numericUpDown_waveSamplerate = new NumericUpDown();
			this.label_info_waveType = new Label();
			this.label_info_waveSamplerate = new Label();
			this.numericUpDown_waveTime = new NumericUpDown();
			this.comboBox_waves = new ComboBox();
			this.button_waveCreate = new Button();
			this.button_normalize = new Button();
			this.button_reset = new Button();
			this.groupBox_stretching = new GroupBox();
			this.comboBox_kernelsStretch = new ComboBox();
			this.button_stretch = new Button();
			this.label_info_stretchFactor = new Label();
			this.numericUpDown_stretchFactor = new NumericUpDown();
			this.label_info_targetBpm = new Label();
			this.numericUpDown_bpmTarget = new NumericUpDown();
			this.label_info_startBpm = new Label();
			this.numericUpDown_bpmStart = new NumericUpDown();
			this.groupBox_beatScan = new GroupBox();
			this.label_info_maxFreq = new Label();
			this.label_info_minFreq = new Label();
			this.numericUpDown_maxFreq = new NumericUpDown();
			this.numericUpDown_minFreq = new NumericUpDown();
			this.comboBox_kernelBeatScan = new ComboBox();
			this.textBox_beatScan = new TextBox();
			this.button_scan = new Button();
			this.label_imageMeta = new Label();
			this.panel_view = new Panel();
			this.pictureBox_image = new PictureBox();
			this.listBox_images = new ListBox();
			this.button_resetImage = new Button();
			this.button_exportImage = new Button();
			this.button_importImage = new Button();
			this.button_moveImage = new Button();
			this.label_info_overlap = new Label();
			this.label_info_chunkSize = new Label();
			this.numericUpDown_zoomImage = new NumericUpDown();
			this.button_info = new Button();
			this.groupBox_video = new GroupBox();
			this.button_renderVideo = new Button();
			this.label_info_fps = new Label();
			this.numericUpDown_framerate = new NumericUpDown();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_wave).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).BeginInit();
			this.groupBox_waves.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_waveSamplerate).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_waveTime).BeginInit();
			this.groupBox_stretching.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_stretchFactor).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmTarget).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmStart).BeginInit();
			this.groupBox_beatScan.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_maxFreq).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_minFreq).BeginInit();
			this.panel_view.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_image).BeginInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoomImage).BeginInit();
			this.groupBox_video.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_framerate).BeginInit();
			this.SuspendLayout();
			// 
			// comboBox_devices
			// 
			this.comboBox_devices.FormattingEnabled = true;
			this.comboBox_devices.Location = new Point(12, 12);
			this.comboBox_devices.Name = "comboBox_devices";
			this.comboBox_devices.Size = new Size(400, 23);
			this.comboBox_devices.TabIndex = 0;
			// 
			// listBox_log
			// 
			this.listBox_log.FormattingEnabled = true;
			this.listBox_log.ItemHeight = 15;
			this.listBox_log.Location = new Point(12, 670);
			this.listBox_log.Name = "listBox_log";
			this.listBox_log.Size = new Size(1200, 124);
			this.listBox_log.TabIndex = 1;
			// 
			// listBox_tracks
			// 
			this.listBox_tracks.FormattingEnabled = true;
			this.listBox_tracks.ItemHeight = 15;
			this.listBox_tracks.Location = new Point(1692, 670);
			this.listBox_tracks.Name = "listBox_tracks";
			this.listBox_tracks.Size = new Size(200, 139);
			this.listBox_tracks.TabIndex = 2;
			this.listBox_tracks.SelectedIndexChanged += this.listBox_tracks_SelectedIndexChanged;
			// 
			// button_play
			// 
			this.button_play.Location = new Point(1692, 641);
			this.button_play.Name = "button_play";
			this.button_play.Size = new Size(23, 23);
			this.button_play.TabIndex = 3;
			this.button_play.Text = ">";
			this.button_play.UseVisualStyleBackColor = true;
			// 
			// textBox_time
			// 
			this.textBox_time.Location = new Point(1721, 641);
			this.textBox_time.Name = "textBox_time";
			this.textBox_time.PlaceholderText = "0:00:00.000";
			this.textBox_time.Size = new Size(80, 23);
			this.textBox_time.TabIndex = 4;
			// 
			// pictureBox_wave
			// 
			this.pictureBox_wave.Location = new Point(12, 527);
			this.pictureBox_wave.Name = "pictureBox_wave";
			this.pictureBox_wave.Size = new Size(1180, 120);
			this.pictureBox_wave.TabIndex = 5;
			this.pictureBox_wave.TabStop = false;
			// 
			// vScrollBar_volume
			// 
			this.vScrollBar_volume.Location = new Point(1195, 527);
			this.vScrollBar_volume.Name = "vScrollBar_volume";
			this.vScrollBar_volume.Size = new Size(17, 120);
			this.vScrollBar_volume.TabIndex = 6;
			// 
			// hScrollBar_offset
			// 
			this.hScrollBar_offset.Location = new Point(12, 650);
			this.hScrollBar_offset.Name = "hScrollBar_offset";
			this.hScrollBar_offset.Size = new Size(1200, 17);
			this.hScrollBar_offset.TabIndex = 7;
			// 
			// numericUpDown_zoom
			// 
			this.numericUpDown_zoom.Location = new Point(1807, 641);
			this.numericUpDown_zoom.Maximum = new decimal(new int[] { 8192, 0, 0, 0 });
			this.numericUpDown_zoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_zoom.Name = "numericUpDown_zoom";
			this.numericUpDown_zoom.Size = new Size(85, 23);
			this.numericUpDown_zoom.TabIndex = 8;
			this.numericUpDown_zoom.Value = new decimal(new int[] { 128, 0, 0, 0 });
			// 
			// button_import
			// 
			this.button_import.Location = new Point(1631, 717);
			this.button_import.Name = "button_import";
			this.button_import.Size = new Size(55, 23);
			this.button_import.TabIndex = 9;
			this.button_import.Text = "Import";
			this.button_import.UseVisualStyleBackColor = true;
			this.button_import.Click += this.button_import_Click;
			// 
			// button_export
			// 
			this.button_export.Location = new Point(1631, 746);
			this.button_export.Name = "button_export";
			this.button_export.Size = new Size(55, 23);
			this.button_export.TabIndex = 10;
			this.button_export.Text = "Export";
			this.button_export.UseVisualStyleBackColor = true;
			this.button_export.Click += this.button_export_Click;
			// 
			// label_meta
			// 
			this.label_meta.AutoSize = true;
			this.label_meta.Location = new Point(12, 509);
			this.label_meta.Name = "label_meta";
			this.label_meta.Size = new Size(148, 15);
			this.label_meta.TabIndex = 11;
			this.label_meta.Text = "No track selected / loaded.";
			// 
			// button_move
			// 
			this.button_move.Location = new Point(1631, 670);
			this.button_move.Name = "button_move";
			this.button_move.Size = new Size(55, 23);
			this.button_move.TabIndex = 12;
			this.button_move.Text = "Move";
			this.button_move.UseVisualStyleBackColor = true;
			this.button_move.Click += this.button_move_Click;
			// 
			// comboBox_kernelNames
			// 
			this.comboBox_kernelNames.FormattingEnabled = true;
			this.comboBox_kernelNames.Location = new Point(1470, 12);
			this.comboBox_kernelNames.Name = "comboBox_kernelNames";
			this.comboBox_kernelNames.Size = new Size(300, 23);
			this.comboBox_kernelNames.TabIndex = 13;
			this.comboBox_kernelNames.Text = "Select OpenCL-Kernel ...";
			this.comboBox_kernelNames.SelectedIndexChanged += this.comboBox_kernelNames_SelectedIndexChanged;
			// 
			// comboBox_kernelVersions
			// 
			this.comboBox_kernelVersions.FormattingEnabled = true;
			this.comboBox_kernelVersions.Location = new Point(1776, 12);
			this.comboBox_kernelVersions.Name = "comboBox_kernelVersions";
			this.comboBox_kernelVersions.Size = new Size(60, 23);
			this.comboBox_kernelVersions.TabIndex = 14;
			this.comboBox_kernelVersions.Text = "Ver.";
			// 
			// button_kernelLoad
			// 
			this.button_kernelLoad.Location = new Point(1842, 12);
			this.button_kernelLoad.Name = "button_kernelLoad";
			this.button_kernelLoad.Size = new Size(50, 23);
			this.button_kernelLoad.TabIndex = 15;
			this.button_kernelLoad.Text = "Load";
			this.button_kernelLoad.UseVisualStyleBackColor = true;
			this.button_kernelLoad.Click += this.button_kernelLoad_Click;
			// 
			// panel_kernelArguments
			// 
			this.panel_kernelArguments.BackColor = Color.Gainsboro;
			this.panel_kernelArguments.Location = new Point(1470, 41);
			this.panel_kernelArguments.Name = "panel_kernelArguments";
			this.panel_kernelArguments.Size = new Size(300, 300);
			this.panel_kernelArguments.TabIndex = 16;
			// 
			// checkBox_invariables
			// 
			this.checkBox_invariables.AutoSize = true;
			this.checkBox_invariables.Location = new Point(1776, 41);
			this.checkBox_invariables.Name = "checkBox_invariables";
			this.checkBox_invariables.Size = new Size(119, 19);
			this.checkBox_invariables.TabIndex = 17;
			this.checkBox_invariables.Text = "Show invariables?";
			this.checkBox_invariables.UseVisualStyleBackColor = true;
			this.checkBox_invariables.CheckedChanged += this.checkBox_invariables_CheckedChanged;
			// 
			// numericUpDown_overlap
			// 
			this.numericUpDown_overlap.DecimalPlaces = 3;
			this.numericUpDown_overlap.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
			this.numericUpDown_overlap.Location = new Point(1807, 612);
			this.numericUpDown_overlap.Maximum = new decimal(new int[] { 9, 0, 0, 65536 });
			this.numericUpDown_overlap.Name = "numericUpDown_overlap";
			this.numericUpDown_overlap.Size = new Size(85, 23);
			this.numericUpDown_overlap.TabIndex = 18;
			this.numericUpDown_overlap.Value = new decimal(new int[] { 5, 0, 0, 65536 });
			// 
			// numericUpDown_chunkSize
			// 
			this.numericUpDown_chunkSize.Location = new Point(1721, 612);
			this.numericUpDown_chunkSize.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
			this.numericUpDown_chunkSize.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
			this.numericUpDown_chunkSize.Name = "numericUpDown_chunkSize";
			this.numericUpDown_chunkSize.Size = new Size(80, 23);
			this.numericUpDown_chunkSize.TabIndex = 19;
			this.numericUpDown_chunkSize.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// listBox_pointers
			// 
			this.listBox_pointers.FormattingEnabled = true;
			this.listBox_pointers.ItemHeight = 15;
			this.listBox_pointers.Location = new Point(1485, 670);
			this.listBox_pointers.Name = "listBox_pointers";
			this.listBox_pointers.Size = new Size(140, 139);
			this.listBox_pointers.TabIndex = 20;
			// 
			// button_fft
			// 
			this.button_fft.Location = new Point(341, 21);
			this.button_fft.Name = "button_fft";
			this.button_fft.Size = new Size(75, 23);
			this.button_fft.TabIndex = 21;
			this.button_fft.Text = "(I)FFT";
			this.button_fft.UseVisualStyleBackColor = true;
			this.button_fft.Click += this.button_fft_Click;
			// 
			// checkBox_log
			// 
			this.checkBox_log.AutoSize = true;
			this.checkBox_log.Checked = true;
			this.checkBox_log.CheckState = CheckState.Checked;
			this.checkBox_log.Location = new Point(12, 800);
			this.checkBox_log.Name = "checkBox_log";
			this.checkBox_log.Size = new Size(51, 19);
			this.checkBox_log.TabIndex = 22;
			this.checkBox_log.Text = "Log?";
			this.checkBox_log.UseVisualStyleBackColor = true;
			// 
			// button_kernelExecute
			// 
			this.button_kernelExecute.Location = new Point(1776, 318);
			this.button_kernelExecute.Name = "button_kernelExecute";
			this.button_kernelExecute.Size = new Size(116, 23);
			this.button_kernelExecute.TabIndex = 23;
			this.button_kernelExecute.Text = "EXECUTE";
			this.button_kernelExecute.UseVisualStyleBackColor = true;
			this.button_kernelExecute.Click += this.button_kernelExecute_Click;
			// 
			// groupBox_waves
			// 
			this.groupBox_waves.Controls.Add(this.label_info_waveTime);
			this.groupBox_waves.Controls.Add(this.numericUpDown_waveSamplerate);
			this.groupBox_waves.Controls.Add(this.label_info_waveType);
			this.groupBox_waves.Controls.Add(this.label_info_waveSamplerate);
			this.groupBox_waves.Controls.Add(this.numericUpDown_waveTime);
			this.groupBox_waves.Controls.Add(this.comboBox_waves);
			this.groupBox_waves.Controls.Add(this.button_waveCreate);
			this.groupBox_waves.Location = new Point(1281, 187);
			this.groupBox_waves.Name = "groupBox_waves";
			this.groupBox_waves.Size = new Size(183, 112);
			this.groupBox_waves.TabIndex = 24;
			this.groupBox_waves.TabStop = false;
			this.groupBox_waves.Text = "Waves";
			// 
			// label_info_waveTime
			// 
			this.label_info_waveTime.AutoSize = true;
			this.label_info_waveTime.Location = new Point(92, 85);
			this.label_info_waveTime.Name = "label_info_waveTime";
			this.label_info_waveTime.Size = new Size(27, 15);
			this.label_info_waveTime.TabIndex = 25;
			this.label_info_waveTime.Text = "sec.";
			// 
			// numericUpDown_waveSamplerate
			// 
			this.numericUpDown_waveSamplerate.Increment = new decimal(new int[] { 100, 0, 0, 0 });
			this.numericUpDown_waveSamplerate.Location = new Point(6, 51);
			this.numericUpDown_waveSamplerate.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
			this.numericUpDown_waveSamplerate.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
			this.numericUpDown_waveSamplerate.Name = "numericUpDown_waveSamplerate";
			this.numericUpDown_waveSamplerate.Size = new Size(80, 23);
			this.numericUpDown_waveSamplerate.TabIndex = 25;
			this.numericUpDown_waveSamplerate.Value = new decimal(new int[] { 44100, 0, 0, 0 });
			// 
			// label_info_waveType
			// 
			this.label_info_waveType.AutoSize = true;
			this.label_info_waveType.Location = new Point(92, 25);
			this.label_info_waveType.Name = "label_info_waveType";
			this.label_info_waveType.Size = new Size(62, 15);
			this.label_info_waveType.TabIndex = 28;
			this.label_info_waveType.Text = "Wave type";
			// 
			// label_info_waveSamplerate
			// 
			this.label_info_waveSamplerate.AutoSize = true;
			this.label_info_waveSamplerate.Location = new Point(92, 53);
			this.label_info_waveSamplerate.Name = "label_info_waveSamplerate";
			this.label_info_waveSamplerate.Size = new Size(66, 15);
			this.label_info_waveSamplerate.TabIndex = 25;
			this.label_info_waveSamplerate.Text = "Samplerate";
			// 
			// numericUpDown_waveTime
			// 
			this.numericUpDown_waveTime.Location = new Point(6, 83);
			this.numericUpDown_waveTime.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
			this.numericUpDown_waveTime.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_waveTime.Name = "numericUpDown_waveTime";
			this.numericUpDown_waveTime.Size = new Size(80, 23);
			this.numericUpDown_waveTime.TabIndex = 27;
			this.numericUpDown_waveTime.Value = new decimal(new int[] { 10, 0, 0, 0 });
			// 
			// comboBox_waves
			// 
			this.comboBox_waves.FormattingEnabled = true;
			this.comboBox_waves.Items.AddRange(new object[] { "Sin", "Square", "Saw", "Noise" });
			this.comboBox_waves.Location = new Point(6, 22);
			this.comboBox_waves.Name = "comboBox_waves";
			this.comboBox_waves.Size = new Size(80, 23);
			this.comboBox_waves.TabIndex = 26;
			this.comboBox_waves.Text = "Sin";
			// 
			// button_waveCreate
			// 
			this.button_waveCreate.Location = new Point(122, 83);
			this.button_waveCreate.Name = "button_waveCreate";
			this.button_waveCreate.Size = new Size(55, 23);
			this.button_waveCreate.TabIndex = 25;
			this.button_waveCreate.Text = "Create";
			this.button_waveCreate.UseVisualStyleBackColor = true;
			this.button_waveCreate.Click += this.button_waveCreate_Click;
			// 
			// button_normalize
			// 
			this.button_normalize.Location = new Point(341, 50);
			this.button_normalize.Name = "button_normalize";
			this.button_normalize.Size = new Size(75, 23);
			this.button_normalize.TabIndex = 25;
			this.button_normalize.Text = "Normalize";
			this.button_normalize.UseVisualStyleBackColor = true;
			this.button_normalize.Click += this.button_normalize_Click;
			// 
			// button_reset
			// 
			this.button_reset.Location = new Point(1631, 786);
			this.button_reset.Name = "button_reset";
			this.button_reset.Size = new Size(55, 23);
			this.button_reset.TabIndex = 26;
			this.button_reset.Text = "Reset";
			this.button_reset.UseVisualStyleBackColor = true;
			this.button_reset.Click += this.button_reset_Click;
			// 
			// groupBox_stretching
			// 
			this.groupBox_stretching.Controls.Add(this.comboBox_kernelsStretch);
			this.groupBox_stretching.Controls.Add(this.button_stretch);
			this.groupBox_stretching.Controls.Add(this.label_info_stretchFactor);
			this.groupBox_stretching.Controls.Add(this.numericUpDown_stretchFactor);
			this.groupBox_stretching.Controls.Add(this.label_info_targetBpm);
			this.groupBox_stretching.Controls.Add(this.numericUpDown_bpmTarget);
			this.groupBox_stretching.Controls.Add(this.label_info_startBpm);
			this.groupBox_stretching.Controls.Add(this.numericUpDown_bpmStart);
			this.groupBox_stretching.Controls.Add(this.button_fft);
			this.groupBox_stretching.Controls.Add(this.button_normalize);
			this.groupBox_stretching.Location = new Point(1470, 347);
			this.groupBox_stretching.Name = "groupBox_stretching";
			this.groupBox_stretching.Size = new Size(422, 140);
			this.groupBox_stretching.TabIndex = 27;
			this.groupBox_stretching.TabStop = false;
			this.groupBox_stretching.Text = "Time-stretching";
			// 
			// comboBox_kernelsStretch
			// 
			this.comboBox_kernelsStretch.FormattingEnabled = true;
			this.comboBox_kernelsStretch.Location = new Point(6, 22);
			this.comboBox_kernelsStretch.Name = "comboBox_kernelsStretch";
			this.comboBox_kernelsStretch.Size = new Size(239, 23);
			this.comboBox_kernelsStretch.TabIndex = 35;
			this.comboBox_kernelsStretch.Text = "Select stretching kernel ...";
			// 
			// button_stretch
			// 
			this.button_stretch.Location = new Point(341, 111);
			this.button_stretch.Name = "button_stretch";
			this.button_stretch.Size = new Size(75, 23);
			this.button_stretch.TabIndex = 34;
			this.button_stretch.Text = "Stretch";
			this.button_stretch.UseVisualStyleBackColor = true;
			this.button_stretch.Click += this.button_stretch_Click;
			// 
			// label_info_stretchFactor
			// 
			this.label_info_stretchFactor.AutoSize = true;
			this.label_info_stretchFactor.Location = new Point(160, 93);
			this.label_info_stretchFactor.Name = "label_info_stretchFactor";
			this.label_info_stretchFactor.Size = new Size(81, 15);
			this.label_info_stretchFactor.TabIndex = 33;
			this.label_info_stretchFactor.Text = "Stretch factor:";
			// 
			// numericUpDown_stretchFactor
			// 
			this.numericUpDown_stretchFactor.DecimalPlaces = 6;
			this.numericUpDown_stretchFactor.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
			this.numericUpDown_stretchFactor.Location = new Point(160, 111);
			this.numericUpDown_stretchFactor.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
			this.numericUpDown_stretchFactor.Minimum = new decimal(new int[] { 5, 0, 0, 196608 });
			this.numericUpDown_stretchFactor.Name = "numericUpDown_stretchFactor";
			this.numericUpDown_stretchFactor.Size = new Size(85, 23);
			this.numericUpDown_stretchFactor.TabIndex = 32;
			this.numericUpDown_stretchFactor.Value = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_stretchFactor.ValueChanged += this.numericUpDown_stretchFactor_ValueChanged;
			// 
			// label_info_targetBpm
			// 
			this.label_info_targetBpm.AutoSize = true;
			this.label_info_targetBpm.Location = new Point(83, 93);
			this.label_info_targetBpm.Name = "label_info_targetBpm";
			this.label_info_targetBpm.Size = new Size(71, 15);
			this.label_info_targetBpm.TabIndex = 31;
			this.label_info_targetBpm.Text = "Target BPM:";
			// 
			// numericUpDown_bpmTarget
			// 
			this.numericUpDown_bpmTarget.DecimalPlaces = 3;
			this.numericUpDown_bpmTarget.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
			this.numericUpDown_bpmTarget.Location = new Point(83, 111);
			this.numericUpDown_bpmTarget.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
			this.numericUpDown_bpmTarget.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
			this.numericUpDown_bpmTarget.Name = "numericUpDown_bpmTarget";
			this.numericUpDown_bpmTarget.Size = new Size(71, 23);
			this.numericUpDown_bpmTarget.TabIndex = 30;
			this.numericUpDown_bpmTarget.Value = new decimal(new int[] { 150, 0, 0, 0 });
			this.numericUpDown_bpmTarget.ValueChanged += this.numericUpDown_bpmTarget_ValueChanged;
			// 
			// label_info_startBpm
			// 
			this.label_info_startBpm.AutoSize = true;
			this.label_info_startBpm.Location = new Point(6, 93);
			this.label_info_startBpm.Name = "label_info_startBpm";
			this.label_info_startBpm.Size = new Size(62, 15);
			this.label_info_startBpm.TabIndex = 29;
			this.label_info_startBpm.Text = "Start BPM:";
			// 
			// numericUpDown_bpmStart
			// 
			this.numericUpDown_bpmStart.DecimalPlaces = 3;
			this.numericUpDown_bpmStart.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
			this.numericUpDown_bpmStart.Location = new Point(6, 111);
			this.numericUpDown_bpmStart.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
			this.numericUpDown_bpmStart.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
			this.numericUpDown_bpmStart.Name = "numericUpDown_bpmStart";
			this.numericUpDown_bpmStart.Size = new Size(71, 23);
			this.numericUpDown_bpmStart.TabIndex = 28;
			this.numericUpDown_bpmStart.Value = new decimal(new int[] { 150, 0, 0, 0 });
			this.numericUpDown_bpmStart.ValueChanged += this.numericUpDown_bpmStart_ValueChanged;
			// 
			// groupBox_beatScan
			// 
			this.groupBox_beatScan.Controls.Add(this.label_info_maxFreq);
			this.groupBox_beatScan.Controls.Add(this.label_info_minFreq);
			this.groupBox_beatScan.Controls.Add(this.numericUpDown_maxFreq);
			this.groupBox_beatScan.Controls.Add(this.numericUpDown_minFreq);
			this.groupBox_beatScan.Controls.Add(this.comboBox_kernelBeatScan);
			this.groupBox_beatScan.Controls.Add(this.textBox_beatScan);
			this.groupBox_beatScan.Controls.Add(this.button_scan);
			this.groupBox_beatScan.Location = new Point(1281, 41);
			this.groupBox_beatScan.Name = "groupBox_beatScan";
			this.groupBox_beatScan.Size = new Size(183, 140);
			this.groupBox_beatScan.TabIndex = 28;
			this.groupBox_beatScan.TabStop = false;
			this.groupBox_beatScan.Text = "Beat-scan";
			// 
			// label_info_maxFreq
			// 
			this.label_info_maxFreq.AutoSize = true;
			this.label_info_maxFreq.Location = new Point(97, 64);
			this.label_info_maxFreq.Name = "label_info_maxFreq";
			this.label_info_maxFreq.Size = new Size(59, 15);
			this.label_info_maxFreq.TabIndex = 29;
			this.label_info_maxFreq.Text = "Max. freq.";
			// 
			// label_info_minFreq
			// 
			this.label_info_minFreq.AutoSize = true;
			this.label_info_minFreq.Location = new Point(6, 64);
			this.label_info_minFreq.Name = "label_info_minFreq";
			this.label_info_minFreq.Size = new Size(58, 15);
			this.label_info_minFreq.TabIndex = 29;
			this.label_info_minFreq.Text = "Min. freq.";
			// 
			// numericUpDown_maxFreq
			// 
			this.numericUpDown_maxFreq.DecimalPlaces = 2;
			this.numericUpDown_maxFreq.Location = new Point(97, 82);
			this.numericUpDown_maxFreq.Maximum = new decimal(new int[] { 24000, 0, 0, 0 });
			this.numericUpDown_maxFreq.Name = "numericUpDown_maxFreq";
			this.numericUpDown_maxFreq.Size = new Size(80, 23);
			this.numericUpDown_maxFreq.TabIndex = 30;
			this.numericUpDown_maxFreq.Value = new decimal(new int[] { 150, 0, 0, 0 });
			// 
			// numericUpDown_minFreq
			// 
			this.numericUpDown_minFreq.DecimalPlaces = 2;
			this.numericUpDown_minFreq.Location = new Point(6, 82);
			this.numericUpDown_minFreq.Maximum = new decimal(new int[] { 24000, 0, 0, 0 });
			this.numericUpDown_minFreq.Name = "numericUpDown_minFreq";
			this.numericUpDown_minFreq.Size = new Size(80, 23);
			this.numericUpDown_minFreq.TabIndex = 29;
			this.numericUpDown_minFreq.Value = new decimal(new int[] { 20, 0, 0, 0 });
			// 
			// comboBox_kernelBeatScan
			// 
			this.comboBox_kernelBeatScan.FormattingEnabled = true;
			this.comboBox_kernelBeatScan.Location = new Point(6, 22);
			this.comboBox_kernelBeatScan.Name = "comboBox_kernelBeatScan";
			this.comboBox_kernelBeatScan.Size = new Size(171, 23);
			this.comboBox_kernelBeatScan.TabIndex = 2;
			this.comboBox_kernelBeatScan.Text = "Select beat-scan kernel ...";
			// 
			// textBox_beatScan
			// 
			this.textBox_beatScan.Location = new Point(6, 111);
			this.textBox_beatScan.Name = "textBox_beatScan";
			this.textBox_beatScan.PlaceholderText = "0,000 BPM";
			this.textBox_beatScan.ReadOnly = true;
			this.textBox_beatScan.Size = new Size(113, 23);
			this.textBox_beatScan.TabIndex = 1;
			// 
			// button_scan
			// 
			this.button_scan.Location = new Point(122, 111);
			this.button_scan.Name = "button_scan";
			this.button_scan.Size = new Size(55, 23);
			this.button_scan.TabIndex = 0;
			this.button_scan.Text = "Scan";
			this.button_scan.UseVisualStyleBackColor = true;
			this.button_scan.Click += this.button_scan_Click;
			// 
			// label_imageMeta
			// 
			this.label_imageMeta.AutoSize = true;
			this.label_imageMeta.Location = new Point(12, 494);
			this.label_imageMeta.Name = "label_imageMeta";
			this.label_imageMeta.Size = new Size(155, 15);
			this.label_imageMeta.TabIndex = 29;
			this.label_imageMeta.Text = "No image selected / loaded.";
			// 
			// panel_view
			// 
			this.panel_view.Controls.Add(this.pictureBox_image);
			this.panel_view.Location = new Point(12, 41);
			this.panel_view.Name = "panel_view";
			this.panel_view.Size = new Size(1200, 450);
			this.panel_view.TabIndex = 30;
			// 
			// pictureBox_image
			// 
			this.pictureBox_image.Location = new Point(3, 3);
			this.pictureBox_image.Name = "pictureBox_image";
			this.pictureBox_image.Size = new Size(1194, 444);
			this.pictureBox_image.TabIndex = 0;
			this.pictureBox_image.TabStop = false;
			// 
			// listBox_images
			// 
			this.listBox_images.FormattingEnabled = true;
			this.listBox_images.ItemHeight = 15;
			this.listBox_images.Location = new Point(1218, 670);
			this.listBox_images.Name = "listBox_images";
			this.listBox_images.Size = new Size(200, 139);
			this.listBox_images.TabIndex = 31;
			// 
			// button_resetImage
			// 
			this.button_resetImage.Location = new Point(1424, 786);
			this.button_resetImage.Name = "button_resetImage";
			this.button_resetImage.Size = new Size(55, 23);
			this.button_resetImage.TabIndex = 34;
			this.button_resetImage.Text = "Reset";
			this.button_resetImage.UseVisualStyleBackColor = true;
			this.button_resetImage.Click += this.button_resetImage_Click;
			// 
			// button_exportImage
			// 
			this.button_exportImage.Location = new Point(1424, 746);
			this.button_exportImage.Name = "button_exportImage";
			this.button_exportImage.Size = new Size(55, 23);
			this.button_exportImage.TabIndex = 33;
			this.button_exportImage.Text = "Export";
			this.button_exportImage.UseVisualStyleBackColor = true;
			this.button_exportImage.Click += this.button_exportImage_Click;
			// 
			// button_importImage
			// 
			this.button_importImage.Location = new Point(1424, 717);
			this.button_importImage.Name = "button_importImage";
			this.button_importImage.Size = new Size(55, 23);
			this.button_importImage.TabIndex = 32;
			this.button_importImage.Text = "Import";
			this.button_importImage.UseVisualStyleBackColor = true;
			this.button_importImage.Click += this.button_importImage_Click;
			// 
			// button_moveImage
			// 
			this.button_moveImage.Location = new Point(1424, 670);
			this.button_moveImage.Name = "button_moveImage";
			this.button_moveImage.Size = new Size(55, 23);
			this.button_moveImage.TabIndex = 35;
			this.button_moveImage.Text = "Move";
			this.button_moveImage.UseVisualStyleBackColor = true;
			this.button_moveImage.Click += this.button_moveImage_Click;
			// 
			// label_info_overlap
			// 
			this.label_info_overlap.AutoSize = true;
			this.label_info_overlap.Location = new Point(1807, 594);
			this.label_info_overlap.Name = "label_info_overlap";
			this.label_info_overlap.Size = new Size(61, 15);
			this.label_info_overlap.TabIndex = 36;
			this.label_info_overlap.Text = "Overlap %";
			// 
			// label_info_chunkSize
			// 
			this.label_info_chunkSize.AutoSize = true;
			this.label_info_chunkSize.Location = new Point(1721, 594);
			this.label_info_chunkSize.Name = "label_info_chunkSize";
			this.label_info_chunkSize.Size = new Size(64, 15);
			this.label_info_chunkSize.TabIndex = 37;
			this.label_info_chunkSize.Text = "Chunk size";
			// 
			// numericUpDown_zoomImage
			// 
			this.numericUpDown_zoomImage.Location = new Point(1147, 497);
			this.numericUpDown_zoomImage.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			this.numericUpDown_zoomImage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_zoomImage.Name = "numericUpDown_zoomImage";
			this.numericUpDown_zoomImage.Size = new Size(65, 23);
			this.numericUpDown_zoomImage.TabIndex = 38;
			this.numericUpDown_zoomImage.Value = new decimal(new int[] { 100, 0, 0, 0 });
			// 
			// button_info
			// 
			this.button_info.Location = new Point(418, 11);
			this.button_info.Name = "button_info";
			this.button_info.Size = new Size(23, 23);
			this.button_info.TabIndex = 39;
			this.button_info.Text = "i";
			this.button_info.UseVisualStyleBackColor = true;
			this.button_info.Click += this.button_info_Click;
			// 
			// groupBox_video
			// 
			this.groupBox_video.Controls.Add(this.button_renderVideo);
			this.groupBox_video.Controls.Add(this.label_info_fps);
			this.groupBox_video.Controls.Add(this.numericUpDown_framerate);
			this.groupBox_video.Location = new Point(1218, 347);
			this.groupBox_video.Name = "groupBox_video";
			this.groupBox_video.Size = new Size(246, 300);
			this.groupBox_video.TabIndex = 40;
			this.groupBox_video.TabStop = false;
			this.groupBox_video.Text = "Video rendering";
			// 
			// button_renderVideo
			// 
			this.button_renderVideo.Location = new Point(165, 271);
			this.button_renderVideo.Name = "button_renderVideo";
			this.button_renderVideo.Size = new Size(75, 23);
			this.button_renderVideo.TabIndex = 2;
			this.button_renderVideo.Text = "Render";
			this.button_renderVideo.UseVisualStyleBackColor = true;
			this.button_renderVideo.Click += this.button_renderVideo_Click;
			// 
			// label_info_fps
			// 
			this.label_info_fps.AutoSize = true;
			this.label_info_fps.Location = new Point(6, 253);
			this.label_info_fps.Name = "label_info_fps";
			this.label_info_fps.Size = new Size(26, 15);
			this.label_info_fps.TabIndex = 1;
			this.label_info_fps.Text = "FPS";
			// 
			// numericUpDown_framerate
			// 
			this.numericUpDown_framerate.Location = new Point(6, 271);
			this.numericUpDown_framerate.Maximum = new decimal(new int[] { 144, 0, 0, 0 });
			this.numericUpDown_framerate.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.numericUpDown_framerate.Name = "numericUpDown_framerate";
			this.numericUpDown_framerate.Size = new Size(50, 23);
			this.numericUpDown_framerate.TabIndex = 0;
			this.numericUpDown_framerate.Value = new decimal(new int[] { 20, 0, 0, 0 });
			// 
			// WindowMain
			// 
			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new Size(1904, 821);
			this.Controls.Add(this.groupBox_video);
			this.Controls.Add(this.button_info);
			this.Controls.Add(this.numericUpDown_zoomImage);
			this.Controls.Add(this.label_info_chunkSize);
			this.Controls.Add(this.label_info_overlap);
			this.Controls.Add(this.button_moveImage);
			this.Controls.Add(this.button_resetImage);
			this.Controls.Add(this.button_exportImage);
			this.Controls.Add(this.button_importImage);
			this.Controls.Add(this.listBox_images);
			this.Controls.Add(this.panel_view);
			this.Controls.Add(this.label_imageMeta);
			this.Controls.Add(this.groupBox_beatScan);
			this.Controls.Add(this.groupBox_stretching);
			this.Controls.Add(this.button_reset);
			this.Controls.Add(this.groupBox_waves);
			this.Controls.Add(this.button_kernelExecute);
			this.Controls.Add(this.checkBox_log);
			this.Controls.Add(this.listBox_pointers);
			this.Controls.Add(this.numericUpDown_chunkSize);
			this.Controls.Add(this.numericUpDown_overlap);
			this.Controls.Add(this.checkBox_invariables);
			this.Controls.Add(this.panel_kernelArguments);
			this.Controls.Add(this.button_kernelLoad);
			this.Controls.Add(this.comboBox_kernelVersions);
			this.Controls.Add(this.comboBox_kernelNames);
			this.Controls.Add(this.button_move);
			this.Controls.Add(this.label_meta);
			this.Controls.Add(this.button_export);
			this.Controls.Add(this.button_import);
			this.Controls.Add(this.numericUpDown_zoom);
			this.Controls.Add(this.hScrollBar_offset);
			this.Controls.Add(this.vScrollBar_volume);
			this.Controls.Add(this.pictureBox_wave);
			this.Controls.Add(this.textBox_time);
			this.Controls.Add(this.button_play);
			this.Controls.Add(this.listBox_tracks);
			this.Controls.Add(this.listBox_log);
			this.Controls.Add(this.comboBox_devices);
			this.MaximumSize = new Size(1920, 860);
			this.MinimumSize = new Size(1920, 860);
			this.Name = "WindowMain";
			this.Text = "MKL Audio Processing using OpenCL Kernels";
			((System.ComponentModel.ISupportInitialize) this.pictureBox_wave).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_overlap).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_chunkSize).EndInit();
			this.groupBox_waves.ResumeLayout(false);
			this.groupBox_waves.PerformLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_waveSamplerate).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_waveTime).EndInit();
			this.groupBox_stretching.ResumeLayout(false);
			this.groupBox_stretching.PerformLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_stretchFactor).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmTarget).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_bpmStart).EndInit();
			this.groupBox_beatScan.ResumeLayout(false);
			this.groupBox_beatScan.PerformLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_maxFreq).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_minFreq).EndInit();
			this.panel_view.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize) this.pictureBox_image).EndInit();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoomImage).EndInit();
			this.groupBox_video.ResumeLayout(false);
			this.groupBox_video.PerformLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_framerate).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ComboBox comboBox_devices;
		private ListBox listBox_log;
		private ListBox listBox_tracks;
		private Button button_play;
		private TextBox textBox_time;
		private PictureBox pictureBox_wave;
		private VScrollBar vScrollBar_volume;
		private HScrollBar hScrollBar_offset;
		private NumericUpDown numericUpDown_zoom;
		private Button button_import;
		private Button button_export;
		private Label label_meta;
		private Button button_move;
		private ComboBox comboBox_kernelNames;
		private ComboBox comboBox_kernelVersions;
		private Button button_kernelLoad;
		private Panel panel_kernelArguments;
		private CheckBox checkBox_invariables;
		private NumericUpDown numericUpDown_overlap;
		private NumericUpDown numericUpDown_chunkSize;
		private ListBox listBox_pointers;
		private Button button_fft;
		private CheckBox checkBox_log;
		private Button button_kernelExecute;
		private GroupBox groupBox_waves;
		private ComboBox comboBox_waves;
		private Button button_waveCreate;
		private Label label_info_waveType;
		private Label label_info_waveSamplerate;
		private NumericUpDown numericUpDown_waveTime;
		private NumericUpDown numericUpDown_waveSamplerate;
		private Label label_info_waveTime;
		private Button button_normalize;
		private Button button_reset;
		private GroupBox groupBox_stretching;
		private Label label_info_stretchFactor;
		private NumericUpDown numericUpDown_stretchFactor;
		private Label label_info_targetBpm;
		private NumericUpDown numericUpDown_bpmTarget;
		private Label label_info_startBpm;
		private NumericUpDown numericUpDown_bpmStart;
		private ComboBox comboBox_kernelsStretch;
		private Button button_stretch;
		private GroupBox groupBox_beatScan;
		private Button button_scan;
		private TextBox textBox_beatScan;
		private ComboBox comboBox_kernelBeatScan;
		private Label label_info_maxFreq;
		private Label label_info_minFreq;
		private NumericUpDown numericUpDown_maxFreq;
		private NumericUpDown numericUpDown_minFreq;
		private Label label_imageMeta;
		private Panel panel_view;
		private PictureBox pictureBox_image;
		private ListBox listBox_images;
		private Button button_resetImage;
		private Button button_exportImage;
		private Button button_importImage;
		private Button button_moveImage;
		private Label label_info_overlap;
		private Label label_info_chunkSize;
		private NumericUpDown numericUpDown_zoomImage;
		private Button button_info;
		private GroupBox groupBox_video;
		private Button button_renderVideo;
		private Label label_info_fps;
		private NumericUpDown numericUpDown_framerate;
	}
}
