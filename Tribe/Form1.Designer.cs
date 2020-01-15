namespace Tribe
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Goat");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Hog");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Wolf");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Bear");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Food");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Stick");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Rock");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Fiber");
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("Leaf");
            this.timeLabel = new System.Windows.Forms.Label();
            this.dayNightBox = new System.Windows.Forms.PictureBox();
            this.myPanel = new Tribe.MyPanel();
            this.taskPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.guardTask = new System.Windows.Forms.CheckBox();
            this.sleepTask = new System.Windows.Forms.CheckBox();
            this.mateTask = new System.Windows.Forms.CheckBox();
            this.huntTask = new System.Windows.Forms.CheckBox();
            this.pickupTask = new System.Windows.Forms.CheckBox();
            this.guardDistanceBox = new System.Windows.Forms.TextBox();
            this.huntTypeList = new System.Windows.Forms.ListView();
            this.pickupTypeList = new System.Windows.Forms.ListView();
            this.applyTaskButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dayNightBox)).BeginInit();
            this.myPanel.SuspendLayout();
            this.taskPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // timeLabel
            // 
            this.timeLabel.AutoSize = true;
            this.timeLabel.Location = new System.Drawing.Point(0, 1);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(0, 17);
            this.timeLabel.TabIndex = 1;
            // 
            // dayNightBox
            // 
            this.dayNightBox.Location = new System.Drawing.Point(112, 0);
            this.dayNightBox.Name = "dayNightBox";
            this.dayNightBox.Size = new System.Drawing.Size(34, 30);
            this.dayNightBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.dayNightBox.TabIndex = 2;
            this.dayNightBox.TabStop = false;
            // 
            // myPanel
            // 
            this.myPanel.BackColor = System.Drawing.SystemColors.Window;
            this.myPanel.Controls.Add(this.taskPanel);
            this.myPanel.Location = new System.Drawing.Point(1, 36);
            this.myPanel.Name = "myPanel";
            this.myPanel.Size = new System.Drawing.Size(1129, 767);
            this.myPanel.TabIndex = 6;
            // 
            // taskPanel
            // 
            this.taskPanel.BackColor = System.Drawing.Color.Transparent;
            this.taskPanel.Controls.Add(this.applyTaskButton);
            this.taskPanel.Controls.Add(this.label1);
            this.taskPanel.Controls.Add(this.guardTask);
            this.taskPanel.Controls.Add(this.sleepTask);
            this.taskPanel.Controls.Add(this.mateTask);
            this.taskPanel.Controls.Add(this.huntTask);
            this.taskPanel.Controls.Add(this.pickupTask);
            this.taskPanel.Controls.Add(this.guardDistanceBox);
            this.taskPanel.Controls.Add(this.huntTypeList);
            this.taskPanel.Controls.Add(this.pickupTypeList);
            this.taskPanel.Location = new System.Drawing.Point(306, 51);
            this.taskPanel.Name = "taskPanel";
            this.taskPanel.Size = new System.Drawing.Size(369, 337);
            this.taskPanel.TabIndex = 7;
            this.taskPanel.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(82, 264);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 17);
            this.label1.TabIndex = 14;
            this.label1.Text = "Distance:";
            // 
            // guardTask
            // 
            this.guardTask.AutoSize = true;
            this.guardTask.Location = new System.Drawing.Point(3, 263);
            this.guardTask.Name = "guardTask";
            this.guardTask.Size = new System.Drawing.Size(70, 21);
            this.guardTask.TabIndex = 13;
            this.guardTask.Text = "Guard";
            this.guardTask.UseVisualStyleBackColor = true;
            // 
            // sleepTask
            // 
            this.sleepTask.AutoSize = true;
            this.sleepTask.Location = new System.Drawing.Point(3, 236);
            this.sleepTask.Name = "sleepTask";
            this.sleepTask.Size = new System.Drawing.Size(66, 21);
            this.sleepTask.TabIndex = 12;
            this.sleepTask.Text = "Sleep";
            this.sleepTask.UseVisualStyleBackColor = true;
            // 
            // mateTask
            // 
            this.mateTask.AutoSize = true;
            this.mateTask.Location = new System.Drawing.Point(3, 209);
            this.mateTask.Name = "mateTask";
            this.mateTask.Size = new System.Drawing.Size(61, 21);
            this.mateTask.TabIndex = 11;
            this.mateTask.Text = "Mate";
            this.mateTask.UseVisualStyleBackColor = true;
            // 
            // huntTask
            // 
            this.huntTask.AutoSize = true;
            this.huntTask.Location = new System.Drawing.Point(3, 103);
            this.huntTask.Name = "huntTask";
            this.huntTask.Size = new System.Drawing.Size(60, 21);
            this.huntTask.TabIndex = 10;
            this.huntTask.Text = "Hunt";
            this.huntTask.UseVisualStyleBackColor = true;
            // 
            // pickupTask
            // 
            this.pickupTask.AutoSize = true;
            this.pickupTask.Location = new System.Drawing.Point(3, 3);
            this.pickupTask.Name = "pickupTask";
            this.pickupTask.Size = new System.Drawing.Size(72, 21);
            this.pickupTask.TabIndex = 9;
            this.pickupTask.Text = "Pickup";
            this.pickupTask.UseVisualStyleBackColor = true;
            // 
            // guardDistanceBox
            // 
            this.guardDistanceBox.Location = new System.Drawing.Point(155, 263);
            this.guardDistanceBox.Name = "guardDistanceBox";
            this.guardDistanceBox.Size = new System.Drawing.Size(85, 22);
            this.guardDistanceBox.TabIndex = 8;
            // 
            // huntTypeList
            // 
            this.huntTypeList.CheckBoxes = true;
            this.huntTypeList.HideSelection = false;
            listViewItem1.StateImageIndex = 0;
            listViewItem2.StateImageIndex = 0;
            listViewItem3.StateImageIndex = 0;
            listViewItem4.StateImageIndex = 0;
            this.huntTypeList.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4});
            this.huntTypeList.Location = new System.Drawing.Point(119, 103);
            this.huntTypeList.MultiSelect = false;
            this.huntTypeList.Name = "huntTypeList";
            this.huntTypeList.Scrollable = false;
            this.huntTypeList.Size = new System.Drawing.Size(246, 97);
            this.huntTypeList.TabIndex = 4;
            this.huntTypeList.UseCompatibleStateImageBehavior = false;
            // 
            // pickupTypeList
            // 
            this.pickupTypeList.CheckBoxes = true;
            this.pickupTypeList.HideSelection = false;
            listViewItem5.StateImageIndex = 0;
            listViewItem6.StateImageIndex = 0;
            listViewItem7.StateImageIndex = 0;
            listViewItem8.StateImageIndex = 0;
            listViewItem9.StateImageIndex = 0;
            this.pickupTypeList.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9});
            this.pickupTypeList.Location = new System.Drawing.Point(119, 0);
            this.pickupTypeList.MultiSelect = false;
            this.pickupTypeList.Name = "pickupTypeList";
            this.pickupTypeList.Scrollable = false;
            this.pickupTypeList.Size = new System.Drawing.Size(246, 97);
            this.pickupTypeList.TabIndex = 1;
            this.pickupTypeList.UseCompatibleStateImageBehavior = false;
            // 
            // applyTaskButton
            // 
            this.applyTaskButton.Location = new System.Drawing.Point(290, 304);
            this.applyTaskButton.Name = "applyTaskButton";
            this.applyTaskButton.Size = new System.Drawing.Size(75, 30);
            this.applyTaskButton.TabIndex = 15;
            this.applyTaskButton.Text = "Apply";
            this.applyTaskButton.UseVisualStyleBackColor = true;
            this.applyTaskButton.Click += new System.EventHandler(this.ApplyTaskButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1132, 803);
            this.Controls.Add(this.myPanel);
            this.Controls.Add(this.dayNightBox);
            this.Controls.Add(this.timeLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dayNightBox)).EndInit();
            this.myPanel.ResumeLayout(false);
            this.taskPanel.ResumeLayout(false);
            this.taskPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.PictureBox dayNightBox;
        private MyPanel myPanel;
        private System.Windows.Forms.Panel taskPanel;
        private System.Windows.Forms.TextBox guardDistanceBox;
        private System.Windows.Forms.ListView huntTypeList;
        private System.Windows.Forms.ListView pickupTypeList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox guardTask;
        private System.Windows.Forms.CheckBox sleepTask;
        private System.Windows.Forms.CheckBox mateTask;
        private System.Windows.Forms.CheckBox huntTask;
        private System.Windows.Forms.CheckBox pickupTask;
        private System.Windows.Forms.Button applyTaskButton;
    }
}

