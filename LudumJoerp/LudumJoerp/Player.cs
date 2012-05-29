using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumJoerp
{
	class Player
	{
		private float
			m_fRotationY;

		public Model model { get; set; }
		public Vector2 velocity { get; set; }
		public Vector3 position { get; set; }
		public string sName { get; set; }
		public float fRadius { get; set; }

		public Player(
			string sNameIn,
			Vector3 positionIn
		)
		{
			sName = sNameIn;
			position = positionIn;
			m_fRotationY = 0;
			fRadius = 35;
		}

		public void vSetRotation(
			float fRotation
		)
		{
			if(fRotation < 0)
				m_fRotationY = 360;
			else if(fRotation > 360)
				m_fRotationY = 0;
			else
				m_fRotationY = fRotation;
		}

		public float fGetRotation(
		)
		{
			return m_fRotationY;
		}

		public List<Projectile> shoot(
		)
		{
			Vector2
				vel = new Vector2((float)Math.Cos(MathHelper.ToRadians(m_fRotationY)) * 30.0f, -(float)Math.Sin(MathHelper.ToRadians(m_fRotationY)) * 30.0f);
			List<Projectile>
				projectiles = new List<Projectile>();
			Vector3
				offset1 = new Vector3((float)Math.Cos(MathHelper.ToRadians(m_fRotationY + 90)) * 40, 0, (float)Math.Sin(MathHelper.ToRadians(m_fRotationY + 90)) * -40),
				offset2 = new Vector3((float)Math.Cos(MathHelper.ToRadians(m_fRotationY - 90)) * 40, 0, (float)Math.Sin(MathHelper.ToRadians(m_fRotationY - 90)) * -40);
			projectiles.Add(new Projectile(offset1 + position + new Vector3((float)Math.Cos(MathHelper.ToRadians(m_fRotationY)) * 25,
																																			0,
																																			(float)Math.Sin(MathHelper.ToRadians(m_fRotationY)) * -25), vel+velocity));
			projectiles.Add(new Projectile(offset2 + position + new Vector3((float)Math.Cos(MathHelper.ToRadians(m_fRotationY)) * 25,
																																			0,
																																			(float)Math.Sin(MathHelper.ToRadians(m_fRotationY)) * -25), vel+velocity));
			return projectiles;
		}
	}
}
