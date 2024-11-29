using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindLuigi
{
    public enum CharacterEnum
    {
        LUIGI,
        MARIO,
        WARIO,
        YOSHI
    }

    public class Character
    {
        public Rectangle CharacterRectangle;

        private Texture2D? _characterTexture;

        private CharacterEnum _characterEnum;

        public int X;

        public int Y;

        public int xDirection = 1;

        public int yDirection = 1;

        public int xSpeed = 1;

        public int ySpeed = 1;

        private static Random random = new();

        public bool IsLuigi 
        { 
            get
            {
                return _characterEnum == CharacterEnum.LUIGI;
            } 
        }

        public Character(int x, int y, CharacterEnum characterEnum)
        {
            CharacterRectangle = new(x, y, 64, 64);
            _characterEnum = characterEnum;
            X = x;
            Y = y;
        }

        public void Update()
        {
            X += (xDirection * xSpeed);
            Y += (yDirection * ySpeed);

            if (X > 936 ||  X < 0)
            {
                xDirection *= -1;
                xSpeed = random.Next(1, 3) * (1 + FindLuigiGame.DifficultySpeedMultipler);

                X = X < 0 ? 0 : X;
                X = X > 936 ? 936 : X;

                if (yDirection == 0)
                    yDirection = random.Next(-1, 2) * (1 + FindLuigiGame.DifficultySpeedMultipler);
            }
            if (Y > 936 || Y < 0)
            {
                yDirection *= -1;
                ySpeed = random.Next(1, 3) * (1 + FindLuigiGame.DifficultySpeedMultipler);

                Y = Y < 0 ? 0 : Y;
                Y = Y > 936 ? 936 : Y;

                if (xDirection == 0)
                    xDirection = random.Next(-1, 2) * (1 + FindLuigiGame.DifficultySpeedMultipler);
            }

            CharacterRectangle = new(X, Y, 64, 64);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_characterTexture is null)
            {
                switch(_characterEnum)
                {
                    case CharacterEnum.LUIGI:
                        _characterTexture = CharacterTextures.LuigiTexture!;
                        break;
                    case CharacterEnum.MARIO:
                        _characterTexture = CharacterTextures.MarioTexture!;
                        break;
                    case CharacterEnum.WARIO:
                        _characterTexture = CharacterTextures.WarioTexture!;
                        break;
                    case CharacterEnum.YOSHI:
                        _characterTexture = CharacterTextures.YoshiTexture!;
                        break;
                }
            }

            spriteBatch.Draw(_characterTexture, CharacterRectangle, Color.White);
        }
    }

    public static class CharacterTextures
    {
        public static Texture2D? LuigiTexture;

        public static Texture2D? MarioTexture;

        public static Texture2D? WarioTexture;

        public static Texture2D? YoshiTexture;

        public static void LoadContent(ContentManager content)
        {
            LuigiTexture = content.Load<Texture2D>("Luigi");
            MarioTexture = content.Load<Texture2D>("Mario");
            WarioTexture = content.Load<Texture2D>("Wario");
            YoshiTexture = content.Load<Texture2D>("Yoshi");
        }
    }
}
