using ManagedCuda;
using ManagedCuda.BasicTypes;
using OpenTK.Compute.OpenCL;

namespace MKLAudio
{
	public class CudaService
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		private string Repopath;
		private ListBox LogList;
		private ComboBox DevicesCombo;
		private ProgressBar PBar;


		public int DeviceIndex { get; set; } = -1;
		private PrimaryContext? context = null;
		private CUdevice? device = null;
		private List<CudaStream> Streams = [];

		public CudaMemoryRegister? MemoryRegister { get; private set; } = null;
		public CudaFourierHandling? FourierHandling { get; private set; } = null;
		public CudaKernelCompiler? KernelCompiler { get; private set; } = null;
		public CudaKernelExecutioner? KernelExecutioner { get; private set; } = null;



		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public bool Initialized => this.context != null;



		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public CudaService(string repopath, ListBox? listBox_log = null, ComboBox? comboBox_devices = null, ProgressBar? progressBar = null)
		{
			// Set attributes
			this.Repopath = repopath;
			this.LogList = listBox_log ?? new ListBox();
			this.DevicesCombo = comboBox_devices ?? new ComboBox();
			this.PBar = progressBar ?? new ProgressBar();

			// Fill devices combobox
			this.FillDevicesCombobox();


		}




		// ----- ----- ----- METHODS ----- ----- ----- \\
		public string Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[CUDA]: " + new string(' ', indent * 2) + message;

			if (!string.IsNullOrEmpty(inner))
			{
				msg += " (" + inner + ")";
			}

			// Add to logList
			this.LogList.Items.Add(msg);

			// Scroll down
			this.LogList.TopIndex = this.LogList.Items.Count - 1;

			return msg;
		}

		public List<string> GetDeviceNames(bool shortIdentifier = false, bool log = false)
		{
			List<string> deviceNames = [];

			// Tracatch get device count
			int devicesCount = 0;
			try
			{
				devicesCount = CudaContext.GetDeviceCount();
			}
			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Failed to get device count: " + ex.Message, ex.InnerException?.ToString() ?? "", 1);
				}
				return deviceNames;
			}

			// Check if devices are available
			if (devicesCount == 0)
			{
				if (log)
				{
					this.Log("No CUDA devices found.", "", 1);
				}
				return deviceNames;
			}

			// Iterate through devices and get names
			for (int i = 0; i < devicesCount; i++)
			{
				try
				{
					CUdevice device = PrimaryContext.GetCUdevice(i);
					CudaDeviceProperties props = PrimaryContext.GetDeviceInfo(i);
					string deviceName = props.DeviceName;
					string capability = props.ComputeCapability.ToString();
					if (shortIdentifier)
					{
						deviceName = deviceName.ToUpper().Replace("NVIDIA", "");
					}
					else
					{
						deviceName = deviceName + " (" + capability + ")";
					}
					deviceNames.Add(deviceName);
				}
				catch (Exception ex)
				{
					if (log)
					{
						this.Log("Failed to get device name for index " + i + ": " + ex.Message, ex.InnerException?.ToString() ?? "", 1);
					}
				}
			}

			// Log device names if requested
			if (log)
			{
				this.Log("Found " + deviceNames.Count + " CUDA devices:", "", 1);
				foreach (var name in deviceNames)
				{
					this.Log(name, "", 2);
				}
			}

			return deviceNames;
		}

		public void FillDevicesCombobox(ComboBox? comboBox = null, bool shortIdentifiers = false)
		{
			comboBox ??= this.DevicesCombo;
			comboBox.Items.Clear();

			List<string> deviceNames = this.GetDeviceNames(shortIdentifiers, false);
			if (deviceNames.Count == 0)
			{
				comboBox.Text = "No CUDA devices found";
			}

			foreach (var name in deviceNames)
			{
				comboBox.Items.Add(name);
			}

			comboBox.SelectedIndexChanged += (sender, e) =>
			{
				this.InitContext(comboBox.SelectedIndex, true);
			};
		}

		public void InitContext(int deviceIndex, bool log = false)
		{
			if (deviceIndex < 0)
			{
				if (log)
				{
					this.Log("Invalid device index: " + deviceIndex, "", 1);
				}
				return;
			}
			try
			{
				this.device = PrimaryContext.GetCUdevice(deviceIndex);
				this.context = new PrimaryContext(this.device.Value);
				this.context.SetCurrent();
				this.DeviceIndex = deviceIndex;

				// Initialize streams
				this.Streams.Clear();
				for (int i = 0; i < 4; i++) // Create 4 streams as an example
				{
					this.Streams.Add(new CudaStream());
				}

				// Init. classes
				this.MemoryRegister = new CudaMemoryRegister(this.Repopath, this.LogList, this.PBar, this.context, this.device.Value, this.Streams);
				this.FourierHandling = new CudaFourierHandling(this.Repopath, this.LogList, this.PBar, this.context, this.device.Value, this.Streams, this.MemoryRegister);
				this.KernelCompiler = new CudaKernelCompiler(this.Repopath, this.LogList, this.PBar, this.context, this.device.Value, this.Streams, this.MemoryRegister);
				this.KernelExecutioner = new CudaKernelExecutioner(this.Repopath, this.LogList, this.PBar, this.context, this.device.Value, this.Streams, this.MemoryRegister, this.KernelCompiler);

				if (log)
				{
					this.Log("Initialized CUDA device: " + this.device.Value.ToString(), "", 1);
				}
			}
			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Failed to initialize CUDA device at index " + deviceIndex + ": " + ex.Message, ex.InnerException?.ToString() ?? "", 1);
				}
			}
		}

		public void Dispose()
		{
			this.context?.Dispose();
			this.context = null;

			this.device = null;

			this.MemoryRegister?.Dispose();
			this.MemoryRegister = null;

			this.FourierHandling?.Dispose();
			this.FourierHandling = null;

			this.KernelCompiler?.Dispose();
			this.KernelCompiler = null;

			this.KernelExecutioner?.Dispose();
			this.KernelExecutioner = null;
		}




		// ----- ----- ----- ACCESSIBLE METHODS ----- ----- ----- \\
		public List<string> FillPointers(ListBox? listBox = null)
		{
			List<string> pointers = this.MemoryRegister?.Memory.Select(m => m.IndexPointer.ToString()).ToList() ?? [];

			if (listBox != null)
			{
				listBox.Items.Clear();
				foreach (var pointer in pointers)
				{
					listBox.Items.Add(pointer);
				}
			}

			return pointers;
		}

		public void SelectDeviceLike(string deviceNameWildcard = "RTX")
		{
			// Get all OpenCL devices & platforms
			List<string> deviceNames = this.GetDeviceNames(true, true);
			if (deviceNames.Count == 0)
			{
				this.Log("No OpenCL devices found", "Please check your OpenCL installation.", 1);
				return;
			}

			// Find device by name wildcard
			int index = deviceNames.FindIndex(name => name.IndexOf(deviceNameWildcard, StringComparison.OrdinalIgnoreCase) >= 0);

			// Select device
			if (index >= 0)
			{
				this.DevicesCombo.SelectedIndex = index;

				if (this.DeviceIndex == index)
				{
					return;
				}
				this.InitContext(index);
			}
			else
			{
				this.Log("OpenCL device not found", "No device found with name like '" + deviceNameWildcard + "'");
			}
		}



		public IntPtr PushAudio(AudioObject obj, int chunkSize = 0, float overlap = 0.0f, bool log = false)
		{
			if (obj == null || obj.Data == null || obj.Data.Length == 0)
			{
				if (log)
				{
					this.Log("No audio data to push.", "", 1);
				}
				return IntPtr.Zero;
			}

			CudaMem? mem = null;
			if (chunkSize <= 0 || chunkSize > obj.Data.Length)
			{
				chunkSize = 0;
				float[] data = obj.Data;

				mem = this.MemoryRegister?.PushData(data, log);
			}
			else
			{
				List<float[]> chunks = obj.GetChunks(chunkSize, overlap);

				mem = this.MemoryRegister?.PushChunks(chunks, log);
			}

			if (mem == null || mem.IndexPointer == IntPtr.Zero)
			{
				if (log)
				{
					this.Log("Failed to push audio data to device memory.", "", 1);
				}
				return IntPtr.Zero;
			}

			obj.Pointer = mem.IndexPointer;
			obj.Data = [];

			if (log)
			{
				this.Log("Pushed audio data to device memory at pointer: " + obj.Pointer, "", 1);
			}

			return obj.Pointer;
		}

		public IntPtr PullAudio(AudioObject obj, float stretchFactor = 1.0f, bool log = false)
		{
			if (obj == null || obj.Pointer == IntPtr.Zero)
			{
				if (log)
				{
					this.Log("No audio data to pull.", "", 1);
				}
				return IntPtr.Zero;
			}

			CudaMem? mem = this.MemoryRegister?.FindMemory(obj.Pointer, log);
			if (mem == null || mem.IndexPointer == IntPtr.Zero)
			{
				if (log)
				{
					this.Log("No memory object found for pointer: " + obj.Pointer, "", 1);
				}
				return IntPtr.Zero;
			}

			if (obj.ChunkSize <= 0)
			{
				obj.Data = this.MemoryRegister?.PullData<float>(obj.Pointer, true, log) ?? [];
			}
			else
			{
				obj.AggregateStretchedChunks(this.MemoryRegister?.PullChunks<float>(obj.Pointer, true,  log) ?? [], stretchFactor);
			}

			if (obj.Data == null || obj.Data.Length == 0)
			{
				if (log)
				{
					this.Log("Failed to pull audio data from device memory.", "", 1);
				}
				return IntPtr.Zero;
			}

			obj.Pointer = IntPtr.Zero; // Clear pointer after pulling data

			if (log)
			{
				this.Log("Pulled audio data from device memory at pointer: " + mem.IndexPointer, "", 1);
			}

			return obj.Pointer; // Return the pointer of the memory object
		}

		public void PerformFFT(AudioObject obj, int chunkSize = 4196, float overlap = 0.5f, bool log = false)
		{
			// check initialized
			if (this.FourierHandling == null)
			{
				if (log)
				{
					this.Log("FourierHandling is not initialized", "", 1);
				}
				return;
			}

			// Validate input
			if (obj == null)
			{
				if (log)
				{
					this.Log("Null audio object provided", "", 1);
				}

				return;
			}

			// Optionally move audio to device
			if (obj.OnHost)
			{
				this.PushAudio(obj, chunkSize, overlap, log);
			}

			if (!obj.OnDevice)
			{
				if (log)
				{
					this.Log("Audio object is not on device", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return;
			}

			// Get CudaMem
			CudaMem? mem = this.MemoryRegister?.FindMemory(obj.Pointer, log);
			if (mem == null || mem.IndexPointer == IntPtr.Zero)
			{
				if (log)
				{
					this.Log("No memory object found for pointer: " + obj.Pointer, "", 1);
				}
				return;
			}

			try
			{
				// Perform FFT/IFFT on device
				CudaMem? resultMem = this.FourierHandling.PerformFFT(mem, obj.Form, log);

				// Update object with result
				if (resultMem != null && resultMem.IndexPointer != IntPtr.Zero)
				{
					obj.Pointer = resultMem.IndexPointer;
					obj.Form = obj.Form == 'f' ? 'c' : 'f'; // Toggle form

					if (log)
					{
						this.Log("Successfully performed transform",
							   $"NewPointer={obj.Pointer.ToString("X16")}, NewForm={obj.Form}", 1);
					}
				}
				else
				{
					if (log)
					{
						this.Log("Transform operation returned null or invalid pointer",
							   $"OriginalPointer={obj.Pointer.ToString("X16")}", 1);
					}
				}
			}
			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Error during FFT/IFFT operation", ex.Message, 2);
				}
			}
		}
	}
}