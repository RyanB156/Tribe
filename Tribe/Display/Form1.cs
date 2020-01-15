using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tribe
{

    public partial class Form1 : Form
    {

        private WorldController worldController;

        private Point mouseLocation;
        private bool shiftHeld = false;

        private Bitmap exitImage;
        private Point displayPanelMouseDownLocation;

        private List<DisplayPanel> displayPanels;
        private DisplayPanel activeDisplayPanel = null;

        private Timer RenderTimer;
        private GameTime gameTime;
        private DateTime startTime;
        private static int frameRate = 24; // per second.
        private static double updateRate = 24.0; // per second. 
        private static double updateDelta = frameRate / updateRate; // Increment by this amount each tick to get updateRate per second.

        public Form1()
        {
            InitializeComponent();

            displayPanels = new List<DisplayPanel>();


            Utilities.ResourceDirectory = @"..\..\ResourceImages\";
            Utilities.DefaultImage = new Bitmap(Utilities.ResourceDirectory + "default.png");
            // Set Size of the display area.
            Utilities.ViewWidth = myPanel.Width;
            Utilities.ViewHeight = myPanel.Height;


            exitImage = Utilities.GetResourceImage("close.png");
            myPanel.Paint += GamePanel_Paint;

            #region GameTime
            gameTime = new GameTime(15); // Setup the game time object to cycle day to night in <n> minutes.
            gameTime.DayNightToggle += OnDayNightToggle;
            SetDayNightBox(); // Set day/night image.
            startTime = DateTime.Now;
            timeLabel.Text = gameTime.ToString();
            #endregion


            worldController = new WorldController(gameTime.Copy(), 2 * myPanel.Width, 2 * myPanel.Height);
            worldController.PlayerDied += WorldController_PlayerDied;

            RenderTimer = new Timer() { Interval = (int)Math.Ceiling(1000.0 / frameRate) };
            RenderTimer.Tick += Timer_Tick;
            RenderTimer.Start();

            KeyPreview = true;

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            Resize += Form1_Resize;
            myPanel.MouseClick += MyPanel_MouseClick;
            myPanel.MouseMove += MyPanel_MouseMove;

        }

        private void AddDisplayPanel(IGetData dataObject, bool canSwap, Point point)
        {
            DisplayPanel d = new DisplayPanel(dataObject, exitImage, canSwap, point, myPanel.Width, myPanel.Height);

            displayPanels.Add(d);
            myPanel.Controls.Add(d.Panel);
            d.Close += RemoveDisplayPanel;
            d.Swap += DisplayPanelSwap;
            d.Panel.MouseMove += DisplayPanel_MouseMove;
            d.MouseDown += DisplayPanel_MouseDown;

            // Deactivate the old panel, reassign the active panel, then activate the new panel.
            if (activeDisplayPanel != null)
                activeDisplayPanel.Deactivate();
            activeDisplayPanel = d;
            activeDisplayPanel.Activate();
        }

        // Ferry swap message from DisplayPanel to WorldController when the player clicks the swap button.
        // Setting the player to the IGetData object should not fail because WorldController takes care of this logic when returning the object
        //  found in the ObjectMesh. It makes sure the player is dead and that the selected object is a Person.
        private void DisplayPanelSwap(object o, SwapEventArgs e)
        {
            worldController.SetPlayerToObject(e.DataObject);
        }

        // The exit button on one of the display panels has been clicked. Remove the display panel from the list.
        private void RemoveDisplayPanel(object sender, EventArgs e)
        {
            DisplayPanel d = (DisplayPanel)sender;

            d.Close -= RemoveDisplayPanel;
            d.Swap -= DisplayPanelSwap;
            d.Panel.MouseMove += DisplayPanel_MouseMove;
            d.MouseDown += DisplayPanel_MouseDown;
            myPanel.Controls.Remove(d.Panel);
            displayPanels.Remove(d); // Delete the reference to the DisplayPanel so the garbage collector can remove it.

            if (displayPanels.Count >= 1)
            {
                activeDisplayPanel = displayPanels[0];
                activeDisplayPanel.Activate();
            }
            else
                activeDisplayPanel = null;

            Focus();
        }

        // Start dragging a display panel
        private void DisplayPanel_MouseDown(object sender, MouseEventArgs e)
        {
            DisplayPanel d = (DisplayPanel)sender;

            if (e.Button == MouseButtons.Left)
            {
                displayPanelMouseDownLocation = e.Location;

                if (activeDisplayPanel != null)
                    activeDisplayPanel.Deactivate(); // Remove border markings for the old active DisplayPanel.

                activeDisplayPanel = d;
                activeDisplayPanel.Activate(); // Apply border markings for the new active DisplayPanel.
            }

        }

        // Move a display panel around the screen.
        private void DisplayPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Panel p = (Panel)sender;

                p.Left += e.X - displayPanelMouseDownLocation.X;
                p.Top += e.Y - displayPanelMouseDownLocation.Y;

            }
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            worldController.Render(e.Graphics);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - startTime;
            if (elapsedTime.TotalMilliseconds >= 1000) // If a second has passed. This should execute 1 per <frameRate> ticks.
            {
                gameTime.AddSeconds(1);

                //Parallel.Invoke(() => DisplayPanelUpdate());

                timeLabel.Text = gameTime.ToString();
                startTime = currentTime;
            }

            worldController.Update(gameTime.Copy(), mouseLocation.X, mouseLocation.Y);
                
            myPanel.Invalidate(); // Cause gamePanel to update and render objects to the screen.
        }

        private void WorldController_PlayerDied(object sender, EventArgs e)
        {
            MessageBox.Show("You Died. Select another person to continue");
        }

        private void MyPanel_MouseMove(object sender, MouseEventArgs e)
        {
            mouseLocation = e.Location;
        }

        public void OnDayNightToggle(object o, EventArgs e)
        {
            SetDayNightBox();
        }

        public void SetDayNightBox()
        {
            dayNightBox.Image?.Dispose();
            Bitmap b = gameTime.IsDay ? new Bitmap(Utilities.ResourceDirectory + "sun.png") : new Bitmap(Utilities.ResourceDirectory + "moon.png");
            dayNightBox.Image = b;
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            myPanel.Size = Size; // Resize the panel to match the new size of the form
            Utilities.ViewWidth = myPanel.Width;
            Utilities.ViewHeight = myPanel.Height;
        }


        private void ShowDisplayPanel(WorldClickData data)
        {

            if (data == null)
            {
                Console.WriteLine("Tried to display null data in Form1.ShowDisplayPanel()");
                return;
            }

            foreach (DisplayPanel d in displayPanels)
            {
                if (d.DataObject == data.DataObject)
                {
                    Console.WriteLine("Error: Attempted to create a duplicate display panel.");
                    return;
                }
            }

            AddDisplayPanel(data.DataObject, data.CanSwap, mouseLocation);
        }

        private void SwapToDisplayPanel(int i)
        {
            activeDisplayPanel.Deactivate();
            activeDisplayPanel = displayPanels[i];
            activeDisplayPanel.Activate();
        }
        
        private void InventoryDisplay()
        {
            WorldClickData clickData = worldController.GetPlayerInventory(mouseLocation);
            ShowDisplayPanel(clickData);
        }

        private void ClickAndDisplay()
        {
            if (mouseLocation == null)
                return;

            WorldClickData clickData = worldController.GetObjectData(mouseLocation, shiftHeld);

            if (clickData != null && clickData.Data != null)
            {
                ShowDisplayPanel(clickData);
            }   
        }

        // Crafting menu. Communicate with WorldController to get the list of items that can be crafted
        //  and add a new DisplayPanel.
        private void CraftingDisplay()
        {
            ShowDisplayPanel(new WorldClickData(worldController.CraftingMenu, false, true, mouseLocation));
        }

        private void EffectBox_Click(object sender, MouseEventArgs e)
        {
            PictureBox box = (PictureBox)sender;
            mouseLocation = new Point(e.Location.X + box.Location.X, e.Location.Y + box.Location.Y);
            ClickAndDisplay();
        }

        private void MyPanel_MouseClick(object sender, MouseEventArgs e)
        {
            mouseLocation = e.Location;

            // If the user right clicks on the house, open the command menu.
            if (worldController.EntityController.House.GetRectangle().Contains(mouseLocation) && e.Button == MouseButtons.Right)
            {
                // Need some way to transfer the data from the form (I still need to create that too) to WorldController, then to EntityController
                //  to add it to the list of tasks in PersonBrain to apply the need delta modifications.
                // Reuse code from display panels somehow, too tired to do that right now.
            }

            Console.WriteLine($"Mouse: {mouseLocation}");
            ClickAndDisplay();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // Route these four commands to player because they are player specific.
                case Keys.W:
                    worldController.ChangePlayerVelocity(Direction.Up);
                    break;
                case Keys.D:
                    worldController.ChangePlayerVelocity(Direction.Right);
                    break;
                case Keys.S:
                    worldController.ChangePlayerVelocity(Direction.Down);
                    break;
                case Keys.A:
                    worldController.ChangePlayerVelocity(Direction.Left);
                    break;
                //
                case Keys.ShiftKey:
                    shiftHeld = true;
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // Route these four commands to player because they are player specific.
                case Keys.W:
                    worldController.ChangePlayerVelocity(Direction.Up, true);
                    break;
                case Keys.D:
                    worldController.ChangePlayerVelocity(Direction.Right, true);
                    break;
                case Keys.S:
                    worldController.ChangePlayerVelocity(Direction.Down, true);
                    break;
                case Keys.A:
                    worldController.ChangePlayerVelocity(Direction.Left, true);
                    break;

                case Keys.ShiftKey:
                    shiftHeld = false;
                    break;

                case Keys.E:
                    if (!worldController.PlayerIsDead)
                        CraftingDisplay();
                    break;

                case Keys.I:
                    InventoryDisplay();
                    break;
                //
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                    worldController.SpawnChosenAnimal(e.KeyCode - Keys.D1, mouseLocation.X, mouseLocation.Y);
                    break;

                case Keys.Space:
                    worldController.PlayerAttack();
                    break;
                case Keys.Escape: // Pause the game.
                    RenderTimer.Enabled = !RenderTimer.Enabled;
                    break;
                case Keys.Tab:
                    // Cycle through the current DisplayPanels.
                    if (displayPanels.Count > 1 && activeDisplayPanel != null)
                    {
                        int displayPanelIndex = displayPanels.FindIndex(dp => dp == activeDisplayPanel);
                        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                        {
                            SwapToDisplayPanel((displayPanelIndex == 0) ? (displayPanels.Count - 1) : (displayPanelIndex - 1));
                        }
                        else
                        {
                            SwapToDisplayPanel((displayPanelIndex + 1) % displayPanels.Count);
                        }
                    }
                    
                    
                    break;
                case Keys.R:
                    Application.Restart();
                    break;
                case Keys.G:
                    worldController.PlayerToggleSex();
                    break;
                case Keys.N:
                    worldController.SpawnItem(ItemType.Apple);
                    break;
                case Keys.B:
                    worldController.SpawnRandomAnimal();
                    break;
                case Keys.M:
                    worldController.PlayerMate();
                    break;
                case Keys.P:

                    // If EntityController's DisplayPanel is displayed and active, have the player take an item from the house.
                    if (activeDisplayPanel != null && activeDisplayPanel.DataObject is EntityController)
                    {
                        worldController.PlayerPickup(true);
                    }
                    else if (activeDisplayPanel != null && activeDisplayPanel.DataObject is CraftingMenu)
                    {
                        // TODO: Create indexing into the crafting menu...
                        Console.WriteLine("Trying to craft a {0}", worldController.CraftingMenu.Recipes[worldController.CraftingMenu.Index].Item2);
                        if (worldController.TryCraftItem())
                        {
                            worldController.EntityController.Player.AddItem(worldController.CraftingMenu.Recipes[worldController.CraftingMenu.Index].Item2);
                        }
                        else
                        {
                            MessageBox.Show("You cannot craft this item right now");
                        }
                    }
                    else
                    {
                        worldController.PlayerPickup(false);
                    }
                    
                    break;
                case Keys.Q:
                    if ((ModifierKeys & Keys.Shift) == Keys.Shift) // Shift + q -> drop all items.
                        worldController.PlayerDropAllItems();
                    else
                        worldController.PlayerDropItems(); // Drop one item at a time.
                    break;
                case Keys.Oem4:
                    if (activeDisplayPanel != null && activeDisplayPanel.Indexable) // Change selection index in the active DisplayPanel.
                    {
                        activeDisplayPanel.DataObject.DecrementItemIndex();
                        activeDisplayPanel.SetIndex(activeDisplayPanel.DataObject.GetItemIndex());
                    }
                    break;
                case Keys.Oem6:
                    if (activeDisplayPanel != null && activeDisplayPanel.Indexable) // Change selection index in the active DisplayPanel.
                    {
                        activeDisplayPanel.DataObject.IncrementItemIndex();
                        activeDisplayPanel.SetIndex(activeDisplayPanel.DataObject.GetItemIndex());
                    }
                    break;
                // View Scaling
                case Keys.OemMinus:
                    worldController.DecreaseScale();
                    break;
                case Keys.Oemplus:
                    worldController.IncreaseScale();
                    break;
                //
                case Keys.T:
                    taskPanel.Visible = !taskPanel.Visible;
                    break;
                case Keys.X:
                    worldController.PlayerHarvest();
                    break;
                case Keys.Z:
                    worldController.PlayerSleep();
                    break;
            }
        }

        // *** Probably not the best setup but Form1 should have no idea what tasks, actions, and Entities are...
        // Maybe change this to an action hierarchy setup similar to Rimworld. This will have to be applied globally though.
        private void ApplyTaskButton_Click(object sender, EventArgs e)
        {
            bool[] taskSelect = { pickupTask.Checked, huntTask.Checked, mateTask.Checked, sleepTask.Checked, guardTask.Checked };
            string[] taskData = { pickupTypeList.CheckedItems.Count > 0 ? pickupTypeList.CheckedItems[0].Text : "",
                                    huntTypeList.CheckedItems.Count > 0 ? huntTypeList.CheckedItems[0].Text : "", "", "",
                                    guardDistanceBox.Text };

            worldController.ApplyTaskChange(taskSelect, taskData);
        }
    }
}
