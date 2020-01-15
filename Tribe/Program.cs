using System;
using System.Windows.Forms;

namespace Tribe
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form = new Form1();

            Application.Run(form);


            // 7,324 lines. 2159. Only 5,000 are useful!

            /*
             * TODO:
             * 
             *      Need a way to increase interest in playing the game/simulation. Right now its pretty boring because gathering food and hunting
             *          are the only real things you can do. Crafting makes it interesting
             *          
             *          More crafting items
             *          Build campfire to gain warmth
             *          Clothes which provide armor and cold resistance (How to display this though?)
             * 
             *      Task based need delta filtering
             *          Balance filter values.
             *          Finished the panel to control tasks.
             *          Need to make sure these work, really hard to test at this point though.
             *      
             *      Maybe add a Cosmetic class for GameObjects that are attached to people. Make male/female crowns for the player.
             *      
             *      More plants and items.
             *      Would need the Pickup class to act more like items. Each person will store a list of Pickups, but adding another one will increment
             *          the value of the Pickup. Pickups in the inventory should advertise actions to the person holding them.
             *      
             *      Reduce number of events. Definitely item requests. Effect events might be bad too because sleep images stay on screen.
             *          Maybe effects need to be checked explicitly each tick.
             *      
             *      Allow people to request any type of item. This will require more AI work though.
             *      
             *      Work on mating rates. These need to be moderated by the amount of food stored in the house so the People don't starve to death.
             *      
             * Now:
             *  
             *          Add campfire object. This will scare off bears and provide warmth for people. Not sure if this should increase social or be another
             *              survival attribute to balance just like hunger.
             *              Add dynamic weather with this displayed somehow on the form. Of course I never run this long enough for a 15 min. day/night
             *                  cycle.
             *              Campfire object added with proximity based warming. May need to adjust the distance on this. It just checks if a campfire
             *                  is within the "nearbyObjects" list taken from the ObjectMesh. This is a huge area compared to how far the campfire 
             *                  should reach.
             *                  
             *          Add need changes for the player. Need to indicate hunger more visibly though so the player doesn't have to keep a display
             *              panel open.
             *              
             *          Command system
             *      
             *          Make equipping a spear increase attack range!
             *      
             *      
             *          Allowed people to eat food off the ground instead of picking it up. This may be redundant, but it may also prevent people from 
             *              exploiting the jobfullfilment bonus from picking up the food, then eating it immediately without carrying the food home.
             *          
             *          
             *      Human community not self sustaining. The bears kill them after a while. Not sure how to feel about this.
             *      
             * BUGLIST:
             *      
             *      Data.UpdateElement threw an exception after opening the inventory menu, closing it, and the player dies.
             *          Literally only happens when the inventory menu is opened then closed.
             *          Something is going on where the Player's CancelData event is still attached, but the DisplayPanel's dataObject is null.
             *      
             *      
             *      Not much I can do about it, but be careful having too many display panels open at the same time. It overloads the event system so some
             *          of them are ignored.
             *      
             */
        }
    }
}
