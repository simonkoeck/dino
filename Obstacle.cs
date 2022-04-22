using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DinoGame
{
    class Obstacle
    {

        public int x;
        public int y;

        public int lastx;
        public int lasty;

        public bool IsDeleted = false;

        // To check if the obstacle already incremented the score
        public bool HasIncrementedScore = false;

        public int WIDTH = 2;
        public int HEIGHT = 2;

        public string Model = "▒";

        private Timer timer;

        public Obstacle(int WINDOW_WIDTH, int WINDOW_HEIGHT, int TICK_SPEED)
        {
            Random r = new Random();
            WIDTH = r.Next(1, 4);
            HEIGHT = r.Next(1, 4);

            x = WINDOW_WIDTH - 1 - 1;
            y = 15 - HEIGHT;
            timer = new Timer
            (this.Move!,
               null,
               0,
               TICK_SPEED);

            
        }

        private void Move(object state)
        {
            this.x--;
            if (this.x == 2)
            {
                this.IsDeleted = true;
                this.timer.Dispose();
            }
        }
    }
}