using UnityEngine;

public class LanguageButtons : MonoBehaviour
{
    public GameObject[] En;
    public GameObject En_Image;
    public GameObject[] Ru;
    public GameObject Ru_Image;
    private int isTypeLanguage;
    private GameObject languageObject;

    [SerializeField] private LanguageScene[] Start_scene;

    private void Start()
    {
        Language_UI();
    }

    public void Language_UI()
    {
        languageObject = GameObject.FindWithTag("Language");
        if (languageObject != null)
        {
            isTypeLanguage = languageObject.GetComponent<SceneLanguage>().isTypeLanguage;

            if (isTypeLanguage == 0)
            {
                foreach (var item in Ru)
                {
                    item.SetActive(false);
                }
                foreach (var item in En)
                {
                    item.SetActive(true);
                }
                En_Image.SetActive(true);
                Ru_Image.SetActive(false);
            }
            else
            {
                foreach (var item in En)
                {
                    item.SetActive(false);
                }
                foreach (var item in Ru)
                {
                    item.SetActive(true);
                }
                En_Image.SetActive(false);
                Ru_Image.SetActive(true);
            }
        }
        else
        {
            isTypeLanguage = 0;
        }
    }

    public void English_language() // нажали на кнопку английский
    {
        foreach (var item in Ru)
        {
            item.SetActive(false);
        }
        foreach (var item in En)
        {
            item.SetActive(true);
        }
        isTypeLanguage = 0;
        languageObject.GetComponent<SceneLanguage>().isTypeLanguage = isTypeLanguage;
        En_Image.SetActive(true);
        Ru_Image.SetActive(false);


        foreach (var item in Start_scene)
        {
            item.isLanguage();// вызываем функцию обновления языка на текущей сцене
        }
    }

    public void Russian_language() // нажали на кнопку русский
    {
        foreach (var item in En)
        {
            item.SetActive(false);
        }
        foreach (var item in Ru)
        {
            item.SetActive(true);
        }
        isTypeLanguage = 1;
        languageObject.GetComponent<SceneLanguage>().isTypeLanguage = isTypeLanguage;
        En_Image.SetActive(false);
        Ru_Image.SetActive(true);

        foreach (var item in Start_scene)
        {
            item.isLanguage();// вызываем функцию обновления языка на текущей сцене
        }
    }
}
