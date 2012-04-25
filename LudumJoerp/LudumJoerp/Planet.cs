using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumJoerp
{
	class Planet
	{
		public Vector3 position { get; set; }
		public Model model { get; set; }
		public float fRadius { get; set; }
		public Texture2D texture { get; set; }

		public Planet(
			Vector3 positionIn,
			Model modelIn,
			float fRadiusIn
		)
		{
			position = positionIn;
			model = modelIn;
			fRadius = fRadiusIn;
		}
	}
}
