using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace RunThisAsAdmin;

public partial class AboutForm : Form
{
    private const string RepositoryUri = "https://github.com/wissamalhindi/RunThisAsAdmin";
    private const string LatestReleaseFileUri = "https://raw.githubusercontent.com/wissamalhindi/RunThisAsAdmin/main/LatestRelease";

    private string LatestReleaseUri { get; set; }

    public AboutForm()
    {
        InitializeComponent();
    }

    private void AboutForm_Load(object sender, EventArgs e)
    {
        ProductNameLabel.Text = Program.ProductName;
        ProductVersionLabel.Text = $@"Version: {Program.ProductVersion}";
    }

    private void CheckForUpdatesPictureBox_Click(object sender, EventArgs e)
    {
        try
        {
            var webRequest = WebRequest.Create(LatestReleaseFileUri);
            var webResponse = webRequest.GetResponse();
            using var memoryStream = new MemoryStream();
            webResponse.GetResponseStream()?.CopyTo(memoryStream);
            webResponse.Close();
            var fileContent = Encoding.UTF8.GetString(memoryStream.ToArray());
            var latestRelease = fileContent.Split('\n');
            var latestReleaseVersion = latestRelease[0];
            LatestReleaseUri = latestRelease[1];

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Program.ApplicationPath);
            var localVersion = new Version($"{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}");
            var serverVersion = new Version(latestReleaseVersion);
            var compareResult = serverVersion.CompareTo(localVersion);
            if (compareResult > 0)
            {
                var dialogResult = MessageBox.Show(@$"  A new version is available!{Environment.NewLine}  Do you want to download it?", Program.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                {
                    Process.Start(LatestReleaseUri);
                }
            }
            else
            {
                MessageBox.Show(@"  Application is up to date!", Program.ProductName);
            }
        }
        catch
        {
            Program.ShowErrorMessage(@"Failed to check for update!");
        }
    }

    private void CheckForUpdatesPictureBox_MouseEnter(object sender, EventArgs e)
    {
        CheckForUpdatesPictureBox.Image = Properties.Resources.CheckForUpdatesSelected;
    }

    private void CheckForUpdatesPictureBox_MouseLeave(object sender, EventArgs e)
    {
        CheckForUpdatesPictureBox.Image = Properties.Resources.CheckForUpdates;
    }

    private void GitHubPictureBox_Click(object sender, EventArgs e)
    {
        Process.Start(RepositoryUri);
    }

    private void GitHubPictureBox_MouseEnter(object sender, EventArgs e)
    {
        var toolTip = new ToolTip();
        toolTip.BackColor = Color.White;
        toolTip.InitialDelay = 100;
        toolTip.ReshowDelay = 200;
        toolTip.SetToolTip(GitHubPictureBox, RepositoryUri);

        GitHubPictureBox.Image = Properties.Resources.GitHubSelected;
    }

    private void GitHubPictureBox_MouseLeave(object sender, EventArgs e)
    {
        GitHubPictureBox.Image = Properties.Resources.GitHub;
    }
}
