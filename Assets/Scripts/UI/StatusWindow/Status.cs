using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Hypocrites
{
    using Defines;

    public class Status : MonoBehaviour
    {
        public BeingStatusType statusType;

        public Button statusUpButton;
        public Button statusDownButton;
        public TextMeshProUGUI statusValueText;

        public delegate bool OnStatusUpClick();
        public OnStatusUpClick onStatusUpClick;

        public delegate void OnStatusDownClick();
        public OnStatusDownClick onStatusDownClick;

        int originStatusValue;
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
                statusValueText.text = (originStatusValue + value).ToString();
            }
        }

        public void Awake()
        {
            statusUpButton.onClick.AddListener(() =>
            {
                // 사용 가능한 sp를 전부 사용한 경우는 모든 Up 버튼을 비활성화해야 하므로 StatusWindowUI에서 처리함
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

            originStatusValue = int.Parse(statusValueText.text);
        }

        /// <summary>
        /// status의 초기값을 설정한다. status point 가감은 UP, DOWN 버튼으로 이뤄져야 하며, 해당 메소드를 사용하면 안 된다.
        /// </summary>
        /// <param name="value">status 초기값</param>
        public void SetStatus(int value)
        {
            originStatusValue = value;
            statusValueText.text = value.ToString();
        }

        /// <summary>
        /// 할당한 status point를 적용한다
        /// </summary>
        public void AdjustStatusPoint()
        {
            int currentStatusPoint = int.Parse(statusValueText.text);
            
            originStatusValue = currentStatusPoint;
            AddedStatusPoint = 0;

            // Up 버튼은 남은 sp에 따라 StatusWindowUI에서 처리함
            DeactivateDownButton();
        }

        public void CancelStatusPoint()
        {
            SetStatus(originStatusValue);
            AddedStatusPoint = 0;

            DeactivateDownButton();
        }

        #region 버튼 활성화 관련 메소드
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
