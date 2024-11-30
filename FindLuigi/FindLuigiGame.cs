using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace FindLuigi
{
    public enum Difficulty
    {
        EASY,
        MEDIUM,
        HARD,
        ULTRA,
        IMPOSSIBLE
    }

    public class FindLuigiGame : Game
    {
        private GraphicsDeviceManager _graphics;

        private SpriteBatch? _spriteBatch;

        private List<Character> _characters = new();

        private Random random = new();

        private Timer? _timeout;

        private Timer? _gameTimer = new(1000);

        private SpriteFont? _scoreFont;

        private Difficulty _difficulty = Difficulty.EASY;

        private int _score = 0;

        private int _lives = 5;

        private int _time = 30;

        private int _highScore = 0;

        private bool _gameOver = false;

        private string _gameOverText = "GAME OVER!";

        private string _restartText = "Press 'R' to restart.";

        public static int DifficultySpeedMultipler = 0;

        public static int DifficultyDirectionMultiplier = 0;

        public static int DifficultyCharacterCountMultiplier = 1;

        public FindLuigiGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _gameTimer.Elapsed += GameTimerElapsed!;
            _gameTimer.AutoReset = true;
        }

        protected override void Initialize()
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferHeight = 1000;
            _graphics.PreferredBackBufferWidth = 1000;
            _graphics.ApplyChanges();

            Window.Title = "Find Luigi";

            GenerateRound();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _scoreFont = Content.Load<SpriteFont>("ScoreFont");

            CharacterTextures.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (!_gameOver && !_gameTimer!.Enabled)
                _gameTimer.Start();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            foreach (Character character in _characters)
            {
                if (!_gameOver)
                    character.Update();
            }

            MouseState state = Mouse.GetState();
            if (state.LeftButton == ButtonState.Pressed && (_timeout is null || !_timeout.Enabled) && !_gameOver)
            {
                Character? clickedCharacter = null;
                List<Character> eligibleClicks = new();

                foreach (Character character in _characters.GetReverse())
                {
                    if (character.CharacterRectangle.Contains(new Point(state.X, state.Y)))
                    {
                        eligibleClicks.Add(character);
                    }
                }

                foreach (Character character in eligibleClicks)
                {
                    if (character.IsLuigi)
                        clickedCharacter = character;
                }

                if (clickedCharacter is null && eligibleClicks.Any())
                    clickedCharacter = eligibleClicks.First();

                if (clickedCharacter is not null && clickedCharacter.IsLuigi)
                {
                    _score++;
                    Difficulty _prevDifficulty = _difficulty;

                    if (_score == 5)
                        _difficulty = Difficulty.MEDIUM;
                    else if (_score == 10)
                        _difficulty = Difficulty.HARD;
                    else if (_score == 25)
                        _difficulty = Difficulty.ULTRA;
                    else if (_score == 50)
                        _difficulty = Difficulty.IMPOSSIBLE;

                    if (_prevDifficulty != _difficulty)
                        UpdateDifficultyMultipliers();

                    GenerateRound();
                    StartClickTimeout();

                    _time += 5;
                }
                else if (clickedCharacter is not null)
                {
                    _characters.Remove(clickedCharacter);
                    _lives--;

                    if (_lives > 0)
                    {
                        StartClickTimeout();
                    }
                    else
                    {
                        _gameOver = true;
                    }
                }
            }

            if (_gameOver)
                _highScore = Math.Max(_score, _highScore);

            if (Keyboard.GetState().IsKeyDown(Keys.R) && _gameOver)
            {
                _score = 0;
                _time = 30;
                _lives = 5;
                _gameOver = false;
                _difficulty = Difficulty.EASY;

                UpdateDifficultyMultipliers();
                GenerateRound();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch!.Begin(samplerState: SamplerState.PointClamp);

            foreach (Character character in _characters)
            {
                if (!_gameOver || character.IsLuigi)
                    character.Draw(_spriteBatch);
            }

            string scoreAsText = "Score: " + _score.ToString();
            Vector2 scoreOrigin = _scoreFont!.MeasureString(scoreAsText) / 3;
            _spriteBatch.DrawString(_scoreFont!, scoreAsText, new Vector2(15 + (7 * scoreAsText.Length), 15), Color.White, 0, scoreOrigin, 3.0f, SpriteEffects.None, 0.5f);

            string livesAsText = "Liv es: " + _lives.ToString();
            Vector2 livesOrigin = _scoreFont!.MeasureString(livesAsText) / 3;
            _spriteBatch.DrawString(_scoreFont!, livesAsText, new Vector2(15 + (7 * scoreAsText.Length), 65), Color.White, 0, livesOrigin, 3.0f, SpriteEffects.None, 0.5f);

            string timeAsText = "Time: " + _time.ToString();
            Vector2 timeOrigin = _scoreFont!.MeasureString(timeAsText) / 3;
            _spriteBatch.DrawString(_scoreFont!, timeAsText, new Vector2(15 + (7 * scoreAsText.Length), 115), Color.White, 0, timeOrigin, 3.0f, SpriteEffects.None, 0.5f);

            if (_gameOver)
            {
                Vector2 gameOverOrigin = _scoreFont!.MeasureString(_gameOverText);
                _spriteBatch.DrawString(_scoreFont!, _gameOverText, new Vector2(775, 500), Color.White, 0, gameOverOrigin, 5.0f, SpriteEffects.None, 0.5f);

                Vector2 restartTextOrigin = _scoreFont!.MeasureString(_restartText);
                _spriteBatch.DrawString(_scoreFont!, _restartText, new Vector2(720, 550), Color.White, 0, restartTextOrigin, 3.0f, SpriteEffects.None, 0.5f);

                string highScoreAsText = "High Score: " + _highScore.ToString();
                Vector2 highScoreOrigin = _scoreFont!.MeasureString(highScoreAsText) / 3;
                _spriteBatch.DrawString(_scoreFont!, highScoreAsText, new Vector2(465, 575), Color.White, 0, highScoreOrigin, 3.0f, SpriteEffects.None, 0.5f);
            }

            _spriteBatch!.End();

            base.Draw(gameTime);
        }


        private void GenerateRound()
        {
            _characters.Clear();

            int totalCharacters = random.Next(75, 100) * DifficultyCharacterCountMultiplier;
            List<CharacterEnum> characterEnums = Enum.GetValues<CharacterEnum>().ToList();
            characterEnums.Remove(CharacterEnum.LUIGI);

            int canMove = random.Next(0, 1 + DifficultyDirectionMultiplier);

            for (int i = 0; i < totalCharacters; i++)
            {
                int x = random.Next(1, 46) * 20;
                int y = random.Next(1, 46) * 20;
                int characterType = random.Next(0, 3);

                if (x < 150 && y < 150)
                {
                    x += 90;
                    y += 90;
                }

                Character character = new Character(x, y, characterEnums[characterType]);

                character.xDirection = random.Next(-1, 2) * canMove;
                character.yDirection = random.Next(-1, 2) * canMove;
                character.xSpeed = random.Next(1, 3) * (1 + DifficultySpeedMultipler);
                character.ySpeed = random.Next(1, 3) * (1 + DifficultySpeedMultipler);

                _characters.Add(character);
            }

            Character luigi = new Character(random.Next(1, 46) * 20, random.Next(1, 46) * 20, CharacterEnum.LUIGI);
            luigi.xDirection = random.Next(-1, 2) * canMove;
            luigi.yDirection = random.Next(-1, 2) * canMove;
            luigi.xSpeed = random.Next(1, 3) * (1 + DifficultySpeedMultipler);
            luigi.ySpeed = random.Next(1, 3) * (1 + DifficultySpeedMultipler);

            _characters.Add(luigi);
            _characters.Shuffle();
        }

        public void UpdateDifficultyMultipliers()
        {
            switch(_difficulty)
            {
                case Difficulty.EASY:
                    DifficultySpeedMultipler = 0;
                    DifficultyDirectionMultiplier = 0;
                    DifficultyCharacterCountMultiplier = 1;
                    break;
                case Difficulty.MEDIUM:
                    DifficultyCharacterCountMultiplier = 2;
                    DifficultyDirectionMultiplier = 1;
                    break;
                case Difficulty.HARD:
                    DifficultyCharacterCountMultiplier = 3;
                    break;
                case Difficulty.ULTRA:
                    DifficultyCharacterCountMultiplier = 2;
                    DifficultySpeedMultipler = 2;
                    break;
                case Difficulty.IMPOSSIBLE:
                    DifficultyCharacterCountMultiplier = 3;
                    DifficultySpeedMultipler = 3;
                    break;
            }
        }

        public void StartClickTimeout()
        {
            _timeout = new(500);
            _timeout.Elapsed += OnTimeoutEvent!;
            _timeout.Start();
        }

        public static void OnTimeoutEvent(object source, ElapsedEventArgs e)
        {
            // Stop and dispose of the timer
            Timer timer = (Timer)source;
            timer.Stop();
            timer.Dispose();
        }

        public void GameTimerElapsed(object source, ElapsedEventArgs e)
        {
            Timer timer = (Timer)source;

            if (_time > 0 && !_gameOver)
            {
                _time--;
            }
            else
            {
                _gameOver = true;

                timer.AutoReset = false;
            }

            timer.Stop();
        }
    }
}
