using OpenTibiaUnity.Core.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    internal class SplitStackWindow : Base.Window
    {
        internal class SplitStackWindowButtonEvent : UnityEvent<SplitStackWindow> { }
        
        [SerializeField] private Button m_OKButton = null;
        [SerializeField] private Button m_CancelButton = null;
        [SerializeField] private SliderWrapper m_SliderWrapper = null;
        [SerializeField] private RawImage m_ItemImage = null;

        private Appearances.AppearanceType m_ObjectType = null;
        private Appearances.ObjectInstance m_ObjectInstance = null;
        private int m_ObjectAmount = 0;
        private int m_SelectedAmount = 0;
        
        private RenderTexture m_RenderTexture = null;

        internal SplitStackWindowButtonEvent onOk;

        internal Appearances.AppearanceType ObjectType {
            get => m_ObjectType;
            set {
                m_ObjectType = value;
            }
        }

        internal int ObjectAmount {
            get => m_ObjectAmount;
            set {
                if (m_ObjectAmount != value) {
                    m_ObjectAmount = value;
                    m_SliderWrapper.SetMinMax(1, m_ObjectAmount);
                }
            }
        }

        internal int SelectedAmount {
            get => m_SelectedAmount;
            set {
                if (m_SelectedAmount != value) {
                    m_SelectedAmount = value;
                    m_SliderWrapper.slider.value = value;
                }
            }
        }

        protected override void Start() {
            base.Start();
            m_OKButton.onClick.AddListener(TriggerOk);
            m_CancelButton.onClick.AddListener(TriggerCancel);
            m_SliderWrapper.slider.onValueChanged.AddListener(TriggerSliderChange);

            OpenTibiaUnity.InputHandler.AddKeyUpListener(Utility.EventImplPriority.Default, (Event e, bool repeat) => {
                if (!InputHandler.IsHighlighted(this))
                    return;

                switch (e.keyCode) {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        TriggerOk();
                        break;

                    case KeyCode.Escape:
                        TriggerCancel();
                        break;

                    case KeyCode.LeftArrow:
                        SelectedAmount = Mathf.Max(0, SelectedAmount - (e.shift ? 10 : 1));
                        break;

                    case KeyCode.RightArrow:
                        SelectedAmount = Mathf.Min(m_ObjectAmount, SelectedAmount + (e.shift ? 10 : 1));
                        break;
                }
            });

            onOk = new SplitStackWindowButtonEvent();
        }

        protected void TriggerOk() {
            onOk.Invoke(this);
            HideWindow();
        }

        protected void TriggerCancel() {
            HideWindow();
        }

        protected void TriggerSliderChange(float newValue) {
            m_SelectedAmount = (int)newValue;
        }

        internal override void ShowWindow() {
            base.ShowWindow();

            SelectedAmount = 1;
        }
        
        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            if (m_RenderTexture == null) {
                m_RenderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                m_RenderTexture.filterMode = FilterMode.Point;
                m_ItemImage.texture = m_RenderTexture;
            } else {
                m_RenderTexture.Release();
            }

            RenderTexture.active = m_RenderTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            if (!!m_ObjectType) {
                if (m_ObjectInstance == null || m_ObjectInstance.ID != m_ObjectType.ID)
                    m_ObjectInstance = OpenTibiaUnity.AppearanceStorage.CreateObjectInstance(m_ObjectType.ID, m_ObjectAmount);

                var zoom = new Vector2(Screen.width / (float)m_RenderTexture.width, Screen.height / (float)m_RenderTexture.height);
                m_ObjectInstance.DrawTo(new Vector2(0, 0), zoom, 0, 0, 0);
            }

            RenderTexture.active = null;
        }
    }
}
