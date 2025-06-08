using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.CudaFFT;
using ManagedCuda.VectorTypes;
using System.Runtime.InteropServices;

namespace MKLAudio
{
	public class CudaKernelExecutioner
	{
		private String Repopath;
		private ListBox LogList;
		private ProgressBar PBar;
		private PrimaryContext Context;
		private CUdevice Device;
		private List<CudaStream> Streams = [];
		private CudaMemoryRegister MemoryRegister;
		private CudaKernelCompiler KernelCompiler;

		public CudaKernelExecutioner(String repopath, ListBox logList, ProgressBar pBar, PrimaryContext context, CUdevice device, List<CudaStream> streams, CudaMemoryRegister memoryRegister, CudaKernelCompiler kernelCompiler)
		{
			this.Repopath = repopath;
			this.LogList = logList;
			this.PBar = pBar;
			this.Context = context;
			this.Device= device;
			this.Streams = streams;
			this.MemoryRegister = memoryRegister;
			this.KernelCompiler = kernelCompiler;
		}




		public string Log(string message = "", string inner = "", int indent = 0)
		{
			string msg = "[CUDA-Kernel]: " + new string(' ', indent * 2) + message;

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
			// Dispose kernel etc.
		}



		
	}
}