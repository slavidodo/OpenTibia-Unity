using UnityEngine;

namespace OpenTibiaUnity.Core
{
    public static class Colors
    {
        public static Color Default = ColorFromRGB(0xC0C0C0);
        public static Color DefaultDisabled = ColorFromRGB(0x6F6F6F);

        public static Color White = ColorFromRGB(0xFFFFFF);
        public static Color DarkRed = ColorFromRGB(0xE04040);
        public static Color Red = ColorFromRGB(0xF55E5E);
        public static Color LightRed = ColorFromRGB(0xF8A4A4);
        public static Color DarkGreen = ColorFromRGB(0x40E040);
        public static Color Green = ColorFromRGB(0x00EB00);
        public static Color LightGreen = ColorFromRGB(0xB4F8B4);

        public const int HSI_SI_VALUES = 7;
        public const int HSI_H_STEPS = 19;

        private static readonly Color?[] s_ColorsCache = new Color?[HSI_SI_VALUES * HSI_H_STEPS + 1];

        public static Color ColorFromHSI(int color) {
            if (color >= HSI_H_STEPS * HSI_SI_VALUES)
                color = 0;

            if (s_ColorsCache[color].HasValue)
                return s_ColorsCache[color].Value;
            
            float loc1, loc2, loc3;
            if (color % HSI_H_STEPS != 0) {
                loc1 = color % HSI_H_STEPS * 1.0f / 18.0f;
                loc2 = 1;
                loc3 = 1;

                switch (color / HSI_H_STEPS) {
                    case 0: loc2 = 0.25f; loc3 = 1.00f; break;
                    case 1: loc2 = 0.25f; loc3 = 0.75f; break;
                    case 2:  loc2 = 0.50f;  loc3 = 0.75f; break;
                    case 3: loc2 = 0.667f; loc3 = 0.75f; break;
                    case 4: loc2 = 1.00f; loc3 = 1.00f; break;
                    case 5: loc2 = 1.00f; loc3 = 0.75f; break;
                    case 6: loc2 = 1.00f; loc3 = 0.50f;  break;
                }
            } else {
                loc1 = 0;
                loc2 = 0;
                loc3 = 1 - (float)color / HSI_H_STEPS / HSI_SI_VALUES;
            }

            if (loc3 == 0)
                return new Color(0, 0, 0);
             else if (loc2 == 0)
                return new Color(loc3, loc3, loc3);

            float red = 0, green = 0, blue = 0;
            if (loc1 < 1.0 / 6.0) {
                red = loc3;
                blue = loc3 * (1 - loc2);
                green = blue + (loc3 - blue) * 6 * loc1;
            } else if (loc1 < 2.0 / 6.0) {
                green = loc3;
                blue = loc3 * (1 - loc2);
                red = green - (loc3 - blue) * (6 * loc1 - 1);
            } else if (loc1 < 3.0 / 6.0) {
                green = loc3;
                red = loc3 * (1 - loc2);
                blue = red + (loc3 - red) * (6 * loc1 - 2);
            } else if (loc1 < 4.0 / 6.0) {
                blue = loc3;
                red = loc3 * (1 - loc2);
                green = blue - (loc3 - red) * (6 * loc1 - 3);
            } else if (loc1 < 5.0 / 6.0) {
                blue = loc3;
                green = loc3 * (1 - loc2);
                red = green + (loc3 - green) * (6 * loc1 - 4);
            } else {
                red = loc3;
                green = loc3 * (1 - loc2);
                blue = red - (loc3 - green) * (6 * loc1 - 5);
            }

            s_ColorsCache[color] = new Color(red, green, blue);
            return new Color(red, green, blue);
        }

        public static Color ColorFromRGBA(byte r, byte g, byte b, byte a) {
            return new Color32(r, g, b, a);
        }

        public static Color ColorFromARGB(uint ARGB) {
            uint a = ARGB >> 24;
            uint r = ARGB >> 16;
            uint g = ARGB >> 8;
            uint b = ARGB;

            return ColorFromRGBA((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static Color ColorFromRGB(byte r, byte g, byte b) {
            return new Color32(r, g, b, 255);
        }

        public static Color ColorFromRGB(uint rgb) {
            uint r = rgb >> 16;
            uint g = rgb >> 8;
            uint b = rgb;

            return ColorFromRGB((byte)r, (byte)g, (byte)b);
        }

        public static Color ColorFrom8Bit(int eightBit) {
            if (eightBit < 0 || eightBit >= 216)
                return Color.black;

            int r = (int)(eightBit / 36f) % 6 * 51;
            int g = (int)(eightBit / 6f) % 6 * 51;
            int b = eightBit % 6 * 51;
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static int EightBitFromColor(Color color) {
            int eightBit = 0;
            eightBit += ((int)(color.r * 255) / 51) * 36;
            eightBit += ((int)(color.g * 255) / 51) * 6;
            eightBit += ((int)(color.b * 255) / 51);
            return eightBit;
        }

        private static uint ARGBFormatInternal(uint r, uint g, uint b, uint a) {
            return a << 24 | r << 16 | g << 8 | b;
        }

        private static uint RGBFormatInternal(uint r, uint g, uint b) {
            return r << 16 | g << 8 | b;
        }

        public static uint ARGBFrom8Bit(uint eightBit) {
            uint r = (eightBit / 36) % 6 * 51;
            uint g = (eightBit / 6) % 6 * 51;
            uint b = eightBit % 6 * 51;
            return ARGBFormatInternal(r, g, b, 255U);
        }

        public static uint ARGBFromColor(Color color) {
            uint r = (uint)(color.r * 255);
            uint g = (uint)(color.g * 255);
            uint b = (uint)(color.b * 255);
            uint a = (uint)(color.a * 255);
            return ARGBFormatInternal(r, g, b, a);
        }

        public static uint RGBFromColor(Color color) {
            uint r = (uint)(color.r * 255);
            uint g = (uint)(color.g * 255);
            uint b = (uint)(color.b * 255);
            return RGBFormatInternal(r, g, b);
        }

        public static uint RGBFrom8Bit(uint eightBit) {
            uint r = (eightBit / 36) % 6 * 51;
            uint g = (eightBit / 6) % 6 * 51;
            uint b = eightBit % 6 * 51;
            return RGBFormatInternal(r, g, b);
        }
    }
}
