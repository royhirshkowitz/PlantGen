namespace ArPlantGenerator
{
  partial class MaterialEditorDB
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MaterialEditorDB));
      this.materialEditorControl1 = new ArControls.MaterialEditorControl();
      this.button1 = new System.Windows.Forms.Button();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // materialEditorControl1
      // 
      this.materialEditorControl1.IsDecal = false;
      this.materialEditorControl1.Location = new System.Drawing.Point(4, 33);
      this.materialEditorControl1.Name = "materialEditorControl1";
      this.materialEditorControl1.Size = new System.Drawing.Size(331, 378);
      this.materialEditorControl1.TabIndex = 0;
      this.materialEditorControl1.Units = "Meters";
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Location = new System.Drawing.Point(252, 414);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Close";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // toolStrip1
      // 
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(339, 25);
      this.toolStrip1.TabIndex = 2;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // toolStripButton1
      // 
      this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
      this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButton1.Name = "toolStripButton1";
      this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
      this.toolStripButton1.Text = "toolStripButton1";
      // 
      // MaterialEditorDB
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(339, 444);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.materialEditorControl1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MaterialEditorDB";
      this.Text = "Material Editor";
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton toolStripButton1;
    public ArControls.MaterialEditorControl materialEditorControl1;
  }
}