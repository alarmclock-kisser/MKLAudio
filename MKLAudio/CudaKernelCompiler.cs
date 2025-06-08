using ManagedCuda;
using ManagedCuda.BasicTypes;

namespace MKLAudio
{
	public class CudaKernelCompiler
	{
		private String Repopath;
		private ListBox LogList;
		private ProgressBar PBar;
		private PrimaryContext Context;
		private CUdevice Device;
		private List<CudaStream> Streams = [];
		private CudaMemoryRegister MemoryRegister;

		public CudaKernelCompiler(String repopath, ListBox logList, ProgressBar pBar, PrimaryContext context, CUdevice device, List<CudaStream> streams, CudaMemoryRegister memoryRegister)
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




		public string Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[CUDA-NVRTC]: " + new string(' ', indent * 2) + message;

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
	}
}