using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace MusicBeePlugin
{
    [Serializable]
    [XmlRoot("root", Namespace = "urn:schemas-upnp-org:device-1-0")]
    public class Hue
    {
        [XmlElement("specVersion")]
        public SpecVersion BridgeSpecVersion { get; set; }
        [XmlElement("URLBase")]
        public string BridgeURLBase { get; set; }
        [XmlElement("device")]
        public Device BridgeDeviceSpec { get; set; }
    }

    public class SpecVersion
    {
        [XmlElement("major")]
        public int MajorVersion { get; set; }
        [XmlElement("minor")]
        public int MinorVersion { get; set; }
    }
 
    public class Device
    {
        [XmlElement("deviceType")]
        public string BridgeType { get; set; }
        [XmlElement("friendlyName")]
        public string BridgeFriendlyName { get; set; }
        [XmlElement("manufacturer")]
        public string BridgeManufacturer { get; set; }
        [XmlElement("manufacturerURL")]
        public string BridgeManURL { get; set; }
        [XmlElement("modelDescription")]
        public string BridgeDescription { get; set; }
        [XmlElement("modelName")]
        public string BridgeModelName { get; set; }
        [XmlElement("modelNumber")]
        public string BridgeModelNumber { get; set; }
        [XmlElement("modelURL")]
        public string BridgeModelURL { get; set; }
        [XmlElement("serialNumber")]
        public string BridgeSerialNumber { get; set; }
        [XmlElement("UDN")]
        public string BridgeUDN { get; set; }
        [XmlElement("presentationURL")]
        public string BridgePresentationURL { get; set; }
    }

    public static class HueConstants
    {
        public static string BODY_POST_CONNECT = "{\"devicetype\":\"my_hue_app\"}";
    }

    class ColorConverter
    {
        private static readonly PointF rGamutB = new PointF(0.675f, 0.322f);
        private static readonly PointF gGamutB = new PointF(0.409f, 0.518f);
        private static readonly PointF bGamutB = new PointF(0.167f, 0.040f);
        private static readonly double RG = FindLength(rGamutB, gGamutB);
        private static readonly double GB = FindLength(gGamutB, bGamutB);
        private static readonly double BR = FindLength(bGamutB, rGamutB);
        private static readonly double areaOfGamutB = AreaOfTriangle(RG, GB, BR);

        public static void ColorToXY(double red, double green, double blue, out double X, out double Y)
        {
            red = (red > 0.04045) ? Math.Pow((red + 0.055) / (1.0 + 0.055), 2.4) : (red / 12.92);
            green = (green > 0.04045) ? Math.Pow((green + 0.055) / (1.0 + 0.055), 2.4) : (green / 12.92);
            blue = (blue > 0.04045) ? Math.Pow((blue + 0.055) / (1.0 + 0.055), 2.4) : (blue / 12.92);

            double EXX = red * 0.664511 + green * 0.154324 + blue * 0.162028;
            double WHY = red * 0.283881 + green * 0.668433 + blue * 0.047685;
            double ZEE = red * 0.000088 + green * 0.072310 + blue * 0.986039;

            X = EXX / (EXX + WHY + ZEE);
            Y = WHY / (EXX + WHY + ZEE);
        }

        private static double AreaOfTriangle(double a, double b, double c)
        {
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        private static double FindLength(PointF a, PointF b)
        {
            return (Math.Sqrt(Math.Pow((b.X - a.X), 2.0) + Math.Pow((b.Y - a.Y), 2.0)));
        }
    }

    public sealed class REST
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string ourIP;
 
        public REST(string mainIP)
        {
            ourIP = "http://" + mainIP;
        }

        public async Task<string> POST(string addendum, string body, Encoding e, string type)
        {
            string postEndpoint = ourIP + addendum;
            StringContent content = new StringContent(body, e, type);
            HttpResponseMessage response = await client.PostAsync(postEndpoint, content);
            if (response.StatusCode == HttpStatusCode.OK) { return await response.Content.ReadAsStringAsync(); }
            return "";
        }

        public async Task<string> GET(string addendum)
        {
            string getEndpoint = ourIP + addendum;
            HttpResponseMessage response = await client.GetAsync(getEndpoint).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK) { return await response.Content.ReadAsStringAsync(); }
            return "";
        }

        public async Task<string> PUT(string addendum, string body, Encoding e, string type)
        {
            string putEndpoint = ourIP + addendum;
            StringContent content = new StringContent(body, e, type);
            HttpResponseMessage response = await client.PutAsync(putEndpoint, content);
            if (response.StatusCode == HttpStatusCode.OK) { return await response.Content.ReadAsStringAsync(); }
            return "";
        }
    }

    public class JSONBridge
    {
        public string Id { get; set; }
        public string Internalipaddress { get; set; }
    }

    public class BridgeConnectionSuccess
    {
        public Success Success { get; set; }
    }

    public class Success
    {
        public string Username { get; set; }
    }

    public class State
    {
        public bool On { get; set; }
        public int Bri { get; set; }
        public int Hue { get; set; }
        public int Sat { get; set; }
        public string Effect { get; set; }
        public List<double> Xy { get; set; }
        public int Ct { get; set; }
        public string Alert { get; set; }
        public string Colormode { get; set; }
        public bool Reachable { get; set; }
    }

    public class HueLight
    {
        public State State { get; set; }
        public string Type { get; set; }
        public string Modelid { get; set; }
        public string Manufacturername { get; set; }
        public string Uniqueid { get; set; }
        public string Swversion { get; set; }
        public string Name { get; set; }
    }
}
