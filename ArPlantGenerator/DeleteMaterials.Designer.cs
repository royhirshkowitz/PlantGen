﻿namespace ArPlantGenerator
{
  partial class DeleteMaterials
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
      this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // checkedListBox1
      // 
      this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.checkedListBox1.FormattingEnabled = true;
      this.checkedListBox1.Location = new System.Drawing.Point(4, 2);
      this.checkedListBox1.Name = "checkedListBox1";
      this.checkedListBox1.Size = new System.Drawing.Size(275, 259);
      this.checkedListBox1.TabIndex = 0;
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Location = new System.Drawing.Point(4, 267);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(108, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Delete";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // DeleteMaterials
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 296);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.checkedListBox1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DeleteMaterials";
      this.Text = "Delete Materials";
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.CheckedListBox checkedListBox1;
    private System.Windows.Forms.Button button1;
  }
}