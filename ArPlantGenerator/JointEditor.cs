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
  public partial class JointEditor : Form
  {
    public JointEditor()
    {
      InitializeComponent();

      listView1.Controls.Add(jointRadius);
    }

    BranchDef m_BranchDef;

    public void Init(BranchDef b)
    {
      m_BranchDef = b;

      listView1.Items.Clear();

      int i = 0;
      foreach (var j in m_BranchDef.joints)
      {
        var item = listView1.Items.Add(i.ToString());
        item.Tag = j;
        item.SubItems.Add(j.r.ToString("F2"));
        i++;
      }

    }

    public event EventHandler JointChanged;
    private void OnJointChanged()
    {
      if (JointChanged != null)
        JointChanged(this, new EventArgs());
    }

    bool m_bIgnore;

    private void listView1_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button != System.Windows.Forms.MouseButtons.Left)
        return;

      jointRadius.Visible = false;

      ListViewHitTestInfo hit = listView1.HitTest(e.Location);
      if (hit.Item != null)
      {
        if (hit.Item.SubItems[1] == hit.SubItem)
        {
          jointRadius.Bounds = hit.SubItem.Bounds;
          m_bIgnore = true;
          jointRadius.Value = Math.Max(jointRadius.Minimum, Math.Min(jointRadius.Maximum, Convert.ToDecimal(hit.SubItem.Text)));
          m_bIgnore = false;
          jointRadius.Visible = true;
          jointRadius.Focus();
        }

      }
    }

    private void jointRadius_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      var item = listView1.SelectedItems[0];
      if (item != null)
      {
        var j = item.Tag as JointDef;
        if (j != null)
        {
          j.r = (float)jointRadius.Value;
          item.SubItems[1].Text = j.r.ToString("F2");
          OnJointChanged();
        }
      }
    }
  }
}
