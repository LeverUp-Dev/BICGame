using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hypocrites.UI.StatusWindow
{
    using Defines;
    using DB.Data;
    using Player;
    using Hypocrites.DB.Save;

    public class StatusWindowUI : MonoBehaviour
    {
        public GameObject statusWindowPanel;

        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI ExpText;
        public TextMeshProUGUI HpText;
        public TextMeshProUGUI MpText;

        public StatusUI[] statuses;
        public Button applyButton;
        public TextMeshProUGUI statusPointText;

        public int statusPerLevel;

        Party party;
        Member currentMember;

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

            party = GameObject.FindGameObjectWithTag("Player").GetComponent<Party>();

            LoadStatus();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ToggleWindow();
            }
        }

        public void ToggleWindow(string name = null)
        {
            isActive = !isActive;
            statusWindowPanel.SetActive(isActive);

            CurStatusPoint = maxStatusPoint;

            if (isActive)
            {
                if (maxStatusPoint > 0)
                    ActivateUpButtons();

                LoadStatus(name);
            }
            else
            {
                for (int i = 0; i < statuses.Length; ++i)
                    statuses[i].CancelStatusPoint();
            }
        }

        public void LoadStatus(string name = null)
        {
            if (currentMember != null)
            {
                currentMember.onHpChanged -= SetHpText;
            }

            currentMember = party.GetMember(name);

            if (currentMember == null)
            {
                throw new System.Exception($"{name}은 존재하지 않는 동료입니다.");
            }

            /* 플레이어 상태 정보 설정 */
            playerNameText.text = currentMember.Name;

            SetLevelText(-999);
            SetExpText(100, -999);
            SetHpText(BeingConstants.MAX_STAT_HEALTH, (int)currentMember.Status.Health);
            SetMpText(BeingConstants.MAX_STAT_MANA, (int)currentMember.Status.Mana);

            for (int i = 0; i < statuses.Length; ++i)
            {
                StatusUI status = statuses[i];
                switch (status.statusType)
                {
                    case BeingStatusType.STRENGTH:
                        status.SetStatus((int)currentMember.Status.Strength);
                        break;

                    case BeingStatusType.DEXTERITY:
                        status.SetStatus((int)currentMember.Status.Dexterity);
                        break;

                    case BeingStatusType.INTELLIGENCE:
                        status.SetStatus((int)currentMember.Status.Intelligence);
                        break;

                    case BeingStatusType.FAITH:
                        status.SetStatus((int)currentMember.Status.Faith);
                        break;
                }
            }

            /* 플레이어 상태 변화 콜백 설정 */
            currentMember.onHpChanged += SetHpText;
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
            /* TODO : 제거 대상 */

            // 플레이어 실제 레벨 업 반영 필요
            int level = int.Parse(levelText.text.Split('.')[1]) + 1;
            //currentMember.Level = level;
            SetLevelText(level);

            maxStatusPoint += statusPerLevel;
            CurStatusPoint = maxStatusPoint;

            ActivateUpButtons();
        }

        public void AdjustStatusPoint()
        {
            StatusSave add = new StatusSave();

            // 플레이어에 수정된 Status Point 반영
            for (int i = 0; i < statuses.Length; ++i)
            {
                StatusUI statusUI = statuses[i];
                statusUI.AdjustStatusPoint();
                
                switch (statusUI.statusType)
                {
                    case BeingStatusType.STRENGTH:
                        add.strength = statusUI.OriginStatusValue;
                        break;

                    case BeingStatusType.DEXTERITY:
                        add.dexterity = statusUI.OriginStatusValue;
                        break;

                    case BeingStatusType.INTELLIGENCE:
                        add.intelligence = statusUI.OriginStatusValue;
                        break;

                    case BeingStatusType.FAITH:
                        add.faith = statusUI.OriginStatusValue;
                        break;
                }
            }

            Status status = new Status();
            status.Load(add);

            currentMember.Status.Add(status);

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