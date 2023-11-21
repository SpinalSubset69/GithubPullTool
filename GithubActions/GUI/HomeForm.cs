using GithubActions.Helpers;
using System;
using System.IO;
using System.Windows.Forms;

namespace GithubActions.GUI
{
    public partial class HomeForm : Form
    {
        private GithubHelper _git;
        private string repositoriesPath = "";
        private string currentDirectory;
        public HomeForm()
        {
            InitializeComponent();
            this.loadingPanel.Visible = false;
            PopulateTreeView();
            this.treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            _git = new GithubHelper();
        }

        /// <summary>
        /// Method to handle user's mouse event on moving window
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }

            base.WndProc(ref m);
        }

        #region Tree view methods        
        private void PopulateTreeView()
        {
            this.treeView1.Nodes.Clear();
            TreeNode rootNode;
            repositoriesPath = String.IsNullOrEmpty(this.folderBrowserDialog1.SelectedPath) ? "../.." : this.folderBrowserDialog1.SelectedPath;
            DirectoryInfo info = new DirectoryInfo(repositoriesPath);
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";                
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;
            currentDirectory = nodeDirInfo.FullName;
                
            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);                
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"),
             new ListViewItem.ListViewSubItem(item,
                dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
             new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        #endregion

        private async void button1_Click(object sender, EventArgs e)
        {            
            try
            {
                if (this.repositoriesPath == "../..")
                {
                    throw new Exception("Must select a path to save repositories");
                }
                this.loadingPanel.Visible = true;
                await _git.GetGithubRepository(new Entities.GithubParams 
                { 
                    RepoName = this.txtRepoName.Text, 
                    GitUserName = this.txtGitUsername.Text, 
                    LocalReposPath = repositoriesPath, 
                    BranchName = this.txtRepoBranch.Text 
                });
                this.loadingPanel.Visible = false;
                this.txtRepoName.Text = "";
                this.txtGitUsername.Text = "";
                this.txtRepoBranch.Text = "";
                PopulateTreeView();
                MessageBox.Show("Repositorio clonado con exito");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = this.folderBrowserDialog1.ShowDialog();

            if(result == DialogResult.OK)
            {
                PopulateTreeView();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            PopulateTreeView();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var selectedItem = this.listView1.SelectedItems[0];
            
            System.IO.FileInfo file = new System.IO.FileInfo(Path.Combine(currentDirectory, selectedItem.Text));

            if (file.Exists)
            {
                System.Diagnostics.Process.Start(file.FullName);
            }
        }
    }
}
