using UnityEngine;

public class LanguageScene : MonoBehaviour
{
    public GameObject[] En;
    public GameObject[] Ru;
    [SerializeField] private int isTypeLanguage;
    private GameObject languageObject;

    public void isLanguage()
    {
        languageObject = GameObject.FindWithTag("Language");
        if(languageObject != null)
        {
            isTypeLanguage = languageObject.GetComponent<SceneLanguage>().isTypeLanguage;
        }
        else
        {
            isTypeLanguage = 0;
        }

        if (isTypeLanguage == 0) // английский
        {
            foreach (var item in Ru)
            {
                item.SetActive(false);
            }
            foreach (var item in En)
            {
                item.SetActive(true);
            }
        }
        else // русский
        {
            foreach (var item in En)
            {
                item.SetActive(false);
            }
            foreach (var item in Ru)
            {
                item.SetActive(true);
            }
        }

    }
}
