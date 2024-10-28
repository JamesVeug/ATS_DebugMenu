using System;
using System.Collections.Generic;
using ATS_API.Helpers;
using Eremite;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Toggle = UnityEngine.UI.Toggle;

public abstract class CanvasWindow : MonoBehaviour
{
    public enum ToggleStates
    {
        Off, Minimal, All
    }
    
    private struct DisabledToggle
    {
        public string Text;
        public RectTransform Container;
        public Func<(bool, string)> IsDisabledCheck;
        public Action<string> SetText;
    }
    
    public abstract string PopupName { get; }
    public abstract Vector2 Size  { get; }
    public virtual bool ClosableWindow => true;
    public bool IsVisible => Root.gameObject.activeInHierarchy;

    public int ColumnWidth = 200;
    public float RowHeight = 50;
    
    public RectTransform Root;
    public RectTransform Content;
    public Canvas Canvas;

    public TMP_Text HeaderLabel;
    public Button MinusButton;
    public Button PlusButton;
    public Button XButton;

    private List<DisabledToggle> disableableToggles = new List<DisabledToggle>();
    
    private ToggleStates currentState = ToggleStates.Off;
    private Stack<RectTransform> Containers = new Stack<RectTransform>();
    private RectTransform CurrentContainer => Containers.Peek();
    private CanvasPrefabData data;

    public void Setup(CanvasPrefabData data)
    {
        this.data = data;
        Root = transform.GetRectTransform();//GameObject.Instantiate(data.WindowPrefab, data.WindowParent, true).GetComponent<RectTransform>();
        Canvas = Root.SafeGetComponent<Canvas>();
        

        Root.gameObject.AddComponent<LogOnUnityEvent>();
        Content = Root.FindChild<ScrollRect>("Scroll View").content;

        RectTransform header = Root.FindChild<RectTransform>("Header");
        HeaderLabel = header.FindChild<TMP_Text>("Text (TMP)");
        HeaderLabel.text = PopupName;

        // foreach (var element in Root.Query(className: "draggable").Build())
        // {
        //     element.AddManipulator(new DragManipulator());
        // }

        MinusButton = header.FindChild<Button>("Button");
        MinusButton.gameObject.SetActive(false);

        PlusButton = header.FindChild<Button>("Button (1)");
        PlusButton.gameObject.SetActive(false);

        XButton = header.FindChild<Button>("Button (2)");

        
        MinusButton.gameObject.SetActive(!ClosableWindow);
        PlusButton.gameObject.SetActive(!ClosableWindow);
        XButton.gameObject.SetActive(true);
        
        if (ClosableWindow)
        {
            XButton.onClick.AddListener(() => Destroy(gameObject));
        }
        else
        {
            MinusButton.onClick.AddListener(() =>
            {
                Debug.Log("Button 1 clicked");
                Root.sizeDelta = new Vector2(100, 30);
            });
        
            PlusButton.onClick.AddListener(() =>
            {
                Debug.Log("Button 2 clicked");
                Root.sizeDelta = new Vector2(240, 420);
            });
            
            XButton.onClick.AddListener(() =>
            {
                Debug.Log("Button 3 clicked");
                Root.sizeDelta = new Vector2(650, 420);
            });
        }

        Containers.Push(Content);
        Containers.Push(StartNewColumn());
        
        SetSize(Size);
    }

    public virtual void CreateGUI()
    {

    }

    private EventSystem eventSystem;
    public virtual void Update()
    {
        // Debug.Log("sortingOrder: " + UIDocument.sortingOrder + " activeInHierarchy: " + UIDocument.gameObject.activeInHierarchy);
        // Debug.Log("Root: " + (Root == null ? "null" : $"display: {Root.style.display}"));
        // Debug.Log("Panel: " + (Panel == null ? "null" : "not null"));
        // Debug.Log("Content: " + (Content == null ? "null" : "not null"));
        // Debug.Log("HeaderLabel: " + (HeaderLabel == null ? "null" : "not null"));
        foreach (var toggle in disableableToggles)
        {
            (bool, string) disabledCheck = toggle.IsDisabledCheck();
            if (disabledCheck.Item1 != toggle.Container.gameObject.activeInHierarchy)
            {
                bool enabled = !disabledCheck.Item1;
                toggle.Container.SetActive(enabled);
                if (toggle.SetText != null)
                {
                    if (!disabledCheck.Item1 && !string.IsNullOrEmpty(disabledCheck.Item2))
                    {
                        toggle.SetText(toggle.Text);
                    }
                    else
                    {
                        toggle.SetText(toggle.Text + "\n" + disabledCheck.Item2);
                    }
                }
            }
        }
    }

    public Button Button(string buttonText, Action onClick, Func<(bool, string)> disabled=null)
    {
        Button button = Instantiate(data.ButtonPrefab, CurrentContainer);
        button.GetComponentInChildren<TMP_Text>().text = buttonText;
        if (disabled != null)
        {
            disableableToggles.Add(new DisabledToggle
            {
                Container = button.GetComponent<RectTransform>(),
                IsDisabledCheck = disabled,
                Text = buttonText,
                SetText = (text) => button.GetComponentInChildren<TMP_Text>().text = text
            });
        }

        return button;
    }

    public TMP_Text Label(string labelText)
    {
        TMP_Text label = Instantiate(data.LabelPrefab, CurrentContainer);
        return label;
    }
    
    public Image Image(Sprite sprite)
    {
        Image image = Instantiate(data.ImagePrefab, CurrentContainer);
        image.sprite = sprite;
        return image;
    }

    public TMP_Text LabelHeader(string labelText)
    {
        TMP_Text label = Instantiate(data.HeaderLabelPrefab, CurrentContainer);
        return label;
    }

    public RectTransform Padding(int padding=10)
    {
        RectTransform visualElement = new GameObject("Padding", typeof(RectTransform)).GetComponent<RectTransform>();
        visualElement.sizeDelta = new Vector2(padding, padding);
        return visualElement;
    }

    public Toggle Toggle(string label, bool value, Action<bool> valueChanged)
    {
        using (HorizontalScope())
        {
            TMP_Text labelElement = Label(label);
            Toggle toggle = Instantiate(data.TogglePrefab, CurrentContainer);
            toggle.SetIsOnWithoutNotify(value);
            toggle.onValueChanged.AddListener(evt => valueChanged(toggle.isOn));
            return toggle;
        }
    }

    public InputField TextField(string s, Action<string> valueChanged)
    {
        InputField textField = GameObject.Instantiate(data.TextFieldPrefab, CurrentContainer);
        textField.SetTextWithoutNotify(s);
        textField.onValueChanged.AddListener(evt => valueChanged(textField.text));
        return textField;
    }
    
    public InputField IntField(int value, Action<int> valueChanged)
    {
        InputField textField = GameObject.Instantiate(data.IntFieldPrefab, CurrentContainer);
        textField.SetTextWithoutNotify(value.ToString());
        textField.onValueChanged.AddListener(evt => valueChanged(int.Parse(textField.text)));
        return textField;
    }
    
    public RectTransform StartNewColumn()
    {
        while (Containers.Count > 1)
        {
            Containers.Pop();
        }

        RectTransform visualElement = GameObject.Instantiate(data.ColumnPrefab, CurrentContainer);
        Containers.Push(visualElement);

        return visualElement;
    }

    public RowScope HorizontalScope()
    {
        return new RowScope(this);
    }

    public ColumnScope VerticalScope()
    {
        return new ColumnScope(this);
    }

    public struct RowScope : IDisposable
    {
        public RectTransform container;
        private CanvasWindow window;

        public RowScope(CanvasWindow window)
        {
            this.window = window;
            RectTransform row = Instantiate(window.data.RowPrefab, window.CurrentContainer);
            window.Containers.Push(row);
            container = row;
        }

        public void Dispose()
        {
            window.Containers.Pop();
        }
    }

    public struct ColumnScope : IDisposable
    {
        public RectTransform container;
        private CanvasWindow window;

        public ColumnScope(CanvasWindow window)
        {
            this.window = window;
            RectTransform column = Instantiate(window.data.ColumnPrefab, window.CurrentContainer);
            window.Containers.Push(column);
            container = column;
        }

        public void Dispose()
        {
            window.Containers.Pop();
        }
    }
    
    public void ToggleVisible(bool visible)
    {
        Root.gameObject.SetActive(visible);
    }
    
    public void SetSize(Vector2 size)
    {
        Root.sizeDelta = size;
    }

    public void SetVisible(bool visible)
    {
        Root.gameObject.SetActive(visible);
    }
}