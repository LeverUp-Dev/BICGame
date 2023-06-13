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

        /* ���̾�α� ������ ���� ���� */
        DSDialogue dialogueComponent;
        DSDialogueContainerSO dialogueContainer;
        DSDialogueGroupSO[] dialogueGroups;

        Dictionary<DSDialogueGroupSO, List<DSDialogueSO>> dialogues;

        /* �̺�Ʈ ���� ���� */
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

            // �� ��ȯ �ÿ� �̱��� ��ü�� �ı����� �ʵ��� ����
            DontDestroyOnLoad(gameObject);

            dialogueComponent = GetComponent<DSDialogue>();
            dialogueContainer = dialogueComponent.GetDialogueContainer();

            // ���̾�α� ���� �˻�
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

            // �� ������� ���� Ƚ�� �ʱ�ȭ
            encounterCount = EnemyEncounterTime;
        }

        #region ���� �̺�Ʈ ���� �޼ҵ�
        /// <summary>
        /// ���� Ȯ���� ���� �̺�Ʈ ����
        /// </summary>
        /// <param name="luck">�������� �̺�Ʈ�� ������ �� luck% Ȯ���� ������ ��ȸ �ο�</param>
        public void Roll(int luck)
        {
            /*
             * Ȯ�� (���� : ���� : ���� = 1 : 1 : RatioOfPeace)
             * ���� : ���� �Ÿ� �̵� ��
             * ���� : �̵� �� ���� Ȯ���� ��ī���ͱ��� ���� Ƚ�� ����
             */

            /* �� ���� ���� */
            if (IsEncounter)
            {
                EnemyData enemy = Database.Instance.Enemies[Random.Range(0, Database.Instance.Enemies.Count)];
                Encounter(enemy);

                return;
            }

            /* ���� �̺�Ʈ ���� */
            int whichGroup = Random.Range(0, dialogueGroups.Length + RatioOfPeace);

            // �̺�Ʈ �߻� X
            if (whichGroup >= dialogueGroups.Length)
                return;
            
            // luck ��ġ�� ���� ������ �̺�Ʈ�� ��� ������ ��ȸ �ο�
            if (dialogueGroups[whichGroup].GroupName.StartsWith(EventConstants.EVENT_NEGATIVE_PREFIX) && Reroll(luck))
            {
                whichGroup = Random.Range(0, dialogueGroups.Length);
            }

            // �������� ���õ� ���̾�α� �׷� �˻�
            DSDialogueGroupSO chosenGroup = dialogueGroups[whichGroup];
            List<DSDialogueSO> chosenDialogues = dialogues[chosenGroup];

            // �ش� ���̾�α� �׷쿡�� �������� ���̾�α� ����
            int whichDialogue = Random.Range(0, chosenDialogues.Count);
            DSDialogueSO chosenDialogue = chosenDialogues[whichDialogue];

            // �̺�Ʈ ���̾�α� ���
            DialogueManager.Instance.SetDialogue(chosenDialogue);
        }

        /// <summary>
        /// ��� ��ġ�� ���� ������ ��ȸ �ο�
        /// </summary>
        /// <param name="luck">������ Ȯ�� (%)</param>
        /// <returns>�������� ��� true, �ƴϸ� false</returns>
        bool Reroll(int luck)
        {
            return Random.Range(0, BeingConstants.MAX_STAT_LUCK) < luck;
        }
        #endregion

        #region ���� �̺�Ʈ ���� �޼ҵ�
        public void Encounter(EnemyData enemy)
        {
            Debug.Log($"{enemy.Name}��(��) �����ߴ�!");
        }
        #endregion
    }
}
