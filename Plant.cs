using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DinoGame
{
    class Plant
    {

        public int x;
        public int y;

        public int lastx;
        public int lasty;

        public bool IsDeleted = false;

        public string Model = "|";

        private Timer timer;

        public Plant(int WINDOW_WIDTH, int WINDOW_HEIGHT, int TICK_SPEED)
        {

            x = WINDOW_WIDTH - 1 - 1;
            y = 14;
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

