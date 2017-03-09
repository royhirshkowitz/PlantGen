namespace ArPlantGenerator
{
  partial class JointEditor
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
      this.listView1 = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.jointRadius = new System.Windows.Forms.NumericUpDown();
      ((System.ComponentModel.ISupportInitialize)(this.jointRadius)).BeginInit();
      this.SuspendLayout();
      // 
      // listView1
      // 
      this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.listView1.FullRowSelect = true;
      this.listView1.GridLines = true;
      this.listView1.HideSelection = false;
      this.listView1.Location = new System.Drawing.Point(3, 2);
      this.listView1.Name = "listView1";
      this.listView1.Size = new System.Drawing.Size(147, 319);
      this.listView1.TabIndex = 0;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = System.Windows.Forms.View.Details;
      this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Joint";
      this.columnHeader1.Width = 50;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Radius";
      // 
      // jointRadius
      // 
      this.jointRadius.DecimalPlaces = 2;
      this.jointRadius.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.jointRadius.Location = new System.Drawing.Point(56, 233);
      this.jointRadius.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.jointRadius.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.jointRadius.Name = "jointRadius";
      this.jointRadius.Size = new System.Drawing.Size(58, 20);
      this.jointRadius.TabIndex = 1;
      this.jointRadius.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.jointRadius.Visible = false;
      this.jointRadius.ValueChanged += new System.EventHandler(this.jointRadius_ValueChanged);
      // 
      // JointEditor
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(152, 366);
      this.Controls.Add(this.jointRadius);
      this.Controls.Add(this.listView1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "JointEditor";
      this.Text = "Joint Editor";
      ((System.ComponentModel.ISupportInitialize)(this.jointRadius)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listView1;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.NumericUpDown jointRadius;
  }
}