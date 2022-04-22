using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DinoGame
{
    class Cloud
    {
        public int WIDTH = 9;
        public int HEIGHT = 3;

        public int x;
        public int y;

        public int lastx;
        public int lasty;

        public bool IsDeleted = false;

        public string model;
        public string[] modelarray;

        private List<String> models = new List<String>
        {
            "  OOOO   \n OOOOOOOO\n OOOOOOOOO ",
            " OOOOOOO \nOOOOOOO  \n OOOOOOO "
        };

        private Timer timer;

        public Cloud(int WINDOW_WIDTH, int WINDOW_HEIGHT)
        {
            /** RANDOM MODEL */
            Random random = new Random();
            int i = random.Next(0, models.Count);
            model = models[i];

            /** Calculate height */
            modelarray = model.Split('\n');
            HEIGHT = modelarray.Length;
            WIDTH = modelarray[0].Length;

            x = WINDOW_WIDTH - WIDTH - 1;
            y = random.Next(1, 5);
            timer = new Timer
            (this.Move!,
               null,
               0,
               random.Next(100, 300));
        }

        private void Move(object state)
        {
            this.x--;
            if(this.x == 2)
            {
                this.IsDeleted = true;
                this.timer.Dispose();
            }
        }
    }
}