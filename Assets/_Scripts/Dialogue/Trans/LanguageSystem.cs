//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using TMPro;
//using UnityEngine;

//public enum LanguageTypes
//{
//    eng,
//    rus,
//    de,
//    chi,
//}

//public class LanguageSystem
//{
//    private static LanguageSystem instance = null;
//    private Dictionary<string, string> languageMap;
//    private string savePath = Application.persistentDataPath;
//    private static List<LanguageData> fallbackLanguageDataList;

//    private TMP_FontAsset currentFont;

//    private LanguageSystem()
//    {
//        TryCreateLanguageFilesFromScriptableObjects();
//        SetSystemLanguage();
//    }

//    public static (string text, TMP_FontAsset font) GetStaticTextOnNowLang(string text)
//    {
//        if (instance == null)
//            instance = new LanguageSystem();

//        string lower = text.ToLower();
//        string translated = instance.languageMap.ContainsKey(lower)
//            ? instance.languageMap[lower]
//            : instance.languageMap.ContainsValue(text)
//                ? text
//                : "[MISSING]";

//        return (translated, instance.currentFont);
//    }


//    private void SetSystemLanguage()
//    {
//        LanguageTypes language;
//        SystemLanguage systemLang = Application.systemLanguage;

//        switch (systemLang)
//        {
//            case SystemLanguage.English:
//                language = LanguageTypes.eng;
//                break;
//            case SystemLanguage.Russian:
//                language = LanguageTypes.rus;
//                break;
//            case SystemLanguage.German:
//                language = LanguageTypes.de;
//                break;
//            case SystemLanguage.Chinese:
//                language = LanguageTypes.chi;
//                break;
//            default:
//                language = LanguageTypes.eng;
//                break;
//        }

//#if UNITY_EDITOR
//        language = LanguageTypes.chi;
//#endif

//        LanguageData data = fallbackLanguageDataList.Find(d => d.languageType == language);

//        if (data != null)
//        {
//            currentFont = data.font;

//            languageMap = new Dictionary<string, string>();
//            foreach (var pair in data.languagePairs)
//                languageMap[pair.Key.ToLower()] = pair.Value;
//        }
//        else
//        {
//            string localSavePath = Path.Combine(savePath, $"{language}.trans");

//            if (File.Exists(localSavePath))
//            {
//                string json = File.ReadAllText(localSavePath);
//                languageMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
//            }
//            else
//            {
//                Debug.LogError("Translation file not found: " + localSavePath);
//            }

//            currentFont = null;
//        }
//    }

//    private void TryCreateLanguageFilesFromScriptableObjects()
//    {
//        fallbackLanguageDataList = new List<LanguageData>();

//        LanguageData[] loadedData = Resources.LoadAll<LanguageData>("Localization");

//        if (loadedData != null && loadedData.Length > 0)
//            fallbackLanguageDataList.AddRange(loadedData);
//        else
//            Debug.LogWarning("No LanguageData found in Resources/Localization!");

//        foreach (LanguageTypes lang in Enum.GetValues(typeof(LanguageTypes)))
//        {
//            string fileName = $"{lang}.trans";
//            string fullPath = Path.Combine(savePath, fileName);

//            if (!File.Exists(fullPath) || Application.isEditor)
//            {
//                LanguageData data = fallbackLanguageDataList.Find(d => d.languageType == lang);
//                if (data != null)
//                {
//                    Dictionary<string, string> dict = new Dictionary<string, string>();
//                    foreach (var pair in data.languagePairs)
//                    {
//                        dict[pair.Key.ToLower()] = pair.Value;
//                    }
//                    CreateLanguageFile(fileName, dict);
//                    Debug.Log($"Created translation file from ScriptableObject: {fileName} / {fullPath}");
//                }
//                else
//                {
//                    Debug.LogWarning($"Missing ScriptableObject for language: {lang}");
//                }
//            }
//        }
//    }

//    private void CreateLanguageFile(string fileName, Dictionary<string, string> data)
//    {
//        string filePath = Path.Combine(savePath, fileName);
//        if (!File.Exists(filePath) || Application.isEditor)
//        {
//            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
//            File.WriteAllText(filePath, json);
//        }
//    }
//}
