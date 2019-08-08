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
        Settings settings= new Settings();
        static Message message = new Message();
        public static LogFile monLogListener;
        public static string LikeADJVersion, LikeADJIniFile;
        public static bool isSettingsChanged = false;
        bool allowbpm, allowharmonickey, allowenergy, allowratings, allowgenres, savesongsplaylist, allowhue;
        public static bool disablelogging, MusicBeeisportable;
        int DiffBPM, minenergy, minrattings, numbersongsplaylist;
        public static int brightnesslightmin, brightnesslightmax;
        string changelightswhen, BeatDetectionEvery;
        public static volatile string APIKey;
        public static Hue theHueBridge = new Hue();
        public static HueLight[] allLights;
        public static int[] lightIndices = new int[20];
        public static int[] lightIndicesAllowed = new int[20];
        public static readonly Random rand = new Random();
        readonly System.Timers.Timer LikeADJTimerBeatDetectedSimple = new System.Timers.Timer();
        readonly System.Timers.Timer LikeADJTimerBeatDetectedSubBand = new System.Timers.Timer();
        readonly System.Timers.Timer LikeADJTimerRedAlertEndOfSong = new System.Timers.Timer();
        public static IniFile ini;
        public static string[] genresAllowed;
        public string playlistName = "LikeADJ History "+ DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss");
        public string[] mbPlaylistSongFiles = new string[1];
        public bool isfirstsong = true;
        public int CountSongsPlaylist;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "LikeADJ";
            about.Description = "Auto Mix your songs according to \nBPM, Initial Key (Camelot), Energy, Track Rating, Genre and Hue lighting";
            about.Author = "DJC👽D - marc.giraudou@outlook.com - 2019";
            about.TargetApplication = "";
            about.Type = PluginType.General;
            about.VersionMajor = 2;
            about.VersionMinor = 0;
            about.Revision = 15;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;

            if (Application.StartupPath != @"C:\Program Files (x86)\MusicBee") MusicBeeisportable = true;
            else MusicBeeisportable = false;

            if (MusicBeeisportable)
            {
                File.Delete(Application.StartupPath + "\\Plugins\\mb_LikeADJ.log");
                monLogListener = new LogFile(Application.StartupPath + "\\Plugins\\mb_LikeADJ.log") { WriteDateInfo = true };
                LikeADJIniFile = Application.StartupPath + "\\Plugins\\mb_LikeADJ.ini";
            }
            else
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.log");
                monLogListener = new LogFile(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.log") { WriteDateInfo = true };
                LikeADJIniFile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.ini";               
            }

            ini = new IniFile(LikeADJIniFile);

            Trace.Listeners.Add(monLogListener);
            Trace.AutoFlush = true;

            LikeADJTimerBeatDetectedSimple.Elapsed += new ElapsedEventHandler(BeatDetection.IsBeatDetectedSimple);
            LikeADJTimerBeatDetectedSubBand.Elapsed += new ElapsedEventHandler(BeatDetection.IsBeatDetectedSubBand);
            LikeADJTimerRedAlertEndOfSong.Elapsed += new ElapsedEventHandler(RedAlertEndOfSong);

            LikeADJVersion = about.VersionMajor + "." + about.VersionMinor + "." + about.Revision;
            Trace.TraceInformation("Starting LikeADJ " + LikeADJVersion + " RC plugin by DJC👽D...");

            if (MusicBeeisportable) Trace.TraceInformation("MusicBee is portable. Using [" + Application.StartupPath + "] to save LikeADJ files.");
            else Trace.TraceInformation("MusicBee is installed. Using [" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\] to save LikeADJ files.");

            mbApiInterface.MB_AddMenuItem("context.Main/Generate a LikeADJ playlist with all songs in your library", "LikeADJ", GeneratePlaylist);
            mbApiInterface.MB_AddMenuItem("context.Main/Configure LikeADJ plugin", "LikeADJ", ConfigurePlugin);

            LoadSettings();
            return about;
        }

        public void GeneratePlaylist(object sender, EventArgs e)
        {
            LoadSettings();

            if (!allowbpm && !allowharmonickey && !allowenergy && !allowratings && !allowgenres) MessageBox.Show("You must activate at least one feature (BPM, Initial Key, Energy, Track Rating or Genre) to generate a LikeADJ playlist !!!", "LikeADJ " + LikeADJVersion);
            else
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                CountSongsPlaylist = 0;
                isfirstsong = true;
                if (isSettingsChanged) LoadSettings();
                int CurrentSongIndex=0;
                string oldplaylistname = playlistName;

                message = new Message();
                message.Show();
                message.Text = "LikeADJ - Generating playslist... Please wait...";

                mbApiInterface.NowPlayingList_Clear();
                mbApiInterface.NowPlayingList_PlayLibraryShuffled();
                mbApiInterface.Player_Stop();

                do
                {
                    if (isfirstsong) CurrentSongIndex = mbApiInterface.NowPlayingList_GetCurrentIndex();
                    string CurrentSongArtist = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.Artist);
                    string CurrentSongTitle = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.TrackTitle);
                    string CurrentSongBPM = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.BeatsPerMin);
                    string CurrentSongKey = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.Custom1);
                    string CurrentSongEnergy = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.Custom2);
                    string CurrentSongRating = mbApiInterface.NowPlayingList_GetFileTag(CurrentSongIndex, MetaDataType.Rating);
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
                        Trace.TraceInformation("Player stopped because first song has some tags required not filled.");
                        break;
                    }

                    int NextSongIndex;
                    string NextSongArtist, NextSongTitle, NextSongBPM, NextSongKey, NextSongEnergy, NextSongRating, NextSongGenre, NextSongURL;

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
                        NextSongKey = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Custom1);
                        NextSongEnergy = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Custom2);
                        NextSongRating = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Rating);
                        NextSongGenre = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Genre);
                        NextSongURL = mbApiInterface.NowPlayingList_GetFileProperty(NextSongIndex, FilePropertyType.Url);

                        if (NBSongsPassed >= CountNowPlayingFiles.Length)
                        {
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
                                Trace.TraceWarning("Skipping Song without GENRE : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                        if (allowharmonickey)
                        {
                            if (NextSongKey != string.Empty)
                            {
                                if ((CurrentSongKey.IsIn("1A", "6m")) && NextSongKey.IsIn("1B", "12A", "1A", "2A", "6d", "5m", "6m", "7m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("2A", "7m")) && NextSongKey.IsIn("2B", "1A", "2A", "3A", "7d", "6m", "7m", "8m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("3A", "8m")) && NextSongKey.IsIn("3B", "2A", "3A", "4A", "8d", "7m", "8m", "9m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("4A", "9m")) && NextSongKey.IsIn("4B", "3A", "4A", "5A", "9d", "8m", "9m", "10m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("5A", "10m")) && NextSongKey.IsIn("5B", "4A", "5A", "6A", "10d", "9m", "10m", "11m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("6A", "11m")) && NextSongKey.IsIn("6B", "5A", "6A", "7A", "11d", "10m", "11m", "12m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("7A", "12m")) && NextSongKey.IsIn("7B", "6A", "7A", "8A", "12d", "11m", "12m", "1m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("8A", "1m")) && NextSongKey.IsIn("8B", "7A", "8A", "9A", "1d", "12m", "1m", "2m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("9A", "2m")) && NextSongKey.IsIn("9B", "8A", "9A", "10A", "2d", "1m", "2m", "3m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("10A", "3m")) && NextSongKey.IsIn("10B", "9A", "10A", "11A", "3d", "2m", "3m", "4m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("11A", "4m")) && NextSongKey.IsIn("11B", "10A", "11A", "12A", "4d", "3m", "4m", "5m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("12A", "5m")) && NextSongKey.IsIn("12B", "11A", "12A", "1A", "5d", "4m", "5m", "6m")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("1B", "6d")) && NextSongKey.IsIn("1A", "12B", "1B", "2B", "6m", "5d", "6d", "7d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("2B", "7d")) && NextSongKey.IsIn("2A", "1B", "2B", "3B", "7m", "6d", "7d", "8d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("3B", "8d")) && NextSongKey.IsIn("3A", "2B", "3B", "4B", "8m", "7d", "8d", "9d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("4B", "9d")) && NextSongKey.IsIn("4A", "3B", "4B", "5B", "9m", "8d", "9d", "10d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("5B", "10d")) && NextSongKey.IsIn("5A", "4B", "5B", "6B", "10m", "9d", "10d", "11d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("6B", "11d")) && NextSongKey.IsIn("6A", "5B", "6B", "7B", "11m", "10d", "11d", "12d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("7B", "12d")) && NextSongKey.IsIn("7A", "6B", "7B", "8B", "12m", "11d", "12d", "1d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("8B", "1d")) && NextSongKey.IsIn("8A", "7B", "8B", "9B", "1m", "12d", "1d", "2d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("9B", "2d")) && NextSongKey.IsIn("9A", "8B", "9B", "10B", "2m", "1d", "2d", "3d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("10B", "3d")) && NextSongKey.IsIn("10A", "9B", "10B", "11B", "3m", "2d", "3d", "4d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("11B", "4d")) && NextSongKey.IsIn("11A", "10B", "11B", "12B", "4m", "3d", "4d", "5d")) FoundNextSong = true;
                                else if ((CurrentSongKey.IsIn("12B", "5d")) && NextSongKey.IsIn("12A", "11B", "12B", "1B", "5m", "4d", "5d", "6d")) FoundNextSong = true;
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
                                Trace.TraceWarning("Skipping Song without KEY : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }

                        if (allowbpm)
                        {
                            if (NextSongBPM != string.Empty)
                            {
                                int BPMDiff = Math.Abs(int.Parse(CurrentSongBPM) - int.Parse(NextSongBPM));

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
                                Trace.TraceWarning("Skipping Song without BPM : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
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
                                Trace.TraceWarning("Skipping Song without Energy : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
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
                                Trace.TraceWarning("Skipping Song without Ratings : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                                FoundNextSong = false;
                                mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                continue;
                            }
                        }
                    } while (!FoundNextSong);

                    if (NBSongsPassed >= CountNowPlayingFiles.Length) { Trace.TraceInformation("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after nothing match your criteria"); break; }
                    else
                    {
                        mbPlaylistSongFiles[0] = NextSongURL;

                        if (isfirstsong)
                        {                               
                            playlistName = DateTime.Now.ToString("LikeADJ dd-MM-yyyy HH-mm-ss");
                            Trace.TraceInformation("Generating playlist " + playlistName + "...");
                            mbApiInterface.Playlist_CreatePlaylist("", playlistName, mbPlaylistSongFiles);                        
                            isfirstsong = false;
                            Trace.TraceInformation("Found the fisrt Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                        }
                        else
                        {
                            if (MusicBeeisportable) mbApiInterface.Playlist_AppendFiles(Application.StartupPath + "\\Library\\Playlists\\" + playlistName + ".mbp", mbPlaylistSongFiles);
                            else mbApiInterface.Playlist_AppendFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\Playlists\\" + playlistName + ".mbp", mbPlaylistSongFiles);
                            Trace.TraceInformation("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after -> Next Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                        }

                        CountSongsPlaylist++;
                        message.Text = "LikeADJ - Generating playslist... " + CountSongsPlaylist + "/" + numbersongsplaylist + " songs found. Please wait...";
                        CurrentSongIndex = NextSongIndex;
                     }
                } while (CountSongsPlaylist < numbersongsplaylist);

                mbApiInterface.NowPlayingList_Clear();                
                message.Close();
                timer.Stop();
                Trace.TraceInformation("Playlist " + playlistName + " generated successfully in " + timer.Elapsed.Minutes.ToString() + "m " + timer.Elapsed.Seconds.ToString() + "s");
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
            Trace.TraceInformation("Closing LikeADJ " + LikeADJVersion + " β plugin by DJC👽D...");
        }

        public void Uninstall()
        {
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:                         
                            break;
                        case PlayState.Paused:
                            break;
                        case PlayState.Stopped:
                            break;
                    }
                    break;
                case NotificationType.TrackChanged:                   
                    LikeADJTimerBeatDetectedSimple.Stop();
                    LikeADJTimerBeatDetectedSubBand.Stop();
                    LikeADJTimerRedAlertEndOfSong.Stop();

                    if (isSettingsChanged) LoadSettings();

                    int CurrentSongIndex = mbApiInterface.NowPlayingList_GetCurrentIndex();
                    string CurrentSongArtist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);
                    string CurrentSongTitle = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.TrackTitle);
                    string CurrentSongBPM = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.BeatsPerMin);
                    string CurrentSongKey = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Custom1);
                    string CurrentSongEnergy = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Custom2);
                    string CurrentSongRating = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Rating);
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
                        Trace.TraceInformation("Player stopped because first song has some tags required not filled.");
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

                    if (allowhue && APIKey != string.Empty && changelightswhen == "Beat is detected (Simple)")
                    {
                        Double.TryParse(BeatDetectionEvery, out Double Interval);
                        LikeADJTimerBeatDetectedSimple.Interval = Interval;
                        LikeADJTimerBeatDetectedSimple.Start();                       
                    }

                    if (allowhue && APIKey != string.Empty && changelightswhen == "Beat is detected (SubBand)")
                    {
                        BeatDetection.SubBands = new BeatDetection.SubBand[64];
                        for (int i = 0; i < BeatDetection.SubBands.Length; i++) { BeatDetection.SubBands[i] = new BeatDetection.SubBand(i + 1); }
                        Double.TryParse(BeatDetectionEvery, out Double Interval);
                        LikeADJTimerBeatDetectedSubBand.Interval = Interval;
                        LikeADJTimerBeatDetectedSubBand.Start();
                    }

                    if (allowhue && APIKey != string.Empty && changelightswhen == "15s before ending (flashing RED) & Track change")
                    {
                        LikeADJTimerRedAlertEndOfSong.Interval = 500;
                        LikeADJTimerRedAlertEndOfSong.Start();
                    }
                    
                    if (allowbpm || allowharmonickey || allowratings || allowgenres)
                    {
                        int NextSongIndex;
                        string NextSongArtist, NextSongTitle, NextSongBPM, NextSongKey, NextSongEnergy, NextSongRating, NextSongGenre, NextSongURL;

                        string[] CountNowPlayingFiles = { };
                        mbApiInterface.NowPlayingList_QueryFilesEx("", out CountNowPlayingFiles);

                        bool FoundNextSong = false;
                        int NBSongsPassed=0;
                        do
                        {
                            NBSongsPassed++;

                            NextSongIndex = CurrentSongIndex + 1;
                            NextSongArtist = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Artist);
                            NextSongTitle = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.TrackTitle);
                            NextSongBPM = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.BeatsPerMin);
                            NextSongKey = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Custom1);
                            NextSongEnergy = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Custom2);
                            NextSongRating = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Rating);
                            NextSongGenre = mbApiInterface.NowPlayingList_GetFileTag(NextSongIndex, MetaDataType.Genre);
                            NextSongURL = mbApiInterface.NowPlayingList_GetFileProperty(NextSongIndex, FilePropertyType.Url);

                            if (NBSongsPassed >= CountNowPlayingFiles.Length)
                            {
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
                                    Trace.TraceWarning("Skipping Song without GENRE : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }

                            if (allowharmonickey)
                            {
                                if (NextSongKey != string.Empty)
                                {
                                    if ((CurrentSongKey.IsIn("1A", "6m")) && NextSongKey.IsIn("1B", "12A", "1A", "2A", "6d", "5m", "6m", "7m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("2A", "7m")) && NextSongKey.IsIn("2B", "1A", "2A", "3A", "7d", "6m", "7m", "8m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("3A", "8m")) && NextSongKey.IsIn("3B", "2A", "3A", "4A", "8d", "7m", "8m", "9m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("4A", "9m")) && NextSongKey.IsIn("4B", "3A", "4A", "5A", "9d", "8m", "9m", "10m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("5A", "10m")) && NextSongKey.IsIn("5B", "4A", "5A", "6A", "10d", "9m", "10m", "11m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("6A", "11m")) && NextSongKey.IsIn("6B", "5A", "6A", "7A", "11d", "10m", "11m", "12m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("7A", "12m")) && NextSongKey.IsIn("7B", "6A", "7A", "8A", "12d", "11m", "12m", "1m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("8A", "1m")) && NextSongKey.IsIn("8B", "7A", "8A", "9A", "1d", "12m", "1m", "2m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("9A", "2m")) && NextSongKey.IsIn("9B", "8A", "9A", "10A", "2d", "1m", "2m", "3m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("10A", "3m")) && NextSongKey.IsIn("10B", "9A", "10A", "11A", "3d", "2m", "3m", "4m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("11A", "4m")) && NextSongKey.IsIn("11B", "10A", "11A", "12A", "4d", "3m", "4m", "5m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("12A", "5m")) && NextSongKey.IsIn("12B", "11A", "12A", "1A", "5d", "4m", "5m", "6m")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("1B", "6d")) && NextSongKey.IsIn("1A", "12B", "1B", "2B", "6m", "5d", "6d", "7d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("2B", "7d")) && NextSongKey.IsIn("2A", "1B", "2B", "3B", "7m", "6d", "7d", "8d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("3B", "8d")) && NextSongKey.IsIn("3A", "2B", "3B", "4B", "8m", "7d", "8d", "9d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("4B", "9d")) && NextSongKey.IsIn("4A", "3B", "4B", "5B", "9m", "8d", "9d", "10d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("5B", "10d")) && NextSongKey.IsIn("5A", "4B", "5B", "6B", "10m", "9d", "10d", "11d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("6B", "11d")) && NextSongKey.IsIn("6A", "5B", "6B", "7B", "11m", "10d", "11d", "12d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("7B", "12d")) && NextSongKey.IsIn("7A", "6B", "7B", "8B", "12m", "11d", "12d", "1d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("8B", "1d")) && NextSongKey.IsIn("8A", "7B", "8B", "9B", "1m", "12d", "1d", "2d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("9B", "2d")) && NextSongKey.IsIn("9A", "8B", "9B", "10B", "2m", "1d", "2d", "3d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("10B", "3d")) && NextSongKey.IsIn("10A", "9B", "10B", "11B", "3m", "2d", "3d", "4d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("11B", "4d")) && NextSongKey.IsIn("11A", "10B", "11B", "12B", "4m", "3d", "4d", "5d")) FoundNextSong = true;
                                    else if ((CurrentSongKey.IsIn("12B", "5d")) && NextSongKey.IsIn("12A", "11B", "12B", "1B", "5m", "4d", "5d", "6d")) FoundNextSong = true;
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
                                    Trace.TraceWarning("Skipping Song without KEY : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }

                            if (allowbpm)
                            {
                                if (NextSongBPM != string.Empty)
                                {
                                    int BPMDiff = Math.Abs(int.Parse(CurrentSongBPM) - int.Parse(NextSongBPM));

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
                                    Trace.TraceWarning("Skipping Song without BPM : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
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
                                    Trace.TraceWarning("Skipping Song without Energy : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
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
                                    Trace.TraceWarning("Skipping Song without Ratings : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                                    FoundNextSong = false;
                                    mbApiInterface.NowPlayingList_RemoveAt(NextSongIndex);
                                    continue;
                                }
                            }
                        } while (!FoundNextSong);

                        if (NBSongsPassed >= CountNowPlayingFiles.Length) { Trace.TraceInformation("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:" + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after nothing match your criteria"); }
                        else
                        {
                            if (savesongsplaylist)
                            {
                                bool playlistexist;
                                mbPlaylistSongFiles[0] = CurrentSongURL;

                                if (MusicBeeisportable) playlistexist = File.Exists(Application.StartupPath + "\\Library\\Playlists\\" + playlistName + ".mbp");
                                else playlistexist = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\Playlists\\" + playlistName + ".mbp");

                                if (isfirstsong || !playlistexist)
                                {
                                    Trace.TraceInformation("Creating playlist " + playlistName + "...");                                 
                                    mbApiInterface.Playlist_CreatePlaylist("", playlistName, mbPlaylistSongFiles);
                                    isfirstsong = false;
                                }
                                else
                                {
                                    Trace.TraceInformation("Adding song to playlist " + playlistName + "...");
                                    if (MusicBeeisportable) mbApiInterface.Playlist_AppendFiles(Application.StartupPath + "\\Library\\Playlists\\" + playlistName + ".mbp", mbPlaylistSongFiles);
                                    else mbApiInterface.Playlist_AppendFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\Playlists\\" + playlistName + ".mbp", mbPlaylistSongFiles);
                                }
                            }

                            Trace.TraceInformation("Current Song : " + CurrentSongArtist + "-" + CurrentSongTitle + " [BPM:" + CurrentSongBPM + " - KEY:" + CurrentSongKey + " - ENERGY:"  + CurrentSongEnergy + " - RATING:" + CurrentSongRating + " - GENRE:'" + CurrentSongGenre + "'] and " + NBSongsPassed + " songs after -> Next Song : " + NextSongArtist + "-" + NextSongTitle + " [BPM:" + NextSongBPM + " - KEY:" + NextSongKey + " - ENERGY:" + NextSongEnergy + " - RATING:" + NextSongRating + " - GENRE:'" + NextSongGenre + "']");
                        }
                    }

                    break;
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (isSettingsChanged) { Trace.TraceInformation("Settings changed. Reloading settings..."); }
                else { Trace.TraceInformation("Loading settings..."); }

                Boolean.TryParse(ini.Read("ALLOWBPM", "BPM"), out allowbpm);
                DiffBPM = int.Parse(ini.Read("DIFFBPM", "BPM"));
                Boolean.TryParse(ini.Read("ALLOWHARMONICKEY", "HARMONICKEY"), out allowharmonickey);
                Boolean.TryParse(ini.Read("ALLOWENERGY", "ENERGY"), out allowenergy);
                minenergy = int.Parse(ini.Read("MINENERGY", "ENERGY"));
                Boolean.TryParse(ini.Read("ALLOWRATINGS", "RATINGS"), out allowratings);
                minrattings = int.Parse(ini.Read("MINRATINGS", "RATINGS"));
                Boolean.TryParse(ini.Read("ALLOWGENRES", "GENRES"), out allowgenres);
                string genresallowed = ini.Read("GENRESSELECTED", "GENRES");
                genresAllowed = genresallowed.Split(',').ToArray();
                Boolean.TryParse(ini.Read("SAVESONGSPLAYLIST", "PLAYLIST"), out savesongsplaylist);
                numbersongsplaylist = int.Parse(ini.Read("NUMBERSONGSPLAYLIST", "PLAYLIST"));
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

                Trace.TraceInformation("Settings : ALLOWBPM=" + allowbpm + "[Max Diff " + DiffBPM + "] - ALLOWHARMONICKEY=" + allowharmonickey + " - ALLOWENERGY = " + allowenergy + "[Min " + minenergy + "] - ALLOWRATINGS=" + allowratings + "[Min " + minrattings + "] - ALLOWGENRES=" + allowgenres + "[" + genresallowed + "] - SAVESONGSPLAYLIST=" + savesongsplaylist + " - NUMBERSONGSPLAYLIST=" + numbersongsplaylist +" - ALLOWHUE=" + allowhue + "[Change lights when " + changelightswhen + "] - LIGHTSALLOWED=" + ini.Read("LIGHTSALLOWED", "HUE") + " - BRIGHTNESSLIGHTSMIN=" + brightnesslightmin + " - BRIGHTNESSLIGHTSMAX=" + brightnesslightmax + " - BEATDETECTIONEVERY=" + BeatDetectionEvery + "ms - DISABLELOGGING=" + disablelogging);
                isSettingsChanged = false;

                if (allowhue && APIKey != string.Empty) { settings.InitBridge(); }
                else if (allowhue && APIKey == string.Empty) MessageBox.Show("Hue is allowed but I can't found API Key !!!!");
            }
            catch
            {
                if (MusicBeeisportable) if (!File.Exists(Application.StartupPath + "\\Plugins\\mb_LikeADJ.ini")) Trace.TraceInformation("No ini file " + Application.StartupPath + "\\Plugins\\mb_LikeADJ.ini found.");
                else if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.ini")) Trace.TraceInformation("No ini file " + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music\\MusicBee\\mb_LikeADJ.ini found.");
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
                Trace.TraceInformation("Red alert of ending song (15s) engaged at " + string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds));
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
            if (!disablelogging) Trace.TraceInformation("Changing color of light " + lightIDX + " to (" + R + "," + G + "," + B + ") and brightness to " + lightBrightness + "...");
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