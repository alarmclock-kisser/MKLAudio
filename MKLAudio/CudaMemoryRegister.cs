using ManagedCuda;
using ManagedCuda.BasicTypes;
using System.Linq;

namespace MKLAudio
{
	public class CudaMemoryRegister
	{
		private String Repopath;
		private ListBox LogList;
		private ProgressBar PBar;
		private PrimaryContext Context;
		private CUdevice Device;
		private List<CudaStream> Streams = [];


		public List<CudaMem> Memory { get; private set; } = [];

		public CudaMemoryRegister(String repopath, ListBox logList, ProgressBar pBar, PrimaryContext context, CUdevice device, List<CudaStream> streams)
		{
			// Set attributes
			this.Repopath = repopath;
			this.LogList = logList;
			this.PBar = pBar;
			this.Context = context;
			this.Device = device;
			this.Streams = streams;

			// Init queue




		}



		public string Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[CUDA-Mem]: " + new string(' ', indent * 2) + message;

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


		public CudaMem? FindMemory(IntPtr pointer , bool log = false)
		{
			// Find memory object by pointer
			CudaMem? mem = this.Memory.FirstOrDefault(x => x.IndexPointer == pointer);
			if (mem != null)
			{
				if (log)
				{
					this.Log("Found memory object with pointer: " + pointer, "", 1);
				}
				return mem;
			}
			else if (log)
			{
				this.Log("No memory object with pointer " + pointer + " found.", "", 1);
			}

			// Not found
			return null;
		}

		public long FreeBuffers(IntPtr pointer, bool readable = false, bool log = false)
		{
			long freed = 0;

			// Find memory object by pointer
			CudaMem? mem = this.FindMemory(pointer, log);
			if (mem == null)
			{
				if (log)
				{
					this.Log("No memory object with pointer " + pointer + " found to free.", "", 1);
				}
				return freed;
			}

			freed = mem.Size;
			if (readable)
			{
				freed = freed / 1024 / 1024; // Convert to MB
			}

			// Free all buffers in memory object
			foreach (var buf in mem.Buffers)
			{
				this.Context.FreeMemory(buf);
			}

			mem.Buffers = [];
			mem.Lengths = [];

			this.Memory.Remove(mem);

			if (log)
			{
				this.Log("Freed " + freed + (readable ? " MB" : " bytes") + " from memory object with pointer: " + pointer, "", 1);
			}

			return freed;
		}

		public void Dispose(bool log = false)
		{
			// Free every mem obj
			foreach (var buffer in this.Memory)
			{
				this.FreeBuffers(buffer.IndexPointer, false, log);
			}

			this.Memory.Clear();
		}


		public CudaMem? PushData<T>(T[] data, bool log = false) where T : unmanaged
		{
			if (data == null || data.Length == 0)
			{
				if (log)
				{
					this.Log("No data to push.", "", 1);
				}
				return null;
			}

			// Get attributes
			IntPtr length = (nint) (data.LongLength);
			CudaDeviceVariable<T> devVar = new(data.LongLength);
			CUdeviceptr pointer = devVar.DevicePointer;

			// Copy data to device
			devVar.CopyToDevice(data);

			// Build CudaMem obj
			CudaMem mem = new CudaMem
			{
				Buffers = [pointer],
				ElementType = typeof(T),
				Lengths = [length]
			};

			this.Memory.Add(mem);
			
			if (log)
			{
				this.Log("Pushed " + data.LongLength + " elements of type " + typeof(T).Name + " to device memory at pointer: " + pointer, "", 1);
			}

			return mem;
		}

		public T[] PullData<T>(IntPtr pointer, bool free = false, bool log = false) where T : unmanaged
		{
			// Find memory object by pointer
			CudaMem? mem = this.FindMemory(pointer, log);
			if (mem == null)
			{
				if (log)
				{
					this.Log("No memory object with pointer " + pointer + " found to pull data from.", "", 1);
				}
				return [];
			}

			// Check type
			if (mem.ElementType != typeof(T))
			{
				if (log)
				{
					this.Log("Memory object with pointer " + pointer + " has type " + mem.ElementType.Name + ", but requested type is " + typeof(T).Name, "", 1);
				}
				return [];
			}

			// Create data array
			T[] data = new T[mem.TotalElements];

			// Loop through buffers and copy data
			for (int i = 0; i < mem.Buffers.Length; i++)
			{
				T[] chunk = new T[mem.Lengths[i]];
				CudaDeviceVariable<T> devVar = new(mem.Buffers[i], mem.Lengths[i]);
				devVar.CopyToHost(chunk);
				chunk.CopyTo(data, i * mem.Lengths[i]);
			}

			// Free buffers if requested
			if (free)
			{
				this.FreeBuffers(pointer, false, log);
			}

			if (log)
			{
				this.Log("Pulled " + data.LongLength + " elements of type " + typeof(T).Name + " from device memory at pointer: " + pointer, "", 1);
			}

			return data;
		}


		public CudaMem? PushChunks<T>(List<T[]> chunks, bool log = false) where T : unmanaged
		{
			// Check chunks
			if (chunks == null || chunks.Count == 0)
			{
				if (log)
				{
					this.Log("No chunks to push.", "", 1);
				}
				return null;
			}
			
			// Get attributes
			CudaMem mem = new()
			{
				ElementType = typeof(T),
				Buffers = new CUdeviceptr[chunks.Count],
				Lengths = new IntPtr[chunks.Count]
			};

			// Loop through chunks and copy data
			for (int i = 0; i < chunks.Count; i++)
			{
				T[] data = chunks[i];
				IntPtr length = (nint) (data.LongLength);
				CudaDeviceVariable<T> devVar = new(data.LongLength);
				CUdeviceptr pointer = devVar.DevicePointer;
				
				// Copy data to device
				devVar.CopyToDevice(data);
				mem.Buffers[i] = pointer;
				mem.Lengths[i] = length;
			}

			// Add to memory register
			this.Memory.Add(mem);


			if (log)
			{
				this.Log("Pushed " + mem.TotalElements + " elements of type " + typeof(T).Name + " in " + mem.Buffers.Length + " chunks to device memory at pointer: " + mem.IndexPointer, "", 1);
			}

			return mem;
		}

		public List<T[]> PullChunks<T>(IntPtr pointer, bool free = false, bool log = false) where T : unmanaged
		{
			// Find memory object by pointer
			CudaMem? mem = this.FindMemory(pointer, log);
			if (mem == null)
			{
				if (log)
				{
					this.Log("No memory object with pointer " + pointer + " found to pull chunks from.", "", 1);
				}
				return [];
			}

			// Check type
			if (mem.ElementType != typeof(T))
			{
				if (log)
				{
					this.Log("Memory object with pointer " + pointer + " has type " + mem.ElementType.Name + ", but requested type is " + typeof(T).Name, "", 1);
				}
				return [];
			}
			List<T[]> chunks = [];
			
			// Loop through buffers and copy data
			for (int i = 0; i < mem.Buffers.Length; i++)
			{
				T[] chunk = new T[mem.Lengths[i]];
				CudaDeviceVariable<T> devVar = new(mem.Buffers[i], mem.Lengths[i]);
				devVar.CopyToHost(chunk);
				chunks.Add(chunk);
			}

			// Free buffers if requested
			if (free)
			{
				this.FreeBuffers(pointer, false, log);
			}

			if (log)
			{
				this.Log("Pulled " + chunks.Count + " chunks of type " + typeof(T).Name + " from device memory at pointer: " + pointer, "", 1);
			}
			
			return chunks;
		}

		public CudaMem? AllocateSingle<T>(IntPtr length, bool log = false) where T : unmanaged
		{
			if (length <= 0)
			{
				if (log)
				{
					this.Log("No length to allocate.", "", 1);
				}
				return null;
			}

			CudaDeviceVariable<T> devVar = new(length);
			CUdeviceptr pointer = devVar.DevicePointer;
			
			// Build CudaMem obj
			CudaMem mem = new CudaMem
			{
				Buffers = [pointer],
				ElementType = typeof(T),
				Lengths = [length]
			};

			this.Memory.Add(mem);
			if (log)
			{
				this.Log("Allocated " + length + " elements of type " + typeof(T).Name + " to device memory at pointer: " + pointer, "", 1);
			}
			
			return mem;
		}

		public CudaMem? AllocateArray<T>(IntPtr[] lengths, bool log = false) where T : unmanaged
		{
			if (lengths == null || lengths.Length == 0 || lengths.Any(x => x <= 0))
			{
				if (log)
				{
					this.Log("No lengths to allocate.", "", 1);
				}
				return null;
			}
			
			CudaMem mem = new CudaMem
			{
				ElementType = typeof(T),
				Buffers = new CUdeviceptr[lengths.Length],
				Lengths = new IntPtr[lengths.Length]
			};

			for (int i = 0; i < lengths.Length; i++)
			{
				CudaDeviceVariable<T> devVar = new(lengths[i]);
				mem.Buffers[i] = devVar.DevicePointer;
				mem.Lengths[i] = lengths[i];
			}
			
			this.Memory.Add(mem);
			
			if (log)
			{
				this.Log("Allocated " + mem.TotalElements + " elements of type " + typeof(T).Name + " in " + mem.Buffers.Length + " buffers to device memory at pointer: " + mem.IndexPointer, "", 1);
			}
			
			return mem;
		}


	}




	public class CudaMem
	{
		public CUdeviceptr[] Buffers { get; set; } = [];
		public IntPtr[] Lengths { get;  set; } = [];

		public Type ElementType { get;  set; } = typeof(object);


		public IntPtr Count => (nint) (this.Buffers.LongLength == this.Lengths.LongLength ? this.Buffers.LongLength : 0);
		public IntPtr TotalElements => (nint) (this.Lengths.LongLength == this.Buffers.LongLength ? this.Lengths.Sum(x => (long) x) : 0);
		public IntPtr IndexPointer => this.Buffers.FirstOrDefault().Pointer;
		public IntPtr IndexLength => this.Lengths.FirstOrDefault();
		public IntPtr Size => (nint) (this.Count * System.Runtime.InteropServices.Marshal.SizeOf(this.ElementType) * this.TotalElements);

	}
		
}