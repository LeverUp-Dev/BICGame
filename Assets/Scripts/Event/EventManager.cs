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

        /* ���̾�α� ������ ���� ���� */
        DSDialogue dialogueComponent;
        DSDialogueContainerSO dialogueContainer;

        Dictionary<string, List<DSDialogueSO>> dialogues;

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

        Player player;

        /* Inspector Values */
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

            // �� ��ȯ �ÿ� �̱��� ��ü�� �ı����� �ʵ��� ����
            DontDestroyOnLoad(gameObject);

            // �÷��̾� ���� �˻�
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            /* ���̾�α� ���� �˻� */
            dialogueComponent = GetComponent<DSDialogue>();
            dialogueContainer = dialogueComponent.GetDialogueContainer();
            
            dialogues = new Dictionary<string, List<DSDialogueSO>>();

            foreach (DSDialogueGroupSO group in dialogueContainer.DialogueGroups.Keys)
            {
                List<DSDialogueSO> list = new List<DSDialogueSO>();
                foreach (DSDialogueSO dialogue in dialogueContainer.DialogueGroups[group])
                {
                    // ���� ���̾�α׸� ����
                    if (!dialogue.IsStartingDialogue)
                        continue;

                    // �̹� ���Ե� ����� �� �̻� ���� �̺�Ʈ�� �߻��ϸ� �� �ǹǷ� ó��
                    if (group.GroupName.Equals(EventConstants.EVENT_MEMBER_GROUP_NAME) && player.GetMember(dialogue.DialogueName) != null)
                        continue;
                    
                    list.Add(dialogue);
                }

                if (list.Count == 0)
                    continue;

                dialogues[group.GroupName] = list;
            }

            // �� ������� ���� Ƚ�� �ʱ�ȭ
            encounterCount = EnemyEncounterTime;
        }

        #region ���� �̺�Ʈ ���� �޼ҵ�
        /// <summary>
        /// ���� Ȯ���� ���� �̺�Ʈ�� �����Ѵ�
        /// </summary>
        /// <param name="luck">�������� �̺�Ʈ�� ������ �� luck% Ȯ���� ������ ��ȸ �ο�</param>
        public void Roll(int luck)
        {
            /* �� ���� ���� */
            if (IsEncounter)
            {
                EnemyData enemy = Database.Instance.Enemies[Random.Range(0, Database.Instance.Enemies.Count)];
                Encounter(enemy);

                return;
            }

            // Ȯ���� ���� �̺�Ʈ �׷� ����
            List<DSDialogueSO> chosenDialogues = GetDialoguesByChance(luck);

            // �̺�Ʈ�� �߻����� ���� ��� ó��
            if (chosenDialogues == null)
                return;

            // ������ ���̾�α� �׷쿡�� �������� ���̾�α� ����
            int whichDialogue = Random.Range(0, chosenDialogues.Count);
            DSDialogueSO chosenDialogue = chosenDialogues[whichDialogue];

            // �̺�Ʈ ���̾�α� ���
            DialogueManager.Instance.SetDialogue(chosenDialogue);
        }

        /// <summary>
        /// ��� ��ġ�� ���� ������ ��ȸ�� �ο��Ѵ�
        /// </summary>
        /// <param name="luck">������ Ȯ�� (%)</param>
        /// <returns>�������� ��� true, �ƴϸ� false</returns>
        bool Reroll(int luck)
        {
            return Random.Range(0, BeingConstants.MAX_STAT_LUCK) < luck;
        }

        /// <summary>
        /// Inspector�� ������ Ȯ���� ���� �̺�Ʈ �׷��� ������ �ش� �׷��� ���̾�α� �迭�� ��ȯ�Ѵ�
        /// </summary>
        /// <param name="luck">������ Ȯ�� (%)</param>
        /// <returns>������ ���̾�α� �׷쿡 ���� ���̾�α� �迭</returns>
        List<DSDialogueSO> GetDialoguesByChance(int luck)
        {
            /* ���� �̺�Ʈ ���� */
            int totalChance = PeaceChance + PositiveEventChance + NegativeEventChance + MemberEventChance;
            int totalChanceDice = Random.Range(0, totalChance);

            // �̺�Ʈ �߻� X ���� Ȯ��
            int chance = PeaceChance;
            if (totalChanceDice < chance)
            {
                return null;
            }

            // ������ �̺�Ʈ ���� Ȯ��
            chance += PositiveEventChance;
            if (totalChanceDice < chance)
            {
                return dialogues[EventConstants.EVENT_POSITIVE_GROUP_NAME];
            }
            
            // ������ �̺�Ʈ ���� Ȯ��
            chance += NegativeEventChance;
            if (totalChanceDice < chance)
            {
                // luck ��ġ�� ���� ������ �̺�Ʈ�� ��� ������ ��ȸ �ο�
                if (Reroll(luck))
                    return GetDialoguesByChance(luck);

                return dialogues[EventConstants.EVENT_NEGATIVE_GROUP_NAME];
            }

            // ���� �̺�Ʈ ��ȯ (�̹� ���Ḧ ��� ������ ��� dialogues�� �����Ͱ� �����Ƿ� null ��ȯ)
            if (!dialogues.ContainsKey(EventConstants.EVENT_MEMBER_GROUP_NAME))
                return null;

            List<DSDialogueSO> memberDialogues = dialogues[EventConstants.EVENT_MEMBER_GROUP_NAME];

            if (memberDialogues == null)
                return null;

            int random = Random.Range(0, memberDialogues.Count);

            // ���� ���� �̺�Ʈ�� �� ���� �̷����� �ϹǷ� �ϳ��� ��� ��ȯ�ϰ� �ش� ���̾�α״� �迭���� ����
            List<DSDialogueSO> memberDialogue = new List<DSDialogueSO>() { memberDialogues[random] };
            memberDialogues.RemoveAt(random);

            // ���Ḧ ��� ������ ��� ���� null�� ��ȯ�� �̺�Ʈ�� �Ͼ�� �ʵ��� �ϱ� ���� dalogues Dictionary���� ������ ����
            if (memberDialogues.Count == 0)
                dialogues.Remove(EventConstants.EVENT_MEMBER_GROUP_NAME);

            return memberDialogue;
        }
        #endregion

        #region ���� �̺�Ʈ ���� �޼ҵ�
        public void Encounter(EnemyData enemy)
        {
            Debug.Log($"{player.Status.Name}��(��) {enemy.Name}��(��) �����ߴ�!");
        }
        #endregion
    }
}
