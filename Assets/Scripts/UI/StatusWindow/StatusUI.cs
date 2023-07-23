using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Hypocrites
{
    using Defines;

    public class StatusUI : MonoBehaviour
    {
        public BeingStatusType statusType;

        public Button statusUpButton;
        public Button statusDownButton;
        public TextMeshProUGUI statusValueText;

        public delegate bool OnStatusUpClick();
        public OnStatusUpClick onStatusUpClick;

        public delegate void OnStatusDownClick();
        public OnStatusDownClick onStatusDownClick;

        public int OriginStatusValue { get; private set; }
        int addedStatusPoint;
        int AddedStatusPoint
        {
            get
            {
                return addedStatusPoint;
            }

            set
            {
                addedStatusPoint = value;
                statusValueText.text = (OriginStatusValue + value).ToString();
            }
        }

        public void Awake()
        {
            statusUpButton.onClick.AddListener(() =>
            {
                // ��� ������ sp�� ���� ����� ���� ��� Up ��ư�� ��Ȱ��ȭ�ؾ� �ϹǷ� StatusWindowUI���� ó����
                if (onStatusUpClick())
                    ActivateAllButtons();
                else
                    ActivateDownButton();

                ++AddedStatusPoint;
            });

            statusDownButton.onClick.AddListener(() =>
            {
                onStatusDownClick();
                    
                ActivateAllButtons();

                if (--AddedStatusPoint == 0)
                    DeactivateDownButton();
            });

            OriginStatusValue = int.Parse(statusValueText.text);
        }

        /// <summary>
        /// status�� �ʱⰪ�� �����Ѵ�. status point ������ UP, DOWN ��ư���� �̷����� �ϸ�, �ش� �޼ҵ带 ����ϸ� �� �ȴ�.
        /// </summary>
        /// <param name="value">status �ʱⰪ</param>
        public void SetStatus(int value)
        {
            OriginStatusValue = value;
            statusValueText.text = value.ToString();
        }

        /// <summary>
        /// �Ҵ��� status point�� �����Ѵ�
        /// </summary>
        public void AdjustStatusPoint()
        {
            int currentStatusPoint = int.Parse(statusValueText.text);
            
            OriginStatusValue = currentStatusPoint;
            AddedStatusPoint = 0;

            // Up ��ư�� ���� sp�� ���� StatusWindowUI���� ó����
            DeactivateDownButton();
        }

        public void CancelStatusPoint()
        {
            SetStatus(OriginStatusValue);
            AddedStatusPoint = 0;

            DeactivateDownButton();
        }

        #region ��ư Ȱ��ȭ ���� �޼ҵ�
        public void ActivateAllButtons()
        {
            ActivateUpButton();
            ActivateDownButton();
        }

        public void ActivateUpButton()
        {
            SetButtonInteractable(statusUpButton, true);
        }

        public void ActivateDownButton()
        {
            SetButtonInteractable(statusDownButton, true);
        }

        public void DeactivateAllButtons()
        {
            DeactivateUpButton();
            DeactivateDownButton();
        }

        public void DeactivateUpButton()
        {
            SetButtonInteractable(statusUpButton, false);
        }

        public void DeactivateDownButton()
        {
            SetButtonInteractable(statusDownButton, false);
        }

        void SetButtonInteractable(Button button, bool interactable)
        {
            button.interactable = interactable;
        }
        #endregion
    }
}