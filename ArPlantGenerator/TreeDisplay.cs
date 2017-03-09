using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AccuRender;
using ArControls;



namespace ArPlantGenerator
{
  public partial class TreeDisplay : Display
  {
    public TreeDisplay()
    {
      InitializeComponent();

    }

    public event EventHandler Draw;

    protected override void OnPaint(PaintEventArgs e)
    {
      if (Draw != null)
        Draw(this, EventArgs.Empty);




    //if (m_Renderer == null || !m_Renderer.IsRendering)
    //  base.OnPaint(e);
    //else
    //{
    //  try
    //  {
    //    e.Graphics.DrawImage(new Bitmap(m_DisplayImage), this.ClientRectangle);
    //  }

    //  catch { }
    //}
  }

    






  }
}
