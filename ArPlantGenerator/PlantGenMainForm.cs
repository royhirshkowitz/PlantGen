using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using AccuRender;
using ArControls;

using ArPlantGenerator.Properties;



namespace ArPlantGenerator
{
    #region Constructors / Load / Closing / Sizing
    public partial class PlantGenMainForm : Form
    {
      public PlantGenMainForm()
      {
        EnableControls = new _EnableControls(EnableControlsMethod);
        m_PassComplete = new PassComplete(PassCompleteMethod);
  
        m_Hand = new Cursor(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Cursors\\Hand.cur");
        m_Rotate = new Cursor(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Cursors\\Rotate.cur");
        InitializeComponent();
      }

      TreeDisplay m_DisplayControl;
      private OpenGL m_OpenGL = new OpenGL();
      private Cursor m_Hand, m_Rotate;
      private Scene m_Scene;
      private Plant m_Plant;

      bool m_bIgnore;

      private void Form1_Load(object sender, EventArgs e)
      {
        m_nFileMenuItems = toolStripMenuItem1.DropDownItems.Count;
        GetMRUFiles();

        m_OpenGL.m_BackgroundColor = new Vec3(0.5f, 0.7f, 0.9f);

        m_DisplayControl = new TreeDisplay();
        m_DisplayControl.Draw += this.Display_Draw;
        m_DisplayControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Display_MouseMove);
        m_DisplayControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Display_MouseWheel);
        m_DisplayControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Display_MouseUp);
        m_DisplayControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Display_MouseDown);
        m_DisplayControl.AllowDrop = true;
        m_DisplayControl.Location = button1.Location;
        m_DisplayControl.Size = button1.Size; 
        m_DisplayControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        splitContainer1.Panel1.Controls.Add(m_DisplayControl);
        tabControl1.TabPages.Remove(leavesPage);
        tabControl1.TabPages.Remove(plantPage);

        ArControls.Common.SetFormInitialSizeAndLocation(this, Settings.Default.WindowLocation, Settings.Default.WindowSize);

      m_LibraryFolder = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\AccuRender\AccuRender Studio\1.0", "Library", false) as String;
      if (m_LibraryFolder == null)
        m_LibraryFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +Path.DirectorySeparatorChar + "AccuRender Libraries" + Path.DirectorySeparatorChar;
      m_LibraryFolder += "Plants" + Path.DirectorySeparatorChar;
      m_LeafTextureFolder = m_LibraryFolder + "Leaves" + Path.DirectorySeparatorChar;
      m_BarkTextureFolder = m_LibraryFolder + "Bark" + Path.DirectorySeparatorChar;

      AddTrunk.Image = imageList2.Images[0];
      AddBranch.Image = imageList2.Images[1];
      AddLeaf.Image = imageList2.Images[2];
      Prune.Image = imageList2.Images[3];
    }

    private void SaveFileToMRU(string fileName)
    {

      if (!m_MRUFiles.Contains(fileName))
      {
        if (m_MRUFiles.Count >= 4)
          m_MRUFiles.Dequeue();
        m_MRUFiles.Enqueue(fileName);
        UpdateRecentlyUsedMenu();
      }

    }



  private void UpdateRecentlyUsedMenu()
    {
      for (int j = toolStripMenuItem1.DropDownItems.Count - 1; j >= m_nFileMenuItems; j--)
        toolStripMenuItem1.DropDownItems.RemoveAt(j);
      string[] _S = m_MRUFiles.ToArray();
      for (int i = 0; i < m_MRUFiles.Count; i++)
      {
        int j = m_MRUFiles.Count - 1 - i;
        var f = toolStripMenuItem1.DropDownItems.Add(Path.GetFileName(_S[j]));
        f.Name = _S[j];
        f.Click += RecentFile_Click;
      }

    }



    private void GetMRUFiles()
    {
      m_MRUFiles.Clear();
      if (Settings.Default.MRUFileList != null)
      {
        string[] _S = Settings.Default.MRUFileList.Split(';');
        for (int i = 0; i < Math.Min(4, _S.Length); i++)
        {
          if (_S[i] != "")
            m_MRUFiles.Enqueue(_S[i]);
        }
      }

      UpdateRecentlyUsedMenu();
    }


    private void StoreMRUFiles()
    {
      string[] _S = m_MRUFiles.ToArray();
      Settings.Default.MRUFileList = "";
      for (int i = 0; i < Math.Min(4, _S.Length); i++)
      {
        if (i != 0)
          Settings.Default.MRUFileList += ";";
        Settings.Default.MRUFileList += _S[i];
      }

    }


    

    private Size m_MaterialEditorSize = new Size(0, 0);
    private Point m_MaterialEditorLocation = new Point(-1, -1);

    private Queue<string> m_MRUFiles = new Queue<string>();
    private int m_nFileMenuItems;


    bool SceneChangedWarning()
    {
      if ((!m_bChanged) || (m_Scene == null))
        return true;

      DialogResult r;

      if ((m_PlantFileName == null) || (m_PlantFileName == ""))
      {
        r = System.Windows.Forms.MessageBox.Show(this, "Save Changes to " + m_Plant.def.name + "?", "File Changed Warning", MessageBoxButtons.YesNoCancel);
        if (r == DialogResult.Yes)
          SavePlantAs_Click(this, new EventArgs());
      }

      else
      {
        r = System.Windows.Forms.MessageBox.Show(this, "Save Changes to " + Path.GetFileNameWithoutExtension(m_PlantFileName) + "?", "File Changed Warning", MessageBoxButtons.YesNoCancel);
        if (r == DialogResult.Yes)
          m_Scene.Write(m_PlantFileName);
      }

      return r != DialogResult.Cancel;
    }


    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
      {
        if (!SceneChangedWarning())
        {
          e.Cancel = true;
          return;
        }

        if (m_Renderer != null)
          StopRendering();

        Settings.Default.MaterialEditorLocation = m_MaterialEditorLocation;
        
        //Settings.Default.TextureOpenFolder = m_MaterialEditorControl.m_DefaultTextureFolder;

        Settings.Default.WindowLocation = this.Location;
        if (this.WindowState == FormWindowState.Normal)
          Settings.Default.WindowSize = this.Size;
        else
          Settings.Default.WindowSize = this.RestoreBounds.Size;

        
        StoreMRUFiles();

        Settings.Default.Save();
      }


    private void button1_Resize(object sender, EventArgs e)
    {
      if (m_DisplayControl != null)
      {
        m_DisplayControl.Size = button1.Size;
        if (m_Scene != null)
          Regen();
      }
    }



    #endregion 

    #region Initialization

    BranchDefBase currentSelection;
    BranchDef currentBranch
    {
      get { return currentSelection as BranchDef; }
      set { currentSelection = value; }
    }

    LeafDef currentLeaf
    {
      get { return currentSelection as LeafDef; }
      set { currentSelection = value; }
    }



    void ApplyThumbnail(Button c, Material m)
    {
      if (c == null) return;
      var sm = m as StandardMaterial;
      //if ((sm != null) && (sm.Color.MaterialMap != null))
      //  c.BackgroundImage = sm.Color.MaterialMap.Texture.Map.ToBitmap();
      //else
        c.BackgroundImage = m.GetThumbnail(c.Width);
    }



    void InitControls()
    {
      m_bIgnore = true;

      if (currentBranch != null)
      {
        if (tabControl1.TabPages[0] != branchesPage)
        {
          tabControl1.TabPages.Remove(leavesPage);
          tabControl1.TabPages.Remove(plantPage);
          tabControl1.TabPages.Add(branchesPage);
        }

        branchesPage.Text = currentBranch.name;
        tabControl1.SelectedTab = branchesPage;

        if (currentBranch.frontMaterial != null)
        {
          Material m;
          if (m_Scene.Materials.TryGetValue(currentBranch.frontMaterial, out m))
            ApplyThumbnail(branchFrontMaterial, m);
        }

        UI.SetUpDown(Wiggle, currentBranch.crookedness);
        UI.SetUpDown(Bend, currentBranch.bending);
        Horizontal.Checked = currentBranch.bHorizontal;
        UI.SetUpDown(Radius, currentBranch.radiusRatio);
        UI.SetUpDown(RadiusTop, currentBranch.radiusRatioTop);
        UI.SetUpDown(RadiusPlusMinus, currentBranch.radiusPlusMinus);
        UI.SetUpDown(Elongation, currentBranch.elongation);
        UI.SetUpDown(ElongationPlusMinus, currentBranch.elongationPlusMinus);
        UI.SetUpDown(Taper, currentBranch.taper);
        UI.SetUpDown(BranchesPerNode, currentBranch.nBranchesPerNode);
        UI.SetUpDown(Nodes, currentBranch.nNodes);
        UI.SetUpDown(Joints, currentBranch.joints.Count);
        UI.SetUpDown(Tesselation, currentBranch.tesselation);
        UI.SetUpDown(BranchStart, currentBranch.branchMin);
        UI.SetUpDown(BranchStop, currentBranch.branchMax);
        UI.SetUpDown(VerticalStart, Utils.Degrees(currentBranch.branchVertMin));
        UI.SetUpDown(VerticalStop, Utils.Degrees(currentBranch.branchVertMax));
        UI.SetUpDown(HorizontalStart, Utils.Degrees(currentBranch.branchHorzInitial));
        UI.SetUpDown(HorizontalStep, Utils.Degrees(currentBranch.branchHorzStep));
        UI.SetUpDown(HorzStepPlusMinus, Utils.Degrees(currentBranch.branchHorzStepPlusMinus));
        UI.SetUpDown(VertPlusMinus, Utils.Degrees(currentBranch.branchVertPlusMinus));
        UI.SetUpDown(BranchOffset, currentBranch.branchOffset);
        FlatWiggle.Checked = currentBranch.flatWiggle;
        BranchEndCap.Checked = currentBranch.bEndCap;
        UI.SetUpDown(branchProbability, currentBranch.branchProbability);
        UI.SetUpDown(BranchHorizontalTiles, currentBranch.horzTiles);
        UI.SetUpDown(BranchVScale, currentBranch.vScale);
        OneTile.Checked = currentBranch.bOneVPerSegment;
        BranchVScale.Enabled = !OneTile.Checked;


        bool bTrunk = currentBranch.parent == null;
        Nodes.Enabled = !bTrunk;
        BranchStart.Enabled = !bTrunk;
        BranchStop.Enabled = !bTrunk;
        VerticalStop.Enabled = !bTrunk;
        HorizontalStep.Enabled = !bTrunk;
        HorzStartPlusMinus.Enabled = !bTrunk;
        RadiusTop.Enabled = !bTrunk;


      }

      else if (currentLeaf != null)
      {
        if (tabControl1.TabPages[0] != leavesPage)
        {
          tabControl1.TabPages.Remove(branchesPage);
          tabControl1.TabPages.Remove(plantPage);
          tabControl1.TabPages.Add(leavesPage);
        }

        tabControl1.SelectedTab = leavesPage;
        leavesPage.Text = currentLeaf.name;

        if (currentLeaf.frontMaterial != null)
        {
          Material m;
          if (m_Scene.Materials.TryGetValue(currentLeaf.frontMaterial, out m))
          {
            var sm = m as StandardMaterial;
            if ((sm != null) && (sm.Color.MaterialMap != null))
              leafFrontMaterial.BackgroundImage = sm.Color.MaterialMap.Texture.Map.ToBitmap();
            else
              leafFrontMaterial.BackgroundImage = m.GetThumbnail();
          }
        }

        UI.SetUpDown(LeavesPerNode, currentLeaf.nBranchesPerNode);
        UI.SetUpDown(NodesPerMeter, currentLeaf.NodesPerMeter);
        UI.SetUpDown(MinNodesPerBranch, currentLeaf.minNodesPerBranch);
        UI.SetUpDown(LeafStart, currentLeaf.branchMin);
        UI.SetUpDown(LeafStop, currentLeaf.branchMax);
        UI.SetUpDown(LeafOffset, currentLeaf.branchOffset);
        UI.SetUpDown(LeafVerticalStart, Utils.Degrees(currentLeaf.branchVertMin));
        UI.SetUpDown(LeafVerticalStop, Utils.Degrees(currentLeaf.branchVertMax));
        UI.SetUpDown(LeafHorizontalStart, Utils.Degrees(currentLeaf.branchHorzInitial));
        UI.SetUpDown(LeafHorizontalStep, Utils.Degrees(currentLeaf.branchHorzStep));
        UI.SetUpDown(LeafVertPlusMinus, Utils.Degrees(currentLeaf.branchVertPlusMinus));
        UI.SetUpDown(LeafHorzStartPlusMinus, Utils.Degrees(currentLeaf.branchHorzInitialPlusMinus));
        UI.SetUpDown(LeafHorzStepPlusMinus, Utils.Degrees(currentLeaf.branchHorzStepPlusMinus));
        UI.SetUpDown(LeafWidth, currentLeaf.Width);
        UI.SetUpDown(LeafLength, currentLeaf.Length);
        UI.SetUpDown(LeafSizePlusMinus, currentLeaf.scalePlusMinus);
        UI.SetUpDown(LeafLengthSegs, currentLeaf.nLength);
        UI.SetUpDown(LeafWidthSegs, currentLeaf.nWidth);
        UI.SetUpDown(LeafBend, currentLeaf.bending);
        UI.SetUpDown(LeafBendX, currentLeaf.xBend);
        LeafHorizontal.Checked = currentLeaf.bHorizontal;
        UI.SetUpDown(LeafOrigin, currentLeaf.OriginLength);
        UI.SetUpDown(RotateZ, Utils.Degrees(currentLeaf.rotateZ));
        UI.SetUpDown(RotateZPlusMinus, Utils.Degrees(currentLeaf.rotateZPlusMinus));
        UI.SetUpDown(LeafCrookedness, currentLeaf.crookedness);
        UI.SetUpDown(LeafProbability, currentLeaf.branchProbability);
      }

      else
      {
        if (tabControl1.TabPages[0] != plantPage)
        {
          tabControl1.TabPages.Remove(branchesPage);
          tabControl1.TabPages.Add(plantPage);
          tabControl1.TabPages.Remove(leavesPage);
        }


        plantName.Text = m_Plant.def.name;
        plantBotanical.Text = m_Plant.def.botanical;
        UI.SetUpDown(MaxBranchOrder, m_Plant.def.maxBranchOrder);

      }

      m_bIgnore = false;

    }

    void AddTreeViewItem(BranchDefBase b, TreeNodeCollection parent)
    {
      while (b != null)
      {
        var t = new TreeNode(b.name);
        t.Tag = b;
        t.Checked = b.visible;
        parent.Add(t);
        if (b.child != null)
        {
          AddTreeViewItem(b.child, t.Nodes);
        }

        b = b.sib;
      }


    }

    void InitBranchTreeControl()
    {
      m_bIgnore = true;

      treeView1.Nodes.Clear();
      var n = treeView1.Nodes.Add(m_Plant.def.name);
      n.Tag = m_Plant.def;
      n.Checked = true;
      AddTreeViewItem(m_Plant.def.branches, treeView1.Nodes[0].Nodes);
      treeView1.ExpandAll();


      currentBranch = m_Plant.def.branches;

      m_bIgnore = false;

      treeView1.SelectedNode = treeView1.Nodes[0].Nodes[0];

    }

    #endregion

    #region File IO


    String m_PlantFileName = "";
    String m_LibraryFolder;


    private void SavePlant_Click(object sender, EventArgs e)
    {
      try
      {
        if ((m_PlantFileName == null) || (m_PlantFileName == ""))
          SavePlantAs_Click(sender, e);
        else
        {
          m_Scene.Thumbnail = m_DisplayControl.GetThumbnail();
          m_Scene.Write(m_PlantFileName, WhatBits.Materials | WhatBits.Cameras | WhatBits.PlantDefs);
          m_bChanged = false;
        }
      }

      catch { }
    }


    private void SavePlantAs_Click(object sender, EventArgs e)
    {
      try
      {
        if (saveFileDialog2.ShowDialog(this) == DialogResult.OK)
        {
          m_Scene.Thumbnail = m_DisplayControl.GetThumbnail();
          m_Scene.Write(saveFileDialog2.FileName, WhatBits.Materials | WhatBits.Cameras | WhatBits.PlantDefs);
          m_PlantFileName = saveFileDialog2.FileName;
          Text = m_PlantFileName;
          m_bChanged = false;
          SaveFileToMRU(saveFileDialog2.FileName);
        }
      }

      catch { }
    }


    private void BeginFileIO()
    {
      StopRendering();

    }


    private void EndFileIO(bool bSuccess)
    {
      if (bSuccess)
      {

        InitBranchTreeControl();

        m_bIgnore = true;
        directSunToolStripMenuItem.Checked = m_Scene.lighting.SunChannelOn;
        m_bIgnore = false;

        m_OpenGL.m_bTexturesRequired = 1;
        Regen();
        m_bChanged = false;
        StartRendering();
      }

      SavePlant.Enabled = bSuccess;
      SavePlantAs.Enabled = bSuccess;
      saveSceneToolStripMenuItem.Enabled = bSuccess;
      SavePreviewRendering.Enabled = bSuccess;

      viewToolStripMenuItem.Enabled = bSuccess;
      toolStrip1.Enabled = bSuccess;
      treeView1.Enabled = bSuccess;
      tabControl1.Enabled = bSuccess;

    }


    //string templateFolder;

    private void NewPlant_Click(object sender, EventArgs e)
    {
      if (!SceneChangedWarning())
        return;

      var templateFolder = m_LibraryFolder + "Templates";

      openFileDialog2.InitialDirectory = templateFolder;
      openFileDialog2.Title = "Select Template File";

      if (openFileDialog2.ShowDialog(this) == DialogResult.OK)
      {
        BeginFileIO();

        m_Scene = new Scene();
        m_Scene.sun.Date = m_Scene.sun.Date.AddHours(2.0);
        bool bSuccess = m_Scene.Read(openFileDialog2.FileName) > 0;

        if (bSuccess)
        {
          m_PlantFileName = "";

          m_Plant = new Plant(m_Scene);

          foreach (var d in m_Scene.PlantDefs)
          {
            m_Plant.def = d.Value;
            break;
          }


          var np = new NewPlantForm();
          np.plantName.Text = "My New " + Path.GetFileNameWithoutExtension(openFileDialog2.FileName) + " Plant";
          np.plantName.SelectAll();


          if (np.ShowDialog(this) == DialogResult.OK)
          {
            m_Plant.def.name = np.plantName.Text;
            m_Plant.def.botanical = np.plantBotanical.Text;
          }
        }

        this.Text = m_Plant.def.name;

        var temp = new Dictionary<string, Material>();
        foreach (var m in m_Scene.Materials)
          temp.Add(m.Key, m.Value);

        foreach (var m in temp)
          ChangeMaterialName(m.Key, m_Plant.def.name + "-" + m.Key);
        
        
        EndFileIO(bSuccess);
        m_bChanged = true;
       
      }


    }

    



    void LoadPlant(string FullPath)
    {
      BeginFileIO();

      m_Scene = new Scene();
      m_Scene.sun.Date = m_Scene.sun.Date.AddHours(2.0);
      bool bSuccess = m_Scene.Read(FullPath) > 0;

      if (bSuccess)
      {
        m_PlantFileName = FullPath;
        this.Text = Path.GetFileNameWithoutExtension(m_PlantFileName);

        m_Plant = new Plant(m_Scene);

        foreach (var d in m_Scene.PlantDefs)
        {
          m_Plant.def = d.Value;
          break;
       }

        SaveFileToMRU(m_PlantFileName);
      }

      
      EndFileIO(bSuccess);

    }
        
    private void OpenPlant_Click(object sender, EventArgs e)
    {
      if (!SceneChangedWarning())
        return;


      if ((Settings.Default.OpenPlantFolder == null) || (Settings.Default.OpenPlantFolder == ""))
      {
        Settings.Default.OpenPlantFolder = m_LibraryFolder + "Plants";
      }

      openFileDialog2.InitialDirectory = Settings.Default.OpenPlantFolder;

      if (openFileDialog2.ShowDialog(this) == DialogResult.OK)
      {
        LoadPlant(openFileDialog2.FileName);
        Settings.Default.OpenPlantFolder = Path.GetDirectoryName(openFileDialog2.FileName);
      }
  
    }

    private void RecentFile_Click(object sender, EventArgs e)
    {
      if (!SceneChangedWarning())
        return;


      var i = sender as ToolStripItem;
      if (i != null)
      {
        m_Scene = new Scene();
        LoadPlant(i.Name);
      }

    }


    String ExportFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    Export exportDB;

    private void exportSceneToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (exportDB == null)
      {
        exportDB = new ArControls.Export();
        exportDB.Init(true);
      }


      exportDB.Folder.Text = ExportFolder;
      if (exportDB.Format.SelectedIndex == -1)
        exportDB.Format.SelectedIndex = 0;
      exportDB.FileName.Text = (m_PlantFileName == "") ? m_Plant.def.name : Path.GetFileNameWithoutExtension(m_PlantFileName);
           

      if (exportDB.ShowDialog(this) == DialogResult.OK)
      {
        ExportFolder = exportDB.Folder.Text;
        var eo = exportDB.Format.SelectedItem as ExportComboBoxItem;
        if (eo == null) return;
        var t = eo.Type;
        String ext, prefix;
        if (t == EType.e3dm) { ext = ".3dm"; prefix = "3DM- "; }
        else if (t == EType.eObj) { ext = ".obj"; prefix = "OBJ- "; }
        else if (t == EType.eFBX) { ext = ".fbx"; prefix = "FBX- "; }
        else  { ext = ".ArScene"; prefix = "ARSCENE- "; }

        var folder = ExportFolder + Path.DirectorySeparatorChar + prefix + exportDB.FileName.Text;

        try
        {
          Directory.CreateDirectory(folder);
        }

        catch { return; }

        var filename = folder + Path.DirectorySeparatorChar + exportDB.FileName.Text + ext;

        if (File.Exists(filename))
          if (MessageBox.Show(this, Path.GetFileName(filename) + " exists. Overwrite?", "File Overwrite Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;


        // regen the scene without the groundplane
        if (bGroundPlane)
        {
          m_Scene.Meshes.Clear();
          var g = m_Plant.Generate();
          int i = 0;
          foreach (var m in g.meshes)
          {
            m.Name = i++.ToString();
            m_Scene.Meshes.Add(m.Name, m);
          }
        }


        if (t == EType.e3dm)
        {
          var rw = new IO_3DM();
          rw.Write(m_Scene, filename);
        }
        else if (t == EType.eObj)
        {
          var obj = new IO_OBJ();
          obj.Write(m_Scene, filename);
        }
        else if (t == EType.eFBX)
        {
          var fbx = new IO_FBX();
          fbx.Write(m_Scene, filename);
        }
        else m_Scene.Write(filename);

        if (bGroundPlane)
          Regen();
      }
      
    }

    private void SavePreviewRendering_Click(object sender, EventArgs e)
    {
      //if (Settings.Default.SaveImageFolder != "")
      //  saveFileDialog3.InitialDirectory = Settings.Default.SaveImageFolder;
      //else if (Settings.Default.FileOpenFolder != "")
      //  saveFileDialog3.InitialDirectory = Settings.Default.FileOpenFolder;

      if (saveFileDialog3.FileName != "")
        saveFileDialog3.FileName = Path.GetFileNameWithoutExtension(saveFileDialog3.FileName);


      if (saveFileDialog3.ShowDialog() == DialogResult.OK)
      {
        PauseRendering();
        if (saveFileDialog3.FilterIndex == 3)
        {
          Application.UseWaitCursor = true;
          m_Renderer.WriteNativeImage(saveFileDialog3.FileName);
          Application.UseWaitCursor = false;
        }

        else

        {
          Application.UseWaitCursor = true;
          var b = new Bitmap(m_Renderer.Width, m_Renderer.Height);
          m_Renderer.GetImage(ref b, saveFileDialog3.FilterIndex == 2);
          b.Save(saveFileDialog3.FileName, System.Drawing.Imaging.ImageFormat.Png);
          Application.UseWaitCursor = false;
        }

        //Settings.Default.SaveImageFolder = Path.GetDirectoryName(saveFileDialog3.FileName);
        ResumeRendering();

      }


    }


    private void PlantGenMainForm_Shown(object sender, EventArgs e)
    {
      var args = Environment.GetCommandLineArgs();
      if (args.Length > 1)
      {
        m_SceneToLoad = args[1];
        timer2.Enabled = true;
      }
    }


    private void timer2_Tick(object sender, EventArgs e)
    {
      LoadPlant(m_SceneToLoad);
      timer2.Enabled = false;
    }



    #endregion

    #region Rendering

    private Renderer m_Renderer;
    private volatile int m_ChangeRequestTime;
    private volatile int m_LastSelectionTime;
    private volatile bool m_ChangeRequest;
    private bool m_Stop, m_Pause, m_Paused;
    private int m_Start;
    private bool m_LongSilence = true;
    private string m_SceneToLoad;


    private long m_Elapsed;

    private void ErrorMessage(string msg)
    {
      //statusErrorMsg.Text = msg;
      statusStrip1.Refresh();
    }

    // invokable methods called by other threads

    void EnableControlsMethod(bool bRendering, bool bLoading)
    {
      //tabControl1.Enabled = !bLoading;
      //splitContainer2.Panel2.Enabled = bRendering;
      if (m_MaterialEditor != null)
        m_MaterialEditor.Enabled = !bLoading;
      Application.UseWaitCursor = bLoading;
    }

    public delegate void _EnableControls(bool bRendering, bool bLoading);
    public _EnableControls EnableControls;
    private bool m_bInvalidateRendering;


    void PassCompleteMethod(Image i, int iPass)
    {


      if (!m_bInvalidateRendering && (Math.Abs(Environment.TickCount - m_LastSelectionTime) > 5000))
      {
        var oldImage = m_DisplayControl.BackgroundImage;
        m_DisplayControl.BackgroundImage = new Bitmap(i);
        m_DisplayControl.BackgroundImageLayout = ImageLayout.None;
        m_DisplayControl.Refresh();
        if (oldImage != null)
          oldImage.Dispose();
        //GC.Collect();
      }
      Pass.Text = "Pass: " + iPass.ToString() + "  ";
      int seconds = (int)(m_Elapsed / 1000);
      int hours = seconds / 3600;
      int minutes = seconds / 60 % 60;
      seconds = seconds % 60;
      Elapsed.Text = hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
      double fps = m_Elapsed > 0 ? (double)iPass * 1000.0 / (double)(m_Elapsed) : 0.0;
      FPS.Text = "FPS:  " + fps.ToString("F3") + "  ";
      RPS.Text = "kRPS:  " + (m_Elapsed > 0 ? (m_Renderer.RayCount / m_Elapsed).ToString() : "0") + "  ";




    }
    public delegate void PassComplete(Image i, int iPass);
    public PassComplete m_PassComplete;
    private volatile bool m_ChannelsChanged;

    private int m_StartTicks;
    private Bitmap m_DisplayImage;

    // render event processing-- always called by other threads

    private void ArStudio_RenderEvent(object sender, AccuRender.RenderEventArgs e)
    {
      var r = sender as AccuRender.Renderer;
      switch (e.Action)
      {
        case RenderAction.BeginLoading:
          this.Invoke(EnableControls, new object[2] { true, true });
          break;

        case RenderAction.EndLoading:
          this.Invoke(EnableControls, new object[2] { true, false });
          break;

        case RenderAction.StartRendering:
          m_Start = Environment.TickCount;
          m_StartTicks = Environment.TickCount;
          m_bInvalidateRendering = false;
          m_Elapsed = 0;
          break;

        case RenderAction.Stop:
          break;

        case RenderAction.Abort:
          goto case RenderAction.Stop;


        case RenderAction.PassComplete:

          if (m_Stop)
          {
            e.Result = 0;
            e.TerminationCode = -3;
          }

          if ((m_DisplayImage == null) || (m_DisplayImage.Width != r.Width) || (m_DisplayImage.Height != r.Height))
          {
            m_DisplayImage = new Bitmap(r.Width, r.Height);

          }

          {
            int timeSpan = Environment.TickCount - m_Start;
            int delay = e.Pass < 10 ? 1000 : e.Pass < 20 ? 2000 : 3000;
            if (!m_ChangeRequest && ((e.Pass == 1) || m_Pause || (e.Result == 0) || (timeSpan > delay) || m_ChannelsChanged))
            {
              if (m_ChannelsChanged)
              {
                m_ChannelsChanged = false;
                //SetChannelsAndExposure();
              }

              m_Elapsed += timeSpan;
              m_Start = Environment.TickCount;

              try
              {
                if (r.GetImage(ref m_DisplayImage, false) > 0)
                  this.Invoke(m_PassComplete, new Object[] { m_DisplayImage, e.Pass });
              }

              catch { }
            }
          }

          if (m_Pause && (e.Result != 0))
          {
            m_Paused = true;
            while (true)
            {
              Application.DoEvents();
              System.Threading.Thread.Sleep(1);
              if (!m_Pause)
              {
                m_Paused = false;
                m_Start = Environment.TickCount;
                break;
              }
            }
          }

          break;

        default:
          break;
      }



    }

    private void OnSettingsChanged()
    {
      m_ChangeRequest = true;
      m_ChangeRequestTime = Environment.TickCount;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      if (m_Scene != null)
        SavePlant.Enabled = m_bChanged;
    
      if (m_ChangeRequest && ((Environment.TickCount - m_ChangeRequestTime) > 1000) && !m_LongSilence)
      {
        int w, h;
        SetDisplaySize(out w, out h);
        //m_Renderer.NumCores = (int)numCores.Value;
        if (m_Renderer.Restart(w, h) < 0)
        {
          MessageBox.Show(this, "Shutting Down", "This version of AccuRender Plant Generator has expired", MessageBoxButtons.OK);
          Close();
        }

        m_OpenGL.Redraw(m_DisplayControl, m_Scene, m_Selected);
        m_ChangeRequest = false;
      }


      if (m_ChangeRequest)
        m_LongSilence = false;
      else if ((Environment.TickCount - m_ChangeRequestTime) > 5000)
        m_LongSilence = true;


    }

    private bool StartRendering()
    {
      var s = m_Scene;
      m_DisplayControl.m_Renderer = null;
      m_DisplayControl.BackgroundImage = null;
      if (m_Renderer != null)
        m_Renderer.Dispose();
      m_Renderer = new Renderer();
      m_Renderer.RenderEvent += ArStudio_RenderEvent;
      m_DisplayControl.m_Renderer = m_Renderer;
      //m_Renderer.NumCores = (int)numCores.Value;


      m_Renderer.m_Scene = s;
      m_Stop = false;
      m_Pause = false;
      int w, h;
      SetDisplaySize(out w, out h);
      if (m_Renderer.Start(w, h) < 0)
      {
        MessageBox.Show(this, "This version of AccuRender Plant Generator has expired", "Shutting Down", MessageBoxButtons.OK);
        Close();
        return false;
      }

      return true;
    }


    private void StopRendering()
    {
      if (m_Renderer == null)
        return;

      m_Stop = true;
      m_Pause = false;
     
      while (m_Renderer.IsRendering)
      {
        Application.DoEvents();
        System.Threading.Thread.Sleep(1);
      }

    }

    // waits for pause
    private void PauseRendering()
    {
      if (m_Renderer == null)
        return;


      m_Pause = true;
      while (!m_Paused)
      {
        Application.DoEvents();
        System.Threading.Thread.Sleep(100);
      }
    }

    private void ResumeRendering()
    {
      m_Pause = false;
    }

    private void SetDisplaySize()
    {
      int w, h;
      SetDisplaySize(out w, out h);
    }


    private void SetDisplaySize(out int w, out int h)
    {
      w = m_DisplayControl.Width;
      h = m_DisplayControl.Height;
    }

    private bool m_bSizeChanged;

    private void Form1_ResizeBegin(object sender, EventArgs e)
    {
      if (m_Scene != null)
        m_bSizeChanged = false;
    }

    private void Form1_SizeChanged(object sender, EventArgs e)
    {
      if (m_Scene != null)
        m_bSizeChanged = true;
    }


    private void Form1_ResizeEnd(object sender, EventArgs e)
    {
      if (m_bSizeChanged && (m_Scene != null))
      {
        SetDisplaySize();
        m_OpenGL.Regen(m_DisplayControl, m_Scene, m_Selected);
        OnSettingsChanged();
      }
    }


    

    #endregion

    #region OpenGL

    void Display_Draw(object sender, EventArgs e)
    {
      if ((m_Renderer == null || !m_Renderer.IsRendering) && m_Scene != null)
        DrawGL();

    }
    

    private void Display_MouseWheel(object sender, MouseEventArgs e)
    {
      //if (m_Loading)
      //  return;

      if (m_Scene == null)
        return;

      Camera c = m_Scene.GetCurrentCamera();
      if (c == null) return;

      // lens zoom
      if (Control.ModifierKeys == Keys.Shift)
        c.Fov -= Math.Sign(e.Delta) * (float)Math.PI / 180f;
      // look up or down
      else if (Control.ModifierKeys == Keys.Control)
      {
        float Phi = Math.Sign(e.Delta) * (float)Math.PI / 180f * 0.25f;
        Vec3 _V = c.Target - c.Eye;
        Vec3 _X = _V.Normalize();
        _X = Vec3.Cross(_X, c.Up);
        Matrix _M = Matrix.Rotation(_X, Phi);
        _V = Vec3.TransformVec(_M, _V);
        c.Up = Vec3.TransformVec(_M, c.Up);
        c.Target = c.Eye + _V;
      }
      // dolly in or out
      else
      {
        Vec3 _D = c.Target - c.Eye;
        float t = _D.Length();
        if (t == 0f) return;
        var _V = _D / t;
        t = (e.Delta > 0) ? t * 0.9f : t / 0.9f;
        t = Math.Max(0.01f, t);
        c.Eye = c.Target - _V * t;
      }

      m_bChanged = true;
      DrawGL();
      //OnSettingsChanged();


    }

    private void DrawGL(bool bInvalidateRendering = false)
    {
      //if (bInvalidateRendering)
      //{
      //  m_bInvalidateRendering = true;
      //  m_DisplayControl.BackgroundImage = null;
      //}
      if (m_Scene != null)
      {
        m_OpenGL.Redraw(m_DisplayControl, m_Scene, m_Selected);
        if (m_Renderer != null)
        {
          m_ChangeRequest = true;
          m_ChangeRequestTime = Environment.TickCount;
        }
      }
    }

    

    private int m_MouseX, m_MouseY;

    private void Display_MouseMove(object sender, MouseEventArgs e)
    {

      if (m_Scene == null)
        return;

      if (m_DisplayControl.Capture && e.Button == System.Windows.Forms.MouseButtons.Right && Control.ModifierKeys != Keys.Control)
      {
        Camera c = m_Scene.GetCurrentCamera();
        if (c == null) return;
        int deltaX = m_MouseX - e.X;
        int deltaY = m_MouseY - e.Y;

        if (Control.ModifierKeys == Keys.Shift)
        // Pan (Elevator)
        {
          Vec3 _V = c.Target - c.Eye;
          float t = _V.Length();
          if (Math.Abs(deltaX) >= Math.Abs(deltaY))
          {
            float fx = 2f * (float)Math.Tan(0.5f * c.Fov) * (float)deltaX / (float)m_DisplayControl.Width * Math.Max(1f, t);
            _V /= t;
            Vec3 _X = Vec3.Cross(_V, c.Up) * fx;
            c.Eye += _X;
            c.Target += _X;

          }
          else
          {
            float fy = 2f * (float)Math.Tan(0.5f * c.Fov) * (float)deltaY / (float)m_DisplayControl.Width * Math.Max(1f, t);
            Vec3 _Y = c.Up * -fy;
            c.Eye += _Y;
            c.Target += _Y;
          }

        }

        else
        // orbit
        {

          if (Math.Abs(deltaX) >= Math.Abs(deltaY))
          {
            float Theta = deltaX * (float)Math.PI / 180f * 0.1f;
            Matrix _M = Matrix.Rotation(new Vec3(0f, 0f, 1f), Theta);
            Vec3 _V = c.Target - c.Eye;
            _V = Vec3.TransformVec(_M, _V);
            c.Up = Vec3.TransformVec(_M, c.Up);
            c.Eye = c.Target - _V;
          }
          else
          {
            float Phi = deltaY * (float)Math.PI / 180f * 0.1f;
            Vec3 _V = c.Target - c.Eye;
            Vec3 _X = _V.Normalize();
            _X = Vec3.Cross(_X, c.Up);
            Matrix _M = Matrix.Rotation(_X, Phi);
            _V = Vec3.TransformVec(_M, _V);
            c.Up = Vec3.TransformVec(_M, c.Up);
            c.Eye = c.Target - _V;
          }
        }

        m_MouseX = e.X;
        m_MouseY = e.Y;
        m_bChanged = true;
        DrawGL();
        OnSettingsChanged();
        return;
      }

    }

    private void Display_MouseDown(object sender, MouseEventArgs e)
    {
 
      if (m_Scene == null)
        return;

 
      m_DisplayControl.Capture = true;

      m_MouseX = e.X;
      m_MouseY = e.Y;

      if (e.Button == System.Windows.Forms.MouseButtons.Right)
      {
        if (Control.ModifierKeys == Keys.Shift)
          Cursor = m_Hand;
        else
          Cursor = m_Rotate;
      }
    }




    private List<Object> m_Selected = new List<Object>();

    private TreeNode FromTag(object itemId, TreeNode rootNode)
    {
      foreach (TreeNode node in rootNode.Nodes)
      {
        if (node.Tag.Equals(itemId)) return node;
        TreeNode next = FromTag(itemId, node);
        if (next != null) return next;
      }
      return null;
    }

    private TreeNode FindTagInTree(object tag)
    {
      TreeNode node = null;

      foreach (TreeNode n in treeView1.Nodes)
      {
        node = FromTag(tag, n);
        if (node != null)
          break;
      }

      return node;
    }



    private void Display_MouseUp(object sender, MouseEventArgs e)
    {
      
      if (m_Scene == null)
        return;

      Cursor = Cursors.Default;

      if (m_DisplayControl.Capture && e.Button == System.Windows.Forms.MouseButtons.Left)
      {
        m_Selected.Clear();
        m_OpenGL.Select(m_Scene, e.X, m_DisplayControl.Height - e.Y - 1, 1, 1, m_Selected);

        if (m_Selected.Count > 0)
        {
          var m = m_Selected[0] as Mesh;
          if (m != null)
          {
            var node = FindTagInTree(m.Tag);
            if (node != null)
              treeView1.SelectedNode = node;
          }
        }

        m_OpenGL.m_bRegenRequired = 1;
        DrawGL();
        m_LastSelectionTime = Environment.TickCount;
      }

      m_DisplayControl.Capture = false;

    }

    #endregion

    #region  Generate Plant
    private void Regen()
    {
      m_bChanged = true;

      m_Scene.Meshes.Clear();

      if (bGroundPlane)

      {
        var gp = new Mesh(m_Scene);
        gp.Name = "GroundPlane";

        var up = new Vec3(0, 0, 1);

        gp.m_Points.Add(new Vec3(-100, -100, 0));
        gp.m_Normals.Add(up);
        gp.m_Points.Add(new Vec3(100, -100, 0));
        gp.m_Normals.Add(up);
        gp.m_Points.Add(new Vec3(100, 100, 0));
        gp.m_Normals.Add(up);
        gp.m_Points.Add(new Vec3(-100, 100, 0));
        gp.m_Normals.Add(up);

        gp.m_Faces.Add(new IVec3(0, 1, 2));
        gp.m_Faces.Add(new IVec3(0, 2, 3));

        m_Scene.Meshes.Add(gp.Name, gp);
      }


      var g = m_Plant.Generate();
      int i = 0;
      int nFaces = 0;

      foreach (var m in g.meshes)
      {
        m.Name = i++.ToString();
        m_Scene.Meshes.Add(m.Name, m);
        nFaces += m.m_Faces.Count;
      }

      m_ChangeRequest = true;
      m_ChangeRequestTime = Environment.TickCount;

      
      m_OpenGL.Regen(m_DisplayControl, m_Scene, m_Selected);
      HeightLabel.Text = m_Scene.ExtentsMax[2].ToString("F2");
      FacesLabel.Text = nFaces.ToString();
    }

    #endregion

    #region PlantControlHandlers

      private void MaxBranchOrder_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        m_Plant.def.maxBranchOrder = (int)MaxBranchOrder.Value;
      }

      private void plantName_TextChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        m_Plant.def.name = plantName.Text;
        treeView1.Nodes[0].Text = m_Plant.def.name;
      }

      private void plantBotanical_TextChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        m_Plant.def.botanical = plantName.Text;
      }


    #endregion

    #region Common (Leaf/Branch) Control Handlers

    bool m_bChanged;
      private void Wiggle_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.crookedness = (float)c.Value;
        Regen();
      }

      private void Bend_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.bending = (float)c.Value;
        Regen();
      }

      private void Horizontal_CheckedChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as CheckBox;
        currentSelection.bHorizontal = c.Checked;
        Regen();
      }

      private void BranchesPerNode_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.nBranchesPerNode = (int)c.Value;
        Regen();
      }


      private void BranchStart_ValueChanged(object sender, EventArgs e)
      {

        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchMin = (float)c.Value;
        if (currentSelection.branchMin > currentSelection.branchMax)
        {
          currentSelection.branchMax = currentSelection.branchMin;
          m_bIgnore = true;
          if (c == BranchStart)
            BranchStop.Value = BranchStart.Value;
          else
            LeafStop.Value = LeafStart.Value;
          m_bIgnore = false;
        }

        Regen();
      }

      private void BranchStop_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchMax = (float)c.Value;
        if (currentSelection.branchMax < currentSelection.branchMin)
        {
          currentSelection.branchMin = currentSelection.branchMax;
          m_bIgnore = true;
          if (c == BranchStop)
            BranchStart.Value = BranchStop.Value;
          else
            LeafStart.Value = LeafStop.Value;
          m_bIgnore = false;
        }
      Regen();
      }

      private void VerticalStart_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchVertMin = (float)c.Value * (float)Math.PI / 180;
        Regen();
      }

      private void VerticalStop_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchVertMax = (float)c.Value * (float)Math.PI / 180;
        Regen();
      }

      private void HorizontalStart_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchHorzInitial = (float)c.Value * (float)Math.PI / 180;
        Regen();
      }

      private void HorizontalStep_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchHorzStep = (float)c.Value * (float)Math.PI / 180;
        Regen();
      }

      private void HorzStartPlusMinus_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchHorzInitialPlusMinus = Utils.Radians((float)c.Value);
        Regen();
      }

      private void HorzStepPlusMinus_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchHorzStepPlusMinus = Utils.Radians((float)c.Value);
        Regen();
      }

      private void VertPlusMinus_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchVertPlusMinus = Utils.Radians((float)c.Value);
        Regen();
      }

      private void Offset_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var c = sender as NumericUpDown;
        currentSelection.branchOffset = (float)c.Value;
        Regen();
      }

    private void branchProbability_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      var c = sender as NumericUpDown;
      currentSelection.branchProbability = (float)c.Value;
      Regen();
    }


    #endregion

    #region Branch Control Handlers
    private void Radius_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.radiusRatio = (float)Radius.Value;
        Regen();
      }

      private void RadiusTop_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.radiusRatioTop = (float)RadiusTop.Value;
        Regen();
      }

      private void Elongation_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.elongation = (float)Elongation.Value;
        Regen();
      }

      private void Taper_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.taper = (float)Taper.Value;
        Regen();
      }

  
      private void Nodes_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.nNodes = (int)Nodes.Value;
        Regen();
      }

      private void Joints_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        var joints = currentBranch.joints;
        int n = (int)Joints.Value - joints.Count;
        if (n == 0) return;
        if (n < 0)
          joints.RemoveRange(joints.Count + n, -n);
        else
          for (int i = 0; i < n; i++)
            joints.Add(new JointDef());
              
        Regen();
      }

      private void Tesselation_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.tesselation = (int)Tesselation.Value;
        Regen();
      }

      private void ElongationPlusMinus_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.elongationPlusMinus = (float)ElongationPlusMinus.Value;
        Regen();
      }

      private void RadiusPlusMinus_ValueChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.radiusPlusMinus = (float)RadiusPlusMinus.Value;
        Regen();
      }

      private void FlatWiggle_CheckedChanged(object sender, EventArgs e)
      {
        if (m_bIgnore) return;
        currentBranch.flatWiggle = FlatWiggle.Checked;
        Regen();
      }


    private void BranchEndCap_CheckedChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentBranch.bEndCap = BranchEndCap.Checked;
      Regen();
    }

    private void BranchHorizontalTiles_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentBranch.horzTiles = (int)BranchHorizontalTiles.Value;
      Regen();
    }

    private void BranchVScale_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentBranch.vScale = (float)BranchVScale.Value;
      Regen();
    }

    private void OneTile_CheckedChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentBranch.bOneVPerSegment = OneTile.Checked;
      BranchVScale.Enabled = !OneTile.Checked;
      Regen();
    }




    private void editJoints_Click(object sender, EventArgs e)
    {
      var je = new JointEditor();
      je.JointChanged += JointEditor_JointChanged;
      je.Init(currentBranch);
      je.ShowDialog(this);
    }

    private void JointEditor_JointChanged(object sender, EventArgs e)
    {
      Regen();
    }


    #endregion

    #region Leaf Control Handlers

    private void NodesPerMeter_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.NodesPerMeter = (float)NodesPerMeter.Value;
      Regen();
    }

    private void MinNodesPerBranch_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.minNodesPerBranch = (int)MinNodesPerBranch.Value;
      Regen();
    }

    private void LeafWidth_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.Width = (float)LeafWidth.Value;
      Regen();
    }

    private void LeafLength_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.Length = (float)LeafLength.Value;
      Regen();
    }


    private void LeafSizePlusMinus_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.scalePlusMinus = (float)LeafSizePlusMinus.Value;
      Regen();
    }


    private void LeafLengthSegs_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.nLength = (int)LeafLengthSegs.Value;
      Regen();
    }

    private void LeafWidthSegs_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.nWidth = (int)LeafWidthSegs.Value;
      Regen();
    }


    private void LeafBendX_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.xBend = (float)LeafBendX.Value;
      Regen();
    }

    private void LeafOrigin_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.OriginLength = (float)LeafOrigin.Value;
      Regen();
    }

    private void RotateZ_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.rotateZ = Utils.Radians((float)RotateZ.Value);
      Regen();
    }

    private void RotateZPlusMinus_ValueChanged(object sender, EventArgs e)
    {
      if (m_bIgnore) return;
      currentLeaf.rotateZPlusMinus = Utils.Radians((float)RotateZPlusMinus.Value);
      Regen();
    }

    #endregion

    #region Tree Control Handlers

    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (m_bIgnore) return;

      if (e.Node.Tag is PlantDef)
      {
        currentSelection = null;
        AddLeaf.Enabled = false;
        AddBranch.Enabled = false;
        Prune.Enabled = false;

      }

      else

      {
        currentSelection = (BranchDefBase)e.Node.Tag;
        AddLeaf.Enabled = !(currentSelection is LeafDef);
        Prune.Enabled = !((currentSelection is BranchDef) && (currentBranch.parent == null) && (m_Plant.def.branches.sib == null));
        var b = currentSelection as BranchDef;
        AddBranch.Enabled = (b != null && b.Order() < m_Plant.def.maxBranchOrder);
      }

            
      InitControls();
    }

    private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
    {
      if (m_bIgnore) return;
      var b = e.Node.Tag as BranchDefBase;
      if (b != null)
        b.visible = e.Node.Checked;
      else
        m_Plant.def.visible = e.Node.Checked;
      treeView1.Refresh();
      Regen();
      
    }

    private void Prune_Click(object sender, EventArgs e)
    {
      if (currentSelection.parent != null)
        BranchDefBase.RemoveChildFromParent(currentSelection);
      else
        m_Plant.def.branches = (BranchDef)BranchDefBase.RemoveSiblingTrunk(currentSelection, m_Plant.def.branches);

      treeView1.SelectedNode.Remove();

      treeView1.SelectedNode = treeView1.Nodes[0];
      Regen();
    }

    private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
      if (e.Button == MouseButtons.Right) treeView1.SelectedNode = e.Node;
    }

    private void AddLeaf_Click(object sender, EventArgs e)
    {
      m_bIgnore = true;
      var b = LeafDef.Leaf((BranchDef)treeView1.SelectedNode.Tag, m_Scene);
      b.frontMaterial = "Leaf";
      BranchDefBase.AddChild(b);
      var n = treeView1.SelectedNode.Nodes.Add(b.name);
      n.Tag = b;
      n.Checked = true;
      treeView1.SelectedNode.Expand();
      Regen();
      m_bIgnore = false;

      treeView1.SelectedNode = n;
    }

    private void AddBranch_Click(object sender, EventArgs e)
    {
      m_bIgnore = true;
      var b = BranchDef.Branch((BranchDef)treeView1.SelectedNode.Tag, m_Scene);
      b.frontMaterial = "Bark";
      BranchDefBase.AddChild(b);
      var n = treeView1.SelectedNode.Nodes.Add(b.name);
      n.Tag = b;
      n.Checked = true;
      treeView1.SelectedNode.Expand();
      Regen();
      m_bIgnore = false;

      treeView1.SelectedNode = n;
    }

    private void AddTrunk_Click(object sender, EventArgs e)
    {
      m_bIgnore = true;
      var c = (BranchDef)treeView1.Nodes[0].Nodes[0].Tag;
      var b = BranchDef.Trunk(m_Scene);
      BranchDefBase.AddSibling(b, c);
      var n = treeView1.Nodes[0].Nodes.Add(b.name);
      n.Tag = b;
      n.Checked = true;
      treeView1.SelectedNode.Expand();
      Regen();
      m_bIgnore = false;

      treeView1.SelectedNode = n;
    }

    private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
      if (e.Label == "")
      {
        e.CancelEdit = true;
        return;
      }


      var b = treeView1.SelectedNode.Tag as BranchDefBase;

      if (b == null)
      {
        m_Plant.def.name = e.Label;
      }

      else

      {
        b.name = e.Label;

        if (b is BranchDef)
          branchesPage.Text = b.name;
        else
          leavesPage.Text = b.name;
      }

      m_Scene.IsDirty |= WhatBits.PlantDefs;
    }


    TreeNode m_Dragged;
    
    private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
    {
      m_Dragged = null;
      var dragged = e.Item as TreeNode;
      var b = dragged.Tag as LeafDef;
      if (b == null)
        return;
      m_Dragged = dragged;
      DoDragDrop(e.Item, DragDropEffects.All);


    }

    private void treeView1_DragEnter(object sender, DragEventArgs e)
    {
      if ((e.KeyState & 8) != 0)
        e.Effect = DragDropEffects.Move;
      else
        e.Effect = DragDropEffects.Copy;
    }

    private void treeView1_DragDrop(object sender, DragEventArgs e)
    {
      if (m_Dragged == null)
        return;
      var leaf = m_Dragged.Tag as LeafDef;
      if (leaf == null)
        return;
      

      Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
      var DestinationNode = ((TreeView)sender).GetNodeAt(pt);
      var branch = DestinationNode.Tag as BranchDef;
      if (branch == null)
        return;

      if (e.Effect == DragDropEffects.Move)
      {
        m_Dragged.Parent.Nodes.Remove(m_Dragged);
        DestinationNode.Nodes.Add(m_Dragged);

        BranchDef.RemoveChildFromParent(leaf);
        leaf.parent = branch;
        leaf.sib = null;
        BranchDefBase.AddChild(leaf);

        Regen();
        treeView1.SelectedNode = m_Dragged;
      }

      else
      {
        var newleaf = LeafDef.Leaf(branch, m_Scene, leaf);
        BranchDefBase.AddChild(newleaf);
        var newNode = new TreeNode(newleaf.name);
        newNode.Checked = true;
        DestinationNode.Nodes.Add(newNode);
        newNode.Tag = newleaf;
        Regen();
        treeView1.SelectedNode = newNode;

      }

      m_Dragged = null;
        

    }

    private void treeView1_DragOver(object sender, DragEventArgs e)
    {
      NativeMethods.Scroll(treeView1);

      Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
      TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);

      m_bIgnore = true;
      treeView1.SelectedNode = DestinationNode;
      m_bIgnore = false;

      if (DestinationNode == null)
      {
        e.Effect = DragDropEffects.None;
        return;
      }

      var b = DestinationNode.Tag as BranchDef;

      if (b == null)
      {
        e.Effect = DragDropEffects.None;
        return;
      }


      if ((e.KeyState & 8) != 0)
        e.Effect = DragDropEffects.Move;
      else
        e.Effect = DragDropEffects.Copy;

    }



    #endregion

    #region Materials / Textures


    String m_BarkTextureFolder, m_LeafTextureFolder;

    private void SetTextureFolder(String folder, Control c)
    {
      if ((c == leafFrontMaterial) || (c == leafBackMaterial))
        m_LeafTextureFolder = folder;
      else
        m_BarkTextureFolder = folder;
    }

    private string GetTextureFolder(Control c)
    {
      if ((c == leafFrontMaterial) || (c == leafBackMaterial))
        return m_LeafTextureFolder;
      else
        return m_BarkTextureFolder;
    }

        

    private void ChangeTexture_Click(object sender, EventArgs e)
    {
      var item = sender as ToolStripMenuItem;
      var owner = item.Owner as ContextMenuStrip;
      var m = MaterialFromControl(owner.SourceControl) as ElementalMaterial;
      if (m == null) return;

      //if ((m_DefaultTextureFolder != null) && (m_DefaultTextureFolder != ""))
      //  openFileDialog1.InitialDirectory = m_DefaultTextureFolder;
      //openFileDialog1.FileName = (m_MaterialMap.Texture.FullPath != null) ? System.IO.Path.GetFileName(m_MaterialMap.Texture.FullPath) : "";
      openFileDialog1.Filter = "Image Maps|*.jpg;*.png;*.tif;*.bmp";
      openFileDialog1.FilterIndex = 1;
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.InitialDirectory = GetTextureFolder(owner.SourceControl);
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        SetTextureFolder(Path.GetDirectoryName(openFileDialog1.FileName) + Path.DirectorySeparatorChar, owner.SourceControl);

        try
        {
          var tm = m.GetTexture(Path.GetFileName(openFileDialog1.FileName));
          if (tm == null)
          {
            tm = TextureMap.Create(m.m_Scene, openFileDialog1.FileName);
            if (tm != null)
              m.AddTexture(tm);
          }

          if (tm == null)
            return;

          m.Color.MaterialMap = new MaterialMap(m_Scene);
          m.Color.MaterialMap.Texture = tm;

          if ((owner.SourceControl == leafFrontMaterial) || (owner.SourceControl == leafBackMaterial))
          {
            ScalarTex Opacity = null;
            var sm = m as StandardMaterial;
            if (sm != null)
              Opacity = sm.Opacity;
            else
            {
              var mm = m as MetallicMaterial;
              if (mm != null)
                Opacity = mm.Opacity;
            }
            
            if (Opacity != null)
            {
              Opacity.MaterialMap = new MaterialMap(m_Scene);
              Opacity.MaterialMap.Texture = tm;
              Opacity.MaterialMap.AlphaUsage = AlphaUsageType.Data;
            }
          

          }

        }

        catch { }

        ApplyThumbnail(owner.SourceControl as Button, m);
        m_OpenGL.m_bTexturesRequired = 1;
        Regen();
      }



    }


    private void NewMaterialFromTexture(Button source)
    {
      if (source == null) return;
      Material m = new StandardMaterial(m_Scene);

      openFileDialog1.Filter = "Image Maps|*.jpg;*.png;*.tif;*.bmp|Material File|*.ArMtl";
      openFileDialog1.FilterIndex = 1;
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.InitialDirectory = GetTextureFolder(source);
      TextureMap tm = null;
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        if (openFileDialog1.FilterIndex == 1)
        {
          SetTextureFolder(Path.GetDirectoryName(openFileDialog1.FileName) + Path.DirectorySeparatorChar, source);
          try
          {
            tm = m.GetTexture(Path.GetFileName(openFileDialog1.FileName));
            if (tm == null)
            {
              tm = TextureMap.Create(m_Scene, openFileDialog1.FileName);
              if (tm != null)
                m.AddTexture(tm);
            }

            if (tm == null)
              return;

            var sm = m as StandardMaterial;
            sm.Color.MaterialMap = new MaterialMap(m_Scene);
            sm.Color.MaterialMap.Texture = tm;

            if ((source == leafFrontMaterial) || (source == leafBackMaterial))
            {
              sm.Opacity.MaterialMap = new MaterialMap(m_Scene);
              sm.Opacity.MaterialMap.Texture = tm;
              sm.Opacity.MaterialMap.AlphaUsage = AlphaUsageType.Data;
            }


            //m_MaterialMap.Texture = tm;
            ////lock (tm.BitmapLock)
            //TextureButton.BackgroundImage = tm.Map.ToBitmap();
            //m_DefaultTextureFolder = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
            //OnTextureMapChanged();


          }

          catch { return; }
        }

        else
        {
          try
          {
            var s = new Scene();
            s.Read(openFileDialog1.FileName, WhatBits.Materials | WhatBits.Textures);
            var mm = s.Materials.First();
            if (mm.Value != null)
            {
              m = mm.Value;
              m.m_Scene = m_Scene;
            }

          }

          catch { return; }


        }


        int i = 1;
        Material mat;
        while (m_Scene.Materials.TryGetValue(m_Plant.def.name + "-New Material " + i.ToString(), out mat))
          i++;

        m.Name = m_Plant.def.name + "-New Material " + i.ToString();

        m_Scene.Materials.Add(m.Name, m);

        if (source == leafBackMaterial)
          currentLeaf.backMaterial = m.Name;
        else
          currentSelection.frontMaterial = m.Name;


        ApplyThumbnail(source, m);
        m_OpenGL.m_bTexturesRequired = 1;
        Regen();
      }

    }



    private void NewMaterialFromTexture_Click(object sender, EventArgs e)
    {
      var item = sender as ToolStripMenuItem;
      var owner = item.Owner as ContextMenuStrip;
      var m = new StandardMaterial(m_Scene);
      NewMaterialFromTexture(owner.SourceControl as Button);
    }






    private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
    {
      ContextMenuStrip owner = sender as ContextMenuStrip;
      String mat;
      if (owner.SourceControl == branchFrontMaterial)
        mat = currentBranch.frontMaterial;
      else if (owner.SourceControl == leafFrontMaterial)
        mat = currentLeaf.frontMaterial;
      else
        mat = currentLeaf.backMaterial;


      ToolStripMenuItem item;
      contextMenuStrip1.Items.Clear();
      item = (ToolStripMenuItem)contextMenuStrip1.Items.Add("Change Texture...");
      item.Click += ChangeTexture_Click;


      item = (ToolStripMenuItem)contextMenuStrip1.Items.Add("New Material From Texture or File...");
      item.Click += NewMaterialFromTexture_Click;


      contextMenuStrip1.Items.Add(new ToolStripSeparator());
    

      foreach (var m in m_Scene.Materials)
      {
        item = (ToolStripMenuItem)contextMenuStrip1.Items.Add(m.Value.Name);
        item.Click += Material_Click;
        if (m.Value.Name == mat)
          item.Checked = true;
      }

      contextMenuStrip1.Items.Add(new ToolStripSeparator());
    
      item = (ToolStripMenuItem)contextMenuStrip1.Items.Add("Edit Material...");
      item.Click += EditMaterial_Click;
      contextMenuStrip1.Items.Add(new ToolStripSeparator());
      item = (ToolStripMenuItem)contextMenuStrip1.Items.Add("Delete Materials...");
      item.Click += DeleteMaterial_Click;

    }


    private String MaterialName (Control c)
    {
      if (c == branchFrontMaterial)
        return currentBranch.frontMaterial;
      else if (c == leafFrontMaterial)
        return currentLeaf.frontMaterial;
      return  currentLeaf.backMaterial;
    }

    private Material MaterialFromControl(Control c)
    {
      Material m;
      m_Scene.Materials.TryGetValue(MaterialName(c), out m);
      return m;
    }


    void MaterialEditor_MaterialEditorChanged(object sender, EventArgs e)
    {
      ApplyThumbnail(editMaterialButton, m_MaterialEditor.materialEditorControl1.m_Material);
      OnSettingsChanged();
    }


    // iterate through scene and change any references to oldMaterialName
    private void ChangeMaterialNameReferences(BranchDefBase b, string oldMaterialName, string newMaterialName)
    {
      b.frontMaterial = b.frontMaterial == oldMaterialName ? newMaterialName : b.frontMaterial;
      b.backMaterial = b.backMaterial == oldMaterialName ? newMaterialName : b.backMaterial;
      if (b.child != null)
        ChangeMaterialNameReferences(b.child, oldMaterialName, newMaterialName);
      if (b.sib != null)
        ChangeMaterialNameReferences(b.sib, oldMaterialName, newMaterialName);
    }

   
    private bool ChangeMaterialName(string oldMaterialName, string newMaterialName)
    {
      if (newMaterialName == null)
        return false;

      Material m;
      if (m_Scene.Materials.TryGetValue(newMaterialName, out m))
        return false;

      if (oldMaterialName == newMaterialName)
        return false;

      if (!m_Scene.Materials.TryGetValue(oldMaterialName, out m))
        return false;

      m.m_Scene.Materials.Remove(m.Name);
      m.Name = newMaterialName;
      try
      {
         m_Scene.Materials.Add(m.Name, m);
      }
      catch
      {
        return false;
      }


      ChangeMaterialNameReferences(m_Plant.def.branches, oldMaterialName, newMaterialName);
  

      return true;
    }




    void MaterialEditor_MaterialEditorNameChanged(object sender, NodeLabelEditEventArgs e)
    {
      if (!ChangeMaterialName(e.Node.Text, e.Label))
      {
        e.CancelEdit = true;
        return;
      }
   
      Material m;
      if (!m_Scene.Materials.TryGetValue(e.Label, out m))
      {
        e.CancelEdit = true;
        return;
      }
     
    }


    MaterialEditorDB m_MaterialEditor;

    private void EditMaterial(Material m)
    {
      if (m == null)
        return;


      if (m_MaterialEditor == null)
      {
        m_MaterialEditor = new MaterialEditorDB();
        m_MaterialEditor.materialEditorControl1.MaterialChanged += MaterialEditor_MaterialEditorChanged;
        m_MaterialEditor.materialEditorControl1.MaterialNameChanged += MaterialEditor_MaterialEditorNameChanged;
        ArControls.Common.SetFormInitialLocation(m_MaterialEditor, Settings.Default.MaterialEditorLocation);
      }

     
      m_MaterialEditor.materialEditorControl1.Init(m);
      m_MaterialEditor.ShowDialog(this);
      m_MaterialEditorLocation = m_MaterialEditor.Location;
     

    }

    Button editMaterialButton;
    


    private void EditMaterial_Click(object sender, EventArgs e)
    {
      var item = sender as ToolStripMenuItem;
      var owner = item.Owner as ContextMenuStrip;
      var m = MaterialFromControl(owner.SourceControl);
      editMaterialButton = owner.SourceControl as Button;
      EditMaterial(m);
      m_OpenGL.m_bTexturesRequired = 1;
      Regen();
    }

    private void DeleteMaterial_Click(object sender, EventArgs e)
    {
      var dm = new DeleteMaterials();
      foreach (var m in m_Scene.Materials)
        dm.checkedListBox1.Items.Add(m.Value.Name);
      
      if (dm.ShowDialog(this) == DialogResult.OK)
      {
        foreach (var item in dm.checkedListBox1.CheckedItems)
        {
          try
          {
            m_Scene.Materials.Remove(item.ToString());
          }

          catch { }
        }

        Regen();
      }


    }


    private void Material_Click(object sender, EventArgs e)
    {
      var item = sender as ToolStripMenuItem;
      if (item.Checked)
        return;
      
      var owner = item.Owner as ContextMenuStrip;
      if (owner.SourceControl == branchFrontMaterial)
        currentBranch.frontMaterial = item.Text;
      else if (owner.SourceControl == leafFrontMaterial)
        currentLeaf.frontMaterial = item.Text;
      else
        currentLeaf.backMaterial = item.Text;

      Material m;
      if (m_Scene.Materials.TryGetValue(item.Text, out m))
      {
        var b = owner.SourceControl as Button;
        ApplyThumbnail(b, m);
      }

      m_OpenGL.m_bTexturesRequired = 1;
      Regen();

    }

    private void branchFrontMaterial_Click(object sender, EventArgs e)
    {
      NewMaterialFromTexture(branchFrontMaterial);
    }


    private void leafFrontMaterial_Click(object sender, EventArgs e)
    {
      NewMaterialFromTexture(leafFrontMaterial);
    }

   

    private void leafBackMaterial_Click(object sender, EventArgs e)
    {
      NewMaterialFromTexture(leafBackMaterial);
    }

   
    #endregion

    #region View Menu



    bool bGroundPlane = true;

    private void groundplaneToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
      bGroundPlane = groundplaneToolStripMenuItem.Checked;
      Regen();
    }

    private void directSunToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
    {
      m_Scene.lighting.SunChannelOn = directSunToolStripMenuItem.Checked;
      m_Renderer.SetLightingChannels(m_Scene);
    }


    #endregion
    
    #region Experimental

    private void genSphereToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var s = new Scene();
      var m = new Mesh(s);
      UVMapping mapping;
      m.AddMapping(out mapping);

      // add junk vertex;
      m.m_Points.Add(new Vec3());
      m.m_Normals.Add(new Vec3());
      mapping.m_UV.Add(new Vec2());

      var _P = new Vec3[12];
      _P[0].x = 0.000f; _P[0].y = 1.000f; _P[0].z = 0.000f;    // Top-most point.
      _P[1].x = 0.894f; _P[1].y = 0.447f; _P[1].z = 0.000f;
      _P[2].x = 0.276f; _P[2].y = 0.447f; _P[2].z = 0.851f;
      _P[3].x = -0.724f; _P[3].y = 0.447f; _P[3].z = 0.526f;
      _P[4].x = -0.724f; _P[4].y = 0.447f; _P[4].z = -0.526f;
      _P[5].x = 0.276f; _P[5].y = 0.447f; _P[5].z = -0.851f;
      _P[6].x = 0.724f; _P[6].y = -0.447f; _P[6].z = 0.526f;
      _P[7].x = -0.276f; _P[7].y = -0.447f; _P[7].z = 0.851f;
      _P[8].x = -0.894f; _P[8].y = -0.447f; _P[8].z = 0.000f;
      _P[9].x = -0.276f; _P[9].y = -0.447f; _P[9].z = -0.851f;
      _P[10].x = 0.724f; _P[10].y = -0.447f; _P[10].z = -0.526f;
      _P[11].x = 0.000f; _P[11].y = -1.000f; _P[11].z = 0.000f;


      var oneOverTwoPI = 1f / ((float)Math.PI * 2f);


      for (int i = 0; i < _P.Length; i++)
      {
        m.m_Normals.Add(_P[i]);
        float theta, phi;
        Utils.CartesianToSpherical(_P[i], out theta, out phi);
        float v = theta * oneOverTwoPI + 0.5f;
        v = Math.Max(0, Math.Min(1, v));
        float u = phi * oneOverTwoPI;
        u = Math.Max(0, Math.Min(1, u));
        mapping.m_UV.Add(new Vec2(u, v));
        m.m_Points.Add((_P[i] + new Vec3(0, 0, 1f)) * 0.5f);
      }

      m.m_Faces.Add(new IVec3(12, 10, 11));
      m.m_Faces.Add(new IVec3(12, 9, 10));
      m.m_Faces.Add(new IVec3(12, 8, 9));
      m.m_Faces.Add(new IVec3(12, 7, 8));
      m.m_Faces.Add(new IVec3(12, 11, 7));
      m.m_Faces.Add(new IVec3(1, 6, 5));
      m.m_Faces.Add(new IVec3(1, 5, 4));
      m.m_Faces.Add(new IVec3(1, 4, 3));
      m.m_Faces.Add(new IVec3(1, 3, 2));
      m.m_Faces.Add(new IVec3(1, 2, 6));
      m.m_Faces.Add(new IVec3(11, 10, 6));   // |,, Downwards pointing
      m.m_Faces.Add(new IVec3(10, 9, 5));   // |   triangles.
      m.m_Faces.Add(new IVec3(9, 8, 4));   // |
      m.m_Faces.Add(new IVec3(8, 7, 3));   // |
      m.m_Faces.Add(new IVec3(7, 11, 2));   // |
      m.m_Faces.Add(new IVec3(6, 10, 5));     // |,, Upwards pointing
      m.m_Faces.Add(new IVec3(5, 9, 4));     // |   triangles.
      m.m_Faces.Add(new IVec3(4, 8, 3));     // |
      m.m_Faces.Add(new IVec3(3, 7, 2));     // |
      m.m_Faces.Add(new IVec3(2, 11, 6));     // |

      s.Meshes.Add("Sphere", m);

      s.Write(@"C:\Users\Roy\Documents\AccuRender Libraries\Plants\Leaves\Meshes\Sphere.ArMesh", WhatBits.Geometry);


    }

    #endregion

  }


}


