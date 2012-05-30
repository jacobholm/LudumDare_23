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
		readonly private GraphicsDeviceManager
			m_graphics;
		private Canvas
			m_canvas;
		private Player
			m_player;
		private Entity
			m_homePlanet;
		private List<Entity>
			m_asteroids;
		private List<Model>
			m_asteroidModels;
		private List<Projectile>
			m_projectiles;
		private List<Vector3>
			m_explosions;
		private Texture2D
			m_textureProjectile,
			m_textureCrosshair,
			m_textureBackground,
			m_textureExplosion,
			m_textureArrow;
		private SoundEffect
			m_shotImpact,
			m_asteroidImpact,
			m_shot;
		private const int
			m_iWindowWidth = 1280,
			m_iWindowHeight = 960;
		private float
			m_fModelScaleFactor = 2.54f;	// FBX scales the model for some stupid reason, NOT MY FAULT!?!!
		private double
			m_dGameTimeLastShot,
			m_dGameTimeLastAsteroid,
			m_dAsteroidFrequency,
			m_dTimePlayed,
			m_dPassedTime;
		private bool
			m_boGameOver = false,
			m_boUseRuneNav = false,
			m_boRuneNavKeyIsDown = false;
		private int
			m_iNrAsteroidsBlasted;

		// Set the position of the model in world space, and set the rotation.
		// Set the position of the camera in world space, for our view matrix.
		private Vector3
			m_cameraPosition;
		private SpriteFont
			m_font1;	// Sprite font
		private BasicEffect
			m_projectEffect,
			m_backgroundEffect,
			m_explosionEffect;
		private VertexPositionNormalTexture[]
			m_planeVerts,
			m_backgroundPlaneVerts,
			m_explosionVerts;
		private int[]
			m_planeIndices;

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
			// GraphicsDevice settings
			//graphics.ToggleFullScreen();
			m_graphics.PreferredBackBufferWidth = m_iWindowWidth;
			m_graphics.PreferredBackBufferHeight = m_iWindowHeight;
			Window.Title = "LudumJoerp";
			m_graphics.ApplyChanges();

			m_dGameTimeLastShot = 0;
			m_dGameTimeLastAsteroid = 0;
			m_dAsteroidFrequency = 3000;
			m_projectiles = new List<Projectile>();
			m_explosions = new List<Vector3>();
			m_player = new Player("Player 1", new Vector3(-1000, 0, 0));
			m_dPassedTime = 0;

			m_iNrAsteroidsBlasted = 0;

			Matrix
				projMat = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (m_iWindowWidth/(float) m_iWindowHeight), 0.1f, 10000.0f);
			m_canvas = new Canvas(projMat, m_graphics.GraphicsDevice);

			m_asteroids = new List<Entity>();
			m_asteroidModels = new List<Model>();

			m_asteroidModels.Add(Content.Load<Model>("asteroid1"));
			m_asteroidModels.Add(Content.Load<Model>("asteroid2"));
			m_asteroidModels.Add(Content.Load<Model>("asteroid3"));
			m_asteroidModels.Add(Content.Load<Model>("asteroid4"));

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			m_graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			m_textureArrow = Content.Load<Texture2D>("textures\\arrow");
			m_textureProjectile = Content.Load<Texture2D>("textures\\projectile");
			m_textureCrosshair = Content.Load<Texture2D>("textures\\crosshair");
			m_textureBackground = Content.Load<Texture2D>("textures\\background");
			m_textureExplosion = Content.Load<Texture2D>("textures\\explosion");
			m_player.model = Content.Load<Model>("spaceship");
			m_homePlanet = new Entity(new Vector3(0, 0, 0), Content.Load<Model>("planet"), 600);
			m_homePlanet.rotation = new Vector3(0, 0, -25);

			// Sound effects
			m_shotImpact = Content.Load<SoundEffect>("shotImpact");
			m_asteroidImpact = Content.Load<SoundEffect>("asteroidImpact");
			m_shot = Content.Load<SoundEffect>("shot");

			// Font
			m_font1 = Content.Load<SpriteFont>("SpriteFont1");

			// Projectile effect
			m_projectEffect = new BasicEffect(GraphicsDevice);
			m_projectEffect.EnableDefaultLighting();
			m_projectEffect.PreferPerPixelLighting = true;
			m_projectEffect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);
			m_projectEffect.TextureEnabled = true;
			m_projectEffect.Texture = m_textureProjectile;

			// Explosion effect
			m_explosionEffect = new BasicEffect(GraphicsDevice);
			m_explosionEffect.EnableDefaultLighting();
			m_explosionEffect.PreferPerPixelLighting = true;
			m_explosionEffect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);
			m_explosionEffect.TextureEnabled = true;
			m_explosionEffect.Texture = m_textureExplosion;

			// Background effect
			m_backgroundEffect = new BasicEffect(GraphicsDevice);
			m_backgroundEffect.EnableDefaultLighting();
			m_backgroundEffect.PreferPerPixelLighting = true;
			m_backgroundEffect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);
			m_backgroundEffect.TextureEnabled = true;
			m_backgroundEffect.Texture = m_textureBackground;

			// Projectile plane verts
			m_planeVerts = new VertexPositionNormalTexture[4];
			m_planeIndices = new [] {0, 1, 2, 0, 2, 3};

			m_planeVerts[0].Position = new Vector3(0, 150, 0); m_planeVerts[0].TextureCoordinate = new Vector2(0, 0); m_planeVerts[0].Normal = new Vector3(0, 1, 0);
			m_planeVerts[1].Position = new Vector3(10, 150, 0); m_planeVerts[1].TextureCoordinate = new Vector2(1, 0); m_planeVerts[1].Normal = new Vector3(0, 1, 0);
			m_planeVerts[2].Position = new Vector3(10, 150, 10); m_planeVerts[2].TextureCoordinate = new Vector2(1, 1); m_planeVerts[2].Normal = new Vector3(0, 1, 0);
			m_planeVerts[3].Position = new Vector3(0, 150, 10); m_planeVerts[3].TextureCoordinate = new Vector2(0, 1); m_planeVerts[3].Normal = new Vector3(0, 1, 0);

			// Projectile plane verts
			m_explosionVerts = new VertexPositionNormalTexture[4];

			m_explosionVerts[0].Position = new Vector3(-32, 150, -32); m_explosionVerts[0].TextureCoordinate = new Vector2(0, 0); m_explosionVerts[0].Normal = new Vector3(0, 1, 0);
			m_explosionVerts[1].Position = new Vector3(32, 150, -32); m_explosionVerts[1].TextureCoordinate = new Vector2(1, 0); m_explosionVerts[1].Normal = new Vector3(0, 1, 0);
			m_explosionVerts[2].Position = new Vector3(32, 150, 32); m_explosionVerts[2].TextureCoordinate = new Vector2(1, 1); m_explosionVerts[2].Normal = new Vector3(0, 1, 0);
			m_explosionVerts[3].Position = new Vector3(-32, 150, 32); m_explosionVerts[3].TextureCoordinate = new Vector2(0, 1); m_explosionVerts[3].Normal = new Vector3(0, 1, 0);

			// Background plane verts
			m_backgroundPlaneVerts = new VertexPositionNormalTexture[4];

			m_backgroundPlaneVerts[0].Position = new Vector3(-4096, 150, -4096); m_backgroundPlaneVerts[0].TextureCoordinate = new Vector2(0, 0); m_backgroundPlaneVerts[0].Normal = new Vector3(0, 1, 0);
			m_backgroundPlaneVerts[1].Position = new Vector3(4096, 150, -4096); m_backgroundPlaneVerts[1].TextureCoordinate = new Vector2(1, 0); m_backgroundPlaneVerts[1].Normal = new Vector3(0, 1, 0);
			m_backgroundPlaneVerts[2].Position = new Vector3(4096, 150, 4096); m_backgroundPlaneVerts[2].TextureCoordinate = new Vector2(1, 1); m_backgroundPlaneVerts[2].Normal = new Vector3(0, 1, 0);
			m_backgroundPlaneVerts[3].Position = new Vector3(-4096, 150, 4096); m_backgroundPlaneVerts[3].TextureCoordinate = new Vector2(0, 1); m_backgroundPlaneVerts[3].Normal = new Vector3(0, 1, 0);

			// Init new game		 
			vInitNewGame();
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

			if (!m_boGameOver)
			{
				// Update time
				m_dTimePlayed = gameTime.TotalGameTime.TotalMilliseconds - m_dPassedTime;

				// Check collisions
				vCheckCollisions(gameTime);

				// Change between Rune nav mode and normal
				if (!m_boRuneNavKeyIsDown && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.LeftControl) && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.D1))
				{
					m_boUseRuneNav = !m_boUseRuneNav;
					m_boRuneNavKeyIsDown = true;
				}
				else if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.LeftControl) && Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.D1))
					m_boRuneNavKeyIsDown = false;

				// Clean up explosions
				for ( int i = m_explosions.Count; i > 0; i-- )
				{
					if( (gameTime.TotalGameTime.TotalMilliseconds - m_explosions[i-1].Z/*gameTime*/) > 50 )
						m_explosions.RemoveAt(i-1);
				}

				// Spawn asteroids
				vSpawnAsteroids(gameTime);

				// Update asteroids
				foreach (Entity asteroid in m_asteroids)
				{
					asteroid.position += new Vector3(asteroid.velocity.X, 0, asteroid.velocity.Y); // Update position
					asteroid.rotation += new Vector3(asteroid.velocity.X / 2, 0.5f, asteroid.velocity.Y / 2);
					// Update rotation based an velocity
				}

				// Update planet rotation
				m_homePlanet.rotation += new Vector3(0, 0.1f, 0);

				// Use Rune navigation or modern approach?
				if (m_boUseRuneNav)
				{
					// Update player rotation
					if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Right))
						m_player.vSetRotation(m_player.fGetRotation() -
																	(float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(15.0f));
					else if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Left))
						m_player.vSetRotation(m_player.fGetRotation() +
																	(float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(15.0f));

					// Update player velocity
					if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Up))
						m_player.velocity += new Vector2((float)Math.Cos(MathHelper.ToRadians(m_player.fGetRotation())) * 0.2f,
																						 -(float)Math.Sin(MathHelper.ToRadians(m_player.fGetRotation())) * 0.2f);
				}
				else
				{
					// Update player rotation
					Vector2
						zeroDegreeVector = new Vector2(1, 0),
						shipMouseVector = new Vector2(Mouse.GetState().X - m_iWindowWidth / 2, Mouse.GetState().Y - m_iWindowHeight / 2) - new Vector2(0, 0);
					shipMouseVector.Normalize();
					float
						fAngle = (zeroDegreeVector.X * shipMouseVector.X) + (zeroDegreeVector.Y * shipMouseVector.Y);
					if (shipMouseVector.Y < 0)
						m_player.vSetRotation(MathHelper.ToDegrees((float)Math.Acos(fAngle)));
					else
						m_player.vSetRotation(360 - MathHelper.ToDegrees((float)Math.Acos(fAngle)));

					// Update player velocity
					if (m_player.velocity.Y > -15 && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.W))
						m_player.velocity += new Vector2(0, -0.5f);
					else if (m_player.velocity.Y < 20 && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S))
						m_player.velocity += new Vector2(0, 0.5f);
					if (m_player.velocity.X > -15 && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.A))
						m_player.velocity += new Vector2(-0.5f, 0);
					else if (m_player.velocity.X < 15 && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.D))
						m_player.velocity += new Vector2(0.6f, 0);
				}

				// Update player position based on velocity
				m_player.position += new Vector3(m_player.velocity.X, 0, m_player.velocity.Y);

				// Update projectile positions and check for collisions with planets and asteroids
				for (int i = 0; i < m_projectiles.Count; i++ )
					m_projectiles[i].position += new Vector3(m_projectiles[i].velocity.X, 0, m_projectiles[i].velocity.Y);

				// Handle shooting
				if (((m_boUseRuneNav && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Space)) || Mouse.GetState().LeftButton == ButtonState.Pressed) &&
						(gameTime.TotalGameTime.TotalMilliseconds - m_dGameTimeLastShot) > 100)
				{
					m_dGameTimeLastShot = gameTime.TotalGameTime.TotalMilliseconds;
					m_projectiles.AddRange(m_player.shoot());

					// Play sound effect
					m_shot.Play(0.07f, 0, 0);
				}

				// Update camera and view projection
				m_cameraPosition = new Vector3(m_player.position.X, 4000.0f, m_player.position.Z);
				m_canvas.m_viewMat = Matrix.CreateLookAt(m_cameraPosition, m_player.position, Vector3.Forward);

				// Game over?
				if(m_homePlanet.health <= 0)
					m_boGameOver = true;
			}
			else
			{
				if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.R))
					vInitNewGame();
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

			// Draw background
			m_backgroundEffect.View = m_canvas.m_viewMat;
			m_backgroundEffect.Projection = m_canvas.m_projMat;
			m_backgroundEffect.LightingEnabled = false;
			//m_backgroundEffect.DiffuseColor = new Vector3(100, 100, 100);

			foreach (EffectPass pass in m_backgroundEffect.CurrentTechnique.Passes)
			{
				pass.Apply();

				GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList,
																																							m_backgroundPlaneVerts, 0, 4,
																																							m_planeIndices, 0, 2);
			}

			// Draw the player
			Matrix
				transformationMatrix = Matrix.CreateScale(1.0f / m_fModelScaleFactor)
															 * Matrix.CreateRotationY(MathHelper.ToRadians(m_player.fGetRotation()))
															 * Matrix.CreateTranslation(m_player.position);
			m_canvas.vDrawModel(m_player.model, transformationMatrix);

			// Draw planet
			transformationMatrix = Matrix.CreateScale(1.0f/m_fModelScaleFactor)
															* Matrix.CreateRotationY(MathHelper.ToRadians(m_homePlanet.rotation.Y))
				                      // rotate around its own axis then give a default rotation of 25degrees
				                      *Matrix.CreateRotationZ(MathHelper.ToRadians(m_homePlanet.rotation.Z))
															* Matrix.CreateTranslation(m_homePlanet.position);

			m_canvas.vDrawModel(m_homePlanet.model, transformationMatrix);

			// Draw asteroids
			foreach (Entity asteroid in m_asteroids)
			{
				transformationMatrix = Matrix.CreateScale(1.0f/m_fModelScaleFactor*(asteroid.fRadius/100))
															* Matrix.CreateRotationX(MathHelper.ToRadians(asteroid.rotation.X))
															* Matrix.CreateRotationY(MathHelper.ToRadians(asteroid.rotation.Y))
															* Matrix.CreateRotationZ(MathHelper.ToRadians(asteroid.rotation.Z))
				                      * Matrix.CreateTranslation(asteroid.position);

				m_canvas.vDrawModel(asteroid.model, transformationMatrix);
			}

			// Draw projectiles
			for (int i = 0; i < m_projectiles.Count; i++)
			{
				m_projectEffect.World = Matrix.CreateTranslation(m_projectiles[i].position + new Vector3(-m_projectiles[i].fRadius, 0, -m_projectiles[i].fRadius));
				m_projectEffect.View = m_canvas.m_viewMat;
				m_projectEffect.Projection = m_canvas.m_projMat;
				m_projectEffect.DiffuseColor = new Vector3(1, 0, 0);

				foreach (EffectPass pass in m_projectEffect.CurrentTechnique.Passes)
				{
					pass.Apply();

					GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList,
					                                                                      m_planeVerts, 0, 4,
					                                                                      m_planeIndices, 0, 2);
				}
			}

			// Draw explosions
			foreach (Vector3 explosion in m_explosions)
			{
				m_explosionEffect.World = Matrix.CreateTranslation(new Vector3(explosion.X, 0, explosion.Y));
				m_explosionEffect.View = m_canvas.m_viewMat;
				m_explosionEffect.Projection = m_canvas.m_projMat;
				m_projectEffect.DiffuseColor = new Vector3(1, 1, 1);

				foreach (EffectPass pass in m_explosionEffect.CurrentTechnique.Passes)
				{
					pass.Apply();

					GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList,
																																								m_explosionVerts, 0, 4,
																																								m_planeIndices, 0, 2);
				}
			}

			// Draw arrows indicating incoming asteroids
			foreach (var asteroid in m_asteroids)
			{
				//m_canvas.vDrawSprite(m_textureArrow, worldToScreenCoords(m_player.position.X, m_player.position.Z));
			}

#if false
			// Draw debug text
			// Check for collisions between player and planet
			if (boCheckCircleCollision(m_player.position, m_homePlanet.position, m_player.fRadius, m_homePlanet.fRadius))
				m_canvas.vDrawText("CRASH!!!", m_font1, new Vector2(0, 125), Color.White);

			m_canvas.vDrawText("Rotation: " + m_player.fGetRotation(), m_font1, new Vector2(0, 0), Color.White);
			m_canvas.vDrawText("Velocity X: " + m_player.velocity.X, m_font1, new Vector2(0, 25), Color.White);
			m_canvas.vDrawText("Velocity Z: " + m_player.velocity.Y, m_font1, new Vector2(0, 50), Color.White);
			m_canvas.vDrawText("Position X: " + m_player.position.X, m_font1, new Vector2(0, 75), Color.White);
			m_canvas.vDrawText("Position Z: " + m_player.position.Z, m_font1, new Vector2(0, 100), Color.White);
			m_canvas.vDrawText("Mouse X/Y Screen: " + Mouse.GetState().X + "   " + Mouse.GetState().Y, m_font1, new Vector2(0, 125), Color.White);
			m_canvas.vDrawText("Asteroid Frequency (every x ms): " + m_dAsteroidFrequency, m_font1, new Vector2(0, 175), Color.White);
			Vector2
				shipMouseVector = new Vector2(Mouse.GetState().X - m_iWindowWidth/2, Mouse.GetState().Y - m_iWindowHeight/2) -
				                  new Vector2(0, 0);
			shipMouseVector.Normalize();
			m_canvas.vDrawText("ShipMouse Vector X/Y: " + shipMouseVector.X + "   " + shipMouseVector.Y, m_font1, new Vector2(0, 200), Color.White);
#else
			m_canvas.vDrawText("Asteroids blasted: " + m_iNrAsteroidsBlasted, m_font1, new Vector2(0, 0), Color.White);
			m_canvas.vDrawText("Homeplanet health: " + m_homePlanet.health, m_font1, new Vector2(0, 25), Color.White);
			m_canvas.vDrawText("Time played: " + (int)(m_dTimePlayed/1000), m_font1, new Vector2(0, 50), Color.White);
			//if(m_asteroids.Count > 0)
				//m_canvas.vDrawText("Asteroid health: " + m_asteroids.Last().health, m_font1, new Vector2(0, 75), Color.White);
#endif
			// Draw crosshair
			m_canvas.vDrawSprite(m_textureCrosshair, new Vector2(Mouse.GetState().X - 16, Mouse.GetState().Y - 16));

			if(m_boGameOver)
				m_canvas.vDrawText("GAME OVER!!! Press 'R' to restart. ", m_font1, new Vector2(m_iWindowWidth/2-100, m_iWindowHeight/2), Color.Red);

			base.Draw(gameTime);
		}

		private Vector2 worldToScreenCoords(
			float fX,
			float fY
		)
		{
			float
				fConstant = (float)Math.Sqrt(Math.Pow((m_cameraPosition.Y/Math.Cos( MathHelper.ToRadians(/*m_fFov*/45/2) )), 2) + Math.Pow((double) m_cameraPosition.Y, 2));
			Vector2
				ret = new Vector2(fX / (m_iWindowWidth / fConstant) + (m_iWindowWidth / 2), fY / (m_iWindowHeight / fConstant) + (m_iWindowHeight / 2));

			return ret;
		}

		private Vector2 screenToWorldCoords(
			float fX,
			float fY
		)
		{
			float
				fConstant = (float)Math.Sqrt(Math.Pow((m_cameraPosition.Y / Math.Cos(MathHelper.ToRadians(/*m_fFov*/45 / 2))), 2) + Math.Pow((double)m_cameraPosition.Y, 2));
			Vector2
				ret = new Vector2(fX * (m_iWindowWidth / fConstant) - (m_iWindowWidth / 2), fY * (m_iWindowHeight / fConstant) - (m_iWindowHeight / 2));

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

		private void vInitNewGame(
		)
		{
			m_boGameOver = false;

			// Reset player
			m_player.position = new Vector3(-1000, 0, 0);
			m_player.velocity = new Vector2(0, 0);
			m_player.vSetRotation(0);

			// Reset homeplanet
			m_homePlanet.health = 100;

			// Reset projectiles
			m_projectiles.Clear();

			// Reset asteroids
			m_asteroids.Clear();
			m_dAsteroidFrequency = 3000;
			m_dPassedTime += m_dTimePlayed;
			m_dTimePlayed = 0;
			m_dGameTimeLastAsteroid = 0;
			m_iNrAsteroidsBlasted = 0;
		}

		private void vCheckCollisions(
			GameTime gameTime
		)
		{
			for (int i = 0; i < m_projectiles.Count; i++)
			{
				// Check projectile collision with homeplanet
				/*if (boCheckCircleCollision(m_projectiles[i].position, m_homePlanet.position, m_projectiles[i].fRadius, m_homePlanet.fRadius))
				{
					m_projectiles.RemoveAt(i);
					m_homePlanet.health -= 10;

					break;	// Projectile hit homeplanet, move on to the next projectile
				}*/

				// Chech collision between projectile and asteroid
				for (int j = 0; j < m_asteroids.Count; j++)
				{
					if (boCheckCircleCollision(m_projectiles[i].position, m_asteroids[j].position, m_projectiles[i].fRadius, m_asteroids[j].fRadius))
					{
						// Add explosion
						m_explosions.Add(new Vector3(m_projectiles[i].position.X, m_projectiles[i].position.Z, (float)gameTime.TotalGameTime.TotalMilliseconds));

						// Remove projectile
						m_projectiles.RemoveAt(i);

						// Reduce health or remove asteroid
						if (m_asteroids[j].health <= 0)
						{
							m_asteroids.RemoveAt(j);
							m_iNrAsteroidsBlasted++;
						}
						else
							m_asteroids[j].health -= 20;

						// Play impact soundeffect
						m_shotImpact.Play(0.1f, 0, 0);

						i--;

						break;	// Projectile hit something, move on to the next projectile
					}
				}
			}

			// Check collision between each asteroid and the homeplanet
			for (int i = 0; i < m_asteroids.Count; i++)
			{
				if (boCheckCircleCollision(m_asteroids[i].position, m_homePlanet.position, m_asteroids[i].fRadius, m_homePlanet.fRadius))
				{
					m_asteroids.RemoveAt(i); // Remove asteroid
					m_homePlanet.health -= 20; // The homeplanet takes damage

					// Play impact soundeffect
					m_asteroidImpact.Play(0.2f, 0, 0);

					i--;
				}
			}

			// Check for collisions between player and planet
			if (boCheckCircleCollision(m_player.position, m_homePlanet.position, m_player.fRadius, m_homePlanet.fRadius))
			{
				// Move the two circles apart so that they don't overlap
				float
					fOverlap,
					magnitude,
					V1nB,
					V1tB,
					V2nB,
					V2tB,
					V1tA,
					V2tA,
					V1nA,
					V2nA;

				Vector3
					distance = m_homePlanet.position - m_player.position;
				Vector2
					normalizedVelocity = m_player.velocity;
				normalizedVelocity.Normalize();
				fOverlap = (m_player.fRadius + m_homePlanet.fRadius) - distance.Length();
				Vector2
					overlapDistance = new Vector2(normalizedVelocity.X * fOverlap, normalizedVelocity.Y * fOverlap);

				m_player.position -= new Vector3(overlapDistance.X, 0, overlapDistance.Y);

				// The normal vector between the colliding surfaces of the m_balls
				float[]
					unitNormalVec = { m_homePlanet.position.X - m_player.position.X, m_homePlanet.position.Z - m_player.position.Z };

				// Normalize the vector
				magnitude = (float)Math.Sqrt(Math.Pow(unitNormalVec[0], 2) + Math.Pow(unitNormalVec[1], 2));
				unitNormalVec[0] /= magnitude;
				unitNormalVec[1] /= magnitude;

				// The tangent vector between the colliding surfaces of the m_balls
				float[]
					unitTangentVec = { -unitNormalVec[1], unitNormalVec[0] };

				// Resolve the SCALAR tangential and normal components from the original velocity vectors
				V1nB = (unitNormalVec[0] * m_player.velocity.X) + (unitNormalVec[1] * m_player.velocity.Y); // Normal vector for ball1
				V1tB = (unitTangentVec[0] * m_player.velocity.X) + (unitTangentVec[1] * m_player.velocity.Y);
				// Tangent vector for ball1
				V2nB = (unitNormalVec[0] * 0f /*planet velocity X*/) + (unitNormalVec[1] * 0f /*planet velocity Y*/);
				// Normal vector for ball2
				V2tB = (unitTangentVec[0] * 0f /*planet velocity X*/) + (unitTangentVec[1] * 0f /*planet velocity Y*/);
				// Tangent vector for ball2

				// Calculate velocity scalar AFTER the collision for the normal and tangent vectors
				V1tA = V1tB;
				V2tA = V2tB;
				// The tangent vector after are the same as the ones before, since there is no force in tangential direction between the m_balls

				// Calculate the normal scalars after collision for both m_balls
				V1nA = ((V1nB * ( /*player mass*/1f - /*planet mass*/ 100f)) + (2 * /*planet mass*/100f * V2nB)) /
							 ( /*player mass*/1f + /*planet mass*/ 100f); // Normal velocity scalar after collision for ball1
				V2nA = ((V2nB * ( /*planet mass*/100f - /*player mass*/ 1f)) + (2 * /*player mass*/1f * V1nB)) /
							 ( /*player mass*/1f + /*planet mass*/ 100f); // Normal velocity scalar after collision for ball2

				// Convert scalar normal/tangential velocities into vectors by multiplying them with the initial normal/tangential vectors
				float[]
					V1nVecA = { V1nA * unitNormalVec[0], V1nA * unitNormalVec[1] },
					V1tVecA = { V1tA * unitTangentVec[0], V1tA * unitTangentVec[1] },
					V2nVecA = { V2nA * unitNormalVec[0], V2nA * unitNormalVec[1] },
					V2tVecA = { V2tA * unitTangentVec[0], V2tA * unitTangentVec[1] },

					// Add the normal/tangential vectors for each object to find the final vectors
					V1A = { V1nVecA[0] + V1tVecA[0], V1nVecA[1] + V1tVecA[1] };

				// Assign the new vector to the planet
				m_player.velocity = new Vector2(V1A[0], V1A[1]);
			}

			// Check collisions between player and world boundaries
			float
				fBoundary = 2000;
			if (m_player.position.X < -fBoundary || m_player.position.X > fBoundary)
				m_player.velocity *= new Vector2(-1, 1);
			if (m_player.position.Z < -fBoundary || m_player.position.Z > fBoundary)
				m_player.velocity *= new Vector2(1, -1);

			// Don't allow shots to go outside boundary
			for(int i = 0; i < m_projectiles.Count; i++)
				if (m_projectiles[i].position.X < -fBoundary || m_projectiles[i].position.X > fBoundary || m_projectiles[i].position.Z < -fBoundary || m_projectiles[i].position.Z > fBoundary)
				{
					m_projectiles.RemoveAt(i);
					i--;
				}


		}	// End vCheckCollisions

		public void vSpawnAsteroids(
			GameTime gameTime
		)
		{
			// Spawn more asteroids based on how long the game has lasted
			if (m_asteroids.Count < 10 && (m_dTimePlayed - m_dGameTimeLastAsteroid) > m_dAsteroidFrequency)
			{
				m_dGameTimeLastAsteroid = m_dTimePlayed;

				if (m_dAsteroidFrequency > 1500)
					m_dAsteroidFrequency -= 30;

				int
					iRandDegree,
					iRandDist,
					iRandAsteroidModel,
					iRandAsteroidSize,
					iMinAsteroidSize = (50 + (int)(m_dTimePlayed * 0.0015)) > 150 ? 150 : (50 + (int)(m_dTimePlayed * 0.0015)),
					iMaxAsteroidSize = (50 + (int)(m_dTimePlayed * 0.0025)) > 150 ? 150 : (50 + (int)(m_dTimePlayed * 0.0025));
				Random
					randomDegree = new Random(),
					randomDistance = new Random(),
					randomAsteroidModel = new Random(),
					randomAsteroidSize = new Random();

				// Place randomly within a random unit radius between 1000 and 2000
				iRandDegree = randomDegree.Next(0, 360);
				iRandDist = randomDistance.Next(3500, 4000);
				iRandAsteroidModel = randomAsteroidModel.Next(0, m_asteroidModels.Count);
				iRandAsteroidSize = randomAsteroidSize.Next(iMinAsteroidSize, iMaxAsteroidSize);

				m_asteroids.Add(new Entity(new Vector3((float)Math.Cos(iRandDegree) * iRandDist, 0, (float)Math.Sin(iRandDegree) * iRandDist), m_asteroidModels[iRandAsteroidModel], iRandAsteroidSize));

				// Send the astoroid hurling towards the home planet
				Vector3
					asteroidVelocity = (m_homePlanet.position - m_asteroids.Last().position);
				asteroidVelocity.Normalize();
				asteroidVelocity *= (3 + (m_iNrAsteroidsBlasted/10));

				m_asteroids.Last().velocity = new Vector2(asteroidVelocity.X, asteroidVelocity.Z);
				m_asteroids.Last().health = (int)(iRandAsteroidSize*1.5f);
			}
		}
	}
}
