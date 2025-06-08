using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaFFT;
using ManagedCuda.VectorTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKLAudio
{
	public class CudaFourierHandling
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- 
		private string Repopath;
		private ListBox LogList;
		private ProgressBar PBar;
		private PrimaryContext Context;
		private CUdevice Device;
		private List<CudaStream> Streams;
		private CudaMemoryRegister MemoryRegister;




		// ----- ----- ----- LAMBDA ----- ----- ----- \\



		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public CudaFourierHandling(string repopath, ListBox logList, ProgressBar pBar, PrimaryContext context, CUdevice device, List<CudaStream> streams, CudaMemoryRegister memoryRegister)
		{
			// Set attributes
			this.Repopath = repopath;
			this.LogList = logList;
			this.PBar = pBar;
			this.Context = context;
			this.Device = device;
			this.Streams = streams;
			this.MemoryRegister = memoryRegister;


		}




		// ----- ----- ----- METHODS ----- ----- ----- \\
		public string Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[CuFFT]: " + new string(' ', indent * 2) + message;

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

		public void Dispose()
		{
			// Dispose compiler etc.

		}



		public async Task<CudaMem?> ExecuteFftAsync(CudaMem mem, bool log = false)
		{
			// Log the start of execution
			this.Log("Starting FFT execution for memory: " + mem.IndexPointer.ToString("X16"), "", 1);

			// Find the memory object in the register
			CudaMem? cudaMem = this.MemoryRegister.FindMemory(mem.IndexPointer, log);
			if (cudaMem == null)
			{
				if (log)
				{
					// Log memory not found
					this.Log("Memory not found in register", "Pointer: " + mem.IndexPointer.ToString("X16"), 2);
				}
				return null;
			}

			// Validate we have enough streams
			if (cudaMem.Count > this.Streams.Count)
			{
				if (log)
				{
					// Log insufficient streams
					this.Log("Not enough streams available", $"Required: {cudaMem.Count}, Available: {this.Streams.Count}", 2);
				}
				return null;
			}

			// Build new CudaMem object for the execution
			CudaMem fftMem = new CudaMem
			{
				Buffers = new CUdeviceptr[cudaMem.Count],
				Lengths = new IntPtr[cudaMem.Count],
				ElementType = typeof(float2),
			};

			try
			{
				// Create FFT plans for each stream
				var fftPlans = new CudaFFTPlan1D[cudaMem.Count];

				// Prepare tasks for parallel execution
				var fftTasks = new Task[cudaMem.Count];

				for (int i = 0; i < cudaMem.Count; i++)
				{
					int streamIndex = i % this.Streams.Count; // Cycle through available streams
					int bufferIndex = i; // Capture for closure

					fftTasks[i] = Task.Run(() =>
					{
						CudaStream stream = this.Streams[streamIndex];

						// Create FFT plan for this stream
						fftPlans[bufferIndex] = new CudaFFTPlan1D(
							(int) cudaMem.Lengths[bufferIndex],
							cufftType.R2C,
							(int) cudaMem.Lengths[bufferIndex],
							stream.Stream
						);

						// Allocate output buffer
						fftMem.Buffers[bufferIndex] = new CudaDeviceVariable<float2>(
							(SizeT) (cudaMem.Lengths[bufferIndex])
						).DevicePointer;

						// Set lengths
						fftMem.Lengths[bufferIndex] = cudaMem.Lengths[bufferIndex];

						// Execute FFT
						fftPlans[bufferIndex].Exec(
							cudaMem.Buffers[bufferIndex],
							fftMem.Buffers[bufferIndex],
							TransformDirection.Forward
						);
					});
				}

				// Wait for all FFT operations to complete
				await Task.WhenAll(fftTasks);

				// Cleanup FFT plans
				foreach (var plan in fftPlans)
				{
					plan?.Dispose();
				}

				return fftMem;
			}
			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Error during FFT execution", ex.Message, 2);
				}
				return mem;
			}
		}

		public async Task<CudaMem?> ExecuteIfftAsync(CudaMem mem, bool log = false)
		{
			// Log the start of execution
			if (log)
			{
				this.Log("Starting IFFT execution for memory: " + mem.IndexPointer.ToString("X16"), "", 1);
			}

			// Find the memory object in the register
			CudaMem? cudaMem = this.MemoryRegister.FindMemory(mem.IndexPointer, log);
			if (cudaMem == null)
			{
				if (log)
				{
					this.Log("Memory not found in register", "Pointer: " + mem.IndexPointer.ToString("X16"), 2);
				}
				return null;
			}

			// Validate we have enough streams
			if (cudaMem.Count > this.Streams.Count)
			{
				if (log)
				{
					this.Log("Not enough streams available", $"Required: {cudaMem.Count}, Available: {this.Streams.Count}", 2);
				}
				return null;
			}

			// Build new CudaMem object for the execution
			CudaMem ifftMem = new CudaMem
			{
				Buffers = new CUdeviceptr[cudaMem.Count],
				Lengths = new IntPtr[cudaMem.Count],
				ElementType = typeof(float2),
			};

			try
			{
				// Create FFT plans for each stream
				var fftPlans = new CudaFFTPlan1D[cudaMem.Count];

				// Prepare tasks for parallel execution
				var ifftTasks = new Task[cudaMem.Count];

				for (int i = 0; i < cudaMem.Count; i++)
				{
					int streamIndex = i % this.Streams.Count; // Cycle through available streams
					int bufferIndex = i; // Capture for closure

					ifftTasks[i] = Task.Run(() =>
					{
						CudaStream stream = this.Streams[streamIndex];

						// Create FFT plan for this stream (note same parameters as FFT)
						fftPlans[bufferIndex] = new CudaFFTPlan1D(
							(int) cudaMem.Lengths[bufferIndex],
							cufftType.C2C, // Changed from R2C to C2C for inverse
							(int) cudaMem.Lengths[bufferIndex],
							stream.Stream
						);

						// Allocate output buffer
						ifftMem.Buffers[bufferIndex] = new CudaDeviceVariable<float2>(
							(SizeT) (cudaMem.Lengths[bufferIndex])
						).DevicePointer;

						// Set lengths
						ifftMem.Lengths[bufferIndex] = cudaMem.Lengths[bufferIndex];

						// Execute INVERSE FFT
						fftPlans[bufferIndex].Exec(
							cudaMem.Buffers[bufferIndex],
							ifftMem.Buffers[bufferIndex],
							TransformDirection.Inverse // Changed to Inverse
						);
					});
				}

				// Wait for all IFFT operations to complete
				await Task.WhenAll(ifftTasks);

				// Cleanup FFT plans
				foreach (var plan in fftPlans)
				{
					plan?.Dispose();
				}

				return ifftMem;
			}
			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Error during IFFT execution", ex.Message, 2);
				}
				return mem;
			}
		}

		public async Task<CudaMem?> PerformFFTUnsync(CudaMem mem, char currentForm, bool log = false)
		{
			if (mem == null || mem.Buffers == null || mem.Lengths == null)
			{
				if (log)
				{
					this.Log("Invalid memory object", "", 1);
				}

				return null;
			}

			try
			{
				int batchSize = this.Streams.Count;
				IntPtr totalChunks = mem.Count;
				var resultMem = new CudaMem
				{
					Buffers = new CUdeviceptr[totalChunks],
					Lengths = new IntPtr[totalChunks],
					ElementType = typeof(float2)
				};

				for (int batchStart = 0; batchStart < totalChunks; batchStart += batchSize)
				{
					IntPtr currentBatchSize = Math.Min(batchSize, (totalChunks - batchStart));
					var batchTasks = new Task[currentBatchSize];

					for (int i = 0; i < currentBatchSize; i++)
					{
						int chunkIndex = batchStart + i;
						int streamIndex = i % this.Streams.Count;

						batchTasks[i] = Task.Run(() =>
						{
							var stream = this.Streams[streamIndex];
							var cufftType = currentForm == 'f' ? ManagedCuda.CudaFFT.cufftType.R2C : ManagedCuda.CudaFFT.cufftType.C2R;
							var direction = currentForm == 'f' ? TransformDirection.Forward : TransformDirection.Inverse;

							using var plan = new CudaFFTPlan1D(
								(int) mem.Lengths[chunkIndex],
								cufftType,
								(int) mem.Lengths[chunkIndex],
								stream.Stream);
							using var resultBuffer = new CudaDeviceVariable<float2>((SizeT) mem.Lengths[chunkIndex]);
							plan.Exec(
								mem.Buffers[chunkIndex],
								resultBuffer.DevicePointer,
								direction
							);

							resultMem.Buffers[chunkIndex] = resultBuffer.DevicePointer;
							resultMem.Lengths[chunkIndex] = mem.Lengths[chunkIndex];

						});
					}

					await Task.WhenAll(batchTasks).ConfigureAwait(false);
				}

				if (log)
				{
					this.Log("Transform completed successfully", $"Processed {totalChunks} chunks", 1);
				}

				return resultMem;
			}
			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Transform failed", ex.Message, 2);
				}

				return null;
			}
		}

		public async Task<CudaMem?> PerformFFTAsync(CudaMem mem, char currentForm, bool log = false)
		{
			if (mem == null || mem.Buffers == null || mem.Lengths == null)
			{
				if (log)
				{
					this.Log("Invalid memory object", "", 1);
				}

				return null;
			}

			try
			{
				int batchSize = this.Streams.Count;
				IntPtr totalChunks = mem.Count;
				var resultMem = new CudaMem
				{
					Buffers = new CUdeviceptr[totalChunks],
					Lengths = new IntPtr[totalChunks],
					ElementType = typeof(float2)
				};

				// Ensure context is set before parallel operations
				using (var ctx = new PrimaryContext(this.Device))
				{
					ctx.SetCurrent();

					for (int batchStart = 0; batchStart < totalChunks; batchStart += batchSize)
					{
						IntPtr currentBatchSize = Math.Min(batchSize, totalChunks - batchStart);
						var batchTasks = new Task[currentBatchSize];

						for (int i = 0; i < currentBatchSize; i++)
						{
							int chunkIndex = batchStart + i;
							int streamIndex = i % this.Streams.Count;

							batchTasks[i] = Task.Run(() =>
							{
								// Ensure context is bound to this thread
								ctx.SetCurrent();

								var stream = this.Streams[streamIndex];
								var cufftType = currentForm == 'f' ? ManagedCuda.CudaFFT.cufftType.R2C : ManagedCuda.CudaFFT.cufftType.C2R;
								var direction = currentForm == 'f' ? TransformDirection.Forward : TransformDirection.Inverse;

								using (var plan = new CudaFFTPlan1D(
									(int) mem.Lengths[chunkIndex],
									cufftType,
									(int) mem.Lengths[chunkIndex],
									stream.Stream))
								using (var resultBuffer = new CudaDeviceVariable<float2>((SizeT) mem.Lengths[chunkIndex]))
								{
									plan.Exec(
										mem.Buffers[chunkIndex],
										resultBuffer.DevicePointer,
										direction
									);

									resultMem.Buffers[chunkIndex] = resultBuffer.DevicePointer;
									resultMem.Lengths[chunkIndex] = mem.Lengths[chunkIndex];
									// resultBuffer.Detach(); // Prevent disposal
								}
							});
						}

						await Task.WhenAll(batchTasks).ConfigureAwait(false);
					}
				}

				if (log)
				{
					this.Log("Transform completed successfully", $"Processed {totalChunks} chunks", 1);
				}

				return resultMem;
			}
			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Transform failed", ex.Message, 2);
				}

				return null;
			}
		}

		public CudaMem? PerformFFT(CudaMem mem, char currentForm, bool log = false)
		{
			if (mem == null || mem.Buffers == null || mem.Lengths == null)
			{
				if (log)
				{
					this.Log("Invalid memory object", "", 1);
				}

				return null;
			}
			try
			{
				// Create single CuFFT plan, call on every pointer in mem.Buffers
				CudaFFTPlan1D plan = new CudaFFTPlan1D(
					(int)mem.Lengths[0],
					currentForm == 'f' ? cufftType.R2C : cufftType.C2R,
					1
				);

				CudaMem resultMem = new()
				{
					Buffers = new CUdeviceptr[mem.Buffers.Length],
					Lengths = new IntPtr[mem.Lengths.Length],
					ElementType = currentForm == 'f' ? typeof(float2) : typeof(float)
				};

				for (int i = 0; i < mem.Buffers.Length; i++)
				{
					// Create a new device variable for the result
					CudaDeviceVariable<float2> resultBuffer = new CudaDeviceVariable<float2>((SizeT)mem.Lengths[i]);
					resultMem.Buffers[i] = resultBuffer.DevicePointer;
					resultMem.Lengths[i] = mem.Lengths[i];
					
					// Execute the FFT/IFFT
					plan.Exec(
						mem.Buffers[i],
						resultBuffer.DevicePointer,
						currentForm == 'f' ? TransformDirection.Forward : TransformDirection.Inverse
					);
				}

				// Dispose the plan after use
				plan.Dispose();

				// Log
				if (log)
				{
					this.Log("Transform completed successfully", $"Processed {mem.Buffers.Length} chunks", 1);
				}

				// Return the result memory object
				this.MemoryRegister.Memory.Add(resultMem); // Register the result memory

				return resultMem;
			}

			catch (Exception ex)
			{
				if (log)
				{
					this.Log("Transform failed", ex.Message, 2);
				}
			}

			return null;
		}
	}
}