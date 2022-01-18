using NotepadCSForm.Properties;
using System.Media;
using System.Text;

namespace NotepadCSForm
{
    public partial class FormMain : Form
    {
        #region Constants

        private const string AppName = "Notepad--";
        private const string Version = "v1.0.0";
        private const string Author = "Cédric-Antoine Ouellet";
        private const string AuthorGithubUsername = "cedricouellet";

        #endregion Constants

        #region Class Fields

        private SoundPlayer? _soundPlayer;
        private string _filename;
        private int _elapsedSeconds;
        private bool _unsavedChanges;
        private bool _warnedTimeSpent;

        #endregion Class Fields

        #region Constructor

        public FormMain()
        {
            InitializeComponent();

            _filename = string.Empty;
            _elapsedSeconds = 0;
            _unsavedChanges = true;
            _warnedTimeSpent = false;
            _soundPlayer = null;

            // Initialize audio menu items
            playToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;

            // Initialize information status strip
            // Align all items to the right, similar to the Windows Notepad app.
            foreach (ToolStripItem item in statusStripInformation.Items)
                item.Alignment = ToolStripItemAlignment.Right;

            // Add a separator between each item in the status strip
            int itemCount = statusStripInformation.Items.Count;
            for (int i = itemCount - 1; i >= 1; i--)
            {
                ToolStripItem separator = new ToolStripSeparator
                {
                    Alignment = ToolStripItemAlignment.Right
                };

                statusStripInformation.Items.Insert(i, separator);
            }

            SetFormTitle();

            Settings prefs = Settings.Default;
            ApplyPrefs(prefs);

            // The following 2 methods must be called after
            // applying preferences
            UpdateWordWrapControls(prefs.WordWrap);
            UpdateZoomDisplay();

            // Detect a file to open
            // Usage case: A file is asked to be opened with this app
            DetectFileToOpen();

            timer.Start();
        }

        #endregion Constructor

        #region Static Methods

        private static DialogResult AskToSave()
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            MessageBoxIcon icon = MessageBoxIcon.Warning;

            string caption = AppName;
            string content = new StringBuilder()
                .AppendLine("You have unsaved changes.")
                .AppendLine("")
                .AppendLine("Do you want to save your changes?")
                .ToString();

            return MessageBox.Show(content, caption, buttons, icon);
        }

        private static DialogResult AskToConfirmCodeTemplate()
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Warning;

            string caption = "Insert Code Template";
            string content = new StringBuilder()
                .AppendLine("Inserting a code template will override the current text.")
                .AppendLine("All previous unsaved changes will be lost.")
                .AppendLine()
                .AppendLine("This cannot be undone.")
                .AppendLine()
                .AppendLine("Are you sure you want to proceed?")
                .ToString();

            return MessageBox.Show(content, caption, buttons, icon);
        }

        #endregion Static Methods

        #region Class Methods

        private void DetectFileToOpen()
        {
            string[] argv = Environment.GetCommandLineArgs();
            if (argv.Length <= 1)
                return;

            _filename = argv[1];
            OpenFile();
        }

        private void SetFormTitle(string? filename = null)
        {
            if (filename == null || filename == string.Empty)
                filename = "Untitled";

            if (_unsavedChanges)
                filename = $"*{filename}";

            this.Text = $"{AppName} [{filename}]";
        }

        private void ApplyPrefs(Settings prefs)
        {
            richTextBoxNotepad.BackColor = prefs.BackgroundColor;
            richTextBoxNotepad.ForeColor = prefs.ForegroundColor;
            richTextBoxNotepad.WordWrap = prefs.WordWrap;
            richTextBoxNotepad.Font = prefs.Font;
        }

        private void InsertCodeTemplate(string template)
        {
            if (richTextBoxNotepad.TextLength > 0)
            {
                DialogResult result = AskToConfirmCodeTemplate();
                if (result != DialogResult.Yes)
                    return;
            }
            richTextBoxNotepad.Clear();
            richTextBoxNotepad.AppendText(template);
            _unsavedChanges = true;
        }

        private void UpdateLineNumbers()
        {
            richTextBoxLineNumbers.Text = "";

            Point point = new(0, 0);

            RichTextBox notepad = richTextBoxNotepad;

            int firstIndex = notepad.GetCharIndexFromPosition(point);
            int firstLine = notepad.GetLineFromCharIndex(firstIndex);

            point.X = ClientRectangle.Width;
            point.Y = ClientRectangle.Height;

            int lastIndex = notepad.GetCharIndexFromPosition(point);
            int lastLine = notepad.GetLineFromCharIndex(lastIndex);

            richTextBoxLineNumbers.SelectionAlignment = HorizontalAlignment.Center;

            for (int i = firstLine; i <= lastLine + 2; i++)
            {
                string linesDisplay = new StringBuilder()
                    .Append(i + 1)
                    .AppendLine()
                    .ToString();

                richTextBoxLineNumbers.AppendText(linesDisplay);
            }
        }

        private void UpdateLineAndColumnCount()
        {
            int lineIndex = richTextBoxNotepad.SelectionStart;
            int line = richTextBoxNotepad.GetLineFromCharIndex(lineIndex);

            int firstCharIndex = richTextBoxNotepad.GetFirstCharIndexFromLine(line);
            int col = lineIndex - firstCharIndex;

            toolStripStatusLabelLineColumn.Text = $"Ln {line + 1}, Col {col + 1}";
        }

        private void UpdateZoomDisplay()
        {
            toolStripStatusLabelZoom.Text = $"Font Size: {richTextBoxNotepad.Font.Size}";
        }

        private void UpdateAudioControls(bool isPlaying)
        {
            stopToolStripMenuItem.Enabled = isPlaying;
            playToolStripMenuItem.Enabled = !isPlaying;
            playToolStripMenuItem.Checked = isPlaying;
        }

        private void UpdateWordWrapControls(bool wordWrap)
        {
            wordWrapOffToolStripMenuItem.Enabled = wordWrap;
            wordWrapOnToolStripMenuItem.Enabled = !wordWrap;

            wordWrapOnToolStripMenuItem.Checked = wordWrap;
            wordWrapOffToolStripMenuItem.Checked = !wordWrap;
        }

        private void UpdateTimer()
        {
            int seconds = _elapsedSeconds % 60;
            int minutes = (_elapsedSeconds - seconds) / 60 % 60;
            int hours = (_elapsedSeconds - (seconds + (minutes * 60))) / 3600 % 60;

            toolStripStatusLabelTimer.Text = $"{hours:00}h {minutes:00}m {seconds:00}s";

            if (_warnedTimeSpent || hours < 1)
                return;

            _warnedTimeSpent = true;

            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.Information;

            string caption = "Woah there, cowboy!";
            string content = new StringBuilder()
                .AppendLine("You've been editing for an hour!")
                .AppendLine("You should take a break for a bit.")
                .ToString();

            MessageBox.Show(content, caption, buttons, icon);
        }

        private void ResetTimer()
        {
            _elapsedSeconds = 0;
        }

        private void OpenFile()
        {
            using StreamReader reader = new(_filename);
            richTextBoxNotepad.Text = reader.ReadToEnd();
            richTextBoxNotepad.SelectionStart = richTextBoxNotepad.TextLength;

            _unsavedChanges = false;

            SetFormTitle(_filename);
            ResetTimer();
        }

        private void SaveFile()
        {
            using StreamWriter writer = new(_filename);
            writer.Write(richTextBoxNotepad.Text);

            _unsavedChanges = false;

            SetFormTitle(_filename);
        }

        private void SaveFileWithDialog()
        {
            DialogResult saveFileResult = saveFileDialog.ShowDialog();
            if (saveFileResult != DialogResult.OK)
                return;

            _filename = saveFileDialog.FileName;
            SaveFile();
        }

        private void OpenNewDocument()
        {
            richTextBoxNotepad.Clear();
            _filename = string.Empty;
            _unsavedChanges = false;
            SetFormTitle(_filename);
            ResetTimer();
            return;
        }

        #endregion Class Methods

        #region Event Listeners

        private void OpenAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult openFileResult = openAudioFileDialog.ShowDialog();
            if (openFileResult != DialogResult.OK)
                return;

            string soundFile = openAudioFileDialog.FileName;
            _soundPlayer = new SoundPlayer(soundFile);
            playToolStripMenuItem.Enabled = true;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_unsavedChanges)
                OpenNewDocument();

            DialogResult askToSaveResult = AskToSave();

            if (askToSaveResult == DialogResult.Cancel)
                return;

            if (askToSaveResult == DialogResult.Yes)
            {
                if (_filename == string.Empty)
                    SaveFileWithDialog();
                else
                    SaveFile();
            }

            OpenNewDocument();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult saveResult = AskToSave();

            if (saveResult == DialogResult.Cancel)
                return;

            if (saveResult == DialogResult.Yes)
            {
                if (_filename == string.Empty)
                    SaveFileWithDialog();
                else
                    SaveFile();
            }

            DialogResult openFileResult = openFileDialog.ShowDialog();
            if (openFileResult != DialogResult.OK)
                return;

            _filename = openFileDialog.FileName;
            OpenFile();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_filename == string.Empty)
            {
                SaveFileWithDialog();
                return;
            }

            SaveFile();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileWithDialog();
        }

        private void PrintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printFileDialog.ShowDialog();
            // nothing else to do here, OS will take care of the rest.
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!richTextBoxNotepad.CanUndo)
                return;

            richTextBoxNotepad.Undo();
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!richTextBoxNotepad.CanRedo)
                return;

            richTextBoxNotepad.Redo();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBoxNotepad.SelectionLength == 0)
                return;

            richTextBoxNotepad.Copy();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (richTextBoxNotepad.SelectionLength == 0)
                return;

            richTextBoxNotepad.Cut();
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetText().Length == 0)
                return;

            richTextBoxNotepad.Paste();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.SelectedText = string.Empty;
        }

        private void DeleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.Clear();
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.SelectAll();
        }

        private void DateTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            richTextBoxNotepad.AppendText(now.ToString());
        }

        private void FontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult selectFontResult = fontDialog.ShowDialog();
            if (selectFontResult != DialogResult.OK)
                return;

            richTextBoxNotepad.Font = fontDialog.Font;
        }

        private void WordWrapOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.WordWrap = true;
            UpdateWordWrapControls(true);
        }

        private void WordWrapOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.WordWrap = false;
            UpdateWordWrapControls(false);
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.Information;

            string caption = $"About {AppName}";
            string content = new StringBuilder()
                .AppendLine($"{AppName} {Version}")
                .AppendLine()
                .AppendLine("This application is a simple notepad made with C#.")
                .AppendLine()
                .Append("It contains common features also present in the builtin Windows Notepad application,")
                .Append("but a few other \"nice to have\" features.")
                .AppendLine()
                .AppendLine()
                .AppendLine("Copyright \u00a9 2021 cedricao. All Rights Reserved")
                .ToString();

            MessageBox.Show(content, caption, buttons, icon);
        }

        private void RichTextBoxNotepad_TextChanged(object sender, EventArgs e)
        {
            UpdateLineNumbers();

            bool hasNoFile = _filename == string.Empty;
            bool textEmpty = richTextBoxNotepad.TextLength == 0;

            _unsavedChanges = !(hasNoFile && textEmpty);

            SetFormTitle(_filename);
        }

        private void RichTextBoxNotepad_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateLineAndColumnCount();
        }

        private void RichTextBoxNotepad_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateLineAndColumnCount();
        }

        private void ForegroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult colorPickResult = colorDialog.ShowDialog();
            if (colorPickResult != DialogResult.OK)
                return;

            richTextBoxNotepad.ForeColor = colorDialog.Color;
        }

        private void BackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult colorPickResult = colorDialog.ShowDialog();
            if (colorPickResult != DialogResult.OK)
                return;

            richTextBoxNotepad.BackColor = colorDialog.Color;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ++_elapsedSeconds;
            UpdateTimer();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save preferences
            Settings.Default.BackgroundColor = richTextBoxNotepad.BackColor;
            Settings.Default.ForegroundColor = richTextBoxNotepad.ForeColor;
            Settings.Default.Font = richTextBoxNotepad.Font;
            Settings.Default.WordWrap = richTextBoxNotepad.WordWrap;
            Settings.Default.Save();

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            if (!_unsavedChanges)
                return;

            DialogResult askToSaveResult = AskToSave();

            if (askToSaveResult == DialogResult.No)
                return;

            if (askToSaveResult == DialogResult.Yes)
            {
                if (_filename == string.Empty)
                    SaveFileWithDialog();
                else
                    SaveFile();
            }

            if (askToSaveResult == DialogResult.Cancel)
                e.Cancel = true;
        }

        private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.Reset();
            ApplyPrefs(Settings.Default);
        }

        private void LennyFaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.AppendText("( ͡° ͜ʖ ͡°)");
        }

        private void FightingPoseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.AppendText("(ง ͠° ͟ل͜ ͡°)ง");
        }

        private void MafiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.AppendText("( ͡°( ͡° ͜ʖ( ͡° ͜ʖ ͡°)ʖ ͡°) ͡°)");
        }

        private void TableFlipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.AppendText("(ノಠ益ಠ)ノ彡┻━┻");
        }

        private void IdkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.AppendText("¯\\_(ツ)_/¯");
        }

        private void Python3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string template = new StringBuilder()
                .AppendLine("def main():")
                .AppendLine("\tprint(\"Hello World!\")")
                .AppendLine()
                .AppendLine("if __name__ == \"__main__\":")
                .AppendLine("\tmain()")
                .ToString();

            InsertCodeTemplate(template);
        }

        private void CSharpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string template = new StringBuilder()
                .AppendLine("using System;")
                .AppendLine()
                .AppendLine("namespace MyApp")
                .AppendLine("{")
                .AppendLine("\tclass Program")
                .AppendLine("\t{")
                .AppendLine("\t\tstatic void Main(string[] args)")
                .AppendLine("\t\t{")
                .AppendLine("\t\t\tConsole.WriteLine(\"Hello World!\");")
                .AppendLine("\t\t}")
                .AppendLine("\t}")
                .AppendLine("}")
                .ToString();

            InsertCodeTemplate(template);
        }

        private void PlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAudioControls(true);
            _soundPlayer?.Play();
        }

        private void StopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAudioControls(false);
            _soundPlayer?.Stop();
        }

        private void DeveloperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Information;

            string caption = $"Developer";
            string content = new StringBuilder()
                .AppendLine($"This app is developed by {Author}")
                .AppendLine()
                .AppendLine("Would you like to visit their GitHub account?")
                .ToString();

            DialogResult result = MessageBox.Show(content, caption, buttons, icon);

            if (result != DialogResult.Yes)
                return;

            BrowserUtils.OpenGithubProfile(AuthorGithubUsername);
        }

        private void LineUnderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxNotepad.AppendText(Environment.NewLine);
        }

        private void SearchOnStackOverflowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selectedText = richTextBoxNotepad.SelectedText;

            if (selectedText.Length > 0)
            {
                BrowserUtils.SearchOnStackOverflow(selectedText);
                return;
            }

            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.Information;

            string caption = "Stack Overflow Search";
            string content = new StringBuilder()
                .AppendLine("No text is currently selected.")
                .AppendLine("Therefore, there is nothing to search on Stack Overflow.")
                .AppendLine()
                .AppendLine("If you intend to perform a search, please try again while selecting some text.")
                .ToString();

            MessageBox.Show(content, caption, buttons, icon);
        }

        private void GoogleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenGoogle();
        }

        private void YoutubeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenYoutube();
        }

        private void RedditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenReddit();
        }

        private void TwitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenTwitter();
        }

        private void InstagramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenInstagram();
        }

        private void ProgrammerHumorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenRedditProgrammerHumor();
        }

        private void MemesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenRedditMemes();
        }

        private void GitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenGithub();
        }

        private void FacebookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BrowserUtils.OpenFacebook();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            richTextBoxLineNumbers.Font = richTextBoxNotepad.Font;

            richTextBoxNotepad.Select();
            UpdateLineNumbers();
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            UpdateLineNumbers();
        }

        private void RichTextBoxNotepad_SelectionChanged(object sender, EventArgs e)
        {
            int caretPosition = richTextBoxNotepad.SelectionStart;
            Point point = richTextBoxNotepad.GetPositionFromCharIndex(caretPosition);

            if (point.X != 1) return;

            UpdateLineNumbers();
        }

        private void RichTextBoxNotepad_VScroll(object sender, EventArgs e)
        {
            richTextBoxLineNumbers.Text = "";
            UpdateLineNumbers();
            richTextBoxLineNumbers.Invalidate();
        }

        private void RichTextBoxNotepad_FontChanged(object sender, EventArgs e)
        {
            richTextBoxLineNumbers.Font = richTextBoxNotepad.Font;
            richTextBoxNotepad.Select();
            UpdateZoomDisplay();
            UpdateLineNumbers();
        }

        private void RichTextBoxLineNumbers_MouseDown(object sender, MouseEventArgs e)
        {
            richTextBoxNotepad.Select();
            richTextBoxLineNumbers.DeselectAll();
        }

        private void RichTextBoxNotepad_MouseWheel(object? sender, MouseEventArgs e)
        {
            bool isResizeAttempt = (ModifierKeys & Keys.Control) == 0;

            // We do not want to be able to zoom.
            // It causes the line numbers to bug.
            // As it is not possible to always keep the ZoomFactor...
            //   the same for both the notepad textbox and the line number textbox...
            //   it is better to disable this feature and use font size to zoom instead.
            ((HandledMouseEventArgs)e).Handled = !isResizeAttempt;
        }

        #endregion Event Listeners
    }
}