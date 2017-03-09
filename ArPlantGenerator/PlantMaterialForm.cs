using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AccuRender;

namespace ArPlantGenerator
{
  public partial class PlantMaterialForm : Form
  {
    public PlantMaterialForm()
    {
      InitializeComponent();
    }


    public Material m_Material;
    public Scene m_Scene;
    public Rectangle m_CallingRect;
    public Control m_Control;

    private bool m_bIgnore;


    public event EventHandler TextureChanged;
    public void OnTextureChanged()
    {
      if (TextureChanged != null)
        TextureChanged(m_Control, new EventArgs());
    }


    public int Init(Material m, Scene s)
    {
      m_Material = m;

      m_bIgnore = true;

      MaterialList.Items.Clear();
      MaterialList.Items.Add("<New...>");
      MaterialList.Items.Add("<None>");

      foreach (var kvb in m.m_Scene.Materials)
        MaterialList.Items.Add(kvb.Value.Name);

      MaterialList.SelectedIndex = MaterialList.FindString(m.Name);

      m_bIgnore = false;

      return 1;
    }


    private void timer1_Tick(object sender, EventArgs e)
    {
      if (Visible && !MaterialList.DroppedDown && !ClientRectangle.Contains(PointToClient(MousePosition)) && !RectangleToClient(m_CallingRect).Contains(PointToClient(MousePosition)))
        Hide();
    }

    private void MaterialList_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (MaterialList.SelectedIndex == 0)
      {

        return;
      }

      Material m;
      if (m_Material.m_Scene.Materials.TryGetValue(MaterialList.SelectedItem.ToString(), out m))
      {
        OnTextureChanged();

      }
    }
  }
}
