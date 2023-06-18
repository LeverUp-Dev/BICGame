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

            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            /* 플레이어 상태 변화 콜백 설정 */
            player.onHpChanged += SetHpText;

            ReflectPlayerStatus();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                isActive = !isActive;
                statusWindowPanel.SetActive(isActive);

                CurStatusPoint = maxStatusPoint;

                if (isActive)
                {
                    if (maxStatusPoint > 0)
                        ActivateUpButtons();

                    ReflectPlayerStatus();
                }
                else
                {
                    for (int i = 0; i < statuses.Length; ++i)
                        statuses[i].CancelStatusPoint();
                }
            }
        }

        public void ReflectPlayerStatus()
        {
            /* 플레이어 상태 정보 설정 */
            playerNameText.text = player.Status.Name;

            SetLevelText(player.Status.Level);
            SetExpText(100, player.Status.Exp);
            SetHpText(BeingConstants.MAX_STAT_HEALTH, player.Status.Health);
            SetMpText(BeingConstants.MAX_STAT_MANA, player.Status.Mana);

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

        #region 상태 변화 콜백 메소드
        public void SetLevelText(int level)
        {
            if (isActive)
                levelText.text = "Lv." + level;
        }

        public void SetExpText(int max, int cur)
        {
            if (isActive)
                ExpText.text = "EXP : " + cur + "/" + max;
        }

        public void SetHpText(int max, int cur)
        {
            if (isActive)
                HpText.text = "HP : " + cur + "/" + max;
        }

        public void SetMpText(int max, int cur)
        {
            if (isActive)
                MpText.text = "MP : " + cur + "/" + max;
        }
        #endregion

        #region 능력치 관련 메소드
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
            // 플레이어에 수정된 Status Point 반영
            for (int i = 0; i < statuses.Length; ++i)
            {
                Status status = statuses[i];
                status.AdjustStatusPoint();
                
                switch (status.statusType)
                {
                    case BeingStatusType.STRENGTH:
                        player.Status.Strength = status.OriginStatusValue;
                        break;

                    case BeingStatusType.DEXTERITY:
                        player.Status.Dexterity = status.OriginStatusValue;
                        break;

                    case BeingStatusType.INTELLIGENCE:
                        player.Status.Intelligence = status.OriginStatusValue;
                        break;

                    case BeingStatusType.VITALITY:
                        player.Status.Vitality = status.OriginStatusValue;
                        break;

                    case BeingStatusType.LUCK:
                        player.Status.Luck = status.OriginStatusValue;
                        break;
                }
            }

            // 사용한 Status Point 수 반영
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