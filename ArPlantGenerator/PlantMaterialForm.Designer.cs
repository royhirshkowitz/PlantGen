namespace ArPlantGenerator
{
  partial class PlantMaterialForm
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
      this.components = new System.ComponentModel.Container();
      this.MaterialList = new System.Windows.Forms.ComboBox();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // MaterialList
      // 
      this.MaterialList.FormattingEnabled = true;
      this.MaterialList.Items.AddRange(new object[] {
            "<None>",
            "<New...>"});
      this.MaterialList.Location = new System.Drawing.Point(12, 12);
      this.MaterialList.Name = "MaterialList";
      this.MaterialList.Size = new System.Drawing.Size(197, 21);
      this.MaterialList.TabIndex = 0;
      this.MaterialList.SelectedIndexChanged += new System.EventHandler(this.MaterialList_SelectedIndexChanged);
      // 
      // timer1
      // 
      this.timer1.Enabled = true;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // PlantMaterialForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(221, 329);
      this.Controls.Add(this.MaterialList);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "PlantMaterialForm";
      this.Text = "PlantMaterialForm";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox MaterialList;
    private System.Windows.Forms.Timer timer1;
  }
}