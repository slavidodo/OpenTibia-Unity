using OpenTibiaUnity.Core.InputManagment;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public class SplitStackWindow : Base.Window
    {
        public class SplitStackWindowButtonEvent : UnityEvent<SplitStackWindow> { }

        private static RenderTexture s_RenderTexture;

#pragma warning disable CS0649 // never assigned to
        [SerializeField] private Button m_OKButton;
        [SerializeField] private Button m_CancelButton;
        [SerializeField] private SliderWrapper m_SliderWrapper;
        [SerializeField] private RawImage m_ItemImage;
#pragma warning restore CS0649 // never assigned to

        private Appearances.AppearanceType m_ObjectType = null;
        private Appearances.ObjectInstance m_ObjectInstance = null;
        private int m_ObjectAmount = 0;
        private int m_SelectedAmount = 0;

        public SplitStackWindowButtonEvent onOk;

        public Appearances.AppearanceType ObjectType {
            get => m_ObjectType;
            set {
                m_ObjectType = value;
            }
        }

        public int ObjectAmount {
            get => m_ObjectAmount;
            set {
                if (m_ObjectAmount != value) {
                    m_ObjectAmount = value;
                    m_SliderWrapper.SetMinMax(1, m_ObjectAmount);
                }
            }
        }

        public int SelectedAmount {
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

        public void Show() {
            SelectedAmount = 1;

            gameObject.SetActive(true);
            LockToOverlay();
            ResetLocalPosition();
        }

        public void HideWindow() {
            UnlockFromOverlay();
            gameObject.SetActive(false);
        }

        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            if (s_RenderTexture == null) {
                s_RenderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                s_RenderTexture.filterMode = FilterMode.Point;
            } else {
                s_RenderTexture.Release();
            }

            RenderTexture.active = s_RenderTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            if (m_ObjectType != null) {
                if (m_ObjectInstance == null || m_ObjectInstance.ID != m_ObjectType.ID)
                    m_ObjectInstance = OpenTibiaUnity.AppearanceStorage.CreateObjectInstance(m_ObjectType.ID, m_ObjectAmount);

                Vector2 zoom = new Vector2(Screen.width / (float)s_RenderTexture.width, Screen.height / (float)s_RenderTexture.height);
                m_ObjectInstance.DrawTo(new Vector2(0, 0), zoom, 0, 0, 0);
            }

            RenderTexture.active = null;
        }
    }
}
