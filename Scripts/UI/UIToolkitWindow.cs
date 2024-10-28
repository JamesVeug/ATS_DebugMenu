using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public abstract class UIToolkitWindow : MonoBehaviour
{
    public enum ToggleStates
    {
        Off, Minimal, All
    }
    
    private struct DisabledToggle
    {
        public string Text;
        public VisualElement VisualElement;
        public Func<(bool, string)> IsDisabledCheck;
        public Action<string> SetText;
    }
    
    public abstract string PopupName { get; }
    public abstract Vector2 Size  { get; }
    public virtual bool ClosableWindow => true;
    public bool IsVisible => Root.style.display == DisplayStyle.Flex;

    public int ColumnWidth = 200;
    public float RowHeight = 50;
    
    public VisualElement Root;
    public VisualElement Panel;
    public VisualElement Content;
    public UIDocument UIDocument;

    public Label HeaderLabel;
    public Button MinusButton;
    public Button PlusButton;
    public Button XButton;

    private List<DisabledToggle> disableableToggles = new List<DisabledToggle>();
    
    private ToggleStates currentState = ToggleStates.Off;
    private Stack<VisualElement> Containers = new Stack<VisualElement>();
    private VisualElement CurrentContainer => Containers.Peek();

    public void Setup(PanelSettings panelSettings, VisualTreeAsset VisualTreeAsset)
    {
        UIDocument = gameObject.AddComponent<UIDocument>();
        UIDocument.panelSettings = panelSettings;
        UIDocument.visualTreeAsset = VisualTreeAsset;
        UIDocument.sortingOrder = 9999;
        UIDocument.gameObject.AddComponent<LogOnUnityEvent>();
        Root = UIDocument.rootVisualElement;
        Panel = Root.Q("Panel");
        Content = Root.Q<ScrollView>("ContentScrollView").contentContainer;
        
        HeaderLabel = Root.Q<Label>("HeaderLabel");
        HeaderLabel.text = PopupName;

        foreach (var element in Root.Query(className: "draggable").Build())
        {
            element.AddManipulator(new DragManipulator());
        }

        MinusButton = Root.Q<Button>("ButtonOne");
        MinusButton.style.display = DisplayStyle.None;

        PlusButton = Root.Q<Button>("ButtonTwo");
        PlusButton.style.display = DisplayStyle.None;

        XButton = Root.Q<Button>("ButtonThree");

        
        MinusButton.style.display = ClosableWindow ? DisplayStyle.None : DisplayStyle.Flex;
        PlusButton.style.display = ClosableWindow ? DisplayStyle.None : DisplayStyle.Flex;
        XButton.style.display = DisplayStyle.Flex;
        
        if (ClosableWindow)
        {
            XButton.clicked += () => Destroy(gameObject);
        }
        else
        {
            MinusButton.clicked += () =>
            {
                Debug.Log("Button 1 clicked");
                Panel.style.width = 100;
                Panel.style.height = 30;
            };
        
            PlusButton.clicked += () =>
            {
                Debug.Log("Button 2 clicked");
                Panel.style.width = 240;
                Panel.style.height = 420;
            };
            
            XButton.clicked += () =>
            {
                Debug.Log("Button 3 clicked");
                Panel.style.width = 650;
                Panel.style.height = 420;
            };
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
        if (eventSystem == null)
        {
            Debug.Log("No EventSystem found, creating one");
            if (EventSystem.current != null)
            {
                Debug.Log("Found EventSystem");
                eventSystem = EventSystem.current;
            }
            else
            {
                Debug.Log("No EventSystem found, creating one");
                eventSystem = gameObject.AddComponent<EventSystem>();
                gameObject.AddComponent<StandaloneInputModule>();
            }
        }

        if (!eventSystem.enabled)
        {
            eventSystem.enabled = true;
        }

        if (!eventSystem.gameObject.TryGetComponent(out StandaloneInputModule module))
        {
            module = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        if (!module.enabled)
        {
            module.enabled = true;
        }
        
        // Debug.Log("sortingOrder: " + UIDocument.sortingOrder + " activeInHierarchy: " + UIDocument.gameObject.activeInHierarchy);
        // Debug.Log("Root: " + (Root == null ? "null" : $"display: {Root.style.display}"));
        // Debug.Log("Panel: " + (Panel == null ? "null" : "not null"));
        // Debug.Log("Content: " + (Content == null ? "null" : "not null"));
        // Debug.Log("HeaderLabel: " + (HeaderLabel == null ? "null" : "not null"));
        foreach (var toggle in disableableToggles)
        {
            (bool, string) disabledCheck = toggle.IsDisabledCheck();
            if (disabledCheck.Item1 != toggle.VisualElement.enabledSelf)
            {
                bool enabled = !disabledCheck.Item1;
                toggle.VisualElement.SetEnabled(enabled);
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
        Button button = new Button(onClick) { text = buttonText };
        CurrentContainer.Add(button);

        if (disabled != null)
        {
            disableableToggles.Add(new DisabledToggle
            {
                VisualElement = button,
                IsDisabledCheck = disabled,
                Text = buttonText,
                SetText = (text) => button.text = text
            });
        }

        return button;
    }

    public Label Label(string labelText)
    {
        Label label = new Label(labelText);
        CurrentContainer.Add(label);
        return label;
    }
    
    public VisualElement Image(Sprite sprite)
    {
        VisualElement visualElement = new VisualElement();
        visualElement.style.backgroundImage = sprite.texture;
        CurrentContainer.Add(visualElement);
        return visualElement;
    }

    public Label LabelHeader(string labelText)
    {
        Label label = new Label(labelText);
        label.AddToClassList("label-header");
        CurrentContainer.Add(label);
        return label;
    }

    public VisualElement Padding(int padding=10)
    {
        VisualElement visualElement = new VisualElement { style = { height = padding } };
        CurrentContainer.Add(visualElement);
        return visualElement;
    }

    public Toggle Toggle(string label, bool value, Action<bool> valueChanged)
    {
        using (HorizontalScope())
        {
            Label labelElement = new Label(label);
            Toggle toggle = new Toggle();
            toggle.value = value;
            toggle.RegisterValueChangedCallback(evt => valueChanged(evt.newValue));
            CurrentContainer.Add(labelElement);
            CurrentContainer.Add(toggle);
            return toggle;
        }
    }

    public TextField TextField(string s, Action<string> valueChanged)
    {
        TextField textField = new TextField();
        textField.value = s;
        textField.RegisterValueChangedCallback(evt => valueChanged(evt.newValue));
        CurrentContainer.Add(textField);
        return textField;
    }
    
    public TextField IntField(int value, Action<int> valueChanged)
    {
        TextField textField = new TextField();
        textField.value = value.ToString();
        textField.RegisterValueChangedCallback(evt =>
        {
            string evtNewValue = evt.newValue;
            if (int.TryParse(evtNewValue, out int result))
            {
                valueChanged(result);
            }
            else
            {
                // Remove the last character if it's not a number
                while(evtNewValue.Length > 0 && !int.TryParse(evtNewValue, out _))
                {
                    evtNewValue = evtNewValue.Substring(0, evtNewValue.Length - 1);
                }
                if(evtNewValue.Length == 0)
                {
                    evtNewValue = "0";
                }
                textField.value = evtNewValue;
                valueChanged(int.Parse(evtNewValue));
            }
        });
        CurrentContainer.Add(textField);
        return textField;
    }
    
    public VisualElement StartNewColumn()
    {
        while (Containers.Count > 1)
        {
            Containers.Pop();
        }

        VisualElement visualElement = new VisualElement();
        visualElement.AddToClassList("column");
        CurrentContainer.Add(visualElement);
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
        public VisualElement container;
        private UIToolkitWindow window;

        public RowScope(UIToolkitWindow window)
        {
            this.window = window;
            VisualElement visualElement = new VisualElement();
            visualElement.AddToClassList("row");
            window.CurrentContainer.Add(visualElement);
            window.Containers.Push(visualElement);
            container = visualElement;
        }

        public void Dispose()
        {
            window.Containers.Pop();
        }
    }

    public struct ColumnScope : IDisposable
    {
        public VisualElement container;
        private UIToolkitWindow window;

        public ColumnScope(UIToolkitWindow window)
        {
            this.window = window;
            VisualElement visualElement = new VisualElement();
            visualElement.AddToClassList("column");
            window.CurrentContainer.Add(visualElement);
            window.Containers.Push(visualElement);
            container = visualElement;
        }

        public void Dispose()
        {
            window.Containers.Pop();
        }
    }
    
    public void ToggleVisible(bool visible)
    {
        Root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    public void SetSize(Vector2 size)
    {
        Root.style.width = size.x;
        Root.style.height = size.y;
    }

    public void SetVisible(bool visible)
    {
        Root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}