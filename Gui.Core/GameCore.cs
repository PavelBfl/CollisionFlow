using CollisionFlow;
using CollisionFlow.Polygons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SolidFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gui.Core
{
	public class GameCore : Game
	{
		public static Texture2D Pixel { get; private set; }
		private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 2f)
		{
			Vector2 delta = end - start;
			spriteBatch.Draw(Pixel, start, null, color, ToAngle(delta), new Vector2(0, 0.5f), new Vector2(delta.Length(), thickness), SpriteEffects.None, 0f);
		}
		private static void DrawPolygon(SpriteBatch spriteBatch, IEnumerable<Vector2> vertices, Color color, float thickness = 2f)
		{
			var verticesInstance = vertices.ToArray();
			var prevIndex = verticesInstance.Length - 1;
			for (int i = 0; i < verticesInstance.Length; i++)
			{
				DrawLine(spriteBatch, verticesInstance[prevIndex], verticesInstance[i], color, thickness);
				prevIndex = i;
			}
		}

		private static float ToAngle(Vector2 vector)
		{
			return (float)Math.Atan2(vector.Y, vector.X);
		}

		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private BodyDispatcher _bodyDispatcher;

		public GameCore()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			_bodyDispatcher = new BodyDispatcher();

			const double WEIGHT_MAX = 10;
			const double HEIGHT_MAX = 10;
			const double SPEED_MAX = 0;
			const int ROWS_COUNT = 5;
			const int COLUMNS_COUNT = 10;
			const double GLOBAL_OFFSET = 300;

			var random = new Random(1);
			for (int iRow = 0; iRow < ROWS_COUNT; iRow++)
			{
				for (int iColumn = 0; iColumn < COLUMNS_COUNT; iColumn++)
				{
					var offsetX = GLOBAL_OFFSET + iColumn * WEIGHT_MAX;
					var offsetY = iRow * HEIGHT_MAX;
					var centerX = offsetX + WEIGHT_MAX / 2;
					var centerY = offsetY + HEIGHT_MAX / 2;
					var points = PolygonBuilder.RegularPolygon((Math.Min(WEIGHT_MAX, HEIGHT_MAX) - 1) / 2, random.Next(3, 10))
						.Select(x => new Vector128(x.X + centerX, x.Y + centerY))
						.ToArray();

					var body = new Body(_bodyDispatcher.Dispatcher, points, new Vector128(random.NextDouble() * SPEED_MAX, random.NextDouble() * SPEED_MAX))
					{
						Pull = new Vector128(0, 0.1),
						Bounce = 0.8,
						Name = $"C:{iColumn};R:{iRow};V:{points.Length}",
					};
					_bodyDispatcher.Bodies.Add(body);
				}
			}

			var bottom = new Body(_bodyDispatcher.Dispatcher, new Vector128[]
			{
				new Vector128(0, -1000),
				new Vector128(0, 300),
				new Vector128(700, 300),
				new Vector128(700, -1000),
				new Vector128(710, -1000),
				new Vector128(710, 310),
				new Vector128(-10, 310),
				new Vector128(-10, -1000),
			})
			{
				Weight = 1000000000000,
				Center = new Vector128(350, 305),
				Name = "Bottom",
			};
			_bodyDispatcher.Bodies.Add(bottom);

			var bod = new Body(_bodyDispatcher.Dispatcher, new Vector128[]
			{
				new Vector128(350, 200),
				new Vector128(400, 250),
				new Vector128(300, 250),
			})
			{
				Weight = 1000000000000,
				Center = new Vector128(350, 225),
				Name = "Bod",
			};
			_bodyDispatcher.Bodies.Add(bod);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			Pixel = new Texture2D(GraphicsDevice, 1, 1);
			Pixel.SetData(new[] { Color.White });

			base.LoadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			_bodyDispatcher.Offset(1);
			base.Update(gameTime);
		}

		private static Vector2 GetCenter(Body body)
		{
			if (body.Center.HasValue)
			{
				return new Vector2((float)body.Center.Value.X, (float)body.Center.Value.Y);
			}
			else
			{
				return new Vector2(
					body.Handler.Vertices.Select(x => (float)x.Target.X).Average(),
					body.Handler.Vertices.Select(x => (float)x.Target.Y).Average()
				);
			}
		}
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
			foreach (var body in _bodyDispatcher.Bodies)
			{
				DrawPolygon(
					_spriteBatch,
					body.Handler.Vertices.Select(x => new Vector2((float)x.Target.X, (float)x.Target.Y)),
					Color.Red,
					1
				);
				var bodyCenter = GetCenter(body);
				foreach (var rest in body.RestOn)
				{
					var restCenter = GetCenter(rest);
					DrawLine(
						_spriteBatch,
						bodyCenter,
						restCenter,
						Color.Green,
						1
					);
				}
			}
			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
