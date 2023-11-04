using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace CustomExtensions
{
    public static class StringExtension
    {
        public static bool IsIn<T>(this T source, params T[] values)
        {
            return values.Contains(source);
        }
    }
}

namespace MusicBeePlugin
{
    using CustomExtensions;
    using System.Globalization;

    public partial class Plugin
    {
        #region Many thanks to
        //http://archive.gamedev.net/archive/reference/programming/features/beatdetection/index.html
        //https://github.com/Roseburrow/Beat-Detection
        //https://github.com/DrXoo/BeatDetection
        //https://github.com/5inay/HueDesktop
        //https://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown?msg=4152597#xx4152597xx
        //https://www.codeproject.com/Questions/158467/TrackBar-with-selection-and-range-thumbs-possibl
        #endregion

        public static MusicBeeApiInterface mbApiInterface;
        private readonly PluginInfo about = new PluginInfo();
        public static Settings settings= new Settings();
        static Message message = new Message();
        public static SimpleLogger Logger;
        public static string LikeADJVersion, LikeADJIniFile, LikeADJLogFile, BridgeXmlPath;
        public static bool isSettingsChanged = false;
        public static bool allowbpm, allowharmonickey, allowenergy, allowratings, allowlove, allowgenres, savesongsplaylist, allowscanningmessagebox, allowhue;
        public static bool disablelogging;
        public static int DiffBPM, minenergy, minrattings, numbersongsplaylist, brightnesslightmin, brightnesslightmax, CountSongsPlaylist;
        public static string changelightswhen, BeatDetectionEvery;
        public static volatile string APIKey;
        public static Hue theHueBridge = new Hue();
        public static HueLight[] allLights;
        public static int[] lightIndices = new int[50];
        public static int[] lightIndicesAllowed = new int[50];
        public static readonly Random rand = new Random();
        public static readonly System.Timers.Timer LikeADJTimerBeatDetectedSimple = new System.Timers.Timer();
        public static readonly System.Timers.Timer LikeADJTimerBeatDetectedSubBand = new System.Timers.Timer();
        public static readonly System.Timers.Timer LikeADJTimerRedAlertEndOfSong = new System.Timers.Timer();
        public static IniFile ini;
        public static string[] genresAllowed;
        public string playlistName = "LikeADJ History "+ DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss");
        public string[] mbPlaylistSongFiles = new string[1];
        public bool isfirstsong = true;
        public bool isfirstsongnext = true;
        public static MetaDataType MetaDataTypeKey, MetaDataTypeEnergy = new MetaDataType();
        public static bool foundmetadatatypekey = false;
        public static bool foundmetadatatypeenergy = false;
        public string PlaylistLive = "";

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "LikeADJ";
            about.Description = "Auto Mix your songs according to \nBPM, Initial Key, Energy, Track Rating, Love, Genre with Hue lighting";
            about.Author = "DJC👽D - marc.giraudou@outlook.com - 2023";
            about.TargetApplication = "";
            about.Type = PluginType.General;
            about.VersionMajor = 2;
            about.VersionMinor = 0;
            about.Revision = 26;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;

            LikeADJIniFile = Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "mb_LikeADJ.ini");
            ini = new IniFile(LikeADJIniFile);

            LikeADJLogFile = Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "mb_LikeADJ.log");
            File.Delete(LikeADJLogFile);
            Logger = new SimpleLogger(LikeADJLogFile);

            BridgeXmlPath = Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "mb_LikeADJ.xml");

            LikeADJVersion = about.VersionMajor + "." + about.VersionMinor + "." + about.Revision;
            Logger.Info("Starting LikeADJ " + LikeADJVersion + " plugin by DJC👽D... " + "[ MusicBee " + Application.ProductVersion + " ]");

            Logger.Info("Using [" + mbApiInterface.Setting_GetPersistentStoragePath() + "] to save LikeADJ files.");

            LikeADJTimerBeatDetectedSimple.Elapsed += new ElapsedEventHandler(BeatDetection.IsBeatDetectedSimple);
            LikeADJTimerBeatDetectedSubBand.Elapsed += new ElapsedEventHandler(BeatDetection.IsBeatDetectedSubBand);
            LikeADJTimerRedAlertEndOfSong.Elapsed += new ElapsedEventHandler(RedAlertEndOfSong);

            mbApiInterface.MB_AddMenuItem("context.Main/LikeADJ - Start", "LikeADJ", StartLikeADJ);
            mbApiInterface.MB_AddMenuItem("context.Main/LikeADJ - Configure", "LikeADJ", ConfigurePlugin);
            mbApiInterface.MB_AddMenuItem("context.Main/LikeADJ - View the logfile", "LikeADJ", ViewLogFile);
            mbApiInterface.MB_AddMenuItem("context.Main/LikeADJ - Generate a playlist", "LikeADJ", GeneratePlaylist);
            mbApiInterface.MB_AddMenuItem("context.Main/LikeADJ - Deactivate", "LikeADJ", DeactivateLikeADJ);

            Check_Custom_Key();

            LoadSettings();
            return about;
        }

        public static void DeactivateLikeADJ(object sender, EventArgs e)
        {
            Plugin.Logger.Info("Deactivating LikeADJ plugin...");
            Plugin.ini.Write("ALLOWBPM", "false", "BPM");
            Plugin.ini.Write("ALLOWHARMONICKEY", "false", "HARMONICKEY");
            Plugin.ini.Write("ALLOWENERGY", "false", "ENERGY");
            Plugin.ini.Write("ALLOWRATINGS", "false", "RATINGS");
            Plugin.ini.Write("ALLOWLOVE", "false", "LOVE");
            Plugin.ini.Write("ALLOWGENRES", "false", "GENRES");
            Plugin.ini.Write("SAVESONGSPLAYLIST", "false", "PLAYLIST");
            Plugin.ini.Write("ALLOWSCANNINGMESSAGEBOX", "false", "GENERAL");
            Plugin.ini.Write("ALLOWHUE", "false", "HUE");

            Plugin.isSettingsChanged = true;
            Plugin.LoadSettings();
        }

        public static void Check_Custom_Key()
        {
            Logger.Info("Scanning for Custom Tag Key and Custom Tag Energy position...");

            if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom1) == "Key") { MetaDataTypeKey = MetaDataType.Custom1; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom2) == "Key") { MetaDataTypeKey = MetaDataType.Custom2; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom3) == "Key") { MetaDataTypeKey = MetaDataType.Custom3; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom4) == "Key") { MetaDataTypeKey = MetaDataType.Custom4; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom5) == "Key") { MetaDataTypeKey = MetaDataType.Custom5; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom6) == "Key") { MetaDataTypeKey = MetaDataType.Custom6; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom7) == "Key") { MetaDataTypeKey = MetaDataType.Custom7; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom8) == "Key") { MetaDataTypeKey = MetaDataType.Custom8; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom9) == "Key") { MetaDataTypeKey = MetaDataType.Custom9; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom10) == "Key") { MetaDataTypeKey = MetaDataType.Custom10; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom11) == "Key") { MetaDataTypeKey = MetaDataType.Custom11; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom12) == "Key") { MetaDataTypeKey = MetaDataType.Custom12; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom13) == "Key") { MetaDataTypeKey = MetaDataType.Custom13; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom14) == "Key") { MetaDataTypeKey = MetaDataType.Custom14; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom15) == "Key") { MetaDataTypeKey = MetaDataType.Custom15; foundmetadatatypekey = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom16) == "Key") { MetaDataTypeKey = MetaDataType.Custom16; foundmetadatatypekey = true; }
            else foundmetadatatypekey = false;

            if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom1) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom1; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom2) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom2; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom3) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom3; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom4) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom4; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom5) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom5; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom6) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom6; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom7) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom7; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom8) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom8; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom9) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom9; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom10) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom10; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom11) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom11; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom12) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom12; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom13) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom13; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom14) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom14; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom15) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom15; foundmetadatatypeenergy = true; }
            else if (mbApiInterface.Setting_GetFieldName(MetaDataType.Custom16) == "Energy") { MetaDataTypeEnergy = MetaDataType.Custom16; foundmetadatatypeenergy = true; }
            else foundmetadatatypeenergy = false;

            if (foundmetadatatypekey) Logger.Info("Custom Tag Key is [" + MetaDataTypeKey + "]."); else Logger.Info("No Custom Tag for Key found.");
            if (foundmetadatatypeenergy) Logger.Info("Custom Tag Energy is [" + MetaDataTypeEnergy + "]."); else Logger.Info("No Custom Tag for Energy found.");
        }

        public static void ViewLogFile(object sender, EventArgs e)
        {
            LogMonitor LogFile = new LogMonitor(Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "mb_LikeADJ.log"));
            LogFile.Show();
        }

        public void StartLikeADJ(object sender, EventArgs e)
        {
            PopulateNowPlayingList(null, null);
            isfirstsong = true;
            ReceiveNotification(mbApiInterface.NowPlayingList_GetFileProperty(1, FilePropertyType.Url), NotificationType.TrackChanged);
            isfirstsong = false;
        }

        public void PopulateNowPlayingList(object sender, EventArgs e)
        {
            Logger.Info("Clearing the NowPlayingList...");
            mbApiInterface.NowPlayingList_Clear();

            mbApiInterface.Library_QueryFilesEx(null, out string[] songsList);
            Logger.Info(songsList.Length + " songs found in your entire library.");

            Random random = new Random();
            songsList = songsList.OrderBy(x => random.Next()).ToArray();
            mbApiInterface.NowPlayingList_QueueFilesNext(songsList);
            Logger.Info(songsList.Length + " songs shuffled and added to the NowPlayingList...");
        }

        public void GeneratePlaylist(object sender, EventArgs e)
        {
            if (!allowbpm && !allowharmonickey && !allowenergy && !allowratings && !allowlove && !allowgenres) MessageBox.Show("You must activate at least one feature (BPM, Initial Key, Energy, Track Rating, Love or Genre) to generate a LikeADJ playlist !!!", "LikeADJ " + LikeADJVersion);
            else
            {
                mbApiInterface.Player_Stop();
                Stopwatch timer = new Stopwatch();
                timer.Start();

                CountSongsPlaylist = 0;
                isfirstsong = true;
                if (isSettingsChanged) LoadSettings();
                int CurrentSongIndex=1;
                string oldplaylistname = playlistName;

                message = new Message();
                message.Show();
                message.Text = "LikeADJ - Generating playlist... Please wait...";

                string PlaylistGenerated = "";

                PopulateNowPlayingList(null,null);

                do
                {
                    if (isfirstsong) CurrentSongIndex = mbApiInterface.NowPlayingList_GetNextIndex(CurrentSongIndex);
                    string CurrentSongArtist = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.Artist);
                    string CurrentSongTitle = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.TrackTitle);
                    string CurrentSongBPM = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.BeatsPerMin);
                    string CurrentSongKey = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataTypeKey);
                    string CurrentSongEnergy = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataTypeEnergy);
                    string CurrentSongRating = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.Rating);
                    string CurrentSongLove = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.RatingLove);
                    string CurrentSongGenre = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.Genre);
                    string CurrentSongURL = mbApiInterface.NowPlayingList_GetFileProperty(CurrentSongIndex, FilePropertyType.Url);

                    if (((CurrentSongBPM == string.Empty) && allowbpm) || ((CurrentSongKey == string.Empty) && allowharmonickey) || ((CurrentSongEnergy == string.Empty) && allowenergy) || ((CurrentSongRating == string.Empty) && allowratings) || ((CurrentSongGenre == string.Empty) && allowgenres))
                    {
                        Plugin.message.Close();
                        string message = "Your first song has some tags not filled.\nLikeADJ will not be able to found the next song.\n\n";
                        if ((CurrentSongBPM == string.Empty) && allowbpm) { message += "- BPM tag is empty but BPM Auto Mix is allowed.\n"; }
                        if ((CurrentSongKey == string.Empty) && allowharmonickey) { message += "- 'Initial Key' tag is empty but 'Initial Key' Auto Mix is allowed.\n"; }
                        if ((CurrentSongEnergy == string.Empty) && allowenergy) { message += "- Energy tag is empty but Energy Auto Mix is allowed.\n"; }
                        if ((CurrentSongRating == string.Empty) && allowratings) { message += "- 'Track Rating' tag is empty but Ratings Auto Mix is allowed.\n"; }
                        if ((CurrentSongGenre == string.Empty) && allowgenres) { message += "- 'Genre' tag is empty but Genres Auto Mix is allowed.\n"; }
                        message += "\nPlease uncheck unwanted features in the plugin configuration or select another first song.";
                        MessageBox.Show(message, "LikeADJ " + LikeADJVersion);
                        Logger.Info("Player stopped because first song has some tags required not filled.");
                        break;
                    }

                    int NextSongIndex;
                    string NextSongArtist, NextSongTitle, NextSongBPM, NextSongKey, NextSongEnergy, NextSongRating, NextSongLove, NextSongGenre, NextSongURL;

                    string[] CountNowPlayingFiles = { };
                    mbApiInterface.NowPlayingList_QueryFilesEx("", out CountNowPlayingFiles);

                    bool FoundNextSong = false;
                    int NBSongsPassed = 0;
                    do
                    {
                        NBSongsPassed++;

                        NextSongIndex = CurrentSongIndex + 1;
                        NextSongArtist = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Artist);
                        NextSongTitle = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.TrackTitle);
                        NextSongBPM = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.BeatsPerMin);
                        NextSongKey = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataTypeKey);
                        NextSongEnergy = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataTypeEnergy);
                        NextSongRating = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Rating);
                        NextSongLove = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.RatingLove);
                        NextSongGenre = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Genre);
                        NextSongURL = mbApiInterface.NowPlayingList_GetFileProperty(NextSongIndex, FilePropertyType.Url);

                        if (NBSongsPassed >= CountNowPlayingFiles.Length)
                        {
                            Plugin.message.Close();
                            MessageBox.Show("Looping detected in the 'NowPlaying' playlist !!!\n\nThis means that LikeADJ is not able to found the next song and is doing an infinite loop.\n\nYou have only " + CountNowPlayingFiles.Length + " songs in this playlist.");
                            mbApiInterface.Player_Stop();
                            break;
                        }

                        if (allowgenres)
                        {
                            if (NextSongGenre != string.Empty)
                            {
                                bool exist = Array.Exists(genresAllowed, element => element == NextSongGenre);
                                if (exist) FoundNextSong = true;
                                else
                                {
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                    continue;
                                }
                            }
                            else
                            {
                                Logger.Warning("Skipping Song without GENRE : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                        if (allowharmonickey)
                        {
                            if (NextSongKey != string.Empty)
                            {
                                if ((CurrentSongKey.IsIn("1A", "6m", "6m (G♯m)")) && NextSongKey.IsIn("1B", "12A", "1A", "2A", "6d", "6d (B)", "5m", "5m (C♯m)", "6m", "6m (G♯m)", "7m", "7m (D♯m/E♭m)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("2A", "7m", "7m (D♯m/E♭m)")) && NextSongKey.IsIn("2B", "1A", "2A", "3A", "7d", "7d (F♯/G♭)", "6m", "6m (G♯m)", "7m", "7m (D♯m/E♭m)", "8m", "8m (B♭m)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("3A", "8m", "8m (B♭m)")) && NextSongKey.IsIn("3B", "2A", "3A", "4A", "8d", "8d (D♭)", "7m", "7m (D♯m/E♭m)", "8m", "8m (B♭m)", "9m", "9m (Fm)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("4A", "9m", "9m (Fm)")) && NextSongKey.IsIn("4B", "3A", "4A", "5A", "9d", "9d (A♭)", "8m", "8m (B♭m)", "9m", "9m (Fm)", "10m", "10m (Cm)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("5A", "10m", "10m (Cm)")) && NextSongKey.IsIn("5B", "4A", "5A", "6A", "10d", "10d (E♭)", "9m", "9m (Fm)", "10m", "10m (Cm)", "11m", "11m (Gm)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("6A", "11m", "11m (Gm)")) && NextSongKey.IsIn("6B", "5A", "6A", "7A", "11d", "11d (B♭)", "10m", "10m (Cm)", "11m", "11m (Gm)", "12m", "12m (Dm)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("7A", "12m", "12m (Dm)")) && NextSongKey.IsIn("7B", "6A", "7A", "8A", "12d", "12d (F)", "11m", "11m (Gm)", "12m", "12m (Dm)", "1m", "1m (Am)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("8A", "1m", "1m (Am)")) && NextSongKey.IsIn("8B", "7A", "8A", "9A", "1d", "1d (C)", "12m", "12m (Dm)", "1m", "1m (Am)", "2m", "2m (Em)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("9A", "2m", "2m (Em)")) && NextSongKey.IsIn("9B", "8A", "9A", "10A", "2d", "2d (G)", "1m", "1m (Am)", "2m", "2m (Em)", "3m", "3m (Bm)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("10A", "3m", "3m (Bm)")) && NextSongKey.IsIn("10B", "9A", "10A", "11A", "3d", "3d (D)", "2m", "2m (Em)", "3m", "3m (Bm)", "4m", "4m (F♯m)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("11A", "4m", "4m (F♯m)")) && NextSongKey.IsIn("11B", "10A", "11A", "12A", "4d", "4d (A)", "3m", "3m (Bm)", "4m", "4m (F♯m)", "5m", "5m (C♯m)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("12A", "5m", "5m (C♯m)")) && NextSongKey.IsIn("12B", "11A", "12A", "1A", "5d", "5d (E)", "4m", "4m (F♯m)", "5m", "5m (C♯m)", "6m", "6m (G♯m)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("1B", "6d", "6d (B)")) && NextSongKey.IsIn("1A", "12B", "1B", "2B", "6m", "6m (G♯m)", "5d", "5d (E)", "6d", "6d (B)", "7d", "7d (F♯/G♭)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("2B", "7d", "7d (F♯/G♭)")) && NextSongKey.IsIn("2A", "1B", "2B", "3B", "7m", "7m (D♯m/E♭m)", "6d", "6d (B)", "7d", "7d (F♯/G♭)", "8d", "8d (D♭)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("3B", "8d", "8d (D♭)")) && NextSongKey.IsIn("3A", "2B", "3B", "4B", "8m", "8m (B♭m)", "7d", "7d (F♯/G♭)", "8d", "8d (D♭)", "9d", "9d (A♭)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("4B", "9d", "9d (A♭)")) && NextSongKey.IsIn("4A", "3B", "4B", "5B", "9m", "9m (Fm)", "8d", "8d (D♭)", "9d", "9d (A♭)", "10d", "10d (E♭)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("5B", "10d", "10d (E♭)")) && NextSongKey.IsIn("5A", "4B", "5B", "6B", "10m", "10m (Cm)", "9d", "9d (A♭)", "10d", "10d (E♭)", "11d", "11d (B♭)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("6B", "11d", "11d (B♭)")) && NextSongKey.IsIn("6A", "5B", "6B", "7B", "11m", "11m (Gm)", "10d", "10d (E♭)", "11d", "11d (B♭)", "12d", "12d (F)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("7B", "12d", "12d (F)")) && NextSongKey.IsIn("7A", "6B", "7B", "8B", "12m", "12m (Dm)", "11d", "11d (B♭)", "12d", "12d (F)", "1d", "1d (C)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("8B", "1d", "1d (C)")) && NextSongKey.IsIn("8A", "7B", "8B", "9B", "1m", "1m (Am)", "12d", "12d (F)", "1d", "1d (C)", "2d", "2d (G)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("9B", "2d", "2d (G)")) && NextSongKey.IsIn("9A", "8B", "9B", "10B", "2m", "2m (Em)", "1d", "1d (C)", "2d", "2d (G)", "3d", "3d (D)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("10B", "3d", "3d (D)")) && NextSongKey.IsIn("10A", "9B", "10B", "11B", "3m", "3m (Bm)", "2d", "2d (G)", "3d", "3d (D)", "4d", "4d (A)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("11B", "4d", "4d (A)")) && NextSongKey.IsIn("11A", "10B", "11B", "12B", "4m", "4m (F♯m)", "3d", "3d (D)", "4d", "4d (A)", "5d", "5d (E)")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("12B", "5d", "5d (E)")) && NextSongKey.IsIn("12A", "11B", "12B", "1B", "5m", "5m (C♯m)", "4d", "4d (A)", "5d", "5d (E)", "6d", "6d (B)")) FoundNextSong = true;
                                else
                                {
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                    continue;
                                }
                            }
                            else
                            {
                                Logger.Warning("Skipping Song without KEY : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                        if (allowbpm)
                        {
                            if (NextSongBPM != string.Empty)
                            {
                                var a = (int)Math.Round(Convert.ToDouble(CurrentSongBPM, CultureInfo.InvariantCulture.NumberFormat));
                                var b = (int)Math.Round(Convert.ToDouble(NextSongBPM, CultureInfo.InvariantCulture.NumberFormat));
                                int BPMDiff = Math.Abs(a - b);

                                if (BPMDiff < DiffBPM) FoundNextSong = true;
                                else
                                {
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                    continue;
                                }
                            }
                            else
                            {
                                Logger.Warning("Skipping Song without BPM : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                        if (allowenergy)
                        {
                            if (NextSongEnergy != string.Empty)
                            {
                                if (int.Parse(NextSongEnergy) >= minenergy) FoundNextSong = true;
                                else
                                {
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                    continue;
                                }
                            }
                            else
                            {
                                Logger.Warning("Skipping Song without Energy : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                        if (allowratings)
                        {
                            if (NextSongRating != string.Empty)
                            {
                                if (int.Parse(NextSongRating) >= minrattings) FoundNextSong = true;
                                else
                                {
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                    continue;
                                }
                            }
                            else
                            {
                                Logger.Warning("Skipping Song without Ratings : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                        if (allowlove)
                        {
                            if (NextSongLove != string.Empty)
                            {
                                if (NextSongLove=="L") FoundNextSong = true;
                                else
                                {
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                    continue;
                                }
                            }
                            else
                            {
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                    } while (!FoundNextSong);

                    if (NBSongsPassed >= CountNowPlayingFiles.Length) { Logger.Info("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after nothing match your criteria"); break; }
                    else
                    {
                        mbPlaylistSongFiles[0] = NextSongURL;

                        if (isfirstsong)
                        {                               
                            playlistName = DateTime.Now.ToString("LikeADJ dd-MM-yyyy HH-mm-ss");
                            Logger.Info("Generating playlist " + playlistName + "...");
                            PlaylistGenerated = mbApiInterface.Playlist_CreatePlaylist("", playlistName, mbPlaylistSongFiles);                        
                            isfirstsong = false;
                            Logger.Info("Found the first Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                        }
                        else
                        {
                            mbApiInterface.Playlist_AppendFiles(PlaylistGenerated, mbPlaylistSongFiles);
                            Logger.Info("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after -> Next Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                        }

                        CountSongsPlaylist++;
                        message.Text = "LikeADJ - Generating playlist... " + CountSongsPlaylist + "/" + numbersongsplaylist + " songs found. Please wait...";
                        CurrentSongIndex = NextSongIndex;
                     }
                } while (CountSongsPlaylist < numbersongsplaylist);

                mbApiInterface.NowPlayingList_Clear();                
                message.Close();
                timer.Stop();
                Logger.Info("Playlist " + playlistName + " generated successfully in " + timer.Elapsed.Minutes.ToString() + "m " + timer.Elapsed.Seconds.ToString() + "s");
                playlistName = oldplaylistname;
            }
        }

        public void ConfigurePlugin(object sender, EventArgs e)
        {
            settings = new Settings();
            settings.Show();
        }

        public bool Configure(IntPtr panelHandle)
        {
            settings = new Settings();           
            settings.Show();
            return true;
        }
       
        public void Close(PluginCloseReason reason)
        {
            Logger.Info("Closing LikeADJ " + LikeADJVersion + " plugin by DJC👽D...");
        }

        public void Uninstall()
        {
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.TrackChanged:

                    if (mbApiInterface.Player_GetShuffle())
                    {
                        Logger.Info("Deactivating Shuffle mode of MusicBee as library is already shuffled...");
                        mbApiInterface.Player_SetShuffle(false);
                    }

                    if (isSettingsChanged) LoadSettings();

                    int CurrentSongIndex = mbApiInterface.NowPlayingList_GetCurrentIndex();
                    string CurrentSongArtist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
                    string CurrentSongTitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle);
                    string CurrentSongBPM = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.BeatsPerMin);
                    string CurrentSongKey = mbApiInterface.NowPlaying_GetFileTag(MetaDataTypeKey);
                    string CurrentSongEnergy = mbApiInterface.NowPlaying_GetFileTag(MetaDataTypeEnergy);
                    string CurrentSongRating = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Rating);
                    string CurrentSongLove = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.RatingLove);
                    string CurrentSongGenre = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Genre);
                    string CurrentSongURL = mbApiInterface.NowPlayingList_GetFileProperty(CurrentSongIndex, FilePropertyType.Url);

                    if (((CurrentSongBPM == string.Empty) && allowbpm) || ((CurrentSongKey == string.Empty) && allowharmonickey) || ((CurrentSongEnergy == string.Empty) && allowenergy) || ((CurrentSongRating == string.Empty) && allowratings) || ((CurrentSongGenre == string.Empty) && allowgenres))
                    {
                        string message = "Your first song has some tags not filled.\nLikeADJ will not be able to found the next song.\n\n";
                        if ((CurrentSongBPM == string.Empty) && allowbpm) { message += "- BPM tag is empty but BPM Auto Mix is allowed.\n"; }
                        if ((CurrentSongKey == string.Empty) && allowharmonickey) { message += "- 'Initial Key' tag is empty but 'Initial Key' Auto Mix is allowed.\n"; }
                        if ((CurrentSongEnergy == string.Empty) && allowenergy) { message += "- Energy tag is empty but Energy Auto Mix is allowed.\n"; }
                        if ((CurrentSongRating == string.Empty) && allowratings) { message += "- 'Track Rating' tag is empty but Ratings Auto Mix is allowed.\n"; }
                        if ((CurrentSongGenre == string.Empty) && allowgenres) { message += "- 'Genre' tag is empty but Genres Auto Mix is allowed.\n"; }
                        message += "\nPlease uncheck unwanted features in the plugin configuration or select another first song.";
                        MessageBox.Show(message,"LikeADJ " + LikeADJVersion);
                        Logger.Info("Player stopped because first song has some tags required not filled.");
                        mbApiInterface.Player_Stop();
                        break;
                    }

                    if (allowhue && APIKey != string.Empty && (changelightswhen == "Track change" || changelightswhen == "15s before ending (flashing RED) & Track change"))
                    {
                        for (int i = 0; i < lightIndicesAllowed.Length; i++)
                        {
                            if (lightIndicesAllowed[i] != 0)
                            {
                                Color color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                                LightChangeColorandBrightness(lightIndicesAllowed[i], color.R, color.G, color.B, rand.Next(1, 255));
                            }
                        }
                    }
                    
                    if (allowbpm || allowharmonickey || allowratings || allowlove || allowgenres)
                    {
                        int NextSongIndex;
                        string NextSongArtist, NextSongTitle, NextSongBPM, NextSongKey, NextSongEnergy, NextSongRating, NextSongLove, NextSongGenre, NextSongURL;

                        string[] CountNowPlayingFiles = { };
                        mbApiInterface.NowPlayingList_QueryFilesEx("", out CountNowPlayingFiles);

                        if (allowscanningmessagebox)
                        { 
                            message = new Message();
                            message.Show();
                        }

                        bool FoundNextSong = false;
                        int NBSongsPassed=0;
                        do
                        {
                            if (isfirstsong) { if (allowscanningmessagebox) message.Text = "LikeADJ - Trying to find the first song... " + NBSongsPassed + "/" + CountNowPlayingFiles.Length + " songs passed. Please wait..."; }
                            else if (allowscanningmessagebox) message.Text = "LikeADJ - Trying to find next song... " + NBSongsPassed + "/" + CountNowPlayingFiles.Length + " songs passed. Please wait...";

                            NBSongsPassed++;

                            NextSongIndex = CurrentSongIndex + 1;
                            NextSongArtist = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Artist);
                            NextSongTitle = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.TrackTitle);
                            NextSongBPM = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.BeatsPerMin);
                            NextSongKey = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataTypeKey);
                            NextSongEnergy = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataTypeEnergy);
                            NextSongRating = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Rating);
                            NextSongLove = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.RatingLove);
                            NextSongGenre = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Genre);
                            NextSongURL = mbApiInterface.NowPlayingList_GetFileProperty(NextSongIndex, FilePropertyType.Url);

                            if (NBSongsPassed >= CountNowPlayingFiles.Length)
                            {
                                Plugin.message.Close();
                                MessageBox.Show("Looping detected in the 'NowPlaying' playlist !!!\n\nThis means that LikeADJ is not able to found the next song and is doing an infinite loop.\n\nYou have only "+ CountNowPlayingFiles.Length + " songs in this playlist.");
                                mbApiInterface.Player_Stop();
                                break;
                            }

                            if (allowgenres)
                            {
                                if (NextSongGenre != string.Empty)
                                {
                                    bool exist = Array.Exists(genresAllowed, element => element == NextSongGenre);
                                    if (exist) FoundNextSong = true;
                                    else
                                    {
                                        FoundNextSong = false;
                                        mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                        mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Logger.Warning("Skipping Song without GENRE : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }

                            if (allowharmonickey)
                            {
                                if (NextSongKey != string.Empty)
                                {
                                    if ((CurrentSongKey.IsIn("1A", "6m", "6m (G♯m)")) && NextSongKey.IsIn("1B", "12A", "1A", "2A", "6d", "6d (B)", "5m", "5m (C♯m)", "6m", "6m (G♯m)", "7m", "7m (D♯m/E♭m)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("2A", "7m", "7m (D♯m/E♭m)")) && NextSongKey.IsIn("2B", "1A", "2A", "3A", "7d", "7d (F♯/G♭)", "6m", "6m (G♯m)", "7m", "7m (D♯m/E♭m)", "8m", "8m (B♭m)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("3A", "8m", "8m (B♭m)")) && NextSongKey.IsIn("3B", "2A", "3A", "4A", "8d", "8d (D♭)", "7m", "7m (D♯m/E♭m)", "8m", "8m (B♭m)", "9m", "9m (Fm)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("4A", "9m", "9m (Fm)")) && NextSongKey.IsIn("4B", "3A", "4A", "5A", "9d", "9d (A♭)", "8m", "8m (B♭m)", "9m", "9m (Fm)", "10m", "10m (Cm)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("5A", "10m", "10m (Cm)")) && NextSongKey.IsIn("5B", "4A", "5A", "6A", "10d", "10d (E♭)", "9m", "9m (Fm)", "10m", "10m (Cm)", "11m", "11m (Gm)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("6A", "11m", "11m (Gm)")) && NextSongKey.IsIn("6B", "5A", "6A", "7A", "11d", "11d (B♭)", "10m", "10m (Cm)", "11m", "11m (Gm)", "12m", "12m (Dm)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("7A", "12m", "12m (Dm)")) && NextSongKey.IsIn("7B", "6A", "7A", "8A", "12d", "12d (F)", "11m", "11m (Gm)", "12m", "12m (Dm)", "1m", "1m (Am)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("8A", "1m", "1m (Am)")) && NextSongKey.IsIn("8B", "7A", "8A", "9A", "1d", "1d (C)", "12m", "12m (Dm)", "1m", "1m (Am)", "2m", "2m (Em)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("9A", "2m", "2m (Em)")) && NextSongKey.IsIn("9B", "8A", "9A", "10A", "2d", "2d (G)", "1m", "1m (Am)", "2m", "2m (Em)", "3m", "3m (Bm)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("10A", "3m", "3m (Bm)")) && NextSongKey.IsIn("10B", "9A", "10A", "11A", "3d", "3d (D)", "2m", "2m (Em)", "3m", "3m (Bm)", "4m", "4m (F♯m)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("11A", "4m", "4m (F♯m)")) && NextSongKey.IsIn("11B", "10A", "11A", "12A", "4d", "4d (A)", "3m", "3m (Bm)", "4m", "4m (F♯m)", "5m", "5m (C♯m)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("12A", "5m", "5m (C♯m)")) && NextSongKey.IsIn("12B", "11A", "12A", "1A", "5d", "5d (E)", "4m", "4m (F♯m)", "5m", "5m (C♯m)", "6m", "6m (G♯m)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("1B", "6d", "6d (B)")) && NextSongKey.IsIn("1A", "12B", "1B", "2B", "6m", "6m (G♯m)", "5d", "5d (E)", "6d", "6d (B)", "7d", "7d (F♯/G♭)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("2B", "7d", "7d (F♯/G♭)")) && NextSongKey.IsIn("2A", "1B", "2B", "3B", "7m", "7m (D♯m/E♭m)", "6d", "6d (B)", "7d", "7d (F♯/G♭)", "8d", "8d (D♭)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("3B", "8d", "8d (D♭)")) && NextSongKey.IsIn("3A", "2B", "3B", "4B", "8m", "8m (B♭m)", "7d", "7d (F♯/G♭)", "8d", "8d (D♭)", "9d", "9d (A♭)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("4B", "9d", "9d (A♭)")) && NextSongKey.IsIn("4A", "3B", "4B", "5B", "9m", "9m (Fm)", "8d", "8d (D♭)", "9d", "9d (A♭)", "10d", "10d (E♭)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("5B", "10d", "10d (E♭)")) && NextSongKey.IsIn("5A", "4B", "5B", "6B", "10m", "10m (Cm)", "9d", "9d (A♭)", "10d", "10d (E♭)", "11d", "11d (B♭)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("6B", "11d", "11d (B♭)")) && NextSongKey.IsIn("6A", "5B", "6B", "7B", "11m", "11m (Gm)", "10d", "10d (E♭)", "11d", "11d (B♭)", "12d", "12d (F)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("7B", "12d", "12d (F)")) && NextSongKey.IsIn("7A", "6B", "7B", "8B", "12m", "12m (Dm)", "11d", "11d (B♭)", "12d", "12d (F)", "1d", "1d (C)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("8B", "1d", "1d (C)")) && NextSongKey.IsIn("8A", "7B", "8B", "9B", "1m", "1m (Am)", "12d", "12d (F)", "1d", "1d (C)", "2d", "2d (G)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("9B", "2d", "2d (G)")) && NextSongKey.IsIn("9A", "8B", "9B", "10B", "2m", "2m (Em)", "1d", "1d (C)", "2d", "2d (G)", "3d", "3d (D)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("10B", "3d", "3d (D)")) && NextSongKey.IsIn("10A", "9B", "10B", "11B", "3m", "3m (Bm)", "2d", "2d (G)", "3d", "3d (D)", "4d", "4d (A)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("11B", "4d", "4d (A)")) && NextSongKey.IsIn("11A", "10B", "11B", "12B", "4m", "4m (F♯m)", "3d", "3d (D)", "4d", "4d (A)", "5d", "5d (E)")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("12B", "5d", "5d (E)")) && NextSongKey.IsIn("12A", "11B", "12B", "1B", "5m", "5m (C♯m)", "4d", "4d (A)", "5d", "5d (E)", "6d", "6d (B)")) FoundNextSong = true;
                                    else
                                    {
                                        FoundNextSong = false;
                                        mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                        mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Logger.Warning("Skipping Song without KEY : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }

                            if (allowbpm)
                            {
                                if (NextSongBPM != string.Empty)
                                {
                                    var a = (int)Math.Round(Convert.ToDouble(CurrentSongBPM, CultureInfo.InvariantCulture.NumberFormat));
                                    var b = (int)Math.Round(Convert.ToDouble(NextSongBPM, CultureInfo.InvariantCulture.NumberFormat));
                                    int BPMDiff = Math.Abs(a - b);

                                    if (BPMDiff < DiffBPM) FoundNextSong = true;
                                    else
                                    {
                                        FoundNextSong = false;
                                        mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                        mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Logger.Warning("Skipping Song without BPM : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }

                            if (allowenergy)
                            {
                                if (NextSongEnergy != string.Empty)
                                {
                                    if (int.Parse(NextSongEnergy) >= minenergy) FoundNextSong = true;
                                    else
                                    {
                                        FoundNextSong = false;
                                        mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                        mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Logger.Warning("Skipping Song without Energy : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }

                            if (allowratings)
                            {
                                if (NextSongRating != string.Empty)
                                {
                                    if (int.Parse(NextSongRating) >= minrattings) FoundNextSong = true;
                                    else
                                    {
                                        FoundNextSong = false;
                                        mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                        mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                        continue;
                                    }
                                }
                                else
                                {
                                    Logger.Warning("Skipping Song without Ratings : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }

                            if (allowlove)
                            {
                                if (NextSongLove != string.Empty)
                                {
                                    if (NextSongLove=="L") FoundNextSong = true;
                                    else
                                    {
                                        FoundNextSong = false;
                                        mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                        mbApiInterface.NowPlayingList_QueueLast(NextSongURL);
                                        continue;
                                    }
                                }
                                else
                                {
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }
                        } while (!FoundNextSong);

                        if (allowscanningmessagebox) message.Close(); 

                        if (NBSongsPassed >= CountNowPlayingFiles.Length) { Logger.Info("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after nothing match your criteria"); }
                        else
                        {
                            if (savesongsplaylist)
                            {
                                mbPlaylistSongFiles[0] = CurrentSongURL;

                                if (isfirstsong)
                                {
                                    mbApiInterface.NowPlayingList_PlayNow(NextSongURL);
                                    isfirstsong = false;
                                }
                                else
                                {
                                    if (isfirstsongnext)
                                    {
                                        Logger.Info("Creating playlist " + playlistName + " with first song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - LOVE:" + CurrentSongLove + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after -> Next Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                        PlaylistLive= mbApiInterface.Playlist_CreatePlaylist("", playlistName, mbPlaylistSongFiles);
                                        isfirstsongnext = false;
                                    }
                                    else
                                    { 
                                        Logger.Info("Adding to playlist " + playlistName + " the song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - LOVE:" + CurrentSongLove + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after -> Next Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']");
                                        mbApiInterface.Playlist_AppendFiles(PlaylistLive, mbPlaylistSongFiles);
                                   }
                                }
                            }
                            else { Logger.Info("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - LOVE:" + CurrentSongLove + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after -> Next Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - LOVE:" + NextSongLove + " - GENRE:'" + NextSongGenre + "']"); }
                        }
                    }

                    break;
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (isSettingsChanged) { Logger.Info("Settings changed. Reloading settings..."); }
                else { Logger.Info("Loading settings..."); }

                Boolean.TryParse(ini.Read("ALLOWBPM", "BPM"), out allowbpm);
                DiffBPM = int.Parse(ini.Read("DIFFBPM", "BPM"));
                Boolean.TryParse(ini.Read("ALLOWHARMONICKEY", "HARMONICKEY"), out allowharmonickey);
                Boolean.TryParse(ini.Read("ALLOWENERGY", "ENERGY"), out allowenergy);
                minenergy = int.Parse(ini.Read("MINENERGY", "ENERGY"));
                Boolean.TryParse(ini.Read("ALLOWRATINGS", "RATINGS"), out allowratings);
                minrattings = int.Parse(ini.Read("MINRATINGS", "RATINGS"));
                Boolean.TryParse(ini.Read("ALLOWLOVE", "LOVE"), out allowlove);
                Boolean.TryParse(ini.Read("ALLOWGENRES", "GENRES"), out allowgenres);
                string genresallowed = ini.Read("GENRESSELECTED", "GENRES");
                genresAllowed = genresallowed.Split(',').ToArray();
                Boolean.TryParse(ini.Read("SAVESONGSPLAYLIST", "PLAYLIST"), out savesongsplaylist);
                numbersongsplaylist = int.Parse(ini.Read("NUMBERSONGSPLAYLIST", "PLAYLIST"));
                Boolean.TryParse(Plugin.ini.Read("ALLOWSCANNINGMESSAGEBOX", "GENERAL"), out allowscanningmessagebox);
                Boolean.TryParse(ini.Read("ALLOWHUE", "HUE"), out allowhue);
                APIKey = ini.Read("APIKEY", "HUE");
                changelightswhen = ini.Read("CHANGELIGHTSWHEN", "HUE");
                string lightallowed = ini.Read("LIGHTSALLOWED", "HUE");
                lightIndicesAllowed = lightallowed.Split(',').Select(s => int.TryParse(s, out int n) ? n : 0).ToArray();
                int.TryParse(Plugin.ini.Read("BRIGHTNESSLIGHTSMIN", "HUE"), out int flag2);
                brightnesslightmin = flag2;
                int.TryParse(Plugin.ini.Read("BRIGHTNESSLIGHTSMAX", "HUE"), out flag2);
                brightnesslightmax = flag2;
                BeatDetectionEvery = ini.Read("BEATDETECTIONEVERY", "HUE");
                Boolean.TryParse(ini.Read("DISABLELOGGING", "HUE"), out disablelogging);

                Logger.Info("Settings : ALLOWBPM=" + allowbpm + "[Max Diff " + DiffBPM + "] - ALLOWHARMONICKEY=" + allowharmonickey + " - ALLOWENERGY = " + allowenergy + "[Min " + minenergy + "] - ALLOWRATINGS=" + allowratings + "[Min " + minrattings + "] - ALLOWLOVE=" + allowlove + " - ALLOWGENRES=" + allowgenres + "[" + genresallowed + "] - SAVESONGSPLAYLIST=" + savesongsplaylist + " - NUMBERSONGSPLAYLIST=" + numbersongsplaylist + " - ALLOWSCANNINGMESSAGEBOX=" + "[" + allowscanningmessagebox + "] - ALLOWHUE=" + allowhue + "[Change lights when " + changelightswhen + "] - LIGHTSALLOWED=" + ini.Read("LIGHTSALLOWED", "HUE") + " - BRIGHTNESSLIGHTSMIN=" + brightnesslightmin + " - BRIGHTNESSLIGHTSMAX=" + brightnesslightmax + " - BEATDETECTIONEVERY=" + BeatDetectionEvery + "ms - DISABLELOGGING=" + disablelogging);
                isSettingsChanged = false;

                if (allowhue && APIKey != string.Empty) { settings.InitBridge(); }
                else if (allowhue && APIKey == string.Empty) MessageBox.Show("Hue is allowed but I can't found API Key !!!!");

                if (allowhue && APIKey != string.Empty && changelightswhen == "Beat is detected (Simple)")
                {
                    Double.TryParse(BeatDetectionEvery, out Double Interval);
                    LikeADJTimerBeatDetectedSimple.Interval = Interval;
                    Logger.Info("Starting timer for 'Beat is detected (Simple)' : " + Interval + "ms...");
                    LikeADJTimerBeatDetectedSimple.Start();
                }
                else if (LikeADJTimerBeatDetectedSimple.Enabled) { LikeADJTimerBeatDetectedSimple.Stop(); Logger.Info("Timer for 'Beat is detected (Simple)' stopped."); }

                if (allowhue && APIKey != string.Empty && changelightswhen == "Beat is detected (SubBand)")
                {
                    BeatDetection.SubBands = new BeatDetection.SubBand[64];
                    for (int i = 0; i < BeatDetection.SubBands.Length; i++) { BeatDetection.SubBands[i] = new BeatDetection.SubBand(i + 1); }
                    Double.TryParse(BeatDetectionEvery, out Double Interval);
                    LikeADJTimerBeatDetectedSubBand.Interval = Interval;
                    Logger.Info("Starting timer for 'Beat is detected (SubBand)' : " + Interval + "ms...");
                    LikeADJTimerBeatDetectedSubBand.Start();
                }
                else if (LikeADJTimerBeatDetectedSubBand.Enabled) { LikeADJTimerBeatDetectedSubBand.Stop(); Logger.Info("Timer for 'Beat is detected (SubBand)' stopped.");}

                if (allowhue && APIKey != string.Empty && changelightswhen == "15s before ending (flashing RED) & Track change")
                {
                    LikeADJTimerRedAlertEndOfSong.Interval = 500;
                    Logger.Info("Starting timer for '15s before ending (flashing RED) & Track change' : " + LikeADJTimerRedAlertEndOfSong.Interval + "ms...");
                    LikeADJTimerRedAlertEndOfSong.Start();
                }
                else if (LikeADJTimerRedAlertEndOfSong.Enabled) { LikeADJTimerRedAlertEndOfSong.Stop(); Logger.Info("Timer for '15s before ending (flashing RED) & Track change' stopped."); }
            }
            catch
            {
                if (!File.Exists(LikeADJIniFile)) Logger.Info("No ini file " + LikeADJIniFile + " found.");
            }
        }

        private void RedAlertEndOfSong(object sender, ElapsedEventArgs e)
        {
            int duration = mbApiInterface.NowPlaying_GetDuration();
            TimeSpan lenght = TimeSpan.FromMilliseconds(duration);

            int songposition = mbApiInterface.Player_GetPosition();
            TimeSpan elapsed = TimeSpan.FromMilliseconds(songposition);

            TimeSpan span = lenght - elapsed;
            int diff = (int)span.TotalSeconds;

            if (diff<15)
            { 
                Logger.Info("Red alert of ending song (15s) engaged at " + string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds));
                for (int i = 0; i < lightIndicesAllowed.Length; i++)
                {
                    if (lightIndicesAllowed[i] != 0)
                    {
                        Color color = Color.FromArgb(255, 0, 0);
                        if (diff % 2 == 0) LightChangeColorandBrightness(lightIndicesAllowed[i], color.R, color.G, color.B, 1);
                        else LightChangeColorandBrightness(lightIndicesAllowed[i], color.R, color.G, color.B, 200);
                    }
                }
            }
        }

        public static async void LightChangeColorandBrightness(int lightIDX, double R, double G, double B, int lightBrightness)
        {
            if (!disablelogging) Logger.Info("Changing color of light " + lightIDX + " to (" + R + "," + G + "," + B + ") and brightness to " + lightBrightness + "...");
            ColorConverter.ColorToXY(R, G, B, out double X, out double Y);

            REST rest = new REST(theHueBridge.BridgeURLBase.Replace("http://", "").Replace(@":80/", ""));
            string singleLight = await rest.GET("/api/" + APIKey + "/lights/" + (lightIDX));

            JObject singleLightObj = JObject.Parse(singleLight);
            HueLight selectedLight = JsonConvert.DeserializeObject<HueLight>(singleLightObj.ToString());

            rest = new REST(theHueBridge.BridgeURLBase.Replace("http://", "").Replace(@":80/", ""));

            string LightX = X.ToString().Replace(",", ".");
            string LightY = Y.ToString().Replace(",", ".");

            if (!selectedLight.State.On) { await rest.PUT("/api/" + APIKey + "/lights/" + (lightIDX) + "/state", "{\"on\":true, \"xy\":[" + LightX + "," + LightY + "], \"bri\":" + lightBrightness + "}", Encoding.UTF8, "application/json"); }
            else { await rest.PUT("/api/" + APIKey + "/lights/" + (lightIDX) + "/state", "{\"xy\":[" + LightX + "," + LightY + "], \"bri\":" + lightBrightness + "}", Encoding.UTF8, "application/json"); }
        }
    }
}