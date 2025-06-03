namespace MKLAudio.Tests
{
	[TestClass]
	public sealed class OpenClServiceTests
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
		public void Initialize_Intel_ShouldHaveContextEtc()
		{
			// Arrange

			// Act

			// Assert
			Assert.IsNotNull(this.Service?.CTX, "Context should not be null after selecting Intel device.");
			Assert.IsNotNull(this.Service?.DEV, "Device should not be null after selecting Intel device.");
			Assert.IsNotNull(this.Service?.PLAT, "Platform should not be null after selecting Intel device.");
		}

		[TestMethod]
		public void MoveAudio_NewObject_ShouldReturnIntPtr()
		{
			// Arrange
			var obj = AudioH?.CreateEmptyTrack(1000, 44100, 2, 24, true);
			Assert.IsNotNull(obj, "Audio object should not be null after creation.");

			// Act
			var result = this.Service?.MoveAudio(obj, 256, 0.5f, 1.0f, false);

			// Assert
			Assert.IsNotNull(result, "Result should not be null after moving audio.");
			Assert.IsInstanceOfType(result, typeof(IntPtr), "Result should be of type IntPtr.");
			Assert.AreNotEqual(IntPtr.Zero, result, "Result IntPtr should not be zero after moving audio.");
		}

		[TestMethod]
		public void MoveAudio_MoveAudio_NewObject_ShouldReturnIntPtrZero()
		{
			// Arrange
			var obj = AudioH?.CreateEmptyTrack(1000, 44100, 2, 24, true);
			Assert.IsNotNull(obj, "Audio object should not be null after creation.");

			// Act
			var result = this.Service?.MoveAudio(obj, 256, 0.5f, 1.0f, false);
			Assert.IsNotNull(result, "Result should not be null after moving audio.");
			Assert.IsInstanceOfType(result, typeof(IntPtr), "Result should be of type IntPtr.");
			Assert.AreNotEqual(IntPtr.Zero, result, "Result IntPtr should not be zero after moving audio.");
			result = this.Service?.MoveAudio(obj);

			// Assert
			Assert.IsNotNull(result, "Result should not be null after moving audio.");
			Assert.IsInstanceOfType(result, typeof(IntPtr), "Result should be of type IntPtr.");
			Assert.AreEqual(IntPtr.Zero, result, "Result IntPtr should not be zero after moving audio.");
		}

		[TestMethod]
		public void PerformFFT_NewObject_ShouldReturnIntPtr()
		{
			// Arrange
			var obj = AudioH?.CreateEmptyTrack(1000, 44100, 2, 24, true);
			Assert.IsTrue(obj != null, "Audio object should not be null after creation.");

			// Act
			var result = this.Service?.PerformFFT(obj, 256, 0.5f, false);

			// Assert
			Assert.IsNotNull(result, "Result should not be null after performing FFT.");
			Assert.IsInstanceOfType(result, typeof(IntPtr), "Result should be of type IntPtr.");
			Assert.AreNotEqual(IntPtr.Zero, result, "Result IntPtr should not be zero after performing FFT.");
		}

		[TestMethod]
		public void PerformFFT_PerformIFFT_MoveBack_NewObject_ShouldReturnIntPtrZero()
		{
			// Arrange
			var obj = AudioH?.CreateEmptyTrack(1000, 44100, 2, 24, true);
			Assert.IsTrue(obj != null, "Audio object should not be null after creation.");

			// Act
			var result = this.Service?.PerformFFT(obj, 256, 0.5f, false);
			Assert.IsNotNull(result, "Result should not be null after performing FFT.");
			result = this.Service?.PerformFFT(obj, 256, 0.5f, false);
			Assert.IsNotNull(result, "Result should not be null after performing IFFT.");
			result = this.Service?.MoveAudio(obj);

			// Assert
			Assert.IsNotNull(result, "Result should not be null after performing FFT-IFFT + move.");
			Assert.IsInstanceOfType(result, typeof(IntPtr), "Result should be of type IntPtr.");
			Assert.AreEqual(IntPtr.Zero, result, "Result IntPtr should be zero after performing FFT-IFFT + move.");
		}
	}
}
