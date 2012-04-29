using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumJoerp
{
	class Canvas
	{
		public Matrix m_viewMat { get; set; }
		public Matrix m_projMat { get; set; }

		readonly private SpriteBatch
			m_spriteBatch;

		public Canvas(
			Matrix projMat,
			//Matrix viewMat,
			GraphicsDevice graphicsDevice
		)
		{
			m_projMat = projMat;
			//m_viewMat = viewMat;

			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(graphicsDevice);
		}

		public void vDrawModel(Model model, Matrix modelTransform/*, Matrix[] absoluteBoneTransforms*/)
		{
			Matrix[]
				transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);

			//Draw the model, a model can have multiple meshes, so loop
			foreach (ModelMesh mesh in model.Meshes)
			{
				//This is where the mesh orientation is set
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					effect.Projection = m_projMat;
					effect.View = m_viewMat;
					effect.World = /*absoluteBoneTransforms*/transforms[mesh.ParentBone.Index] * modelTransform;
				}
				//Draw the mesh, will use the effects set above.
				mesh.Draw();
			}
		}

		public void vDrawText(
			string sText,
			SpriteFont font,
			Vector2 screenPos,
			Color color
		)
		{
			m_spriteBatch.Begin();

			// Draw the string
			m_spriteBatch.DrawString(font, sText, screenPos, color,
					0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);

			m_spriteBatch.End();
		}
	}
}
