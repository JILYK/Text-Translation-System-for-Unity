using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextTranslation : MonoBehaviour
{
    [System.Serializable]
    public class TranslatableText
    {
        public Text textElement;     // Сам элемент Text
        public string russianText;   // Русский текст
        public string englishText;   // Английский текст
        public string turkishText;   // Турецкий текст
    }

    public List<TranslatableText> textElements = new List<TranslatableText>(); // Список текстовых элементов с переводами
    private string currentLanguage;

    private void Start()
    {
        DetectAndApplyLanguage();
    }

    private void DetectAndApplyLanguage()
    {
        currentLanguage = PlayerPrefs.GetString("SelectedLanguage", "");

        if (string.IsNullOrEmpty(currentLanguage))
        {
            if (Application.systemLanguage == SystemLanguage.Russian)
                currentLanguage = "ru";
            else if (Application.systemLanguage == SystemLanguage.Turkish)
                currentLanguage = "tr";
            else
                currentLanguage = "en";  // По умолчанию английский
            
            PlayerPrefs.SetString("SelectedLanguage", currentLanguage);
            PlayerPrefs.Save();
        }

        TranslateText(currentLanguage);
    }

    public void TranslateText(string language)
    {
        foreach (var item in textElements)
        {
            if (language == "ru")
            {
                item.textElement.text = item.russianText;
            }
            else if (language == "tr")
            {
                item.textElement.text = item.turkishText;
            }
            else // По умолчанию английский
            {
                item.textElement.text = item.englishText;
            }
        }
        currentLanguage = language;
        PlayerPrefs.SetString("SelectedLanguage", currentLanguage);
        PlayerPrefs.Save();
    }

    // Метод для загрузки переводов из файла, обновляя значения по индексу
    public void LoadTranslationsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Файл не найден: " + filePath);
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(new string[] { "-::(RU)%::-", "-::(EN)%::-", "-::(TR)%::-" }, System.StringSplitOptions.None);
            if (parts.Length >= 4)
            {
                if (int.TryParse(parts[0], out int index) && index < textElements.Count)
                {
                    // Получаем переводы из строки
                    string russianText = parts[1];
                    string englishText = parts[2];
                    string turkishText = parts[3];

                    // Обновляем значения, если они пусты или отличаются
                    if (string.IsNullOrEmpty(textElements[index].russianText) || textElements[index].russianText != russianText)
                        textElements[index].russianText = russianText;

                    if (string.IsNullOrEmpty(textElements[index].englishText) || textElements[index].englishText != englishText)
                        textElements[index].englishText = englishText;

                    if (string.IsNullOrEmpty(textElements[index].turkishText) || textElements[index].turkishText != turkishText)
                        textElements[index].turkishText = turkishText;
                }
            }
        }
        Debug.Log("Переводы загружены из файла: " + filePath);
    }

    // Метод для вывода всех текстовых элементов в консоль с форматом
    public void PrintTranslationsToConsole()
    {
        string allTranslations = "";

        for (int i = 0; i < textElements.Count; i++)
        {
            var item = textElements[i];
            if (item.textElement != null)
            {
                string line = $"{i}-::(RU)%::-{item.russianText}-::(EN)%::-{item.englishText}-::(TR)%::-{item.turkishText}";
                allTranslations += line + "\n";
            }
        }

        // Выводим все переводы одним сообщением и копируем в буфер обмена
        Debug.Log(allTranslations);
        GUIUtility.systemCopyBuffer = allTranslations;
        Debug.Log("Переводы скопированы в буфер обмена.");
    }

    public void AutoFillTextElements()
    {
        Text[] foundTexts = Resources.FindObjectsOfTypeAll<Text>();

        foreach (var text in foundTexts)
        {
            if (text.gameObject.scene.name != null)
            {
                bool elementExists = textElements.Exists(item => item.textElement == text);

                if (!elementExists)
                {
                    TranslatableText translatable = new TranslatableText
                    {
                        textElement = text,
                        russianText = text.text,  // Используем текущий текст как русский текст по умолчанию
                        englishText = "",
                        turkishText = ""
                    };
                    textElements.Add(translatable);
                }
            }
        }
    }

    public void OnLanguageButtonClicked(string language)
    {
        TranslateText(language);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextTranslation))]
public class TextTranslationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TextTranslation script = (TextTranslation)target;

        if (GUILayout.Button("Auto Fill Text Elements"))
        {
            script.AutoFillTextElements();
        }

        if (GUILayout.Button("Print Translations to Console"))
        {
            script.PrintTranslationsToConsole();
        }

        if (GUILayout.Button("Load Translations From File"))
        {
            string filePath = EditorUtility.OpenFilePanel("Load Translations", "", "txt");
            if (!string.IsNullOrEmpty(filePath))
            {
                script.LoadTranslationsFromFile(filePath);
            }
        }
    }
}
#endif
