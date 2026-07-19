using LinePutScript;
using LinePutScript.Dictionary;
using System;
using System.Windows.Media.Imaging;

namespace VPet_Simulator.Windows.Interface
{
    /// <summary>
    /// Resource set
    /// </summary>
    public class Resources : LPS_D
    {
        public Resources() { }
        /// <summary>
        /// Add a resource; later ones override earlier ones
        /// </summary>
        /// <param name="line">Resource line</param>
        /// <param name="modpath">Feature location</param>
        public void AddSource(ILine line, string modpath)
        {
            ISub source = line.Find("source");
            if (source == null)
                return;
            //else if (!source.Info.Contains(":\\"))
            source.Info = modpath + '\\' + source.Info;
            line.Name = line.Name.ToLowerInvariant();
            AddorReplaceLine(line);
        }
        /// <summary>
        /// Add a resource; later ones override earlier ones
        /// </summary>
        /// <param name="line">Resource line</param>
        public void AddSource(ILine line)
        {
            ISub source = line.Find("source");
            if (source == null)
                return;
            //else if (!source.Info.Contains(":\\"))
            line.Name = line.Name.ToLowerInvariant();
            AddorReplaceLine(line);
        }
        /// <summary>
        /// Add multiple resources; later ones override earlier ones
        /// </summary>
        /// <param name="lps">Resource table</param>
        public void AddSources(ILPS lps)
        {
            foreach (ILine line in lps)
            {
                line.Name = line.Name.ToLowerInvariant();
                AddSource(line);
            }
        }
        /// <summary>
        /// Add multiple resources; later ones override earlier ones
        /// </summary>
        /// <param name="lps">Resource table</param>
        /// <param name="modpath">Feature location</param>
        public void AddSources(ILPS lps, string modpath = "")
        {
            foreach (ILine line in lps)
            {
                AddSource(line, modpath);
            }
        }
        /// <summary>
        /// Add a resource; later ones override earlier ones
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="path">Resource location</param>
        public void AddSource(string name, string path)
        {
            AddorReplaceLine(new Line(name.ToLowerInvariant(), "", "", new Sub("source", path)));
        }
        /// <summary>
        /// Find a resource
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="nofind">Value returned if not found</param>
        /// <returns>The resource location, or nofind if not found</returns>
        public string FindSource(string name, string nofind = null)
        {
            ILine line = FindLine(name.ToLowerInvariant());
            if (line == null)
                return nofind;
            return line.Find("source").Info;
        }
        /// <summary>
        /// Find a resource
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="nofind">Value returned if not found</param>
        /// <returns>The resource location, or nofind if not found</returns>
        public Uri FindSourceUri(string name, string nofind = null)
        {
            ILine line = FindLine(name.ToLowerInvariant());
            if (line == null)
                if (nofind != null)
                    return new Uri(nofind);
                else
                    return null;
            return new Uri(line.Find("source").Info);
        }
    }

    /// <summary>
    /// Image resource collection
    /// </summary>
    public class ImageResources : Resources
    {
        public ImageResources()
        {

        }
        /// <summary>
        /// Add an image set; later ones override earlier ones
        /// </summary>
        /// <param name="lps">Image set</param>
        /// <param name="modpath">Folder location</param>
        public void AddImages(LpsDocument lps, string modpath = "") => AddSources(lps, modpath);
        /// <summary>
        /// Add a single image; later ones override earlier ones
        /// </summary>
        /// <param name="line">Image line</param>
        /// <param name="modpath">Folder location</param>
        public void AddImage(ILine line, string modpath = "") => AddSource(line, modpath);
        /// <summary>
        /// Find an image resource
        /// </summary>
        /// <param name="imagename">Image name</param>
        /// <returns>The image resource, or an error placeholder image if not found</returns>
        public BitmapImage FindImage(string imagename) => NewSafeBitmapImage(FindImageUri(imagename));

        public Uri FindImageUri(string imagename)
        {
#if DEBUGs
            var v = FindSourceUri(imagename, "pack://application:,,,/Res/Image/system/error.png");
            if (v.AbsoluteUri == "pack://application:,,,/Res/Image/system/error.png")
                throw new Exception($"image nofound {imagename}");
            return v;
#else
            return FindSourceUri(imagename, "pack://application:,,,/Res/img/error.png");
#endif
        }

        /// <summary>
        /// Find an image resource; fall back to the superior if not found
        /// </summary>
        /// <param name="imagename">Image name</param>
        /// <returns>The image resource, or an error placeholder image if not found</returns>
        /// <param name="superior">Superior image; used when there is no dedicated image</param>
        public BitmapImage FindImage(string imagename, string superior)
        {
            string source = FindSource(imagename);
            if (source == null)
            {
                return NewSafeBitmapImage(FindImageUri(superior));
            }
            return NewSafeBitmapImage(source);
        }
        /// <summary>
        /// Image settings (e.g. anchor points, etc.)
        /// </summary>
        public LpsDocument ImageSetting = new LpsDocument();
        /// <summary>
        /// Safer image URI loading
        /// </summary>
        /// <param name="source">Image source</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage NewSafeBitmapImage(string source) => NewSafeBitmapImage(new Uri(source));
        /// <summary>
        /// Safer image URI loading
        /// </summary>
        /// <param name="source">Image source</param>
        /// <returns>BitmapImage</returns>
        public static BitmapImage NewSafeBitmapImage(Uri source)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bi.UriSource = source;
            bi.EndInit();
            return bi;
        }
    }
}
