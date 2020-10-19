using System;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Atomus.Diagnostics;
using Atomus.Control.Menu.Controllers;
using Atomus.Control.Menu.Models;

namespace Atomus.Control.Menu
{
    public partial class DefaultMenu : UserControl, IAction
    {
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;
        private ImageList imageList;
        private decimal START_MENU_ID = -1;
        private decimal ONLY_PARENT_MENU_ID = -1;

        #region Init
        public DefaultMenu()
        {
            InitializeComponent();
        }
        #endregion

        #region Dictionary
        #endregion

        #region Spread
        #endregion

        #region IO
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            decimal[] vs;
            try
            {
                this.beforeActionEventHandler?.Invoke(this, e);

                switch (e.Action)
                {
                    case "Search":
                        vs = (decimal[])e.Value;

                        this.START_MENU_ID = vs[0];
                        this.ONLY_PARENT_MENU_ID = vs[1];
                        this.Bnt_Refresh_Click(this.bnt_Refresh, null);
                        return true;
                    default:
                        throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            finally
            {
                this.afterActionEventHandler?.Invoke(this, e);
            }
        }

        private async Task<bool> LoadMenu(decimal START_MENU_ID, decimal ONLY_PARENT_MENU_ID)
        {
            Service.IResponse result;
            TreeNode treeNode;
            TreeNode[] treeNodes;
            string[] tmps;
            string tmp;
            string key;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                this.tvw_menu.BeginUpdate();//트리갱신 준비 
                this.tvw_menu.Nodes.Clear();

                result = await this.SearchAsync(new DefaultMenuSearchModel()
                {
                    START_MENU_ID = START_MENU_ID,
                    ONLY_PARENT_MENU_ID = ONLY_PARENT_MENU_ID
                });

                if (result.Status == Service.Status.OK)
                {
                    foreach (DataRow dataRow in result.DataSet.Tables[1].Rows)
                    {
                        key = string.Format("{0}.{1}", dataRow["IMAGE_URL1"], 1);
                        tmp = dataRow["IMAGE_URL1"].ToString();

                        if (!this.imageList.Images.ContainsKey(key) && tmp != "")
                            this.imageList.Images.Add(key, await this.GetAttributeWebImage(new Uri(tmp)));

                        key = string.Format("{0}.{1}", dataRow["IMAGE_URL2"], 2);
                        tmp = dataRow["IMAGE_URL2"].ToString();
                        if (!this.imageList.Images.ContainsKey(key) && tmp != "")
                            this.imageList.Images.Add(key, await this.GetAttributeWebImage(new Uri(tmp)));

                        key = string.Format("{0}.{1}", dataRow["IMAGE_URL3"], 3);
                        tmp = dataRow["IMAGE_URL3"].ToString();
                        if (!this.imageList.Images.ContainsKey(key) && tmp != "")
                            this.imageList.Images.Add(key, await this.GetAttributeWebImage(new Uri(tmp)));

                        key = string.Format("{0}.{1}", dataRow["IMAGE_URL4"], 4);
                        tmp = dataRow["IMAGE_URL4"].ToString();
                        if (!this.imageList.Images.ContainsKey(key) && tmp != "")
                            this.imageList.Images.Add(key, await this.GetAttributeWebImage(new Uri(tmp)));
                    }

                    foreach (DataRow dataRow in result.DataSet.Tables[1].Rows)
                    {
                        treeNodes = this.tvw_menu.Nodes.Find(dataRow["PARENT_MENU_ID"].ToString(), true);

                        dataRow["NAME"] = dataRow["NAME"].ToString().Translate();


                        tmps = this.GetAttribute("ShowNamespace.RESPONSIBILITY_ID").Split(',');

                        if (tmps.Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString()))
                            dataRow["DESCRIPTION"] = string.Format("{0} {1}", dataRow["DESCRIPTION"].ToString().Translate(), dataRow["NAMESPACE"]);
                        else
                            dataRow["DESCRIPTION"] = dataRow["DESCRIPTION"].ToString().Translate();


                        if (treeNodes.Length == 0)//없다면
                            treeNode = this.tvw_menu.Nodes.Add(dataRow["MENU_ID"].ToString(), dataRow["NAME"].ToString());
                        else//있다면
                        {
                            treeNode = treeNodes[0];
                            treeNode = treeNode.Nodes.Add(dataRow["MENU_ID"].ToString(), dataRow["NAME"].ToString());
                        }

                        treeNode.ToolTipText = dataRow["DESCRIPTION"].ToString();

                        if (dataRow["ASSEMBLY_ID"].ToString().Equals(""))//폴더
                        {
                            if (this.imageList.Images[string.Format("{0}.{1}", dataRow["IMAGE_URL1"], 1)] != null)
                            {
                                treeNode.ImageIndex = -1;
                                treeNode.ImageKey = string.Format("{0}.{1}", dataRow["IMAGE_URL1"], 1);
                            }
                            else
                            {
                                treeNode.ImageKey = "";
                                treeNode.ImageIndex = 0;
                            }

                            if (this.imageList.Images[string.Format("{0}.{1}", dataRow["IMAGE_URL2"], 2)] != null)
                            {
                                treeNode.SelectedImageIndex = -1;
                                treeNode.SelectedImageKey = string.Format("{0}.{1}", dataRow["IMAGE_URL2"], 2);
                            }
                            else
                            {
                                treeNode.SelectedImageKey = "";
                                treeNode.SelectedImageIndex = 1;
                            }
                        }
                        else//화면
                        {
                            if (this.imageList.Images[string.Format("{0}.{1}", dataRow["IMAGE_URL1"], 1)] != null)
                            {
                                treeNode.ImageIndex = -1;
                                treeNode.ImageKey = string.Format("{0}.{1}", dataRow["IMAGE_URL1"], 1);
                            }
                            else
                            {
                                treeNode.ImageKey = "";
                                treeNode.ImageIndex = 2;
                            }

                            if (this.imageList.Images[string.Format("{0}.{1}", dataRow["IMAGE_URL2"], 2)] != null)
                            {
                                treeNode.SelectedImageIndex = -1;
                                treeNode.SelectedImageKey = string.Format("{0}.{1}", dataRow["IMAGE_URL2"], 2);
                            }
                            else
                            {
                                treeNode.SelectedImageKey = "";
                                treeNode.SelectedImageIndex = 3;
                            }

                            //treeNode.ImageIndex = 2;
                            //treeNode.SelectedImageIndex = 3;

                            tmps = this.GetAttribute("ShowAssemblyID.RESPONSIBILITY_ID").Split(',');

                            if (tmps.Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString()))
                                treeNode.Text += string.Format(" ({0}.{1})", dataRow["MENU_ID"].ToString(), dataRow["ASSEMBLY_ID"].ToString());

                            treeNode.Tag = dataRow;
                        }
                    }

                    return true;
                }
                else
                {
                    this.MessageBoxShow(this, result.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            finally
            {
                this.tvw_menu.EndUpdate();
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        #region Event
        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }

        private async void DefaultMenu_Load(object sender, EventArgs e)
        {
            ContextMenuStrip contextMenuStrip;
            ToolStripMenuItem toolStripMenuItem;
            ToolStripSeparator toolStripSeparator;
            string[] tmps;

            try
            {
                if (this.GetAttribute("VisibleResponsibilityID") != "")
                {
                    tmps = this.GetAttribute("VisibleResponsibilityID").Split(',');

                    this.Visible = tmps.Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString());
                }

                this.tableLayoutPanel1.DoubleBuffered(true);
                this.tvw_menu.DoubleBuffered(true);
                this.bnt_Refresh.DoubleBuffered(true);
                this.bnt_ExpendAll.DoubleBuffered(true);
                this.bnt_CollapseAll.DoubleBuffered(true);

                this.bnt_Refresh.FlatAppearance.MouseDownBackColor = this.FindForm().BackColor;
                this.bnt_ExpendAll.FlatAppearance.MouseDownBackColor = this.FindForm().BackColor;
                this.bnt_CollapseAll.FlatAppearance.MouseDownBackColor = this.FindForm().BackColor;

                this.bnt_Refresh.FlatAppearance.MouseOverBackColor = this.FindForm().BackColor;
                this.bnt_ExpendAll.FlatAppearance.MouseOverBackColor = this.FindForm().BackColor;
                this.bnt_CollapseAll.FlatAppearance.MouseOverBackColor = this.FindForm().BackColor;

                try
                {
                    this.bnt_Refresh.BackgroundImage = await this.GetAttributeWebImage("RefreshImage");
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                try
                {
                    this.bnt_ExpendAll.BackgroundImage = await this.GetAttributeWebImage("ExpendAllImage");
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                try
                {
                    this.bnt_CollapseAll.BackgroundImage = await this.GetAttributeWebImage("CollapseAllImage");
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                this.imageList = new ImageList
                {
                    ImageSize = new Size(18, 18)
                };

                try
                {
                    this.imageList.Images.Add(await this.GetAttributeWebImage("FolderImage"));
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                try
                {
                    this.imageList.Images.Add(await this.GetAttributeWebImage("FolderOpenImage"));
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                try
                {
                    this.imageList.Images.Add(await this.GetAttributeWebImage("AssembliesImage"));
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                try
                {
                    this.imageList.Images.Add(await this.GetAttributeWebImage("AssembliesOpenImage"));
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                this.tvw_menu.BeginUpdate();
                this.tvw_menu.ImageList = this.imageList;
                this.tvw_menu.EndUpdate();

                await this.LoadMenu(this.START_MENU_ID, this.ONLY_PARENT_MENU_ID);

                contextMenuStrip = new ContextMenuStrip();

                toolStripMenuItem = new ToolStripMenuItem("Execute", null, Tvw_menu_DoubleClick);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripSeparator = new ToolStripSeparator();
                contextMenuStrip.Items.Add(toolStripSeparator);

                toolStripMenuItem = new ToolStripMenuItem("Refresh", null, Bnt_Refresh_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Expend", null, ToolStripMenuItem_Expend_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Expend all children", null, ToolStripMenuItem_Expend_Child_All_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Expend all", null, Bnt_ExpendAll_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Collapse", null, ToolStripMenuItem_Collapse_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Collapse all children", null, ToolStripMenuItem_Collapse_Child_All_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                toolStripMenuItem = new ToolStripMenuItem("Collapse all", null, Bnt_CollapseAll_Click);
                contextMenuStrip.Items.Add(toolStripMenuItem);

                this.tvw_menu.ContextMenuStrip = contextMenuStrip;
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private async void Bnt_Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                await this.LoadMenu(this.START_MENU_ID, this.ONLY_PARENT_MENU_ID);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void ToolStripMenuItem_Expend_Click(object sender, EventArgs e)
        {
            TreeView treeView;

            try
            {
                if (sender is TreeView)
                    treeView = (TreeView)sender;
                else
                {
                    treeView = (TreeView)((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
                }

                if (treeView.SelectedNode != null)
                    treeView.SelectedNode.Expand();
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }
        private void ToolStripMenuItem_Expend_Child_All_Click(object sender, EventArgs e)
        {
            TreeView treeView;

            try
            {
                if (sender is TreeView)
                    treeView = (TreeView)sender;
                else
                {
                    treeView = (TreeView)((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
                }

                if (treeView.SelectedNode != null)
                    treeView.SelectedNode.ExpandAll();
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }
        private void Bnt_ExpendAll_Click(object sender, EventArgs e)
        {
            this.tvw_menu.ExpandAll();
        }

        private void ToolStripMenuItem_Collapse_Click(object sender, EventArgs e)
        {
            TreeView treeView;

            try
            {
                if (sender is TreeView)
                    treeView = (TreeView)sender;
                else
                {
                    treeView = (TreeView)((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
                }

                if (treeView.SelectedNode != null)
                    treeView.SelectedNode.Collapse(true);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }
        private void ToolStripMenuItem_Collapse_Child_All_Click(object sender, EventArgs e)
        {
            TreeView treeView;

            try
            {
                if (sender is TreeView)
                    treeView = (TreeView)sender;
                else
                {
                    treeView = (TreeView)((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
                }

                if (treeView.SelectedNode != null)
                    treeView.SelectedNode.Collapse(false);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }
        private void Bnt_CollapseAll_Click(object sender, EventArgs e)
        {
            this.tvw_menu.CollapseAll();
        }

        private void Tvw_menu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            TreeView treeView;

            treeView = (TreeView)sender;

            if (e.Button == MouseButtons.Right)
                treeView.SelectedNode = e.Node;
        }

        private void Tvw_menu_DoubleClick(object sender, EventArgs e)
        {
            TreeView treeView;
            DataRow dataRow;

            try
            {
                if (sender is TreeView)
                    treeView = (TreeView)sender;
                else
                {
                    treeView = (TreeView)((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
                }

                if (treeView.SelectedNode != null && treeView.SelectedNode.Tag != null)
                {
                    dataRow = (DataRow)treeView.SelectedNode.Tag;

                    if (!dataRow["ASSEMBLY_ID"].ToString().Equals(""))
                    {
                        try
                        {
                            this.Cursor = Cursors.WaitCursor;
                            this.tvw_menu.Enabled = false;

                            this.afterActionEventHandler?.Invoke(this, "Menu.OpenControl", new object[] { dataRow["MENU_ID"], dataRow["ASSEMBLY_ID"], dataRow["VISIBLE_ONE"].ToString().Equals("Y") });
                        }
                        finally
                        {
                            this.tvw_menu.Enabled = true;
                            this.Cursor = Cursors.Default;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }
        #endregion

        #region "ETC"
        #endregion
    }
}