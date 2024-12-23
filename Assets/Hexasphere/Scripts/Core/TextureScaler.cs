using UnityEngine;

namespace HexasphereGrid {
			
	public class TextureScaler {

		public static void Scale (Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear) {

			RenderTexture currentActiveRT = RenderTexture.active;

			Rect texR = new Rect (0, 0, width, height);
			_gpu_scale (tex, width, height, mode);

			// Update new texture
			tex.Reinitialize (width, height);
			tex.ReadPixels (texR, 0, 0, true);
			tex.Apply (true);

			RenderTexture.active = currentActiveRT;
		}

		static void _gpu_scale (Texture2D src, int width, int height, FilterMode fmode) {
			//We need the source texture in VRAM because we render with it
			src.filterMode = fmode;
			src.Apply (true);	

			//Using RTT for best quality and performance. Thanks, Unity 5
			RenderTexture rtt = new RenderTexture (width, height, 16);

			//Set the RTT in order to render to it
			Graphics.SetRenderTarget (rtt);

			//Setup 2D matrix in range 0..1, so nobody needs to care about sized
			GL.LoadPixelMatrix (0, 1, 1, 0);

			//Then clear & draw the texture to fill the entire RTT.
			GL.Clear(true, true, Color.black);
			Graphics.DrawTexture (new Rect (0, 0, 1, 1), src);

		}
	}

}
