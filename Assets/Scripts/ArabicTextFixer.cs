using System;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// أداة تصحيح وتشكيل النصوص العربية في Unity (ArabicTextFixer).
/// تقوم بربط الحروف العربية ببعضها في أشكالها الصحيحة (بداية، وسط، نهاية، منفصل)
/// وعكس اتجاه القراءة من اليمين إلى اليسار (RTL) لتعرض اللغة العربية بأبهى وأجمل صورة.
/// </summary>
public static class ArabicTextFixer
{
    private struct ArabicCharInfo
    {
        public char isolated;
        public char finalForm;
        public char initialForm;
        public char medialForm;
        public bool connectsLeft;

        public ArabicCharInfo(char iso, char fin, char ini, char med, bool connLeft)
        {
            isolated = iso;
            finalForm = fin;
            initialForm = ini;
            medialForm = med;
            connectsLeft = connLeft;
        }
    }

    private static readonly Dictionary<char, ArabicCharInfo> CharMap = new Dictionary<char, ArabicCharInfo>()
    {
        // همزة وألف
        { 'ء', new ArabicCharInfo('\uFE80', '\uFE80', '\uFE80', '\uFE80', false) },
        { 'آ', new ArabicCharInfo('\uFE81', '\uFE82', '\uFE81', '\uFE82', false) },
        { 'أ', new ArabicCharInfo('\uFE83', '\uFE84', '\uFE83', '\uFE84', false) },
        { 'ؤ', new ArabicCharInfo('\uFE85', '\uFE86', '\uFE85', '\uFE86', false) },
        { 'إ', new ArabicCharInfo('\uFE87', '\uFE88', '\uFE87', '\uFE88', false) },
        { 'ئ', new ArabicCharInfo('\uFE89', '\uFE8A', '\uFE8B', '\uFE8C', true) },
        { 'ا', new ArabicCharInfo('\uFE8D', '\uFE8E', '\uFE8D', '\uFE8E', false) },
        
        // باء تا ثا
        { 'ب', new ArabicCharInfo('\uFE8F', '\uFE90', '\uFE91', '\uFE92', true) },
        { 'ة', new ArabicCharInfo('\uFE93', '\uFE94', '\uFE93', '\uFE94', false) },
        { 'ت', new ArabicCharInfo('\uFE95', '\uFE96', '\uFE97', '\uFE98', true) },
        { 'ث', new ArabicCharInfo('\uFE99', '\uFE9A', '\uFE9B', '\uFE9C', true) },
        
        // جيم حاء خاء
        { 'ج', new ArabicCharInfo('\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0', true) },
        { 'ح', new ArabicCharInfo('\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4', true) },
        { 'خ', new ArabicCharInfo('\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8', true) },
        
        // دال ذال راء زاي
        { 'د', new ArabicCharInfo('\uFEA9', '\uFEAA', '\uFEA9', '\uFEAA', false) },
        { 'ذ', new ArabicCharInfo('\uFEAB', '\uFEAC', '\uFEAB', '\uFEAC', false) },
        { 'ر', new ArabicCharInfo('\uFEAD', '\uFEAE', '\uFEAD', '\uFEAE', false) },
        { 'ز', new ArabicCharInfo('\uFEAF', '\uFEB0', '\uFEAF', '\uFEB0', false) },
        
        // سين شين
        { 'س', new ArabicCharInfo('\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4', true) },
        { 'ش', new ArabicCharInfo('\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8', true) },
        
        // صاد ضاد طاء ظاء
        { 'ص', new ArabicCharInfo('\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC', true) },
        { 'ض', new ArabicCharInfo('\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0', true) },
        { 'ط', new ArabicCharInfo('\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4', true) },
        { 'ظ', new ArabicCharInfo('\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8', true) },
        
        // عين غين
        { 'ع', new ArabicCharInfo('\uFEC9', '\uFECA', '\uFECB', '\uFECC', true) },
        { 'غ', new ArabicCharInfo('\uFECD', '\uFECE', '\uFECF', '\uFED0', true) },
        
        // فاء قاف كاف
        { 'ف', new ArabicCharInfo('\uFED1', '\uFED2', '\uFED3', '\uFED4', true) },
        { 'ق', new ArabicCharInfo('\uFED5', '\uFED6', '\uFED7', '\uFED8', true) },
        { 'ك', new ArabicCharInfo('\uFED9', '\uFEDA', '\uFEDB', '\uFEDC', true) },
        
        // لام ميم نون هاء واو ياء
        { 'ل', new ArabicCharInfo('\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0', true) },
        { 'م', new ArabicCharInfo('\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4', true) },
        { 'ن', new ArabicCharInfo('\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8', true) },
        { 'ه', new ArabicCharInfo('\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC', true) },
        { 'و', new ArabicCharInfo('\uFEED', '\uFEEE', '\uFEED', '\uFEEE', false) },
        { 'ى', new ArabicCharInfo('\uFEEF', '\uFEF0', '\uFEEF', '\uFEF0', false) },
        { 'ي', new ArabicCharInfo('\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4', true) },
    };

    public static string Fix(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // فحص هل النص يحتوي على حروف عربية
        bool hasArabic = false;
        foreach (char c in input)
        {
            if (c >= 0x0600 && c <= 0x06FF) { hasArabic = true; break; }
        }
        if (!hasArabic) return input; // النصوص الإنجليزية والفرنسية تترك كما هي

        // تقسيم النص إلى أسطر لمعالجة كل سطر بشكل منفصل
        string[] lines = input.Split('\n');
        StringBuilder result = new StringBuilder();

        for (int l = 0; l < lines.Length; l++)
        {
            if (l > 0) result.Append('\n');
            result.Append(ProcessLine(lines[l]));
        }

        return result.ToString();
    }

    private static string ProcessLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return line;

        // 1. تشكيل الحروف العربية إلى صورها الصحيحة (Initial, Medial, Final, Isolated)
        char[] shaped = new char[line.Length];
        for (int i = 0; i < line.Length; i++)
        {
            char current = line[i];

            if (CharMap.ContainsKey(current))
            {
                // فحص اتصال الحرف بالذي قبله
                bool connectsPrev = false;
                if (i > 0 && CharMap.ContainsKey(line[i - 1]))
                {
                    connectsPrev = CharMap[line[i - 1]].connectsLeft;
                }

                // فحص اتصال الحرف بالذي بعده
                bool connectsNext = false;
                if (i + 1 < line.Length && CharMap.ContainsKey(line[i + 1]))
                {
                    connectsNext = CharMap[current].connectsLeft;
                }

                ArabicCharInfo info = CharMap[current];
                if (connectsPrev && connectsNext) shaped[i] = info.medialForm;
                else if (connectsPrev && !connectsNext) shaped[i] = info.finalForm;
                else if (!connectsPrev && connectsNext) shaped[i] = info.initialForm;
                else shaped[i] = info.isolated;
            }
            else
            {
                // علامات ترقيم، أرقام، أو حروف غير عربية
                shaped[i] = current;
            }
        }

        // 2. دمج لام ألف (Lam-Alif Ligatures)
        StringBuilder ligatures = new StringBuilder();
        for (int i = 0; i < shaped.Length; i++)
        {
            if (i < shaped.Length - 1 && line[i] == 'ل')
            {
                char nextOrig = line[i + 1];
                bool connectsPrev = (i > 0 && CharMap.ContainsKey(line[i - 1]) && CharMap[line[i - 1]].connectsLeft);

                if (nextOrig == 'ا')
                {
                    ligatures.Append(connectsPrev ? '\uFEFC' : '\uFEFB');
                    i++;
                    continue;
                }
                else if (nextOrig == 'أ')
                {
                    ligatures.Append(connectsPrev ? '\uFEF8' : '\uFEF7');
                    i++;
                    continue;
                }
                else if (nextOrig == 'إ')
                {
                    ligatures.Append(connectsPrev ? '\uFEFA' : '\uFEF9');
                    i++;
                    continue;
                }
                else if (nextOrig == 'آ')
                {
                    ligatures.Append(connectsPrev ? '\uFEF6' : '\uFEF5');
                    i++;
                    continue;
                }
            }
            ligatures.Append(shaped[i]);
        }

        // 3. عكس النص العربي ليعرض من اليمين لليسار في Unity UI مع الحفاظ على ترتيب الأرقام والكلمات اللاتينية
        return ReverseForRTL(ligatures.ToString());
    }

    private static string ReverseForRTL(string text)
    {
        char[] chars = text.ToCharArray();
        Array.Reverse(chars);

        // تصحيح الأقواس المعكوسة
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '(') chars[i] = ')';
            else if (chars[i] == ')') chars[i] = '(';
            else if (chars[i] == '[') chars[i] = ']';
            else if (chars[i] == ']') chars[i] = '[';
            else if (chars[i] == '{') chars[i] = '}';
            else if (chars[i] == '}') chars[i] = '{';
        }

        // تصحيح الأرقام والكلمات الإنجليزية التي عكست داخل الجملة
        StringBuilder finalStr = new StringBuilder();
        int idx = 0;
        while (idx < chars.Length)
        {
            if (IsLatinOrDigit(chars[idx]))
            {
                int start = idx;
                while (idx < chars.Length && IsLatinOrDigit(chars[idx])) idx++;
                char[] sub = new char[idx - start];
                Array.Copy(chars, start, sub, 0, idx - start);
                Array.Reverse(sub);
                finalStr.Append(sub);
            }
            else
            {
                finalStr.Append(chars[idx]);
                idx++;
            }
        }

        return finalStr.ToString();
    }

    private static bool IsLatinOrDigit(char c)
    {
        return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '.' || c == '-' || c == '+' || c == ':' || c == '/';
    }
}
