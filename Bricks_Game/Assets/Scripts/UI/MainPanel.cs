using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct GameModeButton
{
    public GameMode mode;
    public Button button;
}

public class MainPanel : MonoBehaviour
{
    public Text rotationButtonText;
    public Image rotationButtonImage;
    public GameModeButton[] modeButtons;

    void Start()
    {
        for (int i = 0; i < modeButtons.Length; i++)
        {
            int index = i;
            modeButtons[i].button.onClick.AddListener(() => ClickButton(index));
            
        }

        SetButtons(GameController.Instance.mode);

        SetRotationButtonText();
    }

    void SetButtons(GameMode mode)
    {
        for (int i = 0; i < modeButtons.Length; i++)
        {
            modeButtons[i].button.interactable = modeButtons[i].mode != mode;
        }
    }

    void ClickButton(int index)
    {
        GameModeButton modeButton = modeButtons[index];
        GameController.Instance.mode = modeButton.mode;
        SetButtons(modeButton.mode);
    }

    public void ChangeRotation()
    {
        GameController.Instance.NextRotation();
        SetRotationButtonText();
    }

    void SetRotationButtonText()
    {
        rotationButtonText.text = "" + GameController.Instance.GetRotationAngle();
        string rotPicPath = "rotImage" + GameController.Instance.GetRotationAngle();
        var sprite = Resources.Load<Sprite>(rotPicPath);
        rotationButtonImage.sprite = sprite;
    }
}
