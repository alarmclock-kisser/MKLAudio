namespace MKLAudio.Tests
{
	[TestClass]
	public sealed class OpenClMemoryRegisterTests
	{
		private string Repopath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));

		private OpenClService? Service;
		private AudioHandling? AudioH;

		[TestInitialize]
		public void Initialize()
		{
			this.Service = new OpenClService(this.Repopath, null, null);
			this.AudioH = new AudioHandling(this.Repopath, null, null, null, null, null, null, null, null);

			this.Service?.SelectDeviceLike("Intel");
		}

		[TestCleanup]
		public void Cleanup()
		{
			this.Service?.Dispose();
			this.Service = null;

			this.AudioH?.Dispose();
			this.AudioH = null;
		}

		[TestMethod]
		public void TestMethod1()
		{
		}
	}
}
