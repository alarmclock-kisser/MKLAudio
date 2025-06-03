using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKLAudio
{
	public class VideoHandling
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		private AudioHandling AudioH;
		private ImageHandling ImageH;
		private OpenClService Service;

		public List<ImageObject> Frames = [];
		public AudioObject? Track = null;


		// ----- ----- ----- CONSTRUCTORS ----- ----- ----- \\
		public VideoHandling(string repopath, AudioHandling audioH, ImageHandling imageH, OpenClService service)
		{
			this.Repopath = repopath;
			this.AudioH = audioH;
			this.ImageH = imageH;
			this.Service = service;
		}




		// ----- ----- ----- METHODS ----- ----- ----- \\







		// ----- ----- ----- ACCESSIBLE METHODS ----- ----- ----- \\
		public string? RenderVideo(string outputPath = "Resources\\ExportedVideos", string videoName = "export", string extension = ".mp4", int framerate = 20, Size? resize = null, bool useAudio = true, bool useImages = true)
		{
			// Verify size
			resize ??= new Size(this.Frames.FirstOrDefault()?.Width ?? 1920, this.Frames.FirstOrDefault()?.Height ?? 1080);

			// Craft output path
			outputPath = Path.Combine(this.Repopath, outputPath);
			int fileIndex = Directory.GetFiles(outputPath, $"{videoName}*{extension}").Length + 1;
			string filePath = Path.Combine(outputPath, $"{videoName}_{fileIndex}{extension}");

			// Get data
			byte[] audioData = useAudio && this.Track != null ? this.Track.GetBytes() : [];
			List<Image> framesData = [];
			foreach (ImageObject frame in this.Frames)
			{
				if (frame.Img == null)
				{
					continue;
				}
				Image resizedFrame = this.ImageH.ResizeImage(frame.Img, resize.Value.Width, resize.Value.Height);
				framesData.Add(resizedFrame);
			}

			// With framerate and Frames and Track, render video in size -> If succeeded, return file path, else null


			return null;
		}


	}
}
