using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIRatioScaler : MonoBehaviour {

    public RectTransform elmentRectTransform;
    public Text textField;
    
    public float RelativeSizeX;
    public float RelativeSizeY;
    public float RelativePosX;
    public float RelativePosY;
    public float RelativeFontScale;
    public int curFontSize;
    public float curFontScale;

    void Awake()
    {
        elmentRectTransform = GetComponent<RectTransform>();
        
        /*try
        {
            textField = GetComponent<Text>();
        }
        catch (MissingComponentException)
        {
            Debug.Log("Text element doesn't exist in root");
            try
            {
                textField = GetComponentInChildren<Text>();
            }
            catch (MissingComponentException)
            {
                Debug.Log("Text element doesn't exist in children");
            }
        }*/

        textField = GetComponent<Text>();
        
        if (textField == null)
        {
            textField = GetComponentInChildren<Text>();
        }

        //RelativeSizeX = Mathf.Abs(elmentRectTransform.rect.width / Screen.width);
        //RelativeSizeY = Mathf.Abs(elmentRectTransform.rect.height / Screen.height);
        //RelativePosX = Mathf.Abs(elmentRectTransform.rect.position.x / Screen.width);
        //RelativePosY = Mathf.Abs(elmentRectTransform.rect.position.y / Screen.height);
        RelativeSizeX = elmentRectTransform.sizeDelta.x / Screen.width;
        RelativeSizeY = elmentRectTransform.sizeDelta.y / Screen.height;
        RelativePosX = elmentRectTransform.localPosition.x / Screen.width;
        RelativePosY = elmentRectTransform.localPosition.y / Screen.height;
        
        if (textField != null)
        {
            curFontSize = textField.fontSize;
            RelativeFontScale = (float)curFontSize / Screen.height;
        }
    }


    void OnGUI(){
        elmentRectTransform.sizeDelta = new Vector2(Screen.width * RelativeSizeX, Screen.height * RelativeSizeY);
        elmentRectTransform.localPosition = new Vector3(Screen.width * RelativePosX ,Screen.height * RelativePosY, 0);
        
        if (textField != null)
        {
            //curFontSize = curFontSize *
            textField.fontSize = (int)Mathf.Round(Screen.height * RelativeFontScale);
        }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
