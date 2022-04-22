using System.Runtime.InteropServices;


enum Difficulty
{
    EASY,
    MEDIUM,
    HIGH,
    EXPERT
}

namespace DinoGame
{
    class Game
    {
        /** Window variables */
        const int WIDTH = 120;
        const int HEIGHT = 21;

        public bool IsGameOver = false;

        public int score = 0;
        public int prevScore = -1;

        public Difficulty difficulty = Difficulty.EASY;

        /** Timer variables */
        const int RENDER_TICKS = 40;
        private bool IsDrawing = false; // Makes sure that only one render timer is executed once a tick,
                                        // if one draw takes a bit longer - reduces bugs
        private Timer? renderTimer;
        private Timer? collisionDetectionTimer;
        private Timer? scoreDetectionTimer;

        /** Ground variables */
        const int GROUND_HEIGHT = 4;

        /** Player variables */
        const int PLAYER_WIDTH = 3;
        const int PLAYER_HEIGHT = 4;

        private int LAST_PLAYER_X;
        private int LAST_PLAYER_Y;

        private int PLAYER_SPEED = 28; // Milli-Seconds

        private const int PLAYER_X = 0 + 15;
        private int PLAYER_Y = HEIGHT - GROUND_HEIGHT - PLAYER_HEIGHT - 2;

        private const int PLAYER_JUMP_HEIGHT = 7;
        private bool isJumping = false;


        /** Clouds */
        private List<Cloud> clouds = new List<Cloud>();
        private Timer? instantiateCloudsTimer;

        /** Plants */
        private List<Plant> plants = new List<Plant>();
        private Thread? instantiatePlantsThread;

        /** Plants */
        private List<Obstacle> obstacles = new List<Obstacle>();
        private Thread? instantiateObstaclesThread;



        private string[] playermodel = new string[]
        {
            @" O ",
            @"/I\",
            @" | ",
            @"/ \"
        };



        public void Init()
        {
            score = 0;
            prevScore = -1;
            IsGameOver = false;
            difficulty = Difficulty.EASY;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                /** Initialize Console / Window */
                Console.SetWindowSize(WIDTH, HEIGHT);
            }

            /** Clear Window */
            Console.Clear();

            /** Render edge */
            RenderEdge();
            RenderGround();

            /** Initialize Render Timer */
            renderTimer = new Timer
            (this.Render!,
               null,
               0,
               RENDER_TICKS);

            /** Initialize Collision-Detection Timer */
            collisionDetectionTimer = new Timer(
                this.CollisionDetection!, null, 0, RENDER_TICKS);

            /** Initialze Score Detection Timer */
            scoreDetectionTimer = new Timer(
                this.ScoreDetection!, null, 0, RENDER_TICKS);

            /** Create Cloud Timer */
            instantiateCloudsTimer = new Timer
            (this.CreateCloud!,
               null,
               2000,
               18000);


            instantiatePlantsThread = new Thread(this.CreatePlant!);
            instantiatePlantsThread.Start();

            instantiateObstaclesThread = new Thread(this.CreateObstacle!);
            instantiateObstaclesThread.Start();


            /** Key Events */
            while (true)
            {
                if (IsGameOver) return;
                ConsoleKeyInfo k = Console.ReadKey();
                if (k.Key == ConsoleKey.Escape)
                {
                    /** Exit Game */
                    Environment.Exit(1);
                }
                else if (k.Key == ConsoleKey.Spacebar)
                {
                    Thread thread1 = new Thread(this.Jump);
                    thread1.Start();
                }
            }
        }

        private void ScoreDetection(object state)
        {
            foreach (Obstacle o in obstacles)
            {
                if (o.x < PLAYER_X && !o.HasIncrementedScore)
                {
                    score++;
                    o.HasIncrementedScore = true;

                    Difficulty lastDif = difficulty;

                    if (score >= 40) difficulty = Difficulty.EXPERT;
                    else if (score >= 14) difficulty = Difficulty.HIGH;
                    else if (score >= 6) difficulty = Difficulty.MEDIUM;

                    if(lastDif != difficulty) PLAYER_SPEED -= 3;

                }
            }
        }


        private void CollisionDetection(object state)
        {
            foreach(Obstacle o in obstacles)
            {
                if (o == null) continue;
                if(o.x >= PLAYER_X && o.x <= PLAYER_X + PLAYER_WIDTH &&
                    o.y >= PLAYER_Y && o.y <= PLAYER_Y + PLAYER_HEIGHT)
                {
                    GameOver();
                }
            }
        }

        private void GameOver()
        {
            renderTimer?.Dispose();
            // instantiatePlantsThread.Abort();
            instantiateCloudsTimer?.Dispose();
            // instantiateObstaclesThread.Abort();
            collisionDetectionTimer?.Dispose();

            IsGameOver = true;

            Console.Clear();
            Console.WriteLine(@"
          ___                   ___              
         / __|__ _ _ __  ___   / _ \__ _____ _ _ 
        | (_ / _` | '  \/ -_) | (_) \ V / -_) '_|
         \___\__,_|_|_|_\___|  \___/ \_/\___|_|  
                                         
                      Score: %score%
           Press any key to restart the game...
".Replace("%score%", score.ToString()));
        }

        private void Render(object state)
        {
            if (IsDrawing) return;
            IsDrawing = true;


            /** Render Score */
            if(score != prevScore)
            {
                Console.SetCursorPosition(WIDTH - 13, 0);
                Console.Write("Score " + score.ToString());
                prevScore = score;
            }
            

            List<Cloud> ctemp = new List<Cloud>(clouds);
            foreach (Cloud c in ctemp)
            {

                if (c.IsDeleted)
                {
                    /** Remove last object frame */
                    for (var w = 0; w < c.WIDTH; w++)
                    {
                        for (var h = 0; h < c.HEIGHT; h++)
                        {
                            Console.SetCursorPosition(c.lastx + w, c.lasty + h);
                            Console.Write(" ");
                        }
                    }
                    clouds.Remove(c);
                    continue;
                }

                /** Check if has updated */
                if (c.lastx != c.x || c.lasty != c.y)
                {
                    /** Remove last object frame */
                    for (var w = 0; w < c.WIDTH; w++)
                    {
                        for (var h = 0; h < c.HEIGHT; h++)
                        {
                            Console.SetCursorPosition(c.lastx + w, c.lasty + h);
                            
                            Console.Write(" ");
                        }
                    }
                    /** Re-Render */
                    for (var w = 0; w < c.WIDTH; w++)
                    {
                        for (var h = 0; h < c.HEIGHT; h++)
                        {
                            /** Render Cloud */
                            Console.SetCursorPosition(c.x + w, c.y + h);
                            Console.Write(c.modelarray[h].Substring(w, 1));
                        }
                    }
                    c.lastx = c.x;
                    c.lasty = c.y;
                }
            }



            /** Render plants */
            List<Plant> ptemp = new List<Plant>(plants);
            foreach (Plant p in ptemp)
            {

                if (p.IsDeleted)
                {
                    /** Remove last object frame */
                    Console.SetCursorPosition(p.lastx, p.lasty);
                    Console.Write(" ");
                    plants.Remove(p);
                    continue;
                }

                /** Check if has updated */
                if (p.lastx != p.x || p.lasty != p.y)
                {
                    /** Don't draw Plant in Player */
                    if (p.x >= PLAYER_X && p.x <= PLAYER_X + PLAYER_WIDTH && p.y == PLAYER_Y + PLAYER_WIDTH) continue;
                    /** Remove last object frame */
                    Console.SetCursorPosition(p.lastx, p.lasty);
                    Console.Write(" ");
                    /** Re-Render */
                    Console.SetCursorPosition(p.x, p.y);
                    Console.Write(p.Model);
                    p.lastx = p.x;
                    p.lasty = p.y;
                }
            }

            /** Render obstacles */
            List<Obstacle> otemp = new List<Obstacle>(obstacles);
            foreach (Obstacle p in otemp)
            {

                if (p.IsDeleted)
                {
                    /** Remove last object frame */
                    for (var w = 0; w < p.WIDTH; w++)
                    {
                        for (var h = 0; h < p.HEIGHT; h++)
                        {
                            Console.SetCursorPosition(p.lastx + w, p.lasty + h);
                            Console.Write(" ");
                        }
                    }
                    obstacles.Remove(p);
                    continue;
                }

                /** Check if has updated */
                if (p.lastx != p.x || p.lasty != p.y)
                {
                    /** Remove last object frame */
                    for (var w = 0; w < p.WIDTH; w++)
                    {
                        for (var h = 0; h < p.HEIGHT; h++)
                        {
                            Console.SetCursorPosition(p.lastx + w, p.lasty + h);

                            Console.Write(" ");
                        }
                    }
                    /** Re-Render */
                    for (var w = 0; w < p.WIDTH; w++)
                    {
                        for (var h = 0; h < p.HEIGHT; h++)
                        {
                            /** Render Cloud */
                            Console.SetCursorPosition(p.x + w, p.y + h);
                            Console.Write(p.Model);
                        }
                    }
                    p.lastx = p.x;
                    p.lasty = p.y;
                }
            }

            /** Render Player */
            if (LAST_PLAYER_X != PLAYER_X || LAST_PLAYER_Y != PLAYER_Y)
            {
                /** Remove Player */
                for (var w = 0; w < PLAYER_WIDTH; w++)
                {
                    for (var h = 0; h < PLAYER_HEIGHT; h++)
                    {
                        Console.SetCursorPosition(LAST_PLAYER_X + w, LAST_PLAYER_Y + h);
                        Console.Write(" ");
                    }
                }
                /** Render Player */
                for (var w = 0; w < PLAYER_WIDTH; w++)
                {
                    for (var h = 0; h < PLAYER_HEIGHT; h++)
                    {
                        Console.SetCursorPosition(PLAYER_X + w, PLAYER_Y + h);
                        Console.Write(playermodel[h][w]);
                    }
                }
                LAST_PLAYER_X = PLAYER_X;
                LAST_PLAYER_Y = PLAYER_Y;
            }



            /** Set cursor to edge */
            Console.SetCursorPosition(0, HEIGHT - 1);


            IsDrawing = false;

        }

        private void RenderEdge()
        {
            for (var i = 0; i < WIDTH; i++)
            {
                /** Write top bar */
                Console.SetCursorPosition(i, 0);
                Console.Write("-");

                /** Write bottom bar */
                Console.SetCursorPosition(i, HEIGHT - 2);
                Console.Write("-");
            }
            for (var i = 1; i < HEIGHT - 2; i++)
            {
                /** Write top bar */
                Console.SetCursorPosition(0, i);
                Console.Write("|");

                /** Write bottom bar */
                Console.SetCursorPosition(WIDTH - 1, i);
                Console.Write("|");
            }
        }

        private void RenderGround()
        {
            /** Render ground */
            for (var x = 1; x < WIDTH - 1; x++)
            {
                for (var y = 0; y < GROUND_HEIGHT; y++)
                {
                    Console.SetCursorPosition(x, HEIGHT - y - 3);
                    Console.Write("▓");
                }
            }
        }

        private void CreateCloud(object state) { clouds.Add(new Cloud(WIDTH, HEIGHT)); }
        private void CreatePlant(object state)
        {
            Random r = new Random();
            while (true)
            {
                if(IsGameOver) break;

                plants.Add(new Plant(WIDTH, HEIGHT, PLAYER_SPEED));
                Thread.Sleep(r.Next(2000, 6700));
            }
        }

        private void CreateObstacle(object state)
        {
            Random r = new Random();
            while (true)
            {
                if(IsGameOver) break;

                obstacles.Add(new Obstacle(WIDTH, HEIGHT, PLAYER_SPEED));
                int min = 0;
                int max = 0;
                if(difficulty == Difficulty.EASY)
                {
                    min = 3000;
                    max = 3800;
                }
                else if (difficulty == Difficulty.MEDIUM)
                {
                    min = 2000;
                    max = 2300;
                }
                else if (difficulty == Difficulty.HIGH)
                {
                    min = 1000;
                    max = 1700;
                }
                else if (difficulty == Difficulty.EXPERT)
                {
                    min = 600;
                    max = 950;
                }
                Thread.Sleep(r.Next(min, max));
            }
        }


        private void Jump()
        {
            if (isJumping) return;
            isJumping = true;
            int startplayery = PLAYER_Y;
            int vel = -1;
            while (isJumping)
            {
                if (startplayery - PLAYER_Y == PLAYER_JUMP_HEIGHT) vel *= -1;
                PLAYER_Y += vel;
                if (PLAYER_Y == startplayery)
                {
                    isJumping = false;
                };
                Thread.Sleep(40);
            }
        }
    }
}