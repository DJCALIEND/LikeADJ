# LikeADJ

**A new way to become a DJ...** Plugin for [MusicBee](https://getmusicbee.com/).
See my thread [LikeADJ on MusicBee](https://getmusicbee.com/forum/index.php?topic=24631.0).

The **main release** is [mb_LikeADJ.dll 2.1.0](https://github.com/DJCALIEND/LikeADJ/releases/download/2.1.0/mb_LikeADJ.dll). 

## Introduction

This plugin can automix your entire library according to:
1. BPM (Beats Per Minute)
2. Initial Key (only Camelot and/or Short/Long Open Key notations)
3. Energy
4. Ratings
5. Love
6. Genres

With **[Hue lighting](https://www2.meethue.com/)** (with beat detection based on spectrum data of your songs).

## Installation

Download lastest release or developpment version of mb_LikeADJ in your MusicBee\Plugins directory, activate the plugin and configure it. 

## Configuration

More to come with images...

## How to start LikeADJ...

More to come with images...

## How this plugin works !!!

When LikeADJ is started, LikeADJ add shufflelized (not by MusicBee but via my own way) your entire library in the NowPlaylingPlaylist and start to analyse each song sequentially.
When the first song is found depending of your criterias, LikeADJ analyse if the next song in the NowPlayingList is compatible.
If the next song is compatible, LikeADJ create a playlist (or not if your have not checked this option) and try to find the next song compatible following your choices.

The pilosophy:
1. All songs whithout required TAGs (depending of your choices) are removed from the NowPlaylingPlayist (So the NowPlaylingPlaylist can decreased quickly if no required tags are filled).
2. All songs with all required TAGs (depending of your choices) but not matching with the previous song are moved at the end of the NowPlayingList.
3. LikeADJ, each time a new song is found, analyse how many mixable songs are remaining (see mb_LikeADJ.log, you need **mb_LikeADJ.dll 2.1.1** under development).

For **more informations**, don't hesitate to see mb_LikeADJ.log. It will be required by me if you have a problem with LikeADJ.

Each time you execute LikeADJ, it's another playlist to listen.

More to come with images...

## Compilation

You need [Visual Studio Community 2022](https://visualstudio.microsoft.com/fr/vs/community/) & Microsoft Framework.NET 4.8 (free if you have a Microsoft account) in order to compile LikeADJ.

## Participate

If you're interested, help for code optimization, bugs fixing or adding new features is welcome.
Don't forget to add a **STAR** on this project, if you like my little contribution for MusicBee of course.

## How to contact me

More to come...

## A lot of thanks to...

1. **Steven²** (of course) for MusicBee.
2. All **contributors** of MusicBee.
3. [BeatDetection (archive.gamedev.net)](http://archive.gamedev.net/archive/reference/programming/features/beatdetection/index.html)
4. [BeatDetection (Roseburrow)](https://github.com/Roseburrow/Beat-Detection)
5. [BeatDetection (DrXoo)](https://github.com/DrXoo/BeatDetection)
6. [Hue (HueDesktop)](https://github.com/5inay/HueDesktop)
7. [ComboBox (www.codeproject.com)](https://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown?msg=4152597#xx4152597xx)
8. [Trackbar (www.codeproject.com)](https://www.codeproject.com/Questions/158467/TrackBar-with-selection-and-range-thumbs-possibl)

