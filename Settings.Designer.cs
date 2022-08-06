namespace MusicBeePlugin
{
    partial class Settings
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
            this.LB_SeeLogFile = new System.Windows.Forms.Label();
            this.BT_Cancel = new System.Windows.Forms.Button();
            this.BT_Save = new System.Windows.Forms.Button();
            this.GB_LikeADJSettings = new System.Windows.Forms.GroupBox();
            this.CB_AllowScanningMessageBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TB_NumberSongsPlaylist = new System.Windows.Forms.TextBox();
            this.LB_NumberSongsPlaylist = new System.Windows.Forms.Label();
            this.CB_SaveSongsPlaylist = new System.Windows.Forms.CheckBox();
            this.LB_BrightnessLightsRange = new System.Windows.Forms.Label();
            this.RS_BrightnessLightsRange = new MusicBeePlugin.SelectionRangeSlider();
            this.LB_SelectGenres = new System.Windows.Forms.Label();
            this.CB_AllowGenres = new System.Windows.Forms.CheckBox();
            this.CCB_Genres = new MusicBeePlugin.CheckedComboBox();
            this.TB_MinEnergy = new System.Windows.Forms.ComboBox();
            this.LB_MinimumEnergy = new System.Windows.Forms.Label();
            this.CB_AllowEnergy = new System.Windows.Forms.CheckBox();
            this.CB_DisableLogging = new System.Windows.Forms.CheckBox();
            this.TB_BeatDetectionEvery = new System.Windows.Forms.TextBox();
            this.LB_BeatDetectionEvery = new System.Windows.Forms.Label();
            this.LB_Lights = new System.Windows.Forms.Label();
            this.LV_Lights = new System.Windows.Forms.ListView();
            this.Used = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Number = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Lights = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CB_ChangeLightsWhen = new System.Windows.Forms.ComboBox();
            this.LB_ChangeLightsWhen = new System.Windows.Forms.Label();
            this.BT_ScanLights = new System.Windows.Forms.Button();
            this.BT_ResetHue = new System.Windows.Forms.Button();
            this.BT_PairHue = new System.Windows.Forms.Button();
            this.lblBridgeCnx = new System.Windows.Forms.Label();
            this.CB_AllowHue = new System.Windows.Forms.CheckBox();
            this.TB_MinRatings = new System.Windows.Forms.ComboBox();
            this.LB_MinimumRating = new System.Windows.Forms.Label();
            this.LB_MaximumDiffBPM = new System.Windows.Forms.Label();
            this.TB_DiffBPM = new System.Windows.Forms.TextBox();
            this.CB_AllowRatings = new System.Windows.Forms.CheckBox();
            this.CB_AllowInitialKey = new System.Windows.Forms.CheckBox();
            this.CB_AllowBPM = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.LL_LikeADJLog = new System.Windows.Forms.LinkLabel();
            this.GB_LikeADJSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // LB_SeeLogFile
            // 
            this.LB_SeeLogFile.AutoSize = true;
            this.LB_SeeLogFile.Location = new System.Drawing.Point(242, 628);
            this.LB_SeeLogFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_SeeLogFile.Name = "LB_SeeLogFile";
            this.LB_SeeLogFile.Size = new System.Drawing.Size(203, 13);
            this.LB_SeeLogFile.TabIndex = 28;
            this.LB_SeeLogFile.Text = "Don\'t forget, for more informations, to see:";
            this.LB_SeeLogFile.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // BT_Cancel
            // 
            this.BT_Cancel.Location = new System.Drawing.Point(398, 646);
            this.BT_Cancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BT_Cancel.Name = "BT_Cancel";
            this.BT_Cancel.Size = new System.Drawing.Size(112, 35);
            this.BT_Cancel.TabIndex = 27;
            this.BT_Cancel.Text = "Cancel";
            this.BT_Cancel.UseVisualStyleBackColor = true;
            this.BT_Cancel.Click += new System.EventHandler(this.BT_Cancel_Click);
            // 
            // BT_Save
            // 
            this.BT_Save.Location = new System.Drawing.Point(258, 646);
            this.BT_Save.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BT_Save.Name = "BT_Save";
            this.BT_Save.Size = new System.Drawing.Size(112, 35);
            this.BT_Save.TabIndex = 26;
            this.BT_Save.Text = "Save";
            this.BT_Save.UseVisualStyleBackColor = true;
            this.BT_Save.Click += new System.EventHandler(this.BT_OK_Click);
            // 
            // GB_LikeADJSettings
            // 
            this.GB_LikeADJSettings.Controls.Add(this.CB_AllowScanningMessageBox);
            this.GB_LikeADJSettings.Controls.Add(this.label2);
            this.GB_LikeADJSettings.Controls.Add(this.label1);
            this.GB_LikeADJSettings.Controls.Add(this.TB_NumberSongsPlaylist);
            this.GB_LikeADJSettings.Controls.Add(this.LB_NumberSongsPlaylist);
            this.GB_LikeADJSettings.Controls.Add(this.CB_SaveSongsPlaylist);
            this.GB_LikeADJSettings.Controls.Add(this.LB_BrightnessLightsRange);
            this.GB_LikeADJSettings.Controls.Add(this.RS_BrightnessLightsRange);
            this.GB_LikeADJSettings.Controls.Add(this.LB_SelectGenres);
            this.GB_LikeADJSettings.Controls.Add(this.CB_AllowGenres);
            this.GB_LikeADJSettings.Controls.Add(this.CCB_Genres);
            this.GB_LikeADJSettings.Controls.Add(this.TB_MinEnergy);
            this.GB_LikeADJSettings.Controls.Add(this.LB_MinimumEnergy);
            this.GB_LikeADJSettings.Controls.Add(this.CB_AllowEnergy);
            this.GB_LikeADJSettings.Controls.Add(this.CB_DisableLogging);
            this.GB_LikeADJSettings.Controls.Add(this.TB_BeatDetectionEvery);
            this.GB_LikeADJSettings.Controls.Add(this.LB_BeatDetectionEvery);
            this.GB_LikeADJSettings.Controls.Add(this.LB_Lights);
            this.GB_LikeADJSettings.Controls.Add(this.LV_Lights);
            this.GB_LikeADJSettings.Controls.Add(this.CB_ChangeLightsWhen);
            this.GB_LikeADJSettings.Controls.Add(this.LB_ChangeLightsWhen);
            this.GB_LikeADJSettings.Controls.Add(this.BT_ScanLights);
            this.GB_LikeADJSettings.Controls.Add(this.BT_ResetHue);
            this.GB_LikeADJSettings.Controls.Add(this.BT_PairHue);
            this.GB_LikeADJSettings.Controls.Add(this.lblBridgeCnx);
            this.GB_LikeADJSettings.Controls.Add(this.CB_AllowHue);
            this.GB_LikeADJSettings.Controls.Add(this.TB_MinRatings);
            this.GB_LikeADJSettings.Controls.Add(this.LB_MinimumRating);
            this.GB_LikeADJSettings.Controls.Add(this.LB_MaximumDiffBPM);
            this.GB_LikeADJSettings.Controls.Add(this.TB_DiffBPM);
            this.GB_LikeADJSettings.Controls.Add(this.CB_AllowRatings);
            this.GB_LikeADJSettings.Controls.Add(this.CB_AllowInitialKey);
            this.GB_LikeADJSettings.Controls.Add(this.CB_AllowBPM);
            this.GB_LikeADJSettings.Location = new System.Drawing.Point(3, 83);
            this.GB_LikeADJSettings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.GB_LikeADJSettings.Name = "GB_LikeADJSettings";
            this.GB_LikeADJSettings.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.GB_LikeADJSettings.Size = new System.Drawing.Size(791, 532);
            this.GB_LikeADJSettings.TabIndex = 25;
            this.GB_LikeADJSettings.TabStop = false;
            this.GB_LikeADJSettings.Text = "LikeADJ settings :";
            // 
            // CB_AllowScanningMessageBox
            // 
            this.CB_AllowScanningMessageBox.AutoSize = true;
            this.CB_AllowScanningMessageBox.Location = new System.Drawing.Point(27, 306);
            this.CB_AllowScanningMessageBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_AllowScanningMessageBox.Name = "CB_AllowScanningMessageBox";
            this.CB_AllowScanningMessageBox.Size = new System.Drawing.Size(180, 17);
            this.CB_AllowScanningMessageBox.TabIndex = 47;
            this.CB_AllowScanningMessageBox.Text = "Allow the scanning message box";
            this.CB_AllowScanningMessageBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(200, 259);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(263, 13);
            this.label2.TabIndex = 46;
            this.label2.Text = "(available only if one or more features checked above)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(229, 214);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 13);
            this.label1.TabIndex = 45;
            this.label1.Text = "(SHIFT+DEL: Check all or DEL: Uncheck All)";
            // 
            // TB_NumberSongsPlaylist
            // 
            this.TB_NumberSongsPlaylist.Location = new System.Drawing.Point(293, 277);
            this.TB_NumberSongsPlaylist.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TB_NumberSongsPlaylist.Name = "TB_NumberSongsPlaylist";
            this.TB_NumberSongsPlaylist.Size = new System.Drawing.Size(38, 20);
            this.TB_NumberSongsPlaylist.TabIndex = 44;
            this.TB_NumberSongsPlaylist.Text = "10";
            this.TB_NumberSongsPlaylist.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LB_NumberSongsPlaylist
            // 
            this.LB_NumberSongsPlaylist.AutoSize = true;
            this.LB_NumberSongsPlaylist.Location = new System.Drawing.Point(53, 280);
            this.LB_NumberSongsPlaylist.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_NumberSongsPlaylist.Name = "LB_NumberSongsPlaylist";
            this.LB_NumberSongsPlaylist.Size = new System.Drawing.Size(232, 13);
            this.LB_NumberSongsPlaylist.TabIndex = 43;
            this.LB_NumberSongsPlaylist.Text = "Number of songs to add into a LikeADJ playlist :";
            // 
            // CB_SaveSongsPlaylist
            // 
            this.CB_SaveSongsPlaylist.AutoSize = true;
            this.CB_SaveSongsPlaylist.Location = new System.Drawing.Point(27, 258);
            this.CB_SaveSongsPlaylist.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_SaveSongsPlaylist.Name = "CB_SaveSongsPlaylist";
            this.CB_SaveSongsPlaylist.Size = new System.Drawing.Size(179, 17);
            this.CB_SaveSongsPlaylist.TabIndex = 42;
            this.CB_SaveSongsPlaylist.Text = "Save songs played into a playlist";
            this.CB_SaveSongsPlaylist.UseVisualStyleBackColor = true;
            // 
            // LB_BrightnessLightsRange
            // 
            this.LB_BrightnessLightsRange.AutoSize = true;
            this.LB_BrightnessLightsRange.Location = new System.Drawing.Point(483, 475);
            this.LB_BrightnessLightsRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_BrightnessLightsRange.Name = "LB_BrightnessLightsRange";
            this.LB_BrightnessLightsRange.Size = new System.Drawing.Size(223, 13);
            this.LB_BrightnessLightsRange.TabIndex = 38;
            this.LB_BrightnessLightsRange.Text = "Brightness range of lights (on beat detection) :";
            // 
            // RS_BrightnessLightsRange
            // 
            this.RS_BrightnessLightsRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RS_BrightnessLightsRange.Location = new System.Drawing.Point(485, 495);
            this.RS_BrightnessLightsRange.Max = 255;
            this.RS_BrightnessLightsRange.Min = 0;
            this.RS_BrightnessLightsRange.Name = "RS_BrightnessLightsRange";
            this.RS_BrightnessLightsRange.SelectedMax = 255;
            this.RS_BrightnessLightsRange.SelectedMin = 0;
            this.RS_BrightnessLightsRange.Size = new System.Drawing.Size(287, 20);
            this.RS_BrightnessLightsRange.TabIndex = 37;
            this.RS_BrightnessLightsRange.Value = 50;
            // 
            // LB_SelectGenres
            // 
            this.LB_SelectGenres.AutoSize = true;
            this.LB_SelectGenres.Location = new System.Drawing.Point(53, 232);
            this.LB_SelectGenres.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_SelectGenres.Name = "LB_SelectGenres";
            this.LB_SelectGenres.Size = new System.Drawing.Size(80, 13);
            this.LB_SelectGenres.TabIndex = 36;
            this.LB_SelectGenres.Text = "Select Genres :";
            // 
            // CB_AllowGenres
            // 
            this.CB_AllowGenres.AutoSize = true;
            this.CB_AllowGenres.Location = new System.Drawing.Point(27, 210);
            this.CB_AllowGenres.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_AllowGenres.Name = "CB_AllowGenres";
            this.CB_AllowGenres.Size = new System.Drawing.Size(194, 17);
            this.CB_AllowGenres.TabIndex = 35;
            this.CB_AllowGenres.Text = "Allow Auto Mix according to Genres";
            this.CB_AllowGenres.UseVisualStyleBackColor = true;
            // 
            // CCB_Genres
            // 
            this.CCB_Genres.CausesValidation = false;
            this.CCB_Genres.CheckOnClick = true;
            this.CCB_Genres.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.CCB_Genres.DropDownHeight = 1;
            this.CCB_Genres.FormattingEnabled = true;
            this.CCB_Genres.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CCB_Genres.IntegralHeight = false;
            this.CCB_Genres.Location = new System.Drawing.Point(142, 229);
            this.CCB_Genres.Name = "CCB_Genres";
            this.CCB_Genres.Size = new System.Drawing.Size(306, 21);
            this.CCB_Genres.TabIndex = 34;
            this.CCB_Genres.ValueSeparator = ",";
            // 
            // TB_MinEnergy
            // 
            this.TB_MinEnergy.FormattingEnabled = true;
            this.TB_MinEnergy.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.TB_MinEnergy.Location = new System.Drawing.Point(144, 129);
            this.TB_MinEnergy.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TB_MinEnergy.Name = "TB_MinEnergy";
            this.TB_MinEnergy.Size = new System.Drawing.Size(49, 21);
            this.TB_MinEnergy.TabIndex = 33;
            this.TB_MinEnergy.Text = "6";
            // 
            // LB_MinimumEnergy
            // 
            this.LB_MinimumEnergy.AutoSize = true;
            this.LB_MinimumEnergy.Location = new System.Drawing.Point(53, 132);
            this.LB_MinimumEnergy.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_MinimumEnergy.Name = "LB_MinimumEnergy";
            this.LB_MinimumEnergy.Size = new System.Drawing.Size(89, 13);
            this.LB_MinimumEnergy.TabIndex = 32;
            this.LB_MinimumEnergy.Text = "Minimum energy :";
            // 
            // CB_AllowEnergy
            // 
            this.CB_AllowEnergy.AutoSize = true;
            this.CB_AllowEnergy.Location = new System.Drawing.Point(27, 110);
            this.CB_AllowEnergy.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_AllowEnergy.Name = "CB_AllowEnergy";
            this.CB_AllowEnergy.Size = new System.Drawing.Size(193, 17);
            this.CB_AllowEnergy.TabIndex = 31;
            this.CB_AllowEnergy.Text = "Allow Auto Mix according to Energy";
            this.CB_AllowEnergy.UseVisualStyleBackColor = true;
            // 
            // CB_DisableLogging
            // 
            this.CB_DisableLogging.AutoSize = true;
            this.CB_DisableLogging.Location = new System.Drawing.Point(44, 489);
            this.CB_DisableLogging.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_DisableLogging.Name = "CB_DisableLogging";
            this.CB_DisableLogging.Size = new System.Drawing.Size(294, 17);
            this.CB_DisableLogging.TabIndex = 30;
            this.CB_DisableLogging.Text = "Disable logging of beat detection and lights color change";
            this.CB_DisableLogging.UseVisualStyleBackColor = true;
            // 
            // TB_BeatDetectionEvery
            // 
            this.TB_BeatDetectionEvery.Location = new System.Drawing.Point(224, 466);
            this.TB_BeatDetectionEvery.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TB_BeatDetectionEvery.Name = "TB_BeatDetectionEvery";
            this.TB_BeatDetectionEvery.Size = new System.Drawing.Size(38, 20);
            this.TB_BeatDetectionEvery.TabIndex = 29;
            this.TB_BeatDetectionEvery.Text = "500";
            this.TB_BeatDetectionEvery.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LB_BeatDetectionEvery
            // 
            this.LB_BeatDetectionEvery.AutoSize = true;
            this.LB_BeatDetectionEvery.Location = new System.Drawing.Point(40, 467);
            this.LB_BeatDetectionEvery.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_BeatDetectionEvery.Name = "LB_BeatDetectionEvery";
            this.LB_BeatDetectionEvery.Size = new System.Drawing.Size(176, 13);
            this.LB_BeatDetectionEvery.TabIndex = 28;
            this.LB_BeatDetectionEvery.Text = "Beat detection every (milliseconds) :";
            // 
            // LB_Lights
            // 
            this.LB_Lights.AutoSize = true;
            this.LB_Lights.Location = new System.Drawing.Point(482, 18);
            this.LB_Lights.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_Lights.Name = "LB_Lights";
            this.LB_Lights.Size = new System.Drawing.Size(132, 13);
            this.LB_Lights.TabIndex = 27;
            this.LB_Lights.Text = "Select allowed Hue lights :";
            // 
            // LV_Lights
            // 
            this.LV_Lights.CheckBoxes = true;
            this.LV_Lights.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Used,
            this.Number,
            this.Index,
            this.Lights});
            this.LV_Lights.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.LV_Lights.HideSelection = false;
            this.LV_Lights.LabelWrap = false;
            this.LV_Lights.Location = new System.Drawing.Point(485, 34);
            this.LV_Lights.Name = "LV_Lights";
            this.LV_Lights.ShowGroups = false;
            this.LV_Lights.Size = new System.Drawing.Size(286, 430);
            this.LV_Lights.TabIndex = 26;
            this.LV_Lights.UseCompatibleStateImageBehavior = false;
            this.LV_Lights.View = System.Windows.Forms.View.Details;
            // 
            // Used
            // 
            this.Used.Text = "";
            this.Used.Width = 40;
            // 
            // Number
            // 
            this.Number.Text = "N°";
            this.Number.Width = 40;
            // 
            // Index
            // 
            this.Index.Text = "Index";
            // 
            // Lights
            // 
            this.Lights.Text = "Lights";
            this.Lights.Width = 180;
            // 
            // CB_ChangeLightsWhen
            // 
            this.CB_ChangeLightsWhen.FormattingEnabled = true;
            this.CB_ChangeLightsWhen.Items.AddRange(new object[] {
            "Track change",
            "Beat is detected (Simple)",
            "Beat is detected (SubBand)",
            "15s before ending (flashing RED) & Track change"});
            this.CB_ChangeLightsWhen.Location = new System.Drawing.Point(152, 441);
            this.CB_ChangeLightsWhen.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_ChangeLightsWhen.Name = "CB_ChangeLightsWhen";
            this.CB_ChangeLightsWhen.Size = new System.Drawing.Size(281, 21);
            this.CB_ChangeLightsWhen.TabIndex = 25;
            // 
            // LB_ChangeLightsWhen
            // 
            this.LB_ChangeLightsWhen.AutoSize = true;
            this.LB_ChangeLightsWhen.Location = new System.Drawing.Point(42, 444);
            this.LB_ChangeLightsWhen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_ChangeLightsWhen.Name = "LB_ChangeLightsWhen";
            this.LB_ChangeLightsWhen.Size = new System.Drawing.Size(106, 13);
            this.LB_ChangeLightsWhen.TabIndex = 24;
            this.LB_ChangeLightsWhen.Text = "Change lights when :";
            // 
            // BT_ScanLights
            // 
            this.BT_ScanLights.Location = new System.Drawing.Point(214, 405);
            this.BT_ScanLights.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BT_ScanLights.Name = "BT_ScanLights";
            this.BT_ScanLights.Size = new System.Drawing.Size(113, 24);
            this.BT_ScanLights.TabIndex = 23;
            this.BT_ScanLights.Text = "Scan for new lights";
            this.BT_ScanLights.UseVisualStyleBackColor = true;
            this.BT_ScanLights.Click += new System.EventHandler(this.BT_ScanLights_Click);
            // 
            // BT_ResetHue
            // 
            this.BT_ResetHue.Location = new System.Drawing.Point(128, 405);
            this.BT_ResetHue.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BT_ResetHue.Name = "BT_ResetHue";
            this.BT_ResetHue.Size = new System.Drawing.Size(77, 24);
            this.BT_ResetHue.TabIndex = 19;
            this.BT_ResetHue.Text = "Reset Hue";
            this.BT_ResetHue.UseVisualStyleBackColor = true;
            this.BT_ResetHue.Click += new System.EventHandler(this.BT_ResetHue_Click);
            // 
            // BT_PairHue
            // 
            this.BT_PairHue.Location = new System.Drawing.Point(44, 405);
            this.BT_PairHue.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.BT_PairHue.Name = "BT_PairHue";
            this.BT_PairHue.Size = new System.Drawing.Size(76, 24);
            this.BT_PairHue.TabIndex = 18;
            this.BT_PairHue.Text = "Pair";
            this.BT_PairHue.UseVisualStyleBackColor = true;
            this.BT_PairHue.Click += new System.EventHandler(this.BT_PairHue_Click);
            // 
            // lblBridgeCnx
            // 
            this.lblBridgeCnx.AutoSize = true;
            this.lblBridgeCnx.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBridgeCnx.Location = new System.Drawing.Point(42, 360);
            this.lblBridgeCnx.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBridgeCnx.MaximumSize = new System.Drawing.Size(570, 154);
            this.lblBridgeCnx.Name = "lblBridgeCnx";
            this.lblBridgeCnx.Size = new System.Drawing.Size(97, 13);
            this.lblBridgeCnx.TabIndex = 17;
            this.lblBridgeCnx.Text = "Hue bridge status";
            this.lblBridgeCnx.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CB_AllowHue
            // 
            this.CB_AllowHue.AutoSize = true;
            this.CB_AllowHue.Location = new System.Drawing.Point(26, 338);
            this.CB_AllowHue.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_AllowHue.Name = "CB_AllowHue";
            this.CB_AllowHue.Size = new System.Drawing.Size(423, 17);
            this.CB_AllowHue.TabIndex = 16;
            this.CB_AllowHue.Text = "Allow Hue Lighting (latest firmwares for your Hue bridge and your Hue lights requ" +
    "ired)";
            this.CB_AllowHue.UseVisualStyleBackColor = true;
            this.CB_AllowHue.CheckStateChanged += new System.EventHandler(this.CB_AllowHue_CheckStateChanged);
            // 
            // TB_MinRatings
            // 
            this.TB_MinRatings.FormattingEnabled = true;
            this.TB_MinRatings.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.TB_MinRatings.Location = new System.Drawing.Point(144, 179);
            this.TB_MinRatings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TB_MinRatings.Name = "TB_MinRatings";
            this.TB_MinRatings.Size = new System.Drawing.Size(49, 21);
            this.TB_MinRatings.TabIndex = 15;
            this.TB_MinRatings.Text = "3";
            // 
            // LB_MinimumRating
            // 
            this.LB_MinimumRating.AutoSize = true;
            this.LB_MinimumRating.Location = new System.Drawing.Point(53, 182);
            this.LB_MinimumRating.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_MinimumRating.Name = "LB_MinimumRating";
            this.LB_MinimumRating.Size = new System.Drawing.Size(83, 13);
            this.LB_MinimumRating.TabIndex = 14;
            this.LB_MinimumRating.Text = "Minimum rating :";
            // 
            // LB_MaximumDiffBPM
            // 
            this.LB_MaximumDiffBPM.AutoSize = true;
            this.LB_MaximumDiffBPM.Location = new System.Drawing.Point(53, 56);
            this.LB_MaximumDiffBPM.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LB_MaximumDiffBPM.Name = "LB_MaximumDiffBPM";
            this.LB_MaximumDiffBPM.Size = new System.Drawing.Size(145, 13);
            this.LB_MaximumDiffBPM.TabIndex = 13;
            this.LB_MaximumDiffBPM.Text = "Maximum difference of BPM :";
            // 
            // TB_DiffBPM
            // 
            this.TB_DiffBPM.Location = new System.Drawing.Point(203, 53);
            this.TB_DiffBPM.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TB_DiffBPM.Name = "TB_DiffBPM";
            this.TB_DiffBPM.Size = new System.Drawing.Size(38, 20);
            this.TB_DiffBPM.TabIndex = 12;
            this.TB_DiffBPM.Text = "15";
            this.TB_DiffBPM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CB_AllowRatings
            // 
            this.CB_AllowRatings.AutoSize = true;
            this.CB_AllowRatings.Location = new System.Drawing.Point(27, 160);
            this.CB_AllowRatings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_AllowRatings.Name = "CB_AllowRatings";
            this.CB_AllowRatings.Size = new System.Drawing.Size(196, 17);
            this.CB_AllowRatings.TabIndex = 11;
            this.CB_AllowRatings.Text = "Allow Auto Mix according to Ratings";
            this.CB_AllowRatings.UseVisualStyleBackColor = true;
            // 
            // CB_AllowInitialKey
            // 
            this.CB_AllowInitialKey.AutoSize = true;
            this.CB_AllowInitialKey.Location = new System.Drawing.Point(27, 83);
            this.CB_AllowInitialKey.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_AllowInitialKey.Name = "CB_AllowInitialKey";
            this.CB_AllowInitialKey.Size = new System.Drawing.Size(404, 17);
            this.CB_AllowInitialKey.TabIndex = 10;
            this.CB_AllowInitialKey.Text = "Allow Auto Mix according to \'Initial Key\' (Camelot and/or Open Key notation only)" +
    "";
            this.CB_AllowInitialKey.UseVisualStyleBackColor = true;
            // 
            // CB_AllowBPM
            // 
            this.CB_AllowBPM.AutoSize = true;
            this.CB_AllowBPM.Location = new System.Drawing.Point(27, 34);
            this.CB_AllowBPM.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CB_AllowBPM.Name = "CB_AllowBPM";
            this.CB_AllowBPM.Size = new System.Drawing.Size(183, 17);
            this.CB_AllowBPM.TabIndex = 9;
            this.CB_AllowBPM.Text = "Allow Auto Mix according to BPM";
            this.CB_AllowBPM.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 49);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(791, 24);
            this.textBox1.TabIndex = 24;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "When the plugin is activated & configured, shuffle your entire collection and the" +
    " next song will be auto mixed following your choices below and so on...";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(94, 22);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(605, 13);
            this.label7.TabIndex = 23;
            this.label7.Text = "In order to be fully fonctionnal, \'BPM\', \'Initial Key\', \'Energy\', \'Track Rating\' " +
    "and \'Genre\' tags must be filled";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(116, 9);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(561, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Auto Mix your songs according to BPM, Initial Key (Camelot and Open Key), Energy," +
    " Ratings, Genres with Hue ligthing";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LL_LikeADJLog
            // 
            this.LL_LikeADJLog.AutoSize = true;
            this.LL_LikeADJLog.Location = new System.Drawing.Point(444, 628);
            this.LL_LikeADJLog.Name = "LL_LikeADJLog";
            this.LL_LikeADJLog.Size = new System.Drawing.Size(84, 13);
            this.LL_LikeADJLog.TabIndex = 29;
            this.LL_LikeADJLog.TabStop = true;
            this.LL_LikeADJLog.Text = "mb_LikeADJ.log";
            this.LL_LikeADJLog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LL_LikeADJLog_LinkClicked);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(796, 692);
            this.ControlBox = false;
            this.Controls.Add(this.LL_LikeADJLog);
            this.Controls.Add(this.LB_SeeLogFile);
            this.Controls.Add(this.BT_Cancel);
            this.Controls.Add(this.BT_Save);
            this.Controls.Add(this.GB_LikeADJSettings);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.GB_LikeADJSettings.ResumeLayout(false);
            this.GB_LikeADJSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LB_SeeLogFile;
        private System.Windows.Forms.Button BT_Cancel;
        private System.Windows.Forms.Button BT_Save;
        private System.Windows.Forms.GroupBox GB_LikeADJSettings;
        private System.Windows.Forms.Button BT_ScanLights;
        private System.Windows.Forms.Button BT_ResetHue;
        private System.Windows.Forms.Button BT_PairHue;
        private System.Windows.Forms.Label lblBridgeCnx;
        private System.Windows.Forms.CheckBox CB_AllowHue;
        private System.Windows.Forms.ComboBox TB_MinRatings;
        private System.Windows.Forms.Label LB_MinimumRating;
        private System.Windows.Forms.Label LB_MaximumDiffBPM;
        private System.Windows.Forms.TextBox TB_DiffBPM;
        private System.Windows.Forms.CheckBox CB_AllowRatings;
        private System.Windows.Forms.CheckBox CB_AllowInitialKey;
        private System.Windows.Forms.CheckBox CB_AllowBPM;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox CB_ChangeLightsWhen;
        private System.Windows.Forms.Label LB_ChangeLightsWhen;
        private System.Windows.Forms.ListView LV_Lights;
        private System.Windows.Forms.ColumnHeader Used;
        private System.Windows.Forms.ColumnHeader Number;
        private System.Windows.Forms.ColumnHeader Lights;
        private System.Windows.Forms.ColumnHeader Index;
        private System.Windows.Forms.Label LB_Lights;
        private System.Windows.Forms.TextBox TB_BeatDetectionEvery;
        private System.Windows.Forms.Label LB_BeatDetectionEvery;
        private System.Windows.Forms.CheckBox CB_DisableLogging;
        private System.Windows.Forms.ComboBox TB_MinEnergy;
        private System.Windows.Forms.Label LB_MinimumEnergy;
        private System.Windows.Forms.CheckBox CB_AllowEnergy;
        private System.Windows.Forms.Label LB_SelectGenres;
        private System.Windows.Forms.CheckBox CB_AllowGenres;
        private MusicBeePlugin.CheckedComboBox CCB_Genres;
        private SelectionRangeSlider RS_BrightnessLightsRange;
        private System.Windows.Forms.Label LB_BrightnessLightsRange;
        private System.Windows.Forms.TextBox TB_NumberSongsPlaylist;
        private System.Windows.Forms.Label LB_NumberSongsPlaylist;
        private System.Windows.Forms.CheckBox CB_SaveSongsPlaylist;
        private System.Windows.Forms.LinkLabel LL_LikeADJLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox CB_AllowScanningMessageBox;
    }
}