using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static Graphlit.FreeImage;
using static Graphlit.FreeImagePacking;
using System.IO;

namespace Graphlit
{
    [InitializeOnLoad]
    public static class FreeImagePacking
    {
        static FreeImagePacking()
        {
            SetOutputMessage(LogMessage);
        }

        public static FREE_IMAGE_FILTER ImageFilter = FREE_IMAGE_FILTER.Bilinear;
        public static TexturePackingFormat PackingFormat = TexturePackingFormat.tga;


        public static void PackAlbedoAlpha(string destinationPath, string albedoPath, string alphaPath, ChannelSource alphaSource, bool invertAlpha = false)
        {
            var albedoTex = FreeImage_Load(albedoPath);
            var alphaTex = FreeImage_Load(alphaPath);
            var albedoWh = GetWithAndHeight(albedoTex);
            var alphaWh = GetWithAndHeight(alphaTex);

            if (alphaWh.Item1 != albedoWh.Item1 || alphaWh.Item2 != albedoWh.Item2)
            {
                alphaTex = Rescale(alphaTex, (int)albedoWh.Item1, (int)albedoWh.Item2, ImageFilter);
            }

            albedoTex = ConvertTo32Bits(albedoTex);

            uint bpp = GetBPP(alphaTex);
            if (bpp > 16)
            {
                var source = ChannelSourceToFreeImage(alphaSource);
                alphaTex = GetChannel(alphaTex, source);
            }
            if (bpp == 16)
            {
                alphaTex = ConvertTo8Bits(alphaTex);
            }

            if (invertAlpha)
            {
                Invert(alphaTex);
            }

            SetChannel(albedoTex, alphaTex, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);

            FreeImage_Save((ImageFormat)PackingFormat, albedoTex, destinationPath);

            FreeImage_Unload(albedoTex);
            FreeImage_Unload(alphaTex);
        }

        public struct TextureChannel
        {
            [CanBeNull] public string Path;
            public bool Invert;
            public ChannelSource Source;
            public DefaultColor DefaultColor;
        }

        public enum DefaultColor
        {
            White,
            Black
        }

        private static void HandleTextureChannel(TextureChannel textureChannel, (int, int) widthHeight, IntPtr newImage, FREE_IMAGE_COLOR_CHANNEL newChannel)
        {
            var whiteTex = "Packages/com.z3y.shaders/Editor/white.png";

            if (string.IsNullOrEmpty(textureChannel.Path))
            {
                textureChannel.Path = whiteTex;
                textureChannel.Invert = textureChannel.DefaultColor == DefaultColor.Black;
                /*if (textureChannel.DefaultColor == DefaultColor.White)
                {
                    var ch = GetChannel(newImage, ChannelSourceToFreeImage(textureChannel.Source));
                    Invert(ch);
                    SetChannel(newImage, ch, newChannel);
                }
                return;*/
            }

            var ptr = FreeImage_Load(textureChannel.Path);
            var size = GetWithAndHeight(ptr);
            //ptr = ConvertTo32Bits(ptr);

            uint bpp = GetBPP(ptr);
            if (bpp > 16)
            {
                ptr = GetChannel(ptr, ChannelSourceToFreeImage(textureChannel.Source));
            }

            if (bpp == 16)
            {
                ptr = ConvertTo8Bits(ptr);
            }

            bool grayscale = false;
            if (GetBPP(ptr) != 8)
            {
                ptr = ConvertToGreyscale(ptr);
                grayscale = true;
            }

            if (size.Item1 != widthHeight.Item1 || size.Item2 != widthHeight.Item2)
            {
                ptr = Rescale(ptr, widthHeight.Item1, widthHeight.Item2, ImageFilter);
            }

            if (textureChannel.Invert)
            {
                Invert(ptr);
            }

            bool success = SetChannel(newImage, ptr, newChannel);
            if (!success && !grayscale)
            {
                ptr = ConvertToGreyscale(ptr);
                SetChannel(newImage, ptr, newChannel);
            }

            FreeImage_Unload(ptr);
        }

        public static void PackCustom(string destinationPath, TextureChannel textureChannelR, TextureChannel textureChannelG, TextureChannel textureChannelB, TextureChannel textureChannelA, (int, int) widthHeight, TexturePackingFormat format)
        {
            var sw = new Stopwatch();
            sw.Start();

            bool hasAlpha = !string.IsNullOrEmpty(textureChannelA.Path);
            IntPtr newTexture = Allocate(widthHeight.Item1, widthHeight.Item2, hasAlpha ? 32 : 24);

            // if empty use white tex;
            HandleTextureChannel(textureChannelR, widthHeight, newTexture, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);
            HandleTextureChannel(textureChannelG, widthHeight, newTexture, FREE_IMAGE_COLOR_CHANNEL.FICC_GREEN);
            HandleTextureChannel(textureChannelB, widthHeight, newTexture, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
            if (hasAlpha) HandleTextureChannel(textureChannelA, widthHeight, newTexture, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);

            FreeImage_Save((ImageFormat)format, newTexture, destinationPath);

            FreeImage_Unload(newTexture);


            sw.Stop();
            FreeImagePackingEditor.LastPackingTime = (int)sw.ElapsedMilliseconds;
        }

        public static string GetExtension(this TexturePackingFormat format)
        {
            return Enum.GetName(typeof(TexturePackingFormat), format);
        }

        public enum TexturePackingFormat
        {
            tga = 17,
            psd = 20,
            tiff = 18,
            png = 13,
        }

        public enum TextureSize
        {
            Default = 0,
            Custom = -1,
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
        }

        public enum ChannelSource
        {
            Red,
            Green,
            Blue,
            Alpha
        }

        public static FREE_IMAGE_COLOR_CHANNEL ChannelSourceToFreeImage(ChannelSource channelSource)
        {
            switch (channelSource)
            {
                case ChannelSource.Red:
                    return FREE_IMAGE_COLOR_CHANNEL.FICC_RED;
                case ChannelSource.Green:
                    return FREE_IMAGE_COLOR_CHANNEL.FICC_GREEN;
                case ChannelSource.Blue:
                    return FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE;
                case ChannelSource.Alpha:
                    return FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channelSource), channelSource, null);
            }
        }

        private static void LogMessage(FreeImage.ImageFormat fif, string message)
        {
            Debug.Log(message);
        }
    }
}