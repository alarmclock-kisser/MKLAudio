using OpenTK;
using OpenTK.Compute.OpenCL;
using System.Diagnostics;
using System.Text;

namespace MKLAudio
{
	public class OpenClService
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;
		public ListBox LogList;
		public ComboBox DevicesCombo;

		public string KernelPath => Path.Combine(this.Repopath, "Kernels");

		public OpenClMemoryRegister? MemoryRegister;
		public OpenClKernelCompiler? KernelCompiler;
		public OpenClKernelExecutioner? KernelExecutioner;

		public int INDEX = -1;
		public CLContext? CTX = null;
		public CLDevice? DEV = null;
		public CLPlatform? PLAT = null;




		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public Dictionary<CLDevice, CLPlatform> DevicesPlatforms => this.GetDevicesPlatforms();
		public Dictionary<string, string> Names => this.GetNames();






		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public OpenClService(string repopath, ListBox? logList = null, ComboBox? devicesComboBox = null)
		{
			this.Repopath = repopath;
			this.LogList = logList ?? new ListBox();
			this.DevicesCombo = devicesComboBox ?? new ComboBox();

			// Register events
			this.DevicesCombo.SelectedIndexChanged += (sender, e) => this.InitContext(this.DevicesCombo.SelectedIndex, silent: false);

			// Fill devices combo box
			this.FillDevicesCombo();
		}




		// ----- ----- ----- METHODS ----- ----- ----- \\





		// ----- ----- ----- PUBLIC METHODS ----- ----- ----- \\
		// Log
		public void Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[OpenCL]: " + new string(' ', indent * 2) + message;

			if (!string.IsNullOrEmpty(inner))
			{
				msg += " (" + inner + ")";
			}

			// Add to logList
			this.LogList.Items.Add(msg);

			// Scroll down
			this.LogList.SelectedIndex = this.LogList.Items.Count - 1;
		}


		// Dispose
		public void Dispose(bool silent = false)
		{
			// Dispose context
			if (this.CTX != null)
			{
				CL.ReleaseContext(this.CTX.Value);
				this.PLAT = null;
				this.DEV = null;
				this.CTX = null;
			}

			// Dispose memory handling
			this.MemoryRegister?.Dispose();

			// Dispose kernel handling
			this.KernelExecutioner?.Dispose();
			this.KernelExecutioner?.Dispose();

			// Log
			if (!silent)
			{
				this.Log("Disposed OpenCL context", "No context available");
			}
		}


		// Platforms & Devices
		public CLPlatform[] GetPlatforms()
		{
			// Get all OpenCL platforms
			CLResultCode error = CL.GetPlatformIds(out CLPlatform[] platforms);
			if (error != CLResultCode.Success || platforms.Length == 0)
			{
				this.Log("Error getting Cl-Platforms", platforms.Length.ToString(), 1);
			}

			// Return
			return platforms;
		}

		public Dictionary<CLDevice, CLPlatform> GetDevicesPlatforms()
		{
			// Return dict
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = [];

			// Get platforms
			CLPlatform[] platforms = this.GetPlatforms();

			// Foreach platform get devices
			for (int i = 0; i < platforms.Length; i++)
			{
				// Get devices
				CLResultCode error = CL.GetDeviceIds(platforms[i], DeviceType.All, out CLDevice[] devices);
				if (error != CLResultCode.Success)
				{
					this.Log("Error getting Devices for CL-Platform", i.ToString(), 1);
				}

				// Foreach device, add to dict with platform
				foreach (CLDevice dev in devices)
				{
					devicesPlatforms.Add(dev, platforms[i]);
				}
			}

			// Return
			return devicesPlatforms;
		}


		// Get device & platform info
		public string GetDeviceInfo(CLDevice? device = null, DeviceInfo info = DeviceInfo.Name, bool silent = false)
		{
			// Verify device
			device ??= this.DEV;
			if (device == null)
			{
				if (!silent)
				{
					this.Log("No OpenCL device", "No device specified");
				}

				return "N/A";
			}

			// Get device info
			CLResultCode error = CL.GetDeviceInfo(device.Value, info, out byte[] infoBytes);
			if (error != CLResultCode.Success || infoBytes.Length == 0)
			{
				if (!silent)
				{
					this.Log("Failed to get device info", error.ToString());
				}

				return "N/A";
			}

			// Convert to string if T is string
			if (info == DeviceInfo.Name || info == DeviceInfo.DriverVersion || info == DeviceInfo.Version || info == DeviceInfo.Vendor || info == DeviceInfo.Profile || info == DeviceInfo.OpenClCVersion || info == DeviceInfo.Extensions)
			{
				// Handle extensions as comma-separated string
				if (info == DeviceInfo.Extensions)
				{
					string extensions = Encoding.UTF8.GetString(infoBytes).Trim('\0');
					return string.Join(", ", extensions.Split('\0'));
				}
				return Encoding.UTF8.GetString(infoBytes).Trim('\0');
			}

			// Convert to string if T is a numeric type
			if (info == DeviceInfo.MaximumComputeUnits || info == DeviceInfo.MaximumWorkItemDimensions || info == DeviceInfo.MaximumWorkGroupSize || info == DeviceInfo.MaximumClockFrequency || info == DeviceInfo.AddressBits || info == DeviceInfo.VendorId)
			{
				return BitConverter.ToInt32(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.MaximumWorkItemSizes)
			{
				return string.Join(", ", infoBytes.Select(b => b.ToString()).ToArray());
			}
			else if (info == DeviceInfo.MaximumConstantBufferSize)
			{
				return BitConverter.ToInt32(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.GlobalMemorySize || info == DeviceInfo.LocalMemorySize || info == DeviceInfo.GlobalMemoryCacheSize)
			{
				return BitConverter.ToInt64(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.MaximumMemoryAllocationSize)
			{
				return BitConverter.ToUInt64(infoBytes, 0).ToString();
			}
			else if (info == DeviceInfo.MaximumMemoryAllocationSize)
			{
				return BitConverter.ToInt64(infoBytes, 0).ToString();
			}

			// Convert to string if T is a boolean type
			if (info == DeviceInfo.ImageSupport)
			{
				return (infoBytes[0] != 0).ToString();
			}

			// Convert to string if T is a byte array
			// Here you can add more cases if needed

			// Return "N/A" if info type is not supported
			if (!silent)
			{
				this.Log("Unsupported type for device info", info.ToString());
			}
			return "N/A";
		}

		public string GetPlatformInfo(CLPlatform? platform = null, PlatformInfo info = PlatformInfo.Name, bool silent = false)
		{
			// Verify platform
			platform ??= this.PLAT;
			if (platform == null)
			{
				if (!silent)
				{
					this.Log("No OpenCL platform", "No platform specified");
				}
				return "N/A";
			}

			// Get platform info
			CLResultCode error = CL.GetPlatformInfo(platform.Value, info, out byte[] infoBytes);
			if (error != CLResultCode.Success || infoBytes.Length == 0)
			{
				if (!silent)
				{
					this.Log("Failed to get platform info", error.ToString());
				}
				return "N/A";
			}

			// Convert to string for text-based info types
			if (info == PlatformInfo.Name ||
				info == PlatformInfo.Vendor ||
				info == PlatformInfo.Version ||
				info == PlatformInfo.Profile ||
				info == PlatformInfo.Extensions)
			{
				return Encoding.UTF8.GetString(infoBytes).Trim('\0');
			}

			// Convert numeric types to string
			if (info == PlatformInfo.PlatformHostTimerResolution)
			{
				return BitConverter.ToUInt64(infoBytes, 0).ToString();
			}

			// Handle extension list as comma-separated string
			if (info == PlatformInfo.Extensions)
			{
				string extensions = Encoding.UTF8.GetString(infoBytes).Trim('\0');
				return string.Join(", ", extensions.Split('\0'));
			}

			// Return raw hex for unsupported types
			if (!silent)
			{
				this.Log("Unsupported platform info type", info.ToString());
			}
			return BitConverter.ToString(infoBytes).Replace("-", "");
		}

		public Dictionary<string, string> GetNames()
		{
			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Create dictionary for device names and platform names
			Dictionary<string, string> names = [];

			// Iterate over devices
			foreach (CLDevice device in devicesPlatforms.Keys)
			{
				// Get device name
				string deviceName = this.GetDeviceInfo(device, DeviceInfo.Name, true) ?? "N/A";

				// Get platform name
				string platformName = this.GetPlatformInfo(devicesPlatforms[device], PlatformInfo.Name, true) ?? "N/A";

				// Add to dictionary
				names.Add(deviceName, platformName);
			}

			// Return names
			return names;
		}


		// UI
		public void FillDevicesCombo(ComboBox? comboBox = null, int selection = -1)
		{
			comboBox ??= this.DevicesCombo;
			if (comboBox == null)
			{
				this.Log("No combo box provided", "Cannot fill devices combo box");
				return;
			}

			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Clear combo box
			comboBox.Items.Clear();

			// Add devices to combo box
			foreach (KeyValuePair<CLDevice, CLPlatform> device in devicesPlatforms)
			{
				string deviceName = this.GetDeviceInfo(device.Key, DeviceInfo.Name, true) ?? "N/A";
				string platformName = this.GetPlatformInfo(device.Value, PlatformInfo.Name, true) ?? "N/A";
				comboBox.Items.Add(deviceName + " (" + platformName + ")");
			}

			// Select selection if valid
			if (selection >= 0 && selection < comboBox.Items.Count)
			{
				comboBox.SelectedIndex = selection;
			}
			else
			{
				comboBox.SelectedIndex = -1;
			}
		}

		public List<string> GetInfoDeviceInfo(CLDevice? dev = null, bool raw = false, bool showMsgbox = false)
		{
			dev ??= this.DEV;
			if (dev == null)
			{
				return [];
			}

			List<string> infoList = [];
			List<string> descList =
				[
					"Name", "Vendor", "Vendor id", "Address Bits", "Global memory size", "Local memory size",
					"Cache memory size",
					"Compute units", "Clock frequency", "Max. buffer size", "OpenCLC version", "Version",
					"Driver version"
				];

			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.Name));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.Vendor));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.VendorId));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.AddressBits));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.GlobalMemorySize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.LocalMemorySize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.GlobalMemoryCacheSize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.MaximumComputeUnits));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.MaximumClockFrequency));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.MaximumConstantBufferSize));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.OpenClCVersion));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.Version));
			infoList.Add(this.GetDeviceInfo(dev.Value, DeviceInfo.DriverVersion));

			if (!raw)
			{
				for (int i = 0; i < infoList.Count; i++)
				{
					infoList[i] = descList[i] + " : '" + infoList[i] + "'";
				}
			}

			// Show message box if requested
			if (showMsgbox)
			{
				string msg = string.Join(Environment.NewLine, infoList);
				MessageBox.Show(msg, "OpenCL Device Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			// Return info list

			return infoList;
		}

		public List<string> GetInfoPlatformInfo(CLPlatform? plat = null, bool raw = false, bool showMsgbox = false)
		{
			plat ??= this.PLAT;
			if (plat == null)
			{
				return [];
			}

			List<string> infoList = [];
			List<string> descList =
				[
					"Name", "Vendor", "Version", "Profile", "Extensions"
				];

			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Name));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Vendor));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Version));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Profile));
			infoList.Add(this.GetPlatformInfo(plat.Value, PlatformInfo.Extensions));

			if (!raw)
			{
				for (int i = 0; i < infoList.Count; i++)
				{
					infoList[i] = descList[i] + " : '" + infoList[i] + "'";
				}
			}

			// Show message box if requested
			if (showMsgbox)
			{
				string msg = string.Join(Environment.NewLine, infoList);
				MessageBox.Show(msg, "OpenCL Platform Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			return infoList;
		}

		public void FillKernelsCombobox(ComboBox comboBox)
		{
			// Clear combo box
			comboBox.Items.Clear();

			if (this.KernelCompiler == null)
			{
				this.Log("Kernel handling not initialized", "Cannot fill kernels combo box");
				return;
			}

			this.KernelCompiler.FillKernelsCombobox(comboBox);
		}


		// Init context
		public void InitContext(int index = 0, bool silent = false)
		{
			// Dispose prev context
			this.Dispose(true);

			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Check if index is valid
			if (index < 0 || index >= devicesPlatforms.Count)
			{
				if (!silent)
				{
					this.Log("Invalid device index", index.ToString());
				}
				return;
			}

			// Get device and platform
			this.DEV = devicesPlatforms.Keys.ElementAt(index);
			this.PLAT = devicesPlatforms.Values.ElementAt(index);

			// Create context
			this.CTX = CL.CreateContext(0, [this.DEV.Value], 0, IntPtr.Zero, out CLResultCode error);
			if (error != CLResultCode.Success || this.CTX == null)
			{
				if (!silent)
				{
					this.Log("Failed to create OpenCL context", error.ToString());
				}
				return;
			}

			// Init memory handling & fourier handling & kernel handling
			this.MemoryRegister = new OpenClMemoryRegister(this.Repopath, this.CTX.Value, this.DEV.Value, this.PLAT.Value, this.LogList);
			this.KernelCompiler = new OpenClKernelCompiler(this.Repopath, this.MemoryRegister, this.CTX.Value, this.DEV.Value, this.PLAT.Value, this.MemoryRegister.QUE, this.LogList);
			this.KernelExecutioner = new OpenClKernelExecutioner(this.Repopath, this.MemoryRegister, this.CTX.Value, this.DEV.Value, this.PLAT.Value, this.MemoryRegister.QUE, this.KernelCompiler, this.LogList);

			// Set index
			this.INDEX = index;

			// Log
			if (!silent)
			{
				this.Log("Created OpenCL context", this.GetDeviceInfo(this.DEV, DeviceInfo.Name, silent) ?? "N/A");
			}
		}

		public void SelectDeviceLike(string deviceNameWildcard = "Intel")
		{
			// Get all OpenCL devices & platforms
			Dictionary<CLDevice, CLPlatform> devicesPlatforms = this.DevicesPlatforms;

			// Find device with name like deviceNameWildcard
			int index = -1;
			foreach (KeyValuePair<CLDevice, CLPlatform> device in devicesPlatforms)
			{
				string deviceName = this.GetDeviceInfo(device.Key, DeviceInfo.Name, true) ?? "N/A";
				if (deviceName.Contains(deviceNameWildcard))
				{
					index = Array.IndexOf(devicesPlatforms.Keys.ToArray(), device.Key);
					break;
				}
			}

			// Select device
			if (index >= 0)
			{
				this.DevicesCombo.SelectedIndex = index;

				if (this.INDEX == index)
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

		public void FillPointers(ListBox listBox_pointers)
		{
			// Clear listbox
			listBox_pointers.Items.Clear();
			
			// Fill with memory pointers
			if (this.MemoryRegister == null)
			{
				this.Log("Memory register not initialized", "Cannot fill pointers listbox");
				return;
			}

			// Fill handles with type
			foreach (ClMem mem in this.MemoryRegister.Memory)
			{
				listBox_pointers.Items.Add(mem.IndexHandle.ToString("X16") + " - " + mem.ElementType.Name);
			}
		}

		public void FillSpecificKernels(ComboBox comboBox, string wildcard = "stretch")
		{
			// Clear combo box
			comboBox.Items.Clear();
			if (this.KernelCompiler == null)
			{
				this.Log("Kernel compiler not initialized", "", 1);
				return;
			}

			// Get all kernel names
			List<string> kernelNames = this.KernelCompiler.Files.Values.Where(name => name.Contains(wildcard, StringComparison.OrdinalIgnoreCase)).ToList();
			if (kernelNames.Count == 0)
			{
				this.Log("No kernels found with wildcard", wildcard, 1);
				return;
			}

			// Add to combo box
			foreach (string kernelName in kernelNames)
			{
				comboBox.Items.Add(kernelName);
			}

			// Select last index
			comboBox.SelectedIndex = comboBox.Items.Count - 1;
		}

		public void RebuildKernelArgs(Panel panel, string kernelName = "", bool showInvariables = true)
		{
			// Rebuild kernel args from panel controls
			if (this.KernelCompiler == null)
			{
				this.Log("Kernel executioner not initialized", "Cannot rebuild kernel args");
				return;
			}

			if (this.KernelCompiler.Kernel == null)
			{
				this.KernelCompiler.LoadKernel(kernelName, "", panel, showInvariables);
				if (this.KernelCompiler.Kernel == null)
				{
					this.Log("Failed to load kernel", "Kernel name: " + kernelName, 1);
				}
				return;
			}

			this.KernelCompiler.BuildInputPanel(panel, showInvariables);
		}

		public void SelectLatestKernel(ComboBox comboBox)
		{
			// From combobox items, select the latest kernel
			if (comboBox == null || comboBox.Items.Count == 0)
			{
				this.Log("No kernels available", "Cannot select latest kernel", 1);
				return;
			}

			// Get kernel names from items
			List<string> kernelNames = comboBox.Items.Cast<string>().ToList();
			if (kernelNames.Count == 0)
			{
				this.Log("No kernel names found in combo box", "Cannot select latest kernel", 1);
				return;
			}

			// Get kernel files from compiler
			if (this.KernelCompiler == null || this.KernelCompiler.Files == null || this.KernelCompiler.Files.Count == 0)
			{
				this.Log("Kernel compiler not initialized or no files available", "Cannot select latest kernel", 1);
				return;
			}

			List<string> kernelFiles = this.KernelCompiler.Files.Keys.ToList();

			// Sort by last modified date
			kernelFiles.Sort((a, b) =>
			{
				DateTime aDate = File.GetLastWriteTime(Path.Combine(this.KernelPath, a));
				DateTime bDate = File.GetLastWriteTime(Path.Combine(this.KernelPath, b));
				return bDate.CompareTo(aDate); // Sort descending
			});
			if (kernelFiles.Count == 0)
			{
				this.Log("No kernel files found", "Cannot select latest kernel", 1);
				return;
			}

			// While indexof latest kernel file is not in combo box items, loop through kernel files
			int latestIndex = -1;
			for (int i = 0; i < kernelFiles.Count; i++)
			{
				string kernelFile = Path.GetFileNameWithoutExtension(kernelFiles[i]);
				if (kernelNames.Contains(kernelFile))
				{
					latestIndex = kernelNames.IndexOf(kernelFile);
					break;
				}
			}

			if (latestIndex == -1)
			{
				this.Log("No latest kernel found in combo box", "Cannot select latest kernel", 1);
				return;
			}

			// Select latest kernel in combo box
			comboBox.SelectedIndex = latestIndex;
		}


		// ----- ----- ----- ACCESSIBLE METHODS ----- ----- ----- \\
		public IntPtr MoveAudio(AudioObject obj, int chunkSize = 0, float overlap = 0.0f, double stretchFactor = 1.00000f, bool log = false)
		{
			// Move audio host <-> device
			if (obj.OnHost)
			{
				// Push to device
				return this.MemoryRegister?.PushAudio(obj, chunkSize, overlap, log) ?? IntPtr.Zero;
			}
			else if (obj.OnDevice)
			{
				// Pull to host
				return this.MemoryRegister?.PullAudio(obj, stretchFactor, true, log) ?? IntPtr.Zero;
			}
			else
			{
				if (log)
				{
					this.Log("Audio object is neither on host nor on device", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return IntPtr.Zero;
			}
		}

		public IntPtr MoveImage(ImageObject obj, bool toDevice = true, bool log = false)
		{
			if (obj.OnHost)
			{
				// Push to device
				return this.MemoryRegister?.PushImage(obj, log) ?? IntPtr.Zero;
			}
			else if (obj.OnDevice)
			{
				// Pull to host
				return this.MemoryRegister?.PullImage(obj, log) ?? IntPtr.Zero;
			}
			else
			{
				if (log)
				{
					this.Log("Image object is neither on host nor on device", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return IntPtr.Zero;
			}
		}

		public IntPtr PerformFFT(AudioObject obj, int chunkSize = 0, float overlap = 0.0f, bool log = false)
		{
			// Optionally move audio to device
			if (obj.OnHost)
			{
				this.MoveAudio(obj, chunkSize, overlap, 1.0f, log);
			}
			if (!obj.OnDevice)
			{
				if (log)
				{
					this.Log("Audio object is not on device", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return IntPtr.Zero;
			}

			// Perform FFT on device
			obj.Pointer = this.KernelExecutioner?.ExecuteFFT(obj.Pointer, obj.Form, chunkSize, overlap, true, log) ?? obj.Pointer;

			if (obj.Pointer == IntPtr.Zero && log)
			{
				this.Log("Failed to perform FFT", "Pointer=" + obj.Pointer.ToString("X16"), 1);
			}
			else
			{
				if (log)
				{
					this.Log("Performed FFT", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				obj.Form = obj.Form == 'f' ? 'c' : 'f';
			}

			return obj.Pointer;
		}

		public IntPtr ExecuteAudioKernel(AudioObject obj, string kernelBaseName = "normalize", string kernelVersion = "00", int chunkSize = 0, float overlap = 0.0f, Dictionary<string, object>? optionalArguments = null, bool log = false)
		{
			// Check executioner
			if (this.KernelExecutioner == null)
			{
				this.Log("Kernel executioner not initialized", "Cannot execute audio kernel");
				return IntPtr.Zero;
			}

			// Take time
			Stopwatch sw = Stopwatch.StartNew();

			// Optionally move audio to device
			bool moved = false;
			if (obj.OnHost)
			{
				this.MoveAudio(obj, chunkSize, overlap, 1.0f, log);
				moved = true;
			}
			if (!obj.OnDevice)
			{
				if (log)
				{
					this.Log("Audio object is not on device", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return IntPtr.Zero;
			}

			// Execute kernel on device
			obj.Pointer = this.KernelExecutioner.ExecuteAudioKernel(obj.Pointer, out double factor, obj.Length, kernelBaseName + kernelVersion, chunkSize, overlap, obj.Samplerate, obj.Bitdepth, obj.Channels, optionalArguments, log);
			if (obj.Pointer == IntPtr.Zero && log)
			{
				this.Log("Failed to execute audio kernel", "Pointer=" + obj.Pointer.ToString("X16"), 1);
			}

			// Reload kernel
			this.KernelCompiler?.LoadKernel(kernelBaseName + kernelVersion, "", null, false, log);

			// Log factor & set new bpm
			if (factor != 1.00f)
			{
				obj.Bpm = (float) (obj.Bpm / factor);
				this.Log("Factor for audio kernel: " + factor, "Pointer=" + obj.Pointer.ToString("X16") + " BPM: " + obj.Bpm, 1);
			}

			// Move back optionally
			if (moved && obj.OnDevice && obj.Form == 'f')
			{
				this.MoveAudio(obj, chunkSize, overlap, factor, log);
			}

			if (log)
			{
				sw.Stop();
				this.Log("Executed audio kernel", "Pointer=" + obj.Pointer.ToString("X16") + ", Time: " + sw.ElapsedMilliseconds + "ms", 1);
			}

			return obj.Pointer;
		}

		public IntPtr ExecuteImageKernel(ImageObject obj, string kernelBaseName = "mandelbrot", string kernelVersion = "00", object[]? variableArguments = null, bool log = false)
		{
			// Verify obj on device
			bool moved = false;
			if (obj.OnHost)
			{
				if (log)
				{
					this.Log("Image was on host, pushing ...", obj.Width + " x " + obj.Height, 2);
				}

				// Get pixel bytes
				byte[] pixels = obj.GetPixelsAsBytes(true);
				if (pixels == null || pixels.LongLength == 0)
				{
					this.Log("Couldn't get byte[] from image object", "Aborting", 1);
					return IntPtr.Zero;
				}

				// Push pixels -> pointer
				obj.Pointer = this.MemoryRegister?.PushData<byte>(pixels)?.IndexHandle ?? IntPtr.Zero;
				if (obj.OnHost || obj.Pointer == IntPtr.Zero)
				{
					if (log)
					{
						this.Log("Couldn't get pointer after pushing pixels to device", pixels.LongLength.ToString("N0"), 1);
					}
					return IntPtr.Zero;
				}

				moved = true;
			}

			// Get parameters for call
			IntPtr pointer = obj.Pointer;
			int width = obj.Width;
			int height = obj.Height;
			int channels = obj.Channels;
			int bitdepth = obj.BitsPerPixel / obj.Channels;

			// Call exec on image
			IntPtr outputPointer = this.KernelExecutioner?.ExecuteImageKernel(pointer, kernelBaseName + kernelVersion, width, height, channels, bitdepth, variableArguments, log) ?? IntPtr.Zero;
			if (outputPointer == IntPtr.Zero)
			{
				if (log)
				{
					this.Log("Couldn't get output pointer after kernel execution", "Aborting", 1);
				}
				return outputPointer;
			}

			// Set obj pointer
			obj.Pointer = outputPointer;

			// Optionally: Move back to host
			if (obj.OnDevice && moved)
			{
				// Pull pixel bytes
				byte[] pixels = this.MemoryRegister?.PullData<byte>(obj.Pointer) ?? [];
				if (pixels == null || pixels.LongLength == 0)
				{
					if (log)
					{
						this.Log("Couldn't pull pixels (byte[]) from device", "Aborting", 1);
					}
					return IntPtr.Zero;
				}

				// Aggregate image
				obj.SetImageFromBytes(pixels, true);
			}

			return outputPointer;
		}

		public float ExecuteBeatScan(AudioObject obj, string kernelBaseName = "beat_scan", string kernelVersion = "00", int chunkSize = 0, float overlap = 0.0f, Dictionary<string, object>? optionalArguments = null, bool log = false)
		{
			// Check executioner
			if (this.KernelExecutioner == null)
			{
				this.Log("Kernel executioner not initialized", "Cannot execute beat scan kernel");
				return 0.0f;
			}

			// Optionally move audio to device
			bool moved = false;
			if (obj.OnHost)
			{
				this.MoveAudio(obj, chunkSize, overlap, 1.0f, log);
				moved = true;
			}
			if (!obj.OnDevice)
			{
				if (log)
				{
					this.Log("Audio object is not on device", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return 0.0f;
			}

			// Execute beat scan kernel on device
			float[] bpms = this.KernelExecutioner.ExecuteBeatScan(obj.Pointer, obj.Length, kernelBaseName + kernelVersion, 1, chunkSize, overlap, obj.Samplerate, obj.Bitdepth, obj.Channels, optionalArguments, log);
			if (bpms.Length == 0)
			{
				if (log)
				{
					this.Log("No BPMs found", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return 0.0f;
			}

			// Log execution
			if (log)
			{
				this.Log("Executed beat scan kernel", "Pointer=" + obj.Pointer.ToString("X16") + ", BPMs found: " + bpms.Length, 1);
			}

			// Move back optionally
			if (obj.OnDevice && obj.Form == 'f' && moved)
			{
				this.MoveAudio(obj, chunkSize, overlap, 1.0f, log);
			}

			float bpm = this.CalculateBPM(bpms, chunkSize * overlap, obj.Samplerate, 60f, 200f);

			if (log)
			{
				string first50Values = string.Join(", ", bpms.Take(50).Select(v => v.ToString("F5")));
				this.Log("BPMs: " + first50Values + (bpms.Length > 50 ? ", ..." : ""), "Pointer=" + obj.Pointer.ToString("X16") + ", BPM: " + bpm.ToString("F5"), 1);
			}

			if (log)
			{
				this.Log("Calculated BPM", bpm.ToString("F5"), 1);
			}

			return bpm;
		}

		public double[] ExecuteBeatZoom(AudioObject obj, string kernelBaseName = "beatZoom", string kernelVersion = "00", int chunkSize = 8192, int samplerate = 44100, int frameRate = 20, float threshold = 0.2f, double minZoom = 1000, double maxZoom = 10000, double zoomMultiplier = 1.05d, bool log = false)
		{
			double[] results = [];

			// Check executioner
			if (this.KernelExecutioner == null)
			{
				if (log)
				{
					// Log error
					this.Log("Kernel executioner not initialized", "Cannot execute beat zoom kernel", 1);
				}
				return results;
			}

			// Optionally move audio to device
			bool moved = false;
			if (obj.OnHost)
			{
				this.MoveAudio(obj, chunkSize, 0.0f, 1.0f, true);
				moved = true;
			}
			if (!obj.OnDevice)
			{
				if (log)
				{
					this.Log("Audio object is not on device", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return results;
			}

			// Execute beat zoom kernel on device
			results = this.KernelExecutioner.ExecuteBeatZoomKernel(obj.Pointer, kernelBaseName + kernelVersion, (int)obj.Length, 0.0f,  samplerate, frameRate, threshold, minZoom, maxZoom, zoomMultiplier, log);
			if (results.Length == 0)
			{
				if (log)
				{
					this.Log("No results found from beat zoom kernel", "Pointer=" + obj.Pointer.ToString("X16"), 1);
				}
				return results;
			}

			// Move back optionally
			if (obj.OnDevice && obj.Form == 'f' && moved)
			{
				this.MoveAudio(obj, chunkSize, 0.0f, 1.0f, true);
			}

			// Log execution
			if (log)
			{
				string first50Values = string.Join(", ", results.Take(50).Select(v => v.ToString("F5")));
				this.Log("Results: " + first50Values + (results.Length > 50 ? ", ..." : ""), "Pointer=" + obj.Pointer.ToString("X16") + ", Results found: " + results.Length, 1);
			}

			return results;
		}

		public float CalculateBPM(float[] energy, float hopSizeInSamples, float sampleRate, float minBPM = 60, float maxBPM = 200)
		{
			int len = energy.Length;
			float hopTime = hopSizeInSamples / sampleRate;

			// BPM → Lag-Grenzen in Chunks
			int minLag = (int) (60f / maxBPM / hopTime);
			int maxLag = (int) (60f / minBPM / hopTime);

			if (maxLag >= len)
			{
				maxLag = len - 1;
			}

			if (minLag < 1)
			{
				minLag = 1;
			}

			float bestCorr = float.NegativeInfinity;
			int bestLag = minLag;

			for (int lag = minLag; lag <= maxLag; lag++)
			{
				float sum = 0f;
				for (int i = 0; i < len - lag; i++)
				{
					sum += energy[i] * energy[i + lag];
				}

				if (sum > bestCorr)
				{
					bestCorr = sum;
					bestLag = lag;
				}
			}

			float periodInSec = bestLag * hopTime;
			float bpm = 60f / periodInSec;
			return bpm;
		}


	}
}
