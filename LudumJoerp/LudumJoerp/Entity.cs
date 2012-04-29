using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumJoerp
{
	class Entity
	{
		public Vector3 position { get; set; }
		public Model model { get; set; }
		public float fRadius { get; set; }
		public Vector2 velocity { get; set; }
		public int health { get; set; }

		private Vector3
			m_rotation;

		public Vector3 rotation
		{
			get { return m_rotation; }
			set
			{
				// X
				if (value.X > 360)
					m_rotation.X = 0;
				else if (value.X < 0)
					m_rotation.X = 360;
				else
					m_rotation.X = value.X;

				// Y
				if (value.Y > 360)
					m_rotation.Y = 0;
				else if (value.Y < 0)
					m_rotation.Y = 360;
				else
					m_rotation.Y = value.Y;

				// Z
				if (value.Z > 360)
					m_rotation.Z = 0;
				else if (value.Z < 0)
					m_rotation.Z = 360;
				else
					m_rotation.Z = value.Z;
			}
		}

		public Entity(
			Vector3 positionIn,
			Model modelIn,
			float fRadiusIn
		)
		{
			position = positionIn;
			model = modelIn;
			fRadius = fRadiusIn;
			rotation = new Vector3(0, 0, 0);
			health = 100;
		}
	}
}
