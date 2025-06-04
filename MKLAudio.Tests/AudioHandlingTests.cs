namespace MKLAudio.Tests
{
	[TestClass]
	public sealed class AudioHandlingTests
	{
		private string Repopath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));

		private AudioHandling? AudioH;



		[TestInitialize]
		public void Initialize()
		{
			this.AudioH = new AudioHandling(this.Repopath);
		}

		[TestCleanup]
		public void Cleanup()
		{
			this.AudioH?.Dispose();

		}




		[TestMethod]
		public void TESTM()
		{
			
		}


	}
}
