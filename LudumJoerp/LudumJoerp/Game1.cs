using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LudumJoerp
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		readonly GraphicsDeviceManager
			m_graphics;
		SpriteBatch
			m_spriteBatch;

		Player
			m_player;

		private List<Planet>
			m_planets;

		private List<Projectile>
			m_projectiles;

		private Texture2D
			m_texture,
			m_textureProjectile;

		private const int
			m_iWindowWidth = 1024,
			m_iWindowHeight = 1024;

		private float
			m_fFov,
			m_fPlanetRot,
			m_fModelScaleFactor = 2.54f;	// FBX scales the model for some stupid reason, NOT MY FAULT!?!!

		private double
			m_dGameTimeLastShot;

		// Set the position of the model in world space, and set the rotation.
		// Set the position of the camera in world space, for our view matrix.
		readonly private Vector3
			m_cameraPosition = new Vector3(0.0f, 2000.0f, 0.0f);

		// Sprite font
		SpriteFont m_font1;
		//Vector2 FontPos;
		private BasicEffect
			m_bulletEffect;

		private VertexPositionNormalTexture[]
			m_bulletPlaneVerts;

		private VertexPositionColor[]
			m_boundingBoxVerts;

		private int[]
			m_bulletPlaneIndices;

		private Matrix
				m_projMat,
				m_viewMat;

		public Game1()
		{
			m_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			// GraphicsDevice settings
			//graphics.ToggleFullScreen();
			m_graphics.PreferredBackBufferWidth = m_iWindowWidth;
			m_graphics.PreferredBackBufferHeight = m_iWindowHeight;
			Window.Title = "LudumJoerp";
			m_graphics.ApplyChanges();

			m_fFov = 45;
			m_fPlanetRot = 0;
			m_dGameTimeLastShot = 0;
			m_projectiles = new List<Projectile>();
			m_player = new Player("Player 1", new Vector3(-500, 0, 0));

			m_projMat = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(m_fFov),
			                                                (m_iWindowWidth/(float) m_iWindowHeight), 0.1f, 10000.0f);
			m_viewMat = Matrix.CreateLookAt(m_cameraPosition, Vector3.Zero, Vector3.Forward);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			m_spriteBatch = new SpriteBatch(GraphicsDevice);

			//m_model = Content.Load<Model>("ball");											// Load our ball model
			m_texture = Content.Load<Texture2D>("textures\\ballwhite");	// Load our ball texture
			m_textureProjectile = Content.Load<Texture2D>("textures\\projectile");
			m_player.model = Content.Load<Model>("teapot");
			m_planets = new List<Planet>();
			m_planets.Add(new Planet(new Vector3(0, 0, 0), Content.Load<Model>("planet"), 100 ));

			m_font1 = Content.Load<SpriteFont>("SpriteFont1");
			//FontPos = new Vector2(300, 50);

			// Bullet effect
			m_bulletEffect = new BasicEffect(GraphicsDevice);
			m_bulletEffect.EnableDefaultLighting();
			m_bulletEffect.PreferPerPixelLighting = true;
			m_bulletEffect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);

			m_bulletEffect.TextureEnabled = true;
			m_bulletEffect.Texture = m_textureProjectile;

			// Bullet plane verts
			m_bulletPlaneVerts = new VertexPositionNormalTexture[4];
			m_bulletPlaneIndices = new [] {0, 1, 2, 0, 2, 3};

			m_bulletPlaneVerts[0].Position = new Vector3(0, 150, 0); m_bulletPlaneVerts[0].TextureCoordinate = new Vector2(0, 0); m_bulletPlaneVerts[0].Normal = new Vector3(0, 1, 0);
			m_bulletPlaneVerts[1].Position = new Vector3(10, 150, 0); m_bulletPlaneVerts[1].TextureCoordinate = new Vector2(1, 0); m_bulletPlaneVerts[1].Normal = new Vector3(0, 1, 0);
			m_bulletPlaneVerts[2].Position = new Vector3(10, 150, 10); m_bulletPlaneVerts[2].TextureCoordinate = new Vector2(1, 1); m_bulletPlaneVerts[2].Normal = new Vector3(0, 1, 0);
			m_bulletPlaneVerts[3].Position = new Vector3(0, 150, 10); m_bulletPlaneVerts[3].TextureCoordinate = new Vector2(0, 1); m_bulletPlaneVerts[3].Normal = new Vector3(0, 1, 0);
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// Update planet rotation
			m_fPlanetRot += 0.1f;

			// Update player rotation
			if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Right))
				m_player.vSetRotation(m_player.fGetRotation() - (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(10.0f));
			else if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Left))
				m_player.vSetRotation(m_player.fGetRotation() + (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(10.0f));

			// Update player velocity
			if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Up))
				m_player.velocity += new Vector2((float)Math.Cos(MathHelper.ToRadians(m_player.fGetRotation())) * 0.1f, -(float)Math.Sin(MathHelper.ToRadians(m_player.fGetRotation()))  * 0.1f);

			// Update player position based on velocity
			m_player.position += new Vector3(m_player.velocity.X, 0, m_player.velocity.Y);

			// Check player collision with the edges of the screen
			/*
			Vector2
				screenCoordsPos = worldToScreenCoords(m_player.position.X, m_player.position.Z),
				screenCoordsRad = worldToScreenCoords(m_player.fRadius, m_player.fRadius);
			float
				fRad = screenCoordsRad.X - (m_iWindowWidth/2);
			if ((screenCoordsPos.X + fRad) > m_iWindowWidth || (screenCoordsPos.X - fRad) < 0)
				m_player.velocity *= new Vector2(-1, 1);
			if ((screenCoordsPos.Y + fRad) > m_iWindowWidth || (screenCoordsPos.Y - fRad) < 0)
				m_player.velocity *= new Vector2(1, -1);
			*/

			// Update projectile positions and check for collisions with planets
			for (int i = 0; i < m_projectiles.Count; i++)
			{
				if (boCheckCircleCollision(m_projectiles[i].position, m_planets[0].position, m_projectiles[i].fRadius, m_planets[0].fRadius))
					m_projectiles.RemoveAt(i);
				else
					m_projectiles[i].position += new Vector3(m_projectiles[i].velocity.X, 0, m_projectiles[i].velocity.Y);
			}

			// Handle shooting
			if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Space) && (gameTime.TotalGameTime.TotalMilliseconds - m_dGameTimeLastShot) > 500)
			{
				m_dGameTimeLastShot = gameTime.TotalGameTime.TotalMilliseconds;
				m_projectiles.Add(m_player.shoot());
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			Matrix[]
				transforms = new Matrix[m_player.model.Bones.Count];
			m_player.model.CopyAbsoluteBoneTransformsTo(transforms);

			// Draw the player. A model can have multiple meshes, so loop.
			foreach (ModelMesh mesh in m_player.model.Meshes)
			{
				// This is where the mesh orientation is set, as well 
				// as our camera and projection.
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					effect.World = transforms[mesh.ParentBone.Index]
						* Matrix.CreateScale(1.0f/m_fModelScaleFactor)
						* Matrix.CreateRotationY(MathHelper.ToRadians(m_player.fGetRotation()))
						* Matrix.CreateTranslation(m_player.position);
					effect.View = m_viewMat;
					effect.Projection = m_projMat;
					effect.DiffuseColor = new Vector3(255, 0, 0);
				}
				// Draw the mesh, using the effects set above.
				mesh.Draw();
			}

			// Draw planets
			foreach (Planet planet in m_planets)
			{
				foreach (ModelMesh mesh in planet.model.Meshes)
				{
					// This is where the mesh orientation is set, as well 
					// as our camera and projection.
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.EnableDefaultLighting();
						effect.World = transforms[mesh.ParentBone.Index]
							* Matrix.CreateScale(1.0f / m_fModelScaleFactor)
							* Matrix.CreateRotationY(MathHelper.ToRadians(m_fPlanetRot))	// rotate around its own axis then give a default rotation of 25degrees
							* Matrix.CreateRotationZ(MathHelper.ToRadians(-25))
							* Matrix.CreateTranslation(planet.position);
						effect.View = m_viewMat;
						effect.Projection = m_projMat;
						effect.AmbientLightColor = new Vector3(255, 255, 255);
					}

					// Draw the mesh, using the effects set above.
					mesh.Draw();
				}
			}

			/*
			// Draw boundingBox
			{
				m_bulletEffect.TextureEnabled = false;
				m_bulletEffect.LightingEnabled = false;
				m_bulletEffect.VertexColorEnabled = true;
				m_bulletEffect.World = Matrix.CreateTranslation(m_planets[0].position);
				m_bulletEffect.View = viewMat;
				m_bulletEffect.Projection = projMat;
				m_bulletEffect.DiffuseColor = new Vector3(255, 255, 255);

				foreach (EffectPass pass in m_bulletEffect.CurrentTechnique.Passes)
				{
					pass.Apply();

					GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, m_boundingBoxVerts, 0, 4, m_bulletPlaneIndices, 0, 2);
				}
			}
			*/

			// Draw projectiles
			for (int i = 0; i < m_projectiles.Count; i++)
			{
				m_bulletEffect.World = //transforms[mesh.ParentBone.Index] *
					//Matrix.CreateRotationY(MathHelper.ToRadians(m_player.fGetRotation())) *
					Matrix.CreateTranslation(m_projectiles[i].position + new Vector3(-m_projectiles[i].fRadius, 0, -m_projectiles[i].fRadius));
				m_bulletEffect.View = m_viewMat;
				m_bulletEffect.Projection = m_projMat;
				m_bulletEffect.DiffuseColor = new Vector3(0, 255, 255);

				foreach (EffectPass pass in m_bulletEffect.CurrentTechnique.Passes)
				{
					pass.Apply();

					GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList,
					                                                                      m_bulletPlaneVerts, 0, 4,
					                                                                      m_bulletPlaneIndices, 0, 2);
				}
			}

			// Draw text
			
			m_spriteBatch.Begin();

			// Draw Hello World
			string
				outputRotation = "Rotation: " + m_player.fGetRotation(),
				outputVelX = "Velocity X: " + m_player.velocity.X,
				outputVelZ = "Velocity Z: " + m_player.velocity.Y,
				outputPosX = "Position X: " + m_player.position.X,
				outputPosZ = "Position Z: " + m_player.position.Z;

			// Find the center of the string
			Vector2
				FontOrigin = new Vector2(0, 0);

			// Draw the strings
			m_spriteBatch.DrawString(m_font1, outputRotation, new Vector2(0, 0), Color.LightGreen,
					0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
			m_spriteBatch.DrawString(m_font1, outputVelX, new Vector2(0, 25), Color.LightGreen,
					0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
			m_spriteBatch.DrawString(m_font1, outputVelZ, new Vector2(0, 50), Color.LightGreen,
					0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
			m_spriteBatch.DrawString(m_font1, outputPosX, new Vector2(0, 75), Color.LightGreen,
					0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);
			m_spriteBatch.DrawString(m_font1, outputPosZ, new Vector2(0, 100), Color.LightGreen,
					0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);

			m_spriteBatch.End();

			base.Draw(gameTime);
		}

		private Vector2 worldToScreenCoords(
			float fX,
			float fY
		)
		{
			float
				fConstant = (float)Math.Sqrt(Math.Pow((m_cameraPosition.Y/Math.Cos( MathHelper.ToRadians(m_fFov/2) )), 2) + Math.Pow((double) m_cameraPosition.Y, 2));
			Vector2
				ret = new Vector2(fX / (m_iWindowWidth / fConstant) + (m_iWindowWidth / 2), fY / (m_iWindowHeight / fConstant) + (m_iWindowHeight / 2));

			return ret;
		}

		private bool boCheckCircleCollision(
			Vector3 position1,
			Vector3 position2,
			float fRadius1,
			float fRadius2
		)
		{
			Vector3
				distance = position2 - position1;
			if (Math.Abs(distance.Length()) <= (fRadius1 + fRadius2))
				return true;
			return false;
		}

		private void vDrawModel(Model model, Matrix modelTransform/*, Matrix[] absoluteBoneTransforms*/)
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
	}
}
