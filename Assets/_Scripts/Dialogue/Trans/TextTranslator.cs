//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class TextTranslator : MonoBehaviour
//{
//    public void Start()
//    {
//        TMP_Text text = gameObject.GetComponent<TextMeshPro>();
//        if (text == null)
//            text = gameObject.GetComponent<TextMeshProUGUI>();

//        TMP_Dropdown dropdown = gameObject.GetComponent<TMP_Dropdown>();

//        if (dropdown != null)
//        {
//            var (captionText, captionFont) = LanguageSystem.GetStaticTextOnNowLang(dropdown.captionText.text);
//            dropdown.captionText.SetText(captionText);
//            dropdown.captionText.font = captionFont;

//            for (int i = 0; i < dropdown.options.Count; i++)
//            {
//                var (optionText, _) = LanguageSystem.GetStaticTextOnNowLang(dropdown.options[i].text);
//                dropdown.options[i].text = optionText;
//            }

//            dropdown.RefreshShownValue();
//            return;
//        }
//        else if (text != null)
//        {
//            var (localizedText, localizedFont) = LanguageSystem.GetStaticTextOnNowLang(text.text);
//            text.SetText(localizedText);
//            text.font = localizedFont;
//        }
//        else
//        {
//            Debug.LogError("Not A Text");
//        }
//    }
//}
