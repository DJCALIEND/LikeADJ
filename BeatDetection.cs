using System;
using System.Diagnostics;
using System.Drawing;
using System.Timers;

namespace MusicBeePlugin
{
    class BeatDetection
    {
        public static float[] samplesLeftChannel = new float[1024];
        public static float[] samplesRightChannel = new float[1024];

        public static float[] localHistory = new float[43];
        public static float InstantEnergy;
        public static float AverageLocalEnergy;
        public static float EnergyVariance;
        public static float Sensitivity;

        public static SubBand[] SubBands { get; set; }

        public static void IsBeatDetectedSimple(object sender, ElapsedEventArgs e)
        {
            float[] fftdata = new float[2048];
            var ret = Plugin.mbApiInterface.NowPlaying_GetSpectrumData(fftdata);

            for (int i = 0; i < 1024; i++) { samplesLeftChannel[i] = fftdata[i]; }
            for (int i = 1024; i < 2048; i++) samplesRightChannel[i - 1024] = fftdata[i];

            float instantEnergy = 0f;
            for (int i = 0; i < 1024; i++) { instantEnergy += (float)Math.Pow(samplesLeftChannel[i], 2) + (float)Math.Pow(samplesRightChannel[i], 2); }
            InstantEnergy = instantEnergy;

            float averageLocalEnergy = 0f;
            for (int i = 0; i < localHistory.Length; i++) { averageLocalEnergy += localHistory[i]; }
            AverageLocalEnergy = averageLocalEnergy / localHistory.Length;

            float variance = 0f;
            for (int i = 0; i < localHistory.Length; i++) { variance += (float)Math.Pow(localHistory[i] - AverageLocalEnergy, 2); }
            EnergyVariance = variance / localHistory.Length;

            Sensitivity = (0.0025714f * EnergyVariance) + 1.5142857f;

            float[] result = new float[localHistory.Length];
            for (int i = 1; i < localHistory.Length - 1; i++) { result[i] = localHistory[i - 1]; }
            localHistory = result;
            localHistory[0] = InstantEnergy;

            if (InstantEnergy > (AverageLocalEnergy * Sensitivity))
            {
                int songposition = Plugin.mbApiInterface.Player_GetPosition();
                TimeSpan elapsed = TimeSpan.FromMilliseconds(songposition);
                
                if (!Plugin.disablelogging) Plugin.Logger.Info("Beat detected at " + string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds));
                for (int i = 0; i < Plugin.lightIndicesAllowed.Length; i++)
                {
                    if (Plugin.lightIndicesAllowed[i] != 0)
                    {
                        Color color = Color.FromArgb(Plugin.rand.Next(256), Plugin.rand.Next(256), Plugin.rand.Next(256));
                        Plugin.LightChangeColorandBrightness(Plugin.lightIndicesAllowed[i], color.R, color.G, color.B, Plugin.rand.Next(Plugin.brightnesslightmin, Plugin.brightnesslightmax));
                    }
                }
            }
        }

        public static void IsBeatDetectedSubBand(object sender, ElapsedEventArgs e)
        {
            float[] fftdata = new float[2048];
            var ret = Plugin.mbApiInterface.NowPlaying_GetSpectrumData(fftdata);

            for (int i = 0; i < 1024; i++) { samplesLeftChannel[i] = fftdata[i]; }
            for (int i = 1024; i < 2048; i++) samplesRightChannel[i - 1024] = fftdata[i];

            bool asbeated = false;
            for (int i = 0; i < SubBands.Length; i++)
            {
                int startPoint = 0;
                for (int j = 0; j <= i - 1; j++) { startPoint += SubBands[j].frequencyWidth; }
                int endPoint = 0;
                for (int j = 0; j <= i; j++) { endPoint += SubBands[j].frequencyWidth; }

                SubBands[i].ComputeInstantEnergy(startPoint, endPoint, samplesLeftChannel, samplesRightChannel);
                SubBands[i].ComputeAverageEnergy();
                SubBands[i].ComputeInstantVariance();

                if (SubBands[i].HasBeated() && !asbeated)
                {
                    int songposition = Plugin.mbApiInterface.Player_GetPosition();
                    TimeSpan elapsed = TimeSpan.FromMilliseconds(songposition);

                    if (!Plugin.disablelogging) Plugin.Logger.Info("Beat detected at " + string.Format("{0:D2}:{1:D2}:{2:D2}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds));
                    for (int j = 0; j < Plugin.lightIndicesAllowed.Length; j++)
                    {
                        if (Plugin.lightIndicesAllowed[j] != 0)
                        {
                            Color color = Color.FromArgb(Plugin.rand.Next(256), Plugin.rand.Next(256), Plugin.rand.Next(256));
                            Plugin.LightChangeColorandBrightness(Plugin.lightIndicesAllowed[j], color.R, color.G, color.B, Plugin.rand.Next(Plugin.brightnesslightmin, Plugin.brightnesslightmax));
                        }
                    }
                    asbeated = true;
                }
            }
        }

        public class SubBand
        {
            public float instantEnergy;
            public float averageEnergy;
            public float instantVariance;
            public int frequencyWidth;
            private readonly float[] historyBuffer;

            public SubBand(int index)
            {
                frequencyWidth = (int)Math.Round(0.44f * index + 1.56f);
                historyBuffer = new float[43];
            }

            public void ComputeInstantEnergy(int start, int end, float[] samples0, float[] samples1)
            {
                float result = 0;
                for (int i = start; i < end; i++) { result += (float)Math.Pow(samples0[i], 2) + (float)Math.Pow(samples1[i], 2); }
                instantEnergy = result;
            }

            public void ComputeAverageEnergy()
            {
                float result = 0;
                for (int i = 0; i < historyBuffer.Length; i++) { result += historyBuffer[i]; }
                averageEnergy = result / historyBuffer.Length;
                float[] shiftedHistoryBuffer = ShiftArray(historyBuffer, 1);
                shiftedHistoryBuffer[0] = instantEnergy;
                OverrideElementsToAnotherArray(shiftedHistoryBuffer, historyBuffer);
            }

            public void ComputeInstantVariance()
            {
                float result = 0;
                for (int i = 0; i < historyBuffer.Length; i++) { result += (float)Math.Pow(historyBuffer[i] - averageEnergy, 2); }
                instantVariance = result / historyBuffer.Length;
            }

            public bool HasBeated()
            {
                if (instantEnergy > (5 * averageEnergy) && instantVariance > 0.00001f) { return true; }
                return false;
            }

            private void OverrideElementsToAnotherArray(float[] from, float[] to)
            {
                for (int i = 0; i < from.Length; i++) { to[i] = from[i]; }
            }

            private float[] ShiftArray(float[] array, int amount)
            {
                float[] result = new float[array.Length];
                for (int i = 0; i < array.Length - amount; i++) { result[i + amount] = array[i]; }
                return result;
            }
        }
    }
}
