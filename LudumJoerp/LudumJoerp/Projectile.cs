using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumJoerp
{
	class Projectile
	{
		public Vector2 velocity { get; set; }
		public Vector3 position { get; set; }
		public float fRadius { get; set; }
		public Texture2D texture { get; set; }

		public Projectile(
			Vector3 positionIn,
			Vector2 velocityIn,
			/*Texture2D textureIn,*/
			float fRadiusIn = 5
		)
		{
			position = positionIn;
			velocity = velocityIn;
			fRadius = fRadiusIn;
			/*texture = textureIn;*/
		}
	}
}
