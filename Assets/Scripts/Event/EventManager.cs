using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Event {
    using DS;
    using DS.ScriptableObjects;
    using DB.Data;
    using Defines;
    using DB;

    [RequireComponent(typeof(DSDialogue))]

    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        /* 다이얼로그 데이터 관련 변수 */
        DSDialogue dialogueComponent;
        DSDialogueContainerSO dialogueContainer;
        DSDialogueGroupSO[] dialogueGroups;

        Dictionary<DSDialogueGroupSO, List<DSDialogueSO>> dialogues;

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

        /* Inspector Values */
        [field: SerializeField] public int RatioOfPeace { get; private set; }
        /*[field: SerializeField] public int RatioOfPositiveEvent { get; private set; }
        [field: SerializeField] public int RatioOfNegativeEvent { get; private set; }*/

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

            dialogueComponent = GetComponent<DSDialogue>();
            dialogueContainer = dialogueComponent.GetDialogueContainer();

            // 다이얼로그 정보 검색
            int i = 0;
            dialogueGroups = new DSDialogueGroupSO[dialogueContainer.DialogueGroups.Count];
            dialogues = new Dictionary<DSDialogueGroupSO, List<DSDialogueSO>>();

            foreach (DSDialogueGroupSO group in dialogueContainer.DialogueGroups.Keys)
            {
                List<DSDialogueSO> list = new List<DSDialogueSO>();
                foreach (DSDialogueSO dialogue in dialogueContainer.DialogueGroups[group])
                {
                    if (dialogue.IsStartingDialogue)
                        list.Add(dialogue);
                }

                dialogueGroups[i++] = group;
                dialogues[group] = list;
            }

            // 적 조우까지 남은 횟수 초기화
            encounterCount = EnemyEncounterTime;
        }

        #region 랜덤 이벤트 관련 메소드
        /// <summary>
        /// 일정 확률로 랜덤 이벤트 생성
        /// </summary>
        /// <param name="luck">부정적인 이벤트가 나왔을 때 luck% 확률로 재판정 기회 부여</param>
        public void Roll(int luck)
        {
            /*
             * 확률 (긍정 : 부정 : 없음 = 1 : 1 : RatioOfPeace)
             * 동료 : 일정 거리 이동 시
             * 전투 : 이동 시 일정 확률로 인카운터까지 남은 횟수 감소
             */

            /* 적 조우 판정 */
            if (IsEncounter)
            {
                EnemyData enemy = Database.Instance.Enemies[Random.Range(0, Database.Instance.Enemies.Count)];
                Encounter(enemy);

                return;
            }

            /* 랜덤 이벤트 판정 */
            int whichGroup = Random.Range(0, dialogueGroups.Length + RatioOfPeace);

            // 이벤트 발생 X
            if (whichGroup >= dialogueGroups.Length)
                return;
            
            // luck 수치에 따라 부정적 이벤트일 경우 재판정 기회 부여
            if (dialogueGroups[whichGroup].GroupName.StartsWith(EventConstants.EVENT_NEGATIVE_PREFIX) && Reroll(luck))
            {
                whichGroup = Random.Range(0, dialogueGroups.Length);
            }

            // 랜덤으로 선택된 다이얼로그 그룹 검색
            DSDialogueGroupSO chosenGroup = dialogueGroups[whichGroup];
            List<DSDialogueSO> chosenDialogues = dialogues[chosenGroup];

            // 해당 다이얼로그 그룹에서 랜덤으로 다이얼로그 선택
            int whichDialogue = Random.Range(0, chosenDialogues.Count);
            DSDialogueSO chosenDialogue = chosenDialogues[whichDialogue];

            // 이벤트 다이얼로그 출력
            DialogueManager.Instance.SetDialogue(chosenDialogue);
        }

        /// <summary>
        /// 행운 수치에 따라 재판정 기회 부여
        /// </summary>
        /// <param name="luck">재판정 확률 (%)</param>
        /// <returns>재판정일 경우 true, 아니면 false</returns>
        bool Reroll(int luck)
        {
            return Random.Range(0, BeingConstants.MAX_STAT_LUCK) < luck;
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
