using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.StatusWindow
{
    using Defines;
    using Player;

    public class StatusWindowUI : MonoBehaviour
    {
        public GameObject statusWindowPanel;

        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI ExpText;
        public TextMeshProUGUI HpText;
        public TextMeshProUGUI MpText;

        public Status[] statuses;
        public Button applyButton;
        public TextMeshProUGUI statusPointText;

        public int statusPerLevel;

        Player player;

        bool isActive = false;

        int maxStatusPoint = 0;
        int curStatusPoint;
        int CurStatusPoint
        {
            get
            {
                return curStatusPoint;
            }

            set
            {
                curStatusPoint = value;
                statusPointText.text = value.ToString();
            }
        }

        private void Awake()
        {
            for (int i = 0; i < statuses.Length; i++)
            {
                statuses[i].onStatusUpClick += OnUpStatusPoint;
                statuses[i].onStatusDownClick += OnDownStatusPoint;
            }

            applyButton.interactable = false;
            applyButton.onClick.AddListener(AdjustStatusPoint);

            // 플레이어 상태 정보 설정
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            playerNameText.text = player.Status.Name;

            SetLevelText(player.Status.Level);
            SetExpText(100, player.Status.Exp);
            SetHpText(BeingConstants.MAX_STAT_HEALTH, player.Status.Health);
            SetHpText(BeingConstants.MAX_STAT_MANA, player.Status.Mana);

            for (int i = 0; i < statuses.Length; ++i)
            {
                Status status = statuses[i];
                switch (status.statusType)
                {
                    case BeingStatusType.STRENGTH:
                        status.SetStatus(player.Status.Strength);
                        break;

                    case BeingStatusType.DEXTERITY:
                        status.SetStatus(player.Status.Dexterity);
                        break;

                    case BeingStatusType.INTELLIGENCE:
                        status.SetStatus(player.Status.Intelligence);
                        break;

                    case BeingStatusType.VITALITY:
                        status.SetStatus(player.Status.Vitality);
                        break;

                    case BeingStatusType.LUCK:
                        status.SetStatus(player.Status.Luck);
                        break;
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                isActive = !isActive;
                statusWindowPanel.SetActive(isActive);

                CurStatusPoint = maxStatusPoint;

                if (!isActive)
                {
                    for (int i = 0; i < statuses.Length; ++i)
                        statuses[i].CancelStatusPoint();
                }
                else if (maxStatusPoint > 0)
                {
                    ActivateUpButtons();
                }
            }
        }

        #region 능력치 관련 메소드
        public void SetLevelText(int level)
        {
            levelText.text = "Lv." + level;
        }

        public void SetExpText(int max, int cur)
        {
            ExpText.text = "EXP : " + cur + "/" + max;
        }

        public void SetHpText(int max, int cur)
        {
            HpText.text = "HP : " + cur + "/" + max;
        }

        public void SetMpText(int max, int cur)
        {
            MpText.text = "MP : " + cur + "/" + max;
        }

        public void AdjustLevelUp()
        {
            // 플레이어 실제 레벨 업 반영 필요
            SetLevelText(int.Parse(levelText.text.Split('.')[1]) + 1);

            maxStatusPoint += statusPerLevel;
            CurStatusPoint = maxStatusPoint;

            ActivateUpButtons();
        }

        public void AdjustStatusPoint()
        {
            for (int i = 0; i < statuses.Length; ++i)
                statuses[i].AdjustStatusPoint();

            maxStatusPoint = CurStatusPoint;
        }
        #endregion

        #region 능력치 가감 버튼 활성화 관련 메소드
        public void ActivateUpButtons()
        {
            for (int i = 0; i < statuses.Length; ++i)
                statuses[i].ActivateUpButton();
        }

        public void DeactivateUpButtons()
        {
            for (int i = 0; i < statuses.Length; ++i)
                statuses[i].DeactivateUpButton();
        }
        #endregion

        #region 능력치 가감 버튼 콜백 메소드
        public bool OnUpStatusPoint()
        {
            applyButton.interactable = true;

            if (--CurStatusPoint == 0)
            {
                DeactivateUpButtons();
                return false;
            }

            return true;
        }

        public void OnDownStatusPoint()
        {
            ActivateUpButtons();

            if (++CurStatusPoint == maxStatusPoint)
                applyButton.interactable = false;
        }
        #endregion
    }
}