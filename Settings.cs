using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace MusicBeePlugin
{
    public partial class Settings : Form
    {
        public string bridgeXmlPath;
        public JavaScriptSerializer jss = new JavaScriptSerializer();
        public List<JSONBridge> bridges;
        private bool isFormLoading = true;
        private bool alreadypopulated = false;
        private bool isRescanLights = false;
        public string[] genresdistinct = { };

        public Settings()
        {
            InitializeComponent();
            this.Text = "LikeADJ plugin [ DJC👽D - " + Plugin.LikeADJVersion + " ] " + "[MusicBee " + Application.ProductVersion + "]";
            if (Plugin.MusicBeeisportable) bridgeXmlPath = Application.StartupPath + "\\Plugins\\mb_LikeADJ.xml";
            else bridgeXmlPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.xml";
        }

        public void Settings_Load(object sender, EventArgs e)
        {
            Plugin.Logger.Info("Entering settings...");

            Plugin.Logger.Info("Scanning genres from the entire library started...");
            string[] tracks = { };
            Plugin.mbApiInterface.Library_QueryFilesEx("", out tracks);
            string[] genres = new string[tracks.Length];
            for (int i = 0; i < tracks.Length; i++) genres[i] = Plugin.mbApiInterface.Library_GetFileTag(tracks[i], Plugin.MetaDataType.Genre);
            genresdistinct = genres.Distinct().ToArray();
            Array.Sort(genresdistinct);
            Plugin.Logger.Info("Scanning genres from the entire library finished.");

            Plugin.Check_Custom_Key();

            if (File.Exists(Plugin.LikeADJIniFile))
            { 
                Boolean.TryParse(Plugin.ini.Read("ALLOWBPM", "BPM"), out bool flag);
                CB_AllowBPM.Checked = flag;
                TB_DiffBPM.Text = Plugin.ini.Read("DIFFBPM", "BPM");

                Boolean.TryParse(Plugin.ini.Read("ALLOWHARMONICKEY", "HARMONICKEY"), out flag);
                CB_AllowInitialKey.Checked = flag;

                if (!Plugin.foundmetadatatypekey)
                {
                    CB_AllowInitialKey.Enabled = false;
                    CB_AllowInitialKey.Checked = false;
                    LB_CustomTagKey.Text = "No Custom Tag Key found [Feature disabled]";
                    LB_CustomTagKey.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    LB_CustomTagKey.Text = "Custom Tag Key found [" + Plugin.MetaDataTypeKey + "]";
                    LB_CustomTagKey.ForeColor = System.Drawing.Color.Green; 
                }

                Boolean.TryParse(Plugin.ini.Read("ALLOWENERGY", "ENERGY"), out flag);
                CB_AllowEnergy.Checked = flag;
                TB_MinEnergy.Text = Plugin.ini.Read("MINENERGY", "ENERGY");

                if (!Plugin.foundmetadatatypeenergy)
                {
                    CB_AllowEnergy.Enabled = false;
                    CB_AllowEnergy.Checked = false;
                    LB_MinimumEnergy.Enabled = false;
                    TB_MinEnergy.Enabled = false;
                    LB_CustomTagEnergy.Text = "No Custom Tag Energy found [Feature disabled]";
                    LB_CustomTagEnergy.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    LB_CustomTagEnergy.ForeColor = System.Drawing.Color.Green;
                    LB_CustomTagEnergy.Text = "Custom Tag Energy found [" + Plugin.MetaDataTypeEnergy + "]";
                }

                Boolean.TryParse(Plugin.ini.Read("ALLOWRATINGS", "RATINGS"), out flag);
                CB_AllowRatings.Checked = flag;
                TB_MinRatings.Text = Plugin.ini.Read("MINRATINGS", "RATINGS");
                Boolean.TryParse(Plugin.ini.Read("ALLOWGENRES", "GENRES"), out flag);
                CB_AllowGenres.Checked = flag;
                CCB_Genres.Text = Plugin.ini.Read("GENRESSELECTED", "GENRES");
                Plugin.genresAllowed = CCB_Genres.Text.Split(',').ToArray();

                if (CB_AllowBPM.Checked || CB_AllowInitialKey.Checked || CB_AllowEnergy.Checked || CB_AllowRatings.Checked || CB_AllowGenres.Checked)
                {
                    CB_SaveSongsPlaylist.Enabled = true;
                    LB_NumberSongsPlaylist.Enabled = true;
                    TB_NumberSongsPlaylist.Enabled = true;

                    Boolean.TryParse(Plugin.ini.Read("SAVESONGSPLAYLIST", "PLAYLIST"), out flag);
                    CB_SaveSongsPlaylist.Checked = flag;
                    TB_NumberSongsPlaylist.Text = Plugin.ini.Read("NUMBERSONGSPLAYLIST", "PLAYLIST");
                    Boolean.TryParse(Plugin.ini.Read("ALLOWSCANNINGMESSAGEBOX", "GENERAL"), out flag);
                    CB_AllowScanningMessageBox.Checked = flag;
                }
                else 
                {
                    CB_SaveSongsPlaylist.Checked = false;
                    CB_SaveSongsPlaylist.Enabled = false;
                    LB_NumberSongsPlaylist.Enabled = false;
                    TB_NumberSongsPlaylist.Enabled = false;
                }

                Boolean.TryParse(Plugin.ini.Read("ALLOWHUE", "HUE"), out flag);
                CB_AllowHue.Checked = flag;
                Plugin.APIKey = Plugin.ini.Read("APIKEY", "HUE");
                CB_ChangeLightsWhen.Text = Plugin.ini.Read("CHANGELIGHTSWHEN", "HUE");
                string lightallowed = Plugin.ini.Read("LIGHTSALLOWED", "HUE");
                Plugin.lightIndicesAllowed = lightallowed.Split(',').Select(s => int.TryParse(s, out int n) ? n : 0).ToArray();
                int.TryParse(Plugin.ini.Read("BRIGHTNESSLIGHTSMIN", "HUE"), out int flag2);
                RS_BrightnessLightsRange.SelectedMin = flag2;
                int.TryParse(Plugin.ini.Read("BRIGHTNESSLIGHTSMAX", "HUE"), out flag2);
                RS_BrightnessLightsRange.SelectedMax = flag2;
                TB_BeatDetectionEvery.Text = Plugin.ini.Read("BEATDETECTIONEVERY", "HUE");
                Boolean.TryParse(Plugin.ini.Read("DISABLELOGGING", "HUE"), out flag);
                CB_DisableLogging.Checked = flag;

                BT_PairHue.Visible = false;
                BT_ResetHue.Visible = false;
                BT_ScanLights.Visible = false;
                LB_ChangeLightsWhen.Visible = false;
                CB_ChangeLightsWhen.Visible = false;
                LB_Lights.Visible = false;
                LV_Lights.Visible = false;
                LB_BrightnessLightsRange.Visible = false;
                RS_BrightnessLightsRange.Visible = false;
                LB_BeatDetectionEvery.Visible = false;
                TB_BeatDetectionEvery.Visible = false;
                CB_DisableLogging.Visible = false;

                if (CB_AllowHue.Checked)
                {
                    lblBridgeCnx.Visible = true;

                    if (CheckForExistingKey()) { InitBridge(); }
                    else { FindBridges(); }
                }
                else { lblBridgeCnx.Visible = false; }

                CCB_Genres.MaxDropDownItems = 10;
                CCB_Genres.DisplayMember = "Name";
                CCB_Genres.ValueSeparator = ",";

                for (int i = 0; i < genresdistinct.Length; i++)
                {
                    CCBoxItem item = new CCBoxItem(genresdistinct[i], i);
                    CCB_Genres.Items.Add(item);
                    bool exist = Array.Exists(Plugin.genresAllowed, element => element == genresdistinct[i]);
                    if (exist) CCB_Genres.SetItemChecked(i, true);
                }
            }
            else
            {
                if (Plugin.MusicBeeisportable) Plugin.Logger.Info("No ini file " + Application.StartupPath + "\\Plugins\\mb_LikeADJ.ini found. Creating a new one...");
                else Plugin.Logger.Info("No ini file " + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.ini found. Creating a new one...");

                TB_DiffBPM.Text = "10";
                TB_MinEnergy.Text = "6";
                TB_MinRatings.Text = "4";
                TB_NumberSongsPlaylist.Text = "20";
                TB_BeatDetectionEvery.Text = "200";
                CB_AllowScanningMessageBox.Checked = false;
                CB_AllowHue.Checked = false;
                lblBridgeCnx.Visible = false;
                BT_PairHue.Visible = false;
                BT_ResetHue.Visible = false;
                BT_ScanLights.Visible = false;
                LB_ChangeLightsWhen.Visible = false;
                CB_ChangeLightsWhen.Visible = false;
                LB_Lights.Visible = false;
                LV_Lights.Visible = false;
                LB_BrightnessLightsRange.Visible = false;
                RS_BrightnessLightsRange.Visible = false;
                LB_BeatDetectionEvery.Visible = false;
                TB_BeatDetectionEvery.Visible = false;
                CB_DisableLogging.Visible = false;
                CB_SaveSongsPlaylist.Checked = false;
                CB_SaveSongsPlaylist.Enabled = false;
                LB_NumberSongsPlaylist.Enabled = false;
                TB_NumberSongsPlaylist.Enabled = false;

                CCB_Genres.MaxDropDownItems = 10;
                CCB_Genres.DisplayMember = "Name";
                CCB_Genres.ValueSeparator = ",";

                for (int i = 0; i < genresdistinct.Length; i++)
                {
                    CCBoxItem item = new CCBoxItem(genresdistinct[i], i);
                    CCB_Genres.Items.Add(item);
                }

                if (!Plugin.foundmetadatatypekey)
                {
                    CB_AllowInitialKey.Enabled = false;
                    CB_AllowInitialKey.Checked = false;
                    LB_CustomTagKey.Text = "No Custom Tag Key found [Feature disabled]";
                    LB_CustomTagKey.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    LB_CustomTagKey.Text = "Custom Tag Key found [" + Plugin.MetaDataTypeKey + "]";
                    LB_CustomTagKey.ForeColor = System.Drawing.Color.Green;
                }

                if (!Plugin.foundmetadatatypeenergy)
                {
                    CB_AllowEnergy.Enabled = false;
                    CB_AllowEnergy.Checked = false;
                    LB_MinimumEnergy.Enabled = false;
                    TB_MinEnergy.Enabled = false;
                    LB_CustomTagEnergy.Text = "No Custom Tag Energy found [Feature disabled]";
                    LB_CustomTagEnergy.ForeColor = System.Drawing.Color.Red;
                }
                else
                {
                    LB_CustomTagEnergy.ForeColor = System.Drawing.Color.Green;
                    LB_CustomTagEnergy.Text = "Custom Tag Energy found [" + Plugin.MetaDataTypeEnergy + "]";
                }
            }

            isFormLoading = false;
        }

        private void BT_Cancel_Click(object sender, EventArgs e)
        {
            Plugin.Logger.Info("Closing settings without saving...");
            Close();
        }

        private void BT_OK_Click(object sender, EventArgs e)
        {
            Plugin.Logger.Info("Saving settings...");
            Plugin.ini.Write("ALLOWBPM", CB_AllowBPM.Checked.ToString(), "BPM");
            Plugin.ini.Write("DIFFBPM", TB_DiffBPM.Text, "BPM");
            Plugin.ini.Write("ALLOWHARMONICKEY", CB_AllowInitialKey.Checked.ToString(), "HARMONICKEY");
            Plugin.ini.Write("ALLOWENERGY", CB_AllowEnergy.Checked.ToString(), "ENERGY");
            Plugin.ini.Write("MINENERGY", TB_MinEnergy.Text, "ENERGY");
            Plugin.ini.Write("ALLOWRATINGS", CB_AllowRatings.Checked.ToString(), "RATINGS");
            Plugin.ini.Write("MINRATINGS", TB_MinRatings.Text, "RATINGS");
            Plugin.ini.Write("ALLOWGENRES", CB_AllowGenres.Checked.ToString(), "GENRES");
            Plugin.ini.Write("GENRESSELECTED", CCB_Genres.Text, "GENRES");
            Plugin.ini.Write("SAVESONGSPLAYLIST", CB_SaveSongsPlaylist.Checked.ToString(), "PLAYLIST");
            Plugin.ini.Write("NUMBERSONGSPLAYLIST", TB_NumberSongsPlaylist.Text, "PLAYLIST");
            Plugin.ini.Write("ALLOWSCANNINGMESSAGEBOX", CB_AllowScanningMessageBox.Checked.ToString(), "GENERAL");
            Plugin.ini.Write("ALLOWHUE", CB_AllowHue.Checked.ToString(), "HUE");
            Plugin.ini.Write("APIKEY", Plugin.APIKey, "HUE");
            Plugin.ini.Write("CHANGELIGHTSWHEN", CB_ChangeLightsWhen.Text, "HUE");
            int j = 0;
            Array.Clear(Plugin.lightIndices, 0, Plugin.lightIndices.Length);
            for (int i = 0; i < LV_Lights.Items.Count; i++)
            {
                if (LV_Lights.Items[i].Checked)
                {
                    Plugin.lightIndices[j] = int.Parse(LV_Lights.Items[i].SubItems[2].Text);
                    j++;
                }
            }
            Plugin.ini.Write("LIGHTSALLOWED", string.Join(",", Plugin.lightIndices), "HUE");
            Plugin.ini.Write("BRIGHTNESSLIGHTSMIN", RS_BrightnessLightsRange.SelectedMin.ToString(), "HUE");
            Plugin.ini.Write("BRIGHTNESSLIGHTSMAX", RS_BrightnessLightsRange.SelectedMax.ToString(), "HUE");
            Plugin.ini.Write("BEATDETECTIONEVERY", TB_BeatDetectionEvery.Text, "HUE");
            Plugin.ini.Write("DISABLELOGGING", CB_DisableLogging.Checked.ToString(), "HUE");

            Plugin.isSettingsChanged = true;
            Plugin.LoadSettings();
            Close();
        }

        private void FindBridges()
        {
            Plugin.Logger.Info("Scanning to find Hue bridge...");
            lblBridgeCnx.Text = "Scanning to find Hue bridge...";

            WebClient client = new WebClient();
            client.DownloadStringCompleted += Client_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("https://discovery.meethue.com"), null);
            client.Dispose();
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                bridges = jss.Deserialize<List<JSONBridge>>(e.Result);

                WebClient client = new WebClient();
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                string descriptionURL = "http://" + bridges[0].Internalipaddress + "/description.xml";
                client.DownloadFileAsync(new Uri(descriptionURL), bridgeXmlPath);
                BT_PairHue.Visible = true;
                lblBridgeCnx.Visible = true;
                lblBridgeCnx.Text = "Hue bridge found :)\nClick on the button on your hue and after to 'Pair' button";
                client.Dispose();
            }
            catch { lblBridgeCnx.Text = "No Hue bridge found :( !!!\nPlease check you are connected to your network."; }
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (CheckForExistingKey()) { InitBridge(); }
        }

        private bool CheckForExistingKey()
        {
            Plugin.APIKey = Plugin.ini.Read("APIKEY", "HUE");
            if (Plugin.APIKey != string.Empty)
            {
                Plugin.Logger.Info("HUE APIKey found : " + Plugin.APIKey);
                return true;
            }

            Plugin.Logger.Info("No HUE APIKey found !");
            return false;
        }

        public void InitBridge()
        {
            if (Plugin.MusicBeeisportable) bridgeXmlPath = Application.StartupPath + "\\Plugins\\mb_LikeADJ.xml";
            else bridgeXmlPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.xml";

            XmlSerializer bridgeGetter = new XmlSerializer(typeof(Hue));
            TextReader bridgeXmlReader = new StreamReader(bridgeXmlPath);
            Plugin.theHueBridge = (Hue)bridgeGetter.Deserialize(bridgeXmlReader);
            bridgeXmlReader.Close();

            if (Plugin.theHueBridge == null)
            {
                MessageBox.Show("No Hue bridges found !!!\nPlease check if you are connected to the same network.", "Scan Failed", MessageBoxButtons.OK);
                return;
            }
       
            if (!alreadypopulated) PopulateBridgeLights();
        }

        public async void PopulateBridgeLights()
        {
            try
            {
                BT_PairHue.Visible = false;
                BT_ResetHue.Visible = false;
                BT_ScanLights.Visible = false;
                LB_ChangeLightsWhen.Visible = false;
                CB_ChangeLightsWhen.Visible = false;
                LB_Lights.Visible = false;
                LV_Lights.Visible = false;
                LB_BrightnessLightsRange.Visible = false;
                RS_BrightnessLightsRange.Visible = false;
                LB_BeatDetectionEvery.Visible = false;
                TB_BeatDetectionEvery.Visible = false;
                CB_DisableLogging.Visible = false;
                lblBridgeCnx.Visible = true;

                if (isRescanLights)
                {
                    lblBridgeCnx.Text = "Rescanning Hue lights paired with your Hue bridge...";
                    Plugin.Logger.Info("Rescanning Hue lights paired with your Hue bridge...");
                    isRescanLights = false;
                }
                else
                { 
                    lblBridgeCnx.Text = "Scanning Hue lights paired with your Hue bridge...";
                    Plugin.Logger.Info("Scanning Hue lights paired with your Hue bridge...");
                }

                string endpoint = Plugin.theHueBridge.BridgeURLBase.Replace("http://", "").Replace(@":80/", "");
                REST r = new REST(endpoint);

                string result = await r.GET("/api/" + Plugin.APIKey + "/lights");
                if (result == null || result.Contains("error") || result == "" || result == "{}")
                {
                    MessageBox.Show("Error", "You have not paired your lights with the Hue bridge.\nPlease pair your lights before with official hue application for Android or IOS.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                JObject lightObj = JObject.Parse(result);
                IList<JToken> idxObj = lightObj.Children().ToList();
                int j = 0;
                foreach (var pair in lightObj)
                {
                    Int32.TryParse(pair.Key, out Plugin.lightIndices[j]);
                    j++;
                }
                IList<JToken> allPaired = idxObj.Children().ToList();
                Plugin.allLights = new HueLight[allPaired.Count];

                int i = 0;
                foreach (JToken jt in allPaired)
                {
                    ListViewItem item = new ListViewItem();
                    Plugin.allLights[i] = JsonConvert.DeserializeObject<HueLight>(jt.ToString());
                    item.SubItems.Add((i + 1).ToString());
                    item.SubItems.Add(Plugin.lightIndices[i].ToString());
                    bool exist = Array.Exists(Plugin.lightIndicesAllowed, element => element == Plugin.lightIndices[i]);
                    if (exist) item.Checked = true;
                    item.SubItems.Add(Plugin.allLights[i].Name);
                    LV_Lights.Items.Add(item);
                    Plugin.Logger.Info("Found light : " + (i + 1) + " - Index : " + Plugin.lightIndices[i].ToString() + " - " + Plugin.allLights[i].Name + " [Type : " + Plugin.allLights[i].Type + " - Model : " + Plugin.allLights[i].Modelid + " - Firmware : " + Plugin.allLights[i].Swversion + "]");
                    i++;
                }

                BT_PairHue.Visible = false;
                BT_ResetHue.Visible = true;
                BT_ScanLights.Visible = true;
                LB_ChangeLightsWhen.Visible = true;
                CB_ChangeLightsWhen.Visible = true;
                LB_Lights.Visible = true;
                LV_Lights.Visible = true;
                LB_BrightnessLightsRange.Visible = true;
                RS_BrightnessLightsRange.Visible = true;
                LB_BeatDetectionEvery.Visible = true;
                TB_BeatDetectionEvery.Visible = true;
                CB_DisableLogging.Visible = true;
                lblBridgeCnx.Visible = true;

                alreadypopulated = true;

                Plugin.Logger.Info("Connected to bridge : " + Plugin.theHueBridge.BridgeDeviceSpec.BridgeFriendlyName + " - Found " + i + " lights");
                lblBridgeCnx.Text = "Connected to bridge : " + Plugin.theHueBridge.BridgeDeviceSpec.BridgeFriendlyName + "\nFound " + i + " lights";
            }
            catch
            {
                BT_PairHue.Visible = false;
                BT_ResetHue.Visible = true;
                BT_ScanLights.Visible = false;
                LB_ChangeLightsWhen.Visible = false;
                CB_ChangeLightsWhen.Visible = false;
                LB_Lights.Visible = false;
                LV_Lights.Visible = false;
                LB_BrightnessLightsRange.Visible = false;
                RS_BrightnessLightsRange.Visible = false;
                LB_BeatDetectionEvery.Visible = false;
                TB_BeatDetectionEvery.Visible = false;
                CB_DisableLogging.Visible = false;
                lblBridgeCnx.Visible = false;
                Plugin.Logger.Info("No Hue bridges found !!! Perhaps your bridge has changed. Try to reset your pairing.");
                MessageBox.Show("No Hue bridges found !!!\nPerhaps your bridge has changed.\n\nTry to reset your pairing.", "Scan Failed", MessageBoxButtons.OK);
            }
        }

        private async void BT_PairHue_Click(object sender, EventArgs e)
        {
            Plugin.Logger.Info("Pairing your bridge...");
            XmlSerializer bridgeGetter = new XmlSerializer(typeof(Hue));
            TextReader bridgeXmlReader = new StreamReader(bridgeXmlPath);
            Plugin.theHueBridge = (Hue)bridgeGetter.Deserialize(bridgeXmlReader);
            bridgeXmlReader.Close();

            if (Plugin.theHueBridge == null)
            {
                MessageBox.Show("No Hue bridges found ! Please check if you are connected to the same network.", "Scan Failed", MessageBoxButtons.OK);
                return;
            }

            REST r = new REST(Plugin.theHueBridge.BridgeURLBase.Replace("http://", "").Replace(@":80/", ""));
            string result = await r.POST("/api", HueConstants.BODY_POST_CONNECT, Encoding.UTF8, "application/json");
            if (result == null || result.Contains("error") || result == "")
            {
                MessageBox.Show("Please press the button on your Hue bridge before clicking the 'Pair' button.", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<BridgeConnectionSuccess> s = jss.Deserialize<List<BridgeConnectionSuccess>>(result);
            if (s == null || s[0].Success == null || String.IsNullOrEmpty(s[0].Success.Username))
            {
                MessageBox.Show("Please press the button on your Hue bridge before clicking the 'Pair' button.", "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Plugin.APIKey = s[0].Success.Username;
            Plugin.ini.Write("APIKEY", Plugin.APIKey, "HUE");
            BT_PairHue.Visible = false;
            BT_ResetHue.Visible = true;
            BT_ScanLights.Visible = true;
            LB_ChangeLightsWhen.Visible = true;
            CB_ChangeLightsWhen.Visible = true;
            LB_Lights.Visible = true;
            LV_Lights.Visible = true;
            LB_BrightnessLightsRange.Visible = true;
            RS_BrightnessLightsRange.Visible = true;
            LB_BeatDetectionEvery.Visible = true;
            TB_BeatDetectionEvery.Visible = true;
            CB_DisableLogging.Visible = true;
            lblBridgeCnx.Visible = true;
            lblBridgeCnx.Text = "Connected to bridge :\n" + Plugin.theHueBridge.BridgeDeviceSpec.BridgeFriendlyName;
            Plugin.Logger.Info("Connected to bridge : " + Plugin.theHueBridge.BridgeDeviceSpec.BridgeFriendlyName);
            PopulateBridgeLights();
        }

        private void BT_ResetHue_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset the Hue pairing ? \n(Select Yes if the bridge has been reset or the lights are not populating)", "Reset HUE Pairing", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) { return; }

            Plugin.Logger.Info("Resetting your Hue bridge...");
            Plugin.APIKey = Plugin.ini.Read("APIKEY", "HUE");
            if (Plugin.APIKey != string.Empty) { Plugin.ini.DeleteKey("APIKEY", "HUE"); }

            if (File.Exists(bridgeXmlPath)) { File.Delete(bridgeXmlPath); }

            lblBridgeCnx.Visible = true;
            BT_PairHue.Visible = false;
            BT_ResetHue.Visible = false;
            BT_ScanLights.Visible = false;
            LB_ChangeLightsWhen.Visible = false;
            CB_ChangeLightsWhen.Visible = false;
            LB_Lights.Visible = false;
            LV_Lights.Visible = false;
            LB_BrightnessLightsRange.Visible = false;
            RS_BrightnessLightsRange.Visible = false;
            LB_BeatDetectionEvery.Visible = false;
            TB_BeatDetectionEvery.Visible = false;
            CB_DisableLogging.Visible = false;
            FindBridges();
        }

        private void BT_ScanLights_Click(object sender, EventArgs e)
        {
            isRescanLights = true;
            LV_Lights.Items.Clear();
            PopulateBridgeLights();
        }

        private void CB_AllowHue_CheckStateChanged(object sender, EventArgs e)
        {
            if (!isFormLoading)
            {
                if (CB_AllowHue.Checked == true)
                {
                    lblBridgeCnx.Visible = true;

                    if (CheckForExistingKey())
                    {
                        BT_PairHue.Visible = false;
                        BT_ResetHue.Visible = true;
                        BT_ScanLights.Visible = true;
                        LB_ChangeLightsWhen.Visible = true;
                        CB_ChangeLightsWhen.Visible = true;
                        LB_Lights.Visible = true;
                        LV_Lights.Visible = true;
                        LB_BrightnessLightsRange.Visible = true;
                        RS_BrightnessLightsRange.Visible = true;
                        LB_BeatDetectionEvery.Visible = true;
                        TB_BeatDetectionEvery.Visible = true;
                        CB_DisableLogging.Visible = true;
                        InitBridge();
                    }
                    else
                    {
                        BT_PairHue.Visible = false;
                        BT_ResetHue.Visible = false;
                        BT_ScanLights.Visible = false;
                        LB_ChangeLightsWhen.Visible = false;
                        CB_ChangeLightsWhen.Visible = false;
                        LB_Lights.Visible = false;
                        LV_Lights.Visible = false;
                        LB_BrightnessLightsRange.Visible = false;
                        RS_BrightnessLightsRange.Visible = false;
                        LB_BeatDetectionEvery.Visible = false;
                        TB_BeatDetectionEvery.Visible = false;
                        CB_DisableLogging.Visible = false;
                        FindBridges();
                    }
                }
                else
                {
                    lblBridgeCnx.Visible = false;
                    BT_PairHue.Visible = false;
                    BT_ResetHue.Visible = false;
                    BT_ScanLights.Visible = false;
                    LB_ChangeLightsWhen.Visible = false;
                    CB_ChangeLightsWhen.Visible = false;
                    LB_Lights.Visible = false;
                    LV_Lights.Visible = false;
                    LB_BrightnessLightsRange.Visible = false;
                    RS_BrightnessLightsRange.Visible = false;
                    LB_BeatDetectionEvery.Visible = false;
                    TB_BeatDetectionEvery.Visible = false;
                    CB_DisableLogging.Visible = false;
                }
            }
        }

        public class CCBoxItem
        {
            public int Value { get; set; }
            public string Name { get; set; }

            public CCBoxItem()
            {
            }

            public CCBoxItem(string name, int val)
            {
                this.Name = name;
                this.Value = val;
            }

            public override string ToString()
            {
                return string.Format("name: '{0}', value: {1}", Name, Value);
            }
        }

        private void LL_LikeADJLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Plugin.ViewLogFile(sender, e);
        }

        private void CB_AllowBPM_CheckStateChanged(object sender, EventArgs e)
        {
            Toggle_SaveSongsPlaylist();
        }

        private void CB_AllowInitialKey_CheckStateChanged(object sender, EventArgs e)
        {
            Toggle_SaveSongsPlaylist();
        }

        private void CB_AllowEnergy_CheckStateChanged(object sender, EventArgs e)
        {
            Toggle_SaveSongsPlaylist();
        }

        private void CB_AllowRatings_CheckStateChanged(object sender, EventArgs e)
        {
            Toggle_SaveSongsPlaylist();
        }

        private void CB_AllowGenres_CheckStateChanged(object sender, EventArgs e)
        {
            Toggle_SaveSongsPlaylist();
        }

        private void Toggle_SaveSongsPlaylist()
        {
            if (CB_AllowBPM.Checked || CB_AllowInitialKey.Checked || CB_AllowEnergy.Checked || CB_AllowRatings.Checked || CB_AllowGenres.Checked)
            {
                CB_SaveSongsPlaylist.Enabled = true;
                LB_NumberSongsPlaylist.Enabled = true;
                TB_NumberSongsPlaylist.Enabled = true;
            }
            else
            {
                CB_SaveSongsPlaylist.Checked = false;
                CB_SaveSongsPlaylist.Enabled = false;
                LB_NumberSongsPlaylist.Enabled = false;
                TB_NumberSongsPlaylist.Enabled = false;
            }
        }
    }
}
