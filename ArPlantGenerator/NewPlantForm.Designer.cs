namespace ArPlantGenerator
{
  partial class NewPlantForm
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
      this.label42 = new System.Windows.Forms.Label();
      this.label41 = new System.Windows.Forms.Label();
      this.plantBotanical = new System.Windows.Forms.TextBox();
      this.plantName = new System.Windows.Forms.TextBox();
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label42
      // 
      this.label42.AutoSize = true;
      this.label42.Location = new System.Drawing.Point(12, 54);
      this.label42.Name = "label42";
      this.label42.Size = new System.Drawing.Size(82, 13);
      this.label42.TabIndex = 4;
      this.label42.Text = "Botanical Name";
      // 
      // label41
      // 
      this.label41.AutoSize = true;
      this.label41.Location = new System.Drawing.Point(12, 9);
      this.label41.Name = "label41";
      this.label41.Size = new System.Drawing.Size(35, 13);
      this.label41.TabIndex = 5;
      this.label41.Text = "Name";
      // 
      // plantBotanical
      // 
      this.plantBotanical.Location = new System.Drawing.Point(11, 73);
      this.plantBotanical.Name = "plantBotanical";
      this.plantBotanical.Size = new System.Drawing.Size(398, 20);
      this.plantBotanical.TabIndex = 2;
      // 
      // plantName
      // 
      this.plantName.Location = new System.Drawing.Point(11, 28);
      this.plantName.Name = "plantName";
      this.plantName.Size = new System.Drawing.Size(398, 20);
      this.plantName.TabIndex = 3;
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Location = new System.Drawing.Point(141, 111);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(111, 23);
      this.button1.TabIndex = 6;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // NewPlantForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(421, 146);
      this.ControlBox = false;
      this.Controls.Add(this.button1);
      this.Controls.Add(this.label42);
      this.Controls.Add(this.label41);
      this.Controls.Add(this.plantBotanical);
      this.Controls.Add(this.plantName);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NewPlantForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "New Plant";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label42;
    private System.Windows.Forms.Label label41;
    public System.Windows.Forms.TextBox plantBotanical;
    public System.Windows.Forms.TextBox plantName;
    private System.Windows.Forms.Button button1;
  }
}