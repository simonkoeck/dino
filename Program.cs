using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"
                   ___  _                        O 
                  / _ \(_)__  ___               /I\
                 / // / / _ \/ _ \               |
                /____/_/_//_/\___/              / \
                - Simon Koeck -
          (https://github.com/simonkoeck)
    --------------- CONTROLS ---------------
       [ESC] To exit game [Space] To jump
    ----------------------------------------
       Press any key to start the Dino...");

            /** Wait for Key-Press */
            Console.ReadKey();

            while (true)
            {
                Game game = new Game();
                    game.Init();
                
            }
    
        }
    }
}