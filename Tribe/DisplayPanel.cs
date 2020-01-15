using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tribe
{

    public class SwapEventArgs : EventArgs
    {
        public readonly IGetData DataObject;

        public SwapEventArgs(IGetData dataObject)
        {
            DataObject = dataObject;
        }
    }

    public delegate void SwapEventHandler(object o, SwapEventArgs e);

    public class DisplayPanel
    {

        public event EventHandler Close;
        public event SwapEventHandler Swap;
        public event MouseEventHandler MouseDown; // Catch the MouseDown event from the Panel and wrap it in a new event containing the DisplayPanel.

        public Panel Panel { get; private set; }
        public IGetData DataObject { get; private set; }
        public List<Label> Fields { get; private set; }
        public List<Label> Values { get; private set; }
        public bool Indexable { get; private set; }
        public int IndexPosition { get; private set; }

        private Button exitButton;
        private Button swapButton;
        private Label markLabel;
        private readonly bool displaySwapButton;
        private Point point;
        private readonly int parentWidth;
        private readonly int parentHeight;
        private readonly Bitmap exitImage;

        private readonly int panelPad = 2;
        private readonly int leftPad = 10;
        private readonly int rightPad = 2;
        private readonly int middlePad = 10;
        private readonly int topPad = 22;
        private readonly int heightPad = 22;

        public DisplayPanel(IGetData dataObject, Bitmap b, bool canSwap, Point point, int parentWidth, int parentHeight)
        {

            Panel = new Panel()
            {
                Visible = false,
                BackColor = Color.FromArgb(100, 235, 228, 200),
            }; // 255, 248, 220

            Panel.MouseDown += Panel_MouseDown;

            DataObject = dataObject;
            displaySwapButton = canSwap;
            this.parentWidth = parentWidth;
            this.parentHeight = parentHeight;
            this.point = point;
            exitImage = b;
            IndexPosition = DataObject.GetItemIndex();
            Indexable = IndexPosition != -1;

            exitButton = new Button() { Image = b };
            exitButton.Click += ExitButton_Click;
            markLabel = new Label()
            {
                Size = new Size(10, 10),
                Text = "*",
                BackColor = Color.Transparent,
                Visible = Indexable
            };
            Panel.Controls.Add(markLabel);

            DataObject.UpdateElement += DataObject_UpdateElement;
            DataObject.CancelData += DataObject_CancelData;

            DrawPanel();

        }

        // Catch the MouseDown event from the Panel and wrap it in a new event containing the DisplayPanel.
        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            MouseEventHandler handler = MouseDown;
            handler?.Invoke(this, e);
        }

        public void Activate()
        {
            Panel.BorderStyle = BorderStyle.Fixed3D;
        }

        public void Deactivate()
        {
            Panel.BorderStyle = BorderStyle.None;
        }

        public void SetIndex(int index)
        {
            IndexPosition = index;

            if (Indexable)
            {
                if (DataObject.GetData().FixedCount == Fields.Count)
                {
                    markLabel.Visible = false;
                }
                else
                {
                    markLabel.Location = new Point(0, (IndexPosition + DataObject.GetData().FixedCount) * heightPad);
                    markLabel.Visible = true;
                }
            }
        }

        private void DrawPanel()
        {

            Fields = new List<Label>();
            Values = new List<Label>();

            // Height determined by length of data. Width determined by width of longest row. Position determned by point, width, and height.
            Panel.Size = new Size(50, (DataObject.GetData().DataList.Count + 1) * heightPad + topPad + topPad);

            int maxWidth = int.MinValue; // Determine width of the panel.

            // Create field and value labels, populate them, and add them to the displayPanel in the correct positions.
            int i; // Create i in scope just outside the loop to use it for the swap button.
            for (i = 0; i < DataObject.GetData().DataList.Count; i++)
            {
                var pair = DataObject.GetData().DataList[i];

                // Fields. Positioned to the left.
                Label fieldLabel = new Label() { Text = pair.Item1, Location = new Point(leftPad, i * heightPad), Width = 75, BackColor = Color.Transparent };

                // Add a marker to the field to let the player know which item is selected.
                if (Indexable && i == IndexPosition + DataObject.GetData().FixedCount)
                {
                    markLabel.Location = new Point(0, i * heightPad);
                    markLabel.Visible = true;
                }

                Panel.Controls.Add(fieldLabel);
                Fields.Add(fieldLabel);

                // Values. Positioned to the right. X position determined by the X position and width of the field.
                int valueLabelX = Fields[i].Location.X + Fields[i].Width + middlePad;

                Label valueLabel = new Label()
                {
                    Text = pair.Item2.ToString(),
                    Location = new Point(valueLabelX, i * heightPad),
                    BackColor = Color.Transparent,
                    AutoSize = true
                };

                Values.Add(valueLabel);
                Panel.Controls.Add(valueLabel);

                if (valueLabelX + valueLabel.Width > maxWidth)
                    maxWidth = valueLabelX + valueLabel.Width;
            }

            maxWidth += 50;

            Panel.Size = new Size(maxWidth + rightPad, Panel.Height);

            int xPos, yPos;

            if (point.X + panelPad + Panel.Width + 10 > parentWidth) // Panel to the left of point.
                xPos = point.X - panelPad - 10 - Panel.Width;
            else // Panel to the right of point.
                xPos = point.X + panelPad + 10;

            if (point.Y + Panel.Height + panelPad > parentHeight) // Panel shifted up a bit to stay within the window.
                yPos = parentHeight - Panel.Height + panelPad;
            else // Panel at the same y as point.
                yPos = point.Y;

            // Display exit button.
            exitButton = new Button()
            {
                Size = new Size(20, 20),
                Location = new Point(Panel.Width - 22, 2),
                BackColor = Color.Transparent,
                Image = new Bitmap(exitImage, new Size(20, 20))
            };

            exitButton.Click += ExitButton_Click;
            Panel.Controls.Add(exitButton);

            // Display swap button.
            if (displaySwapButton)
            {
                swapButton = new Button()
                {
                    Size = new Size(70, 20),
                    Location = new Point(leftPad, i * heightPad),
                    Text = "Swap"
                };
                swapButton.Click += SwapButton_Click;
                Panel.Controls.Add(swapButton);
            }

            Panel.Location = new Point(xPos, yPos);

            Panel.Visible = true;
        }

        private void AddRow(string name, object value)
        {
            Panel.Size = new Size(Panel.Width, Panel.Height + Fields[0].Height);

            // TODO: Make this update the width of the panel too...

            // Shift the panel if necessary to keep all elements visible.
            int xPos, yPos;

            if (point.X + panelPad + Panel.Width + 10 > parentWidth) // Panel to the left of point.
                xPos = point.X - panelPad - 10 - Panel.Width;
            else // Panel to the right of point.
                xPos = point.X + panelPad + 10;

            if (point.Y + Panel.Height + panelPad > parentHeight) // Panel shifted up a bit to stay within the window.
                yPos = parentHeight - Panel.Height + panelPad;
            else // Panel at the same y as point.
                yPos = point.Y;

            Label fieldLabel = new Label() { Text = name, Location = new Point(leftPad, Fields.Count * heightPad), Width = 75, BackColor = Color.Transparent };

            int valueLabelX = fieldLabel.Location.X + fieldLabel.Width + middlePad;
            Label valueLabel = new Label()
            {
                Text = value.ToString(),
                Location = new Point(valueLabelX, Values.Count * heightPad),
                BackColor = Color.Transparent,
                AutoSize = true
            };

            Fields.Add(fieldLabel);
            Panel.Controls.Add(fieldLabel);
            Values.Add(valueLabel);
            Panel.Controls.Add(valueLabel);

            if (swapButton != null)
                swapButton.Location = new Point(leftPad, Fields.Count * heightPad); // Move the swap button to make room for the new row.
        }

        // Remove the row of fields and values at the specified index and cascade update the rest upwards.
        private void RemoveRow(int i)
        {

            if (i == Fields.Count - 1)
                markLabel.Visible = false;

            Panel.Controls.Remove(Fields[i]);
            Panel.Controls.Remove(Values[i]);
            Fields.RemoveAt(i);
            Values.RemoveAt(i);

            for (int j = i; j < Fields.Count; j++)
            {
                Fields[j].Location = new Point(Fields[j].Location.X, Fields[j].Location.Y - heightPad);
                Values[j].Location = new Point(Values[j].Location.X, Values[j].Location.Y - heightPad);
            }
        }

        private void DataObject_UpdateElement(object sender, DataChangedArgs e)
        {
            int i;
            switch (e.Type)
            {
                case ChangeType.NewElement:
                    //Console.WriteLine("DataObject_UpdateElement.NewElement. Adding {0} with value {1}", e.ElementName, e.Value.ToString());

                    // Increase size of panel to allow a new row. If the panel exceeds the size of the window, redraw the DisplayPanel.
                    AddRow(e.ElementName, e.Value);
                    break;
                case ChangeType.UpdateElement:
                    //Console.WriteLine("DataObject_UpdateElement.UpdateElement. Changing {0} to {1}", e.ElementName, e.Value.ToString());

                    // Find the correct value label and change its value. Also allows for a marked field "*FieldName" to be selected.
                    i = Fields.FindIndex(field => field.Text.Equals(e.ElementName) || field.Text.Remove(0).Equals(e.ElementName));

                    if (i == -1)
                        Console.WriteLine($"Error in DataObject_UpdateElement. Could not find element {e.ElementName} in Fields");
                    else
                        Values[i].Text = (e.Value == null ? "Null" : e.Value.ToString());

                    break;
                case ChangeType.RemoveElement:
                    //Console.WriteLine("DataObject_UpdateElement.RemoveElement. Removing {0}", e.ElementName);

                    // Remove a row from the list. Maybe trigger a redraw? Not as important as adding a new element.

                    i = Fields.FindIndex(field => field.Text.Equals(e.ElementName));
                    RemoveRow(i);
                    break;
            }
        }

        // TODO: Wire this up with a new event in DisplayPanel to an event handler in Form1...
        private void SwapButton_Click(object sender, EventArgs e)
        {
            SwapEventHandler handler = Swap;
            handler?.Invoke(this, new SwapEventArgs(DataObject));
        }

        private void End()
        {
            EventHandler handler = Close;
            Close?.Invoke(this, new EventArgs());

            if (DataObject != null)
                DataObject.UpdateElement -= DataObject_UpdateElement;
            Panel.MouseDown -= Panel_MouseDown;
            Panel.Visible = false;
            Panel.Controls.Clear();
            exitButton = null;

            Fields = null;
            Values = null;
            DataObject = null;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            End();
        }

        private void DataObject_CancelData(object sender, EventArgs e)
        {
            End();
        }
    }
}
