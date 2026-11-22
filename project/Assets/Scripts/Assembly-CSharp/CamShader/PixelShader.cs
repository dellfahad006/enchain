using UnityEngine;

namespace CamShader
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Custom/PixelShader")]
	public class PixelShader : MonoBehaviour
	{
		public static readonly int MAX_NUM_COLORS = 224;

		public int horizontalResolution = 160;

		public int verticalResolution = 200;

		public static int numColors = MAX_NUM_COLORS;

		private int oldNumColors;

		public Texture2D image;

		private Color[] colors = new Color[223];

		private Shader shader;

		private Material m_material;

		private Material material
		{
			get
			{
				if (m_material == null)
				{
					string text = "CamShader/PixelShader224";
					shader = Shader.Find(text);
					if (shader == null)
					{
						Debug.LogError("Shader '" + text + "' not found. Was it deleted?");
						base.enabled = false;
						return null;
					}
					m_material = new Material(shader);
					m_material.hideFlags = HideFlags.DontSave;
				}
				return m_material;
			}
		}

		private void Start()
		{
			colors = image.GetPixels();
			if (!SystemInfo.supportsImageEffects)
			{
				base.enabled = false;
			}
		}

		public void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			horizontalResolution = Mathf.Clamp(horizontalResolution, 1, 2048);
			verticalResolution = Mathf.Clamp(verticalResolution, 1, 2048);
			if ((bool)material)
			{
				material.shader = shader;
				material.SetColorArray("_Colors", colors);
				RenderTexture temporary = RenderTexture.GetTemporary(horizontalResolution, verticalResolution);
				temporary.filterMode = FilterMode.Point;
				Graphics.Blit(src, temporary, material);
				Graphics.Blit(temporary, dest);
				RenderTexture.ReleaseTemporary(temporary);
			}
			else
			{
				Graphics.Blit(src, dest);
			}
		}

		private void OnDisable()
		{
			if ((bool)m_material)
			{
				Object.DestroyImmediate(m_material);
			}
		}
	}
}
