using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UIElements.Image;

public class BricksSelectPanel : MonoBehaviour
{
    public BricksPanelButton original;
    public Transform buttonsParent;

    BricksPanelButton[] buttons;
    bool inited = false;
    

    
    void Start()
    {
        buttons = new BricksPanelButton[DataController.Instance.bricks.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = Instantiate(original, buttonsParent);
            BrickItem brickData = DataController.Instance.GetBrickData(i);
            //buttons[i].buttonText.text = brickData.brickPrefab.size.z + "x" + brickData.brickPrefab.size.x;
            string picPath;

            if (i == 9 || i == 10)
            {
                picPath = "cubenaklon" + brickData.brickPrefab.size.z + "x" + brickData.brickPrefab.size.x;
            }
            else
            {
                picPath = "cube" + brickData.brickPrefab.size.z + "x" + brickData.brickPrefab.size.x;
            }
            var sprite = Resources.Load<Sprite>(picPath);
            buttons[i].buttonImage.sprite = sprite;
            SetButton(i);
        }

        Destroy(original.gameObject);

        inited = true;
    }

    void OnEnable()
    {
        if (inited)
        {
            SetButtons();
        }

        GameController.Instance.AllowCast(false);
    }

    void SetButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            SetButton(i);
        }
    }

    void SetButton(int index)
    {
        BrickItem brickData = DataController.Instance.GetBrickData(index);
        BricksPanelButton brickButton = buttons[index];

        brickButton.button.onClick.RemoveAllListeners();
        if (index == DataController.Instance.GetSelectedBrickIndex())
        {
            brickButton.button.interactable = false;
        }
        else
        {
            brickButton.button.onClick.AddListener(() => Select(index));
            brickButton.button.interactable = true;
        }
    }

    void Select(int index)
    {
        DataController.Instance.SelectBrick(index);
        SetButtons();
    }
    
    void OnDisable(){
   GameController.Instance.AllowCast(true);
}
}
