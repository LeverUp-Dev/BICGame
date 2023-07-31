using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Event {
    using DS;
    using DS.ScriptableObjects;
    using DB.Data;
    using Defines;
    using DB;
    using Player;

    [RequireComponent(typeof(DSDialogue))]

    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        /* 다이얼로그 데이터 관련 변수 */
        DSDialogue dialogueComponent;
        DSDialogueContainerSO dialogueContainer;

        Dictionary<string, List<DSDialogueSO>> dialogues;

        /* 이벤트 관련 변수 */
        private int encounterCount;
        public bool IsEncounter
        {
            get
            {
                if (Random.Range(0, 100) < EnemyEncounterChance)
                    --encounterCount;

                if (encounterCount == 0)
                {
                    encounterCount = EnemyEncounterTime;
                    return true;
                }

                return false;
            }
        }

        Party party;

        /* Inspector Values */
        [field: SerializeField] public bool Run { get; private set; }

        [field: SerializeField] public int PeaceChance { get; private set; }
        [field: SerializeField] public int PositiveEventChance { get; private set; }
        [field: SerializeField] public int NegativeEventChance { get; private set; }
        [field: SerializeField] public int MemberEventChance { get; private set; }

        [field: SerializeField] public int EnemyEncounterTime { get; private set; }
        [field: SerializeField] public int EnemyEncounterChance { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            // 씬 전환 시에 싱글톤 객체가 파괴되지 않도록 유지
            DontDestroyOnLoad(gameObject);

            // 플레이어 정보 검색
            party = GameObject.FindGameObjectWithTag("Player").GetComponent<Party>();

            /* 다이얼로그 정보 검색 */
            dialogueComponent = GetComponent<DSDialogue>();
            dialogueContainer = dialogueComponent.GetDialogueContainer();
            
            dialogues = new Dictionary<string, List<DSDialogueSO>>();

            foreach (DSDialogueGroupSO group in dialogueContainer.DialogueGroups.Keys)
            {
                List<DSDialogueSO> list = new List<DSDialogueSO>();
                foreach (DSDialogueSO dialogue in dialogueContainer.DialogueGroups[group])
                {
                    // 시작 다이얼로그만 저장
                    if (!dialogue.IsStartingDialogue)
                        continue;

                    // 이미 영입된 동료는 더 이상 영입 이벤트가 발생하면 안 되므로 처리
                    if (group.GroupName.Equals(EventDefines.EVENT_MEMBER_GROUP_NAME) && party.GetMember(dialogue.DialogueName) != null)
                        continue;
                    
                    list.Add(dialogue);
                }

                if (list.Count == 0)
                    continue;

                dialogues[group.GroupName] = list;
            }

            // 적 조우까지 남은 횟수 초기화
            encounterCount = EnemyEncounterTime;
        }

        #region 랜덤 이벤트 관련 메소드
        /// <summary>
        /// 일정 확률로 랜덤 이벤트를 생성한다
        /// </summary>
        /// <param name="luck">부정적인 이벤트가 나왔을 때 luck% 확률로 재판정 기회 부여</param>
        public void Roll(int luck)
        {
            if (!Run)
                return;

            /* 적 조우 판정 */
            if (IsEncounter)
            {
                EnemyData enemy = Database.Instance.Enemies[Random.Range(0, Database.Instance.Enemies.Count)];
                Encounter(enemy);

                return;
            }

            // 확률에 따라 이벤트 그룹 선정
            List<DSDialogueSO> chosenDialogues = GetDialoguesByChance(luck);

            // 이벤트가 발생하지 않은 경우 처리
            if (chosenDialogues == null)
                return;

            // 선정된 다이얼로그 그룹에서 랜덤으로 다이얼로그 선택
            int whichDialogue = Random.Range(0, chosenDialogues.Count);
            DSDialogueSO chosenDialogue = chosenDialogues[whichDialogue];

            // 이벤트 다이얼로그 출력
            DialogueManager.Instance.SetDialogue(chosenDialogue);
        }

        /// <summary>
        /// 행운 수치에 따라 재판정 기회를 부여한다
        /// </summary>
        /// <param name="luck">재판정 확률 (%)</param>
        /// <returns>재판정일 경우 true, 아니면 false</returns>
        bool Reroll(int luck)
        {
            return Random.Range(0, BeingConstants.MAX_STAT) < luck;
        }

        /// <summary>
        /// Inspector로 설정한 확률에 따라 이벤트 그룹을 선정해 해당 그룹의 다이얼로그 배열을 반환한다
        /// </summary>
        /// <param name="luck">재판정 확률 (%)</param>
        /// <returns>선정된 다이얼로그 그룹에 속한 다이얼로그 배열</returns>
        List<DSDialogueSO> GetDialoguesByChance(int luck)
        {
            /* 랜덤 이벤트 판정 */
            int totalChance = PeaceChance + PositiveEventChance + NegativeEventChance + MemberEventChance;
            int totalChanceDice = Random.Range(0, totalChance);

            // 이벤트 발생 X 여부 확인
            int chance = PeaceChance;
            if (totalChanceDice < chance)
            {
                return null;
            }

            // 긍정적 이벤트 여부 확인
            chance += PositiveEventChance;
            if (totalChanceDice < chance)
            {
                return dialogues[EventDefines.EVENT_POSITIVE_GROUP_NAME];
            }
            
            // 부정적 이벤트 여부 확인
            chance += NegativeEventChance;
            if (totalChanceDice < chance)
            {
                // luck 수치에 따라 부정적 이벤트일 경우 재판정 기회 부여
                if (Reroll(luck))
                    return GetDialoguesByChance(luck);

                return dialogues[EventDefines.EVENT_NEGATIVE_GROUP_NAME];
            }

            // 동료 이벤트 반환 (이미 동료를 모두 영입한 경우 dialogues에 데이터가 없으므로 null 반환)
            if (!dialogues.ContainsKey(EventDefines.EVENT_MEMBER_GROUP_NAME))
                return null;

            List<DSDialogueSO> memberDialogues = dialogues[EventDefines.EVENT_MEMBER_GROUP_NAME];

            if (memberDialogues == null)
                return null;

            int random = Random.Range(0, memberDialogues.Count);

            // 동료 영입 이벤트는 한 번만 이뤄져야 하므로 하나만 골라 반환하고 해당 다이얼로그는 배열에서 제거
            List<DSDialogueSO> memberDialogue = new List<DSDialogueSO>() { memberDialogues[random] };
            memberDialogues.RemoveAt(random);

            // 동료를 모두 영입한 경우 이후 null을 반환해 이벤트가 일어나지 않도록 하기 위해 dalogues Dictionary에서 데이터 제거
            if (memberDialogues.Count == 0)
                dialogues.Remove(EventDefines.EVENT_MEMBER_GROUP_NAME);

            return memberDialogue;
        }
        #endregion

        #region 전투 이벤트 관련 메소드
        public void Encounter(EnemyData enemy)
        {
            Debug.Log($"{enemy.Name}을(를) 조우했다!");
        }
        #endregion
    }
}
