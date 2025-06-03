﻿
using Microsoft.VisualBasic.Logging;
using OpenTK.Compute.OpenCL;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Numerics;
using TagLib.Mpeg4;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace MKLAudio
{
	public class OpenClKernelExecutioner
	{
		// ----- ----- -----  ATTRIBUTES  ----- ----- ----- \\
		private string Repopath;
		private ListBox LogList;
		private OpenClMemoryRegister MemR;
		private CLContext Context;
		private CLDevice Device;
		private CLPlatform Platform;
		private CLCommandQueue Queue;
		private OpenClKernelCompiler Compiler;






		// ----- ----- -----  LAMBDA  ----- ----- ----- \\
		public CLKernel? Kernel => this.Compiler?.Kernel;
		public string? KernelFile => this.Compiler?.KernelFile;




		// ----- ----- -----  CONSTRUCTOR ----- ----- ----- \\
		public OpenClKernelExecutioner(string repopath, OpenClMemoryRegister memR, CLContext context, CLDevice device, CLPlatform platform, CLCommandQueue queue, OpenClKernelCompiler compiler, ListBox? listBox_log = null)
		{
			this.Repopath = repopath;
			this.MemR = memR;
			this.Context = context;
			this.Device = device;
			this.Platform = platform;
			this.Queue = queue;
			this.Compiler = compiler;
			this.LogList = listBox_log ?? new ListBox();
		}






		// ----- ----- -----  METHODS  ----- ----- ----- \\
		public void Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[Exec]: " + new string(' ', indent * 2) + message;

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
			// Dispose logic here
			
		}





		// EXEC
		public IntPtr ExecuteFFT(IntPtr pointer, char form, int chunkSize, float overlap, bool free = true, bool log = false)
		{
			int overlapSize = (int) (overlap * chunkSize);

			string kernelsPath = Path.Combine(this.Repopath, "Kernels", "Audio", "Fourier");
			string file = "";
			if (form == 'f')
			{
				file = Path.Combine(kernelsPath, "fft01.cl");
			}
			else if (form == 'c')
			{
				file = Path.Combine(kernelsPath, "ifft01.cl");
			}

			// STOPWATCH START
			Stopwatch sw = Stopwatch.StartNew();

			// Load kernel from file, else abort
			this.Compiler.LoadKernel("", file);
			if (this.Kernel == null)
			{
				return pointer;
			}

			// Get input buffers
			ClMem? inputBuffers = this.MemR.GetBuffer(pointer);
			if (inputBuffers == null || inputBuffers.Count == 0)
			{
				if (log)
				{
					this.Log("Input buffer not found or invalid length: " + pointer.ToString("X16"), "", 2);
				}
				return pointer;
			}

			// Get output buffers
			ClMem? outputBuffers = null;
			if (form == 'f')
			{
				outputBuffers = this.MemR.AllocateGroup<Vector2>(inputBuffers.Count, inputBuffers.IndexLength);
			}
			else if (form == 'c')
			{
				outputBuffers = this.MemR.AllocateGroup<float>(inputBuffers.Count, inputBuffers.IndexLength);
			}
			if (outputBuffers == null || outputBuffers.Lengths.Length == 0 || outputBuffers.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Couldn't allocate valid output buffers / lengths", "", 2);
				}
				return pointer;
			}


			// Set static args
			CLResultCode error = this.SetKernelArgSafe(2, (int) inputBuffers.IndexLength);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for chunk size: " + error, "", 2);
				}
				return pointer;
			}
			error = this.SetKernelArgSafe(3, overlapSize);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for overlap size: " + error, "", 2);
				}
				return pointer;
			}

			// Calculate optimal work group size
			uint maxWorkGroupSize = this.GetMaxWorkGroupSize();
			uint globalWorkSize = 1;
			uint localWorkSize = 1;


			// Loop through input buffers
			for (int i = 0; i < inputBuffers.Count; i++)
			{
				error = this.SetKernelArgSafe(0, inputBuffers.Buffers[i]);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to set kernel argument for input buffer {i}: {error}", "", 2);
					}
					return pointer;
				}
				error = this.SetKernelArgSafe(1, outputBuffers.Buffers[i]);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to set kernel argument for output buffer {i}: {error}", "", 2);
					}
					return pointer;
				}

				// Execute kernel
				error = CL.EnqueueNDRangeKernel(this.Queue, this.Kernel.Value, 1, null, [(UIntPtr) globalWorkSize], [(UIntPtr) localWorkSize], 0, null, out CLEvent evt);

				// Wait for completion
				error = CL.WaitForEvents(1, [evt]);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Wait failed for buffer {i}: " + error, "", 2);
					}
				}

				// Release event
				CL.ReleaseEvent(evt);
			}

			// STOPWATCH END
			sw.Stop();

			// LOG SUCCESS
			if (log)
			{
				if (form == 'f')
				{
					this.Log("Ran FFT successfully on " + inputBuffers.Count + " buffers within " + sw.ElapsedMilliseconds + " ms", "Form now: " + 'c' + ", Chunk: " + chunkSize + ", Overlap: " + overlapSize, 1);
				}
				else if (form == 'c')
				{
					this.Log("Ran IFFT successfully on " + inputBuffers.Count + " buffers within " + sw.ElapsedMilliseconds + " ms", "Form now: " + 'f' + ", Chunk: " + chunkSize + ", Overlap: " + overlapSize, 1);
				}
			}

			if (outputBuffers != null && free)
			{
				this.MemR.FreeBuffer(pointer);
			}

			return outputBuffers?.IndexHandle ?? pointer;
		}

		public IntPtr ExecuteAudioKernel(IntPtr objPointer, out float factor, long length = 0, string kernelName = "normalize00", int chunkSize = 1024, float overlap = 0.5f, int samplerate = 44100, int bitdepth = 24, int channels = 2, Dictionary<string, object>? optionalArguments = null, bool log = false)
		{
			factor = 1.0f; // Default factor

			// Get kernel path
			string kernelPath = this.Compiler.Files.FirstOrDefault(f => f.Key.Contains(kernelName)).Key ?? "";
			if (string.IsNullOrEmpty(kernelPath))
			{
				this.Log("Kernel file not found: " + kernelName, "", 2);
				return IntPtr.Zero;
			}

			// Load kernel if not loaded
			if (this.Kernel == null || this.KernelFile != kernelPath)
			{
				this.Compiler.LoadKernel(kernelName);
				if (this.Kernel == null || this.KernelFile == null || !this.KernelFile.Contains("\\Audio\\"))
				{
					if (log)
					{
						this.Log("Kernel not loaded or invalid kernel file: " + kernelName, "", 2);
					}
					return IntPtr.Zero;
				}
			}

			// Get input buffers
			ClMem? inputMem = this.MemR.GetBuffer(objPointer);
			if (inputMem == null || inputMem.Count == 0 || inputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Input buffer not found or invalid length: " + objPointer.ToString("X16"), "", 2);
				}
				return IntPtr.Zero;
			}

			// Get variable arguments
			object[] variableArguments = this.Compiler.GetArgumentValues();

			// Check if FFT is needed
			bool didFft = false;
			if (this.Compiler.GetKernelPointerInputType().Name.Contains("Vector2") && inputMem.ElementType == typeof(float))
			{
				if (optionalArguments != null && optionalArguments.ContainsKey("factor"))
				{
					// Set factor to optional argument if provided (contains "stretch")
					factor = optionalArguments.ContainsKey("factor") ? Convert.ToSingle(optionalArguments["factor"]) : 1.0f;
				}
				else
				{
					// Otherwise try to get factor from NumericUpDown control in InputPanel
					NumericUpDown? numeric = this.Compiler.InputPanel?.Controls.OfType<NumericUpDown>().FirstOrDefault(n => n.Name.Contains("factor"));
					if (numeric != null && numeric.Value != 1.0M)
					{
						factor = (float) numeric.Value;
					}
				}

					IntPtr fftPointer = this.ExecuteFFT(objPointer, 'f', chunkSize, overlap, true, log);
				if (fftPointer == IntPtr.Zero)
				{
					return IntPtr.Zero;
				}
				objPointer = fftPointer;
				didFft = true;

				// Load kernel if not loaded
				if (this.Kernel == null || this.KernelFile != kernelPath)
				{
					this.Compiler.LoadKernel(kernelName);
					if (this.Kernel == null || this.KernelFile == null || !this.KernelFile.Contains("\\Audio\\"))
					{
						if (log)
						{
							this.Log("Kernel not loaded or invalid kernel file: " + kernelName, "", 2);
						}
						return IntPtr.Zero;
					}
				}
			}

			// Get input buffers
			inputMem = this.MemR.GetBuffer(objPointer);
			if (inputMem == null || inputMem.Count == 0 || inputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Input buffer not found or invalid length: " + objPointer.ToString("X16"), "", 2);
				}
				return IntPtr.Zero;
			}

			// Get output buffers
			ClMem? outputMem = null;
			if (this.Compiler.GetKernelPointerOutputType() == typeof(float*))
			{
				outputMem = this.MemR.AllocateGroup<float>(inputMem.Count, inputMem.IndexLength);
			}
			else if (this.Compiler.GetKernelPointerOutputType() == typeof(Vector2*))
			{
				outputMem = this.MemR.AllocateGroup<Vector2>(inputMem.Count, inputMem.IndexLength);
			}
			else
			{
				if (log)
				{
					this.Log("Unsupported input buffer type: " + inputMem.ElementType.Name, "", 2);
				}
				return IntPtr.Zero;
			}

			if (outputMem == null || outputMem.Count == 0 || outputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Couldn't allocate valid output buffers / lengths", "", 2);
				}
				return IntPtr.Zero;
			}

			// Loop through input buffers
			for (int i = 0; i < inputMem.Count; i++)
			{
				// Get buffers
				CLBuffer inputBuffer = inputMem.Buffers[i];
				CLBuffer outputBuffer = outputMem.Buffers[i];

				// Merge arguments
				List<object> arguments = this.MergeArgumentsAudio(variableArguments, inputBuffer, outputBuffer, length, chunkSize, overlap, samplerate, bitdepth, channels, optionalArguments, false);
				if (arguments == null || arguments.Count == 0)
				{
					if (log)
					{
						this.Log("Failed to merge arguments for buffer " + i, "", 2);
					}
					return IntPtr.Zero;
				}

				// Set kernel arguments
				CLResultCode error = CLResultCode.Success;
				for (uint j = 0; j < arguments.Count; j++)
				{
					error = this.SetKernelArgSafe(j, arguments[(int) j]);
					if (error != CLResultCode.Success)
					{
						if (log)
						{
							this.Log($"Failed to set kernel argument {j} for buffer {i}: " + error, "", 2);
						}
						return IntPtr.Zero;
					}
				}

				// Get work dimensions
				uint maxWorkGroupSize = this.GetMaxWorkGroupSize();
				uint globalWorkSize = (uint) inputMem.Lengths[i];
				uint localWorkSize = Math.Min(maxWorkGroupSize, globalWorkSize);
				if (localWorkSize == 0)
				{
					localWorkSize = 1; // Fallback to 1 if no valid local size
				}
				if (globalWorkSize < localWorkSize)
				{
					globalWorkSize = localWorkSize; // Ensure global size is at least local size
				}

				// Execute kernel
				error = CL.EnqueueNDRangeKernel(this.Queue, this.Kernel.Value, 1, null, [(UIntPtr) globalWorkSize], [(UIntPtr) localWorkSize], 0, null, out CLEvent evt);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to enqueue kernel for buffer {i}: " + error, "", 2);
					}
					return IntPtr.Zero;
				}

				// Wait for completion
				error = CL.WaitForEvents(1, [evt]);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Wait failed for buffer {i}: " + error, "", 2);
					}
				}

				// Release event
				error = CL.ReleaseEvent(evt);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to release event for buffer {i}: " + error, "", 2);
					}
				}
			}

			// Free input buffer if necessary
			if (outputMem.IndexHandle != IntPtr.Zero)
			{
				long freed = this.MemR.FreeBuffer(objPointer, true);
				if (freed > 0)
				{
					if (log)
					{
						this.Log("Freed input buffer: " + objPointer.ToString("X16") + ", Freed " + freed + " Mbytes", "", 1);
					}
				}
			}

			// Optionally execute IFFT if FFT was done
			IntPtr outputPointer = outputMem.IndexHandle;
			if (didFft && outputMem.ElementType == typeof(Vector2))
			{
				IntPtr ifftPointer = this.ExecuteFFT(outputMem.IndexHandle, 'c', chunkSize, overlap, true, log);
				if (ifftPointer == IntPtr.Zero)
				{
					return IntPtr.Zero;
				}
				outputPointer = ifftPointer; // Update output pointer to IFFT result
			}

			// Log success
			if (log)
			{
				this.Log($"Executed kernel '{kernelName}' successfully on {inputMem.Count} buffers with chunk size {chunkSize} and overlap {overlap}", "", 1);
			}

			// Return output buffer handle if available, else return original pointer
			return outputPointer != IntPtr.Zero ? outputPointer : objPointer;
		}

		public IntPtr ExecuteImageKernel(IntPtr pointer = 0, string kernelName = "mandelbrot01", int width = 0, int height = 0, int channels = 4, int bitdepth = 8, object[]? variableArguments = null, bool logSuccess = false)
		{
			// Start stopwatch
			List<long> times = [];
			List<string> timeNames = ["load: ", "mem: ", "args: ", "exec: ", "total: "];
			Stopwatch sw = Stopwatch.StartNew();

			// Get kernel path
			string kernelPath = this.Compiler.Files.FirstOrDefault(f => f.Key.Contains(kernelName)).Key ?? "";

			// Load kernel if not loaded
			if (this.Kernel == null || this.KernelFile != kernelPath)
			{
				this.Compiler.LoadKernel(kernelName);
				if (this.Kernel == null || this.KernelFile == null || !this.KernelFile.Contains("\\Imaging\\"))
				{
					this.Log("Could not load Kernel '" + kernelName + "'", $"ExecuteKernelIPGeneric({string.Join(", ", variableArguments ?? [])})");
					return pointer;
				}
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Get input buffer & length
			ClMem? inputMem = this.MemR.GetBuffer(pointer);
			if (inputMem == null)
			{
				this.Log("Input buffer not found or invalid length: " + pointer.ToString("X16"), "", 2);
				return pointer;
			}

			// Get kernel arguments & work dimensions
			List<string> argNames = this.Compiler.Arguments.Keys.ToList();

			// Dimensions
			int pixelsTotal = (int) inputMem.IndexLength / 4; // Anzahl der Pixel
			int workWidth = width > 0 ? width : pixelsTotal; // Falls kein width gegeben, 1D
			int workHeight = height > 0 ? height : 1;        // Falls kein height, 1D

			// Work dimensions
			uint workDim = (width > 0 && height > 0) ? 2u : 1u;
			UIntPtr[] globalWorkSize = workDim == 2
				? [(UIntPtr) workWidth, (UIntPtr) workHeight]
				: [(UIntPtr) pixelsTotal];

			// Create output buffer
			IntPtr outputPointer = IntPtr.Zero;
			if (this.Compiler.GetArgumentPointerCount() == 0)
			{
				if (logSuccess)
				{
					this.Log("No output buffer needed", "No output buffer", 1);
				}
				return pointer;
			}
			else if (this.Compiler.GetArgumentPointerCount() == 1)
			{
				if (logSuccess)
				{
					this.Log("Single pointer kernel detected", "Single pointer kernel", 1);
				}
			}
			else if (this.Compiler.GetArgumentPointerCount() >= 2)
			{
				ClMem? outputMem = this.MemR.AllocateSingle<byte>(inputMem.IndexLength);
				if (outputMem == null)
				{
					if (logSuccess)
					{
						this.Log("Error allocating output buffer", "", 2);
					}
					return pointer;
				}
				outputPointer = outputMem.IndexHandle;
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Merge arguments
			List<object> arguments = this.MergeArgumentsImage(variableArguments ?? this.Compiler.GetArgumentValues(), pointer, outputPointer, width, height, channels, bitdepth, false);

			// Set kernel arguments
			for (int i = 0; i < arguments.Count; i++)
			{
				// Set argument
				CLResultCode err = this.SetKernelArgSafe((uint) i, arguments[i]);
				if (err != CLResultCode.Success)
				{
					this.Log("Error setting kernel argument " + i + ": " + err.ToString(), arguments[i].ToString() ?? "");
					return pointer;
				}
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());

			// Log arguments
			if (logSuccess)
			{
				this.Log("Kernel arguments set: " + string.Join(", ", argNames.Select((a, i) => a + ": " + arguments[i].ToString())), "'" + kernelName + "'", 2);
			}

			// Exec
			CLResultCode error = CL.EnqueueNDRangeKernel(
				this.Queue,
				this.Kernel.Value,
				workDim,          // 1D oder 2D
				null,             // Kein Offset
				globalWorkSize,   // Work-Größe in Pixeln
				null,             // Lokale Work-Size (automatisch)
				0, null, out CLEvent evt
			);
			if (error != CLResultCode.Success)
			{
				this.Log("Error executing kernel: " + error.ToString(), "", 2);
				return pointer;
			}

			// Wait for kernel to finish
			error = CL.WaitForEvents(1, [evt]);
			if (error != CLResultCode.Success)
			{
				this.Log("Error waiting for kernel to finish: " + error.ToString(), "", 2);
				return pointer;
			}

			// Release event
			error = CL.ReleaseEvent(evt);
			if (error != CLResultCode.Success)
			{
				this.Log("Error releasing event: " + error.ToString(), "", 2);
				return pointer;
			}

			// Take time
			times.Add(sw.ElapsedMilliseconds - times.Sum());
			times.Add(times.Sum());
			sw.Stop();

			// Free input buffer
			long freed;
			if (outputPointer == IntPtr.Zero)
			{
				freed = 0;
			}
			else
			{
				freed = this.MemR.FreeBuffer(pointer, true);
			}

			// Log success with timeNames
			if (logSuccess)
			{
				this.Log("Kernel executed successfully! Times: " + string.Join(", ", times.Select((t, i) => timeNames[i] + t + "ms")) + "(freed input: " + freed + "MB)", "'" + kernelName + "'", 1);
			}

			// Return valued pointer
			return outputPointer != IntPtr.Zero ? outputPointer : pointer;
		}

		public float[] ExecuteBeatScan(IntPtr objPointer, long length = 0, string kernelName = "beatscan", int resultSizePerChunk = 1, int chunkSize = 1024, float overlap = 0.5f, int samplerate = 44100, int bitdepth = 32, int channels = 2, Dictionary<string, object>? optionalArguments = null, bool log = false)
		{
			float[] bpms = [];

			// Get kernel path
			string kernelPath = this.Compiler.Files.FirstOrDefault(f => f.Key.Contains(kernelName)).Key ?? "";
			if (string.IsNullOrEmpty(kernelPath))
			{
				this.Log("Kernel file not found: " + kernelName, "", 2);
				return bpms;
			}

			// Load kernel if not loaded
			if (this.Kernel == null || this.KernelFile != kernelPath)
			{
				this.Compiler.LoadKernel(kernelName);
				if (this.Kernel == null || this.KernelFile == null || !this.KernelFile.Contains("\\Audio\\"))
				{
					if (log)
					{
						this.Log("Kernel not loaded or invalid kernel file: " + kernelName, "", 2);
					}
					return bpms;
				}
			}

			// Get input buffers
			ClMem? inputMem = this.MemR.GetBuffer(objPointer);
			if (inputMem == null || inputMem.Count == 0 || inputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Input buffer not found or invalid length: " + objPointer.ToString("X16"), "", 2);
				}
				return bpms;
			}

			// Get variable arguments
			object[] variableArguments = this.Compiler.GetArgumentValues();

			// Check if FFT is needed
			bool didFft = false;
			if (this.Compiler.GetKernelPointerInputType().Name.Contains("Vector2") && inputMem.ElementType == typeof(float))
			{

				IntPtr fftPointer = this.ExecuteFFT(objPointer, 'f', chunkSize, overlap, false, log);
				if (fftPointer == IntPtr.Zero)
				{
					return bpms;
				}
				objPointer = fftPointer;
				didFft = true;

				// Load kernel if not loaded
				if (this.Kernel == null || this.KernelFile != kernelPath)
				{
					this.Compiler.LoadKernel(kernelName);
					if (this.Kernel == null || this.KernelFile == null || !this.KernelFile.Contains("\\Audio\\"))
					{
						if (log)
						{
							this.Log("Kernel not loaded or invalid kernel file: " + kernelName, "", 2);
						}
						return bpms;
					}
				}
			}

			// Get input buffers
			inputMem = this.MemR.GetBuffer(objPointer);
			if (inputMem == null || inputMem.Count == 0 || inputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Input buffer not found or invalid length: " + objPointer.ToString("X16"), "", 2);
				}
				return bpms;
			}

			// Get output buffers
			ClMem? outputMem = null;
			if (this.Compiler.GetKernelPointerOutputType() == typeof(float*))
			{
				outputMem = this.MemR.AllocateGroup<float>(inputMem.Count, resultSizePerChunk);
			}
			else if (this.Compiler.GetKernelPointerOutputType() == typeof(Vector2*))
			{
				outputMem = this.MemR.AllocateGroup<Vector2>(inputMem.Count, resultSizePerChunk);
			}
			else
			{
				if (log)
				{
					this.Log("Unsupported input buffer type: " + inputMem.ElementType.Name, "", 2);
				}
				return bpms;
			}

			// Check output buffers
			if (outputMem == null || outputMem.Count == 0 || outputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Couldn't allocate valid output buffers / lengths", "", 2);
				}
				return bpms;
			}

			// Loop through input buffers
			for (int i = 0; i < inputMem.Count; i++)
			{
				// Get buffers
				CLBuffer inputBuffer = inputMem.Buffers[i];
				CLBuffer outputBuffer = outputMem.Buffers[i];

				// Merge arguments
				List<object> arguments = this.MergeArgumentsAudio(variableArguments, inputBuffer, outputBuffer, length, chunkSize, overlap, samplerate, bitdepth, channels, optionalArguments, false);
				if (arguments == null || arguments.Count == 0)
				{
					if (log)
					{
						this.Log("Failed to merge arguments for buffer " + i, "", 2);
					}
					return bpms;
				}

				// Set kernel arguments
				CLResultCode error = CLResultCode.Success;
				for (uint j = 0; j < arguments.Count; j++)
				{
					error = this.SetKernelArgSafe(j, arguments[(int) j]);
					if (error != CLResultCode.Success)
					{
						if (log)
						{
							this.Log($"Failed to set kernel argument {j} for buffer {i}: " + error, "", 2);
						}
						return bpms;
					}
				}

				// Get work dimensions
				uint maxWorkGroupSize = this.GetMaxWorkGroupSize();
				uint globalWorkSize = (uint) inputMem.Lengths[i];
				uint localWorkSize = Math.Min(maxWorkGroupSize, globalWorkSize);
				if (localWorkSize == 0)
				{
					localWorkSize = 1; // Fallback to 1 if no valid local size
				}
				if (globalWorkSize < localWorkSize)
				{
					globalWorkSize = localWorkSize; // Ensure global size is at least local size
				}

				// Execute kernel
				error = CL.EnqueueNDRangeKernel(this.Queue, this.Kernel.Value, 1, null, [(UIntPtr) globalWorkSize], [(UIntPtr) localWorkSize], 0, null, out CLEvent evt);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to enqueue kernel for buffer {i}: " + error, "", 2);
					}
					return bpms;
				}

				// Wait for completion
				error = CL.WaitForEvents(1, [evt]);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Wait failed for buffer {i}: " + error, "", 2);
					}
				}

				// Release event
				error = CL.ReleaseEvent(evt);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to release event for buffer {i}: " + error, "", 2);
					}
				}
			}

			// Pull results from output buffers
			List<float[]> results = this.MemR.PullChunks<float>(outputMem.IndexHandle);
			if (results == null || results.Count == 0)
			{
				if (log)
				{
					this.Log("No results found in output buffers", "", 2);
				}
				return bpms;
			}

			// Free input buffer if fft was done & free output buffer
			if (didFft && outputMem.ElementType == typeof(Vector2))
			{
				long freed = this.MemR.FreeBuffer(objPointer, true);
				if (freed > 0 && log)
				{
					this.Log("Freed input FFT buffer: " + inputMem.IndexHandle.ToString("X16") + ", Freed " + freed + " Mbytes", "", 2);
				}
			}
			if (outputMem.IndexHandle != IntPtr.Zero)
			{
				long freed = this.MemR.FreeBuffer(outputMem.IndexHandle, true);
				if (freed > 0 && log)
				{
					this.Log("Freed output results buffer: " + outputMem.IndexHandle.ToString("X16") + ", Freed " + freed + " Mbytes", "", 2);
				}
			}

			// Aggregate results
			bpms = new float[results.Count];
			for (int i = 0; i < results.Count; i++)
			{
				bpms[i] = results[i].FirstOrDefault();
			}

			if (log)
			{
				this.Log($"Executed beat scan kernel '{kernelName}' successfully on {inputMem.Count} buffers with chunk size {chunkSize} and overlap {overlap}", "Results: " + bpms.LongLength, 1);
			}

			// Return results
			return bpms;
		}

		public double[] ExecuteBeatZoomKernel(IntPtr objPointer, string kernelName = "beatZoom01", int chunkSize = 8192, float overlap = 0.0f, int samplerate = 44100, int frameRate = 20, float threshold = 0.2f, double minZoom = 1000, double maxZoom = 10000, double zoomMultiplier = 1.05d, bool log = false)
		{
			double[] zooms = [];

			// Get kernel path
			string kernelPath = this.Compiler.Files.FirstOrDefault(f => f.Key.Contains(kernelName)).Key ?? "";
			if (string.IsNullOrEmpty(kernelPath))
			{
				this.Log("Kernel file not found: " + kernelName, "", 2);
				return zooms;
			}

			// Load kernel if not loaded
			if (this.Kernel == null || this.KernelFile != kernelPath)
			{
				this.Compiler.LoadKernel(kernelName);
				if (this.Kernel == null || this.KernelFile == null || !this.KernelFile.Contains("\\Audio\\"))
				{
					if (log)
					{
						this.Log("Kernel not loaded or invalid kernel file: " + kernelName, "", 2);
					}
					return zooms;
				}
			}

			// Get input buffers
			ClMem? inputMem = this.MemR.GetBuffer(objPointer);
			if (inputMem == null || inputMem.Count == 0 || inputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Input buffer not found or invalid length: " + objPointer.ToString("X16"), "", 2);
				}
				return zooms;
			}

			// Get output buffers
			ClMem? outputMem = this.MemR.AllocateGroup<double>(inputMem.Count, (int) ((chunkSize * frameRate) / samplerate));
			if (outputMem == null || outputMem.Count == 0 || outputMem.Lengths.Any(l => l < 1))
			{
				if (log)
				{
					this.Log("Couldn't allocate valid output buffers / lengths", "", 2);
				}
				return zooms;
			}

			// Set arguments
			CLResultCode error = this.SetKernelArgSafe(1, chunkSize);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for chunk size: " + error, "", 2);
				}
				return zooms;
			}
			error = this.SetKernelArgSafe(3, frameRate);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for frame rate: " + error, "", 2);
				}
				return zooms;
			}
			error = this.SetKernelArgSafe(4, samplerate);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for samplerate: " + error, "", 2);
				}
				return zooms;
			}
			error = this.SetKernelArgSafe(5, threshold);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for threshold: " + error, "", 2);
				}
				return zooms;
			}
			error = this.SetKernelArgSafe(6, minZoom);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for minZoom: " + error, "", 2);
				}
				return zooms;
			}
			error = this.SetKernelArgSafe(7, maxZoom);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for maxZoom: " + error, "", 2);
				}
				return zooms;
			}
			error = this.SetKernelArgSafe(8, zoomMultiplier);
			if (error != CLResultCode.Success)
			{
				if (log)
				{
					this.Log("Failed to set kernel argument for zoomMultiplier: " + error, "", 2);
				}
				return zooms;
			}

			// Loop through input buffers
			for (int i = 0; i < inputMem.Count; i++)
			{
				// Get buffers
				CLBuffer inputBuffer = inputMem.Buffers[i];
				CLBuffer outputBuffer = outputMem.Buffers[i];

				// Set buffer args
				error = this.SetKernelArgSafe(0, inputBuffer); // Input buffer
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to set input buffer argument for buffer {i}: " + error, "", 2);
					}
					return zooms;
				}
				error = this.SetKernelArgSafe(2, outputBuffer); // Output buffer
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to set output buffer argument for buffer {i}: " + error, "", 2);
					}
					return zooms;
				}

				// Get work dimensions
				uint maxWorkGroupSize = this.GetMaxWorkGroupSize();
				uint globalWorkSize = (uint) inputMem.Lengths[i];
				uint localWorkSize = Math.Min(maxWorkGroupSize, globalWorkSize);
				if (localWorkSize == 0)
				{
					localWorkSize = 1; // Fallback to 1 if no valid local size
				}
				if (globalWorkSize < localWorkSize)
				{
					globalWorkSize = localWorkSize; // Ensure global size is at least local size
				}

				// Execute kernel
				error = CL.EnqueueNDRangeKernel(this.Queue, this.Kernel.Value, 1, null, [(UIntPtr) globalWorkSize], [(UIntPtr) localWorkSize], 0, null, out CLEvent evt);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to enqueue kernel for buffer {i}: " + error, "", 2);
					}
					return zooms;
				}

				// Wait for completion
				error = CL.WaitForEvents(1, [evt]);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Wait failed for buffer {i}: " + error, "", 2);
					}
				}

				// Release event
				error = CL.ReleaseEvent(evt);
				if (error != CLResultCode.Success)
				{
					if (log)
					{
						this.Log($"Failed to release event for buffer {i}: " + error, "", 2);
					}
				}
			}

			// Pull results from output buffers
			List<double[]> results = this.MemR.PullChunks<double>(outputMem.IndexHandle);
			if (results == null || results.Count == 0)
			{
				if (log)
				{
					this.Log("No results found in output buffers", "", 2);
				}
				return zooms;
			}

			// Aggregate results
			zooms = new double[results.Count];
			for (int i = 0; i < results.Count; i++)
			{

				// Zooms[i] is max value in results[i] array
				zooms[i] = results[i].Max();
			}

			// Log success
			if (log)
			{
				this.Log($"Executed beat zoom kernel '{kernelName}' successfully on {inputMem.Count} buffers with chunk size {chunkSize} and overlap {overlap}", "Results: " + string.Join(", ", zooms), 1);
			}

			return zooms;
		}



		// Helpers		
		public List<object> MergeArgumentsAudio(object[] variableArguments, CLBuffer inputBuffer, CLBuffer outputBuffer, long length, int chunkSize, float overlap, int samplerate, int bitdepth, int channels, Dictionary<string, object>? optionalArgs = null, bool log = false)
		{
			List<object> arguments = [];

			// Make overlap to size
			int overlapSize = (int) (overlap * chunkSize);

			// Get argument definitions
			Dictionary<string, Type> definitions = this.Compiler.GetKernelArguments();
			if (definitions == null || definitions.Count == 0)
			{
				this.Log("No argument definitions found", "", 2);
				return arguments;
			}

			// Merge args
			int found = 0;
			for (int i = 0; i < definitions.Count; i++)
			{
				string key = definitions.Keys.ElementAt(i);
				Type type = definitions[key];
				if (type.Name.Contains("*") && key.Contains("in"))
				{
					if (log)
					{
						this.Log($"Adding input buffer for key '{key}'", "", 2);
					}
					arguments.Add(inputBuffer);
					found++;
				}
				else if (type.Name.Contains("*") && key.Contains("out"))
				{
					if (log)
					{
						this.Log($"Adding output buffer for key '{key}'", "", 2);
					}
					arguments.Add(outputBuffer);
					found++;
				}
				else if ((type == typeof(long) || type == typeof(int)) && key.Contains("len"))
				{
					if (log)
					{
						this.Log($"Adding length for key '{key}': {(chunkSize > 0 ? chunkSize : length)}", "", 2);
					}
					arguments.Add(chunkSize > 0 ? chunkSize : length);
					found++;
				}
				else if (type == typeof(int) && key.Contains("chunk"))
				{
					if (log)
					{
						this.Log($"Adding chunk size for key '{key}': {chunkSize}", "", 2);
					}
					arguments.Add(chunkSize);
					found++;
				}
				else if (type == typeof(int) && key.Contains("overlap"))
				{
					if (log)
					{
						this.Log($"Adding overlap size for key '{key}': {overlapSize}", "", 2);
					}
					arguments.Add(overlapSize);
					found++;
				}
				else if (type == typeof(int) && key == "samplerate")
				{
					if (log)
					{
						this.Log($"Adding samplerate for key '{key}': {samplerate}", "", 2);
					}
					arguments.Add(samplerate);
					found++;
				}
				else if (type == typeof(int) && key == "bit")
				{
					if (log)
					{
						this.Log($"Adding bitdepth for key '{key}': {bitdepth}", "", 2);
					}
					arguments.Add(bitdepth);
					found++;
				}
				else if (type == typeof(int) && key == "channel")
				{
					if (log)
					{
						this.Log($"Adding channels for key '{key}': {channels}", "", 2);
					}
					arguments.Add(channels);
					found++;
				}
				else
				{
					if (found < variableArguments.Length)
					{
						if (log)
						{
							this.Log($"Adding variable argument for key '{key}': {variableArguments[found]}", "", 2);
						}
						arguments.Add(variableArguments[found]);
						found++;
					}
					else
					{
						if (log)
						{
							this.Log($"Missing variable argument for key '{key}'", "", 2);
						}
						return arguments; // Return early if a required argument is missing
					}
				}
			}

			// Integrate / replace with optional arguments
			if (optionalArgs != null && optionalArgs.Count > 0)
			{
				foreach (var kvp in optionalArgs)
				{
					string key = kvp.Key.ToLowerInvariant();
					object value = kvp.Value;
					
					// Find matching argument by name
					int index = definitions.Keys.ToList().FindIndex(k => k.ToLower().Contains(key.ToLower()));
					if (index >= 0 && index < arguments.Count)
					{
						if (log)
						{
							this.Log($"Replacing argument '{definitions.Keys.ElementAt(index)}' with optional value: {value}", "", 2);
						}
						arguments[index] = value; // Replace existing argument
					}
					else
					{
						if (log)
						{
							this.Log($"Adding new optional argument '{key}': {value}", "", 2);
						}
						arguments.Add(value); // Add new optional argument
					}
				}
			}

			return arguments;
		}

		public List<object> MergeArgumentsImage(object[] arguments, IntPtr inputPointer = 0, IntPtr outputPointer = 0, int width = 0, int height = 0, int channels = 4, int bitdepth = 8, bool log = false)
		{
			List<object> result = [];

			// Get kernel arguments
			Dictionary<string, Type> kernelArguments = this.Compiler.GetKernelArguments(this.Kernel);
			if (kernelArguments.Count == 0)
			{
				this.Log("Kernel arguments not found", "", 2);
				kernelArguments = this.Compiler.GetKernelArgumentsAnalog(this.KernelFile);
				if (kernelArguments.Count == 0)
				{
					this.Log("Kernel arguments not found", "", 2);
					return [];
				}
			}
			int bpp = bitdepth * channels;

			// Match arguments to kernel arguments
			bool inputFound = false;
			for (int i = 0; i < kernelArguments.Count; i++)
			{
				string argName = kernelArguments.ElementAt(i).Key;
				Type argType = kernelArguments.ElementAt(i).Value;

				// If argument is pointer -> add pointer
				if (argType.Name.EndsWith("*"))
				{
					// Get pointer value
					IntPtr argPointer = 0;
					if (!inputFound)
					{
						argPointer = arguments[i] is IntPtr ? (IntPtr) arguments[i] : inputPointer;
						inputFound = true;
					}
					else
					{
						argPointer = arguments[i] is IntPtr ? (IntPtr) arguments[i] : outputPointer;
					}

					// Get buffer
					ClMem? argBuffer = this.MemR.GetBuffer(argPointer);
					if (argBuffer == null || argBuffer.IndexLength == IntPtr.Zero)
					{
						this.Log("Argument buffer not found or invalid length: " + argPointer.ToString("X16"), argBuffer?.IndexLength.ToString() ?? "None", 2);
						return [];
					}
					CLBuffer buffer = argBuffer.Buffers.FirstOrDefault();

					// Add pointer to result
					result.Add(buffer);

					// Log buffer found
					if (log)
					{
						// Log buffer found
						this.Log("Kernel argument buffer found: " + argPointer.ToString("X16"), "Index: " + i, 3);
					}
				}
				else if (argType == typeof(int))
				{
					// If name is "width" or "height" -> add width or height
					if (argName.ToLower() == "width")
					{
						result.Add(width <= 0 ? arguments[i] : width);

						// Log width found
						if (log)
						{
							this.Log("Kernel argument width found: " + width.ToString(), "Index: " + i, 3);
						}
					}
					else if (argName.ToLower() == "height")
					{
						result.Add(height <= 0 ? arguments[i] : height);

						// Log height found
						if (log)
						{
							this.Log("Kernel argument height found: " + height.ToString(), "Index: " + i, 3);
						}
					}
					else if (argName.ToLower() == "channels")
					{
						result.Add(channels <= 0 ? arguments[i] : channels);

						// Log channels found
						if (log)
						{
							this.Log("Kernel argument channels found: " + channels.ToString(), "Index: " + i, 3);
						}
					}
					else if (argName.ToLower() == "bitdepth")
					{
						result.Add(bitdepth <= 0 ? arguments[i] : bitdepth);

						// Log channels found
						if (log)
						{
							this.Log("Kernel argument bitdepth found: " + bitdepth.ToString(), "Index: " + i, 3);
						}
					}
					else if (argName.ToLower() == "bpp")
					{
						result.Add(bpp <= 0 ? arguments[i] : bpp);

						// Log channels found
						if (log)
						{
							this.Log("Kernel argument bpp found: " + bpp.ToString(), "Index: " + i, 3);
						}
					}
					else
					{
						result.Add((int) arguments[Math.Min(arguments.Length - 1, i)]);
					}
				}
				else if (argType == typeof(float))
				{
					// Sicher konvertieren
					result.Add(Convert.ToSingle(arguments[i]));
				}
				else if (argType == typeof(double))
				{
					result.Add(Convert.ToDouble(arguments[i]));
				}
				else if (argType == typeof(long))
				{
					result.Add((long) arguments[i]);
				}
			}

			// Log arguments
			if (log)
			{
				this.Log("Kernel arguments: " + string.Join(", ", result.Select(a => a.ToString())), "'" + Path.GetFileName(this.KernelFile) + "'", 2);
			}

			return result;
		}

		public CLResultCode SetKernelArgSafe(uint index, object value)
		{
			// Check kernel
			if (this.Kernel == null)
			{
				this.Log("Kernel is null");
				return CLResultCode.InvalidKernelDefinition;
			}

			switch (value)
			{
				case CLBuffer buffer:
					return CL.SetKernelArg(this.Kernel.Value, index, buffer);

				case int i:
					return CL.SetKernelArg(this.Kernel.Value, index, i);

				case long l:
					return CL.SetKernelArg(this.Kernel.Value, index, l);

				case float f:
					return CL.SetKernelArg(this.Kernel.Value, index, f);

				case double d:
					return CL.SetKernelArg(this.Kernel.Value, index, d);

				case byte b:
					return CL.SetKernelArg(this.Kernel.Value, index, b);

				case IntPtr ptr:
					return CL.SetKernelArg(this.Kernel.Value, index, ptr);

				// Spezialfall für lokalen Speicher (Größe als uint)
				case uint u:
					return CL.SetKernelArg(this.Kernel.Value, index, new IntPtr(u));

				// Fall für Vector2
				case Vector2 v:
					// Vector2 ist ein Struct, daher muss es als Array übergeben werden
					return CL.SetKernelArg(this.Kernel.Value, index, v);

				default:
					throw new ArgumentException($"Unsupported argument type: {value?.GetType().Name ?? "null"}");
			}
		}

		private uint GetMaxWorkGroupSize()
		{
			const uint FALLBACK_SIZE = 64;
			const string FUNCTION_NAME = "GetMaxWorkGroupSize";

			if (!this.Kernel.HasValue)
			{
				this.Log("Kernel not initialized", FUNCTION_NAME, 2);
				return FALLBACK_SIZE;
			}

			try
			{
				// 1. Zuerst die benötigte Puffergröße ermitteln
				CLResultCode result = CL.GetKernelWorkGroupInfo(
					this.Kernel.Value,
					this.Device,
					KernelWorkGroupInfo.WorkGroupSize,
					UIntPtr.Zero,
					null,
					out nuint requiredSize);

				if (result != CLResultCode.Success || requiredSize == 0)
				{
					this.Log($"Failed to get required size: {result}", FUNCTION_NAME, 2);
					return FALLBACK_SIZE;
				}

				// 2. Puffer mit korrekter Größe erstellen
				byte[] paramValue = new byte[requiredSize];

				// 3. Tatsächliche Abfrage durchführen
				result = CL.GetKernelWorkGroupInfo(
					this.Kernel.Value,
					this.Device,
					KernelWorkGroupInfo.WorkGroupSize,
					new UIntPtr(requiredSize),
					paramValue,
					out _);

				if (result != CLResultCode.Success)
				{
					this.Log($"Failed to get work group size: {result}", FUNCTION_NAME, 2);
					return FALLBACK_SIZE;
				}

				// 4. Ergebnis konvertieren (abhängig von der Plattform)
				uint maxSize;
				if (requiredSize == sizeof(uint))
				{
					maxSize = BitConverter.ToUInt32(paramValue, 0);
				}
				else if (requiredSize == sizeof(ulong))
				{
					maxSize = (uint) BitConverter.ToUInt64(paramValue, 0);
				}
				else
				{
					this.Log($"Unexpected return size: {requiredSize}", FUNCTION_NAME, 2);
					return FALLBACK_SIZE;
				}

				// 5. Gültigen Wert sicherstellen
				if (maxSize == 0)
				{
					this.Log("Device reported max work group size of 0", FUNCTION_NAME, 2);
					return FALLBACK_SIZE;
				}

				return maxSize;
			}
			catch (Exception ex)
			{
				this.Log($"Error in {FUNCTION_NAME}: {ex.Message}", ex.StackTrace ?? "", 3);
				return FALLBACK_SIZE;
			}
		}




		// ----- ----- ----- ACCESSIBLE METHODS ----- ----- ----- \\


	}
}