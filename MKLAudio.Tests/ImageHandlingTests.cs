namespace MKLAudio.Tests
{
	[TestClass]
	public sealed class ImageHandlingTests
	{
		private string Repopath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));

		private ImageHandling? ImageH;

		[TestInitialize]
		public void Initialize()
		{
			this.ImageH = new ImageHandling(this.Repopath);
		}

		[TestCleanup]
		public void Cleanup()
		{
			this.ImageH?.Dispose();
		}



		[TestMethod]
		public void TESTM()
		{
			
		}


	}
}
